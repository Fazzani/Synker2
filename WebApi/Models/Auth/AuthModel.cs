using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace hfa.WebApi.Models.Auth
{
    public class AuthModel
    {
        public string UserName { get; set; }

        public string Password { get; set; }

        public string RefreshToken { get; set; }

        public GrantType GrantType { get; set; } = GrantType.Password;
    }
    public enum GrantType : byte
    {
        Password = 0,
        RefreshToken
    }
}
