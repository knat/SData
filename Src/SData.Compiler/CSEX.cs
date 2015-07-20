using System;
using System.Linq;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SData.Internal;

namespace SData.Compiler {
    internal static class CSEX {
        internal static readonly string[] IgnoreCaseStringNameParts = new string[] { "IgnoreCaseString", "SData" };
        internal static readonly string[] BinaryNameParts = new string[] { "Binary", "SData" };
        //internal static readonly string[] IObjectSet2NameParts = new string[] { "IObjectSet`2", "SData" };
        internal static readonly string[] SchemaNamespaceAttributeNameParts = new string[] { "SchemaNamespaceAttribute", "SData" };
        internal static readonly string[] __CompilerSchemaNamespaceAttributeNameParts = new string[] { "__CompilerSchemaNamespaceAttribute", "SData" };
        internal static readonly string[] SchemaClassAttributeNameParts = new string[] { "SchemaClassAttribute", "SData" };
        internal static readonly string[] SchemaPropertyAttributeNameParts = new string[] { "SchemaPropertyAttribute", "SData" };

        internal static int MapNamespaces(NamespaceInfoMap nsInfoMap, IAssemblySymbol assSymbol, bool isRef) {
            var count = 0;
            foreach (AttributeData attData in assSymbol.GetAttributes()) {
                if (attData.AttributeClass.FullNameEquals(isRef ? __CompilerSchemaNamespaceAttributeNameParts : SchemaNamespaceAttributeNameParts)) {
                    var ctorArgs = attData.ConstructorArguments;
                    string uri = null, dottedString = null;
                    var ctorArgsLength = ctorArgs.Length;
                    if (ctorArgsLength >= 2) {
                        uri = ctorArgs[0].Value as string;
                        if (uri != null) {
                            dottedString = ctorArgs[1].Value as string;
                        }
                    }
                    if (dottedString == null) {
                        if (isRef) {
                            CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.Invalid__CompilerSchemaNamespaceAttribute,
                                assSymbol.Identity.Name, uri, dottedString), default(TextSpan));
                        }
                        else {
                            CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.InvalidSchemaNamespaceAttribute),
                                GetTextSpan(attData));
                        }
                    }
                    NamespaceInfo nsInfo;
                    if (!nsInfoMap.TryGetValue(uri, out nsInfo)) {
                        if (isRef) {
                            continue;
                        }
                        else {
                            CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.InvalidSchemaNamespaceAttributeUri, uri),
                                GetTextSpan(attData));
                        }
                    }
                    if (nsInfo.DottedName != null) {
                        if (isRef) {
                            CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.Invalid__CompilerSchemaNamespaceAttribute,
                                assSymbol.Identity.Name, uri, dottedString), default(TextSpan));
                        }
                        else {
                            CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.DuplicateSchemaNamespaceAttributeUri, uri),
                                GetTextSpan(attData));
                        }
                    }
                    CSDottedName dottedName;
                    if (!CSDottedName.TryParse(dottedString, out dottedName)) {
                        if (isRef) {
                            CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.Invalid__CompilerSchemaNamespaceAttribute,
                                assSymbol.Identity.Name, uri, dottedString), default(TextSpan));
                        }
                        else {
                            CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.InvalidSchemaNamespaceAttributeNamespaceName, dottedString),
                                GetTextSpan(attData));
                        }
                    }
                    nsInfo.DottedName = dottedName;
                    nsInfo.IsRef = isRef;
                    ++count;
                    if (isRef) {
                        if (ctorArgsLength >= 4) {
                            nsInfo.SetRefData(ctorArgs[2].Values.Select(i => i.Value as string),
                               ctorArgs[3].Values.Select(i => i.Value as string));
                        }
                    }
                }
            }
            return count;
        }

        internal static string GetFirstArgumentAsString(AttributeData attData) {
            var ctorArgs = attData.ConstructorArguments;
            if (ctorArgs.Length > 0) {
                return ctorArgs[0].Value as string;
            }
            return null;
        }
        internal static void MapGlobalTypes(NamespaceInfoMap nsInfoMap, INamespaceSymbol nsSymbol) {
            if (!nsSymbol.IsGlobalNamespace) {
                foreach (var nsInfo in nsInfoMap.Values) {
                    if (nsSymbol.FullNameEquals(nsInfo.DottedName.NameParts)) {
                        var typeSymbolList = nsSymbol.GetMembers().OfType<INamedTypeSymbol>().Where(i => i.TypeKind == Microsoft.CodeAnalysis.TypeKind.Class).ToList();
                        for (var i = 0; i < typeSymbolList.Count; ) {
                            var typeSymbol = typeSymbolList[i];
                            var clsAttData = typeSymbol.GetAttributeData(SchemaClassAttributeNameParts);
                            if (clsAttData != null) {
                                if (typeSymbol.IsGenericType) {
                                    CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.SchemaClassCannotBeGeneric), GetTextSpan(typeSymbol));
                                }
                                if (typeSymbol.IsStatic) {
                                    CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.SchemaClassCannotBeStatic), GetTextSpan(typeSymbol));
                                }
                                var clsName = GetFirstArgumentAsString(clsAttData);
                                if (clsName == null) {
                                    CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.InvalidSchemaClassAttribute), GetTextSpan(clsAttData));
                                }
                                var clsInfo = nsInfo.TryGetGlobalType<ClassTypeInfo>(clsName);
                                if (clsInfo == null) {
                                    CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.InvalidSchemaClassAttributeName, clsName), GetTextSpan(clsAttData));
                                }
                                if (clsInfo.Symbol != null) {
                                    CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.DuplicateSchemaClassAttributeName, clsName), GetTextSpan(clsAttData));
                                }
                                if (!clsInfo.IsAbstract) {
                                    if (typeSymbol.IsAbstract) {
                                        CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.NonAbstractSchemaClassRequired),
                                            GetTextSpan(typeSymbol));
                                    }
                                    if (!typeSymbol.HasParameterlessConstructor()) {
                                        CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.ParameterlessConstructorRequired),
                                            GetTextSpan(typeSymbol));
                                    }
                                }
                                clsInfo.Symbol = typeSymbol;
                                typeSymbolList.RemoveAt(i);
                                continue;
                            }
                            ++i;
                        }

                        foreach (var typeSymbol in typeSymbolList) {
                            if (!typeSymbol.IsGenericType) {
                                var clsName = typeSymbol.Name;
                                var clsInfo = nsInfo.TryGetGlobalType<ClassTypeInfo>(clsName);
                                if (clsInfo != null) {
                                    if (clsInfo.Symbol == null) {
                                        if (typeSymbol.IsStatic) {
                                            CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.SchemaClassCannotBeStatic), GetTextSpan(typeSymbol));
                                        }
                                        if (!clsInfo.IsAbstract) {
                                            if (typeSymbol.IsAbstract) {
                                                CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.NonAbstractSchemaClassRequired),
                                                    GetTextSpan(typeSymbol));
                                            }
                                            if (!typeSymbol.HasParameterlessConstructor()) {
                                                CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.ParameterlessConstructorRequired),
                                                    GetTextSpan(typeSymbol));
                                            }
                                        }
                                        clsInfo.Symbol = typeSymbol;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            foreach (var subNsSymbol in nsSymbol.GetNamespaceMembers()) {
                MapGlobalTypes(nsInfoMap, subNsSymbol);
            }
        }
        #region
        internal static TextSpan GetTextSpan(AttributeData attData) {
            if (attData != null) {
                return GetTextSpan(attData.ApplicationSyntaxReference);
            }
            return default(TextSpan);
        }
        internal static TextSpan GetTextSpan(SyntaxReference sr) {
            if (sr != null) {
                return GetTextSpan(sr.GetSyntax().GetLocation());
            }
            return default(TextSpan);
        }
        internal static TextSpan GetTextSpan(ISymbol symbol) {
            if (symbol != null) {
                var locations = symbol.Locations;
                if (locations.Length > 0) {
                    return GetTextSpan(locations[0]);
                }
            }
            return default(TextSpan);
        }
        internal static TextSpan GetTextSpan(Location location) {
            if (location != null && location.IsInSource) {
                var csLineSpan = location.GetLineSpan();
                if (csLineSpan.IsValid) {
                    var csTextSpan = location.SourceSpan;
                    return new TextSpan(csLineSpan.Path, csTextSpan.Start, csTextSpan.Length,
                        ToTextPosition(csLineSpan.StartLinePosition), ToTextPosition(csLineSpan.EndLinePosition));
                }
            }
            return default(TextSpan);
        }
        private static TextPosition ToTextPosition(this LinePosition csPosition) {
            return new TextPosition(csPosition.Line + 1, csPosition.Character + 1);
        }
        #endregion
        internal static bool IsAtomType(TypeKind typeKind, ITypeSymbol typeSymbol) {
            switch (typeKind) {
                case TypeKind.String:
                    return typeSymbol.SpecialType == SpecialType.System_String;
                case TypeKind.IgnoreCaseString:
                    return typeSymbol.FullNameEquals(IgnoreCaseStringNameParts);
                case TypeKind.Char:
                    return typeSymbol.SpecialType == SpecialType.System_Char;
                case TypeKind.Decimal:
                    return typeSymbol.SpecialType == SpecialType.System_Decimal;
                case TypeKind.Int64:
                    return typeSymbol.SpecialType == SpecialType.System_Int64;
                case TypeKind.Int32:
                    return typeSymbol.SpecialType == SpecialType.System_Int32;
                case TypeKind.Int16:
                    return typeSymbol.SpecialType == SpecialType.System_Int16;
                case TypeKind.SByte:
                    return typeSymbol.SpecialType == SpecialType.System_SByte;
                case TypeKind.UInt64:
                    return typeSymbol.SpecialType == SpecialType.System_UInt64;
                case TypeKind.UInt32:
                    return typeSymbol.SpecialType == SpecialType.System_UInt32;
                case TypeKind.UInt16:
                    return typeSymbol.SpecialType == SpecialType.System_UInt16;
                case TypeKind.Byte:
                    return typeSymbol.SpecialType == SpecialType.System_Byte;
                case TypeKind.Double:
                    return typeSymbol.SpecialType == SpecialType.System_Double;
                case TypeKind.Single:
                    return typeSymbol.SpecialType == SpecialType.System_Single;
                case TypeKind.Boolean:
                    return typeSymbol.SpecialType == SpecialType.System_Boolean;
                case TypeKind.Binary:
                    return typeSymbol.FullNameEquals(BinaryNameParts);
                case TypeKind.Guid:
                    return typeSymbol.FullNameEquals(CS.GuidNameParts);
                case TypeKind.TimeSpan:
                    return typeSymbol.FullNameEquals(CS.TimeSpanNameParts);
                case TypeKind.DateTimeOffset:
                    return typeSymbol.FullNameEquals(CS.DateTimeOffsetNameParts);
                default:
                    throw new ArgumentException("Invalid type kind: " + typeKind.ToString());
            }
        }
        //internal static bool IsAtomType(TypeKind typeKind, bool isNullable, ITypeSymbol typeSymbol) {
        //    if (!isNullable || typeKind.IsClrRefAtom()) {
        //        return IsAtomType(typeKind, typeSymbol);
        //    }
        //    if (typeSymbol.SpecialType == SpecialType.System_Nullable_T) {
        //        return IsAtomType(typeKind, ((INamedTypeSymbol)typeSymbol).TypeArguments[0]);
        //    }
        //    return false;
        //}
        internal static ExpressionSyntax AtomValueLiteral(TypeKind typeKind, object value) {
            switch (typeKind) {
                case TypeKind.String:
                    return CS.Literal((string)value);
                case TypeKind.IgnoreCaseString:
                    return Literal((IgnoreCaseString)value);
                case TypeKind.Char:
                    return CS.Literal((char)value);
                case TypeKind.Decimal:
                    return CS.Literal((decimal)value);
                case TypeKind.Int64:
                    return CS.Literal((long)value);
                case TypeKind.Int32:
                    return CS.Literal((int)value);
                case TypeKind.Int16:
                    return CS.Literal((short)value);
                case TypeKind.SByte:
                    return CS.Literal((sbyte)value);
                case TypeKind.UInt64:
                    return CS.Literal((ulong)value);
                case TypeKind.UInt32:
                    return CS.Literal((uint)value);
                case TypeKind.UInt16:
                    return CS.Literal((ushort)value);
                case TypeKind.Byte:
                    return CS.Literal((byte)value);
                case TypeKind.Double:
                    return CS.Literal((double)value);
                case TypeKind.Single:
                    return CS.Literal((float)value);
                case TypeKind.Boolean:
                    return CS.Literal((bool)value);
                case TypeKind.Binary:
                    return Literal((Binary)value);
                case TypeKind.Guid:
                    return CS.Literal((Guid)value); ;
                case TypeKind.TimeSpan:
                    return CS.Literal((TimeSpan)value); ;
                case TypeKind.DateTimeOffset:
                    return CS.Literal((DateTimeOffset)value); ;
                default:
                    throw new ArgumentException("Invalid type kind: " + typeKind.ToString());
            }
        }
        //internal static ExpressionSyntax NameValuePairLiteral(NameValuePair value, TypeKind kind) {
        //    return CS.NewObjExpr(NameValuePairName, CS.Literal(value.Name), AtomValueLiteral(kind, value.Value));
        //}

        internal static string ToValidId(string s) {
            if (string.IsNullOrEmpty(s)) {
                return s;
            }
            var sb = StringBuilderBuffer.Acquire();
            foreach (var ch in s) {
                if (SyntaxFacts.IsIdentifierPartCharacter(ch)) {
                    sb.Append(ch);
                }
                else {
                    sb.Append('_');
                }
            }
            return sb.ToStringAndRelease();
        }
        internal static string SDataProgramName(string assName) {
            return "SData_" + ToValidId(assName);
        }
        internal static AliasQualifiedNameSyntax SDataName {
            get { return CS.GlobalAliasQualifiedName("SData"); }
        }
        internal static QualifiedNameSyntax __CompilerSchemaNamespaceAttributeName {
            get { return CS.QualifiedName(SDataName, "__CompilerSchemaNamespaceAttribute"); }
        }
        internal static QualifiedNameSyntax ProgramMdName {
            get { return CS.QualifiedName(SDataName, "ProgramMd"); }
        }
        internal static QualifiedNameSyntax GlobalTypeMdName {
            get { return CS.QualifiedName(SDataName, "GlobalTypeMd"); }
        }
        internal static ArrayTypeSyntax GlobalTypeMdArrayType {
            get { return CS.OneDimArrayType(GlobalTypeMdName); }
        }
        internal static QualifiedNameSyntax EnumTypeMdName {
            get { return CS.QualifiedName(SDataName, "EnumTypeMd"); }
        }
        //internal static QualifiedNameSyntax NameValuePairName {
        //    get { return CS.QualifiedName(SDataName, "NameValuePair"); }
        //}
        //internal static ArrayTypeSyntax NameValuePairArrayType {
        //    get { return CS.OneDimArrayType(NameValuePairName); }
        //}
        internal static QualifiedNameSyntax ClassTypeMdName {
            get { return CS.QualifiedName(SDataName, "ClassTypeMd"); }
        }
        internal static QualifiedNameSyntax ClassTypePropertyMdName {
            get { return CS.QualifiedName(SDataName, "ClassTypePropertyMd"); }
        }
        internal static ArrayTypeSyntax ClassTypePropertyMdArrayType {
            get { return CS.OneDimArrayType(ClassTypePropertyMdName); }
        }
        internal static QualifiedNameSyntax NullableTypeMdName {
            get { return CS.QualifiedName(SDataName, "NullableTypeMd"); }
        }
        internal static QualifiedNameSyntax GlobalTypeRefMdName {
            get { return CS.QualifiedName(SDataName, "GlobalTypeRefMd"); }
        }
        internal static MemberAccessExpressionSyntax GlobalTypeRefMdExpr {
            get { return CS.MemberAccessExpr(SDataName, "GlobalTypeRefMd"); }
        }
        internal static QualifiedNameSyntax CollectionMdName {
            get { return CS.QualifiedName(SDataName, "CollectionMd"); }
        }
        internal static ExpressionSyntax Literal(TypeKind value) {
            return CS.MemberAccessExpr(CS.MemberAccessExpr(SDataName, "TypeKind"), value.ToString());
        }
        internal static QualifiedNameSyntax FullNameName {
            get { return CS.QualifiedName(SDataName, "FullName"); }
        }
        internal static ExpressionSyntax Literal(FullName value) {
            return CS.NewObjExpr(FullNameName, CS.Literal(value.Uri), CS.Literal(value.Name));
        }
        internal static QualifiedNameSyntax IgnoreCaseStringName {
            get { return CS.QualifiedName(SDataName, "IgnoreCaseString"); }
        }
        internal static ExpressionSyntax Literal(IgnoreCaseString value) {
            return CS.NewObjExpr(IgnoreCaseStringName, CS.Literal(value.Value), CS.Literal(value.IsReadOnly));
        }
        internal static QualifiedNameSyntax BinaryName {
            get { return CS.QualifiedName(SDataName, "Binary"); }
        }
        internal static ExpressionSyntax Literal(Binary value) {
            return CS.NewObjExpr(BinaryName, CS.Literal(value.ToBytes()), CS.Literal(value.IsReadOnly));
        }
        internal static QualifiedNameSyntax ObjectSetOf(TypeSyntax keyType, TypeSyntax objectType) {
            return SyntaxFactory.QualifiedName(SDataName, CS.GenericName("ObjectSet", keyType, objectType));
        }

        internal static QualifiedNameSyntax LoadingContextName {
            get { return CS.QualifiedName(SDataName, "LoadingContext"); }
        }
        internal static QualifiedNameSyntax TextSpanName {
            get { return CS.QualifiedName(SDataName, "TextSpan"); }
        }
        internal static MemberAccessExpressionSyntax SerializerExpr {
            get { return CS.MemberAccessExpr(SDataName, "Serializer"); }
        }
        internal static MemberAccessExpressionSyntax ExtensionsExpr {
            get { return CS.MemberAccessExpr(SDataName, "Extensions"); }
        }
        internal static MemberAccessExpressionSyntax AtomTypeMdExpr {
            get { return CS.MemberAccessExpr(SDataName, "AtomTypeMd"); }
        }



    }

}
