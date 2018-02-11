using PlaylistManager.Entities;
using System;

namespace hfa.PlaylistBaseLibrary.Providers
{
    public interface IProviderFactory
    {
        PlaylistProvider<Playlist<TvgMedia>, TvgMedia> CreateInstance(string playlistUrl, string provider = "m3u");
        PlaylistProvider<Playlist<TvgMedia>, TvgMedia> CreateInstance(Uri playlistUrl, string provider = "m3u");
    }
}