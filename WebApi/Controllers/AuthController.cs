using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using hfa.WebApi.Common.Auth;
using Microsoft.AspNetCore.Authorization;
using Hfa.WebApi.Controllers;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using hfa.WebApi.Common;
using hfa.WebApi.Common.Filters;
using Microsoft.EntityFrameworkCore;
using hfa.WebApi.Models.Auth;
using hfa.Synker.Services.Dal;
using hfa.Synker.Service.Services.Elastic;
using hfa.Synker.Service.Elastic;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace hfa.WebApi.Controllers
{
    [Route("api/v1/[controller]")]
    public class AuthController : BaseController
    {
        private IAuthentificationService _authentificationService;

        public AuthController(IAuthentificationService authentificationService, IOptions<ElasticConfig> config, ILoggerFactory loggerFactory,
            IElasticConnectionClient elasticConnectionClient, SynkerDbContext context)
            : base(config, loggerFactory, elasticConnectionClient, context)
        {
            _authentificationService = authentificationService;
        }

        [HttpGet("me")]
        [Authorize]
        public IActionResult Get() => Content($"Hello {User.Identity.Name}");

        [Route("token")]
        [AllowAnonymous]
        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> GetToken([FromBody] AuthModel model)
        {
            JwtReponse jwtReponse = null;
            var user = _dbContext.Users.Include(x => x.ConnectionState).SingleOrDefault(it => it.ConnectionState.UserName == model.UserName);

            if (model.GrantType == GrantType.Password)
            {
                if (user != null && _authentificationService.VerifyPassword(model.Password, user.ConnectionState.Password))
                {
                    jwtReponse = _authentificationService.GenerateToken(user);
                }
            }
            else
            {
                jwtReponse = _authentificationService.Authenticate(model.RefreshToken, user);
            }

            if (jwtReponse == null)
                return new UnauthorizedResult();

            user.ConnectionState.LastConnection = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
            return Ok(jwtReponse);
        }

        [Route("revoketoken")]
        [AllowAnonymous]
        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> RevokeToken([FromBody] TokenModel tokenModel)
        {
            var user = _dbContext
               .Users
               .Include(x => x.ConnectionState)
               .SingleOrDefault(it => it.ConnectionState.AccessToken == tokenModel.Token || it.ConnectionState.RefreshToken == tokenModel.Token);

            if (user != null && _authentificationService.ValidateToken(user.ConnectionState.AccessToken))
            {
                user.ConnectionState.RefreshToken = null;
                user.ConnectionState.AccessToken = null;
            }

            await _dbContext.SaveChangesAsync();
            return Ok();
        }

        /// <summary>
        /// Register new user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [Route("register")]
        [AllowAnonymous]
        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> Register([FromBody] RegisterModel user)
        {
            if (_dbContext.Users.Any(x => x.Email == user.Email) || _dbContext.Users.Any(x => x.ConnectionState.UserName == user.UserName))
                return BadRequest($"The user {user.UserName} is already exist");

            user.Password = user.Password.HashPassword(_authentificationService.Salt);
            var result = await _dbContext.Users.AddAsync(user.Entity);

            return Ok(await _dbContext.SaveChangesAsync());
        }

        /// <summary>
        /// Reset password
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [Route("reset")]
        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> Reset([FromBody] ResetModel user)
        {
            if (!_dbContext.Users.Any(x => x.ConnectionState.UserName == user.UserName))
                return BadRequest($"The user {user.UserName} is not exist");

            var userEntity = _dbContext.Users.Include(x => x.ConnectionState).SingleOrDefault(it => it.ConnectionState.UserName == user.UserName);
            if (user != null && _authentificationService.VerifyPassword(user.Password, userEntity.ConnectionState.Password))
            {
                userEntity.ConnectionState.Password = user.NewPassword.HashPassword(_authentificationService.Salt);
            }
            _dbContext.Users.Update(userEntity);

            return Ok(await _dbContext.SaveChangesAsync());
        }
    }
}
