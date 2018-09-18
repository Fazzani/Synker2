namespace hfa.Brokers.Messages.Contracts
{
    using PlaylistManager.Entities;
    using System;
    using System.Collections.Generic;

    public class DiffPlaylistEvent : ApplicationEvent
    {
        
        public int Id { get; set; }
        public int UserId { get; set; }
        public string PlaylistName { get; set; }

        public IEnumerable<TvgMedia> RemovedMedias { get; set; }
        public IEnumerable<TvgMedia> NewMedias { get; set; }
        public int NewMediasCount { get; set; }
        public int RemovedMediasCount { get; set; }

        public bool Changed { get { return NewMediasCount > 0 || RemovedMediasCount > 0; } }

        public override string ToString() => $"{CreatedDate}:{CorrelationId} => Playlist: {Id}, Newest : {NewMediasCount}, Removed : {RemovedMediasCount}";
    }
}
