using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace PlaylistBaseLibrary.Providers.Linq
{
    class FunctionFinderExpressionVisitor : ExpressionVisitor
    {
        readonly Dictionary<string, Expression> _methods = new Dictionary<string, Expression>();

        public void GetMethods(Expression expression)
        {
            Visit(expression);
        }

        public Expression this[string index]
        {
            get { return _methods.ContainsKey(index.ToLowerInvariant()) ? _methods[index.ToLowerInvariant()] : null; }
            private set { }
        }

        protected override Expression VisitMethodCall(MethodCallExpression expression)
        {
            var lambdaSelect = (LambdaExpression)TypeSystem.StripQuotes(expression.Arguments[1]);
            _methods.Add(expression.Method.Name.ToLowerInvariant(), lambdaSelect);
            Visit(expression.Arguments[0]);

            return expression;
        }
    }
}
