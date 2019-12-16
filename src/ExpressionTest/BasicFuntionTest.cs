using maskx.Expression;
using maskx.Expression.Expressions;
using System.Collections.Generic;
using Xunit;

namespace ExpressionTest
{
    [Trait("C", "BasicFuntion")]
    public class BasicFuntionTest
    {
        [Fact(DisplayName = "ShouldEvaluateOperators")]
        public void ShouldEvaluateOperators()
        {
            var expressions = new Dictionary<string, object>
                                  {
                                      {"2 * 3", 6},
                                      {"6 / 2", 3d},
                                      {"7 % 2", 1},
                                      {"2 + 3", 5},
                                      {"2 - 1", 1},
                                      {"1 < 2", true},
                                      {"1 > 2", false},
                                      {"1 <= 2", true},
                                      {"1 <= 1", true},
                                      {"1 >= 2", false},
                                      {"1 >= 1", true},
                                      {"1 = 1", true},
                                      {"1 == 1", true},
                                      {"1 != 1", false},
                                      {"1 <> 1", false},
                                      {"1 & 1", 1},
                                      {"1 | 1", 1},
                                      {"1 ^ 1", 0},
                                      {"~1", ~1},
                                      {"2 >> 1", 1},
                                      {"2 << 1", 4}
                                  };

            foreach (KeyValuePair<string, object> pair in expressions)
            {
                Assert.Equal(pair.Value, new Expression(pair.Key).Evaluate());
            }
        }

        [Fact(DisplayName = "ShouldHandleOperatorsPriority")]
        public void ShouldHandleOperatorsPriority()
        {
            Assert.Equal(8, new Expression("2+2+2+2").Evaluate());
            Assert.Equal(16, new Expression("2*2*2*2").Evaluate());
            Assert.Equal(6, new Expression("2*2+2").Evaluate());
            Assert.Equal(6, new Expression("2+2*2").Evaluate());

            Assert.Equal(9d, new Expression("1 + 2 + 3 * 4 / 2").Evaluate());
            Assert.Equal(13.5, new Expression("18/2/2*3").Evaluate());
        }

        [Fact(DisplayName = "ShouldEvaluateTernaryExpression")]
        public void ShouldEvaluateTernaryExpression()
        {
            Assert.Equal(1, new Expression("1+2<3 ? 3+4 : 1").Evaluate());
        }

        [Fact(DisplayName = "ShouldSerializeExpression")]
        public void ShouldSerializeExpression()
        {
            Assert.Equal("1 / 2", new BinaryExpression(BinaryExpressionType.Div, new ValueExpression(1), new ValueExpression(2)).ToString());
            Assert.Equal("1 = 2", new BinaryExpression(BinaryExpressionType.Equal, new ValueExpression(1), new ValueExpression(2)).ToString());
            Assert.Equal("1 > 2", new BinaryExpression(BinaryExpressionType.Greater, new ValueExpression(1), new ValueExpression(2)).ToString());
            Assert.Equal("1 >= 2", new BinaryExpression(BinaryExpressionType.GreaterOrEqual, new ValueExpression(1), new ValueExpression(2)).ToString());
            Assert.Equal("1 < 2", new BinaryExpression(BinaryExpressionType.Lesser, new ValueExpression(1), new ValueExpression(2)).ToString());
            Assert.Equal("1 <= 2", new BinaryExpression(BinaryExpressionType.LesserOrEqual, new ValueExpression(1), new ValueExpression(2)).ToString());
            Assert.Equal("1 - 2", new BinaryExpression(BinaryExpressionType.Minus, new ValueExpression(1), new ValueExpression(2)).ToString());
            Assert.Equal("1 % 2", new BinaryExpression(BinaryExpressionType.Modulo, new ValueExpression(1), new ValueExpression(2)).ToString());
            Assert.Equal("1 != 2", new BinaryExpression(BinaryExpressionType.NotEqual, new ValueExpression(1), new ValueExpression(2)).ToString());
            Assert.Equal("1 + 2", new BinaryExpression(BinaryExpressionType.Plus, new ValueExpression(1), new ValueExpression(2)).ToString());
            Assert.Equal("1 * 2", new BinaryExpression(BinaryExpressionType.Times, new ValueExpression(1), new ValueExpression(2)).ToString());

            Assert.Equal("1", new ValueExpression(1).ToString());
            Assert.Equal("1.234", new ValueExpression(1.234).ToString());
            Assert.Equal("\"hello\"", new ValueExpression("hello").ToString());

            Assert.Equal("Sum(1 + 2)", new FunctionExpression(
                new IdentifierExpression("Sum"),
                new[] { new BinaryExpression(BinaryExpressionType.Plus, new ValueExpression(1), new ValueExpression(2)) }).ToString());
        }
    }
}