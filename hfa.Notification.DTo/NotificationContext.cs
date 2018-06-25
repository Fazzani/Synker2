namespace hfa.Brokers.Messages
{
    using System.Reflection;
    public class NotificationContext
    {

        public NotificationContext()
        {
            AppId = Assembly.GetExecutingAssembly().FullName;
        }

        public NotificationContext(string userId) : this()
        {
            UserId = userId;
        }

        public string UserId { get; set; }

        public string AppId { get; set; }
    }
}
