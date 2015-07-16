using SData.Internal;
using System;

namespace SData.Compiler {
    internal sealed class ParsingContext : LoadingContext {
        [ThreadStatic]
        public static ParsingContext Current;
        private static void Error(DiagMsgEx diagMsg, TextSpan textSpan) {
            Current.AddDiagnostic(DiagnosticSeverity.Error, (int)diagMsg.Code, diagMsg.GetMessage(), textSpan);
        }
        public static void ErrorAndThrow(DiagMsgEx diagMsg, TextSpan textSpan) {
            Error(diagMsg, textSpan);
            throw LoadingException.Instance;
        }

    }
}
