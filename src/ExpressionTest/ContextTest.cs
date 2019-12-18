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

        [Fact(DisplayName = "ArgsShouldTakeContext")]
        public void ArgsShouldTakeContext()
        {
            var expression = new Expression("A(B())");
            expression.EvaluateFunction = (name, args, cxt) =>
            {
                if (name == "A")
                {
                    args.Result = args.Parameters[0].Evaluate(cxt);
                }
                else if (name == "B")
                {
                    args.Result = cxt["value"];
                }
            };
            Assert.Equal(1, expression.Evaluate(new Dictionary<string, object>() { { "value", 1 } }));
        }

        [Fact(DisplayName = "FunctionShouldTakeContextWhenGetReturnObjectProperty")]
        public void FunctionShouldTakeContextWhenGetReturnObjectProperty()
        {
            var expression = new Expression("A().ToString()");
            expression.EvaluateFunction = (name, args, cxt) =>
            {
                if (name == "A")
                {
                    args.Result = cxt["value"];
                }
            };
            Assert.Equal("1", expression.Evaluate(new Dictionary<string, object>() { { "value", 1 } }));
        }
    }
}