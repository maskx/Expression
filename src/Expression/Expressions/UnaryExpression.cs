using maskx.Expression.Visitors;
using System.Collections.Generic;

namespace maskx.Expression.Expressions
{
    public class UnaryExpression : LogicalExpression
    {
        public UnaryExpression(UnaryExpressionType type, LogicalExpression expression)
        {
            Type = type;
            Expression = expression;
        }

        public LogicalExpression Expression { get; set; }

        public UnaryExpressionType Type { get; set; }

        public override void Accept(LogicalExpressionVisitor visitor, Dictionary<string, object> context = null)
        {
            visitor.Visit(this,context);
        }
    }

    public enum UnaryExpressionType
    {
        Not,
        Negate,
        BitwiseNot
    }
}