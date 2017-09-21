using PlaylistBaseLibrary.Providers.Linq;
using PlaylistManager.Entities;
using System;
using System.Linq;
using System.Linq.Expressions;
using TvheadendLibrary.Common;

namespace TvheadendLibrary
{
    internal class QueryParamsTranslator : ExpressionVisitor
    {
        QueryParams _queryParams;
        internal QueryParamsTranslator()
        {
            _queryParams = new QueryParams();
        }

        internal IQuery Translate(Expression expression)
        {
            Visit(expression);
            return _queryParams;
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method.DeclaringType == typeof(Queryable))
            {
                switch (m.Method.Name)
                {
                    case "Select":
                        Visit(m.Arguments[0]);
                        var lambdaSelect = (LambdaExpression)TypeSystem.StripQuotes(m.Arguments[1]);
                        var projection = new ColumnProjector().ProjectColumns(lambdaSelect.Body);
                        _queryParams.Columns = projection.Columns;
                        return m;
                    case "Where":
                        Visit(m.Arguments[0]);
                        var lambda = (LambdaExpression)TypeSystem.StripQuotes(m.Arguments[1]);
                        Visit(lambda.Body);
                        return m;
                    case "OrderBy":
                    case "OrderByDescending":
                        Visit(m.Arguments[0]);
                        //TODO: must New ExpressionVisitor
                        //var lambdaOrderBy = (LambdaExpression)StripQuotes(m.Arguments[1]);
                        //var memberOrderBy = lambdaOrderBy.Body;
                        //if (memberOrderBy == null)
                        //    throw new NotImplementedException("OrderBy supported only for member");
                        //_queryParams.Dir = m.Method.Name.Equals("OrderBy") ? "ASC" : "DESC";
                        //_queryParams.Sort =  memberOrderBy.Member.Name;
                        return m;
                    case "Take":
                        Visit(m.Arguments[0]);
                        var constantTake = m.Arguments[1] as ConstantExpression;
                        if (constantTake == null)
                            throw new NotImplementedException("Take supported only for constant values");
                        if (int.TryParse(constantTake.Value.ToString(), out int take))
                            _queryParams.Limit = take;
                        return m;
                    case "Skip":
                        Visit(m.Arguments[0]);
                        var constantSkip = m.Arguments[1] as ConstantExpression;
                        if (constantSkip == null)
                            throw new NotImplementedException("Skip supported only for constant values");
                        if (int.TryParse(constantSkip.Value.ToString(), out int skip))
                            _queryParams.Start  = skip;
                        return m;
                }
            }
            throw new NotSupportedException(string.Format("The method '{0}' is not supported", m.Method.Name));
        }

        /// <summary>
        /// filter:[{"type":"string","value":"bein","field":"name"},
        ///         {"type":"numeric","comparison":"lt","value":10000000,"intsplit":1000000,"field":"number"}]
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override Expression VisitBinary(BinaryExpression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    Visit(node.Left);
                    Visit(node.Right);
                    break;

                case ExpressionType.Constant:
                    return node;

                case ExpressionType.NotEqual:
                case ExpressionType.Equal:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                    // FIRST STEP
                    // We support only a comparison between constant
                    // and a possible flight query parameter
                    ConstantExpression constant =
                    (node.Left as ConstantExpression ?? node.Right as ConstantExpression);
                    MemberExpression memberAccess =
                    (node.Left as MemberExpression ?? node.Right as MemberExpression);
                    // SECOND STEP
                    // Sanity check of parameters
                    if ((memberAccess == null) || (constant == null))
                        throw new NotSupportedException(string.Format("The binary operator '{0}' must compare a valid " + "flight attribute with a constant", node.NodeType));
                    if (constant.Value == null)
                        throw new NotSupportedException(string.Format("NULL constant is not supported in binary operator {0}", node.ToString()));

                    switch (Type.GetTypeCode(constant.Value.GetType()))
                    {
                        case TypeCode.String:
                        case TypeCode.Int16:
                        case TypeCode.Int32:
                        case TypeCode.Double:
                            break;
                        default:
                            throw new NotSupportedException(string.Format("Constant {0} is of an unsupported type ({1})", constant.ToString(), constant.Value.GetType().Name));
                    }

                    _queryParams.Filters.Add(FactoryQueryParamsFilter.CreateNumericQueryParamsFilter(memberAccess.Member.Name.ToLowerInvariant(), constant.Value.ToString(), GetComparaison(node.NodeType)));
                    break;
                case ExpressionType.Parameter:
                    break;
                case ExpressionType.IsTrue:
                case ExpressionType.IsFalse:
                    CheckType(node, typeof(bool));
                    break;
                default:
                    throw new NotSupportedException();
            }

            return node;
        }

        private string GetComparaison(ExpressionType nodeType)
        {
            //TODO: Get Comparaison
            throw new NotImplementedException();
        }

        private static void CheckType(BinaryExpression node, Type type)
        {
            var member = (node.Left as MemberExpression) ?? (node.Right as MemberExpression);
            if (member != null && member.Member.DeclaringType != type)
                throw new NotSupportedException($"{member.Member.Name} must be boolean type");
        }

        //protected override Expression VisitConstant(ConstantExpression c)
        //{
        //    //StringBuilder s = new StringBuilder ();
        //    //s.AppendFormat("CopyAndModify.VisitConstant expression type {0}", c.Type.ToString());
        //    //System.Console.WriteLine(s.ToString ());
        //    // if (c.Type == typeof(IQueryable<LinqFileSystemProvider.FileSystemContext>))
        //    if (c.Type == typeof(LinqFileSystemProvider.FileSystemContext))
        //    {
        //        //System.Console.WriteLine("CopyAndModify.VisitConstant(IQueryable<FileSystemContext)");
        //        return Expression.Constant(this.fileSystemElements);
        //    }
        //    else
        //    {
        //        //System.Console.WriteLine("CopyAndModify.VisitConstant(uninteresting type)");
        //        return c;
        //    }
        //}

        //protected override Expression VisitUnary(UnaryExpression node)
        //{
        //    switch (node.NodeType)
        //    {
        //        case ExpressionType.IsTrue:
        //        case ExpressionType.IsFalse:
        //            var member = (node.Left as MemberExpression) ?? (node.Right as MemberExpression);
        //            if (member != null && member.Member.DeclaringType != typeof(bool))
        //                throw new NotSupportedException($"{member.Member.Name} must be boolean type");
        //            break;
        //    }
        //}

        
    }
}