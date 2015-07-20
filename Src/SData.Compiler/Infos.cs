using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SData.Internal;

namespace SData.Compiler {
    internal sealed class NamespaceInfoMap : Dictionary<string, NamespaceInfo> {
    }
    internal sealed class NamespaceInfo {
        public NamespaceInfo(string uri) {
            Uri = uri;
            NamespaceNodeList = new List<NamespaceNode>();
            GlobalTypeMap = new Dictionary<string, GlobalTypeInfo>();
        }
        public readonly string Uri;
        public CSDottedName DottedName;
        public bool IsRef;
        public readonly List<NamespaceNode> NamespaceNodeList;
        public readonly Dictionary<string, GlobalTypeInfo> GlobalTypeMap;
        public void CheckDuplicateGlobalTypeNodes() {
            var list = NamespaceNodeList;
            var count = list.Count;
            for (var i = 0; i < count - 1; ++i) {
                var thisGlobalTypeMap = list[i].GlobalTypeMap;
                for (var j = i + 1; j < count; ++j) {
                    foreach (var otherGlobalTypeName in list[j].GlobalTypeMap.Keys) {
                        if (thisGlobalTypeMap.ContainsKey(otherGlobalTypeName)) {
                            CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.DuplicateGlobalTypeName, otherGlobalTypeName.Value),
                                otherGlobalTypeName.TextSpan);
                        }
                    }
                }
            }
        }
        public GlobalTypeNode TryGetGlobalTypeNode(Token name) {
            foreach (var ns in NamespaceNodeList) {
                GlobalTypeNode gt;
                if (ns.GlobalTypeMap.TryGetValue(name, out gt)) {
                    return gt;
                }
            }
            return null;
        }
        public T TryGetGlobalType<T>(string name) where T : GlobalTypeInfo {
            GlobalTypeInfo gt;
            if (GlobalTypeMap.TryGetValue(name, out gt)) {
                return gt as T;
            }
            return null;
        }
        public void SetRefData(IEnumerable<string> dottedTypeNames, IEnumerable<string> dottedPropertyNames) {
            var typeMap = GlobalTypeMap;
            var nsDottedName = DottedName;
            foreach (var dottedTypeName in dottedTypeNames) {
                string typeName, typeCSName;
                if (CSDottedName.TrySplit(dottedTypeName, out typeName, out typeCSName)) {
                    GlobalTypeInfo gti;
                    if (typeMap.TryGetValue(typeName, out gti)) {
                        gti.DottedName = new CSDottedName(nsDottedName, typeCSName);
                    }
                }
            }
            foreach (var gti in typeMap.Values) {
                if (gti.DottedName == null) {
                    gti.DottedName = new CSDottedName(nsDottedName, gti.Name);
                }
            }
            //
            foreach (var dottedPropertyName in dottedPropertyNames) {
                string typeName, propName, propCSName;
                if (CSDottedName.TrySplit(dottedPropertyName, out typeName, out propName, out propCSName)) {
                    GlobalTypeInfo gti;
                    if (typeMap.TryGetValue(typeName, out gti)) {
                        gti.SetPropertyRefData(propName, propCSName);
                    }
                }
            }
        }
        public List<string> GetRefData(out List<string> dottedPropertyNames) {
            var dottedTypeNames = new List<string>();
            dottedPropertyNames = new List<string>();
            foreach (var gti in GlobalTypeMap.Values) {
                var tyepCSName = gti.CSName;
                if (gti.Name != tyepCSName) {
                    dottedTypeNames.Add(gti.Name + "." + tyepCSName);
                }
                gti.GetPropertyRefData(dottedPropertyNames);
            }
            return dottedTypeNames;
        }


        //public bool TrySetRefMd(RefMdNamespace mdns) {
        //    var mdGlobalTypeMap = mdns.GlobalTypeMap;
        //    if (GlobalTypeMap.Count != mdGlobalTypeMap.Count) {
        //        return false;
        //    }
        //    var nsDottedName = DottedName;
        //    foreach (var globalType in GlobalTypeMap.Values) {
        //        RefMdGlobalType mdGlobalType;
        //        if (!mdGlobalTypeMap.TryGetValue(globalType.Name, out mdGlobalType)) {
        //            return false;
        //        }
        //        globalType.DottedName = new DottedName(nsDottedName, mdGlobalType.CSName);
        //        if (!globalType.TrySetRefMd(mdGlobalType)) {
        //            return false;
        //        }
        //    }
        //    return true;
        //}
        //public RefMdNamespace GetRefMd(out string uri, out string nsName) {
        //    if (!IsRef) {
        //        uri = Uri;
        //        nsName = DottedName.ToString();
        //        var mdGlobalTypeMap = new Dictionary<string, RefMdGlobalType>();
        //        foreach (var globalType in GlobalTypeMap.Values) {
        //            mdGlobalTypeMap.Add(globalType.Name, globalType.GetRefMd());
        //        }
        //        return new RefMdNamespace(mdGlobalTypeMap);
        //    }
        //    uri = null;
        //    nsName = null;
        //    return null;
        //}
        public void SetGlobalTypeDottedNames() {
            if (!IsRef) {
                var nsDottedName = DottedName;
                foreach (var globalType in GlobalTypeMap.Values) {
                    string name;
                    if (globalType.Symbol != null) {
                        name = globalType.Symbol.Name;
                    }
                    else {
                        name = globalType.Name;
                    }
                    globalType.DottedName = new CSDottedName(nsDottedName, name);
                }
            }
        }
        public void MapGlobalTypeMembers() {
            if (!IsRef) {
                foreach (var globalType in GlobalTypeMap.Values) {
                    globalType.MapMembers();
                }
            }
        }
        public void GetSyntax(List<MemberDeclarationSyntax> list, List<ExpressionSyntax> globalTypeMdRefList) {
            if (!IsRef) {
                var memberList = new List<MemberDeclarationSyntax>();
                foreach (var globalType in GlobalTypeMap.Values) {
                    globalType.GetSyntax(memberList);
                    globalTypeMdRefList.Add(globalType.MetadataRefSyntax);
                }
                list.Add(SyntaxFactory.NamespaceDeclaration(DottedName.NonGlobalFullNameSyntax,
                    default(SyntaxList<ExternAliasDirectiveSyntax>), default(SyntaxList<UsingDirectiveSyntax>),
                    SyntaxFactory.List(memberList)));
            }
        }

    }

    internal abstract class TypeInfo {
        protected TypeInfo(TypeKind kind) {
            Kind = kind;
        }
        public readonly TypeKind Kind;
    }

    internal abstract class GlobalTypeInfo : TypeInfo {
        protected GlobalTypeInfo(TypeKind kind, NamespaceInfo ns, string name)
            : base(kind) {
            Namespace = ns;
            Name = name;
        }
        public readonly NamespaceInfo Namespace;
        public readonly string Name;
        public FullName FullName {
            get {
                return new FullName(Namespace.Uri, Name);
            }
        }
        public CSDottedName DottedName;
        public string CSName {
            get {
                return DottedName.LastName;
            }
        }
        public NameSyntax FullNameSyntax {
            get {
                return DottedName.FullNameSyntax;
            }
        }
        public ExpressionSyntax FullExprSyntax {
            get {
                return DottedName.FullExprSyntax;
            }
        }
        public INamedTypeSymbol Symbol;//opt
        public abstract void SetPropertyRefData(string propName, string propCSName);
        public abstract void GetPropertyRefData(List<string> dottedPropertyNames);

        //public abstract bool TrySetRefMd(RefMdGlobalType mdGlobalType);
        //public abstract RefMdGlobalType GetRefMd();
        public abstract void MapMembers();
        public abstract void GetSyntax(List<MemberDeclarationSyntax> list);
        private ExpressionSyntax _metadataRefSyntax;
        public ExpressionSyntax MetadataRefSyntax {
            get {
                return _metadataRefSyntax ?? (_metadataRefSyntax = GetMetadataRefSyntax());
            }
        }
        protected abstract ExpressionSyntax GetMetadataRefSyntax();
    }
    internal abstract class SimpleGlobalTypeInfo : GlobalTypeInfo {
        protected SimpleGlobalTypeInfo(TypeKind kind, NamespaceInfo ns, string name)
            : base(kind, ns, name) {
        }
    }
    internal sealed class AtomTypeInfo : SimpleGlobalTypeInfo {
        private AtomTypeInfo(TypeKind kind, TypeSyntax typeSyntax, TypeSyntax nullableTypeSyntax)
            : base(kind, null, null) {
            TypeSyntax = typeSyntax;
            //NullableTypeSyntax = nullableTypeSyntax;
            //TypeDisplayName = typeSyntax.ToString();
            //NullableTypeDisplayName = nullableTypeSyntax.ToString();
        }
        public readonly TypeSyntax TypeSyntax;
        //public readonly TypeSyntax NullableTypeSyntax;
        //public readonly string TypeDisplayName;
        //public readonly string NullableTypeDisplayName;
        public bool IsClrRefType {
            get {
                return Kind.IsClrRefAtom();
            }
        }
        public static AtomTypeInfo Get(TypeKind kind) {
            return _map[kind];
        }
        private static readonly Dictionary<TypeKind, AtomTypeInfo> _map = new Dictionary<TypeKind, AtomTypeInfo> {
            { TypeKind.String, new AtomTypeInfo(TypeKind.String, CS.StringType, CS.StringType) },
            { TypeKind.IgnoreCaseString, new AtomTypeInfo(TypeKind.IgnoreCaseString, CSEX.IgnoreCaseStringName, CSEX.IgnoreCaseStringName) },
            { TypeKind.Char, new AtomTypeInfo(TypeKind.Char, CS.CharType, CS.CharNullableType) },
            { TypeKind.Decimal, new AtomTypeInfo(TypeKind.Decimal, CS.DecimalType, CS.DecimalNullableType) },
            { TypeKind.Int64, new AtomTypeInfo(TypeKind.Int64, CS.LongType, CS.ULongNullableType) },
            { TypeKind.Int32, new AtomTypeInfo(TypeKind.Int32, CS.IntType, CS.IntNullableType) },
            { TypeKind.Int16, new AtomTypeInfo(TypeKind.Int16, CS.ShortType, CS.ShortNullableType) },
            { TypeKind.SByte, new AtomTypeInfo(TypeKind.SByte, CS.SByteType, CS.SByteNullableType) },
            { TypeKind.UInt64, new AtomTypeInfo(TypeKind.UInt64, CS.ULongType, CS.ULongNullableType) },
            { TypeKind.UInt32, new AtomTypeInfo(TypeKind.UInt32, CS.UIntType, CS.UIntNullableType) },
            { TypeKind.UInt16, new AtomTypeInfo(TypeKind.UInt16, CS.UShortType, CS.UShortNullableType) },
            { TypeKind.Byte, new AtomTypeInfo(TypeKind.Byte, CS.ByteType, CS.ByteNullableType) },
            { TypeKind.Double, new AtomTypeInfo(TypeKind.Double, CS.DoubleType, CS.DoubleNullableType) },
            { TypeKind.Single, new AtomTypeInfo(TypeKind.Single, CS.FloatType, CS.FloatNullableType) },
            { TypeKind.Boolean, new AtomTypeInfo(TypeKind.Boolean, CS.BoolType, CS.BoolNullableType) },
            { TypeKind.Binary, new AtomTypeInfo(TypeKind.Binary, CSEX.BinaryName, CSEX.BinaryName) },
            { TypeKind.Guid, new AtomTypeInfo(TypeKind.Guid, CS.GuidName, CS.GuidNullableType) },
            { TypeKind.TimeSpan, new AtomTypeInfo(TypeKind.TimeSpan, CS.TimeSpanName, CS.TimeSpanNullableType) },
            { TypeKind.DateTimeOffset, new AtomTypeInfo(TypeKind.DateTimeOffset, CS.DateTimeOffsetName, CS.DateTimeOffsetNullableType) },
        };
        public override void SetPropertyRefData(string propName, string propCSName) {
            throw new NotImplementedException();
        }
        public override void GetPropertyRefData(List<string> dottedPropertyNames) {
            throw new NotImplementedException();
        }
        //public override bool TrySetRefMd(RefMdGlobalType mdGlobalType) {
        //    throw new NotImplementedException();
        //}
        //public override RefMdGlobalType GetRefMd() {
        //    throw new NotImplementedException();
        //}
        public override void MapMembers() {
            throw new NotImplementedException();
        }
        public override void GetSyntax(List<MemberDeclarationSyntax> list) {
            throw new NotImplementedException();
        }
        protected override ExpressionSyntax GetMetadataRefSyntax() {
            //>SData.AtomTypeMd.Get(TypeKind)
            return CS.InvoExpr(CS.MemberAccessExpr(CSEX.AtomTypeMdExpr, "Get"), CSEX.Literal(Kind));
        }
    }

    internal sealed class EnumTypeInfo : SimpleGlobalTypeInfo {
        public EnumTypeInfo(NamespaceInfo ns, string name, AtomTypeInfo underlyingType, Dictionary<string, object> memberMap)
            : base(TypeKind.Enum, ns, name) {
            UnderlyingType = underlyingType;
            MemberMap = memberMap;
        }
        public readonly AtomTypeInfo UnderlyingType;
        public readonly Dictionary<string, object> MemberMap;
        public override void SetPropertyRefData(string propName, string propCSName) {
        }
        public override void GetPropertyRefData(List<string> dottedPropertyNames) {
        }
        //public override bool TrySetRefMd(RefMdGlobalType mdGlobalType) {
        //    return mdGlobalType is RefMdEnumType;
        //}
        //public override RefMdGlobalType GetRefMd() {
        //    return new RefMdEnumType(Name);
        //}
        public override void MapMembers() {
        }
        public override void GetSyntax(List<MemberDeclarationSyntax> list) {
            var memberSyntaxList = new List<MemberDeclarationSyntax>();
            var memberMap = MemberMap;
            var atomKind = UnderlyingType.Kind;
            var mustBeStatic = atomKind == TypeKind.IgnoreCaseString || atomKind == TypeKind.Binary || atomKind == TypeKind.Guid
                || atomKind == TypeKind.TimeSpan || atomKind == TypeKind.DateTimeOffset;
            var atomTypeSyntax = UnderlyingType.TypeSyntax;
            foreach (var kv in memberMap) {
                //>public static readonly Type Name = ...;
                memberSyntaxList.Add(CS.Field(mustBeStatic ? CS.PublicStaticReadOnlyTokenList : CS.PublicConstTokenList, atomTypeSyntax,
                   CS.UnescapedId(kv.Key), CSEX.AtomValueLiteral(atomKind, kv.Value)));
            }
            //>public static readonly EnumTypeMd __ThisMetadata = new EnumTypeMd(FullName fullName, AtomTypeMd underlyingType, Dictionary<string, object> members);
            memberSyntaxList.Add(CS.Field(CS.PublicStaticReadOnlyTokenList, CSEX.EnumTypeMdName, ReflectionExtensions.ThisMetadataNameStr,
                CS.NewObjExpr(CSEX.EnumTypeMdName, CSEX.Literal(FullName), UnderlyingType.MetadataRefSyntax,
                CS.NewObjWithCollInitOrNullExpr(CS.DictionaryOf(CS.StringType, CS.ObjectType), memberMap.Keys.Select(i => new ExpressionSyntax[] {
                    CS.Literal(i), CS.UnescapedIdName(i) }))
                )));
            //>public static partial class XX { }
            list.Add(CS.Class(null, CS.PublicStaticPartialTokenList, CS.UnescapedId(CSName), null, memberSyntaxList));
        }
        protected override ExpressionSyntax GetMetadataRefSyntax() {
            return CS.MemberAccessExpr(FullExprSyntax, ReflectionExtensions.ThisMetadataNameStr);
        }
    }
    internal sealed class ClassTypeInfo : GlobalTypeInfo {
        public ClassTypeInfo(NamespaceInfo ns, string name, bool isAbstract, bool isSealed, ClassTypeInfo baseClass,
            List<KeyNode> keyNodeList, Dictionary<string, PropertyInfo> propertyMap)
            : base(TypeKind.Class, ns, name) {
            IsAbstract = isAbstract;
            IsSealed = isSealed;
            BaseClass = baseClass;
            PropertyMap = propertyMap;
            List<KeyInfo> keyList = null;
            if (keyNodeList != null) {
                if (baseClass != null && baseClass.HasKey) {
                    CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.DuplicatePropertyName),
                        keyNodeList[0][0].TextSpan);
                }
                keyList = new List<KeyInfo>();
                foreach (var keyNode in keyNodeList) {
                    var key = new KeyInfo();
                    var clsType = this;
                    var tkCount = keyNode.Count;
                    for (var i = 0; i < tkCount; ++i) {
                        var token = keyNode[i];
                        var prop = clsType.TryGetPropertyInHierarchy(token.Value);
                        if (prop == null) {
                            CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.DuplicatePropertyName, token.Value),
                                token.TextSpan);
                        }
                        var propType = prop.Type;
                        if (propType.IsNullable) {
                            CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.DuplicatePropertyName, token.Value),
                                token.TextSpan);
                        }
                        if (propType.IsCollection) {
                            CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.DuplicatePropertyName, token.Value),
                                token.TextSpan);
                        }
                        if (propType.IsClass) {
                            clsType = (propType as GlobalTypeRefInfo).ClassType;
                            if (i == tkCount - 1 && !clsType.HasKey) {
                                CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.DuplicatePropertyName, token.Value),
                                    token.TextSpan);
                            }
                        }
                        else {
                            if (i < tkCount - 1) {
                                CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.DuplicatePropertyName, token.Value),
                                    token.TextSpan);
                            }
                        }
                        key.Add(prop);
                    }
                    keyList.Add(key);
                }
            }
            KeyList = keyList;
        }
        public readonly bool IsAbstract;
        public readonly bool IsSealed;
        public readonly ClassTypeInfo BaseClass;//opt
        public readonly List<KeyInfo> KeyList;//opt
        public readonly Dictionary<string, PropertyInfo> PropertyMap;//only includes this class's props 
        public bool HasKey {
            get {
                for (var info = this; info != null; info = info.BaseClass) {
                    if (info.KeyList != null) {
                        return true;
                    }
                }
                return false;
            }
        }
        public PropertyInfo TryGetProperty(string name) {
            PropertyInfo p;
            PropertyMap.TryGetValue(name, out p);
            return p;
        }
        public PropertyInfo TryGetPropertyInHierarchy(string name) {
            for (var info = this; info != null; info = info.BaseClass) {
                var p = info.TryGetProperty(name);
                if (p != null) {
                    return p;
                }
            }
            return null;
        }
        public override void SetPropertyRefData(string propName, string propCSName) {
            PropertyInfo pi;
            if (PropertyMap.TryGetValue(propName, out pi)) {
                pi.CSName = propCSName;
            }
        }
        public override void GetPropertyRefData(List<string> dottedPropertyNames) {
            foreach (var pi in PropertyMap.Values) {
                var propCSName = pi.CSName;
                if (pi.Name != propCSName) {
                    dottedPropertyNames.Add(Name + "." + pi.Name + "." + propCSName);
                }
            }
        }
        //public override bool TrySetRefMd(RefMdGlobalType mdGlobalType) {
        //    var mdCls = mdGlobalType as RefMdClassType;
        //    if (mdCls == null) {
        //        return false;
        //    }
        //    var mdPropMap = mdCls.PropertyMap;
        //    if (PropertyMap.Count != mdPropMap.Count) {
        //        return false;
        //    }
        //    foreach (var prop in PropertyMap.Values) {
        //        RefMdClassTypeProperty mdProp;
        //        if (!mdPropMap.TryGetValue(prop.Name, out mdProp)) {
        //            return false;
        //        }
        //        prop.CSName = mdProp.CSName;
        //        prop.IsCSProperty = mdProp.IsCSProperty;
        //    }
        //    return true;
        //}
        //public override RefMdGlobalType GetRefMd() {
        //    var mdPropertyMap = new Dictionary<string, RefMdClassTypeProperty>();
        //    foreach (var prop in PropertyMap.Values) {
        //        mdPropertyMap.Add(prop.Name, new RefMdClassTypeProperty(prop.CSName, prop.IsCSProperty));
        //    }
        //    return new RefMdClassType(CSDottedName.LastName, mdPropertyMap);
        //}
        public override void MapMembers() {
            var typeSymbol = Symbol;
            if (typeSymbol != null) {
                var memberSymbolList = typeSymbol.GetMembers().Where(i => {
                    var kind = i.Kind;
                    return kind == SymbolKind.Property || kind == SymbolKind.Field;
                }).ToList();
                for (var i = 0; i < memberSymbolList.Count; ) {
                    var memberSymbol = memberSymbolList[i];
                    var propAttData = memberSymbol.GetAttributeData(CSEX.SchemaPropertyAttributeNameParts);
                    if (propAttData != null) {
                        var propName = CSEX.GetFirstArgumentAsString(propAttData);
                        if (propName == null) {
                            CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.InvalidSchemaPropertyAttribute),
                                CSEX.GetTextSpan(propAttData));
                        }
                        var propInfo = TryGetProperty(propName);
                        if (propInfo == null) {
                            CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.InvalidSchemaPropertyAttributeName, propName),
                                CSEX.GetTextSpan(propAttData));
                        }
                        if (propInfo.Symbol != null) {
                            CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.DuplicateSchemaPropertyAttributeName, propName),
                                CSEX.GetTextSpan(propAttData));
                        }
                        propInfo.Symbol = memberSymbol;
                        memberSymbolList.RemoveAt(i);
                        continue;
                    }
                    ++i;
                }
                foreach (var memberSymbol in memberSymbolList) {
                    var propName = memberSymbol.Name;
                    var propInfo = TryGetProperty(propName);
                    if (propInfo != null) {
                        if (propInfo.Symbol == null) {
                            propInfo.Symbol = memberSymbol;
                        }
                    }
                }
            }

            foreach (var propInfo in PropertyMap.Values) {
                var memberSymbol = propInfo.Symbol;
                if (memberSymbol != null) {
                    if (memberSymbol.IsStatic) {
                        CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.SchemaPropertyCannotBeStatic),
                            CSEX.GetTextSpan(memberSymbol));
                    }
                    var propSymbol = propInfo.PropertySymbol;
                    var fieldSymbol = propInfo.FieldSymbol;
                    if (propSymbol != null) {
                        if (propSymbol.IsReadOnly || propSymbol.IsWriteOnly) {
                            CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.SchemaPropertyMustHaveGetterAndSetter),
                                CSEX.GetTextSpan(memberSymbol));
                        }
                        if (propSymbol.IsIndexer) {
                            CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.SchemaPropertyCannotBeIndexer),
                                CSEX.GetTextSpan(memberSymbol));
                        }
                    }
                    else {
                        if (fieldSymbol.IsConst) {
                            CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.SchemaFieldCannotBeConst),
                                CSEX.GetTextSpan(memberSymbol));
                        }
                    }
                    propInfo.CSName = memberSymbol.Name;
                    propInfo.IsCSProperty = propSymbol != null;
                    propInfo.CheckSymbol();
                }
                else {
                    propInfo.CSName = propInfo.Name;
                    propInfo.IsCSProperty = true;
                }
            }

        }
        public override void GetSyntax(List<MemberDeclarationSyntax> list) {
            var memberList = new List<MemberDeclarationSyntax>();
            var propMap = PropertyMap;
            foreach (var prop in propMap.Values) {
                prop.GetSyntax(memberList);
            }
            var baseClass = BaseClass;
            if (baseClass == null) {
                //>public Dictionary<string, object> __UnknownProperties {get; set;}
                memberList.Add(CS.Property(null, CS.PublicTokenList, CS.DictionaryOf(CS.StringType, CS.ObjectType), CS.Id(ReflectionExtensions.UnknownPropertiesNameStr), CS.GetSetAccessorList));
            }
            //>public static bool TryLoad(string filePath, TextReader reader, LoadingContext context, out XXX result) {
            //  return SData.Serializer.TryLoad<XXX>(filePath, reader, context, __ThisMetadata, out result);
            //}
            memberList.Add(CS.Method(CS.PublicStaticTokenList, CS.BoolType, "TryLoad", new[] {
                    CS.Parameter(CS.StringType, "filePath"),
                    CS.Parameter(CS.TextReaderName, "reader"),
                    CS.Parameter(CSEX.LoadingContextName, "context"),
                    CS.OutParameter(FullNameSyntax, "result")
                },
                CS.ReturnStm(CS.InvoExpr(CS.MemberAccessExpr(CSEX.SerializerExpr, CS.GenericName("TryLoad", FullNameSyntax)),
                    CS.Argument(CS.IdName("filePath")), CS.Argument(CS.IdName("reader")), CS.Argument(CS.IdName("context")),
                    CS.Argument(CS.IdName(ReflectionExtensions.ThisMetadataNameStr)), CS.OutArgument("result")))));
            if (baseClass == null) {
                //>public void Save(TextWriter writer, string indentString = "\t", string newLineString = "\n") {
                //>  SData.Serializer.Save(this, __Metadata, writer, indentString, newLineString);
                //>}
                memberList.Add(CS.Method(CS.PublicTokenList, CS.VoidType, "Save", new[] { 
                        CS.Parameter(CS.TextWriterName, "writer"),
                        CS.Parameter(CS.StringType, "indentString", CS.Literal("\t")),
                        CS.Parameter(CS.StringType, "newLineString", CS.Literal("\n"))
                    },
                    CS.ExprStm(CS.InvoExpr(CS.MemberAccessExpr(CSEX.SerializerExpr, "Save"), CS.ThisExpr(),
                        CS.IdName(ReflectionExtensions.MetadataNameStr), CS.IdName("writer"), CS.IdName("indentString"), CS.IdName("newLineString")
                    ))));
                //>public void Save(StringBuilder stringBuilder, string indentString = "\t", string newLineString = "\n") {
                //>  SData.Serializer.Save(this, __Metadata, stringBuilder, indentString, newLineString);
                //>}
                memberList.Add(CS.Method(CS.PublicTokenList, CS.VoidType, "Save", new[] { 
                        CS.Parameter(CS.StringBuilderName, "stringBuilder"),
                        CS.Parameter(CS.StringType, "indentString", CS.Literal("\t")),
                        CS.Parameter(CS.StringType, "newLineString", CS.Literal("\n"))
                    },
                    CS.ExprStm(CS.InvoExpr(CS.MemberAccessExpr(CSEX.SerializerExpr, "Save"), CS.ThisExpr(),
                        CS.IdName(ReflectionExtensions.MetadataNameStr), CS.IdName("stringBuilder"), CS.IdName("indentString"), CS.IdName("newLineString")
                    ))));
            }
            //>new public static readonly ClassTypeMd __ThisMetadata =
            //>  new ClassTypeMd(FullName fullName, bool isAbstract, ClassTypeMd baseClass, Dictionary<string, ClassTypePropertyMd> thisPropertyMap, Type clrType);
            memberList.Add(CS.Field(baseClass == null ? CS.PublicStaticReadOnlyTokenList : CS.NewPublicStaticReadOnlyTokenList,
                CSEX.ClassTypeMdName, ReflectionExtensions.ThisMetadataNameStr,
                CS.NewObjExpr(CSEX.ClassTypeMdName, CSEX.Literal(FullName), CS.Literal(IsAbstract),
                    baseClass == null ? CS.NullLiteral : baseClass.MetadataRefSyntax,
                    CS.NewObjWithCollInitOrNullExpr(CS.DictionaryOf(CS.StringType, CSEX.ClassTypePropertyMdArrayType),
                        propMap.Values.Select(i => new ExpressionSyntax[] { CS.Literal(i.Name), i.GetMetadataSyntax() })),
                    CS.TypeOfExpr(FullNameSyntax)
                )));
            //>public virtual/override ClassTypeMd __Metadata {
            //>    get { return __ThisMetadata; }
            //>}
            memberList.Add(CS.Property(baseClass == null ? CS.PublicVirtualTokenList : CS.PublicOverrideTokenList,
                CSEX.ClassTypeMdName, ReflectionExtensions.MetadataNameStr, true, default(SyntaxTokenList),
                new StatementSyntax[] { 
                    CS.ReturnStm(CS.IdName(ReflectionExtensions.ThisMetadataNameStr))
                }));
            TypeSyntax itfType = null;
            var keyList = KeyList;
            if (keyList != null) {
                itfType = CS.IEquatableOf(FullNameSyntax);
                //>public bool Equals(TYPE other) {
                //>    if ((object)this == (object)other) return true;
                //>    if ((object)other == null) return false;
                //>    return this.p1?.p2 == other.p1?.p2 && this.p2 == other.p2;
                //>}
                ExpressionSyntax retExpr = null;
                foreach (var key in keyList) {
                    var expr = CS.EqualsExpr(key.GetMemberAccessSyntax(CS.ThisExpr()), key.GetMemberAccessSyntax(CS.IdName("other")));
                    if (retExpr == null) {
                        retExpr = expr;
                    }
                    else {
                        retExpr = CS.LogicalAndExpr(retExpr, expr);
                    }
                }
                memberList.Add(CS.Method(CS.PublicTokenList, CS.VoidType, "Equals", new[] { CS.Parameter(FullNameSyntax, "other") },
                    CS.IfStm(CS.EqualsExpr(CS.CastExpr(CS.ObjectType, CS.ThisExpr()), CS.CastExpr(CS.ObjectType, CS.IdName("other"))),
                        CS.ReturnStm(CS.TrueLiteral)),
                    CS.IfStm(CS.EqualsExpr(CS.CastExpr(CS.ObjectType, CS.IdName("other")), CS.NullLiteral),
                        CS.ReturnStm(CS.FalseLiteral)),
                    CS.ReturnStm(retExpr)
                    ));
                //>public override bool Equals(object other) {
                //>    return Equals(other as TYPE);
                //>}
                memberList.Add(CS.Method(CS.PublicOverrideTokenList, CS.BoolType, "Equals", new[] { CS.Parameter(CS.ObjectType, "other") },
                    CS.ReturnStm(CS.InvoExpr(CS.IdName("Equals"), CS.AsExpr(CS.IdName("other"), FullNameSyntax)))
                    ));
                //>public override int GetHashCode() {
                //>    return ...;
                //>}
                var keyCount = keyList.Count;
                if (keyCount == 1) {
                    retExpr = keyList[0].GetGetHashCodeSyntax();
                }
                else {
                    //>SData.Extensions.CombineHash()
                    keyCount = Math.Min(keyCount, 3);
                    var args = new ArgumentSyntax[keyCount];
                    for (var i = 0; i < keyCount; ++i) {
                        args[i] = CS.Argument(keyList[i].GetGetHashCodeSyntax());
                    }
                    retExpr = CS.InvoExpr(CS.MemberAccessExpr(CSEX.ExtensionsExpr, "CombineHash"), args);
                }
                memberList.Add(CS.Method(CS.PublicOverrideTokenList, CS.IntType, "GetHashCode", null, CS.ReturnStm(retExpr)));

                //>public static bool operator ==(TYPE left, TYPE right) {
                //>    if ((object)left == null) {
                //>        return (object)right == null;
                //>    }
                //>    return left.Equals(right);
                //>}
                memberList.Add(CS.OperatorDecl(CS.BoolType, CS.EqualsEqualsToken, new[] { CS.Parameter(FullNameSyntax, "left"), CS.Parameter(FullNameSyntax, "right") },
                    CS.IfStm(CS.EqualsExpr(CS.CastExpr(CS.ObjectType, CS.IdName("left")), CS.NullLiteral),
                        CS.ReturnStm(CS.EqualsExpr(CS.CastExpr(CS.ObjectType, CS.IdName("right")), CS.NullLiteral))),
                    CS.ReturnStm(CS.InvoExpr(CS.MemberAccessExpr(CS.IdName("left"), "Equals"), CS.IdName("right")))
                    ));
                //>public static bool operator !=(TYPE left, TYPE right) {
                //>    return !(left == right);
                //>}
                memberList.Add(CS.OperatorDecl(CS.BoolType, CS.ExclamationEqualsToken, new[] { CS.Parameter(FullNameSyntax, "left"), CS.Parameter(FullNameSyntax, "right") },
                    CS.ReturnStm(CS.LogicalNotExpr(CS.ParedExpr(CS.EqualsExpr(CS.IdName("left"), CS.IdName("right")))))
                    ));
            }
            BaseListSyntax baseList;
            if (baseClass != null && itfType != null) {
                baseList = CS.BaseList(baseClass.FullNameSyntax, itfType);
            }
            else if (baseClass != null) {
                baseList = CS.BaseList(baseClass.FullNameSyntax);
            }
            else if (itfType != null) {
                baseList = CS.BaseList(itfType);
            }
            else {
                baseList = null;
            }
            list.Add(SyntaxFactory.ClassDeclaration(
                attributeLists: default(SyntaxList<AttributeListSyntax>),
                modifiers: IsAbstract ? CS.PublicAbstractPartialTokenList : CS.PublicPartialTokenList,
                identifier: CS.UnescapedId(CSName),
                typeParameterList: null,
                baseList: baseList,
                constraintClauses: default(SyntaxList<TypeParameterConstraintClauseSyntax>),
                members: SyntaxFactory.List(memberList)
                ));
        }
        protected override ExpressionSyntax GetMetadataRefSyntax() {
            return CS.MemberAccessExpr(FullExprSyntax, ReflectionExtensions.ThisMetadataNameStr);
        }
    }
    internal sealed class KeyInfo : List<PropertyInfo> {
        public ExpressionSyntax GetMemberAccessSyntax(ExpressionSyntax head) {
            var ma = CS.MemberAccessExpr(head, this[0].CSName.EscapeId());
            var count = Count;
            if (count == 1) {
                //>x.a
                return ma;
            }
            //> (x.a) ? ((.b) ? (.c))
            ExpressionSyntax result = null;
            for (var i = count - 1; i >= 1; --i) {
                var mbe = CS.MemberBindingExpr(CS.UnescapedIdName(this[i].CSName));
                if (result == null) {
                    result = mbe;
                }
                else {
                    result = CS.ConditionalAccessExpr(mbe, result);
                }
            }
            return CS.ConditionalAccessExpr(ma, result);
        }
        public ExpressionSyntax GetGetHashCodeSyntax() {
            var count = Count;
            if (count == 1) {
                var prop = this[0];
                if ((prop.Type as GlobalTypeRefInfo).IsClrRefType) {
                    //>this.p?.GetHashCode() ?? 0
                    return CS.CoalesceExpr(CS.ConditionalAccessExpr(CS.MemberAccessExpr(CS.ThisExpr(), prop.CSName.EscapeId()),
                        CS.InvoExpr(CS.MemberBindingExpr(CS.IdName("GetHashCode")))), CS.Literal(0));
                }
                else {
                    //this.p.GetHashCode()
                    return CS.InvoExpr(CS.MemberAccessExpr(CS.MemberAccessExpr(CS.ThisExpr(), prop.CSName.EscapeId()), "GetHashCode"));
                }
            }
            //> this.a ? .b ? .c ? .GetHashCode() ?? 0
            //> this.a ? .b ? .c .GetHashCode() ?? 0
            ExpressionSyntax result = null;
            for (var i = count - 1; i >= 1; --i) {
                var prop = this[i];
                if (result == null) {
                    if ((prop.Type as GlobalTypeRefInfo).IsClrRefType) {
                        result = CS.ConditionalAccessExpr(CS.MemberBindingExpr(CS.UnescapedIdName(prop.CSName)),
                            CS.InvoExpr(CS.MemberBindingExpr(CS.IdName("GetHashCode"))));
                    }
                    else {
                        result = CS.InvoExpr(CS.MemberAccessExpr(CS.MemberBindingExpr(CS.UnescapedIdName(prop.CSName)), "GetHashCode"));
                    }
                }
                else {
                    result = CS.ConditionalAccessExpr(CS.MemberBindingExpr(CS.UnescapedIdName(prop.CSName)), result);
                }

            }
            return CS.CoalesceExpr(CS.ConditionalAccessExpr(CS.MemberAccessExpr(CS.ThisExpr(), this[0].CSName.EscapeId()), result), CS.Literal(0));
        }
    }
    internal sealed class PropertyInfo {
        public PropertyInfo(string name, LocalTypeInfo type) {
            Name = name;
            Type = type;
        }
        public readonly string Name;
        public readonly LocalTypeInfo Type;
        private string _csName;
        public string CSName {
            get {
                return _csName ?? Name;
            }
            set {
                _csName = value;
            }
        }
        public bool IsCSProperty;
        public ISymbol Symbol;//opt
        public IPropertySymbol PropertySymbol {
            get {
                return Symbol as IPropertySymbol;
            }
        }
        public IFieldSymbol FieldSymbol {
            get {
                return Symbol as IFieldSymbol;
            }
        }
        public void CheckSymbol() {
            var propSymbol = PropertySymbol;
            Type.CheckSymbol(Symbol, propSymbol != null ? propSymbol.Type : FieldSymbol.Type, null);
        }
        public void GetSyntax(List<MemberDeclarationSyntax> list) {
            if (Symbol == null) {
                //>public Type Name {get; set;}
                list.Add(CS.Property(null, CS.PublicTokenList, Type.TypeSyntax, CS.UnescapedId(CSName), CS.GetSetAccessorList));
            }
        }
        public ExpressionSyntax GetMetadataSyntax() {
            //>new ClassTypePropertyMd(string name, LocalTypeMd type, string clrName, bool isClrProperty)
            return CS.NewObjExpr(CSEX.ClassTypePropertyMdName, CS.Literal(Name), Type.MetadataSyntax,
                CS.Literal(CSName), CS.Literal(IsCSProperty));
        }
    }

    internal abstract class LocalTypeInfo : TypeInfo {
        protected LocalTypeInfo(TypeKind kind)
            : base(kind) {
        }
        public bool IsNullable {
            get { return Kind == TypeKind.Nullable; }
        }
        public bool IsAtom {
            get { return Kind.IsAtom(); }
        }
        public bool IsEnum {
            get { return Kind == TypeKind.Enum; }
        }
        public bool IsClass {
            get { return Kind == TypeKind.Class; }
        }
        public bool IsCollection {
            get { return this is CollectionInfo; }
        }

        //public NonNullableTypeInfo NonNullableType {
        //    get {
        //        var nnt = this as NonNullableTypeInfo;
        //        if (nnt != null) {
        //            return nnt;
        //        }
        //        return (this as NullableTypeInfo).ElementType;
        //    }
        //}
        public abstract void CheckSymbol(ISymbol propSymbol, ITypeSymbol typeSymbol, string parentTypeName);
        private TypeSyntax _typeSyntax;
        public TypeSyntax TypeSyntax {
            get {
                return _typeSyntax ?? (_typeSyntax = GetTypeSyntax());
            }
        }
        protected abstract TypeSyntax GetTypeSyntax();
        private ExpressionSyntax _metadataSyntax;
        public ExpressionSyntax MetadataSyntax {
            get {
                return _metadataSyntax ?? (_metadataSyntax = GetMetadataSyntax());
            }
        }
        protected abstract ExpressionSyntax GetMetadataSyntax();
    }
    internal sealed class NullableTypeInfo : LocalTypeInfo {
        public NullableTypeInfo(NonNullableTypeInfo elementType)
            : base(TypeKind.Nullable) {
            ElementType = elementType;
        }
        public readonly NonNullableTypeInfo ElementType;
        private bool NeedClrNullable {
            get {
                var gtr = ElementType as GlobalTypeRefInfo;
                if (gtr != null) {
                    var effAtomType = gtr.EffectiveAtomType;
                    if (effAtomType != null) {
                        return !effAtomType.IsClrRefType;
                    }
                }
                return false;
            }
        }
        public override void CheckSymbol(ISymbol propSymbol, ITypeSymbol typeSymbol, string parentTypeName) {
            if (NeedClrNullable) {
                if (typeSymbol.SpecialType != SpecialType.System_Nullable_T) {
                    CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.InvalidSchemaPropertyType,
                        parentTypeName, "System.Nullable<T>"), CSEX.GetTextSpan(propSymbol));
                }
                typeSymbol = ((INamedTypeSymbol)typeSymbol).TypeArguments[0];
            }
            ElementType.CheckSymbol(propSymbol, typeSymbol, parentTypeName);
        }
        protected override TypeSyntax GetTypeSyntax() {
            if (NeedClrNullable) {
                return CS.NullableOf(ElementType.TypeSyntax);
            }
            return ElementType.TypeSyntax;
        }
        protected override ExpressionSyntax GetMetadataSyntax() {
            //>new NullableTypeMd(elementType)
            return CS.NewObjExpr(CSEX.NullableTypeMdName, ElementType.MetadataSyntax);
        }
    }
    internal abstract class NonNullableTypeInfo : LocalTypeInfo {
        protected NonNullableTypeInfo(TypeKind kind)
            : base(kind) {
        }
        //public T TryGetGlobalType<T>() where T : GlobalTypeInfo {
        //    var gtr = this as GlobalTypeRefInfo;
        //    if (gtr != null) {
        //        return gtr.GlobalType as T;
        //    }
        //    return null;
        //}
    }
    internal sealed class GlobalTypeRefInfo : NonNullableTypeInfo {
        public GlobalTypeRefInfo(GlobalTypeInfo globalType)
            : base(globalType.Kind) {
            GlobalType = globalType;
        }
        public readonly GlobalTypeInfo GlobalType;
        public ClassTypeInfo ClassType {
            get {
                return GlobalType as ClassTypeInfo;
            }
        }
        public SimpleGlobalTypeInfo SimpleType {
            get {
                return GlobalType as SimpleGlobalTypeInfo;
            }
        }
        public EnumTypeInfo EnumType {
            get {
                return GlobalType as EnumTypeInfo;
            }
        }
        public AtomTypeInfo AtomType {
            get {
                return GlobalType as AtomTypeInfo;
            }
        }
        public AtomTypeInfo EffectiveAtomType {
            get {
                if (IsEnum) {
                    return EnumType.UnderlyingType;
                }
                return AtomType;
            }
        }
        public bool IsClrRefType {
            get {
                var effAtomType = EffectiveAtomType;
                if (effAtomType != null) {
                    return effAtomType.IsClrRefType;
                }
                return true;
            }
        }
        public override void CheckSymbol(ISymbol propSymbol, ITypeSymbol typeSymbol, string parentTypeName) {
            if (IsClass) {
                if (!typeSymbol.FullNameEquals(GlobalType.DottedName.NameParts)) {
                    CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.InvalidSchemaPropertyTypeOrExplicitTypeExpected,
                        parentTypeName, TypeSyntax.ToString()), CSEX.GetTextSpan(propSymbol));
                }
            }
            else {
                var effAtomType = EffectiveAtomType;
                if (!CSEX.IsAtomType(effAtomType.Kind, typeSymbol)) {
                    CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.InvalidSchemaPropertyType,
                        parentTypeName, TypeSyntax.ToString()), CSEX.GetTextSpan(propSymbol));
                }
            }
        }
        protected override TypeSyntax GetTypeSyntax() {
            if (IsClass) {
                return GlobalType.FullNameSyntax;
            }
            return EffectiveAtomType.TypeSyntax;
        }
        protected override ExpressionSyntax GetMetadataSyntax() {
            //>new GlobalTypeRefMd(GlobalType)
            return CS.NewObjExpr(CSEX.GlobalTypeRefMdName, GlobalType.MetadataRefSyntax);
        }
    }
    //internal sealed class ObjectSetKeySelectorInfo : List<PropertyInfo> {
    //    public GlobalTypeRefInfo KeyType {
    //        get { return this[Count - 1].Type as GlobalTypeRefInfo; }
    //    }
    //}
    internal sealed class CollectionInfo : NonNullableTypeInfo {
        public CollectionInfo(TypeKind kind, LocalTypeInfo itemOrValueType, GlobalTypeRefInfo mapKeyType)
            : base(kind) {
            ItemOrValueType = itemOrValueType;
            MapKeyType = mapKeyType;
            //ObjectSetKeySelector = objectSetKeySelector;
        }
        public readonly LocalTypeInfo ItemOrValueType;
        public readonly GlobalTypeRefInfo MapKeyType;//opt, for map
        //public readonly ObjectSetKeySelectorInfo ObjectSetKeySelector;//opt
        public INamedTypeSymbol CollectionClassSymbol;//opt
        public bool IsList {
            get { return Kind == TypeKind.List; }
        }
        public bool IsSet {
            get { return Kind == TypeKind.Set; }
        }
        public bool IsMap {
            get { return Kind == TypeKind.Map; }
        }
        public override void CheckSymbol(ISymbol propSymbol, ITypeSymbol typeSymbol, string parentTypeName) {
            if (IsList) {
                var itfSymbol = typeSymbol.GetSelfOrInterface(CS.ICollection1NameParts);
                if (itfSymbol == null) {
                    CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.InvalidSchemaPropertyType,
                        parentTypeName, "System.Collections.Generic.ICollection<T> or implementing class"),
                        CSEX.GetTextSpan(propSymbol));
                }
                ItemOrValueType.CheckSymbol(propSymbol, itfSymbol.TypeArguments[0], parentTypeName + " <list item>");
            }
            else if (IsMap) {
                var itfSymbol = typeSymbol.GetSelfOrInterface(CS.IDictionary2NameParts);
                if (itfSymbol == null) {
                    CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.InvalidSchemaPropertyType,
                        parentTypeName, "System.Collections.Generic.IDictionary<TKey, TValue> or implementing class"),
                        CSEX.GetTextSpan(propSymbol));
                }
                var typeArgs = itfSymbol.TypeArguments;
                MapKeyType.CheckSymbol(propSymbol, typeArgs[0], parentTypeName + " <map key>");
                ItemOrValueType.CheckSymbol(propSymbol, typeArgs[1], parentTypeName + " <map value>");
            }
            //else if (IsObjectSet) {
            //    var itfSymbol = typeSymbol.GetSelfOrInterface(CSEX.IObjectSet2NameParts);
            //    if (itfSymbol == null) {
            //        CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.InvalidSchemaPropertyType,
            //            parentTypeName, "SData.IObjectSet<TKey, TObject> or implementing class"),
            //            CSEX.GetTextSpan(propSymbol));
            //    }
            //    var typeArgs = itfSymbol.TypeArguments;
            //    ObjectSetKeySelector.KeyType.CheckSymbol(propSymbol, typeArgs[0], parentTypeName + " <object set key>");
            //    ItemOrValueType.CheckSymbol(propSymbol, typeArgs[1], parentTypeName + " <object set item>");
            //}
            else {//SimpleSet
                var itfSymbol = typeSymbol.GetSelfOrInterface(CS.ISet1NameParts);
                if (itfSymbol == null) {
                    CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.InvalidSchemaPropertyType,
                        parentTypeName, "System.Collections.Generic.ISet<T> or implementing class"),
                        CSEX.GetTextSpan(propSymbol));
                }
                ItemOrValueType.CheckSymbol(propSymbol, itfSymbol.TypeArguments[0], parentTypeName + " <simple set item>");
            }
            //
            var symbolTypeKind = typeSymbol.TypeKind;
            if (symbolTypeKind != Microsoft.CodeAnalysis.TypeKind.Interface) {
                string errmsg = null;
                if (symbolTypeKind != Microsoft.CodeAnalysis.TypeKind.Class) {
                    errmsg = "Class";
                }
                else if (typeSymbol.IsAbstract) {
                    errmsg = "Non-abstract class";
                }
                else if (!((INamedTypeSymbol)typeSymbol).HasParameterlessConstructor()) {
                    errmsg = "Parameterless-constructor class";
                }
                if (errmsg != null) {
                    CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.InvalidSchemaPropertyType,
                        parentTypeName, errmsg), CSEX.GetTextSpan(propSymbol));
                }
                CollectionClassSymbol = (INamedTypeSymbol)typeSymbol;
            }
        }
        protected override TypeSyntax GetTypeSyntax() {
            if (CollectionClassSymbol != null) {
                return CollectionClassSymbol.ToNameSyntax();
            }
            if (IsList) {
                return CS.ListOf(ItemOrValueType.TypeSyntax);
            }
            else if (IsMap) {
                return CS.DictionaryOf(MapKeyType.TypeSyntax, ItemOrValueType.TypeSyntax);
            }
            //else if (IsObjectSet) {
            //    return CSEX.ObjectSetOf(ObjectSetKeySelector.KeyType.TypeSyntax, ItemOrValueType.TypeSyntax);
            //}
            else {//SimpleSet
                return CS.HashSetOf(ItemOrValueType.TypeSyntax);
            }
        }
        protected override ExpressionSyntax GetMetadataSyntax() {
            //ExpressionSyntax objectSetKeySelectorExpr = null;
            //if (IsObjectSet) {
            //    //>(Func<ObjType, KeyType>)(obj => obj.Prop1.Prop2)
            //    ExpressionSyntax bodyExpr = CS.IdName("obj");
            //    foreach (var propInfo in ObjectSetKeySelector) {
            //        bodyExpr = CS.MemberAccessExpr(bodyExpr, CS.EscapeId(propInfo.CSName));
            //    }
            //    objectSetKeySelectorExpr = CS.CastExpr(
            //        CS.FuncOf(ItemOrValueType.TypeSyntax, ObjectSetKeySelector.KeyType.TypeSyntax),
            //        CS.ParedExpr(CS.SimpleLambdaExpr("obj", bodyExpr)));
            //}
            //>new CollectionTypeMd(kind, itemOrValueType, mapKeyType, clrType)
            return CS.NewObjExpr(CSEX.CollectionMdName,
                CSEX.Literal(Kind), ItemOrValueType.MetadataSyntax,
                IsMap ? MapKeyType.MetadataSyntax : CS.NullLiteral,
                CS.TypeOfExpr(TypeSyntax)
                );
        }
    }


}
