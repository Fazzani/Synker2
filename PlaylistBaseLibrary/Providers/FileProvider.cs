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
    public abstract class FileProvider : PlaylistProvider<Playlist<TvgMedia>, TvgMedia>
    {
        private bool _disposed = false;
        protected Stream _sr;
        protected FileProvider(Stream sr)
        {
            _sr = sr;
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
        /// <param name="stream"></param>
        /// <returns></returns>
        public static FileProvider Create(string provider, List<PlaylistProviderOption> providersOptions, Stream stream)
        {
            var sourceOption = providersOptions.FirstOrDefault(x => x.Name.Equals(provider, StringComparison.InvariantCultureIgnoreCase));
            if (sourceOption == null)
                throw new InvalidFileProviderException($"Source Provider not found : {provider}");

            var targetProviderType = Type.GetType(sourceOption.Type, false, true);
            if (targetProviderType == null)
                throw new InvalidFileProviderException($"Target Provider not found : {sourceOption.Type}");

            return (FileProvider)Activator.CreateInstance(targetProviderType, stream);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _sr.Close();
                    _sr.Dispose();
                }
                _disposed = true;
            }
        }
    }

    public class PlaylistProviderOption
    {
        public string Name { get; set; }
        public string Type { get; set; }
    }

    public class InvalidFileProviderException : Exception
    {
        public InvalidFileProviderException(string message) : base(message)
        {

        }

        public InvalidFileProviderException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}
