using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PlaylistBaseLibrary.Providers.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static TvheadendLibrary.Constants;
using System.Linq.Expressions;
using PlaylistManager.Providers;
using PlaylistManager.Entities;
using System.Threading;
using System.Reflection;

namespace TvheadendLibrary
{
    public class TvheadendService : PlaylistProvider<Playlist<TvgMedia>, TvgMedia>
    {
        readonly private string _url;
        readonly private string _username;
        readonly private string _password;
        const string LOGO_EXTENTION_PATTERN = "*.png";

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="url">Tvheadend base url</param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        public TvheadendService(string url, string username = "", string password = "")
        {
            if (!url.EndsWith("/"))
                url += "/";
            _url = url;
            _username = username;
            _password = password;
        }

        #region CHANNELS

        /// <summary>
        /// Get node list
        /// </summary>
        /// <param name="query"></param>
        /// <param name="nodeType"></param>
        /// <returns></returns>
        public JToken Nodes(QueryParams query, string nodeType)
        {
            //get nodes from Tvheadend
            var response = MakeHttpRequest(query, nodeType);
            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            var jsonObj = JObject.Parse(responseString);
            return jsonObj["entries"];
        }

        /// <summary>
        /// Get Node list
        /// </summary>
        /// <param name="query"></param>
        /// <param name="nodeType"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<JToken> NodesAsync(QueryParams query, string nodeType, CancellationToken token)
        {
            //get nodes from Tvheadend
            var response = MakeHttpRequest(query, nodeType);
            token.ThrowIfCancellationRequested();
            var responseString = await new StreamReader(response.GetResponseStream()).ReadToEndAsync();
            var jsonObj = JObject.Parse(responseString);
            return jsonObj["entries"];
        }

        /// <summary>
        /// Get channels list
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public JToken Channels(QueryParams query) => Nodes(query, API_URLS.CHANNELS_LIST);

        /// <summary>
        /// Get all channels than havn't logo
        /// </summary>
        /// <param name="dirPathLogos">The path to the logos directory</param>
        /// <returns></returns>
        public IEnumerable<JToken> ChannelsWithoutLogo(string dirPathLogos)
        {
            var channels = Channels(QueryParams.ALL);
            return channels.Where(x => !File.Exists(Path.Combine(dirPathLogos, x["icon"].ToString().Split('/').Last())));
        }

        ///// <summary>
        /////  Try fix logo channels
        ///// create new file picon with the same channel name
        ///// </summary>
        ///// <param name="dirPathLogos">Picons directory path</param>
        ///// <param name="defaultPiconName">Default picon file name</param>
        //public void TryFixLogos(string dirPathLogos, string defaultPiconName = "")
        //{
        //    $"Loading channels without logos".InfoConsole();
        //    var channels = ChannelsWithoutLogo(dirPathLogos).ToList();
        //    $"{channels.Count} channels detected without picons".InfoConsole();

        //    var charsToRemove = ConfigurationManager.AppSettings["charsToRemove"].Split(' ').Select(c => Char.Parse(c)).ToArray();
        //    var stringsToRemove = ConfigurationManager.AppSettings["stringsToRemove"].Split('_');

        //    $"Loading picons".InfoConsole();
        //    var piconNames = new DirectoryInfo(dirPathLogos).EnumerateFiles("*.png").Select(x => new Tuple<string, string>(
        //        x.Name
        //        .Remove(x.Name.LastIndexOf(x.Extension, StringComparison.InvariantCultureIgnoreCase))
        //        .Replace("+", "plus")
        //        .RemoveChars(charsToRemove)
        //        .RemoveStrings(stringsToRemove)
        //        .RemoveDiacritics()
        //        .Replace(" ", string.Empty), x.Name)).ToList();

        //    $"Trying to match picons from {piconNames.Count()} picons".InfoConsole();
        //    int complete = 0;
        //    foreach (var chan in channels)
        //    {
        //        $"{(int)Math.Round((double)(100 * complete++) / channels.Count)} %".InfoConsole();

        //        var originaleChannelName = chan["icon"]
        //           .ToString()
        //           .Split('/')
        //           .LastOrDefault();

