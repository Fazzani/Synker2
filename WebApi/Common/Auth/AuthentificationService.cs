using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;


namespace hfa.WebApi.Common.Auth
{
    using hfa.Synker.Service.Entities.Auth;
    using NETCore.Encrypt.Extensions.Internal;
    public class AuthentificationService : IAuthentificationService
    {
        private SecurityOptions _securityOptions;
        public TimeSpan ValidFor { get; }
        public DateTime IssuedAt => DateTime.UtcNow;
        public DateTime Expiration => IssuedAt.Add(ValidFor);
        public string Salt { get { return _securityOptions.Salt; } }
        private SecurityKey _issuerSigningKey;
        private SigningCredentials _signingCredentials;
        private JwtHeader _jwtHeader;

        public TokenValidationParameters Parameters { get; private set; }

        public AuthentificationService(IOptions<SecurityOptions> securityOptions)
        {
            _securityOptions = securityOptions.Value;
            ValidFor = TimeSpan.FromMinutes(_securityOptions.TokenLifetimeInMinutes);

            if (_securityOptions.UseRsa)
            {
                InitializeRsa();
            }
            else
            {
                InitializeHmac();
            }

            InitializeJwtParameters();
        }

        private void InitializeRsa()
        {
            using (var publicRsa = RSA.Create())
            {
                var publicKeyXml = File.ReadAllText(_securityOptions.RsaPublicKeyXML);
                publicRsa.FromXmlString(publicKeyXml, true);
                _issuerSigningKey = new RsaSecurityKey(publicRsa);
            }
            if (string.IsNullOrWhiteSpace(_securityOptions.RsaPrivateKeyXML))
            {
                return;
            }
            using (RSA privateRsa = RSA.Create())
            {
                var privateKeyXml = File.ReadAllText(_securityOptions.RsaPrivateKeyXML);
                privateRsa.FromXmlString(privateKeyXml, true);
                var privateKey = new RsaSecurityKey(privateRsa);
                _signingCredentials = new SigningCredentials(privateKey, SecurityAlgorithms.RsaSha256);
            }
        }

        private void InitializeHmac()
        {
            _issuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_securityOptions.HmacSecretKey));
            _signingCredentials = new SigningCredentials(_issuerSigningKey, SecurityAlgorithms.HmacSha256);
        }

        private void InitializeJwtParameters()
        {
            _jwtHeader = new JwtHeader(_signingCredentials);
            Parameters = new TokenValidationParameters
            {
                IssuerSigningKey = _issuerSigningKey,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _securityOptions.Issuer,
                ValidateIssuer = true,
                ValidAudience = _securityOptions.Audience,
                ValidateAudience = true,

                // Validate the token expiry
                ValidateLifetime = true,
                // If you want to allow a certain amount of clock drift, set that here:
                ClockSkew = TimeSpan.Zero,
                RequireExpirationTime = true,
                SaveSigninToken = true,
                RequireSignedTokens = true
            };
        }

        /// <summary>
        /// Verify Salt password
        /// </summary>
        /// <param name="password"></param>
        /// <param name="password64"></param>
        /// <returns></returns>
        public bool VerifyPassword(string password, string password64) => password.VerifyPassword(password64, _securityOptions.Salt);

        /// <summary>
        /// Authenticate by refresh token
        /// </summary>
        /// <param name="refreshToken"></param>
        /// <returns></returns>
        public JwtReponse Authenticate(string refreshToken, User user)
        {
            var jwtHandler = new JwtSecurityTokenHandler();

            try
            {
                if (user != null && ValidateToken(user.ConnectionState.AccessToken))
                {
                    user.ConnectionState.RefreshToken = refreshToken;
                    return GenerateToken(user);
                }
            }
            catch (SecurityTokenExpiredException)
            {
                //Token expired do try to refresh it
                if (ValidateToken(refreshToken))
                    return GenerateToken(user);
            }

            return null;
        }

        public JwtReponse GenerateToken(User user)
        {
            var claims = GetClaims(user);

            var jwt = new JwtSecurityToken(

                issuer: _securityOptions.Issuer,
                audience: _securityOptions.Audience,
                claims: claims,
                expires: Expiration,
                signingCredentials: _signingCredentials
            );
            user.ConnectionState.AccessToken = new JwtSecurityTokenHandler().WriteToken(jwt);
            user.ConnectionState.LastConnection = DateTime.UtcNow;
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

                var principal = jwtHandler.ValidateToken(accessToken, Parameters, out SecurityToken securityToken);
                return securityToken != null;
            }
            return false;
        }

        public List<Claim> GetClaims(User user)
        {
            var now = DateTime.UtcNow;

            var claims = new List<Claim> {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.ConnectionState.UserName),
                new Claim(JwtRegisteredClaimNames.Sub, user.ConnectionState.UserName),
                new Claim(JwtRegisteredClaimNames.AuthTime, now.ToString(), ClaimValueTypes.DateTime),
                new Claim(JwtRegisteredClaimNames.Aud, _securityOptions.Audience),
                new Claim(JwtRegisteredClaimNames.Birthdate, user.BirthDay.ToString(), ClaimValueTypes.DateTime),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName),
                new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName),
                new Claim(JwtRegisteredClaimNames.Exp, Expiration.ToString(), ClaimValueTypes.DateTime),
                new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUniversalTime().ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                new Claim("photo", user.Photo, ClaimValueTypes.String),
                new Claim(ClaimTypes.Gender, user.Gender.ToString()),
                new Claim("id", user.Id.ToString(), ClaimValueTypes.Integer),
                new Claim(JwtRegisteredClaimNames.Nbf, now.ToString())
                    };

            if (user.UserRoles.Any())
            {
                claims.AddRange(user.UserRoles.Select(x => new Claim(ClaimTypes.Role, x.Role.Name)));
            }

            return claims;
        }
    }
}

