using hfa.WebApi.Dal;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace hfa.WebApi.Common.Auth
{
    public class AuthentificationService : IAuthentificationService
    {
        private SynkerDbContext _synkerDbContext;
        private IOptions<SecurityOptions> _securityOptions;
        public TimeSpan ValidFor { get; }
        public DateTime IssuedAt => DateTime.UtcNow;
        public DateTime Expiration => IssuedAt.Add(ValidFor);
        public string Salt { get { return _securityOptions.Value.Salt; } }
        public AuthentificationService(SynkerDbContext synkerDbContext, IOptions<SecurityOptions> securityOptions)
        {
            _synkerDbContext = synkerDbContext;
            _securityOptions = securityOptions;
            ValidFor = TimeSpan.FromMinutes(_securityOptions.Value.TokenLifetimeInMinutes);
        }

        /// <summary>
        /// Authenticate user with credentials
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <returns>JWT Token</returns>
        public string Authenticate(string username, string password)
        {
            string jwtToDelivery = null;
            var user = _synkerDbContext.Users.SingleOrDefault(it => it.UserName == username);
            if (user != null && password.VerifyPassword(user.Password, _securityOptions.Value.Salt))
            {
                var claims = new List<Claim> {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
                new Claim(JwtRegisteredClaimNames.AuthTime, DateTime.Now.ToString()),
                new Claim(JwtRegisteredClaimNames.Aud, _securityOptions.Value.Audience),
                new Claim(JwtRegisteredClaimNames.Birthdate, user.BirthDay.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName),
                new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName),
                new Claim(JwtRegisteredClaimNames.Exp, Expiration.ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, IssuedAt.ToString()),
                new Claim(JwtRegisteredClaimNames.Nbf, DateTime.UtcNow.ToString()),
                new Claim("management", "management"),
                    };
                if (user.Roles.Any())
                    claims.AddRange(user.Roles.Select(x => new Claim("role", x.Libelle)));

                var jwt = new JwtSecurityToken(
                    issuer: _securityOptions.Value.Issuer,
                    audience: _securityOptions.Value.Audience,
                    claims: claims,
                    expires: Expiration,
                    signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_securityOptions.Value.SymmetricSecurityKey)), SecurityAlgorithms.HmacSha256)
                );
                jwtToDelivery = new JwtSecurityTokenHandler().WriteToken(jwt);
            }
            return jwtToDelivery;
        }
    }
}
