using System;
using System.Collections.Generic;
using System.Dynamic;
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

        [Fact(DisplayName = "DynamicObjectProperty")]
        public void DynamicObjectProperty()
        {
            var expression = new Expression("GetDynamicObject().Year");
            expression.EvaluateFunction = (name, args, cxt) =>
            {
                if (name == "GetDynamicObject")
                {
                    args.Result = new MyDynamicObject();
                }
            };
            Assert.Equal("Year", expression.Evaluate());
        }

        //[Fact(DisplayName = "DynamicObjectMethod")]
        //public void DynamicObjectMethod()
        //{
        //    var expression = new Expression("GetDynamicObject().Method(1,2)");
        //    expression.EvaluateFunction = (name, args, cxt) =>
        //    {
        //        if (name == "GetDynamicObject")
        //        {
        //            args.Result = new MyDynamicObject();
        //        }
        //    };
        //    Assert.Equal("Year", expression.Evaluate());
        //}

        public class MyDynamicObject : DynamicObject
        {
            public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
            {
                result = $"{binder.Name}:{string.Join(",", args)}";
                return true;
            }

            public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
            {
                return base.TryGetIndex(binder, indexes, out result);
            }

            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                result = binder.Name;
                return true;
            }
        }

        [Fact(DisplayName = "SouldSupportNamespce")]
        public void SouldSupportNamespce()
        {
            var expression = new Expression("N1.N2.DateNow.Date.AddDays(1).ToString()");
            expression.TryGetObject = (name) =>
              {
                  if (name == "N1.N2.DateNow")
                      return DateTime.Now;
                  return null;
              };
            Assert.Equal(DateTime.Now.Date.AddDays(1).ToString(), expression.Evaluate(new Dictionary<string, object>() { { "value", 1 } }));
        }

        private class dd
        {
            public string bb { get; set; }
        }

        [Fact(DisplayName = "SupportStaticFunction")]
        public void SupportStaticFunction()
        {
            var expression = new Expression("System.Math.Abs(-1)");
            expression.TryGetObject = (name) =>
            {
                if (name == "System.Math")
                    return typeof(System.Math);
                return null;
            };
            Assert.Equal(1d, expression.Evaluate(new Dictionary<string, object>() { { "value", 1 } }));
        }

        [Fact(DisplayName = "SupportStaticField")]
        public void SupportStaticField()
        {
            var expression = new Expression("System.Math.PI");
            expression.TryGetObject = (name) =>
            {
                if (name == "System.Math")
                    return typeof(System.Math);
                return null;
            };
            Assert.Equal(System.Math.PI, expression.Evaluate(new Dictionary<string, object>() { { "value", 1 } }));
        }

        [Fact(DisplayName = "SupportStaticProperty")]
        public void SupportStaticProperty()
        {
            var expression = new Expression("System.DateTime.Now.Year");
            expression.TryGetObject = (name) =>
            {
                if (name == "System.DateTime")
                    return typeof(System.DateTime);
                return null;
            };
            Assert.Equal(System.DateTime.Now.Year, expression.Evaluate(new Dictionary<string, object>() { { "value", 1 } }));
        }
    }
}