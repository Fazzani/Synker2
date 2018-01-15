using System.Collections.Generic;
using System.Threading.Tasks;
using hfa.Synker.Service.Entities.Playlists;
using System.Threading;
using hfa.PlaylistBaseLibrary.Providers;
using System;
using PlaylistManager.Entities;
using System.IO;

namespace hfa.Synker.Service.Services.Playlists
{
    public interface IPlaylistService
    {
        Task<IEnumerable<Playlist>> ListByUserAsync(int userId);
        Task<Playlist> SynkPlaylist(Func<Playlist> getPlaylist, FileProvider provider, bool isXtreamPlaylist, bool force = false,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Genére un rapport avec les new medias et 
        /// les médias qui n'existes plus
        /// </summary>
        /// <param name="id"></param>
        /// <param name="force"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<(IEnumerable<TvgMedia> tvgMedia, IEnumerable<TvgMedia> removed)> DiffWithSource(Func<Playlist> getPlaylist, FileProvider provider, bool force = false,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Execute Handlers on Tvgmedias list
        /// </summary>
        /// <returns></returns>
        List<TvgMedia> ExecuteHandlersAsync(List<TvgMedia> tvgmedias, CancellationToken cancellationToken = default);

    }
}