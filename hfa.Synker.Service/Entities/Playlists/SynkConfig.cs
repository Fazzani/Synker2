using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace hfa.Synker.Service.Entities.Playlists
{
    public class SynkConfig
    {
        [MaxLength(10)]
        [RegularExpression(@"(28|\*) (2|\*) (7|\*) (1|\*) (1|\*)")]
        public string Cron { get; set; } = "* * * * 1";

        public bool SynkLogos { get; set; }

        public bool SynkEpg { get; set; }

        public SynkGroupEnum SynkGroup { get; set; } = SynkGroupEnum.None;

        public bool CleanName { get; set; }
    }

    public enum SynkGroupEnum : byte
    {
        None = 0,
        ByCountry = 1,
        ByLanguage = 2
    }
}
