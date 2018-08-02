using System;
namespace hfa.WebApi.Models.MediaServer
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Threading.Tasks;

    public class MediaServerLivePost
    {
        /// <summary>
        /// Stream Source
        /// </summary>
        [Required]
        public string Stream { get; set; }
    }
}
