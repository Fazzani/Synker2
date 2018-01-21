using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using hfa.Synker.Service.Elastic;
using hfa.Synker.Service.Entities.Auth;
using hfa.Synker.Service.Services.Elastic;
using hfa.Synker.Services.Dal;
using hfa.WebApi.Common.Filters;
using Hfa.WebApi.Controllers;
using Hfa.WebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace hfa.WebApi.Controllers
{
    [Route("api/v1/[controller]")]
    [Authorize]
    public class UsersController : BaseController
    {
        public UsersController(IOptions<ElasticConfig> config, ILoggerFactory loggerFactory, IElasticConnectionClient elasticConnectionClient, SynkerDbContext context)
            : base(config, loggerFactory, elasticConnectionClient, context)
        {

        }

        /// <summary>
        /// Update user
        /// </summary>
        /// <param name="user"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{id}")]
        [ValidateModel]
        [Authorize(Roles = Role.ADMIN_ROLE_NAME)]
        public async Task<IActionResult> Put(int id, [FromBody] User userModel, CancellationToken cancellationToken)
        {
            var user = await _dbContext.Users.FindAsync(id, cancellationToken);
            if (user == null)
                return NotFound(id);

            user.BirthDay = userModel.BirthDay;
            user.Email = userModel.Email;
            user.Gender = userModel.Gender;
            user.FirstName = userModel.FirstName;
            user.LastName = userModel.LastName;
            user.Photo = userModel.Photo;

            var res = _dbContext.SaveChangesAsync(cancellationToken);
            return Ok(res);
        }

        /// <summary>
        /// Get connected user
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Me(CancellationToken cancellationToken)
        {
            var user = await _dbContext.Users.FindAsync(UserId, cancellationToken);
            return Ok(user);
        }

        /// <summary>
        /// Update connected user
        /// </summary>
        /// <param name="user"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPut]
        [ValidateModel]
        [Authorize(Roles = Role.ADMIN_ROLE_NAME)]
        public async Task<IActionResult> Put([FromBody] User userModel, CancellationToken cancellationToken)
        {
            var user = await _dbContext.Users.FindAsync(UserId, cancellationToken);
            if (user == null)
                return NotFound(UserId);

            user.BirthDay = userModel.BirthDay;
            user.Email = userModel.Email;
            user.Gender = userModel.Gender;
            user.FirstName = userModel.FirstName;
            user.LastName = userModel.LastName;
            user.Photo = userModel.Photo;

            var res = _dbContext.SaveChangesAsync(cancellationToken);
            return Ok(res);
        }

        /// <summary>
        /// Get user
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}")]
        [Authorize(Roles = Role.ADMIN_ROLE_NAME)]
        public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
        {
            var user = await _dbContext.Users.FindAsync(UserId, cancellationToken);
            if (user == null)
                return NotFound(id);
            return Ok(user);
        }

        /// <summary>
        /// List Users
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("search")]
        [Authorize(Roles = Role.ADMIN_ROLE_NAME)]
        public IActionResult List([FromBody] QueryListBaseModel query)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = _dbContext.Users
                .OrderByDescending(x => x.Id)
                .GetPaged(query.PageNumber, query.Skip);

            return Ok(response);
        }
    }
}