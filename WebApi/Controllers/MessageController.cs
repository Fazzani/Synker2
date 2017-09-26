using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PlaylistBaseLibrary.Entities;
using Hfa.WebApi.Common;
using Hfa.WebApi.Models;
using hfa.SyncLibrary.Global;
using Hfa.SyncLibrary.Messages;
using Nest;
using hfa.WebApi.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using hfa.WebApi.Dal;
using hfa.WebApi.Dal.Entities;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Hfa.WebApi.Controllers
{
    [Route("api/v1/[controller]")]
    public class MessageController : BaseController
    {
        const string MessageIndex = "messages";
        public MessageController(IOptions<ApplicationConfigData> config, ILoggerFactory loggerFactory, IElasticConnectionClient elasticConnectionClient, SynkerDbContext context)
            : base(config, loggerFactory, elasticConnectionClient, context)
        {

        }

        /// <summary>
        /// Messages received from the batch
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task<IActionResult> Post([FromBody] Message message)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _dbContext.Messages.AddAsync(message, cancelToken.Token);
            var response = await _dbContext.SaveChangesAsync();
            //response.AssertElasticResponse();
            return new OkObjectResult(response);
        }

        /// <summary>
        /// Get message by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var response = await _dbContext.Messages.FindAsync(id);
            if (response == null)
                return NotFound(id);
            //response.AssertElasticResponse();
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
            var response = _dbContext.Messages.OrderByDescending(x => x.Id).Take(query.PageSize).Skip(query.Skip).ToList();
            return Ok(response);
        }

        /// <summary>
        /// Get Messages by status
        /// </summary>
        /// <param name="messageStatus"></param>
        /// <returns></returns>
        [HttpGet("status/{messageStatus:int}/{page:int?}/{pageSize:int?}")]
        public IActionResult GetByStatus(MessageStatus messageStatus, int page = 0, int pageSize = 10)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var response = _dbContext.Messages.OrderByDescending(x => x.Id).Where(x => x.Status == messageStatus)
                .OrderByDescending(x => x.Id).Take(pageSize).Skip(pageSize * page).ToList();
            return Ok(response);
        }

    }
}
