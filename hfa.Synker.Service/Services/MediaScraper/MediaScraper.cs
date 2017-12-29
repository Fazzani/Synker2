using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TMDbLib.Client;
using System.Linq;
using hfa.Synker.Service.Entities.MediaScraper;

namespace hfa.Synker.Service.Services.Scraper
{
    public class MediaScraper : IMediaScraper
    {
        public async Task<List<MediaInfo>> SearchAsync(string term, string apiKey, CancellationToken cancellationToken)
        {
            List<MediaInfo> listMediaInfo = null;
            var client = new TMDbClient(apiKey);
            var searchResult = await client.SearchListAsync(term);

            if (searchResult.TotalResults > 0)
                listMediaInfo = searchResult.Results.Select(x => new MediaInfo { Description = x.Description, Title = x.Name, PosterPath = x.PosterPath, Id = x.Id }).ToList();
            return listMediaInfo;
        }

    }
}
