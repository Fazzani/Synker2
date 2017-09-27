using System.ComponentModel.DataAnnotations;

namespace hfa.WebApi.Dal.Entities
{
    public class Role : EntityBase
    {
        [MaxLength(32)]
        public string Libelle { get; set; }
    }
}