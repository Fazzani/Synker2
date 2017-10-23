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
using hfa.WebApi.Common.Filters;
using hfa.WebApi.Models.Xmltv;

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
        public async Task<IActionResult> GetCommand([FromQuery] bool? all)
        {
            if (all.HasValue && all.Value)
                return new OkObjectResult((await _dbContext.Command
                    .OrderByDescending(x => x.Id)
                    .ToListAsync())
                    .Select(CommandModel.ToModel));

            return new OkObjectResult((await _dbContext
                    .Command
                    .Where(x => x.UserId == UserId && x.TreatedDate == null)
                    .OrderByDescending(x => x.Id)
                    .ToListAsync())
                    .Select(CommandModel.ToModel));
        }

        [HttpGet("users/{userId}")]
        public async Task<IActionResult> GetCommandsByUser([FromRoute] int userId, [FromQuery] bool? all)
        {
            if (await _dbContext.Users.FindAsync(userId) == null)
            {
                return BadRequest("User not exist");
            }

            if (all.HasValue)
                return new OkObjectResult((await _dbContext.Command.OrderByDescending(x => x.Id).Where(x => x.UserId == userId).ToListAsync()).Select(CommandModel.ToModel));
            return new OkObjectResult((await _dbContext.Command.OrderByDescending(x => x.Id).Where(x => x.UserId == userId && x.TreatedDate == null).ToListAsync()).Select(CommandModel.ToModel));
        }

        [HttpGet("{id}")]
        [ValidateModel]
        public async Task<IActionResult> GetCommand([FromRoute] int id)
        {
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
        [ValidateModel]
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
        public async Task<IActionResult> PostCommand([FromBody] Command command)
        {
            _dbContext.Command.Add(command);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction("GetCommand", new { id = command.Id }, command);
        }

        [HttpDelete("{id}")]
        [ValidateModel]
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

        private bool CommandExists(int id)
        {
            return _dbContext.Command.Any(e => e.Id == id);
        }
    }
}