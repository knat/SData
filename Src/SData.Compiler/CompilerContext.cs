using System;
using SData.Internal;

namespace SData.Compiler
{
    internal sealed class CompilerContext : LoadingContext
    {
        [ThreadStatic]
        public static CompilerContext Current;
        private static void Error(DiagMsgEx diagMsg, TextSpan textSpan)
        {
            Current.AddDiagnostic(DiagnosticSeverity.Error, (int)diagMsg.Code, diagMsg.GetMessage(), textSpan);
        }
        public static void ErrorAndThrow(DiagMsgEx diagMsg, TextSpan textSpan)
        {
            Error(diagMsg, textSpan);
            throw LoadingException.Instance;
        }

    }
}
