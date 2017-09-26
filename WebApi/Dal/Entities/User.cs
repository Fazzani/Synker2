using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace hfa.WebApi.Dal.Entities
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(64)]
        [Required]
        public string FirstName { get; set; }

        [MaxLength(64)]
        [Required]
        public string LasrName { get; set; }

        [Required]
        public string Email { get; set; }

        public string Photo { get; set; }

        public DateTime BirthDay { get; set; }

       // public virtual JsonObject<List<string>> Tags { get; set; }
    }
}
