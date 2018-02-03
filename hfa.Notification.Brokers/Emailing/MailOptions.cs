using System;
using System.Collections.Generic;
using System.Text;

namespace hfa.Notification.Brokers.Emailing
{
    public class MailOptions
    {
        public string SmtpServer { get; set; }
        public string PopServer { get; set; }
        public int SmtpPort { get; set; }
        public int PopPort { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
