using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace hfa.WebApi.Models
{
    public class MatchTvgPostModel
    {
        public MatchTvgPostModel()
        {
            MinScore = 0.5;
            OverrideTvg = false;
        }

        /// <summary>
        /// Media name
        /// </summary>
        [Required]
        public string MediaName { get; set; }

        /// <summary>
        /// Site pack name
        /// </summary>
        public IEnumerable<string> TvgSites { get; set; }

        /// <summary>
        /// Country
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// Force override tvg
        /// </summary>
        [Required]
        public bool OverrideTvg { get; set; }

        /// <summary>
        /// Distance matching minimum
        /// </summary>
        [Required]
        [Range(0, 1)]
        public double MinScore { get; set; }

    }

    [Flags]
    enum MatchingTvgType
    {
        ByCountry = 1,
        BySites = 2,
    }
}
