using System.ComponentModel.DataAnnotations;

namespace hfa.Synker.Service.Entities.Auth
{
    public class Role : EntityBase
    {
        [MaxLength(32)]
        public string Libelle { get; set; }
    }
}