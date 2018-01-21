using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace hfa.Synker.Service.Entities.Auth
{
    public class Role : EntityBase
    {
       public const string ADMIN_ROLE_NAME = "Administrator";
       public const string GUEST_ROLE_NAME = "Guest";
       public const string DEFAULT_ROLE_NAME = "Default";

        [MaxLength(32)]
        public string Name { get; set; } = DEFAULT_ROLE_NAME;

        public static Role CreateAdminRole() => new Role { Name = ADMIN_ROLE_NAME };
        public static Role CreateGuestRole() => new Role { Name = GUEST_ROLE_NAME };
        public static Role CreateDefaultRole() => new Role { Name = DEFAULT_ROLE_NAME };

        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}