using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Hfa.WebApi.Controllers;
using hfa.WebApi.Common;
using hfa.WebApi.Dal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using hfa.WebApi.Common.Exceptions;
using Microsoft.AspNetCore.Authorization;
using System.Net;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace hfa.WebApi.Controllers
{
    [AllowAnonymous]
    [Route("api/v1/[controller]")]
    public class HealthCheckController : BaseController
    {
        public HealthCheckController(IOptions<ApplicationConfigData> config, ILoggerFactory loggerFactory, IElasticConnectionClient elasticConnectionClient, SynkerDbContext context)
            : base(config, loggerFactory, elasticConnectionClient, context)
        {
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetBy(HealthCheakEnum id)
        {
            switch (id)
            {
                case HealthCheakEnum.WebApi:
                    return Ok();
                case HealthCheakEnum.Database:
                    if (await _dbContext.Database.EnsureCreatedAsync(Request.HttpContext.RequestAborted))
                        return Ok();
                    return StatusCode((int)HttpStatusCode.InternalServerError);
                case HealthCheakEnum.Elastic:
                    var elasticResponse = await _elasticConnectionClient.Client.ClusterHealthAsync(cancellationToken: HttpContext.RequestAborted);
                    return elasticResponse.IsValid ? Ok(): StatusCode((int)HttpStatusCode.InternalServerError);
                default:
                    return Ok();
            }
        }

        [HttpGet("error")]
        public string GetError() =>
            throw new BusinessException("Test exception");

    }

    public enum HealthCheakEnum : byte
    {
        WebApi = 0,
        Database = 1,
        Elastic = 2
    }
}
