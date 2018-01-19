using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PlaylistBaseLibrary.Entities;
using Hfa.WebApi.Common;
using Hfa.WebApi.Models;
using hfa.SyncLibrary.Global;
using Nest;
using hfa.WebApi.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authorization;
using hfa.WebApi.Common.Filters;
using System.Threading;
using hfa.Synker.Services.Entities.Messages;
using hfa.Synker.Services.Dal;
using hfa.Synker.Service.Services.Elastic;
using hfa.Synker.Service.Elastic;
using hfa.WebApi.Models.Messages;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Hfa.WebApi.Controllers
{
    [Route("api/v1/[controller]")]
    [Authorize]
    public class MessageController : BaseController
    {
        const string MessageIndex = "messages";
        public MessageController(IOptions<ElasticConfig> config, ILoggerFactory loggerFactory, IElasticConnectionClient elasticConnectionClient, SynkerDbContext context)
            : base(config, loggerFactory, elasticConnectionClient, context)
        {

        }

        /// <summary>
        /// Messages received from the batch
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Message message, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _dbContext.Messages.AddAsync(message, cancellationToken);
            var response = await _dbContext.SaveChangesAsync(HttpContext.RequestAborted);
            return Ok(response);
        }

        /// <summary>
        /// Get message by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
        {
            var response = await _dbContext.Messages.FindAsync(id, cancellationToken);
            if (response == null)
                return NotFound(id);

            return Ok(response);
        }

        /// <summary>
        /// List Messages
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("search")]
        public IActionResult List([FromBody] QueryListBaseModel query)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = _dbContext.Messages
                .OrderByDescending(x => x.Id)
                .GetPaged(query.PageNumber, query.Skip);

            return Ok(response);
        }

        /// <summary>
        /// Get Messages by status for connected user
        /// </summary>
        /// <param name="messageStatus"></param>
        /// <returns></returns>
        [ValidateModel]
        [Route("search/status")]
        [HttpPost]
        public IActionResult ListByStatus([FromBody]MessageQueryModel messageQuery)
        {
            var response = _dbContext.Messages
                .OrderByDescending(x => x.Id)
                .Where(x => messageQuery.MessageStatus.Any(m => x.Status == m) && UserId == x.UserId)
                .GetPaged(messageQuery.PageIndex, messageQuery.PageSize);

            return Ok(response);
        }

    }
}
