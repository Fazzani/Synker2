using hfa.SyncLibrary.Common;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using PlaylistBaseLibrary.ChannelHandlers;
using PlaylistBaseLibrary.Entities;
using PlaylistManager.Entities;
using SyncLibrary.Configuration;
using hfa.SyncLibrary.Global;
using SyncLibrary.TvgMediaHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Hfa.SyncLibrary;
using Hfa.SyncLibrary.Verbs;
using CommandLine;
using Hfa.SyncLibrary.Messages;
using Microsoft.Extensions.Logging;
using static hfa.SyncLibrary.Global.Common;
using Microsoft.Extensions.Options;
using Hfa.SyncLibrary.Infrastructure;
using System.IO;

namespace SyncLibrary
{
    class SynchElastic
    {
        static CancellationTokenSource ts = new CancellationTokenSource();
        static IMessagesService _messagesService;
        static IElasticConnectionClient _elasticClient;
        static IOptions<ApplicationConfigData> _config;

        public static void Main(string[] args)
        {
            Init.Build();

            try
            {
                _config = Init.ServiceProvider.GetService<IOptions<ApplicationConfigData>>();
                _elasticClient = Init.ServiceProvider.GetService<IElasticConnectionClient>();
                _messagesService = Init.ServiceProvider.GetService<IMessagesService>();

                Logger(nameof(SynchElastic)).LogInformation("Init Synker...");

                _messagesService.SendAsync(Message.PingMessage, ts.Token).GetAwaiter().GetResult();

                ArgsParser(args).GetAwaiter().GetResult();
            }
            catch (OperationCanceledException ex)
            {
                Logger(nameof(SynchElastic)).LogCritical(ex, "Canceled operation");
            }
            catch (Exception e)
            {
                _messagesService.SendAsync(e.Message, MessageIdEnum.EXCEPTION.ToString(), ts.Token).GetAwaiter().GetResult();
                Logger(nameof(SynchElastic)).LogCritical(e, e.Message);
                throw;
            }
        }

        /// <summary>
        /// Args Parser
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static async Task ArgsParser(string[] args)
        {
            await Parser.Default.ParseArguments<PurgeTempFilesVerb, SyncEpgElasticVerb, SyncMediasElasticVerb, SaveNewConfigVerb>(args).MapResult(
                 (PurgeTempFilesVerb opts) => PurgeTempFilesVerb.MainPurgeAsync(opts),
                  (SyncEpgElasticVerb opts) => SyncEpgElasticAsync(opts, _elasticClient, _config, ts.Token),
                  (SyncMediasElasticVerb opts) => SyncMediasElasticAsync(opts, _elasticClient, _config, ts.Token),
                  (SaveNewConfigVerb opts) => SaveConfigAsync(opts, ts.Token),
                 errs => throw new AggregateException(errs.Select(e => new Exception(e.Tag.ToString()))));
        }

        /// <summary>
        /// Sync medias to elastic
        /// </summary>
        /// <param name="options"></param>
        /// <param name="elasticClient"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task SyncMediasElasticAsync(SyncMediasElasticVerb options, IElasticConnectionClient elasticClient, IOptions<ApplicationConfigData> config, CancellationToken token)
        {
            var res = await SynchronizableConfigManager.LoadEncryptedConfig(options.FilePath, options.CertificateName);

            await _messagesService.SendAsync($"Start Sync Elastic By {options.FilePath} config", MessageIdEnum.START_SYNC_MEDIAS.ToString(), token);

            if (res != null & res.Sources.Any())
            {
#if DEBUG
                foreach (var source in res.Sources.Skip(1))
#else
                foreach (var source in res.Sources)
#endif
                {
                    var provider = await ProviderFactory.Create(source);
                    if (provider != null)
                    {
                        var newMedias = new List<TvgMedia>();
                        using (var pl = new Playlist<TvgMedia>(provider))
                        {
                            var medias = (from c in pl select c).ToList();

                            //Faire passer les handlers
                            var handler = FabricHandleMedias(elasticClient);

                            foreach (var media in medias)
                            {
                                handler.HandleTvgMedia(media);
                                if (media.IsValid)
                                {
                                    Logger(nameof(SyncMediasElasticAsync)).LogInformation($"Treating media  => {media.Name} : {media.Url}");

                                    var result = await elasticClient.Client
                                        .SearchAsync<TvgMedia>(x => x.Index<TvgMedia>()
                                            .Query(q => q.Term(m => m.Url, media.Url)), token);

                                    if (result.Total < 1)
                                    {
                                        newMedias.Add(media);
                                    }
                                    else
                                    {
                                        if (options.Force && media != result.Documents.FirstOrDefault() || (media.Tvg?.Id != result.Documents.FirstOrDefault()?.Id))
                                        {
                                            //Modification
                                            Logger(nameof(SyncMediasElasticAsync)).LogInformation($"Updating media {result.Documents.SingleOrDefault().Id} in Elastic");

                                            var response = await elasticClient.Client.UpdateAsync<TvgMedia>(result.Documents.SingleOrDefault().Id,
                                                m => m.Doc(new TvgMedia { Id = null, Name = media.Name, Lang = media.Lang }), token);
                                            response.AssertElasticResponse();
                                        }
                                    }
                                }
                            }
                            if (newMedias.Any())
                            {
                                //Push to Elastic
                                var responseBulk = await elasticClient.Client.BulkAsync(x => x.Index(config.Value.DefaultIndex).CreateMany(newMedias,
                                    (bd, q) => bd.Index(config.Value.DefaultIndex)), token);
                                responseBulk.AssertElasticResponse();
                            }
                        }
                    }
                }
            }
            await _messagesService.SendAsync($"End Sync Elastic By {options.FilePath} config", MessageIdEnum.END_SYNC_MEDIAS.ToString(), token);
        }

