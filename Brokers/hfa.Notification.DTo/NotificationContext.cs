using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace hfa.Brokers.Messages
{
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
