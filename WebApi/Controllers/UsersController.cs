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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace hfa.WebApi.Controllers
{
    [Produces("application/json")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Authorize]
    [ApiController]
    public class UsersController : BaseController
    {
        public UsersController(IOptions<ElasticConfig> config, ILoggerFactory loggerFactory, IElasticConnectionClient elasticConnectionClient, SynkerDbContext context)
            : base(config, loggerFactory, elasticConnectionClient, context)
        {

        }

        /// <summary>
        /// Update user
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userModel"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{id}")]
        [ValidateModel]
        [Authorize(Policy = AuthorizePolicies.ADMIN)]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(int), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Put(int id, [FromBody] User userModel,
            CancellationToken cancellationToken = default)
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
        [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
        public async Task<IActionResult> Me(CancellationToken cancellationToken = default)
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
        [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Put([FromBody] User userModel, CancellationToken cancellationToken = default)
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
        [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get(int id, CancellationToken cancellationToken = default)
        {
            var user = await _dbContext.Users.FindAsync(new object[] { UserId.Value }, cancellationToken);
            return user == null ? NotFound(id) : (IActionResult)Ok(user);
        }

        /// <summary>
        /// List Users
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("search")]
        [Authorize(Policy = AuthorizePolicies.ADMIN)]
        [ProducesResponseType(typeof(PagedResult<User>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult List([FromBody] QueryListBaseModel query)
        {
            var response = _dbContext.Users
                .Include(x => x.UserRoles)
                    .ThenInclude(r => r.Role)
                .Include(x => x.ConnectionState)
                .OrderByDescending(x => x.Id)
                .Select(x => new UserModel(x))
                .GetPaged(query.PageNumber, query.PageSize, query.GetAll);

            return Ok(response);
        }

        /// <summary>
        /// Delete user by id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Authorize(Policy = AuthorizePolicies.ADMIN)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (user == null)
                return NotFound(user);

            _dbContext.Users.Remove(user);

            await _dbContext.SaveChangesAsync(cancellationToken);
            return NoContent();
        }

        [HttpGet("identity")]
        public IActionResult GetIdentity()
        {
            return new JsonResult(from c in User.Claims select new { c.Type, c.Value });
        }
    }
}