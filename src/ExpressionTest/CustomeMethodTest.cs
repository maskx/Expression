using maskx.Expression;
using Xunit;

namespace ExpressionTest
{
    [Trait("C", "CustomeMethod")]
    public class CustomeMethodTest
    {
        [Fact(DisplayName = "Abs")]
        public void Abs()
        {
            var expression = new Expression("Abs(-1)");
            expression.EvaluateFunction = (name, args, cxt) =>
            {
                if (name == "Abs")
                {
                    args.Result = System.Math.Abs((int)args.Parameters[0].Evaluate());
                }
            };
            Assert.Equal(1, expression.Evaluate());
        }
    }
}