using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Hfa.WebApi.Controllers;
using hfa.WebApi.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using hfa.WebApi.Common.Exceptions;
using Microsoft.AspNetCore.Authorization;
using System.Net;
using hfa.WebApi.Models.HealthCheck;
using hfa.Synker.Services.Dal;
using hfa.Synker.Service.Services.Elastic;
using hfa.Synker.Service.Elastic;
using System.Reflection;

namespace hfa.WebApi.Controllers
{
    [AllowAnonymous]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class HealthCheckController : BaseController
    {
        public HealthCheckController(IOptions<ElasticConfig> config, ILoggerFactory loggerFactory, IElasticConnectionClient elasticConnectionClient,
            SynkerDbContext context)
            : base(config, loggerFactory, elasticConnectionClient, context)
        {
        }

        [HttpGet("{id}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetBy(HealthCheckEnum id)
        {
            switch (id)
            {
                case HealthCheckEnum.WebApi:
                    return Ok();
                case HealthCheckEnum.Database:
                    try
                    {
                        var connectionString = Startup.Configuration.GetSection("ConnectionStrings:PlDatabase")?.Value;
                        using (var cnx = new MySql.Data.MySqlClient.MySqlConnection(connectionString))
                        {
                            return Ok();
                        }
                    }
                    catch (Exception)
                    {
                        return StatusCode((int)HttpStatusCode.InternalServerError);
                    }
                case HealthCheckEnum.Elastic:
                    var elasticResponse = await _elasticConnectionClient.Client.Value.ClusterHealthAsync(cancellationToken: HttpContext.RequestAborted);
                    return elasticResponse.IsValid ? Ok() : StatusCode((int)HttpStatusCode.InternalServerError);
                default:
                    return Ok();
            }
        }

        [HttpGet("/")]
        public IActionResult Index() => 
            new RedirectResult("~/swagger");
            //Ok($"Welcome to Synker Api : {typeof(Startup).GetTypeInfo().Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>().Version}");

        [HttpGet("error")]
        public string GetError() =>
            throw new BusinessException("Test exception");

    }
}
