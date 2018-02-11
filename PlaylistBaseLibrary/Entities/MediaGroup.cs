using System;
using System.Collections.Generic;
using System.Text;

namespace hfa.PlaylistBaseLibrary.Entities
{
    public class MediaGroup
    {
        public MediaGroup()
        {

        }

        public MediaGroup(string name)
        {
            Name = name;
        }
        public string Name { get; set; }
        public bool Disabled { get; set; } = false;
        public int Position { get; set; }

        /// <summary>
        /// Regex pattern to auto grouping medias
        /// </summary>
        public string MatchingMediaPattern { get; set; }
    }
}
