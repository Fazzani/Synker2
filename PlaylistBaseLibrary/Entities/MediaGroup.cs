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

    public class GroupComparerByName : IEqualityComparer<MediaGroup>
    {
        public bool Equals(MediaGroup x, MediaGroup y)
        {
            if (x == null && y == null)
                return true;
            return x?.Name?.Equals(y?.Name, StringComparison.InvariantCultureIgnoreCase) ?? true;
        }

        public int GetHashCode(MediaGroup obj) => obj?.Name?.GetHashCode() ?? 0;

        public static GroupComparerByName Factory => new GroupComparerByName();
    }

}
