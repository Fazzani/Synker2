using PlaylistBaseLibrary.Providers.Linq;
using PlaylistManager.Entities;
using PlaylistManager.Providers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace PlaylistBaseLibrary.Providers
{
    public abstract class File : PlaylistProvider<Playlist<TvgMedia>, TvgMedia>
    {
        private bool _disposed = false;
        protected Stream _sr;
        protected File(Stream sr)
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

                return selectMethod.Invoke(null, new object[]  { IqRes, selectExpression.Compile() });
            }
            //TODO: finish the rest of functions (GroupBy, OrderBy, Take, Distinct, Skip, etc...)
            return IqRes;
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
}