        //        var channelName = originaleChannelName
        //            .Replace("png", string.Empty)
        //            .RemoveChars(charsToRemove)
        //            .RemoveStrings(stringsToRemove)
        //            .RemoveDiacritics()
        //            .Replace(" ", string.Empty);

        //        var piconName = piconNames.FirstOrDefault(x => x.Item1.Equals(channelName));
        //        if (piconName != null)
        //        {
        //            File.Copy(Path.Combine(dirPathLogos, piconName.Item2), Path.Combine(dirPathLogos, originaleChannelName));
        //            $"EPG updated for the channel {chan["name"]}".InfoConsole();
        //        }
        //        else
        //        {
        //            piconName = piconNames.FirstOrDefault(x => x.Item1.Contains(channelName) || channelName.Contains(x.Item1));
        //            if (piconName != null)
        //            {
        //                File.Copy(Path.Combine(dirPathLogos, piconName.Item2), Path.Combine(dirPathLogos, originaleChannelName), true);
        //                $"EPG updated for the channel {chan["name"]}".WarningConsole();
        //            }
        //            else
        //            {
        //                var data = new
        //                {
        //                    uuid = chan["uuid"],
        //                    icon = chan["icon"].ToString().Replace(originaleChannelName, defaultPiconName)
        //                };
        //                UpdateNode(data);
        //                $"EPG updated for the channel {chan["name"]}".ErrorConsole();
        //            }
        //        }
        //    }
        //}

        /// <summary>
        /// Get all channels than havn't epg
        /// </summary>
        /// <returns></returns>
        public IEnumerable<JToken> ChannelsWithoutEPG()
        {
            var epgs = Channels(QueryParams.ALL);
            return epgs.Where(x => x["epggrab"].ToString() == "[]");
        }

        ///// <summary>
        ///// Try fix EPG
        ///// </summary>
        //public void TryFixEPG()
        //{
        //    var channels = ChannelsWithoutEPG();
        //    var epgs = EPG(QueryParams.EPG_ALL);

        //    var charsToRemove = ConfigurationManager.AppSettings["charsToRemove"].Split(' ').Select(c => Char.Parse(c)).ToArray();

        //    var stringsToRemove = ConfigurationManager.AppSettings["stringsToRemove"].Split('_');

        //    var epgNames = epgs.Select(x => new Tuple<string, string>(x["name"].ToString()
        //        .RemoveStrings(stringsToRemove)
        //        .RemoveDiacritics()
        //        .Replace(" ", string.Empty), x["uuid"].ToString()));

        //    foreach (var c in channels)
        //    {
        //        var channelName = c["name"].ToString()
        //            .RemoveChars(charsToRemove)
        //            .RemoveStrings(stringsToRemove)
        //            .RemoveDiacritics()
        //            .Replace(" ", string.Empty);
        //        var epg = epgNames.FirstOrDefault(x => x.Item1.Equals(channelName, StringComparison.InvariantCultureIgnoreCase));
        //        if (epg != null)
        //        {
        //            c["epggrab"] = $"[\"{epg.Item2}\"]";
        //            var data = new
        //            {
        //                uuid = c["uuid"],
        //                epggrab = c["epggrab"]
        //            };
        //            UpdateNode(data);
        //            $"EPG updated for the channel {c["name"]}".InfoConsole();
        //        }
        //    }
        //}

