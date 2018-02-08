﻿using Microsoft.Extensions.DependencyInjection;
using Nest;
using PlaylistBaseLibrary.ChannelHandlers;
using PlaylistBaseLibrary.Entities;
using PlaylistManager.Entities;
using SyncLibrary.Configuration;
using hfa.SyncLibrary.Global;
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
using Microsoft.Extensions.Logging;
using static hfa.SyncLibrary.Global.Common;
using Microsoft.Extensions.Options;
using Hfa.SyncLibrary.Infrastructure;
using System.IO;
using System.Runtime.CompilerServices;
using hfa.Synker.Services.Entities.Messages;
using hfa.Synker.Services.Messages;
using hfa.Synker.Service.Services.TvgMediaHandlers;
using hfa.Synker.Service.Services.Elastic;
using hfa.Synker.Service.Elastic;
using hfa.Synker.batch;
using hfa.Synker.Service.Services.Playlists;
using hfa.Synker.Services.Dal;
using hfa.PlaylistBaseLibrary.Providers;
using Newtonsoft.Json;
using System.Text;
using hfa.Synker.Service.Services.Notification;
using Microsoft.EntityFrameworkCore;
using hfa.Brokers.Messages.Emailing;
using System.Reflection;

[assembly: InternalsVisibleTo("hfa.synker.batch.test")]
namespace SyncLibrary
{
    internal class SynchElastic
    {
        static CancellationTokenSource ts = new CancellationTokenSource();
        static IMessageService _messagesService;
        private static INotificationService _notificationService;
        static IElasticConnectionClient _elasticClient;
        static ApplicationConfigData _config;
        private static IOptions<ElasticConfig> _elastiConfig;
        private static ILogger _logger;

        public static void Main(string[] args)
        {
            Init.Build();

            try
            {
                _logger = Logger(nameof(SynchElastic));
                _config = Init.ServiceProvider.GetService<IOptions<ApplicationConfigData>>().Value;
                _elastiConfig = Init.ServiceProvider.GetService<IOptions<ElasticConfig>>();
                _elasticClient = Init.ServiceProvider.GetService<IElasticConnectionClient>();
                _messagesService = Init.ServiceProvider.GetService<IMessageService>();
                _notificationService = Init.ServiceProvider.GetService<INotificationService>();

                Logger(nameof(SynchElastic)).LogInformation("Init batch Synker...");

                _messagesService.SendAsync(Message.PingMessage, _config.ApiUserName, _config.ApiPassword, ts.Token).GetAwaiter().GetResult();

                ArgsParserAsync(args, _messagesService).GetAwaiter().GetResult();
            }
            catch (OperationCanceledException ex)
            {
                Logger(nameof(SynchElastic)).LogCritical(ex, "Operation was Canceled");
            }
            catch (Exception e)
            {
                Logger(nameof(SynchElastic)).LogCritical(e, e.Message);
                _messagesService.SendAsync(e.Message, MessageTypeEnum.EXCEPTION, _config.ApiUserName, _config.ApiPassword, ts.Token).GetAwaiter().GetResult();
            }
        }

        /// <summary>
        /// Args Parser
        /// </summary>
        /// <param name="args"></param>
        /// <param name="messagesService"></param>
        /// <returns></returns>
        public static async Task ArgsParserAsync(string[] args, IMessageService messagesService)
        {
            await Parser.Default
                .ParseArguments<PurgeTempFilesVerb, DiffPlaylistVerb, SyncEpgElasticVerb, SaveNewConfigVerb, PushXmltvVerb, SendMessageVerb, ScanPlaylistFileVerb>(args)
                .MapResult(
                 (PurgeTempFilesVerb opts) => PurgeTempFilesVerb.MainPurgeAsync(opts),
                  (DiffPlaylistVerb opts) => DiffPlaylistAsync(opts, _config, ts.Token),
                  (SyncEpgElasticVerb opts) => SyncEpgElasticAsync(opts, _elasticClient, _config, ts.Token),
                  (SaveNewConfigVerb opts) => SaveConfigAsync(opts, _config, ts.Token),
                  (PushXmltvVerb opts) => PushXmltvAsync(opts, messagesService, new HttpClient(), _config, ts.Token),
                  (SendMessageVerb opts) => SendMessageAsync(opts, _config, ts.Token),
                  (ScanPlaylistFileVerb opts) => ScanPlaylist(opts, ts.Token),
                 errs => throw new AggregateException(errs.Select(e => new Exception(e.Tag.ToString()))));
        }

        public static async Task ScanPlaylist(ScanPlaylistFileVerb options, CancellationToken token = default(CancellationToken))
        {
            await new ScanPlaylistService().ScanAsync(options, Logger(nameof(SynchElastic)), token);
        }

