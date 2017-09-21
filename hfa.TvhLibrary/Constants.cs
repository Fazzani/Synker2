using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TvheadendLibrary
{
    public static class Constants
    {
        public class API_URLS
        {
            public const string CHANNELS_LIST = "api/channel/grid";
            public const string CLEAR_CACHE = "/api/imagecache/config/clean";
            internal static string EPG_LIST = "/api/epggrab/channel/grid";
            internal static string UPDATE = "api/idnode/save";
        }

        public static class KEYS
        {
            public const string CHARS_TO_REMOVE = "charsToRemove";
            public const string STRINGS_TO_REMOVE = "stringsToRemove";
        }
    }
}
