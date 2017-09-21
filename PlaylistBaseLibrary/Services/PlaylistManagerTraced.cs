using PlaylistManager.Entities;
using PlaylistManager.Providers;
using System;
using System.Collections.Generic;
using System.Text;
using PlaylistManager.Services;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Reflection;

namespace PlaylistBaseLibrary.Services
{
    public class PlaylistManagerTraced : PlaylistDecorator
    {
        readonly TraceSource _traceSource;

        public PlaylistManagerTraced(PlaylistManagerBase playlistManager) : base(playlistManager)
        {
            _traceSource = new TraceSource(Assembly.GetCallingAssembly().GetName().Name, SourceLevels.All);
        }

        public override Task CleanMediaNameAsync<TPlaylist>(TPlaylist playlist, CancellationToken token)
        {
            _traceSource.TraceInformation($"CleanMediaNameAsync :  {playlist.Name}");
            var res =  base.CleanMediaNameAsync(playlist, token);
            _traceSource.TraceInformation($"CleanMediaNameAsync : was called");
            return res;
        }

        public override Task<(TPlaylist, TPlaylist, TPlaylist)> DiffAsync<TPlaylist>(TPlaylist playlist1, TPlaylist playlist2, CancellationToken token)
        {
            _traceSource.TraceInformation($"Diff : between {playlist1.Name} and {playlist2.Name}");
            var taskDiff = base.DiffAsync(playlist1,  playlist2, token);
            _traceSource.TraceInformation($"Diff : was called");
            return taskDiff;
        }

        public override Task GroupMediaAsync<TPlaylist>(TPlaylist playlist, CancellationToken token)
        {
            _traceSource.TraceInformation($"GroupMedia : {playlist.Name}");
            var res = base.GroupMediaAsync(playlist, token);
            _traceSource.TraceInformation($"GroupMedia : was called");
            return res;
        }

        public override Task MatchEPGAsync<TPlaylist>(TPlaylist playlist, CancellationToken token)
        {
            _traceSource.TraceInformation($"MatchEPG : {playlist.Name}");
            var res = base.MatchEPGAsync(playlist, token);
            _traceSource.TraceInformation($"MatchEPG : was called");
            return res;
        }

    }
}
