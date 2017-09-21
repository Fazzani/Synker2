using PlaylistBaseLibrary.ChannelHandlers;
using PlaylistManager.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SyncLibrary.TvgMediaHandlers
{
    internal class TvgMediaCleanNameHandler : TvgMediaHandler
    {
        public TvgMediaCleanNameHandler(IContextTvgMediaHandler contextTvgMediaHandler) : base(contextTvgMediaHandler)
        {

        }

        /// <summary>
        /// Clean DisplayName selon la config sauvegradée dans l'elastic 
        /// </summary>
        /// <param name="tvgMedia"></param>
        public override void HandleTvgMedia(TvgMedia tvgMedia)
        {
            if (_contextTvgMediaHandler is ContextTvgMediaHandler context)

                if (context.MediaConfiguration != null)
                {
                    tvgMedia.DisplayName = tvgMedia.DisplayName
                    .ToLowerInvariant()
                    .RemoveChars(context.MediaConfiguration.DisplayChannelNameCharsToRemove.ToArray())
                    .RemoveStrings(context.MediaConfiguration.DisplayChannelNameStringsToRemove.ToArray())
                    .RemoveDiacritics();
                }
            if (_successor != null)
                _successor.HandleTvgMedia(tvgMedia);
        }
    }
}
