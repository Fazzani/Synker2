using hfa.Synker.Services.Entities.Messages;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace hfa.WebApi.Models.Messages
{
    public class MessageQueryModel
    {
        [Required]
        public List<MessageStatusEnum> MessageStatus { get; set; }
        public int PageSize { get; set; } = 10;
        public int PageIndex { get; set; } = 0;
    }
}
