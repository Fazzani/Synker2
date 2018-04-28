using System.Collections.Generic;
using PastebinAPI;
using System.Threading.Tasks;

namespace hfa.WebApi.Services
{
    public interface IPasteBinService
    {
        /// <summary>
        /// Create a public paste
        /// </summary>
        /// <param name="text"></param>
        /// <param name="title"></param>
        /// <param name="language"></param>
        /// <param name="visibility"></param>
        /// <param name="expiration"></param>
        /// <returns></returns>
        Task<Paste> CreateAsync(string text, string title = null, Language language = null, Visibility visibility = Visibility.Public, Expiration expiration = null);

        /// <summary>
        /// Delete a paste
        /// </summary>
        /// <param name="paste"></param>
        Task DeleteAsync(Paste paste);

        /// <summary>
        /// List of paste
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        Task<IEnumerable<Paste>> ListAsync(int count = 50);

        /// <summary>
        /// Trending pastes
        /// </summary>
        Task ListTrendingPastesAsync();

        /// <summary>
        /// Push with your account a paste
        /// </summary>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <param name="expiration"></param>
        /// <param name="language"></param>
        /// <param name="visibility"></param>
        /// <returns></returns>
        Task<Paste> PushAsync(string title, string content, Expiration expiration, Language language, Visibility visibility = Visibility.Private);

        /// <summary>
        /// Delete by paste title
        /// </summary>
        /// <param name="title"></param>
        /// <returns>True if paste exist</returns>
        Task<bool> DeleteAsync(string title);
    }
}