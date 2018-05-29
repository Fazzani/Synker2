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
using hfa.Synker.Services.Dal;
using hfa.Synker.Service.Services.Elastic;
using hfa.Synker.Service.Elastic;
using hfa.Synker.Service.Entities;
using hfa.WebApi.Common.Auth;
using System.Threading;
using Hfa.WebApi.Models;
using hfa.WebApi.Models;
using System.Net;

namespace hfa.WebApi.Controllers
{
    [Produces("application/json")]
    [Route("api/v1/[controller]")]
    [Authorize]
    [Authorize(Policy = AuthorizePolicies.ADMIN)]
    [ApiVersion("1.0")]
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

        /// <summary>
        /// List Hosts
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("search")]
        [Authorize(Policy = AuthorizePolicies.ADMIN)]
        [ProducesResponseType(typeof(PagedResult<Host>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public IActionResult List([FromBody] QueryListBaseModel query)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = _dbContext.Hosts
                .OrderByDescending(x => x.Id)
                .GetPaged(query.PageNumber, query.PageSize, query.GetAll);

            return Ok(response);
        }

        [HttpGet("{id}")]
        [ValidateModel]
        [ProducesResponseType(typeof(Host), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
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
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
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

        [HttpPost]
        [ValidateModel]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        public async Task<IActionResult> Post([FromBody] Host host, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(host.Authentication?.Password))
            {
                host.Authentication.Password.HashPassword(_authentificationService.Salt);
            }

            _dbContext.Hosts.Add(host);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction("Get", new { id = host.Id }, host);
        }

        [HttpDelete("{id}")]
        [ValidateModel]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(Host), (int)HttpStatusCode.OK)]
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