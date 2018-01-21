using System;
using System.Collections.Generic;
using System.Text;

namespace hfa.Synker.Service.Entities.Notifications
{
    public class EmailNotification
    {
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
