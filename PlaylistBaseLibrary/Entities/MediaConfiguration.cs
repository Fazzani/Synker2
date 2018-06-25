namespace Hfa.PlaylistBaseLibrary.Entities
{
    using System.Collections.Generic;
    public class MediaConfiguration
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IList<Tag> Tags { get; set; }
        public IList<string> HeadLinePattern { get; set; }
        public IList<FixChannelName> FixChannelNames { get; set; }
        public IList<char> DisplayChannelNameCharsToRemove { get; set; }
        public IList<string> DisplayChannelNameStringsToRemove { get; set; }
        public IList<string> EpgStringsToRemove { get; set; }
        public bool RemoveHeadLines { get; set; }
        public IList<Quality> Qualities { get; set; }
        public string PiconsPath { get; set; }
        /// <summary>
        /// Une fausse media
        /// </summary>
        public string StartChannelsHeadLinePattern { get; set; }
              
    }

    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsDefault { get; set; }
        public int Position { get; set; }
        public IList<string> ChannelPatternMatcher { get; set; }
        public IList<string> HeadLinesMatcher { get; set; }
        public IList<string> Lang { get; set; }
    }

    public class FixChannelName
    {
        public int Id { get; set; }
        public string Pattern { get; set; }
        public string ReplaceBy { get; set; }
        public int Order { get; set; }
    }

    public class Quality
    {
        public string Name { get; set; }
        public IList<string> MatcherPattern { get; set; }
        public int Priority { get; set; }
        public bool IsDefault { get; set; }
    }
    
}
