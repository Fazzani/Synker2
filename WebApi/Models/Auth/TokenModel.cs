using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace hfa.WebApi.Models.Auth
{
    public class TokenModel
    {
        [Required]
        public string Token { get; set; }
    }
}