        public static async Task DiffPlaylistAsync(DiffPlaylistVerb options, ApplicationConfigData config, CancellationToken cancellationToken = default(CancellationToken))
        {
            var playlistService = Init.ServiceProvider.GetService<IPlaylistService>();
            var _dbContext = Init.ServiceProvider.GetService<SynkerDbContext>();

            foreach (var pl in _dbContext.Playlist.Include(x => x.User).Where(x => x.Status == hfa.Synker.Service.Entities.Playlists.PlaylistStatus.Enabled))
            {
                if (pl.IsXtreamTag)
                {
                    var res = await playlistService.DiffWithSourceAsync(() => pl, new XtreamProvider(pl.SynkConfig.Url), false, cancellationToken);
                    if (res.removed.Any() || res.tvgMedia.Any())
                    {
                        //TODO :  send notif to user with result
                        _logger.LogInformation($"diff detected for the playlist {pl.Id} of user {pl.UserId} ");
                        var message = new Message
                        {
                            Content = $"<h1>{res.tvgMedia.Count()} medias was added and {res.removed.Count()} medias was removed from the playlist {pl.Freindlyname}</h1><h3>Added medias</h3><ul>{string.Join("</li><li>", res.tvgMedia)}<h3>Removed medias</h3></ul><ul>{string.Join("</li><li>", res.removed)}</ul>",
                            MessageType = MessageTypeEnum.DIFF_PLAYLIST,
                            UserId = pl.UserId,
                            TimeStamp = DateTime.Now,
                            Status = MessageStatusEnum.NotReaded
                        };

                        //Add new message for user
                        await _messagesService.SendAsync(message, _config.ApiUserName, _config.ApiPassword, ts.Token);
                        //Send Email Notification
                        await _notificationService.SendMailAsync(new EmailNotification("synker.batch")
                        {
                            Body = message.Content,
                            FromDisplayName = "Synker Team",
                            From = "synker-team@synker.ovh",
                            Subject = $"New Playlist changement detected for {pl.Freindlyname}",
                            IsBodyHtml = true,
                            To = pl.User.Email
                        }, ts.Token);
                    }
                }
            }
        }

        /// <summary>
        /// sync EPG
        /// </summary>
        /// <param name="options"></param>
        /// <param name="elasticClient"></param>
        /// <param name="config"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task SyncEpgElasticAsync(SyncEpgElasticVerb options, IElasticConnectionClient elasticClient, ApplicationConfigData config, CancellationToken token = default(CancellationToken))
        {
            Stream response = null;
            await _messagesService.SendAsync($"Start Sync Xmltv file {options.FilePath} to Elastic", MessageTypeEnum.START_SYNC_EPG_CONFIG,
                config.ApiUserName, config.ApiPassword, token);
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
                var responseBulk = await elasticClient.Client.BulkAsync(x => x.Index(_elastiConfig.Value.DefaultIndex)
                .CreateMany(tvModel.channel, (bd, q) => bd.Index(_elastiConfig.Value.DefaultIndex)), token);
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
            await _messagesService.SendAsync($"END Sync Xmltv file {options.FilePath} to Elastic", MessageTypeEnum.END_SYNC_EPG_CONFIG,
                config.ApiUserName, config.ApiPassword, token);
        }

        /// <summary>
        /// Send new message to synker API
        /// </summary>
        /// <param name="options"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task SendMessageAsync(SendMessageVerb options, ApplicationConfigData config, CancellationToken token = default(CancellationToken))
        {
            await _messagesService.SendAsync(new Message
            {
                //Author = options?.Author,
                Content = options.Message,
                MessageType = (MessageTypeEnum)Enum.Parse(typeof(MessageTypeEnum), options.MessageType.ToString())
            },
            config.ApiUserName,
            config.ApiPassword,
            token);
        }

        /// <summary>
        /// save config provider Sync
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static async Task SaveConfigAsync(SaveNewConfigVerb options, ApplicationConfigData config, CancellationToken token = default(CancellationToken))
        {
            await _messagesService.SendAsync($"Start save new config {options.FilePath}", MessageTypeEnum.START_CREATE_CONFIG, token);
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
            await _messagesService.SendAsync($"END save new config {options.FilePath}", MessageTypeEnum.END_CREATE_CONFIG, token);
        }

        /// <summary>
        /// Push xmltv file to the api
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static async Task<bool> PushXmltvAsync(PushXmltvVerb options, IMessageService messagesService, HttpClient httpClient, ApplicationConfigData config, CancellationToken token = default(CancellationToken))
        {
            await messagesService.SendAsync($"Pushing {options.FilePath}", MessageTypeEnum.START_PUSH_XMLTV, config.ApiUserName, config.ApiPassword, token);
            if (!File.Exists(options.FilePath))
            {
                await messagesService.SendAsync($"File not Exist {options.FilePath}", MessageTypeEnum.START_PUSH_XMLTV, token);
                return false;
            }

            var fileContent = await File.ReadAllTextAsync(options.FilePath);
            using (var sr = new StringReader(fileContent))
            {
                var xs = new XmlSerializer(typeof(tv));
                using (httpClient)
                {
                    var httpResponseMessage = await httpClient.PostAsync(new Uri(options.ApiUrl), new JsonContent(xs.Deserialize(sr)), token);
                    await messagesService.SendAsync($"END save new config {options.FilePath} with httpResponseMessage : {httpResponseMessage.ReasonPhrase} ",
                        MessageTypeEnum.END_PUSH_XMLTV, config.ApiUserName, config.ApiPassword, token);
                    return httpResponseMessage.IsSuccessStatusCode;
                }
            }
        }

        /// <summary>
        /// Fabriquer les medias handlers (clean names, match epg, etc ...)
        /// </summary>
        private static TvgMediaHandler FabricHandleMedias(IElasticConnectionClient elasticConnectionClient)
        {
            var contextHandler = Init.ServiceProvider.GetService<IContextTvgMediaHandler>();
            var cleanNameHandler = new TvgMediaCleanNameHandler(contextHandler);
            var shiftHandler = new TvgMediaShiftMatcherHandler(contextHandler);
            var groupHandler = new TvgMediaGroupMatcherHandler(contextHandler);
            var epgHandler = new TvgMediaEpgMatcherNameHandler(contextHandler, elasticConnectionClient, _elastiConfig);
            var langHandler = new TvgMediaShiftMatcherHandler(contextHandler);

            langHandler.SetSuccessor(shiftHandler);
            shiftHandler.SetSuccessor(groupHandler);
            groupHandler.SetSuccessor(epgHandler);
            epgHandler.SetSuccessor(cleanNameHandler);
            return langHandler;
        }
    }
}
