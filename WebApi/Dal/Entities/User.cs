﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace hfa.WebApi.Dal.Entities
{
    public class User : EntityBase
    {
        public User()
        {
            Roles = new List<Role>();
        }
        [MaxLength(64)]
        [Required]
        public string FirstName { get; set; }

        [MaxLength(64)]
        [Required]
        public string LastName { get; set; }

        [Required]
        public string Email { get; set; }

        public string Photo { get; set; }

        public DateTime BirthDay { get; set; }

        public GenderTypeEnum Gender { get; set; } = GenderTypeEnum.Mr;

        public int ConnectionStateId { get; set; }

        [ForeignKey(nameof(ConnectionStateId))]
        public virtual ConnectionState ConnectionState { get; set; }

        public virtual ICollection<Role> Roles { get; set; }
        // public virtual JsonObject<List<string>> Tags { get; set; }
    }
    public enum GenderTypeEnum : byte
    {
        Mr = 0,
        Mrs
    }
}