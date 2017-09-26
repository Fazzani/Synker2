﻿using System;
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
using hfa.WebApi.Dal;

namespace Hfa.WebApi.Controllers
{
    public class BaseController : Controller
    {
        protected readonly ILogger _logger;
        protected readonly IElasticConnectionClient _elasticConnectionClient;
        protected CancellationTokenSource cancelToken;
        IOptions<ApplicationConfigData> _config;
        SynkerDbContext context;

        public BaseController(IOptions<ApplicationConfigData> config, ILoggerFactory loggerFactory, IElasticConnectionClient elasticConnectionClient, SynkerDbContext context)
        {
            _logger = loggerFactory.CreateLogger("BaseController");
            _elasticConnectionClient = elasticConnectionClient;
            cancelToken = new CancellationTokenSource();
            _config = config;
            this.context = context;
        }

        internal protected async Task<IActionResult> SearchAsync<T>([FromBody] string query) where T : class
        {
            var response = await _elasticConnectionClient.Client.LowLevel.SearchAsync<SearchResponse<T>>(_config.Value.DefaultIndex, typeof(T).Name.ToLowerInvariant(), query);

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

    public class Constants
    {
        public const string ELK_KEYWORD_SUFFIX = "keyword";
    }
}
