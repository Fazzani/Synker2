using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace hfa.WebApi.Common.Auth
{
    /// <summary>
    /// JWT Reponse
    /// </summary>
    public class JwtReponse
    {
        public JwtReponse(string accessToken)
        {
            AccessToken = accessToken;
            RefreshToken = Guid.NewGuid().ToString().Replace("-", "");

        }
        public JwtReponse(string accessToken, string refreshToken) : this(accessToken)
        {
            RefreshToken = accessToken;
        }

        public JwtReponse(string accessToken, string refreshToken, DateTime expire) : this(accessToken, refreshToken)
        {
            Expire = expire;
        }

        public JwtReponse(string accessToken, DateTime expire) : this(accessToken)
        {
            Expire = expire;
        }

        public string AccessToken { get; private set; }
        public string RefreshToken { get; set; }
        public DateTime Expire { get; set; }
    }
}
