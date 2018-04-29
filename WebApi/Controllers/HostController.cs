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
using hfa.Synker.Service.Entities;
using hfa.WebApi.Common.Auth;

namespace hfa.WebApi.Controllers
{
    [Produces("application/json")]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class HostController : BaseController
    {
        private IAuthentificationService _authentificationService;
        public HostController(IAuthentificationService authentificationService, 
            IOptions<ElasticConfig> config,
            ILoggerFactory loggerFactory,
            IElasticConnectionClient elasticConnectionClient,
            SynkerDbContext context)
            : base(config, loggerFactory, elasticConnectionClient, context)
        {
            _authentificationService = authentificationService;
        }

        [HttpGet("{id}")]
        [ValidateModel]
        public async Task<IActionResult> GetCommand([FromRoute] int id)
        {
            var host = await _dbContext.Hosts.SingleOrDefaultAsync(m => m.Id == id);

            if (host == null)
            {
                return NotFound();
            }

            return Ok(host);
        }

        [HttpPut("{id}")]
        [ValidateModel]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Host host)
        {
            if (id != host.Id)
            {
                return BadRequest();
            }

            _dbContext.Entry(host).State = EntityState.Modified;

            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!HostExists(id))
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
        public async Task<IActionResult> Post([FromBody] Host host)
        {
            if (!string.IsNullOrEmpty(host.Authentication.Password))
            {
                host.Authentication.Password.HashPassword(_authentificationService.Salt);
            }

            _dbContext.Hosts.Add(host);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction("Get", new { id = host.Id }, host);
        }

        [HttpDelete("{id}")]
        [ValidateModel]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var host = await _dbContext.Hosts.SingleOrDefaultAsync(m => m.Id == id);
            if (host == null)
            {
                return NotFound();
            }

            _dbContext.Hosts.Remove(host);
            await _dbContext.SaveChangesAsync();

            return Ok(host);
        }

        private bool HostExists(int id) =>
            _dbContext.Hosts.Any(e => e.Id == id);
    }
}