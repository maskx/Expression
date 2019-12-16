using System;
using maskx.Expression;
using Xunit;

namespace ExpressionTest
{
    [Trait("C", "MemberExpression")]
    public class MemberExpressionTest
    {
        [Fact(DisplayName = "CanAccessProperty")]
        public void CanAccessProperty()
        {
            var expression = new Expression("GetDate().Year");
            expression.EvaluateFunction = (name, args, cxt) =>
            {
                if (name == "GetDate")
                {
                    args.Result = DateTime.Now;
                }
            };
            Assert.Equal(DateTime.Now.Year, expression.Evaluate());
        }

        [Fact(DisplayName = "CanAccessMultiLevelProperty")]
        public void CanAccessMultiLevelProperty()
        {
            var expression = new Expression("GetDate().Date.Year");
            expression.EvaluateFunction = (name, args, cxt) =>
            {
                if (name == "GetDate")
                {
                    args.Result = DateTime.Now;
                }
            };
            Assert.Equal(DateTime.Now.Date.Year, expression.Evaluate());
        }

        [Fact(DisplayName = "MixlPropertyAndMethod")]
        public void MixlPropertyAndMethod()
        {
            var expression = new Expression("GetDate().Date.AddYears(1).Year");
            expression.EvaluateFunction = (name, args, cxt) =>
            {
                if (name == "GetDate")
                {
                    args.Result = DateTime.Now;
                }
            };
            Assert.Equal(DateTime.Now.Date.AddYears(1).Year, expression.Evaluate());
        }
    }
}