using System;
using System.Collections.Generic;
using System.Text;

namespace hfa.Brokers.Messages
{
    public class FirebaseNotifications
    {
        public static string TableName = "notifications";
        public Dictionary<string, FirebaseNotification> Items { get; set; }
        public class FirebaseNotification
        {
            public static class LevelEnum
            {
                public static string Info = "info";
                public static string Warning = "warning";
                public static string Error = "error";
            }
            public string Body { get; set; }
            public string Date { get; set; }
            public string Level { get; set; }
            public string Source { get; set; }
            public string Title { get; set; }
            public int UnixTimestamp { get; set; }
            public int UserId { get; set; }

            public int Order
            {
                get { return Int32.MaxValue - UnixTimestamp; }
            }
        }
    }

}
