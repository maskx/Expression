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
                result = indexes[0];
                if (result.ToString()=="{}"){
                    result = new MyDynamicObject();
                }
                return true;
            }

            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                result = binder.Name;
                if (result.ToString() == "MyDynamicObject")
                {
                    result = new MyDynamicObject();
                }
                return true;
            }
        }
        [Fact(DisplayName = "ShouldSupportArray_Int")]
        public void ShouldSupportArray_Int()
        {
            var expression = new Expression("GetDynamicObject()[1]");
            expression.EvaluateFunction = (name, args, cxt) =>
            {
                if (name == "GetDynamicObject")
                {
                    args.Result = new MyDynamicObject();
                }
            };
            Assert.Equal(1, expression.Evaluate());
        }
        [Fact(DisplayName = "ShouldSupportArray_String")]
        public void ShouldSupportArray_String()
        {
            var expression = new Expression("GetDynamicObject()['name']");
            expression.EvaluateFunction = (name, args, cxt) =>
            {
                if (name == "GetDynamicObject")
                {
                    args.Result = new MyDynamicObject();
                }
            };
            Assert.Equal("name", expression.Evaluate());
        }
        [Fact(DisplayName = "ShouldSupportArray_Method")]
        public void ShouldSupportArray_Method()
        {
            var expression = new Expression("GetDynamicObject()[GetIndex()]");
            expression.EvaluateFunction = (name, args, cxt) =>
            {
                if (name == "GetDynamicObject")
                {
                    args.Result = new MyDynamicObject();
                }
                if(name== "GetIndex")
                {
                    args.Result = 1;
                }
            };
            Assert.Equal(1, expression.Evaluate());
        }
        [Fact(DisplayName = "ShouldSupportArray_Member")]
        public void ShouldSupportArray_Member()
        {
            var expression = new Expression("GetDynamicObject()[GetDynamicObject().b]");
            expression.EvaluateFunction = (name, args, cxt) =>
            {
                if (name == "GetDynamicObject")
                {
                    args.Result = new MyDynamicObject();
                }
            };
            Assert.Equal("b", expression.Evaluate());
        }
        [Fact(DisplayName = "ShouldSupportArray_DotMethod")]
        public void ShouldSupportArray_DotMethod()
        {
            var expression = new Expression("GetDynamicObject()[123].ToString('00000')");
            expression.EvaluateFunction = (name, args, cxt) =>
            {
                if (name == "GetDynamicObject")
                {
                    args.Result = new MyDynamicObject();
                }
            };
            Assert.Equal("00123", expression.Evaluate());
        }
        [Fact(DisplayName = "ShouldSupportArray_DotMemeber")]
        public void ShouldSupportArray_DotMember()
        {
            var expression = new Expression("GetDynamicObject()['123'].Length");
            expression.EvaluateFunction = (name, args, cxt) =>
            {
                if (name == "GetDynamicObject")
                {
                    args.Result = new MyDynamicObject();
                }
            };
            Assert.Equal(3, expression.Evaluate());
        }
        [Fact(DisplayName = "ShouldSupportArray_DotArray")]
        public void ShouldSupportArray_DotArray()
        {
            var expression = new Expression("GetDynamicObject()['{}'].MyDynamicObject[4]");
            expression.EvaluateFunction = (name, args, cxt) =>
            {
                if (name == "GetDynamicObject")
                {
                    args.Result = new MyDynamicObject();
                }
            };
            Assert.Equal(4, expression.Evaluate());
        }
        [Fact(DisplayName = "Indexer_Method")]
        public void Indexer_Method()
        {
            var expression = new Expression("GetDynamicObject()['{}'].MyDynamicObject(4)");
            expression.EvaluateFunction = (name, args, cxt) =>
            {
                if (name == "GetDynamicObject")
                {
                    args.Result = new MyDynamicObject();
                }
            };
            Assert.Equal(4, expression.Evaluate());
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

        [Fact(DisplayName = "SupportFunctionWithNamespace")]
        public void SupportFunctionWithNamespace()
        {
            var expression = new Expression("NS1.Method1()");
            expression.EvaluateFunction = (name, args, cxt) =>
            {
                if (name == "NS1.Method1")
                {
                    args.Result = 1;
                }
            };
            Assert.Equal(1, expression.Evaluate(new Dictionary<string, object>() { { "value", 1 } }));
        }
    }
}