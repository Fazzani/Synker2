using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using hfa.Synker.Service.Elastic;
using hfa.Synker.Service.Entities.Auth;
using hfa.Synker.Service.Services.Elastic;
using hfa.Synker.Services.Dal;
using hfa.WebApi.Common.Auth;
using hfa.WebApi.Common.Filters;
using hfa.WebApi.Models;
using hfa.WebApi.Models.Admin;
using Hfa.WebApi.Controllers;
using Hfa.WebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace hfa.WebApi.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
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
        [Authorize(Policy = AuthorizePolicies.ADMIN)]
        [ProducesResponseType(typeof(int), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Put(int id, [FromBody] User userModel, CancellationToken cancellationToken)
        {
            var user = await _dbContext.Users.FindAsync(new object[] { id }, cancellationToken: cancellationToken);
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
        [ProducesResponseType(typeof(User), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Me(CancellationToken cancellationToken)
        {
            var user = await _dbContext.Users
                .Include(x => x.UserRoles)
                    .ThenInclude(r => r.Role)
                .Include(x => x.ConnectionState)
                .FirstOrDefaultAsync(x => x.Id == UserId, cancellationToken);
            return Ok(new UserModel(user));
        }

        /// <summary>
        /// Update connected user
        /// </summary>
        /// <param name="userModel"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPut]
        [ValidateModel]
        [Authorize(Policy = AuthorizePolicies.ADMIN)]
        [ProducesResponseType(typeof(User), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> Put([FromBody] User userModel, CancellationToken cancellationToken)
        {
            var user = await _dbContext.Users.FindAsync(new object[] { UserId }, cancellationToken: cancellationToken);
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
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}")]
        [Authorize(Policy = AuthorizePolicies.ADMIN)]
        [ProducesResponseType(typeof(User), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
        {
            var user = await _dbContext.Users.FindAsync(new object[] { UserId.Value }, cancellationToken);
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
        [Authorize(Policy = AuthorizePolicies.ADMIN)]
        [ProducesResponseType(typeof(PagedResult<User>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public IActionResult List([FromBody] QueryListBaseModel query)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = _dbContext.Users
                .Include(x => x.UserRoles)
                    .ThenInclude(r => r.Role)
                .Include(x => x.ConnectionState)
                .OrderByDescending(x => x.Id)
                .Select(x => new UserModel(x))
                .GetPaged(query.PageNumber, query.PageSize, query.GetAll);

            return Ok(response);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = AuthorizePolicies.ADMIN)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var user = _dbContext.Users.FirstOrDefault(x => x.Id == id);
            if (user == null)
                return NotFound(user);

            _dbContext.Users.Remove(user);

            await _dbContext.SaveChangesAsync();
            return NoContent();
        }
    }
}