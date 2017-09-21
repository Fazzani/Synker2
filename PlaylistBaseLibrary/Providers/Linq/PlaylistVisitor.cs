using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace PlaylistBaseLibrary.Providers.Linq
{

    public class PlaylistVisitor : ExpressionVisitor
    {
        private StringBuilder command = new StringBuilder("{x}");
        public string GetCommand(Expression expression)
        {
            Visit(expression);

            return string.Format("SELECT * FROM table WHERE {0}", command.ToString());
        }
    }

    internal static class StringBuilderExtensions
    {
        internal static void ReplaceFirst(this StringBuilder sb, string search, string replacement)
        {
            int position = sb.ToString().IndexOf(search);

            if (position > -1)
            {
                sb.Replace(search, replacement, position, search.Length);
            }
        }
    }
}
