using PlaylistManager.Entities;
using System.Collections.Generic;
using System.Linq;

namespace hfa.Brokers.Messages.Contracts
{
    public class DiffPlaylistEvent : ApplicationEvent
    {
        public int Id { get; set; }

        public IEnumerable<TvgMedia> RemovedMedias { get; set; }
        public IEnumerable<TvgMedia> NewMedias { get; set; }

        public override string ToString() => $"{CreatedDate}: Playlist: {Id}, Newest : {NewMedias.Count()}, Removed : {RemovedMedias.Count()}";
    }
}
