﻿using maskx.Expression.Expressions;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using ValueType = maskx.Expression.Expressions.ValueType;

namespace maskx.Expression.Visitors
{
    public class SerializationVisitor : LogicalExpressionVisitor
    {
        private readonly NumberFormatInfo numberFormatInfo;

        public SerializationVisitor()
        {
            Result = new StringBuilder();
            numberFormatInfo = new NumberFormatInfo { NumberDecimalSeparator = "." };
        }

        public StringBuilder Result { get; protected set; }

        public override void Visit(TernaryExpression expression, Dictionary<string, object> context = null)
        {
            EncapsulateNoValue(expression.LeftExpression);

            Result.Append("? ");

            EncapsulateNoValue(expression.MiddleExpression);

            Result.Append(": ");

            EncapsulateNoValue(expression.RightExpression);
        }

        public override void Visit(BinaryExpression expression, Dictionary<string, object> context = null)
        {
            EncapsulateNoValue(expression.LeftExpression);

            switch (expression.Type)
            {
                case BinaryExpressionType.And:
                    Result.Append("and ");
                    break;

                case BinaryExpressionType.Or:
                    Result.Append("or ");
                    break;

                case BinaryExpressionType.Div:
                    Result.Append("/ ");
                    break;

                case BinaryExpressionType.Equal:
                    Result.Append("= ");
                    break;

                case BinaryExpressionType.Greater:
                    Result.Append("> ");
                    break;

                case BinaryExpressionType.GreaterOrEqual:
                    Result.Append(">= ");
                    break;

                case BinaryExpressionType.Lesser:
                    Result.Append("< ");
                    break;

                case BinaryExpressionType.LesserOrEqual:
                    Result.Append("<= ");
                    break;

                case BinaryExpressionType.Minus:
                    Result.Append("- ");
                    break;

                case BinaryExpressionType.Modulo:
                    Result.Append("% ");
                    break;

                case BinaryExpressionType.NotEqual:
                    Result.Append("!= ");
                    break;

                case BinaryExpressionType.Plus:
                    Result.Append("+ ");
                    break;

                case BinaryExpressionType.Times:
                    Result.Append("* ");
                    break;

                case BinaryExpressionType.BitwiseAnd:
                    Result.Append("& ");
                    break;

                case BinaryExpressionType.BitwiseOr:
                    Result.Append("| ");
                    break;

                case BinaryExpressionType.BitwiseXOr:
                    Result.Append("~ ");
                    break;

                case BinaryExpressionType.LeftShift:
                    Result.Append("<< ");
                    break;

                case BinaryExpressionType.RightShift:
                    Result.Append(">> ");
                    break;
            }

            EncapsulateNoValue(expression.RightExpression);
        }

        public override void Visit(UnaryExpression expression, Dictionary<string, object> context = null)
        {
            switch (expression.Type)
            {
                case UnaryExpressionType.Not:
                    Result.Append("!");
                    break;

                case UnaryExpressionType.Negate:
                    Result.Append("-");
                    break;

                case UnaryExpressionType.BitwiseNot:
                    Result.Append("~");
                    break;
            }

            EncapsulateNoValue(expression.Expression);
        }

        public override void Visit(ValueExpression expression, Dictionary<string, object> context = null)
        {
            switch (expression.Type)
            {
                case ValueType.Float:
                    Result.Append(decimal.Parse(expression.Value.ToString()).ToString(numberFormatInfo)).Append(" ");
                    break;

                case ValueType.Integer:
                    Result.Append(expression.Value).Append(" ");
                    break;

                case ValueType.String:
                    Result.Append("\"").Append(expression.Value).Append("\"").Append(" ");
                    break;
            }
        }

        public override void Visit(FunctionExpression function, Dictionary<string, object> context)
        {
            Result.Append(function.Identifier.Name);

            Result.Append("(");

            for (int i = 0; i < function.Expressions.Length; i++)
            {
                function.Expressions[i].Accept(this);
                if (i < function.Expressions.Length - 1)
                {
                    Result.Remove(Result.Length - 1, 1);
                    Result.Append(", ");
                }
            }

            // trim spaces before adding a closing paren
            while (Result[Result.Length - 1] == ' ')
                Result.Remove(Result.Length - 1, 1);

            Result.Append(") ");
        }

        public override void Visit(IdentifierExpression parameter, Dictionary<string, object> context = null)
        {
            Result.Append("[").Append(parameter.Name).Append("] ");
        }

        protected void EncapsulateNoValue(LogicalExpression expression, Dictionary<string, object> context = null)
        {
            if (expression is ValueExpression)
            {
                expression.Accept(this);
            }
            else
            {
                Result.Append("(");
                expression.Accept(this);

                // trim spaces before adding a closing paren
                while (Result[Result.Length - 1] == ' ')
                    Result.Remove(Result.Length - 1, 1);

                Result.Append(") ");
            }
        }

        public override void Visit(MemberExpression property, Dictionary<string, object> context = null)
        {
            throw new System.NotImplementedException();
        }

        public override void Visit(IndexerExpression expression, Dictionary<string, object> context = null)
        {
            throw new System.NotImplementedException();
        }
    }
}