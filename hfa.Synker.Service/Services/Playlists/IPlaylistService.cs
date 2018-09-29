using System.Collections.Generic;
using System.Threading.Tasks;
using hfa.Synker.Service.Entities.Playlists;
using System.Threading;
using hfa.PlaylistBaseLibrary.Providers;
using System;
using PlaylistManager.Entities;
using System.IO;
using hfa.Brokers.Messages;

namespace hfa.Synker.Service.Services.Playlists
{
    public interface IPlaylistService
    {
        Task<IEnumerable<Playlist>> ListByUserAsync(int userId);
        Task<Playlist> SynkPlaylistAsync(Func<Playlist> getPlaylist, PlaylistProvider<Playlist<TvgMedia>, TvgMedia> provider, bool isXtreamPlaylist, bool force = false,
            CancellationToken cancellationToken = default);

        Task<Playlist> SynkPlaylistAsync(Playlist playlist, bool resetAndSynch = false,
            CancellationToken cancellationToken = default);
        /// <summary>
        /// Genére un rapport avec les new medias et 
        /// les médias qui n'existes plus
        /// </summary>
        /// <param name="getPlaylist"></param>
        /// <param name="force"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<(IEnumerable<TvgMedia> tvgMedia, IEnumerable<TvgMedia> removed)> DiffWithSourceAsync(Func<Playlist> getPlaylist, PlaylistProvider<Playlist<TvgMedia>,
            TvgMedia> provider, bool force = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Synchronize playlist
        /// </summary>
        /// <param name="playlistId"></param>
        /// <param name="resetAndsynch">Reset and synchronize playlist from scrach</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Playlist> SynkPlaylistAsync(int playlistId, bool resetAndsynch = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Execute Handlers on Tvgmedias list
        /// </summary>
        /// <returns></returns>
        List<TvgMedia> ExecuteHandlersAsync(List<TvgMedia> tvgmedias, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get playlist health state
        /// </summary>
        /// <param name="pl"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<PlaylistHealthState> HealthAsync(Playlist pl, CancellationToken cancellationToken);
    }
}