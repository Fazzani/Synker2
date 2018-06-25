namespace hfa.Synker.Service.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;
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
