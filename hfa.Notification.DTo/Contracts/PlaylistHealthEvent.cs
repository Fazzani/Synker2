namespace hfa.Brokers.Messages.Contracts
{
    using PlaylistManager.Entities;
    using System;
    using System.Collections.Generic;

    public class PlaylistHealthEvent : ApplicationEvent
    {
        public int Id { get; set; }

        public string PlaylistName { get; set; }

        public bool IsOnline { get; set; }
        public int MediaCount { get; set; }

        public override string ToString() => $"{CreatedDate}:{CorrelationId} => Playlist: {Id}, IsOnline : {IsOnline}";
    }
}
