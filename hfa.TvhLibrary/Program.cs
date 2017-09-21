using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TvheadendLibrary
{
    class Program
    {
        /*
         * RAF:
         * 
         * ---Try match logo and update channel
         * ---Load all EPG channel from Tvheadend
         * Try match EPG and update channel and m3u file
         * Log all to actions
         * Save all in SQLLITE database
         * Merge channels and set priority
         * --Add all parameters to config file
         * auto chech muxes
         * output m3u file
         * Mono compatibility
         */

        static void Main(string[] args)
        {
            //using (var client = new SshClient("192.168.1.29", 22, "pi", "Fezzeni82"))
            //{
            //    client.Connect();
            //    var cm1 = client.RunCommand("ls -l");
            //    Console.WriteLine(cm1.Result);
            //}


            //var res = "bein sports ar +2".RemoveRegex(XmltTv.Infrastructure.Constants.RegexsToRemove).RemoveStrings(XmltTv.Infrastructure.Constants.StringsToRemove);
            //var res1 = "bein sports 10ar ee +2hd".RemoveRegex(XmltTv.Infrastructure.Constants.RegexsToRemove).RemoveStrings(XmltTv.Infrastructure.Constants.StringsToRemove);
            //var res2 = "bein sports aree".RemoveRegex(XmltTv.Infrastructure.Constants.RegexsToRemove).RemoveStrings(XmltTv.Infrastructure.Constants.StringsToRemove);
            //var res3 = "bein sports hd10".RemoveRegex(XmltTv.Infrastructure.Constants.RegexsToRemove).RemoveStrings(XmltTv.Infrastructure.Constants.StringsToRemove);
            //var res4 = "bein sportsSD 02hd".RemoveRegex(XmltTv.Infrastructure.Constants.RegexsToRemove).RemoveStrings(XmltTv.Infrastructure.Constants.StringsToRemove);
            //var res5 = "bein sports hd01".RemoveRegex(XmltTv.Infrastructure.Constants.RegexsToRemove).RemoveStrings(XmltTv.Infrastructure.Constants.StringsToRemove);

          //  var tvheadendService = new TvheadendService(ConfigurationManager.AppSettings["TVHEADEND_URL"], ConfigurationManager.AppSettings["USER_NAME"], ConfigurationManager.AppSettings["PWD"]);

            //tvheadendService.TryFixLogos(@"\\FREEBOX\toshiba ext\logosChannels", "default.png");

            // tvheadendService.TryFixEPG();

            //foreach (var chaine in tvheadendService.ChannelsWithoutEPG())
            //    $"EPG not found for the channel {chaine["name"]}".InfoConsole();

            //var channelsWithoutLogos = tvheadendService.ChannelsWithoutLogo(@"\\FREEBOX\toshiba ext\logosChannels");

            //var channelsWithoutLogo = new StringBuilder();
            //foreach (var chaine in channelsWithoutLogos)
            //{
            //    $"Traitement de la chaine {chaine["name"].ToString().Replace("{", "-").Replace("}","-")}".InfoConsole();
            //    channelsWithoutLogo.Append($"{chaine["name"].ToString()}{Environment.NewLine}");
            //    //   DownloadFile(chaine["icon_public_url"].ToString(), args.Count() > 0 ? args[0] : Environment.CurrentDirectory);
            //}
            //Utility.Push("Channels without logo", channelsWithoutLogo.ToString());

            //var channels = tvheadendService.Channels(QueryParams.Default);
            ////On Parcour la liste des chaines
            //foreach (var chaine in channels)
            //{
            //    $"Traitement de la chaine {chaine["name"]}".InfoConsole();
            //    //   DownloadFile(chaine["icon_public_url"].ToString(), args.Count() > 0 ? args[0] : Environment.CurrentDirectory);
            //}

            ////DownloadFile("http://www.lyngsat-logo.com/logo/tv/mm/m_net_jp.png");

            Console.ReadKey();
        }

        //private static void DownloadFile(string url, string dirPath = "")
        //{
        //    try
        //    {
        //        using (var wc = new WebClient())
        //        {
        //            wc.Headers["Accept"] = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
        //            wc.Headers.Add("Accept-Encoding", "gzip, deflate, sdch");
        //            //wc.Headers.Add("Upgrade-Insecure-Requests", "1");
        //            wc.Headers.Add("Accept-Language", "fr-FR,fr;q=0.8,en-US;q=0.6,en;q=0.4");
        //            wc.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36");
        //            var splited = url.Split('/');
        //            var fileName = splited.LastOrDefault()?.Replace(".html", string.Empty);
        //            wc.DownloadData(url);   //, dirPath + fileName
        //        }
        //    }
        //    catch (Exception)
        //    {

        //    }

        //}
    }
}
