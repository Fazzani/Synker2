using System;

namespace Hfa.SyncLibrary.Messages
{
    public class Message
    {

        public static Message PingMessage => new Message { Content = "Ping", Id = "test", Status = MessageStatus.None, TimeStamp = DateTime.Now };

        public string Id { get; set; }
        public string Content { get; set; }
        public DateTime TimeStamp { get; set; }

        public MessageStatus Status { get; set; }
    }

    public enum MessageStatus
    {
        None = 0,
        NotReaded = 1,
        Readed = 2
    }

}