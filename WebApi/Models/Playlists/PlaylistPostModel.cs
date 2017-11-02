using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace hfa.WebApi.Models.Playlists
{
    public class PlaylistPostModel
    {
        public string PlaylistName { get; set; }
        [Required]
        public string Provider { get; set; }
        [Required]
        public string PlaylistUrl { get; set; }
    }
}
