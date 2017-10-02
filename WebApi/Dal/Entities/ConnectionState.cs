﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace hfa.WebApi.Dal.Entities
{
    public class ConnectionState : EntityBase
    {
        //public int UserId { get; set; }

        //[ForeignKey(nameof(UserId))]
        //public virtual User User { get; set; }

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