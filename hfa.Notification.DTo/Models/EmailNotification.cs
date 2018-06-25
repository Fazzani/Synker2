namespace hfa.Brokers.Messages.Models
{
    using System.Collections.Generic;
    public class EmailNotification : NotificationContext
    {
        public EmailNotification() : base()
        {

        }

        public EmailNotification(string userId) : base(userId)
        {

        }

        public string FromDisplayName { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public List<string> Cci { get; set; }
        public List<string> Cc { get; set; }
        public string Body { get; set; }
        public string Subject { get; set; }

        public bool IsBodyHtml { get; set; }
    }
}
