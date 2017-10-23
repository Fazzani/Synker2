using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using hfa.WebApi.Dal;
using hfa.WebApi.Dal.Entities;
using Hfa.WebApi.Controllers;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using hfa.WebApi.Common;
using Microsoft.AspNetCore.Authorization;

namespace hfa.WebApi.Controllers
{
    [Produces("application/json")]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class CommandController : BaseController
    {
        public CommandController(IOptions<ApplicationConfigData> config, ILoggerFactory loggerFactory, IElasticConnectionClient elasticConnectionClient, SynkerDbContext context)
            : base(config, loggerFactory, elasticConnectionClient, context)
        {

        }

        [HttpGet]
        public IEnumerable<Command> GetCommand()
        {
            return _dbContext.Command;
        }

        [HttpGet("users/{userId}")]
        public async Task<IActionResult> GetCommandsByUser([FromRoute] int userId, [FromQuery] bool? all)
        {
            if (await _dbContext.Users.FindAsync(userId) == null)
            {
                return BadRequest("User not exist");
            }

            if (all.HasValue)
                return new OkObjectResult(_dbContext.Command.Where(x => x.UserId == userId));
            return new OkObjectResult(_dbContext.Command.Where(x => x.UserId == userId && x.TreatedDate == null));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCommand([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var command = await _dbContext.Command.SingleOrDefaultAsync(m => m.Id == id);

            if (command == null)
            {
                return NotFound();
            }

            return Ok(command);
        }

        [HttpPut("treat/{id}")]
        public async Task<IActionResult> PutCommandTreated([FromRoute] int id)
        {
            var command = await _dbContext.Command.FindAsync(id);
            if (command == null)
            {
                return NotFound();
            }

            command.TreatedDate = DateTime.UtcNow;
            var response = await _dbContext.SaveChangesAsync(HttpContext.RequestAborted);
            return Ok(response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutCommand([FromRoute] int id, [FromBody] Command command)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

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
        public async Task<IActionResult> PostCommand([FromBody] Command command)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _dbContext.Command.Add(command);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction("GetCommand", new { id = command.Id }, command);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCommand([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var command = await _dbContext.Command.SingleOrDefaultAsync(m => m.Id == id);
            if (command == null)
            {
                return NotFound();
            }

            _dbContext.Command.Remove(command);
            await _dbContext.SaveChangesAsync();

            return Ok(command);
        }

        private bool CommandExists(int id)
        {
            return _dbContext.Command.Any(e => e.Id == id);
        }
    }
}