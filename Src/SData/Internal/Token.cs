using System;

namespace SData.Internal
{
    public static class TokenKind
    {
        public const int NormalName = -1000;// id
        public const int VerbatimName = -999;// @id
        public const int NormalString = -998;// "..."
        public const int VerbatimString = -997;// @"..."
        public const int Char = -996;// 'c'
        public const int Integer = -995;// +-123
        public const int Decimal = -994;// +-123.45
        public const int Real = -993;// +-123.45Ee+-12
        //
        public const int ColonColon = -900;// ::
        public const int DollarOpenBracket = -899;// $[
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
                return Kind == TokenKind.NormalName;
            }
        }
        public bool IsVerbatimName
        {
            get
            {
                return Kind == TokenKind.VerbatimName;
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
                return Kind == TokenKind.NormalString;
            }
        }
        public bool IsVerbatimString
        {
            get
            {
                return Kind == TokenKind.VerbatimString;
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
                return Kind == TokenKind.Char;
            }
        }
        public bool IsInteger
        {
            get
            {
                return Kind == TokenKind.Integer;
            }
        }
        public bool IsDecimal
        {
            get
            {
                return Kind == TokenKind.Decimal;
            }
        }
        public bool IsReal
        {
            get
            {
                return Kind == TokenKind.Real;
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
