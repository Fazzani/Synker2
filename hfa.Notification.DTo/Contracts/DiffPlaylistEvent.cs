namespace hfa.Brokers.Messages.Contracts
{
    using PlaylistManager.Entities;
    using System;
    using System.Collections.Generic;

    public class DiffPlaylistEvent : ApplicationEvent
    {
        public int Id { get; set; }
        public IEnumerable<TvgMedia> RemovedMedias { get; set; }
        public IEnumerable<TvgMedia> NewMedias { get; set; }
        public int NewMediasCount { get; set; }
        public int RemovedMediasCount { get; set; }

        public override string ToString() => $"{CreatedDate}:{CorrelationId} => Playlist: {Id}, Newest : {NewMediasCount}, Removed : {RemovedMediasCount}";
    }
}
