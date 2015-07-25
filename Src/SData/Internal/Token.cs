using System;

namespace SData.Internal
{
    public enum TokenKind
    {
        NormalName = -1000,// id
        VerbatimName,// @id
        NormalString,// "..."
        VerbatimString,// @"..."
        Char,// 'c'
        Integer,// +-123
        Decimal,// +-123.45
        Real,// +-123.45Ee+-12
        //
        ColonColon,// ::
        HashOpenBracket,// #[
    }
    public struct Token : IEquatable<Token>
    {
        public Token(int kind, string value, TextSpan textSpan)
        {
            Kind = kind;
            Value = value;
            TextSpan = textSpan;
        }
        public readonly int Kind;
        public readonly string Value;//for TokenKind.NormalName to TokenKind.Real
        public readonly TextSpan TextSpan;
        public TokenKind TokenKind
        {
            get
            {
                return (TokenKind)Kind;
            }
        }
        public bool IsValid
        {
            get
            {
                return TextSpan.IsValid;
            }
        }
        public bool IsEndOfFile
        {
            get
            {
                return Kind == char.MaxValue;
            }
        }
        public bool IsNormalName
        {
            get
            {
                return TokenKind == TokenKind.NormalName;
            }
        }
        public bool IsVerbatimName
        {
            get
            {
                return TokenKind == TokenKind.VerbatimName;
            }
        }
        public bool IsName
        {
            get
            {
                return IsNormalName || IsVerbatimName;
            }
        }
        public bool IsKeyword(string value)
        {
            return IsNormalName && Value == value;
        }
        public bool IsNull
        {
            get
            {
                return IsKeyword("null");
            }
        }
        public bool IsTrue
        {
            get
            {
                return IsKeyword("true");
            }
        }
        public bool IsFlase
        {
            get
            {
                return IsKeyword("false");
            }
        }
        public bool IsBoolean
        {
            get
            {
                return IsTrue || IsFlase;
            }
        }
        public bool IsNormalString
        {
            get
            {
                return TokenKind == TokenKind.NormalString;
            }
        }
        public bool IsVerbatimString
        {
            get
            {
                return TokenKind == TokenKind.VerbatimString;
            }
        }
        public bool IsString
        {
            get
            {
                return IsNormalString || IsVerbatimString;
            }
        }
        public bool IsChar
        {
            get
            {
                return TokenKind == TokenKind.Char;
            }
        }
        public bool IsInteger
        {
            get
            {
                return TokenKind == TokenKind.Integer;
            }
        }
        public bool IsDecimal
        {
            get
            {
                return TokenKind == TokenKind.Decimal;
            }
        }
        public bool IsReal
        {
            get
            {
                return TokenKind == TokenKind.Real;
            }
        }
        public bool IsAtomValue
        {
            get
            {
                return IsString || IsInteger || IsBoolean || IsDecimal || IsReal || IsChar;
            }
        }
        //
        public override string ToString()
        {
            return Value;
        }
        public bool Equals(Token other)
        {
            return Value == other.Value;
        }
        public override bool Equals(object obj)
        {
            return obj is Token && Equals((Token)obj);
        }
        public override int GetHashCode()
        {
            return Value != null ? Value.GetHashCode() : 0;
        }
        public static bool operator ==(Token left, Token right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(Token left, Token right)
        {
            return !left.Equals(right);
        }

    }


}
