using System;
using System.Collections.Generic;

namespace maskx.Expression
{
    public class FunctionArgs
    {
        private object result;

        public object Result
        {
            get { return result; }
            set
            {
                result = value;
                HasResult = true;
            }
        }

        public bool HasResult { get; set; }

        private Expression[] parameters = new Expression[0];

        public Expression[] Parameters
        {
            get { return parameters; }
            set { parameters = value; }
        }

        public object[] EvaluateParameters(Dictionary<string, object> context)
        {
            var values = new object[parameters.Length];
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = parameters[i].Evaluate(context);
            }

            return values;
        }
    }
}