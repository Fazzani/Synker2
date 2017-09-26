using Nest;
using System;
using System.ComponentModel.DataAnnotations;

namespace Hfa.SyncLibrary.Messages
{
    //TODO: Ef : https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql/blob/master/README.md
    [ElasticsearchType(IdProperty = nameof(Id))]
    public class Message
    {
        public static Message PingMessage => new Message { Content = "Ping", Type = "Ping", Status = MessageStatus.None, TimeStamp = DateTime.Now };

        public Guid Id { get; set; } = Guid.NewGuid();
        public string Type { get; set; }
        public string Content { get; set; }
        public DateTime TimeStamp { get; set; }

        public MessageStatus Status { get; set; }

        [MaxLength(64)]
        public string Author { get; set; }
    }

    public enum MessageIdEnum
    {
        START_SYNC_MEDIAS,
        END_SYNC_MEDIAS,
        START_CREATE_CONFIG,
        END_CREATE_CONFIG,
        START_SYNC_EPG_CONFIG,
        END_SYNC_EPG_CONFIG,
        EXCEPTION
    }

    public enum MessageStatus
    {
        None = 0,
        NotReaded = 1,
        Readed = 2
    }

}