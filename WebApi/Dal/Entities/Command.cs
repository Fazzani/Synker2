using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace hfa.WebApi.Dal.Entities
{
    public class Command : EntityBase
    {
        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; }

        public int UserId { get; set; }

        [Required]
        public string CommandText { get; set; }

        public DateTime TreatedDate { get; set; }

        public string Comments { get; set; }
    }
}
