using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace SData.Internal {
    public sealed class Lexer {
        [ThreadStatic]
        private static Lexer _instance;
        private static Lexer Instance {
            get { return _instance ?? (_instance = new Lexer()); }
        }
        public static Lexer Get(string filePath, TextReader reader, LoadingContext context) {
            return Instance.Init(filePath, reader, context);
        }
        private Lexer() {
            _buf = new char[_bufLength];
        }
        //inputs
        private string _filePath;
        private TextReader _reader;
        private LoadingContext _context;
        //
        private const int _bufLength = 1024;
        private readonly char[] _buf;
        private int _index, _count;
        private bool _isEOF;
        private int _totalIndex;
        private int _lastLine, _lastColumn, _line, _column;
        private const int _stringBuilderCapacity = 256;
        private StringBuilder _stringBuilder;
        //
        private Lexer Init(string filePath, TextReader reader, LoadingContext context) {
            if (filePath == null) throw new ArgumentNullException("filePath");
            if (reader == null) throw new ArgumentNullException("reader");
            if (context == null) throw new ArgumentNullException("context");
            _filePath = filePath;
            _reader = reader;
            _context = context;
            _index = _count = 0;
            _isEOF = false;
            _totalIndex = 0;
            _lastLine = _lastColumn = _line = _column = 1;
            if (_stringBuilder == null) {
                _stringBuilder = new StringBuilder(_stringBuilderCapacity);
            }
            return this;
        }
        public void Clear() {
            _filePath = null;
            _reader = null;
            _context = null;
            if (_stringBuilder != null && _stringBuilder.Capacity > _stringBuilderCapacity * 8) {
                _stringBuilder = null;
            }
        }
        private StringBuilder GetStringBuilder() {
            return _stringBuilder.Clear();
        }
        private char GetChar(int offset = 0) {
            var pos = _index + offset;
            if (pos < _count) {
                return _buf[pos];
            }
            if (_isEOF) {
                return char.MaxValue;
            }
            var remainCount = _count - _index;
            if (remainCount > 0) {
                for (var i = 0; i < remainCount; ++i) {
                    _buf[i] = _buf[_index + i];
                }
            }
            var retCount = _reader.Read(_buf, remainCount, _bufLength - remainCount);
            if (retCount == 0) {
                _isEOF = true;
            }
            _index = 0;
            _count = remainCount + retCount;
            return GetChar(offset);
        }
        private char GetNextChar() {
            return GetChar(1);
        }
        private char GetNextNextChar() {
            return GetChar(2);
        }
        private void AdvanceChar(bool checkNewLine) {
            _lastLine = _line;
            _lastColumn = _column;
            if (_index < _count) {
                if (checkNewLine) {
                    var ch = _buf[_index++];
                    ++_totalIndex;
                    if (IsNewLine(ch)) {
                        if (ch == '\r' && GetChar() == '\n') {
                            ++_index;
                            ++_totalIndex;
                        }
                        ++_line;
                        _column = 1;
                    }
                    else {
                        ++_column;
                    }
                }
                else {
                    ++_index;
                    ++_totalIndex;
                    ++_column;
                }
            }
        }
        private int _tokenStartIndex;
        private TextPosition _tokenStartPosition;
        private void MarkTokenStart() {
            _tokenStartIndex = _totalIndex;
            _tokenStartPosition = new TextPosition(_line, _column);
        }
        private Token CreateToken(TokenKind tokenKind, string value) {
            var startIndex = _tokenStartIndex;
            return new Token((int)tokenKind, value, new TextSpan(_filePath, startIndex, _totalIndex - startIndex,
                _tokenStartPosition, new TextPosition(_lastLine, _lastColumn)));
        }
        private TextSpan CreateTextSpan() {
            var pos = new TextPosition(_line, _column);
            return new TextSpan(_filePath, _totalIndex, _index < _count ? 1 : 0, pos, pos);
        }
        private Token CreateTokenAndAdvanceChar(char ch) {
            var token = new Token(ch, null, CreateTextSpan());
            AdvanceChar(false);
            return token;
        }
        private void ErrorAndThrow(string errMsg, TextSpan textSpan) {
            _context.AddDiagnostic(DiagnosticSeverity.Error, (int)DiagnosticCode.Parsing, errMsg, textSpan);
            throw LoadingException.Instance;
        }
        private void ErrorAndThrow(string errMsg) {
            ErrorAndThrow(errMsg, CreateTextSpan());
        }
        private enum State : byte {
            None = 0,
            //InWhitespaceOrNewLine,
            InSingleLineComment,
            InDelimitedComment,
            InNormalName,
            InVerbatimName,
            InNormalString,
            InVerbatimString,
            InChar,
            InNumberInteger,
            InNumberFraction,
            InNumberExponent,
        }
        public Token GetToken() {
            var state = State.None;
            StringBuilder sb = null;
            while (true) {
                var ch = GetChar();
                switch (state) {
                    case State.None:
                        switch (ch) {
                            case char.MaxValue:
                                return CreateTokenAndAdvanceChar(ch);
                            case '/':
                                {
                                    var nextch = GetNextChar();
                                    if (nextch == '/') {
                                        state = State.InSingleLineComment;
                                        MarkTokenStart();
                                        AdvanceChar(false);
                                        AdvanceChar(false);
                                    }
                                    else if (nextch == '*') {
                                        state = State.InDelimitedComment;
                                        MarkTokenStart();
                                        AdvanceChar(false);
                                        AdvanceChar(false);
                                    }
                                    else {
                                        return CreateTokenAndAdvanceChar(ch);
                                    }
                                }
                                break;
                            case '@':
                                {
                                    var nextch = GetNextChar();
                                    if (nextch == '"') {
                                        state = State.InVerbatimString;
                                        MarkTokenStart();
                                        AdvanceChar(false);
                                        AdvanceChar(false);
                                        sb = GetStringBuilder();
                                    }
                                    else if (IsIdentifierStartCharacter(nextch)) {
                                        state = State.InVerbatimName;
                                        MarkTokenStart();
                                        AdvanceChar(false);
                                        AdvanceChar(false);
                                        sb = GetStringBuilder();
                                        sb.Append(nextch);
                                    }
                                    else {
                                        return CreateTokenAndAdvanceChar(ch);
                                    }
                                }
                                break;
                            case '"':
                                {
                                    state = State.InNormalString;
                                    MarkTokenStart();
                                    AdvanceChar(false);
                                    sb = GetStringBuilder();
                                }
                                break;
                            case '\'':
                                {
                                    state = State.InChar;
                                    MarkTokenStart();
                                    AdvanceChar(false);
                                    sb = GetStringBuilder();
                                }
                                break;
                            case '-':
                            case '+':
                                {
                                    var nextch = GetNextChar();
                                    if (IsDecDigit(nextch)) {
                                        state = State.InNumberInteger;
                                        MarkTokenStart();
                                        AdvanceChar(false);
                                        AdvanceChar(false);
                                        sb = GetStringBuilder();
                                        sb.Append(ch);
                                        sb.Append(nextch);
                                    }
                                    else if (nextch == '.') {
                                        var nextnextch = GetNextNextChar();
                                        if (IsDecDigit(nextnextch)) {
                                            state = State.InNumberFraction;
                                            MarkTokenStart();
                                            AdvanceChar(false);
                                            AdvanceChar(false);
                                            AdvanceChar(false);
                                            sb = GetStringBuilder();
                                            sb.Append(ch);
                                            sb.Append(nextch);
                                            sb.Append(nextnextch);
                                        }
                                        else {
                                            return CreateTokenAndAdvanceChar(ch);
                                        }
                                    }
                                    else {
                                        return CreateTokenAndAdvanceChar(ch);
                                    }
                                }
                                break;
                            case '.':
                                {
                                    var nextch = GetNextChar();
                                    if (IsDecDigit(nextch)) {
                                        state = State.InNumberFraction;
                                        MarkTokenStart();
                                        AdvanceChar(false);
                                        AdvanceChar(false);
                                        sb = GetStringBuilder();
                                        sb.Append(ch);
                                        sb.Append(nextch);
                                    }
                                    else {
                                        return CreateTokenAndAdvanceChar(ch);
                                    }
                                }
                                break;
                            case ':':
                                {
                                    var nextch = GetNextChar();
                                    if (nextch == ':') {
                                        MarkTokenStart();
                                        AdvanceChar(false);
                                        AdvanceChar(false);
                                        return CreateToken(TokenKind.ColonColon, null);
                                    }
                                    else {
                                        return CreateTokenAndAdvanceChar(ch);
                                    }
                                }
                            case '#':
                                {
                                    var nextch = GetNextChar();
                                    if (nextch == '[') {
                                        MarkTokenStart();
                                        AdvanceChar(false);
                                        AdvanceChar(false);
                                        return CreateToken(TokenKind.HashOpenBracket, null);
                                    }
                                    else {
                                        return CreateTokenAndAdvanceChar(ch);
                                    }
                                }
                            default:
                                if (IsWhitespace(ch)) {
                                    AdvanceChar(false);
                                }
                                else if (IsNewLine(ch)) {
                                    AdvanceChar(true);
                                }
                                else if (IsIdentifierStartCharacter(ch)) {
                                    state = State.InNormalName;
                                    MarkTokenStart();
                                    AdvanceChar(false);
                                    sb = GetStringBuilder();
                                    sb.Append(ch);
                                }
                                else if (IsDecDigit(ch)) {
                                    state = State.InNumberInteger;
                                    MarkTokenStart();
                                    AdvanceChar(false);
                                    sb = GetStringBuilder();
                                    sb.Append(ch);
                                }
                                else {
                                    return CreateTokenAndAdvanceChar(ch);
                                }
                                break;
                        }
                        break;

                    case State.InSingleLineComment:
                        if (IsNewLine(ch) || ch == char.MaxValue) {
                            state = State.None;
                        }
                        else {
                            AdvanceChar(false);
                        }
                        break;
                    case State.InDelimitedComment:
                        if (ch == '*') {
                            AdvanceChar(false);
                            ch = GetChar();
                            if (ch == '/') {
                                AdvanceChar(false);
                                state = State.None;
                            }
                        }
                        else if (ch == char.MaxValue) {
                            ErrorAndThrow("*/ expected.");
                        }
                        else {
                            AdvanceChar(true);
                        }
                        break;
                    case State.InNormalName:
                    case State.InVerbatimName:
                        if (IsIdentifierPartCharacter(ch)) {
                            sb.Append(ch);
                            AdvanceChar(false);
                        }
                        else {
                            return CreateToken(state == State.InNormalName ? TokenKind.NormalName : TokenKind.VerbatimName, sb.ToString());
                        }
                        break;
                    case State.InNormalString:
                        if (ch == '\\') {
                            AdvanceChar(false);
                            ProcessCharEscSeq(sb);
                        }
                        else if (ch == '"') {
                            AdvanceChar(false);
                            return CreateToken(TokenKind.NormalString, sb.ToString());
                        }
                        else if (IsNewLine(ch) || ch == char.MaxValue) {
                            ErrorAndThrow("\" expected.");
                        }
                        else {
                            sb.Append(ch);
                            AdvanceChar(false);
                        }
                        break;
                    case State.InVerbatimString:
                        if (ch == '"') {
                            AdvanceChar(false);
                            ch = GetChar();
                            if (ch == '"') {
                                sb.Append('"');
                                AdvanceChar(false);
                            }
                            else {
                                return CreateToken(TokenKind.VerbatimString, sb.ToString());
                            }
                        }
                        else if (ch == char.MaxValue) {
                            ErrorAndThrow("\" expected.");
                        }
                        else {
                            sb.Append(ch);
                            AdvanceChar(true);
                        }
                        break;
                    case State.InChar:
                        if (sb.Length == 0) {
                            if (ch == '\\') {
                                AdvanceChar(false);
                                ProcessCharEscSeq(sb);
                            }
                            else if (ch == '\'' || IsNewLine(ch) || ch == char.MaxValue) {
                                ErrorAndThrow("Character expected.");
                            }
                            else {
                                sb.Append(ch);
                                AdvanceChar(false);
                            }
                        }
                        else if (ch == '\'') {
                            AdvanceChar(false);
                            return CreateToken(TokenKind.Char, sb.ToString());
                        }
                        else {
                            ErrorAndThrow("' expected.");
                        }
                        break;
                    case State.InNumberInteger:
                        if (IsDecDigit(ch)) {
                            sb.Append(ch);
                            AdvanceChar(false);
                        }
                        else if (ch == '.') {
                            var nextch = GetNextChar();
                            if (IsDecDigit(nextch)) {
                                state = State.InNumberFraction;
                                sb.Append(ch);
                                sb.Append(nextch);
                                AdvanceChar(false);
                                AdvanceChar(false);
                            }
                            else {
                                return CreateToken(TokenKind.Integer, sb.ToString());
                            }
                        }
                        else if (ch == 'E' || ch == 'e') {
                            sb.Append(ch);
                            AdvanceChar(false);
                            ch = GetChar();
                            if (ch == '+' || ch == '-') {
                                sb.Append(ch);
                                AdvanceChar(false);
                                ch = GetChar();
                            }
                            if (IsDecDigit(ch)) {
                                state = State.InNumberExponent;
                                sb.Append(ch);
                                AdvanceChar(false);
                            }
                            else {
                                ErrorAndThrow("Decimal digit expected.");
                            }
                        }
                        else {
                            return CreateToken(TokenKind.Integer, sb.ToString());
                        }
                        break;
                    case State.InNumberFraction:
                        if (IsDecDigit(ch)) {
                            sb.Append(ch);
                            AdvanceChar(false);
                        }
                        else if (ch == 'E' || ch == 'e') {
                            sb.Append(ch);
                            AdvanceChar(false);
                            ch = GetChar();
                            if (ch == '+' || ch == '-') {
                                sb.Append(ch);
                                AdvanceChar(false);
                                ch = GetChar();
                            }
                            if (IsDecDigit(ch)) {
                                state = State.InNumberExponent;
                                sb.Append(ch);
                                AdvanceChar(false);
                            }
                            else {
                                ErrorAndThrow("Decimal digit expected.");
                            }
                        }
                        else {
                            return CreateToken(TokenKind.Decimal, sb.ToString());
                        }
                        break;
                    case State.InNumberExponent:
                        if (IsDecDigit(ch)) {
                            sb.Append(ch);
                            AdvanceChar(false);
                        }
                        else {
                            return CreateToken(TokenKind.Real, sb.ToString());
                        }
                        break;
                    default:
                        throw new InvalidOperationException("Invalid stateKind: " + state);
                }
            }
        }
        private void ProcessCharEscSeq(StringBuilder sb) {
            var ch = GetChar();
            switch (ch) {
                case 'u':
                    {
                        AdvanceChar(false);
                        int value = 0;
                        for (var i = 0; i < 4; ++i) {
                            ch = GetChar();
                            if (IsHexDigit(ch)) {
                                value <<= 4;
                                value |= HexValue(ch);
                                AdvanceChar(false);
                            }
                            else {
                                ErrorAndThrow("Invalid character escape sequence.");
                            }
                        }
                        sb.Append((char)value);
                    }
                    return;
                case '\'': sb.Append('\''); break;
                case '"': sb.Append('"'); break;
                case '\\': sb.Append('\\'); break;
                case '0': sb.Append('\0'); break;
                case 'a': sb.Append('\a'); break;
                case 'b': sb.Append('\b'); break;
                case 'f': sb.Append('\f'); break;
                case 'n': sb.Append('\n'); break;
                case 'r': sb.Append('\r'); break;
                case 't': sb.Append('\t'); break;
                case 'v': sb.Append('\v'); break;
                default: ErrorAndThrow("Invalid character escape sequence."); break;
            }
            AdvanceChar(false);
        }

        #region helpers
        private static bool IsNewLine(char ch) {
            return ch == '\r'
                || ch == '\n'
                || ch == '\u0085'
                || ch == '\u2028'
                || ch == '\u2029';
        }
        private static bool IsWhitespace(char ch) {
            return ch == ' '
                || ch == '\t'
                || ch == '\v'
                || ch == '\f'
                || ch == '\u00A0'
                || ch == '\uFEFF'
                || ch == '\u001A'
                || (ch > 255 && CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.SpaceSeparator);
        }
        private static bool IsDecDigit(char ch) {
            return ch >= '0' && ch <= '9';
        }
        private static bool IsHexDigit(char ch) {
            return (ch >= '0' && ch <= '9') ||
                   (ch >= 'A' && ch <= 'F') ||
                   (ch >= 'a' && ch <= 'f');
        }
        private static int DecValue(char ch) {
            return ch - '0';
        }
        private static int HexValue(char ch) {
            return (ch >= '0' && ch <= '9') ? ch - '0' : (ch & 0xdf) - 'A' + 10;
        }

        public static bool IsIdentifierStartCharacter(char ch) {
            // identifier-start-character:
            //   letter-character
            //   _ (the underscore character U+005F)

            if (ch < 'a') // '\u0061'
            {
                if (ch < 'A') // '\u0041'
                {
                    return false;
                }

                return ch <= 'Z'  // '\u005A'
                    || ch == '_'; // '\u005F'
            }

            if (ch <= 'z') // '\u007A'
            {
                return true;
            }

            if (ch <= '\u007F') // max ASCII
            {
                return false;
            }

            return IsLetterChar(CharUnicodeInfo.GetUnicodeCategory(ch));
        }
        public static bool IsIdentifierPartCharacter(char ch) {
            // identifier-part-character:
            //   letter-character
            //   decimal-digit-character
            //   connecting-character
            //   combining-character
            //   formatting-character

            if (ch < 'a') // '\u0061'
            {
                if (ch < 'A') // '\u0041'
                {
                    return ch >= '0'  // '\u0030'
                        && ch <= '9'; // '\u0039'
                }

                return ch <= 'Z'  // '\u005A'
                    || ch == '_'; // '\u005F'
            }

            if (ch <= 'z') // '\u007A'
            {
                return true;
            }

            if (ch <= '\u007F') // max ASCII
            {
                return false;
            }

            UnicodeCategory cat = CharUnicodeInfo.GetUnicodeCategory(ch);
            return IsLetterChar(cat)
                || IsDecimalDigitChar(cat)
                || IsConnectingChar(cat)
                || IsCombiningChar(cat)
                || IsFormattingChar(cat);
        }
        private static bool IsLetterChar(UnicodeCategory cat) {
            // letter-character:
            //   A Unicode character of classes Lu, Ll, Lt, Lm, Lo, or Nl 
            //   A Unicode-escape-sequence representing a character of classes Lu, Ll, Lt, Lm, Lo, or Nl

            switch (cat) {
                case UnicodeCategory.UppercaseLetter:
                case UnicodeCategory.LowercaseLetter:
                case UnicodeCategory.TitlecaseLetter:
                case UnicodeCategory.ModifierLetter:
                case UnicodeCategory.OtherLetter:
                case UnicodeCategory.LetterNumber:
                    return true;
            }

            return false;
        }
        private static bool IsCombiningChar(UnicodeCategory cat) {
            // combining-character:
            //   A Unicode character of classes Mn or Mc 
            //   A Unicode-escape-sequence representing a character of classes Mn or Mc

            switch (cat) {
                case UnicodeCategory.NonSpacingMark:
                case UnicodeCategory.SpacingCombiningMark:
                    return true;
            }

            return false;
        }
        private static bool IsDecimalDigitChar(UnicodeCategory cat) {
            // decimal-digit-character:
            //   A Unicode character of the class Nd 
            //   A unicode-escape-sequence representing a character of the class Nd

            return cat == UnicodeCategory.DecimalDigitNumber;
        }
        private static bool IsConnectingChar(UnicodeCategory cat) {
            // connecting-character:  
            //   A Unicode character of the class Pc
            //   A unicode-escape-sequence representing a character of the class Pc

            return cat == UnicodeCategory.ConnectorPunctuation;
        }
        private static bool IsFormattingChar(UnicodeCategory cat) {
            // formatting-character:  
            //   A Unicode character of the class Cf
            //   A unicode-escape-sequence representing a character of the class Cf

            return cat == UnicodeCategory.Format;
        }
        #endregion helpers
    }


}
