using maskx.Expression.Visitors;
using System.Collections.Generic;

namespace maskx.Expression.Expressions
{
    public class IdentifierExpression : LogicalExpression
    {
        public IdentifierExpression(string name)
        {
            Name = name;
        }

        public string Name { get; set; }

        public override void Accept(LogicalExpressionVisitor visitor, Dictionary<string, object> context = null)
        {
            visitor.Visit(this);
        }
    }
}