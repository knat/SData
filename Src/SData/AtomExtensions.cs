using System;
using System.Globalization;
using System.Text;
using SData.Internal;

namespace SData {
    internal static class AtomExtensions {
        internal const TypeKind AtomTypeStart = TypeKind.String;
        internal const TypeKind AtomTypeEnd = TypeKind.DateTimeOffset;
        internal static bool IsAtom(this TypeKind kind) {
            return kind >= AtomTypeStart && kind <= AtomTypeEnd;
        }
        internal static bool IsSimple(this TypeKind kind) {
            return IsAtom(kind) || kind == TypeKind.Enum;
        }
        //internal static bool IsClrEnum(this TypeKind kind) {
        //    return kind >= TypeKind.Int64 && kind <= TypeKind.Byte;
        //}
        internal static bool IsClrRefAtom(this TypeKind kind) {
            return kind == TypeKind.String || kind == TypeKind.IgnoreCaseString || kind == TypeKind.Binary;
        }
        internal static FullName GetFullName(TypeKind kind) {
            return new FullName(Extensions.SystemUri, kind.ToString());
        }
        //
        internal static bool TryInvParse(this string s, out char result) {
            if (s.Length == 1) {
                result = s[0];
                return true;
            }
            result = default(char);
            return false;
        }
        internal static bool TryInvParse(this string s, out decimal result) {
            return decimal.TryParse(s, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, NumberFormatInfo.InvariantInfo, out result);
        }
        internal static string ToInvString(this decimal value) {
            return value.ToString(CultureInfo.InvariantCulture);
        }
        internal static bool TryInvParse(this string s, out long result) {
            return long.TryParse(s, NumberStyles.AllowLeadingSign, NumberFormatInfo.InvariantInfo, out result);
        }
        internal static string ToInvString(this long value) {
            return value.ToString(CultureInfo.InvariantCulture);
        }
        internal static bool TryInvParse(this string s, out int result) {
            return int.TryParse(s, NumberStyles.AllowLeadingSign, NumberFormatInfo.InvariantInfo, out result);
        }
        internal static string ToInvString(this int value) {
            return value.ToString(CultureInfo.InvariantCulture);
        }
        internal static bool TryInvParse(this string s, out short result) {
            return short.TryParse(s, NumberStyles.AllowLeadingSign, NumberFormatInfo.InvariantInfo, out result);
        }
        internal static string ToInvString(this short value) {
            return value.ToString(CultureInfo.InvariantCulture);
        }
        internal static bool TryInvParse(this string s, out sbyte result) {
            return sbyte.TryParse(s, NumberStyles.AllowLeadingSign, NumberFormatInfo.InvariantInfo, out result);
        }
        internal static string ToInvString(this sbyte value) {
            return value.ToString(CultureInfo.InvariantCulture);
        }
        internal static bool TryInvParse(this string s, out ulong result) {
            return ulong.TryParse(s, NumberStyles.AllowLeadingSign, NumberFormatInfo.InvariantInfo, out result);
        }
        internal static string ToInvString(this ulong value) {
            return value.ToString(CultureInfo.InvariantCulture);
        }
        internal static bool TryInvParse(this string s, out uint result) {
            return uint.TryParse(s, NumberStyles.AllowLeadingSign, NumberFormatInfo.InvariantInfo, out result);
        }
        internal static string ToInvString(this uint value) {
            return value.ToString(CultureInfo.InvariantCulture);
        }
        internal static bool TryInvParse(this string s, out ushort result) {
            return ushort.TryParse(s, NumberStyles.AllowLeadingSign, NumberFormatInfo.InvariantInfo, out result);
        }
        internal static string ToInvString(this ushort value) {
            return value.ToString(CultureInfo.InvariantCulture);
        }
        internal static bool TryInvParse(this string s, out byte result) {
            return byte.TryParse(s, NumberStyles.AllowLeadingSign, NumberFormatInfo.InvariantInfo, out result);
        }
        internal static string ToInvString(this byte value) {
            return value.ToString(CultureInfo.InvariantCulture);
        }
        internal static bool TryInvParse(this string s, out double result) {
            if (s == "INF") {
                result = double.PositiveInfinity;
            }
            else if (s == "-INF") {
                result = double.NegativeInfinity;
            }
            else if (s == "NaN") {
                result = double.NaN;
            }
            else if (!double.TryParse(s, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent, NumberFormatInfo.InvariantInfo, out result)) {
                return false;
            }
            return true;
        }
        internal static string ToInvString(this double value, out bool isLiteral) {
            if (double.IsPositiveInfinity(value)) {
                isLiteral = true;
                return "INF";
            }
            else if (double.IsNegativeInfinity(value)) {
                isLiteral = true;
                return "-INF";
            }
            else if (double.IsNaN(value)) {
                isLiteral = true;
                return "NaN";
            }
            isLiteral = false;
            return value.ToString(NumberFormatInfo.InvariantInfo);
        }
        internal static bool TryInvParse(this string s, out float result) {
            if (s == "INF") {
                result = float.PositiveInfinity;
            }
            else if (s == "-INF") {
                result = float.NegativeInfinity;
            }
            else if (s == "NaN") {
                result = float.NaN;
            }
            else if (!float.TryParse(s, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent, NumberFormatInfo.InvariantInfo, out result)) {
                return false;
            }
            return true;
        }
        internal static string ToInvString(this float value, out bool isLiteral) {
            if (float.IsPositiveInfinity(value)) {
                isLiteral = true;
                return "INF";
            }
            else if (float.IsNegativeInfinity(value)) {
                isLiteral = true;
                return "-INF";
            }
            else if (float.IsNaN(value)) {
                isLiteral = true;
                return "NaN";
            }
            isLiteral = false;
            return value.ToString(NumberFormatInfo.InvariantInfo);
        }
        internal static bool TryInvParse(this string s, out bool result) {
            if (s == "true") {
                result = true;
            }
            else if (s == "false") {
                result = false;
            }
            else {
                result = false;
                return false;
            }
            return true;
        }
        internal static string ToInvString(this bool value) {
            return value ? "true" : "false";
        }
        internal static bool TryInvParse(this string s, out Guid result) {
            return Guid.TryParseExact(s, "D", out result);
        }
        internal static string ToInvString(this Guid value) {
            return value.ToString("D");
        }
        internal static bool TryInvParse(this string s, out TimeSpan result) {
            return TimeSpan.TryParseExact(s, "c", DateTimeFormatInfo.InvariantInfo, out result);
        }
        internal static string ToInvString(this TimeSpan value) {
            return value.ToString("c");
        }
        private const string _dtoFormatString = "yyyy-MM-ddTHH:mm:ss.FFFFFFFzzz";
        internal static bool TryInvParse(this string s, out DateTimeOffset result) {
            return DateTimeOffset.TryParseExact(s, _dtoFormatString, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out result);
        }
        internal static string ToInvString(this DateTimeOffset value) {
            return value.ToString(_dtoFormatString, DateTimeFormatInfo.InvariantInfo);
        }
        //
        internal static object TryParse(TypeKind typeKind, string s, bool isReadOnly = false) {
            switch (typeKind) {
                case TypeKind.String:
                    return s;
                case TypeKind.IgnoreCaseString:
                    return new IgnoreCaseString(s, isReadOnly);
                case TypeKind.Char: {
                        char r;
                        if (s.TryInvParse(out r)) {
                            return r;
                        }
                    }
                    break;
                case TypeKind.Decimal: {
                        decimal r;
                        if (s.TryInvParse(out r)) {
                            return r;
                        }
                    }
                    break;
                case TypeKind.Int64: {
                        long r;
                        if (s.TryInvParse(out r)) {
                            return r;
                        }
                    }
                    break;
                case TypeKind.Int32: {
                        int r;
                        if (s.TryInvParse(out r)) {
                            return r;
                        }
                    }
                    break;
                case TypeKind.Int16: {
                        short r;
                        if (s.TryInvParse(out r)) {
                            return r;
                        }
                    }
                    break;
                case TypeKind.SByte: {
                        sbyte r;
                        if (s.TryInvParse(out r)) {
                            return r;
                        }
                    }
                    break;
                case TypeKind.UInt64: {
                        ulong r;
                        if (s.TryInvParse(out r)) {
                            return r;
                        }
                    }
                    break;
                case TypeKind.UInt32: {
                        uint r;
                        if (s.TryInvParse(out r)) {
                            return r;
                        }
                    }
                    break;
                case TypeKind.UInt16: {
                        ushort r;
                        if (s.TryInvParse(out r)) {
                            return r;
                        }
                    }
                    break;
                case TypeKind.Byte: {
                        byte r;
                        if (s.TryInvParse(out r)) {
                            return r;
                        }
                    }
                    break;
                case TypeKind.Double: {
                        double r;
                        if (s.TryInvParse(out r)) {
                            return r;
                        }
                    }
                    break;
                case TypeKind.Single: {
                        float r;
                        if (s.TryInvParse(out r)) {
                            return r;
                        }
                    }
                    break;
                case TypeKind.Boolean: {
                        bool r;
                        if (s.TryInvParse(out r)) {
                            return r;
                        }
                    }
                    break;
                case TypeKind.Binary: {
                        Binary r;
                        if (Binary.TryFromBase64String(s, out r, isReadOnly)) {
                            return r;
                        }
                    }
                    break;
                case TypeKind.Guid: {
                        Guid r;
                        if (s.TryInvParse(out r)) {
                            return r;
                        }
                    }
                    break;
                case TypeKind.TimeSpan: {
                        TimeSpan r;
                        if (s.TryInvParse(out r)) {
                            return r;
                        }
                    }
                    break;
                case TypeKind.DateTimeOffset: {
                        DateTimeOffset r;
                        if (s.TryInvParse(out r)) {
                            return r;
                        }
                    }
                    break;

                default:
                    throw new ArgumentException("Invalid type kind: " + typeKind.ToString());
            }
            return null;
        }
        //
        //
        internal static void GetLiteral(string value, StringBuilder sb) {
            var length = value.Length;
            if (length == 0) {
                sb.Append("\"\"");
            }
            else {
                sb.Append("@\"");
                for (var i = 0; i < length; ++i) {
                    var ch = value[i];
                    if (ch == '"') {
                        sb.Append("\"\"");
                    }
                    else {
                        sb.Append(ch);
                    }
                }
                sb.Append('"');
            }
        }
        internal static string ToLiteral(this string value) {
            var sb = StringBuilderBuffer.Acquire();
            GetLiteral(value, sb);
            return sb.ToStringAndRelease();
        }
        internal static void GetLiteral(char value, StringBuilder sb) {
            sb.Append(@"'\u");
            sb.Append(((int)value).ToString("X4", CultureInfo.InvariantCulture));
            sb.Append('\'');
        }

    }
}
