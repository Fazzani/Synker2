namespace hfa.synker.entities.Notifications
{
    using hfa.Synker.Service.Entities;
    using hfa.Synker.Service.Entities.Auth;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Device : EntityBase
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string PushEndpoint { get; set; }
        [Required]
        public string PushP256DH { get; set; }
        [Required]
        public string PushAuth { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; }
        public int? UserId { get; set; }
    }
}
