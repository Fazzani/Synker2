using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using hfa.WebApi.Common.Auth;
using Microsoft.AspNetCore.Authorization;
using hfa.WebApi.Dal.Entities;
using Hfa.WebApi.Controllers;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using hfa.WebApi.Common;
using hfa.WebApi.Dal;
using hfa.WebApi.Models;
using hfa.WebApi.Common.Filters;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace hfa.WebApi.Controllers
{
    [Route("api/v1/[controller]")]
    public class AuthController : BaseController
    {
        private IAuthentificationService _authentificationService;

        public AuthController(IAuthentificationService authentificationService, IOptions<ApplicationConfigData> config, ILoggerFactory loggerFactory, IElasticConnectionClient elasticConnectionClient, SynkerDbContext context)
            : base(config, loggerFactory, elasticConnectionClient, context)
        {
            _authentificationService = authentificationService;
        }

        [Route("token")]
        [AllowAnonymous]
        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> GetToken([FromBody] AuthModel model)
        {
            JwtReponse jwtReponse = null;

            if (model.GrantType == GrantType.Password)
            {
                jwtReponse = _authentificationService.Authenticate(model.UserName, model.Password);
            }
            else
            {
                jwtReponse = _authentificationService.Authenticate(model.RefreshToken);
            }

            if (jwtReponse == null)
                return new UnauthorizedResult();

            await _dbContext.SaveChangesAsync();
            return Ok(jwtReponse);
        }

        [Route("revoketoken")]
        [Authorize]
        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> RevokeToken([FromBody] TokenModel tokenModel)
        {
            _authentificationService.RevokeToken(tokenModel.Token);
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

            var userEntity = _authentificationService.ResetPassword(user.UserName, user.Password, user.NewPassword);

            return Ok(await _dbContext.SaveChangesAsync());
        }
    }
}
