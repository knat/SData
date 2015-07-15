using System;

namespace SData.Internal {
    public enum TokenKind {
        Whitespace = -1000,
        NewLine,
        SingleLineComment,
        MultiLineComment,
        NormalIdentifier,// id
        VerbatimIdentifier,// @id
        NormalString,// "..."
        VerbatimString,// @"..."
        Char,// 'c'
        Integer,// +-123
        Decimal,// +-123.45
        Real,// +-123.45Ee+-12
        //
        //
        //BarBar,// ||
        //BarEquals,// |=
        //AmpersandAmpersand,// &&
        //AmpersandEquals,// &=
        //MinusMinus,// --
        //MinusEquals,// -=
        //MinusGreaterThan,// ->
        //PlusPlus,// ++
        //PlusEquals,// +=
        //ExclamationEquals,// !=
        //EqualsEquals,// ==
        //EqualsGreaterThan,// =>
        //LessThanEquals,// <=
        //LessThanLessThan,// <<
        //LessThanLessThanEquals,// <<=
        //GreaterThanEquals,// >=
        ////GreaterThanGreaterThan,// >>
        ////GreaterThanGreaterThanEquals,// >>=
        //SlashEquals,// /=
        //AsteriskEquals,// *=
        //CaretEquals,// ^=
        //PercentEquals,// %=
        //QuestionQuestion,// ??
        ColonColon,// ::
        HashOpenBracket,// #[
        //
        //HashHash,// ##

    }
    public struct Token {
        public Token(int kind, string value, TextSpan textSpan) {
            Kind = kind;
            Value = value;
            TextSpan = textSpan;
        }
        public readonly int Kind;
        public readonly string Value;//for TokenKind.NormalIdentifier to TokenKind.Real
        public readonly TextSpan TextSpan;
        public TokenKind TokenKind {
            get {
                return (TokenKind)Kind;
            }
        }
        //public bool IsWhitespace {
        //    get {
        //        return TokenKind == TokenKind.Whitespace;
        //    }
        //}
        //public bool IsNewLine {
        //    get {
        //        return TokenKind == TokenKind.NewLine;
        //    }
        //}
        //public bool IsSingleLineComment {
        //    get {
        //        return TokenKind == TokenKind.SingleLineComment;
        //    }
        //}
        public bool IsEndOfFile {
            get {
                return Kind == char.MaxValue;
            }
        }
        public bool IsNormalIdentifier {
            get {
                return TokenKind == TokenKind.NormalIdentifier;
            }
        }
        public bool IsVerbatimIdentifier {
            get {
                return TokenKind == TokenKind.VerbatimIdentifier;
            }
        }
        public bool IsIdentifier {
            get {
                return IsNormalIdentifier || IsVerbatimIdentifier;
            }
        }
        public bool IsNull {
            get {
                return IsNormalIdentifier && Value == "null";
            }
        }
        public bool IsTrue {
            get {
                return IsNormalIdentifier && Value == "true";
            }
        }
        public bool IsFlase {
            get {
                return IsNormalIdentifier && Value == "false";
            }
        }
        public bool IsBoolean {
            get {
                return IsTrue || IsFlase;
            }
        }
        public bool IsNormalString {
            get {
                return TokenKind == TokenKind.NormalString;
            }
        }
        public bool IsVerbatimString {
            get {
                return TokenKind == TokenKind.VerbatimString;
            }
        }
        public bool IsString {
            get {
                return IsNormalString || IsVerbatimString;
            }
        }
        public bool IsChar {
            get {
                return TokenKind == TokenKind.Char;
            }
        }
        public bool IsInteger {
            get {
                return TokenKind == TokenKind.Integer;
            }
        }
        public bool IsDecimal {
            get {
                return TokenKind == TokenKind.Decimal;
            }
        }
        public bool IsReal {
            get {
                return TokenKind == TokenKind.Real;
            }
        }
        public bool IsAtom {
            get {
                return IsString || IsBoolean || IsInteger || IsDecimal || IsReal || IsChar;
            }
        }

    }


}
