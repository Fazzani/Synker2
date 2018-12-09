namespace hfa.WebApi.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Hfa.WebApi.Controllers;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using hfa.WebApi.Common.Exceptions;
    using Microsoft.AspNetCore.Authorization;
    using System.Net;
    using hfa.WebApi.Models.HealthCheck;
    using hfa.Synker.Services.Dal;
    using hfa.Synker.Service.Services.Elastic;
    using hfa.Synker.Service.Elastic;

    [AllowAnonymous]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiController]
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
                        using (var cnx = new Npgsql.NpgsqlConnection(connectionString))
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

        ///// <summary>
        ///// Sync database mysql => PostgresSQL
        ///// </summary>
        ///// <returns></returns>
        //[HttpGet("syncdatabase")]
        //[ProducesResponseType((int)HttpStatusCode.OK)]
        //public async Task<IActionResult> GetSyncDataBases(CancellationToken cancellationToken)
        //{
        //    using (var mysqlDb = new SynkerDbContext(new DbContextOptionsBuilder().UseMySql("server=synker.ovh;user id=pl;pwd=password;port=8889;database=playlist;").Options))
        //    {
        //        await _dbContext.ConnectionState.AddRangeAsync(mysqlDb.ConnectionState, cancellationToken);
        //        await _dbContext.Hosts.AddRangeAsync(mysqlDb.Hosts, cancellationToken);
        //        await _dbContext.Roles.AddRangeAsync(mysqlDb.Roles, cancellationToken);
        //        await _dbContext.Users.AddRangeAsync(mysqlDb.Users, cancellationToken);
        //        await _dbContext.Playlist.AddRangeAsync(mysqlDb.Playlist, cancellationToken);
        //        await _dbContext.Command.AddRangeAsync(mysqlDb.Command, cancellationToken);
        //        //await _dbContext.Messages.AddRangeAsync(mysqlDb.Messages, cancellationToken);
        //        await _dbContext.SaveChangesAsync(cancellationToken);
        //    }
        //    return Ok();
        //}

    }
}
