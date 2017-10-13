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

        [HttpGet]
        public bool Get() => true;

        [HttpGet("error")]
        public string GetError() => 
            throw new BusinessException("Test exception");

    }
}
