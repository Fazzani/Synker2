using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Hfa.WebApi.Controllers;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using hfa.WebApi.Common.Filters;
using hfa.WebApi.Models.Xmltv;
using hfa.Synker.Service.Entities.Auth;
using hfa.Synker.Services.Dal;
using hfa.Synker.Service.Services.Elastic;
using hfa.Synker.Service.Elastic;
using System.Threading;
using hfa.WebApi.Common.Auth;

namespace hfa.WebApi.Controllers
{
    [Produces("application/json")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Authorize]
    [ApiController]
    public class CommandController : BaseController
    {
        public CommandController(IOptions<ElasticConfig> config, 
            ILoggerFactory loggerFactory, 
            IElasticConnectionClient elasticConnectionClient,
            SynkerDbContext context)
            : base(config, loggerFactory, elasticConnectionClient, context)
        {

        }

        /// <summary>
        /// All commands or commands by connected user
        /// </summary>
        /// <param name="all">if true get all commands</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Policy = AuthorizePolicies.READER)]
        public async Task<IActionResult> GetCommand([FromQuery] bool? all, CancellationToken cancellationToken = default)
        {
            if (all.HasValue && all.Value)
            {
                return new OkObjectResult((await _dbContext.Command
                    .OrderByDescending(x => x.Id)
                    .ToListAsync())
                  .Select(x => x.CommandText));
            }

            //TODO: A virer apres la migration de l'auth
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email.Equals(this.UserEmail), cancellationToken);
            if (user == null) return BadRequest($"User {this.UserEmail} not found");

            return new OkObjectResult((await _dbContext
                    .Command
                    .Where(x => x.UserId == user.Id && x.TreatedDate == null)
                    .OrderByDescending(x => x.Id)
                    .ToListAsync())
                  .Select(x => x.CommandText));
        }

        [HttpGet("users/{userId}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Policy = AuthorizePolicies.READER)]
        public async Task<IActionResult> GetCommandsByUser([FromRoute] int userId, [FromQuery] bool? all, CancellationToken cancellationToken = default)
        {
            if (await _dbContext.Users.FindAsync(userId) == null)
            {
                return BadRequest("User doesn't exist");
            }

            if (all.HasValue)
            {
                return new OkObjectResult((await _dbContext
                    .Command
                    .OrderBy(x => x.Id)
                    .Where(x => x.UserId == userId)
                    .ToListAsync())
                  .Select(CommandModel.ToModel));
            }

            return new OkObjectResult((await _dbContext
                .Command
                .OrderBy(x => x.Id)
                .Where(x => x.UserId == userId && x.Status == CommandStatusEnum.None)
                .ToListAsync())
              .Select(CommandModel.ToModel));
        }

        [HttpGet("{id}")]
        [ValidateModel]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        [Authorize(Policy = AuthorizePolicies.READER)]
        public async Task<IActionResult> GetCommand([FromRoute] int id)
        {
            var command = await _dbContext.Command.SingleOrDefaultAsync(m => m.Id == id);

            if (command == null)
            {
                return NotFound();
            }

            return Ok(command);
        }

        [HttpPut("{id}/status/{status}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        [Authorize(Policy = AuthorizePolicies.FULLACCESS)]
        public async Task<IActionResult> PutCommandByStatus([FromRoute] int id, [FromRoute] CommandStatusEnum status)
        {
            var command = await _dbContext.Command.FindAsync(id);
            if (command == null)
            {
                return NotFound();
            }

            command.Status = status;
            var response = await _dbContext.SaveChangesAsync(HttpContext.RequestAborted);
            return Ok(response);
        }

        [HttpPut("{id}")]
        [ValidateModel]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesDefaultResponseType]
        [Authorize(Policy = AuthorizePolicies.FULLACCESS)]
        public async Task<IActionResult> PutCommand([FromRoute] int id, [FromBody] Command command)
        {
            if (id != command.Id)
            {
                return BadRequest();
            }

            _dbContext.Entry(command).State = EntityState.Modified;

            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CommandExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpPost]
        [ValidateModel]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [Authorize(Policy = AuthorizePolicies.FULLACCESS)]
        public async Task<IActionResult> PostCommand([FromBody] Command command)
        {
            _dbContext.Command.Add(command);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction("GetCommand", new { id = command.Id }, command);
        }

        [HttpDelete("{id}")]
        [ValidateModel]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        [Authorize(Policy = AuthorizePolicies.FULLACCESS)]
        public async Task<IActionResult> DeleteCommand([FromRoute] int id)
        {
            var command = await _dbContext.Command.SingleOrDefaultAsync(m => m.Id == id);
            if (command == null)
            {
                return NotFound();
            }

            _dbContext.Command.Remove(command);
            await _dbContext.SaveChangesAsync();

            return Ok(command);
        }

        private bool CommandExists(int id) => 
            _dbContext.Command.Any(e => e.Id == id);
    }
}