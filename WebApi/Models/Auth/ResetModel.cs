using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace hfa.WebApi.Models.Auth
{
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
