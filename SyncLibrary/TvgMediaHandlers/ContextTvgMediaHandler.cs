using PlaylistBaseLibrary.ChannelHandlers;
using PlaylistManager.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Hfa.PlaylistBaseLibrary.Entities;
using Nest;

namespace SyncLibrary.TvgMediaHandlers
{
    class ContextTvgMediaHandler : IContextTvgMediaHandler
    {
        public ContextTvgMediaHandler()
        {
            LastLang = "Ar";
        }
        public string LastLang { get; set; }
        public string StartChannelsHeadLineLastMatched { get; set; }
        public MediaConfiguration MediaConfiguration { get; internal set; }
    }


    public class StartChannelsHeadLinePattern
    {
        public string Pattern { get; set; }
        public TvgMedia Media { get; set; }
    }
}
