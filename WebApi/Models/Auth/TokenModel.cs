namespace hfa.WebApi.Models.Auth
{
    using System.ComponentModel.DataAnnotations;
    public class TokenModel
    {
        [Required]
        public string Token { get; set; }
    }
}
