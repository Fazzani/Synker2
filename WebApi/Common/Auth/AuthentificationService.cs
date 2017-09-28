using hfa.WebApi.Dal;
using Microsoft.EntityFrameworkCore;
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
        public JwtReponse Authenticate(string username, string password)
        {
            var user = _synkerDbContext.Users.Include(x => x.ConnectionState).SingleOrDefault(it => it.ConnectionState.UserName == username);
            if (user != null && password.VerifyPassword(user.ConnectionState.Password, _securityOptions.Value.Salt))
            {
                return GenerateToken(user);
            }
            return null;
        }

        /// <summary>
        /// Authenticate by refresh token
        /// </summary>
        /// <param name="refreshToken"></param>
        /// <returns></returns>
        public JwtReponse Authenticate(string refreshToken)
        {
            var user = _synkerDbContext
                .Users
                .Include(x => x.ConnectionState)
                .SingleOrDefault(it => it.ConnectionState.RefreshToken == refreshToken);

            var jwtHandler = new JwtSecurityTokenHandler();

            if (user != null && ValidateToken(user.ConnectionState.AccessToken))
            {
                user.ConnectionState.RefreshToken = refreshToken;
                return GenerateToken(user);
            }
            return null;
        }

        private JwtReponse GenerateToken(Dal.Entities.User user)
        {
            List<Claim> claims = GetClaims(user);

            var jwt = new JwtSecurityToken(
                issuer: _securityOptions.Value.Issuer,
                audience: _securityOptions.Value.Audience,
                claims: claims,
                expires: Expiration,
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_securityOptions.Value.SymmetricSecurityKey)), SecurityAlgorithms.HmacSha256)
            );
            user.ConnectionState.AccessToken = new JwtSecurityTokenHandler().WriteToken(jwt);
            var jwtResponse = new JwtReponse(user.ConnectionState.AccessToken);
            user.ConnectionState.RefreshToken = jwtResponse.RefreshToken;
            return jwtResponse;
        }

        public bool ValidateToken(string accessToken)
        {
            var jwtHandler = new JwtSecurityTokenHandler();

            if (jwtHandler.CanReadToken(accessToken))
            {
                var jwtToken = jwtHandler.ReadJwtToken(accessToken);
                var validationParameters = new TokenValidationParameters
                {
                    IssuerSigningKeys = new List<SecurityKey>() { new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_securityOptions.Value.SymmetricSecurityKey)) },
                    ValidAudience = _securityOptions.Value.Audience,
                    ValidIssuer = _securityOptions.Value.Issuer,
                    ValidateLifetime = true,
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidateIssuerSigningKey = true
                };

                SecurityToken securityToken;
                try
                {
                    var principal = jwtHandler.ValidateToken(accessToken, validationParameters, out securityToken);
                    return securityToken != null;
                }
                catch (SecurityTokenException)
                {
                    return false;
                }
            }
            return false;
        }

        private List<Claim> GetClaims(Dal.Entities.User user)
        {
            var claims = new List<Claim> {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.ConnectionState.UserName),
                new Claim(JwtRegisteredClaimNames.AuthTime, DateTime.Now.ToString()),
                new Claim(JwtRegisteredClaimNames.Aud, _securityOptions.Value.Audience),
                new Claim(JwtRegisteredClaimNames.Birthdate, user.BirthDay.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName),
                new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName),
                new Claim(JwtRegisteredClaimNames.Exp, Expiration.ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, IssuedAt.ToString()),
                new Claim(JwtRegisteredClaimNames.Nbf, DateTime.UtcNow.ToString())
                    };
            if (user.Roles.Any())
                claims.AddRange(user.Roles.Select(x => new Claim("role", x.Libelle)));
            return claims;
        }
    }
    public class JwtReponse
    {
        public JwtReponse(string accessToken)
        {
            this.AccessToken = accessToken;
            RefreshToken = Guid.NewGuid().ToString().Replace("-", "");

        }
        public JwtReponse(string accessToken, string refreshToken)
        {
            this.AccessToken = accessToken;
            RefreshToken = Guid.NewGuid().ToString().Replace("-", "");
        }

        public string AccessToken { get; private set; }
        public string RefreshToken { get; set; }
    }
}
