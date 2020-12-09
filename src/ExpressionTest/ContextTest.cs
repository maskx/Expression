using maskx.Expression;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Xunit;

namespace ExpressionTest
{
    [Trait("C", "Context")]
    public class ContextTest
    {
        [Fact(DisplayName = "CanAccessContext")]
        public void CanAccessContext()
        {
            var expression = new Expression("Abs()")
            {
                EvaluateFunction = (name, args, cxt) =>
                {
                    if (name == "Abs")
                    {
                        args.Result = System.Math.Abs((int)cxt["value"]);
                    }
                }
            };
            Assert.Equal(1, expression.Evaluate(new Dictionary<string, object>() { { "value", -1 } }));
        }
        [Fact(DisplayName = "IndexerShouldPassContext")]
        public void IndexerShouldPassContext()
        {
            var expression = new Expression("parameters('diskinfo').dataDiskResources[copyIndex()].diskSize")
            {
                EvaluateFunction = (name, args, cxt) =>
                {
                    if (name == "parameters")
                    {
                        args.Result = new { dataDiskResources = new List<object> { new { diskSize = 1 } } };
                    }
                    else if (name == "copyIndex")
                    {
                        Assert.NotEmpty(cxt);
                        args.Result = 0;
                    }
                }
            };
            Assert.Equal(1, expression.Evaluate(new Dictionary<string, object>() { { "value", -1 } }));
        }
        [Fact(DisplayName = "ArgsShouldTakeContext")]
        public void ArgsShouldTakeContext()
        {
            var expression = new Expression("A(B())")
            {
                EvaluateFunction = (name, args, cxt) =>
                {
                    if (name == "A")
                    {
                        args.Result = args.Parameters[0].Evaluate(cxt);
                    }
                    else if (name == "B")
                    {
                        args.Result = cxt["value"];
                    }
                }
            };
            Assert.Equal(1, expression.Evaluate(new Dictionary<string, object>() { { "value", 1 } }));
        }

        [Fact(DisplayName = "FunctionShouldTakeContextWhenGetReturnObjectProperty")]
        public void FunctionShouldTakeContextWhenGetReturnObjectProperty()
        {
            var expression = new Expression("A().ToString()")
            {
                EvaluateFunction = (name, args, cxt) =>
                {
                    if (name == "A")
                    {
                        args.Result = cxt["value"];
                    }
                }
            };
            Assert.Equal("1", expression.Evaluate(new Dictionary<string, object>() { { "value", 1 } }));
        }
    }
}