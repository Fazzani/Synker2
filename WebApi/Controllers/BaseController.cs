using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using Nest;
using Hfa.WebApi.Models;
using Microsoft.Extensions.Logging;
using Hfa.WebApi.Common;
using System.Linq.Expressions;
using System.Reflection;
using hfa.SyncLibrary.Global;
using hfa.WebApi.Common;
using Microsoft.Extensions.Options;
using hfa.Synker.Services.Dal;
using hfa.Synker.Service.Services.Elastic;
using hfa.Synker.Service.Elastic;
using hfa.WebApi.Models.Elastic;

namespace Hfa.WebApi.Controllers
{
    public class BaseController : Controller
    {
        protected readonly ILogger _logger;
        protected readonly IElasticConnectionClient _elasticConnectionClient;
        protected readonly ElasticConfig _elasticConfig;
        readonly protected SynkerDbContext _dbContext;

        public BaseController(IOptions<ElasticConfig> elasticConfig, ILoggerFactory loggerFactory, IElasticConnectionClient elasticConnectionClient, 
            SynkerDbContext context)
        {
            _logger = loggerFactory.CreateLogger("BaseController");
            _elasticConnectionClient = elasticConnectionClient;
            _elasticConfig = elasticConfig.Value;
            _dbContext = context;
        }

        internal virtual protected async Task<IActionResult> SearchAsync<T, T2>([FromBody] string query, CancellationToken cancellationToken)
            where T : class where T2 : class, IModel<T,T2>, new()
        {
            var response = await _elasticConnectionClient.Client.LowLevel
                .SearchAsync<SearchResponse<T>>(_elasticConfig.DefaultIndex, typeof(T).Name.ToLowerInvariant(), query, null, cancellationToken);

            if (!response.SuccessOrKnownError)
                return BadRequest(response.DebugInformation);

            response.Body.AssertElasticResponse();
            return new OkObjectResult(response.Body.GetResultListModel<T, T2>());
        }

        internal virtual protected async Task<IActionResult> SearchAsync<T, T2>([FromBody] string query, string indexName, CancellationToken cancellationToken)
             where T : class where T2 : class, IModel<T, T2>, new()
        {
            var response = await _elasticConnectionClient.Client.LowLevel
                .SearchAsync<SearchResponse<T>>(indexName, typeof(T).Name.ToLowerInvariant(), query, null, cancellationToken);

            if (!response.SuccessOrKnownError)
                return BadRequest(response.DebugInformation);

            response.Body.AssertElasticResponse();
            return new OkObjectResult(response.Body.GetResultListModel<T, T2>());
        }

        internal virtual protected async Task<IActionResult> SearchQueryStringAsync<T, T2>([FromBody] SimpleQueryElastic simpleQueryElastic,CancellationToken cancellationToken)
             where T : class where T2 : class, IModel<T, T2>, new()
        {
            var response = await _elasticConnectionClient.Client.SearchAsync<T>(s => s
            .Index(simpleQueryElastic.IndexName)
            .From(simpleQueryElastic.From)
            .Size(simpleQueryElastic.Size)
            .Query(q => new SimpleQueryStringQuery { Query = simpleQueryElastic.Query, AllFields = true, AnalyzeWildcard = true }));

            if (!response.IsValid)
                return BadRequest(response.DebugInformation);

            return new OkObjectResult(response.GetResultListModel<T, T2>());
        }

        protected static IPromise<IList<ISort>> GetSortDescriptor<T>(SortDescriptor<T> me, Dictionary<string, SortDirectionEnum> dictionary) where T : class
        {
            if (dictionary != null)
            {
                foreach (var item in dictionary)
                {
                    var prop = typeof(T).GetProperty(item.Key, BindingFlags.IgnoreCase | BindingFlags.FlattenHierarchy | BindingFlags.Instance |
                              BindingFlags.Public);

                    var fieldNameExp = Expression.Property(Expression.Parameter(typeof(T)), prop);
                    var suffixMethod = typeof(SuffixExtensions).GetMethod("Suffix", BindingFlags.Public | BindingFlags.Static);
                    var callmethod = Expression.Call(suffixMethod, fieldNameExp, Expression.Constant(Constants.ELK_KEYWORD_SUFFIX));
                    var lambda = Expression.Lambda(callmethod, Expression.Parameter(typeof(T)));

                    if (item.Value == SortDirectionEnum.Asc)
                    {
                        me.Ascending(lambda);
                    }
                    else
                    {
                        me.Descending(lambda);
                    }
                }

                return me;
            }

            return default(IPromise<IList<ISort>>);
        }

        internal class NestPromise<T> : IPromise<T> where T : class
        {
            public NestPromise(T value)
            {
                Value = value;
            }
            public T Value { get; }
        }

        protected int? UserId => Convert.ToInt32(User.FindFirst("id")?.Value);
    }

    public class Constants
    {
        public const string ELK_KEYWORD_SUFFIX = "keyword";
    }
}
