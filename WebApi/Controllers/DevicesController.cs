﻿using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using hfa.synker.entities.Notifications;
using hfa.Synker.Service.Elastic;
using hfa.Synker.Service.Services.Elastic;
using hfa.Synker.Services.Dal;
using hfa.WebApi.Common.Auth;
using hfa.WebApi.Common.Filters;
using hfa.WebApi.Models.Notifications;
using Hfa.WebApi.Controllers;
using Hfa.WebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace hfa.WebApi.Controllers
{
    [Produces("application/json")]
    [ApiVersion("1.0")]
    [Authorize(Policy = AuthorizePolicies.ADMIN)]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class DevicesController : BaseController
    {
        public DevicesController(IOptions<ElasticConfig> config, ILoggerFactory loggerFactory,
           IElasticConnectionClient elasticConnectionClient, SynkerDbContext context)
           : base(config, loggerFactory, elasticConnectionClient, context)
        {
        }

        /// <summary>
        /// List Messages
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult List([FromBody] QueryListBaseModel query)
        {
            var response = _dbContext.Devices
                .OrderByDescending(x => x.Id)
                .GetPaged(query.PageNumber, query.PageSize, query.GetAll);

            return Ok(response);
        }

        /// <summary>
        /// Create new device
        /// </summary>
        /// <param name="pushSubscription"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateModel]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> PostAsync([FromBody] PushSubscriptionModel pushSubscription, CancellationToken cancellationToken)
        {
            var device = new Device {
                Name = Request.Headers["User-Agent"],
                PushEndpoint = pushSubscription.EndPoint,
                PushAuth = pushSubscription.Keys.Auth,
                PushP256DH = pushSubscription.Keys.P256dh
            };

            var result = await _dbContext.Devices.AddAsync(device, cancellationToken);
            var response = await _dbContext.SaveChangesAsync(HttpContext.RequestAborted);
            return Ok(response);
        }

        /// <summary>
        /// Delete device
        /// </summary>
        /// <param name="id">Device id</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken)
        {
            var device = _dbContext.Devices.FirstOrDefault(x => x.Id == id);
            if (device == null)
                return NotFound(device);

            _dbContext.Devices.Remove(device);

            await _dbContext.SaveChangesAsync();
            return NoContent();
        }

    }
}