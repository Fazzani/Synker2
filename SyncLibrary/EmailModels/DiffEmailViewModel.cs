using PlaylistManager.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace hfa.Synker.batch.EmailModels
{
    public class DiffEmailViewModel: EmailViewModel
    {
        public IEnumerable<TvgMedia>  RemovedMedias { get; set; }
        public IEnumerable<TvgMedia> AddedMedias { get; set; }

        public string PlaylistName { get; set; }
        public string UserName { get; internal set; }
    }
}
