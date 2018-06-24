using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace hfa.WebApi.Models.Notifications
{
    public class NotificationModel
    {
        public NotificationTypeEnum NotificationType { get; set; } = NotificationTypeEnum.Email;

        public string Body { get; set; }
        public string Subject { get; set; }

        public string From { get; set; }
        public string To { get; set; }
        public List<string> Cci { get; set; }
        public List<string> Cc { get; set; }
        public string FromDisplayName { get; set; }

        public bool IsBodyHtml { get; set; }
    }

    public enum NotificationTypeEnum : byte
    {
        Email = 0,
        Sms,
        PushBrowser,
        PushMobile
    }
}
