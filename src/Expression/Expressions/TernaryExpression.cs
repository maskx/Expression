using maskx.Expression.Visitors;
using System.Collections.Generic;

namespace maskx.Expression.Expressions
{
    public class TernaryExpression : LogicalExpression
    {
        public TernaryExpression(LogicalExpression leftExpression, LogicalExpression middleExpression, LogicalExpression rightExpression)
        {
            this.LeftExpression = leftExpression;
            this.MiddleExpression = middleExpression;
            this.RightExpression = rightExpression;
        }

        public LogicalExpression LeftExpression { get; set; }

        public LogicalExpression MiddleExpression { get; set; }

        public LogicalExpression RightExpression { get; set; }

        public override void Accept(LogicalExpressionVisitor visitor, Dictionary<string, object> context = null)
        {
            visitor.Visit(this,context);
        }
    }
}