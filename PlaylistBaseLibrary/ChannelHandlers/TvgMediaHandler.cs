
namespace hfa.PlaylistBaseLibrary.ChannelHandlers
{
    using PlaylistManager.Entities;
    public abstract class TvgMediaHandler
    {
        protected IContextTvgMediaHandler _contextTvgMediaHandler;
        public TvgMediaHandler(IContextTvgMediaHandler contextTvgMediaHandler)
        {
            _contextTvgMediaHandler = contextTvgMediaHandler;
        }
        protected TvgMediaHandler _successor;

        public void SetSuccessor(TvgMediaHandler successor)
        {
            _successor = successor;
        }

        public abstract void HandleTvgMedia(TvgMedia tvgMedia);
    }
}