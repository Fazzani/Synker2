using PlaylistManager.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace PlaylistBaseLibrary.Providers.Linq
{
    public abstract class QueryProvider : IQueryProvider
    {
        protected QueryProvider()
        {

        }

        IQueryable<S> IQueryProvider.CreateQuery<S>(Expression expression)
        {
            return new Query<S>(this, expression);
        }
        IQueryable IQueryProvider.CreateQuery(Expression expression)
        {
            Type elementType = TypeSystem.GetElementType(expression.Type);

            try
            {
                return (IQueryable)Activator.CreateInstance(typeof(Playlist<>).MakeGenericType(elementType), new object[] { this, expression });
            }

            catch (TargetInvocationException tie)
            {
                throw tie.InnerException;
            }

        }

        TResult IQueryProvider.Execute<TResult>(Expression expression)
        {
            var res = _(expression);
            return (TResult)res;
        }

        object IQueryProvider.Execute(Expression expression) => _(expression);

        public abstract object _(Expression expression);

        protected static bool IsQueryOverDataSource(Expression expression)
        {
            // If expression represents an unqueried IQueryable data source instance, 
            // expression is of type ConstantExpression, not MethodCallExpression. 
            return (expression is MethodCallExpression);
        }
    }
}
