using maskx.Expression;
using System.Collections.Generic;
using Xunit;

namespace ExpressionTest
{
    [Trait("C", "Context")]
    public class ContextTest
    {
        [Fact(DisplayName = "CanAccessContext")]
        public void CanAccessContext()
        {
            var expression = new Expression("Abs()");
            expression.EvaluateFunction = (name, args, cxt) =>
            {
                if (name == "Abs")
                {
                    args.Result = System.Math.Abs((int)cxt["value"]);
                }
            };
            Assert.Equal(1, expression.Evaluate(new Dictionary<string, object>() { { "value", -1 } }));
        }
    }
}