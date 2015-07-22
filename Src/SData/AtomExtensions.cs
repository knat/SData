using System;
using SData.Internal;

namespace SData {
    public static class AtomExtensions {
        public static object TryParse(TypeKind typeKind, string s, bool isReadOnly = false) {
            switch (typeKind) {
                case TypeKind.String:
                    return s;
                case TypeKind.IgnoreCaseString:
                    return new IgnoreCaseString(s, isReadOnly);
                case TypeKind.Char:
                    {
                        char r;
                        if (s.TryInvParse(out r)) {
                            return r;
                        }
                    }
                    break;
                case TypeKind.Decimal:
                    {
                        decimal r;
                        if (s.TryInvParse(out r)) {
                            return r;
                        }
                    }
                    break;
                case TypeKind.Int64:
                    {
                        long r;
                        if (s.TryInvParse(out r)) {
                            return r;
                        }
                    }
                    break;
                case TypeKind.Int32:
                    {
                        int r;
                        if (s.TryInvParse(out r)) {
                            return r;
                        }
                    }
                    break;
                case TypeKind.Int16:
                    {
                        short r;
                        if (s.TryInvParse(out r)) {
                            return r;
                        }
                    }
                    break;
                case TypeKind.SByte:
                    {
                        sbyte r;
                        if (s.TryInvParse(out r)) {
                            return r;
                        }
                    }
                    break;
                case TypeKind.UInt64:
                    {
                        ulong r;
                        if (s.TryInvParse(out r)) {
                            return r;
                        }
                    }
                    break;
                case TypeKind.UInt32:
                    {
                        uint r;
                        if (s.TryInvParse(out r)) {
                            return r;
                        }
                    }
                    break;
                case TypeKind.UInt16:
                    {
                        ushort r;
                        if (s.TryInvParse(out r)) {
                            return r;
                        }
                    }
                    break;
                case TypeKind.Byte:
                    {
                        byte r;
                        if (s.TryInvParse(out r)) {
                            return r;
                        }
                    }
                    break;
                case TypeKind.Double:
                    {
                        double r;
                        if (s.TryInvParse(out r)) {
                            return r;
                        }
                    }
                    break;
                case TypeKind.Single:
                    {
                        float r;
                        if (s.TryInvParse(out r)) {
                            return r;
                        }
                    }
                    break;
                case TypeKind.Boolean:
                    {
                        bool r;
                        if (s.TryInvParse(out r)) {
                            return r;
                        }
                    }
                    break;
                case TypeKind.Binary:
                    {
                        Binary r;
                        if (Binary.TryFromBase64String(s, out r, isReadOnly)) {
                            return r;
                        }
                    }
                    break;
                case TypeKind.Guid:
                    {
                        Guid r;
                        if (s.TryInvParse(out r)) {
                            return r;
                        }
                    }
                    break;
                case TypeKind.TimeSpan:
                    {
                        TimeSpan r;
                        if (s.TryInvParse(out r)) {
                            return r;
                        }
                    }
                    break;
                case TypeKind.DateTimeOffset:
                    {
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
        public static string ToString(TypeKind typeKind, object value) {
            switch (typeKind) {
                case TypeKind.String:
                    return (string)value;
                case TypeKind.IgnoreCaseString:
                    return ((IgnoreCaseString)value).Value;
                case TypeKind.Char:
                    return ((char)value).ToString();
                case TypeKind.Decimal:
                    return ((decimal)value).ToInvString();
                case TypeKind.Int64:
                    return ((long)value).ToInvString();
                case TypeKind.Int32:
                    return ((int)value).ToInvString();
                case TypeKind.Int16:
                    return ((short)value).ToInvString();
                case TypeKind.SByte:
                    return ((sbyte)value).ToInvString();
                case TypeKind.UInt64:
                    return ((ulong)value).ToInvString();
                case TypeKind.UInt32:
                    return ((uint)value).ToInvString();
                case TypeKind.UInt16:
                    return ((ushort)value).ToInvString();
                case TypeKind.Byte:
                    return ((byte)value).ToInvString();
                case TypeKind.Double:
                    {
                        bool isLiteral;
                        return ((double)value).ToInvString(out isLiteral);
                    }
                case TypeKind.Single:
                    {
                        bool isLiteral;
                        return ((float)value).ToInvString(out isLiteral);
                    }
                case TypeKind.Boolean:
                    return ((bool)value).ToInvString();
                case TypeKind.Binary:
                    return ((Binary)value).ToBase64String();
                case TypeKind.Guid:
                    return ((Guid)value).ToInvString();
                case TypeKind.TimeSpan:
                    return ((TimeSpan)value).ToInvString();
                case TypeKind.DateTimeOffset:
                    return ((DateTimeOffset)value).ToInvString();

                default:
                    throw new ArgumentException("Invalid type kind: " + typeKind.ToString());
            }
        }


    }
}
