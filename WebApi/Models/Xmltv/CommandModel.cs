namespace hfa.WebApi.Models.Xmltv
{
    using hfa.Synker.Service.Entities.Auth;
    using System;
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
