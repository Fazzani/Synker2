using hfa.WebApi.Dal.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace hfa.WebApi.Models.Xmltv
{
    public class CommandModel
    {
        public int Id { get; set; }
        public string CommandText { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }

        public static CommandModel ToModel(Command command) => new CommandModel
        {
            CommandText = command.CommandText,
            CreatedDate = command.CreatedDate,
            Id = command.Id,
            UpdatedDate = command.UpdatedDate,
            UserId = command.UserId
        };
    }
}
