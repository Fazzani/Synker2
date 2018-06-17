namespace hfa.WebApi.Models.Notifications
{
    using System.ComponentModel.DataAnnotations;
    public class PushSubscriptionModel
    {
        [Required]
        public string EndPoint { get; set; }
        public string ExpirationTime { get; set; }
        [Required]
        public Keys Keys { get; set; }

    }

    public class Keys
    {
        [Required]
        public string Auth { get; set; }
        [Required]
        public string P256dh { get; set; }
    }
}
