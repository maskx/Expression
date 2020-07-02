using maskx.Expression.Expressions;
using System.Collections.Generic;

namespace maskx.Expression.Visitors
{
    public abstract class LogicalExpressionVisitor
    {
        public abstract void Visit(TernaryExpression expression, Dictionary<string, object> context = null);

        public abstract void Visit(BinaryExpression expression, Dictionary<string, object> context = null);

        public abstract void Visit(UnaryExpression expression, Dictionary<string, object> context = null);

        public abstract void Visit(ValueExpression expression, Dictionary<string, object> context = null);

        public abstract void Visit(FunctionExpression function, Dictionary<string, object> context = null);

        public abstract void Visit(IdentifierExpression function, Dictionary<string, object> context = null);

        public abstract void Visit(MemberExpression property, Dictionary<string, object> context = null);

        public abstract void Visit(IndexerExpression expression, Dictionary<string, object> context = null);
    }
}