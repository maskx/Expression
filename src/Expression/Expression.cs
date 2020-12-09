using Antlr4.Runtime;
using maskx.Expression.Expressions;
using maskx.Expression.Visitors;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace maskx.Expression
{
    public class Expression
    {
        public EvaluateOptions Options { get; set; }

        /// <summary>
        /// Textual representation of the expression to evaluate.
        /// </summary>
        protected string OriginalExpression;

        public Expression(string expression)
            : this(expression, EvaluateOptions.None)
        {
        }

        public Expression(string expression, EvaluateOptions options)
        {
            if (string.IsNullOrEmpty(expression))
                throw new
                    ArgumentException("Expression can't be empty", "expression");

            OriginalExpression = expression;
            Options = options;
        }

        public Expression(LogicalExpression expression, EvaluateOptions options)
        {
            _ParsedExpression = expression ?? throw new
                    ArgumentException("Expression can't be null", "expression");
            Options = options;
        }

        #region Cache management

        private static bool _cacheEnabled = true;
        private static Dictionary<string, WeakReference> _compiledExpressions = new Dictionary<string, WeakReference>();
        private static readonly ReaderWriterLock Rwl = new ReaderWriterLock();

        public static bool CacheEnabled
        {
            get { return _cacheEnabled; }
            set
            {
                _cacheEnabled = value;

                if (!CacheEnabled)
                {
                    // Clears cache
                    _compiledExpressions = new Dictionary<string, WeakReference>();
                }
            }
        }

        /// <summary>
        /// Removed unused entries from cached compiled expression
        /// </summary>
        private static void CleanCache()
        {
            var keysToRemove = new List<string>();

            try
            {
                Rwl.AcquireWriterLock(Timeout.Infinite);
                foreach (var de in _compiledExpressions)
                {
                    if (!de.Value.IsAlive)
                    {
                        keysToRemove.Add(de.Key);
                    }
                }

                foreach (string key in keysToRemove)
                {
                    _compiledExpressions.Remove(key);
                    Trace.TraceInformation("Cache entry released: " + key);
                }
            }
            finally
            {
                Rwl.ReleaseReaderLock();
            }
        }

        #endregion Cache management

        public static LogicalExpression Compile(string expression, bool nocache)
        {
            LogicalExpression logicalExpression = null;

            if (_cacheEnabled && !nocache)
            {
                try
                {
                    Rwl.AcquireReaderLock(Timeout.Infinite);

                    if (_compiledExpressions.ContainsKey(expression))
                    {
                        Trace.TraceInformation("Expression retrieved from cache: " + expression);
                        var wr = _compiledExpressions[expression];
                        logicalExpression = wr.Target as LogicalExpression;

                        if (wr.IsAlive && logicalExpression != null)
                        {
                            return logicalExpression;
                        }
                    }
                }
                finally
                {
                    Rwl.ReleaseReaderLock();
                }
            }

            if (logicalExpression == null)
            {
                var lexer = new ExpressionLexer(new AntlrInputStream(expression));
                var parser = new ExpressionParser(new CommonTokenStream(lexer));
                var errorListener = new ErrorListener();
                parser.AddErrorListener(errorListener);
                logicalExpression = parser.run().retValue;

                if (errorListener.Errors.Any())
                {
                    throw new EvaluationException(string.Join(Environment.NewLine, errorListener.Errors.ToArray()));
                }

                if (_cacheEnabled && !nocache)
                {
                    try
                    {
                        Rwl.AcquireWriterLock(Timeout.Infinite);
                        _compiledExpressions[expression] = new WeakReference(logicalExpression);
                    }
                    finally
                    {
                        Rwl.ReleaseWriterLock();
                    }

                    CleanCache();

                    Trace.TraceInformation("Expression added to cache: " + expression);
                }
            }

            return logicalExpression;
        }

        /// <summary>
        /// Pre-compiles the expression in order to check syntax errors.
        /// If errors are detected, the Error property contains the message.
        /// </summary>
        /// <returns>True if the expression syntax is correct, otherwiser False</returns>
        public bool HasErrors()
        {
            try
            {
                if (_ParsedExpression == null)
                {
                    _ParsedExpression = Compile(OriginalExpression, (Options & EvaluateOptions.NoCache) == EvaluateOptions.NoCache);
                }

                // In case HasErrors() is called multiple times for the same expression
                return ParsedExpression != null && Error != null;
            }
            catch (Exception e)
            {
                Error = e.Message;
                return true;
            }
        }

        public string Error { get; private set; }
        private LogicalExpression _ParsedExpression;

        public LogicalExpression ParsedExpression
        {
            get
            {
                if (_ParsedExpression == null)
                {
                    if (HasErrors())
                    {
                        throw new EvaluationException(Error);
                    }
                }
                return _ParsedExpression;
            }
        }

        private EvaluationVisitor _EvaluationVisitor;

        public EvaluationVisitor EvaluationVisitor
        {
            get
            {
                if (_EvaluationVisitor == null)
                {
                    _EvaluationVisitor = new EvaluationVisitor(Options)
                    {
                        EvaluateFunction = this.EvaluateFunction,
                        Parameters = this.Parameters,
                        TryGetType = this.TryGetType,
                        TryGetObject = this.TryGetObject
                    };
                }
                return _EvaluationVisitor;
            }
            set
            {
                _EvaluationVisitor = value;
            }
        }

        protected Dictionary<string, IEnumerator> ParameterEnumerators;
        protected Dictionary<string, object> ParametersBackup;

        public object Evaluate(Dictionary<string, object> context = null)
        {
            // if array evaluation, execute the same expression multiple times
            if ((Options & EvaluateOptions.IterateParameters) == EvaluateOptions.IterateParameters)
            {
                int size = -1;
                ParametersBackup = new Dictionary<string, object>();
                foreach (string key in Parameters.Keys)
                {
                    ParametersBackup.Add(key, Parameters[key]);
                }

                ParameterEnumerators = new Dictionary<string, IEnumerator>();

                foreach (object parameter in Parameters.Values)
                {
                    if (parameter is IEnumerable enumerable)
                    {
                        int localsize = 0;
                        foreach (object o in enumerable)
                        {
                            localsize++;
                        }

                        if (size == -1)
                        {
                            size = localsize;
                        }
                        else if (localsize != size)
                        {
                            throw new EvaluationException("When IterateParameters option is used, IEnumerable parameters must have the same number of items");
                        }
                    }
                }

                foreach (string key in Parameters.Keys)
                {
                    if (Parameters[key] is IEnumerable parameter)
                    {
                        ParameterEnumerators.Add(key, parameter.GetEnumerator());
                    }
                }

                var results = new List<object>();
                for (int i = 0; i < size; i++)
                {
                    foreach (string key in ParameterEnumerators.Keys)
                    {
                        IEnumerator enumerator = ParameterEnumerators[key];
                        enumerator.MoveNext();
                        Parameters[key] = enumerator.Current;
                    }

                    ParsedExpression.Accept(EvaluationVisitor);
                    results.Add(EvaluationVisitor.Result);
                }

                return results;
            }

            ParsedExpression.Accept(EvaluationVisitor, context);
            return EvaluationVisitor.Result;
        }

        public Action<string, FunctionArgs, Dictionary<string, object>> EvaluateFunction { get; set; }
        public Func<string, object> TryGetObject { get; set; }
        public Func<string, Type> TryGetType { get; set; }

        private Dictionary<string, object> _parameters;

        public Dictionary<string, object> Parameters
        {
            get { return _parameters ?? (_parameters = new Dictionary<string, object>()); }
            set { _parameters = value; }
        }
    }
}