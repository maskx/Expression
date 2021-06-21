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

        public Expression[] Parameters { get; set; } = new Expression[0];

        public object[] EvaluateParameters(Dictionary<string, object> context)
        {
            var values = new object[Parameters.Length];
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = Parameters[i].Evaluate(context);
            }

            return values;
        }
    }
}