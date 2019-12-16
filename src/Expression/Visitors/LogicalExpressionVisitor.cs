using maskx.Expression.Expressions;
using System.Collections.Generic;

namespace maskx.Expression.Visitors
{
    public abstract class LogicalExpressionVisitor
    {
        public abstract void Visit(TernaryExpression expression);

        public abstract void Visit(BinaryExpression expression);

        public abstract void Visit(UnaryExpression expression);

        public abstract void Visit(ValueExpression expression);

        public abstract void Visit(FunctionExpression function, Dictionary<string, object> context = null);

        public abstract void Visit(IdentifierExpression function);

        public abstract void Visit(MemberExpression property, Dictionary<string, object> context = null);
    }
}