        /// <summary>
        /// sync EPG
        /// </summary>
        /// <param name="options"></param>
        /// <param name="elasticClient"></param>
        /// <param name="config"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task SyncEpgElasticAsync(SyncEpgElasticVerb options, IElasticConnectionClient elasticClient, IOptions<ApplicationConfigData> config, CancellationToken token)
        {
            Stream response = null;
            await _messagesService.SendAsync($"Start Sync Xmltv file {options.FilePath} to Elastic", MessageIdEnum.START_SYNC_EPG_CONFIG.ToString(), token);
            try
            {
                if (!File.Exists(options.FilePath))
                {
                    using (var client = new HttpClient())
                    {
                        response = await client.GetStreamAsync(new Uri(options.FilePath));
                    }
                }
                else
                {
                    response = File.OpenRead(options.FilePath);
                }
                var ser = new XmlSerializer(typeof(tv));
                var tvModel = (tv)ser.Deserialize(response);

                //Sync Elastic
                var responseBulk = await elasticClient.Client.BulkAsync(x => x.Index(config.Value.DefaultIndex).CreateMany(tvModel.channel, (bd, q) => bd.Index(config.Value.DefaultIndex)), token);
                responseBulk.AssertElasticResponse();
            }
            catch (Exception ex)
            {
                Logger(nameof(SyncEpgElasticAsync)).LogCritical(ex, ex.Message);
            }
            finally
            {
                response?.Close();
                response?.Dispose();
            }
            await _messagesService.SendAsync($"END Sync Xmltv file {options.FilePath} to Elastic", MessageIdEnum.END_SYNC_EPG_CONFIG.ToString(), token);
        }

        /// <summary>
        /// save config provider Sync
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static async Task SaveConfigAsync(SaveNewConfigVerb options, CancellationToken token)
        {
            await _messagesService.SendAsync($"Start save new config {options.FilePath}", MessageIdEnum.END_CREATE_CONFIG.ToString(), token);
            //await SynchronizableConfigManager.SaveAndEncrypt(new ConfigSync
            //{
            //    Sources = new List<ISynchronizableConfig>
            //    {
            //        new SynchronizableConfigFile("C:\\dynamic.m3u"){ SynchConfigType = SynchronizableConfigType.M3u},
            //        new SynchronizableConfigUri(urlM3u){ SynchConfigType = SynchronizableConfigType.M3u}
            //    },
            //    Target = new SynchronizableConfigConnected
            //    {
            //        SynchConfigType = SynchronizableConfigType.ElasticSearch,
            //        Connection = new ConnectionData
            //        {
            //            ServerUri = new Uri("http://155.ip-151-80-235.eu:9201/")
            //        }
            //    }
            //}, options.FilePath, options.CertificateName);

            var res = await SynchronizableConfigManager.Load($"{options.FilePath}.loc");
            await SynchronizableConfigManager.SaveAndEncrypt(res, options.FilePath, options.CertificateName);
            await _messagesService.SendAsync($"END save new config {options.FilePath}", MessageIdEnum.END_CREATE_CONFIG.ToString(), token);
        }

        /// <summary>
        /// Fabriquer les medias handlers (clean names, match epg, etc ...)
        /// </summary>
        private static TvgMediaHandler FabricHandleMedias(IElasticConnectionClient elasticConnectionClient)
        {
            var contextHandler = Init.ServiceProvider.GetService<IContextTvgMediaHandler>();
            var cleanNameHandler = new TvgMediaCleanNameHandler(contextHandler);
            var groupHandler = new TvgMediaGroupMatcherHandler(contextHandler);
            var epgHandler = new TvgMediaEpgMatcherNameHandler(contextHandler, elasticConnectionClient);
            var langHandler = new TvgMediaLangMatcherHandler(contextHandler);

            langHandler.SetSuccessor(groupHandler);
            groupHandler.SetSuccessor(epgHandler);
            epgHandler.SetSuccessor(cleanNameHandler);
            return langHandler;
        }
    }
}
