using hfa.WebApi.Dal.Entities;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Threading.Tasks;

namespace hfa.WebApi.Common.Auth
{
    public interface IAuthentificationService
    {
        /// <summary>
        /// Verify Salt password
        /// </summary>
        /// <param name="password"></param>
        /// <param name="password64"></param>
        /// <returns></returns>
        bool VerifyPassword(string password, string password64);

        /// <summary>
        /// Generate user's token
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        JwtReponse GenerateToken(Dal.Entities.User user);

        /// <summary>
        /// Authenticate by refresh token
        /// </summary>
        /// <param name="refreshToken"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        JwtReponse Authenticate(string refreshToken, User user);

        /// <summary>
        /// Validate Access token
        /// </summary>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        bool ValidateToken(string accessToken);

        /// <summary>
        /// prefix
        /// </summary>
        string Salt { get; }

        /// <summary>
        /// Token validation parameters
        /// </summary>
        TokenValidationParameters Parameters { get; }
    }
}