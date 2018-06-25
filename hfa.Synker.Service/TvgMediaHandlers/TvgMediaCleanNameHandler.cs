using PlaylistBaseLibrary.ChannelHandlers;
namespace hfa.Synker.Service.Services.TvgMediaHandlers
{
    using PlaylistManager.Entities;
    using System.Linq;
    using System.Text.RegularExpressions;
    public class TvgMediaCleanNameHandler : TvgMediaHandler
    {
        public TvgMediaCleanNameHandler(IContextTvgMediaHandler contextTvgMediaHandler)
            : base(contextTvgMediaHandler)
        {

        }

        /// <summary>
        /// Clean DisplayName selon la config sauvegardée dans Elastic 
        /// </summary>
        /// <param name="tvgMedia"></param>
        public override void HandleTvgMedia(TvgMedia tvgMedia)
        {
            if (_contextTvgMediaHandler is ContextTvgMediaHandler context)
            {
                if (context.FixChannelNames != null && context.FixChannelNames.Any())
                {
                    context.FixChannelNames.AsParallel().OrderBy(x => x.Order).ForAll(pattern =>
                      {
                          var reg = new Regex(pattern.Pattern, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
                          if (reg.IsMatch(tvgMedia.Name))
                          {
                              tvgMedia.DisplayName = reg.Replace(tvgMedia.Name, pattern.ReplaceBy);
                          }
                      });
                }
            }
            if (_successor != null)
                _successor.HandleTvgMedia(tvgMedia);
        }
    }
}
