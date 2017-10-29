using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace hfa.WebApi.Dal.Entities
{
    /// <summary>
    /// Synker command entity
    /// </summary>
    public class Command : EntityBase
    {
        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; }

        public int UserId { get; set; }

        [Required]
        public string CommandText { get; set; }

        /// <summary>
        /// La dateTime when the command was treated
        /// </summary>
        public DateTime? TreatedDate { get; set; }

        /// <summary>
        /// Datetime de début du lancement de la commande
        /// </summary>
        public DateTime? TreatingDate { get; set; }

        public string Comments { get; set; }

        /// <summary>
        /// Command Status
        /// </summary>
        public CommandStatusEnum Status { get; set; } = CommandStatusEnum.None;

        /// <summary>
        /// Priority 0 c'est la plus Haute
        /// </summary>
        public int Priority { get; set; } = 1000;

        /// <summary>
        /// Command Interpreter (shell by default)
        /// </summary>
        public CommandInterpreter Interpreter { get; set; } = CommandInterpreter.Shell;
    } 

    /// <summary>
    /// Synker command status
    /// </summary>
    public enum CommandStatusEnum : byte
    {
        None = 0,
        Treating = 1,
        Treated = 2,
        /// <summary>
        /// Abandonnée
        /// </summary>
        Outdated = 3
    }

    /// <summary>
    /// Command Interpreter
    /// </summary>
    public enum CommandInterpreter : byte
    {
        /// <summary>
        /// shell command type
        /// </summary>
        Shell = 0
    }
}
