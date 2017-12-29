using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using hfa.Synker.Service.Entities.MediaScraper;

namespace hfa.Synker.Service.Services.Scraper
{
    public interface IMediaScraper
    {
        Task<List<MediaInfo>> SearchAsync(string term, string apiKey, CancellationToken cancellationToken);
    }
}