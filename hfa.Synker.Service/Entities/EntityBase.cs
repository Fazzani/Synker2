using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace hfa.Synker.Service.Entities
{
    public class EntityBase : EntityBaseAudit
    {
        [Key]
        public int Id { get; set; }
      
    }

    public class EntityBaseAudit
    {
        public DateTime UpdatedDate { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
