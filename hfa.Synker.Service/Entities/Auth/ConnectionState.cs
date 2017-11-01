using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace hfa.Synker.Service.Entities.Auth
{
    public class ConnectionState : EntityBase
    {
        [Required]
        [MaxLength(512)]
        public string UserName { get; set; }

        [Required]
        [MaxLength(512)]
        public string Password { get; set; }

        public DateTime LastConnection { get; set; } = DateTime.Now;

        [MaxLength(255)]
        public string RefreshToken { get; set; }

        public string AccessToken { get; set; }

        public bool Disabled { get; set; } = false;

        public bool Approved { get; set; } = true;
    }
}
