using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TMDbLib.Client;
using System.Linq;
using hfa.Synker.Service.Entities.MediaScraper;
using TMDbLib.Objects.Search;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace hfa.Synker.Service.Services.Scraper
{
    public class MediaScraper : IMediaScraper
    {
        private ILogger _logger;

        public MediaScraper(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(nameof(MediaScraper));
        }

        public async Task<List<MediaInfo>> SearchAsync(string term, string apiKey, string posterBaseUrl,  CancellationToken cancellationToken)
        {
            List<MediaInfo> listMediaInfo = null;
            try
            {
                var client = new TMDbClient(apiKey);
                var cleanTerm = term.Replace(".", " ");
                cleanTerm = Regex.Replace(cleanTerm, @"\b\d{4}\b", string.Empty, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);//YEAR
                cleanTerm = Regex.Replace(cleanTerm, @"\bs\d{1,2}ep?\d{1,2}\b", string.Empty, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);//SEASON EPISODE,,
                cleanTerm = Regex.Replace(cleanTerm, @"\b(\d{3,4}p)(h|s)d(tv)?|4k\b", string.Empty, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);//QUALITY
                cleanTerm = cleanTerm.RemoveStrings("french", "avi", "dvdrip", "xvid", "bluray", "hdcam", "cam", "vostfr", "fr", "ar","en");

                _logger.LogDebug($"scraping input {term} => {cleanTerm}");

                var searchResult = await client.SearchMultiAsync(cleanTerm);

                if (searchResult.TotalResults > 0)
                {
                    listMediaInfo = searchResult.Results.Select(x =>
                    {
                        if (x is SearchMovie movie)
                        {
                            return new MediaInfo { MediaType = x.MediaType, Description = movie.Overview, Title = movie.Title, PosterPath = $"{posterBaseUrl}{movie.PosterPath}", Id = x.Id.ToString() };
                        }
                        else if (x is SearchTv tv)
                        {
                            return new MediaInfo { MediaType = x.MediaType, Description = tv.Overview, Title = tv.Name, PosterPath = $"{posterBaseUrl}{tv.PosterPath}", Id = x.Id.ToString() };
                        }
                        return new MediaInfo { MediaType = x.MediaType, Id = x.Id.ToString() };
                    }).ToList();
                }
            }
            catch (Exception)
            {
            }
            return listMediaInfo;
        }

    }
}
