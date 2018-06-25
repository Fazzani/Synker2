namespace hfa.WebApi.Models
{
    using System.ComponentModel.DataAnnotations;
    public class SimpleModelPost
    {
        [Required]
        public string Value { get; set; }
    }
}
