using PlaylistManager.Entities;
using System.Collections.Generic;

namespace hfa.Brokers.Messages.Contracts
{
    public class DiffPlaylistEvent : ApplicationEvent
    {
        public int Id { get; set; }

        public IEnumerable<TvgMedia> RemovedMedias { get; set; }
        public IEnumerable<TvgMedia> NewMedias { get; set; }
        
    }
}