        public JObject UpdateNode(object data)
        {
            var uri = new Uri(_url + API_URLS.UPDATE);
            // uuid["scan_state"] = 2;

            //uuid["iptv_sname"] = ;
            //  var res = GetMuxCorrectInfoFromUrl(uuid["iptv_muxname"].ToString());
            // var data = $"{{'enabled':'{uuid["enabled"]}','epg':'{uuid["epg"]}','iptv_url':'{uuid["iptv_url"]}','iptv_atsc':'{uuid["iptv_atsc"]}','iptv_muxname':'ff','channel_number':'{uuid["channel_number"]}','iptv_sname':'{res.iptv_sname}','scan_state':'{uuid["scan_state"]}','charset':'{uuid["charset"]}','priority':'{uuid["priority"]}','spriority':'{uuid["spriority"]}','iptv_substitute':'{uuid["iptv_substitute"]}','iptv_interface':'{uuid["iptv_interface"]}','iptv_epgid':'{res.iptv_epgid}','iptv_icon':'{res.iptv_icon}','iptv_tags':'{res.iptv_tags}','iptv_satip_dvbt_freq':'{uuid["iptv_satip_dvbt_freq"]}','iptv_buffer_limit':'{uuid["iptv_buffer_limit"]}','tsid_zero':false,'pmt_06_ac3':0,'eit_tsid_nocheck':'{uuid["eit_tsid_nocheck"]}','iptv_respawn':'{uuid["iptv_respawn"]}','iptv_kill':'{uuid["iptv_kill"]}','iptv_kill_timeout':'{uuid["iptv_kill_timeout"]}','iptv_env':'{uuid["iptv_env"]}','iptv_hdr':'{uuid["iptv_hdr"]}','uuid':'{uuid["uuid"]}'}}".Replace("'", "\"");
            //var data = new { enabled = uuid["enabled"], epg = uuid["epg"], iptv_url = uuid["iptv_url"], iptv_atsc = uuid["iptv_atsc"], iptv_muxname = res.iptv_sname, channel_number = uuid["channel_number"], iptv_sname = uuid["iptv_sname"], scan_state = 1, charset = uuid["charset"], priority = uuid["priority"], spriority = uuid["spriority"], iptv_substitute = uuid["iptv_substitute"], iptv_interface = uuid["iptv_interface"], res.iptv_epgid, res.iptv_icon, res.iptv_tags, iptv_satip_dvbt_freq = uuid["iptv_satip_dvbt_freq"], iptv_buffer_limit = uuid["iptv_buffer_limit"], tsid_zero = false, pmt_06_ac3 = 0, eit_tsid_nocheck = uuid["eit_tsid_nocheck"], iptv_respawn = uuid["iptv_respawn"], iptv_kill = uuid["iptv_kill"], iptv_kill_timeout = uuid["iptv_kill_timeout"], iptv_env = uuid["iptv_env"], iptv_hdr = uuid["iptv_hdr"], uuid = uuid["uuid"] };

            //data = new { enabled=uuid["enabled"], name= uuid["name"] ="BeIn Sports 9",number= 2595, services ="[\"50686628f3ad5de05b9b5fca389dc112\"]",
            //    tags ="[\"cc464c1ba8d83770c134cdc600ba6657\"]",autoname=true,icon="file=///home/pi/toshiba/logosChannels/dmtnbeinsports9.png",epgauto=true,epggrab="[]",
            //    dvr_pre_time =0,dvr_pst_time=0,epg_running=-1,epg_parent="", uuid ="5eb8a50fba0fafaa54e7c7876a6b996f"};

            var request = WebRequest.Create(uri);
            request.Credentials = new CredentialCache
            {
                {
                    new Uri(uri.GetLeftPart(UriPartial.Authority)), // request url's host
                    "Basic",  // authentication type 
                    new NetworkCredential(_username, _password) // credentials 
                }
            };

            //request.Headers.Add("Authorization", "basic ")
            request.Method = "POST";
            var reqContent = new { node = data };
            var byteArray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(reqContent).ToDataPostEncodedUrl2());
            // Set the ContentType property of the WebRequest.
            request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            // Set the ContentLength property of the WebRequest.
            request.ContentLength = byteArray.Length;

            var postStream = request.GetRequestStream();
            postStream.Write(byteArray, 0, byteArray.Length);
            postStream.Flush();
            postStream.Close();

            var response = request.GetResponse();
            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            var jsonObj = JObject.Parse(responseString);
            return jsonObj;
        }

        private MuxCorrectFormat GetMuxCorrectInfoFromUrl(string url)
        {
            var res = new MuxCorrectFormat { iptv_icon = string.Empty, iptv_epgid = "", iptv_tags = "", iptv_sname = "" };
            //tvg-id=\"\" tvg-logo=\"http://www.lyngsat-logo.com/logo/tv/ff/fox_crime_global.png\" tvg-name=\"\" group-title=\"UK\", Fox Crime ",
            foreach (var item in url.Split(' ').Where(x => !string.IsNullOrWhiteSpace(x)))
            {

                var valueKey = item.Split('=');
                switch (valueKey[0])
                {
                    case "tvg-logo":
                        res.iptv_icon = valueKey[1].Replace("\\", "").Replace("\"", "");
                        break;
                    case "tvg-name":
                    case "tvg-id":
                        res.iptv_epgid = valueKey[1].Replace("\\", "").Replace("\"", "");
                        break;
                    case "group-title":
                        res.iptv_tags = valueKey[1].Replace("\\", "").Replace("\"", "").Replace(',', ' ');
                        break;

                    default:
                        res.iptv_sname = valueKey[0].Trim().Replace('_', ' ').Replace('-', ' ').Replace(',', ' ');
                        break;
                }
            }
            return res;
        }

