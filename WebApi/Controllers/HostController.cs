using hfa.Synker.Service.Elastic;
using hfa.Synker.Service.Entities;
using hfa.Synker.Service.Services.Elastic;
using hfa.Synker.Services.Dal;
using hfa.WebApi.Common.Auth;
using hfa.WebApi.Common.Filters;
using hfa.WebApi.Models;
using Hfa.WebApi.Controllers;
using Hfa.WebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace hfa.WebApi.Controllers
{
    [Produces("application/json")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    [ApiVersion("1.0")]
    [ApiController]
    public class HostController : BaseController
    {

        public HostController(IOptions<ElasticConfig> config,
            ILoggerFactory loggerFactory,
            IElasticConnectionClient elasticConnectionClient,
            SynkerDbContext context)
            : base(config, loggerFactory, elasticConnectionClient, context)
        {
        }

        /// <summary>
        /// List Hosts
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("search")]
        [Authorize(Policy = AuthorizePolicies.ADMIN)]
        [ProducesResponseType(typeof(PagedResult<Host>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult List([FromBody] QueryListBaseModel query)
        {
            var response = _dbContext.Hosts
                .OrderByDescending(x => x.Id)
                .GetPaged(query.PageNumber, query.PageSize, query.GetAll);

            return Ok(response);
        }

        [HttpGet("{id}")]
        [ValidateModel]
        [ProducesResponseType(typeof(Host), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get([FromRoute] int id, CancellationToken cancellationToken)
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
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Host host, CancellationToken cancellationToken)
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

        //[HttpPost]
        //[ValidateModel]
        //[ProducesResponseType(StatusCodes.Status201Created)]
        //public async Task<IActionResult> Post([FromBody] Host host, CancellationToken cancellationToken)
        //{
        //    if (!string.IsNullOrEmpty(host.Authentication?.Password))
        //    {
        //        host.Authentication.Password.HashPassword(_authentificationService.Salt);
        //    }

        //    _dbContext.Hosts.Add(host);
        //    await _dbContext.SaveChangesAsync();

        //    return CreatedAtAction("Get", new { id = host.Id }, host);
        //}

        [HttpDelete("{id}")]
        [ValidateModel]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Host), StatusCodes.Status200OK)]
        public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken cancellationToken)
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