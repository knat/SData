using System.Diagnostics;
using System.IO;

namespace SData.Internal {
    public abstract class ParserBase {
        protected ParserBase() {
        }
        protected void Set(string filePath, TextReader reader, LoadingContext context) {
            _lexer = Lexer.Get(filePath, reader, context);
            _context = context;
            _token = null;
        }
        protected void Clear() {
            if (_lexer != null) {
                _lexer.Clear();
            }
            _context = null;
        }
        private Lexer _lexer;
        protected LoadingContext _context;
        private Token? _token;
        //
        //
        protected void Error(int code, string errMsg, TextSpan textSpan) {
            _context.AddDiagnostic(DiagnosticSeverity.Error, code, errMsg, textSpan);
        }
        protected void Error(DiagMsg diagMsg, TextSpan textSpan) {
            Error((int)diagMsg.Code, diagMsg.GetMessage(), textSpan);
        }
        protected void Throw() {
            throw LoadingException.Instance;
        }
        protected void ErrorAndThrow(string errMsg, TextSpan textSpan) {
            Error((int)DiagnosticCode.Parsing, errMsg, textSpan);
            Throw();
        }
        protected void ErrorAndThrow(string errMsg) {
            ErrorAndThrow(errMsg, GetToken().TextSpan);
        }
        protected void ErrorAndThrow(DiagMsg diagMsg, TextSpan textSpan) {
            Error(diagMsg, textSpan);
            Throw();
        }
        protected void ErrorAndThrow(DiagMsg diagMsg) {
            ErrorAndThrow(diagMsg, GetToken().TextSpan);
        }
        protected Token GetToken() {
            return (_token ?? (_token = _lexer.GetToken())).Value;
        }
        protected void ConsumeToken() {
            Debug.Assert(_token != null);
            _token = null;
        }
        protected bool PeekToken(int kind) {
            return GetToken().Kind == kind;
        }
        protected bool PeekToken(int kind1, int kind2) {
            var kind = GetToken().Kind;
            return kind == kind1 || kind == kind2;
        }
        protected bool PeekToken(int kind1, int kind2, int kind3) {
            var kind = GetToken().Kind;
            return kind == kind1 || kind == kind2 || kind == kind3;
        }
        protected bool PeekToken(int kind1, int kind2, int kind3, int kind4) {
            var kind = GetToken().Kind;
            return kind == kind1 || kind == kind2 || kind == kind3 || kind == kind4;
        }

        protected bool Token(int kind) {
            if (GetToken().Kind == kind) {
                ConsumeToken();
                return true;
            }
            return false;
        }
        protected bool Token(int kind, out Token result) {
            result = GetToken();
            if (result.Kind == kind) {
                ConsumeToken();
                return true;
            }
            result = default(Token);
            return false;
        }
        protected Token TokenExpected(char ch) {
            Token result;
            if (!Token(ch, out result)) {
                ErrorAndThrow(ch.ToString() + " expected.");
            }
            return result;
        }
        protected Token TokenExpected(int kind, string errMsg) {
            Token result;
            if (!Token(kind, out result)) {
                ErrorAndThrow(errMsg);
            }
            return result;
        }
        protected void EndOfFileExpected() {
            TokenExpected(char.MaxValue, "End-of-file expected.");
        }

        protected bool Name(out Token result) {
            result = GetToken();
            if (result.IsName) {
                ConsumeToken();
                return true;
            }
            result = default(Token);
            return false;
        }
        protected Token NameExpected() {
            Token result;
            if (!Name(out result)) {
                ErrorAndThrow("Name expected.");
            }
            return result;
        }
        protected bool Keyword(string value) {
            if (GetToken().IsKeyword(value)) {
                ConsumeToken();
                return true;
            }
            return false;
        }
        protected bool Keyword(string value, out Token result) {
            result = GetToken();
            if (result.IsKeyword(value)) {
                ConsumeToken();
                return true;
            }
            result = default(Token);
            return false;
        }
        protected Token KeywordExpected(string value) {
            Token result;
            if (!Keyword(value, out result)) {
                ErrorAndThrow(value + " expected.");
            }
            return result;
        }
        protected bool String(out Token result) {
            result = GetToken();
            if (result.IsString) {
                ConsumeToken();
                return true;
            }
            result = default(Token);
            return false;
        }
        protected Token StringExpected() {
            Token result;
            if (!String(out result)) {
                ErrorAndThrow("String expected.");
            }
            return result;
        }
        protected bool Null(out Token result) {
            result = GetToken();
            if (result.IsNull) {
                ConsumeToken();
                return true;
            }
            result = default(Token);
            return false;
        }
        protected bool AtomValue(out Token result) {
            result = GetToken();
            if (result.IsAtomValue) {
                ConsumeToken();
                return true;
            }
            result = default(Token);
            return false;
        }
        protected Token AtomValueExpected() {
            Token result;
            if (!AtomValue(out result)) {
                ErrorAndThrow("Atom value expected.");
            }
            return result;
        }
    }

}
