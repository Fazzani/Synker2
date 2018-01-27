using hfa.PlaylistBaseLibrary.Options;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Options;
using PlaylistManager.Entities;
using System.Linq;
using hfa.PlaylistBaseLibrary.Exceptions;

namespace hfa.PlaylistBaseLibrary.Providers
{
    public class ProviderFactory : IProviderFactory
    {
        private List<PlaylistProviderOption> _providersOptions;

        public ProviderFactory(IOptions<List<PlaylistProviderOption>> options)
        {
            _providersOptions = options.Value;
        }

        /// <summary>
        /// Create provider instance from provider name
        /// </summary>
        /// <param name="playlistUrl"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public PlaylistProvider<Playlist<TvgMedia>, TvgMedia> CreateInstance(string playlistUrl, string provider = "m3u")
        {
            if (string.IsNullOrEmpty(playlistUrl))
                throw new ArgumentNullException(nameof(playlistUrl));

            return CreateInstance(new Uri(playlistUrl), provider);
        }

        public PlaylistProvider<Playlist<TvgMedia>, TvgMedia> CreateInstance(Uri playlistUrl, string provider = "m3u")
        {

            var optionsProvider = _providersOptions.FirstOrDefault(x => x.Name.Equals(provider, StringComparison.InvariantCultureIgnoreCase));
            if (optionsProvider == null)
                throw new NotSupportedProviderException($"Not supported Provider : {provider}");

            var providerType = Type.GetType(optionsProvider.Type, false, true);
            if (providerType == null)
                throw new NotFoundProviderException($"Provider type not found : {provider}");

            var providerInstance = (PlaylistProvider<Playlist<TvgMedia>, TvgMedia>)Activator.CreateInstance(providerType, playlistUrl);
            return providerInstance;
        }
    }
}
