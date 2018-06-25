namespace hfa.Synker.Service.Entities.Auth
{
    public class UserRole : EntityBaseAudit
    {
        public int UserId { get; set; }
        public User User { get; set; }
        public int RoleId { get; set; }
        public Role Role { get; set; }
    }
}
