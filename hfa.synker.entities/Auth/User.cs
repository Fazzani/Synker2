namespace hfa.Synker.Service.Entities.Auth
{
    using hfa.synker.entities.Notifications;
    using hfa.Synker.Service.Entities.Playlists;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    public class User : EntityBase
    {
        [Required]
        public string Email { get; set; }

        public virtual ICollection<Command> Commands { get; set; } = new List<Command>();
        public virtual ICollection<Playlist> Playlists { get; set; }

        public virtual ICollection<Device> Devices { get; set; } = new List<Device>();

    }
}
