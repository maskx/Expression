using Antlr4.Runtime;
using System.Collections.Generic;

namespace maskx.Expression
{
    public class ErrorListener : IAntlrErrorListener<IToken>
    {
        public readonly List<string> Errors = new List<string>();

        public void SyntaxError(IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg,
            RecognitionException e)
        {
            Errors.Add(string.Format("{0} at {1}:{2}", msg, line, charPositionInLine));
        }
    }
}