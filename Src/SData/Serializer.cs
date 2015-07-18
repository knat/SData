using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Collections;
using SData.Internal;

namespace SData {
    public static class Serializer {
        public static bool TryLoad<T>(string filePath, TextReader reader, LoadingContext context, ClassTypeMd classTypeMd, out T result) where T : class {
            object obj;
            if (Parser.Parse(filePath, reader, context, classTypeMd, out obj)) {
                result = (T)obj;
                return true;
            }
            result = null;
            return false;
        }
        //public static bool TryLoad<T>(string filePath, string text, LoadingContext context, ClassTypeMd classTypeMd, out T result) where T : class {
        //    return TryLoad<T>(filePath, new SimpleStringReader(text), context, classTypeMd, out result);
        //}
        public static void Save(object obj, ClassTypeMd classTypeMd, TextWriter writer, string indentString = "\t", string newLineString = "\n") {
            if (writer == null) throw new ArgumentNullException("writer");
            var sb = StringBuilderBuffer.Acquire();
            Save(obj, classTypeMd, sb, indentString, newLineString);
            writer.Write(sb.ToStringAndRelease());
        }
        public static void Save(object obj, ClassTypeMd classTypeMd, StringBuilder stringBuilder, string indentString = "\t", string newLineString = "\n") {
            if (obj == null) throw new ArgumentNullException("obj");
            if (classTypeMd == null) throw new ArgumentNullException("classTypeMd");
            SaveClassValue(true, obj, classTypeMd, new SavingContext(stringBuilder, indentString, newLineString));
        }
        private static void SaveClassValue(bool isRoot, object obj, ClassTypeMd declaredClsMd, SavingContext context) {
            if (declaredClsMd != null) {
                var clsMd = declaredClsMd.GetMetadata(obj);
                if (clsMd != declaredClsMd) {
                    context.AppendTypeIndicator(clsMd.FullName);
                }
                context.Append('{');
                context.AppendLine();
                context.PushIndent();
                var sb = context.StringBuilder;
                foreach (var propMd in clsMd._propertyMap.Values) {
                    var propValue = propMd.GetValue(obj);
                    if (propValue != null) {
                        context.Append(propMd.Name);
                        sb.Append(" = ");
                        SaveLocalValue(propValue, propMd.Type, context);
                        sb.Append(',');
                        context.AppendLine();
                    }
                }
                SaveUntypedProperties(clsMd.GetUnknownProperties(obj), context);
                context.PopIndent();
                context.Append('}');
            }
            else {
                var utobj = obj as UntypedObject;
                if (utobj != null) {
                    if (utobj.ClassFullName != null) {
                        context.AppendTypeIndicator(utobj.ClassFullName.Value);
                    }
                    context.Append('{');
                    context.AppendLine();
                    context.PushIndent();
                    SaveUntypedProperties(utobj.Properties, context);
                    context.PopIndent();
                    context.Append('}');
                }
            }
            if (isRoot) {
                context.InsertAliasUriList();
            }
        }
        private static void SaveUntypedProperties(Dictionary<string, object> props, SavingContext context) {
            if (props != null) {
                var sb = context.StringBuilder;
                foreach (var kv in props) {
                    var propValue = kv.Value;
                    if (propValue != null) {
                        context.Append(kv.Key);
                        sb.Append(" = ");
                        SaveLocalValue(propValue, null, context);
                        sb.Append(',');
                        context.AppendLine();
                    }
                }
            }
        }
        private static void SaveLocalValue(object value, LocalTypeMd typeMd, SavingContext context) {
            var sb = context.StringBuilder;
            if (value == null) {
                context.Append("null");
            }
            else if (typeMd == null) {
                var typeKind = AtomExtensionsEx.GetTypeKind(value);
                if (typeKind != TypeKind.None) {
                    SaveAtomValue(value, typeKind, context);
                }
                else if (value is UntypedObject) {
                    SaveClassValue(false, value, null, context);
                }
                else {
                    var uem = value as UntypedEnumValue;
                    if (uem != null) {
                        context.AppendFullName(uem.EnumFullName);
                        sb.Append('.');
                        sb.Append(uem.MemberName);
                    }
                    else {
                        var enumerable = value as IEnumerable<object>;
                        if (enumerable != null) {
                            context.Append('[');
                            context.AppendLine();
                            context.PushIndent();
                            foreach (var item in enumerable) {
                                SaveLocalValue(item, null, context);
                                sb.Append(',');
                                context.AppendLine();
                            }
                            context.PopIndent();
                            context.Append(']');
                        }
                        else {
                            var dict = value as IDictionary<object, object>;
                            if (dict != null) {
                                context.Append("#[");
                                context.AppendLine();
                                context.PushIndent();
                                foreach (var kv in dict) {
                                    SaveLocalValue(kv.Key, null, context);
                                    sb.Append(" = ");
                                    SaveLocalValue(kv.Value, null, context);
                                    sb.Append(',');
                                    context.AppendLine();
                                }
                                context.PopIndent();
                                context.Append(']');
                            }
                            else {
                                string s;
                                var formattable = value as IFormattable;
                                if (formattable != null) {
                                    s = formattable.ToString(null, System.Globalization.CultureInfo.InvariantCulture);
                                }
                                else {
                                    s = value.ToString();
                                }
                                if (s != null) {
                                    SaveAtomValue(s, TypeKind.String, context);
                                }
                                else {
                                    context.Append("null");
                                }
                            }
                        }
                    }
                }
            }
            else {
                var nonNullableTypeMd = typeMd.NonNullableType;
                var typeKind = nonNullableTypeMd.Kind;
                if (typeKind.IsAtom()) {
                    SaveAtomValue(value, typeKind, context);
                }
                else if (typeKind == TypeKind.Class) {
                    SaveClassValue(false, value, nonNullableTypeMd.TryGetGlobalType<ClassTypeMd>(), context);
                }
                else if (typeKind == TypeKind.Enum) {
                    var enumMd = nonNullableTypeMd.TryGetGlobalType<EnumTypeMd>();
                    var memberName = enumMd.TryGetMemberName(value);
                    if (memberName == null) {
                        context.Append("null");
                    }
                    else {
                        context.AppendFullName(enumMd.FullName);
                        sb.Append('.');
                        sb.Append(memberName);
                    }
                }
                else if (typeKind == TypeKind.Map) {
                    var collMd = (CollectionTypeMd)nonNullableTypeMd;
                    var keyMd = collMd.MapKeyType;
                    var valueMd = collMd.ItemOrValueType;
                    context.Append("#[");
                    context.AppendLine();
                    context.PushIndent();
                    IDictionaryEnumerator mapEnumerator = collMd.GetMapEnumerator(value);
                    if (mapEnumerator != null) {
                        while (mapEnumerator.MoveNext()) {
                            SaveLocalValue(mapEnumerator.Key, keyMd, context);
                            sb.Append(" = ");
                            SaveLocalValue(mapEnumerator.Value, valueMd, context);
                            sb.Append(',');
                            context.AppendLine();
                        }
                        var disposable = mapEnumerator as IDisposable;
                        if (disposable != null) {
                            disposable.Dispose();
                        }
                    }
                    context.PopIndent();
                    context.Append(']');
                }
                else {
                    var itemMd = ((CollectionTypeMd)nonNullableTypeMd).ItemOrValueType;
                    context.Append('[');
                    context.AppendLine();
                    context.PushIndent();
                    foreach (var item in (IEnumerable)value) {
                        SaveLocalValue(item, itemMd, context);
                        sb.Append(',');
                        context.AppendLine();
                    }
                    context.PopIndent();
                    context.Append(']');
                }
            }
        }
        private static void SaveAtomValue(object value, TypeKind typeKind, SavingContext context) {
            context.Append(null);
            var sb = context.StringBuilder;
            switch (typeKind) {
                case TypeKind.String:
                    AtomExtensionsEx.GetLiteral((string)value, sb);
                    break;
                case TypeKind.IgnoreCaseString:
                    AtomExtensionsEx.GetLiteral(((IgnoreCaseString)value).Value, sb);
                    break;
                case TypeKind.Char:
                    AtomExtensionsEx.GetLiteral((char)value, sb);
                    break;
                case TypeKind.Decimal:
                    sb.Append(((decimal)value).ToInvString());
                    break;
                case TypeKind.Int64:
                    sb.Append(((long)value).ToInvString());
                    break;
                case TypeKind.Int32:
                    sb.Append(((int)value).ToInvString());
                    break;
                case TypeKind.Int16:
                    sb.Append(((short)value).ToInvString());
                    break;
                case TypeKind.SByte:
                    sb.Append(((sbyte)value).ToInvString());
                    break;
                case TypeKind.UInt64:
                    sb.Append(((ulong)value).ToInvString());
                    break;
                case TypeKind.UInt32:
                    sb.Append(((uint)value).ToInvString());
                    break;
                case TypeKind.UInt16:
                    sb.Append(((ushort)value).ToInvString());
                    break;
                case TypeKind.Byte:
                    sb.Append(((byte)value).ToInvString());
                    break;
                case TypeKind.Double: {
                        bool isLiteral;
                        var s = ((double)value).ToInvString(out isLiteral);
                        if (isLiteral) {
                            AtomExtensionsEx.GetLiteral(s, sb);
                        }
                        else {
                            sb.Append(s);
                        }
                    }
                    break;
                case TypeKind.Single: {
                        bool isLiteral;
                        var s = ((float)value).ToInvString(out isLiteral);
                        if (isLiteral) {
                            AtomExtensionsEx.GetLiteral(s, sb);
                        }
                        else {
                            sb.Append(s);
                        }
                    }
                    break;
                case TypeKind.Boolean:
                    sb.Append(((bool)value).ToInvString());
                    break;
                case TypeKind.Binary:
                    sb.Append('"');
                    sb.Append(((Binary)value).ToBase64String());
                    sb.Append('"');
                    break;
                case TypeKind.Guid:
                    sb.Append('"');
                    sb.Append(((Guid)value).ToInvString());
                    sb.Append('"');
                    break;
                case TypeKind.TimeSpan:
                    sb.Append('"');
                    sb.Append(((TimeSpan)value).ToInvString());
                    sb.Append('"');
                    break;
                case TypeKind.DateTimeOffset:
                    sb.Append('"');
                    sb.Append(((DateTimeOffset)value).ToInvString());
                    sb.Append('"');
                    break;

                default:
                    throw new ArgumentException("Invalid type kind: " + typeKind.ToString());
            }
        }

    }
}
