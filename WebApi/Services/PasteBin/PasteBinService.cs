using hfa.WebApi.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PastebinAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace hfa.WebApi.Services
{
    public class PasteBinService : IPasteBinService
    {
        private readonly PastBinOptions _pasteBinOptions;
        private readonly User _me;
        private readonly ILogger _logger;

        public PasteBinService(IOptions<PastBinOptions> pasteBinOptions, ILoggerFactory loggerFactory)
        {
            _pasteBinOptions = pasteBinOptions?.Value;
            Pastebin.DevKey = _pasteBinOptions.UserKey;
            _me = Pastebin.LoginAsync(_pasteBinOptions.UserName, _pasteBinOptions.Password).GetAwaiter().GetResult();
            _logger = loggerFactory.CreateLogger(typeof(PasteBinService));
        }

        public async Task<Paste> PushAsync(string title, string content, Expiration expiration, Language language, Visibility visibility = Visibility.Private)
        {
            try
            {
                return await _me.CreatePasteAsync(content, title, language, visibility, expiration);
            }
            catch (PastebinException ex) //api throws PastebinException
            {
                if (ex.Parameter == PastebinException.ParameterType.Login)
                {
                    _logger.LogError("Invalid username/password");
                }
                else
                {
                    throw; //all other types are rethrown and not swalowed!
                }
            }
            return null;
        }

        public async Task<IEnumerable<Paste>> ListAsync(int count = 50) => await _me.ListPastesAsync(count);
        public async Task DeleteAsync(Paste paste) => await _me.DeletePasteAsync(paste);

        /// <summary>
        /// Delete by paste title
        /// </summary>
        /// <param name="title"></param>
        /// <returns>True if paste exist</returns>
        public async Task<bool> DeleteAsync(string title)
        {
            var paste = (await _me.ListPastesAsync(1000)).FirstOrDefault(x => x.Title.Equals(title, StringComparison.InvariantCultureIgnoreCase));
            if (paste != null)
            {
                await _me.DeletePasteAsync(paste);
                return true;
            }
            return false;
        }
        public async Task ListTrendingPastesAsync() => await Pastebin.ListTrendingPastesAsync();

        /// <summary>
        /// Create public paste
        /// </summary>
        /// <param name="text"></param>
        /// <param name="title"></param>
        /// <param name="language"></param>
        /// <param name="visibility"></param>
        /// <param name="expiration"></param>
        /// <returns></returns>
        public async Task<Paste> CreateAsync(string text, string title = null, Language language = null, Visibility visibility = Visibility.Public, Expiration expiration = null)
            => await Paste.CreateAsync(text, title, language, visibility, expiration);
    }
}
