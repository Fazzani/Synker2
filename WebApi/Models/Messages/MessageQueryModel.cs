namespace hfa.WebApi.Models.Messages
{
    using hfa.Synker.Services.Entities.Messages;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    public class MessageQueryModel
    {
        [Required]
        public List<MessageStatusEnum> MessageStatus { get; set; }
        public int PageSize { get; set; } = 10;
        public int PageIndex { get; set; } = 0;
    }
}