        #endregion

        #region EPG

        /// <summary>
        /// Get epg list
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public JToken EPG(QueryParams query)
        {
            //get channels from Tvheadend
            var response = MakeHttpRequest(query, Constants.API_URLS.EPG_LIST);
            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            var jsonObj = JObject.Parse(responseString);
            return jsonObj["entries"];
        }

        #endregion

        public void ClearImagesCache()
        {
            //clean:1
            var response = MakeHttpRequest(new GenericQuery(new Dictionary<string, object>() { { "clean", 1 } }), Constants.API_URLS.CHANNELS_LIST);
        }

        /// <summary>
        /// Make HttpRequest
        /// </summary>
        /// <param name="query"></param>
        /// <param name="pathUrl"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        private WebResponse MakeHttpRequest(IQuery query, string pathUrl, string method = "POST")
        {
            
            var uri = new Uri(_url + pathUrl);
            var request = WebRequest.Create(uri);
            request.Method = method;
            var credentialCache = new CredentialCache
            {
                {
                    new Uri(uri.GetLeftPart(UriPartial.Authority)), // request url's host
                    "Basic",  // authentication type 
                    new NetworkCredential(_username, _password) // credentials 
                }
            };

            request.Credentials = credentialCache;

            var byteArray = query.PostData;
            // Set the ContentType property of the WebRequest.
            request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            // Set the ContentLength property of the WebRequest.
            request.ContentLength = byteArray.Length;

            using (var postStream = request.GetRequestStream())
            {
                postStream.Write(byteArray, 0, byteArray.Length);
                return request.GetResponse();
            }
        }

        #region QueryProvider

        /// <summary>
        /// Execute provider
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public override object _(Expression expression)
        {
            Type elementType = TypeSystem.GetElementType(expression.Type);
            var isEnumerable = elementType.Name == "IEnumerable`1";

            // IQueryable queryable = Activator.CreateInstance(typeof(Playlist<>).MakeGenericType(elementType), this, expression) as IQueryable;

            var queryParams = new QueryParamsTranslator().Translate(expression);

            var queryResult = Channels(queryParams as QueryParams).Select(item => new TvgMedia(item["name"].ToString(), item["uuid"].ToString()) { Position = (int)item["number"] });

            return Activator.CreateInstance(
              typeof(Playlist<>).MakeGenericType(elementType),
              BindingFlags.Instance | BindingFlags.Public, null,
              new object[] { queryResult },
              null);
        }

        #endregion

        #region IPlaylistProvider
        public override IEnumerable<TvgMedia> Pull()
        {
            foreach (var item in Channels(QueryParams.ALL))
                yield return new TvgMedia(item["name"].ToString(), item["uuid"].ToString()) { Position = (int)item["number"] };
        }

        public override async Task<IEnumerable<TvgMedia>> PullAsync(CancellationToken token)
        {
            var nodes = await NodesAsync(QueryParams.ALL, API_URLS.CHANNELS_LIST, token);
            return nodes.Select(item => new TvgMedia(item["name"].ToString(), item["uuid"].ToString()) { Position = (int)item["number"] });
        }

        public override void Push(Playlist<TvgMedia> playlist)
        {
            throw new NotImplementedException();
        }

        public override Task PushAsync(Playlist<TvgMedia> playlist, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public override Playlist<TvgMedia> Sync(Playlist<TvgMedia> playlist)
        {
            throw new NotImplementedException();
        }

        public override Task<Playlist<TvgMedia>> SyncAsync(Playlist<TvgMedia> playlist, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public override void Dispose()
        {
        }

        #endregion
    }
}
