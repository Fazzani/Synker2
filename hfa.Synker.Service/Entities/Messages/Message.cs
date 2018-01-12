using hfa.Synker.Service.Entities.Auth;
using Nest;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hfa.Synker.Services.Entities.Messages
{
    //[ElasticsearchType(IdProperty = nameof(Id))]
    public class Message
    {
        public static Message PingMessage => new Message { Content = "Ping", MessageType = MessageTypeEnum.Ping, Status = MessageStatus.None, TimeStamp = DateTime.Now };

        [Key]
        public int Id { get; set; } 
        public string Content { get; set; }
        public DateTime TimeStamp { get; set; }
        public MessageTypeEnum MessageType { get; set; }
        public MessageStatus Status { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; }

        public int UserId { get; set; }
    }

    public enum MessageTypeEnum : int
    {
        None = 0,
        Ping,
        START_SYNC_MEDIAS,
        END_SYNC_MEDIAS,
        START_CREATE_CONFIG,
        END_CREATE_CONFIG,
        START_SYNC_EPG_CONFIG,
        END_SYNC_EPG_CONFIG,
        EXCEPTION,
        START_PUSH_XMLTV,
        END_PUSH_XMLTV
    }

    public enum MessageStatus
    {
        None = 0,
        NotReaded = 1,
        Readed = 2
    }

}