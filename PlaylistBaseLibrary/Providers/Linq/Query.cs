using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace PlaylistBaseLibrary.Providers.Linq
{
    public class Query<T> : IOrderedQueryable<T>, IOrderedQueryable
    {
        readonly QueryProvider _provider;
        readonly Expression _expression;

        public Query(QueryProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException("provider");

            _provider = provider;
            _expression = Expression.Constant(this);
        }

        public Query(QueryProvider provider, Expression expression)
        {
            if (provider == null)
                throw new ArgumentNullException("provider");

            if (expression == null)
                throw new ArgumentNullException("expression");

            if (!typeof(IQueryable<T>).IsAssignableFrom(expression.Type))
                throw new ArgumentOutOfRangeException("expression");

            _provider = provider;
            _expression = expression;
        }

        Expression IQueryable.Expression
        {
            get { return _expression; }
        }

        Type IQueryable.ElementType
        {
            get { return typeof(T); }
        }

        IQueryProvider IQueryable.Provider
        {
            get { return _provider; }
        }

        public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)_provider._(_expression)).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_provider._(_expression)).GetEnumerator();

    }
}
