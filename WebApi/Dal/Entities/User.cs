using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace hfa.WebApi.Dal.Entities
{
    public class User: EntityBase
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
        public string UserName { get;  set; }

        public string Password { get; set; }

        public DateTime LastConnection { get; set; } = DateTime.Now;
        public virtual ICollection<Role> Roles { get; set; }
        // public virtual JsonObject<List<string>> Tags { get; set; }
    }
}
