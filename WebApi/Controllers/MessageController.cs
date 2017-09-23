﻿using System;
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

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Hfa.WebApi.Controllers
{
    [Route("api/v1/[controller]")]
    public class MessageController : BaseController
    {
        const string MessageIndex = "messages";
        public MessageController(IOptions<ApplicationConfigData> config, ILoggerFactory loggerFactory, IElasticConnectionClient elasticConnectionClient)
            : base(config, loggerFactory, elasticConnectionClient)
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
            var response = await _elasticConnectionClient.Client.UpdateAsync<Message, Message>(new DocumentPath<Message>(message.Id), x => x.Index(MessageIndex).Doc(message).DocAsUpsert(), cancelToken.Token);

            //Notify clients
            cancelToken.Token.ThrowIfCancellationRequested();

            if (!response.IsValid)
                return BadRequest(response.DebugInformation);
            //response.AssertElasticResponse();
            return new OkObjectResult(response);
        }

        /// <summary>
        /// Get message by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var response = await _elasticConnectionClient.Client.SearchAsync<Message, Message>(doc =>
                doc.Index<Message>(), cancelToken.Token);

            if (!response.IsValid)
                return BadRequest(response.DebugInformation);

            return new OkObjectResult(response.GetResultListModel());
        }

        /// <summary>
        /// List Messages
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var response = await _elasticConnectionClient.Client.SearchAsync<Message, Message>(doc =>
             doc.Index<Message>(),
            cancelToken.Token);

            if (!response.IsValid)
                return BadRequest(response.DebugInformation);

            return new OkObjectResult(response.GetResultListModel());
        }

        /// <summary>
        /// Get Messages by status
        /// </summary>
        /// <param name="messageStatus"></param>
        /// <returns></returns>
        [HttpGet("status/{messageStatus:int}")]
        public async Task<IActionResult> GetByStatus(MessageStatus messageStatus)
        {
            var response = await _elasticConnectionClient.Client.SearchAsync<Message>(doc =>
            doc.Index<Message>().Query(q => q.Term(t => t.Field(f => f.Status).Value(messageStatus))),
            cancelToken.Token);

            response.AssertElasticResponse();

            if (!response.IsValid)
                return BadRequest(response.DebugInformation);

            return new OkObjectResult(response.GetResultListModel());
        }

    }
}
