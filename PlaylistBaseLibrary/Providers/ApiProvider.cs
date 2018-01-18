using PlaylistBaseLibrary.Providers.Linq;
using PlaylistManager.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace hfa.PlaylistBaseLibrary.Providers
{
    public abstract class ApiProvider : PlaylistProvider<Playlist<TvgMedia>, TvgMedia>
    {
        private bool _disposed = false;

        public string Url { get; }

        protected ApiProvider(string url)
        {
            Url = url;
        }

        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        public override object _(Expression expression)
        {
            //if (!IsQueryOverDataSource(expression))
            //    throw new InvalidProgramException("No query over the data source was specified.");

            var res = Pull();
            var IqRes = res.AsQueryable();
            var methodsVisitor = new FunctionFinderExpressionVisitor();
            methodsVisitor.GetMethods(expression);

            if (methodsVisitor["where"] is Expression<Func<TvgMedia, bool>> whereExpression)
                res = IqRes.Where(whereExpression);

            if (methodsVisitor["select"] is LambdaExpression selectExpression)
            {
                Type outputType = expression.Type.GenericTypeArguments.FirstOrDefault();
                var selectMethod = typeof(Enumerable)
                  .GetMethods(BindingFlags.Static | BindingFlags.Public)
                  .Single(mi => mi.Name == "Select" &&
                               mi.GetParameters()[1].ParameterType.GetGenericArguments().Count() == 2)
                  .MakeGenericMethod(new Type[] { typeof(TvgMedia), outputType });

                return selectMethod.Invoke(null, new object[] { IqRes, selectExpression.Compile() });
            }
            //TODO: finish the rest of functions (GroupBy, OrderBy, Take, Distinct, Skip, etc...)
            return IqRes;
        }

        /// <summary>
        /// Get FileProvider instance from name (string)
        /// ex: m3u, tvlist
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="providersOptions"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public static ApiProvider Create(string provider, List<PlaylistProviderOption> providersOptions, string url)
        {
            var sourceOption = providersOptions.FirstOrDefault(x => x.Name.Equals(provider, StringComparison.InvariantCultureIgnoreCase));
            if (sourceOption == null)
                throw new InvalidProviderException($"Source Provider not found : {provider}");

            var targetProviderType = Type.GetType(sourceOption.Type, false, true);
            if (targetProviderType == null)
                throw new InvalidProviderException($"Target Provider not found : {sourceOption.Type}");

            return (ApiProvider)Activator.CreateInstance(targetProviderType, url);
        }

        protected virtual void Dispose(bool disposing)
        {
        }
    }

    public class InvalidProviderException : Exception
    {
        public InvalidProviderException(string message) : base(message)
        {

        }

        public InvalidProviderException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }

}
