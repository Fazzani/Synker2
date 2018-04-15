namespace hfa.SyncLibrary
{
    using Hfa.SyncLibrary;
    using Microsoft.Extensions.Logging;
    using Nest;
    using PlaylistManager.Entities;
    using System;

    namespace Global
    {

        public static class Extentions
        {
            public static void AssertElasticResponse(this IResponse response)
            {
                //Debug.Assert(response.IsValid);
                if (!response.IsValid)
                    Common.Logger("Elastic").LogError(response.DebugInformation);
            }
        }
        
        public class Common
        {
            public static ILogger Logger(string cat = "default") => _LoggerFactory.CreateLogger(cat);
            private static ILoggerFactory _LoggerFactory = null;

            static Common()
            {
                _LoggerFactory = (ILoggerFactory)Init.ServiceProvider.GetService(typeof(ILoggerFactory));
            }

            internal static void DisplayList(Playlist<TvgMedia> pl, Action<string> logAction, string message)
            {
                logAction?.Invoke(message);
                Logger().LogInformation(message);
                if (pl != null)
                    Logger().LogInformation(message);
                //  logAction?.Invoke(pl.ToString(false));
            }
        }

    }
}
