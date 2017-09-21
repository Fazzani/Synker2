using PlaylistManager.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Reflection;

namespace PlaylistManager.Services
{
    public class PlaylistManager : PlaylistManagerBase
    {
        public override Task CleanMediaNameAsync<TPlaylist>(TPlaylist playlist, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public override Task<(TPlaylist, TPlaylist, TPlaylist)> DiffAsync<TPlaylist>(TPlaylist source, TPlaylist target, CancellationToken token)
        {
            var trace = new TraceSource(Assembly.GetCallingAssembly().GetName().Name, SourceLevels.All);
            //Removed medias
            var removedMedias = new Playlist<TvgMedia>(source.Where(x => target.All(y => y != x)));
            //Newest medias
            var newestMedias = new Playlist<TvgMedia>(target.Where(x => source.All(y => y != x && string.Compare(x.Name, y.Name, true) != 0)));
            //Modified medias
            var modifiedMedias = new Playlist<TvgMedia>(source.Where(x => target.Any(y => y == x && string.Compare(x.Name, y.Name, true) != 0)));

            trace.Flush();
            trace.Close();
            token.ThrowIfCancellationRequested();

            return Task.FromResult((Removed : (TPlaylist)removedMedias, Newest: (TPlaylist)newestMedias, Modified: (TPlaylist)modifiedMedias));
        }

        public override Task GroupMediaAsync<TPlaylist>(TPlaylist playlist, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public override Task MatchEPGAsync<TPlaylist>(TPlaylist playlist, CancellationToken token)
        {
            throw new NotImplementedException();
        }
       
    }
}
