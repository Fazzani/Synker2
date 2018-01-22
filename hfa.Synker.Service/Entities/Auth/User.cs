using hfa.Synker.Service.Entities.Playlists;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace hfa.Synker.Service.Entities.Auth
{
    public class User : EntityBase
    {
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

        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public virtual ICollection<Command> Commands { get; set; } = new List<Command>();
        public virtual ICollection<Playlist> Playlists { get; set; }

        [NotMapped]
        public IEnumerable<string> Roles
        {
            get
            {
                if (UserRoles != null && UserRoles.Any())
                    return UserRoles.Select(x => x.Role.Name);
                return null;
            }
        }
        // public virtual JsonObject<List<string>> Tags { get; set; }
    }
    public enum GenderTypeEnum : byte
    {
        Mr = 0,
        Mrs
    }
}
