using maskx.Expression.Visitors;
using System.Collections.Generic;

namespace maskx.Expression.Expressions
{
    public abstract class LogicalExpression
    {
        public override string ToString()
        {
            var serializer = new SerializationVisitor();
            this.Accept(serializer);

            return serializer.Result.ToString().TrimEnd(' ');
        }

        public abstract void Accept(LogicalExpressionVisitor visitor, Dictionary<string, object> context = null);
    }
}