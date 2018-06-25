namespace hfa.WebApi.Models.Auth
{
    using System.ComponentModel.DataAnnotations;
    /// <summary>
    /// Reset Password Model
    /// </summary>
    public class ResetModel
    {
        [Required]
        public string Password { get; set; }
        [Required]
        public string NewPassword { get; set; }
        [Required]
        public string UserName { get; set; }
    }
}
