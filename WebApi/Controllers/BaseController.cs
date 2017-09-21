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

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Hfa.WebApi.Controllers
{
    public class BaseController : Controller
    {
        protected readonly Microsoft.Extensions.Logging.ILogger _logger;

        public BaseController(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger("BaseController");
        }

        protected CancellationTokenSource cancelToken;

        public BaseController()
        {
            cancelToken = new CancellationTokenSource();
        }

        internal protected async Task<IActionResult> SearchAsync<T>([FromBody] string query) where T : class
        {
            var response = await ElasticConnectionClient.Client.LowLevel.SearchAsync<SearchResponse<T>>(ElasticConnectionClient.DEFAULT_INDEX, typeof(T).Name.ToLowerInvariant(), query);

            cancelToken.Token.ThrowIfCancellationRequested();

            if (!response.SuccessOrKnownError)
                return BadRequest(response.DebugInformation);

            response.Body.AssertElasticResponse();
            return new OkObjectResult(response.Body.GetResultListModel());
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

    }
}
