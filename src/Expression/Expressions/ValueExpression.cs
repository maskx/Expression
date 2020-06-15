using maskx.Expression.Visitors;
using System;
using System.Collections.Generic;

namespace maskx.Expression.Expressions
{
    public class ValueExpression : LogicalExpression
    {
        public ValueExpression(object value, ValueType type)
        {
            Value = value;
            Type = type;
        }

        public ValueExpression(object value)
        {
            switch (System.Type.GetTypeCode(value.GetType()))
            {
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    Type = ValueType.Float;
                    break;

                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    Type = ValueType.Integer;
                    break;

                case TypeCode.String:
                    Type = ValueType.String;
                    break;

                default:
                    throw new EvaluationException("This value could not be handled: " + value);
            }

            Value = value;
        }

        public ValueExpression(string value)
        {
            Value = value;
            Type = ValueType.String;
        }

        public ValueExpression(int value)
        {
            Value = value;
            Type = ValueType.Integer;
        }

        public object Value { get; set; }

        public ValueType Type { get; set; }

        public override void Accept(LogicalExpressionVisitor visitor, Dictionary<string, object> context = null)
        {
            visitor.Visit(this,context);
        }
    }

    public enum ValueType
    {
        Integer,
        String,
        Float
    }
}