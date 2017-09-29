using System;

namespace hfa.WebApi.Common.Auth
{
    public interface IAuthentificationService
    {
        /// <summary>
        /// Authenticate user with credentials
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <returns>JWT Token</returns>
        JwtReponse Authenticate(string username, string password);

        /// <summary>
        /// Authenticate by refresh token
        /// </summary>
        /// <param name="refreshToken"></param>
        /// <returns></returns>
        JwtReponse Authenticate(string refreshToken);

        /// <summary>
        /// Revoke by  refreshtoken or accessToken
        /// </summary>
        /// <param name="AccessToken"></param>
        /// <returns></returns>
        void RevokeToken(string accessTokenOrRefreshToken);

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
    }
}