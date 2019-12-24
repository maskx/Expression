using maskx.Expression.Visitors;
using System.Collections.Generic;

namespace maskx.Expression.Expressions
{
    public class FunctionExpression : LogicalExpression
    {
        public FunctionExpression(IdentifierExpression identifier, LogicalExpression[] expressions)
        {
            Identifier = identifier;
            Expressions = expressions;
        }

        public string Namespace { get; set; }
        public IdentifierExpression Identifier { get; set; }

        public LogicalExpression[] Expressions { get; set; }

        public override void Accept(LogicalExpressionVisitor visitor, Dictionary<string, object> context = null)
        {
            visitor.Visit(this, context);
        }
    }
}