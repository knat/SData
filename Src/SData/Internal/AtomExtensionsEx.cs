using System;
using System.Globalization;
using System.Text;
using SData.Internal;
using System.Collections.Generic;

namespace SData.Internal {
    public static class AtomExtensionsEx {
        public const TypeKind AtomTypeStart = TypeKind.String;
        public const TypeKind AtomTypeEnd = TypeKind.DateTimeOffset;
        public static bool IsAtom(this TypeKind kind) {
            return kind >= AtomTypeStart && kind <= AtomTypeEnd;
        }
        public static bool IsSimple(this TypeKind kind) {
            return IsAtom(kind) || kind == TypeKind.Enum;
        }
        public static bool IsClrRefAtom(this TypeKind kind) {
            return kind == TypeKind.String || kind == TypeKind.IgnoreCaseString || kind == TypeKind.Binary;
        }
        public static FullName GetFullName(TypeKind kind) {
            return new FullName(Extensions.SystemUri, kind.ToString());
        }
        //
        public static bool TryInvParse(this string s, out char result) {
            if (s.Length == 1) {
                result = s[0];
                return true;
            }
            result = default(char);
            return false;
        }
        public static bool TryInvParse(this string s, out decimal result) {
            return decimal.TryParse(s, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, NumberFormatInfo.InvariantInfo, out result);
        }
        public static string ToInvString(this decimal value) {
            return value.ToString(CultureInfo.InvariantCulture);
        }
        public static bool TryInvParse(this string s, out long result) {
            return long.TryParse(s, NumberStyles.AllowLeadingSign, NumberFormatInfo.InvariantInfo, out result);
        }
        public static string ToInvString(this long value) {
            return value.ToString(CultureInfo.InvariantCulture);
        }
        public static bool TryInvParse(this string s, out int result) {
            return int.TryParse(s, NumberStyles.AllowLeadingSign, NumberFormatInfo.InvariantInfo, out result);
        }
        public static string ToInvString(this int value) {
            return value.ToString(CultureInfo.InvariantCulture);
        }
        public static bool TryInvParse(this string s, out short result) {
            return short.TryParse(s, NumberStyles.AllowLeadingSign, NumberFormatInfo.InvariantInfo, out result);
        }
        public static string ToInvString(this short value) {
            return value.ToString(CultureInfo.InvariantCulture);
        }
        public static bool TryInvParse(this string s, out sbyte result) {
            return sbyte.TryParse(s, NumberStyles.AllowLeadingSign, NumberFormatInfo.InvariantInfo, out result);
        }
        public static string ToInvString(this sbyte value) {
            return value.ToString(CultureInfo.InvariantCulture);
        }
        public static bool TryInvParse(this string s, out ulong result) {
            return ulong.TryParse(s, NumberStyles.AllowLeadingSign, NumberFormatInfo.InvariantInfo, out result);
        }
        public static string ToInvString(this ulong value) {
            return value.ToString(CultureInfo.InvariantCulture);
        }
        public static bool TryInvParse(this string s, out uint result) {
            return uint.TryParse(s, NumberStyles.AllowLeadingSign, NumberFormatInfo.InvariantInfo, out result);
        }
        public static string ToInvString(this uint value) {
            return value.ToString(CultureInfo.InvariantCulture);
        }
        public static bool TryInvParse(this string s, out ushort result) {
            return ushort.TryParse(s, NumberStyles.AllowLeadingSign, NumberFormatInfo.InvariantInfo, out result);
        }
        public static string ToInvString(this ushort value) {
            return value.ToString(CultureInfo.InvariantCulture);
        }
        public static bool TryInvParse(this string s, out byte result) {
            return byte.TryParse(s, NumberStyles.AllowLeadingSign, NumberFormatInfo.InvariantInfo, out result);
        }
        public static string ToInvString(this byte value) {
            return value.ToString(CultureInfo.InvariantCulture);
        }
        public static bool TryInvParse(this string s, out double result) {
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
        public static string ToInvString(this double value, out bool isLiteral) {
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
        public static bool TryInvParse(this string s, out float result) {
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
        public static string ToInvString(this float value, out bool isLiteral) {
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
        public static bool TryInvParse(this string s, out bool result) {
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
        public static string ToInvString(this bool value) {
            return value ? "true" : "false";
        }
        public static bool TryInvParse(this string s, out Guid result) {
            return Guid.TryParseExact(s, "D", out result);
        }
        public static string ToInvString(this Guid value) {
            return value.ToString("D");
        }
        public static bool TryInvParse(this string s, out TimeSpan result) {
            return TimeSpan.TryParseExact(s, "c", DateTimeFormatInfo.InvariantInfo, out result);
        }
        public static string ToInvString(this TimeSpan value) {
            return value.ToString("c");
        }
        private const string _dtoFormatString = "yyyy-MM-ddTHH:mm:ss.FFFFFFFzzz";
        public static bool TryInvParse(this string s, out DateTimeOffset result) {
            return DateTimeOffset.TryParseExact(s, _dtoFormatString, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out result);
        }
        public static string ToInvString(this DateTimeOffset value) {
            return value.ToString(_dtoFormatString, DateTimeFormatInfo.InvariantInfo);
        }
        //

        //
        //
        public static void GetLiteral(string value, StringBuilder sb) {
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
        public static string ToLiteral(this string value) {
            var sb = StringBuilderBuffer.Acquire();
            GetLiteral(value, sb);
            return sb.ToStringAndRelease();
        }
        public static void GetLiteral(char value, StringBuilder sb) {
            sb.Append(@"'\u");
            sb.Append(((int)value).ToString("X4", CultureInfo.InvariantCulture));
            sb.Append('\'');
        }

        private static readonly Dictionary<Type, TypeKind> _typeKindMap = new Dictionary<Type, TypeKind> {
                { typeof(string), TypeKind.String },
                { typeof(IgnoreCaseString), TypeKind.IgnoreCaseString },
                { typeof(char), TypeKind.Char },
                { typeof(decimal), TypeKind.Decimal },
                { typeof(long), TypeKind.Int64 },
                { typeof(int), TypeKind.Int32 },
                { typeof(short), TypeKind.Int16 },
                { typeof(sbyte), TypeKind.SByte },
                { typeof(ulong), TypeKind.UInt64 },
                { typeof(uint), TypeKind.UInt32 },
                { typeof(ushort), TypeKind.UInt16 },
                { typeof(byte), TypeKind.Byte },
                { typeof(double), TypeKind.Double },
                { typeof(float), TypeKind.Single },
                { typeof(bool), TypeKind.Boolean },
                { typeof(Binary), TypeKind.Binary },
                { typeof(Guid), TypeKind.Guid },
                { typeof(TimeSpan), TypeKind.TimeSpan },
                { typeof(DateTimeOffset), TypeKind.DateTimeOffset },
            };
        public static TypeKind GetTypeKind(object value) {
            TypeKind kind;
            if (_typeKindMap.TryGetValue(value.GetType(), out kind)) {
                return kind;
            }
            return TypeKind.None;
        }

    }
}
