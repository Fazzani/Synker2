namespace TvheadendLibrary
{
    using System.Collections.Generic;
    using System.Linq.Expressions;

    internal class ColumnProjection
    {
        internal List<string> Columns;
        internal Expression Selector;
    }
   
    internal class ColumnProjector : ExpressionVisitor
    {
        private List<string> _columns;

        internal List<string> Columns { get => _columns; private set => _columns = value; }

        internal ColumnProjector()
        {
            Columns = new List<string>();
        }

        internal ColumnProjection ProjectColumns(Expression expression)
        {
            var selector = Visit(expression);
            return new ColumnProjection { Columns = _columns, Selector = selector };
        }

        protected override Expression VisitMember(MemberExpression m)
        {
            if (m.Expression != null && m.Expression.NodeType == ExpressionType.Parameter)
            {
                _columns.Add(m.Member.Name.ToLowerInvariant());
                return m;
            }
            else
            {
                return base.VisitMember(m);
            }
        }

    }

}
