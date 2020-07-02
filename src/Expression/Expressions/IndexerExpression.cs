using maskx.Expression.Visitors;
using System.Collections.Generic;

namespace maskx.Expression.Expressions
{
    public class IndexerExpression : LogicalExpression
    {
        public LogicalExpression LeftExpression { get; private set; }
        public LogicalExpression rightExpression { get; private set; }

        public IndexerExpression(LogicalExpression left, LogicalExpression right)
        {
            this.LeftExpression = left;
            this.rightExpression = right;
        }

        public bool IsNamespace { get; set; }

        public override void Accept(LogicalExpressionVisitor visitor, Dictionary<string, object> context = null)
        {
            visitor.Visit(this, context);
        }
    }
}
