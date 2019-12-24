using maskx.Expression.Expressions;
using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace maskx.Expression.Visitors
{
    public class EvaluationVisitor : LogicalExpressionVisitor
    {
        private readonly EvaluateOptions _options = EvaluateOptions.None;

        public EvaluationVisitor(EvaluateOptions options)
        {
            _options = options;
        }

        public object Result { get; private set; }

        private object Evaluate(LogicalExpression expression)
        {
            expression.Accept(this);
            return Result;
        }

        /// <summary>
        /// Gets the the most precise type.
        /// </summary>
        /// <param name="a">Type a.</param>
        /// <param name="b">Type b.</param>
        /// <returns></returns>
        private static Type GetMostPreciseType(Type a, Type b)
        {
            foreach (Type t in new[] { typeof(String), typeof(Decimal), typeof(Double), typeof(Int32), typeof(Boolean) })
            {
                if (a == t || b == t)
                {
                    return t;
                }
            }

            return a;
        }

        public int CompareUsingMostPreciseType(object a, object b)
        {
            Type mpt = GetMostPreciseType(a.GetType(), b.GetType());
            return Comparer.Default.Compare(Convert.ChangeType(a, mpt), Convert.ChangeType(b, mpt));
        }

        public override void Visit(TernaryExpression expression)
        {
            // Evaluates the left expression and saves the value
            expression.LeftExpression.Accept(this);
            bool left = Convert.ToBoolean(Result);

            if (left)
            {
                expression.MiddleExpression.Accept(this);
            }
            else
            {
                expression.RightExpression.Accept(this);
            }
        }

        public override void Visit(BinaryExpression expression)
        {
            // Evaluates the left expression and saves the value
            expression.LeftExpression.Accept(this);
            dynamic left = Result;

            if (expression.Type == BinaryExpressionType.And && !left)
            {
                Result = false;
                return;
            }

            if (expression.Type == BinaryExpressionType.Or && left)
            {
                Result = true;
                return;
            }

            // Evaluates the right expression and saves the value
            expression.RightExpression.Accept(this);
            dynamic right = Result;
            try
            {
                switch (expression.Type)
                {
                    case BinaryExpressionType.And:
                        Result = left && right;
                        break;

                    case BinaryExpressionType.Or:
                        Result = left || right;
                        break;

                    case BinaryExpressionType.Div:
                        // force integer divisions to double result, otherwise use standard operators
                        if (left is int && right is int)
                        {
                            Result = Convert.ToDouble(left) / right;
                        }
                        else
                        {
                            Result = left / right;
                        }

                        break;

                    case BinaryExpressionType.Equal:
                        Result = left == right;
                        break;

                    case BinaryExpressionType.Greater:
                        Result = left > right;
                        break;

                    case BinaryExpressionType.GreaterOrEqual:
                        Result = left >= right;
                        break;

                    case BinaryExpressionType.Lesser:
                        Result = left < right;
                        break;

                    case BinaryExpressionType.LesserOrEqual:
                        Result = left <= right;
                        break;

                    case BinaryExpressionType.Minus:
                        Result = left - right;
                        break;

                    case BinaryExpressionType.Modulo:
                        Result = left % right;
                        break;

                    case BinaryExpressionType.NotEqual:
                        Result = left != right;
                        break;

                    case BinaryExpressionType.Plus:
                        if ((left is double && right is decimal) ||
                            (left is decimal && right is double))
                        {
                            Result = Convert.ToDouble(left) + Convert.ToDouble(right);
                        }
                        else
                        {
                            Result = left + right;
                        }
                        break;

                    case BinaryExpressionType.Times:
                        Result = left * right;
                        break;

                    case BinaryExpressionType.BitwiseAnd:
                        Result = left & right;
                        break;

                    case BinaryExpressionType.BitwiseOr:
                        Result = left | right;
                        break;

                    case BinaryExpressionType.BitwiseXOr:
                        Result = left ^ right;
                        break;

                    case BinaryExpressionType.LeftShift:
                        Result = left << right;
                        break;

                    case BinaryExpressionType.RightShift:
                        Result = left >> right;
                        break;
                }
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(e.Message);
            }
        }

        public override void Visit(UnaryExpression expression)
        {
            // Recursively evaluates the underlying expression
            expression.Expression.Accept(this);

            switch (expression.Type)
            {
                case UnaryExpressionType.Not:
                    Result = !(dynamic)Result;
                    break;

                case UnaryExpressionType.Negate:
                    Result = 0 - (dynamic)Result;
                    break;

                case UnaryExpressionType.BitwiseNot:
                    Result = ~(dynamic)Result;
                    break;
            }
        }

        public override void Visit(ValueExpression expression)
        {
            Result = expression.Value;
        }

        public override void Visit(FunctionExpression function, Dictionary<string, object> context = null)
        {
            var args = new FunctionArgs
            {
                Parameters = new Expression[function.Expressions.Length]
            };

            // Don't call parameters right now, instead let the function do it as needed.
            // Some parameters shouldn't be called, for instance, in a if(), the "not" value might be a division by zero
            // Evaluating every value could produce unexpected behaviour
            for (int i = 0; i < function.Expressions.Length; i++)
            {
                args.Parameters[i] = new Expression(function.Expressions[i], _options);
                // Assign the parameters of the Expression to the arguments so that custom Functions and Parameters can use them
                args.Parameters[i].Parameters = Parameters;
                args.Parameters[i].EvaluateFunction = EvaluateFunction;
            }

            if (EvaluateFunction != null)
            {
                if (string.IsNullOrEmpty(function.Namespace))
                    EvaluateFunction(function.Identifier.Name, args, context);
                else
                    EvaluateFunction($"{function.Namespace}.{function.Identifier.Name}", args, context);
            }

            // If an external implementation was found get the result back
            if (args.HasResult)
            {
                Result = args.Result;
                return;
            }
        }

        public Action<string, FunctionArgs, Dictionary<string, object>> EvaluateFunction;
        public Func<string, object> TryGetObject;
        public Func<string, Type> TryGetType;

        public override void Visit(IdentifierExpression identifierExpression)
        {
            if (GetObjectOrType(identifierExpression.Name, out object o))
            {
                this.Result = o;
                identifierExpression.IsNamespace = false;
            }
            this.Result = identifierExpression.Name;
            identifierExpression.IsNamespace = true;
        }

        public Dictionary<string, object> Parameters { get; set; }

        private bool GetObjectOrType(string uri, out object result)
        {
            if (this.TryGetObject != null)
            {
                var objTemp = this.TryGetObject(uri);
                if (objTemp != null)
                {
                    result = objTemp;
                    return true;
                }
            }
            if (this.TryGetType != null)
            {
                var typeTemp = this.TryGetType(uri);
                if (typeTemp != null)
                {
                    result = typeTemp;
                    return true;
                }
            }
            result = null;
            return false;
        }

        private bool GetObjectOrType(string baseUri, MemberExpression memberExpression, out object result, out MemberExpression resultMemeberExpression)
        {
            string uri = baseUri;
            if (memberExpression.LeftExpression is IdentifierExpression identifierExpression)
            {
                uri = identifierExpression.Name;
                if (!string.IsNullOrEmpty(baseUri))
                    uri = baseUri + "." + uri;
                if (GetObjectOrType(uri, out result))
                {
                    resultMemeberExpression = memberExpression;
                    return true;
                }
                if (memberExpression.rightExpression is IdentifierExpression identifier)
                {
                    uri = uri + "." + identifier.Name;
                    if (GetObjectOrType(uri, out result))
                    {
                        resultMemeberExpression = memberExpression;
                        return true;
                    }
                }
            }
            else if (memberExpression.LeftExpression is MemberExpression m)
            {
                return GetObjectOrType(baseUri, m, out result, out resultMemeberExpression);
            }

            result = null;
            resultMemeberExpression = null;
            return false;
        }

        public override void Visit(MemberExpression expression, Dictionary<string, object> context = null)
        {
            expression.LeftExpression.Accept(this, context);
            if ((expression.LeftExpression is IdentifierExpression left && left.IsNamespace)
            || (expression.LeftExpression is MemberExpression mem && mem.IsNamespace)
                )
            {
                if (expression.rightExpression is IdentifierExpression identifier)
                {
                    var ns = $"{this.Result}.{ identifier.Name}";
                    if (GetObjectOrType(ns, out object o))
                    {
                        this.Result = o;
                        expression.IsNamespace = false;
                    }
                    else
                    {
                        this.Result = ns;
                        expression.IsNamespace = true;
                    }
                }
                else if (expression.rightExpression is FunctionExpression func)
                {
                    func.Namespace = this.Result.ToString();
                    func.Accept(this, context);
                }
                else
                {
                    throw new Exception($"cannot find object or type:{this.Result}");
                }
            }
            else
            {
                object obj = this.Result;
                Type objType = this.Result as Type;
                if (objType == null)
                {
                    objType = obj.GetType();
                }
                else
                {
                    obj = null;
                }
                if (expression.rightExpression is FunctionExpression right)
                {
                    List<object> pars = new List<object>();
                    List<Type> parTypes = new List<Type>();
                    foreach (var item in right.Expressions)
                    {
                        item.Accept(this);
                        pars.Add(this.Result);
                        parTypes.Add(this.Result.GetType());
                    }
                    if (objType.IsSubclassOf(typeof(DynamicObject)))
                    {
                        // TODO： Support invoke DynamicObject's method
                        //var callSiteBinder = Microsoft.CSharp.RuntimeBinder.Binder.InvokeMember(
                        //    CSharpBinderFlags.None,
                        //    right.Identifier.Name,
                        //    parTypes,
                        //    objType,
                        //     new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) });
                        //var callSite = CallSite<Func<CallSite, object, object, object, object>>.Create(callSiteBinder);
                        //Result = callSite.Target(callSite, obj, 1, 2);
                    }
                    else
                    {
                        MethodInfo mInfo = null;
                        foreach (var method in objType.GetMethods())
                        {
                            if (method.Name != right.Identifier.Name)
                                continue;
                            var p = method.GetParameters();
                            if (p.Length != pars.Count)
                                continue;
                            mInfo = method;
                            break;
                        }
                        if (mInfo == null)
                            throw new Exception(string.Format("Cannot find matched method {0} with parameters:{1}",
                                right.Identifier.Name,
                                pars.Count
                                ));
                        if (pars.Count > 0)

                            Result = mInfo.Invoke(obj, pars.ToArray());
                        else
                            Result = mInfo.Invoke(obj, null);
                    }
                }
                if (expression.rightExpression is IdentifierExpression id)
                {
                    if (objType.IsSubclassOf(typeof(DynamicObject)))
                    {
                        var callSiteBinder = Microsoft.CSharp.RuntimeBinder.Binder.GetMember(
                            CSharpBinderFlags.None,
                            id.Name,
                             objType,
                             new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) });
                        var callSite = CallSite<Func<CallSite, object, object>>.Create(callSiteBinder);
                        Result = callSite.Target(callSite, obj);
                    }
                    else
                    {
                        var property = objType.GetProperty(id.Name);
                        if (property != null)
                            Result = property.GetValue(obj);
                        else
                        {
                            var field = objType.GetField(id.Name);
                            if (field != null)
                                Result = field.GetValue(obj);
                        }
                    }
                }
            }
        }
    }
}