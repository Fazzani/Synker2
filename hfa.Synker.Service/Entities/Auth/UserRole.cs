using System;
using System.Collections.Generic;
using System.Text;

namespace hfa.Synker.Service.Entities.Auth
{
    public class UserRole : EntityBase
    {
        public int UserId { get; set; }
        public User User { get; set; }
        public int RoleId { get; set; }
        public Role Role { get; set; }
    }
}
