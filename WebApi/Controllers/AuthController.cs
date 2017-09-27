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
        public IActionResult GetToken([FromBody] AuthModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = _authentificationService.Authenticate(model.UserName, model.Password);
            if (result == null)
                return new UnauthorizedResult();
            return Ok(result);
        }

        [Route("register")]
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (_dbContext.Users.Any(x => x.Email == user.Email) || _dbContext.Users.Any(x => x.UserName == user.UserName))
                return BadRequest($"User {user.UserName} already exist");

            user.Password = user.Password.HashPassword(_authentificationService.Salt);
            var result = await _dbContext.Users.AddAsync(user);

            return Ok(await _dbContext.SaveChangesAsync());
        }
    }
}
