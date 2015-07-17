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
        public DottedName DottedName;
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
        public bool TrySetMd(MdNamespace mdns) {
            var mdGlobalTypeMap = mdns.GlobalTypeMap;
            if (GlobalTypeMap.Count != mdGlobalTypeMap.Count) {
                return false;
            }
            var nsDottedName = DottedName;
            foreach (var globalType in GlobalTypeMap.Values) {
                MdGlobalType mdGlobalType;
                if (!mdGlobalTypeMap.TryGetValue(globalType.Name, out mdGlobalType)) {
                    return false;
                }
                globalType.DottedName = new DottedName(nsDottedName, mdGlobalType.CSName);
                if (!globalType.TrySetMd(mdGlobalType)) {
                    return false;
                }
            }
            return true;
        }
        public MdNamespace GetMd(out string uri, out string nsName) {
            if (!IsRef) {
                uri = Uri;
                nsName = DottedName.ToString();
                var mdGlobalTypeMap = new Dictionary<string, MdGlobalType>();
                foreach (var globalType in GlobalTypeMap.Values) {
                    mdGlobalTypeMap.Add(globalType.Name, globalType.GetMd());
                }
                return new MdNamespace(mdGlobalTypeMap);
            }
            uri = null;
            nsName = null;
            return null;
        }
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
                    globalType.DottedName = new DottedName(nsDottedName, name);
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
        public void GetSyntax(List<MemberDeclarationSyntax> list, ExpressionSyntax assMdExpr, List<ExpressionSyntax> globalTypeMdList) {
            if (!IsRef) {
                var memberList = new List<MemberDeclarationSyntax>();
                foreach (var globalType in GlobalTypeMap.Values) {
                    globalType.GetSyntax(memberList, assMdExpr);
                    globalTypeMdList.Add(globalType.MetadataRefSyntax);
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
        public DottedName DottedName;
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
        public abstract bool TrySetMd(MdGlobalType mdGlobalType);
        public abstract MdGlobalType GetMd();
        public abstract void MapMembers();
        public abstract void GetSyntax(List<MemberDeclarationSyntax> list, ExpressionSyntax assMdExpr);
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
            NullableTypeSyntax = nullableTypeSyntax;
            TypeDisplayName = typeSyntax.ToString();
            NullableTypeDisplayName = nullableTypeSyntax.ToString();
        }
        public readonly TypeSyntax TypeSyntax;
        public readonly TypeSyntax NullableTypeSyntax;
        public readonly string TypeDisplayName;
        public readonly string NullableTypeDisplayName;
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
        public override bool TrySetMd(MdGlobalType mdGlobalType) {
            throw new NotImplementedException();
        }
        public override MdGlobalType GetMd() {
            throw new NotImplementedException();
        }
        public override void MapMembers() {
            throw new NotImplementedException();
        }
        public override void GetSyntax(List<MemberDeclarationSyntax> list, ExpressionSyntax assMdExpr) {
            throw new NotImplementedException();
        }
        protected override ExpressionSyntax GetMetadataRefSyntax() {
            throw new NotImplementedException();
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
        public override bool TrySetMd(MdGlobalType mdGlobalType) {
            return mdGlobalType is MdEnumType;
        }
        public override MdGlobalType GetMd() {
            return new MdEnumType(Name, Name);
        }
        public override void MapMembers() {
        }
        public override void GetSyntax(List<MemberDeclarationSyntax> list, ExpressionSyntax assMdExpr) {
            var memberSyntaxList = new List<MemberDeclarationSyntax>();
            var memberMap = MemberMap;
            var atomKind = UnderlyingType.Kind;
            var mustBeStatic = atomKind == TypeKind.IgnoreCaseString || atomKind == TypeKind.Binary || atomKind == TypeKind.Guid
                || atomKind == TypeKind.TimeSpan || atomKind == TypeKind.DateTimeOffset;
            var atomTypeSyntax = UnderlyingType.TypeSyntax;
            foreach (var kv in memberMap) {
                memberSyntaxList.Add(CS.Field(mustBeStatic ? CS.PublicStaticReadOnlyTokenList : CS.PublicConstTokenList, atomTypeSyntax,
                   CS.UnescapedId(kv.Key), CSEX.AtomValueLiteral(atomKind, kv.Value)));
            }
            //>public static readonly EnumTypeMd __ThisMetadata = new EnumTypeMd(FullName fullName, AtomTypeMd underlyingType, Dictionary<string, object> members);
            //memberSyntaxList.Add(CS.Field(CS.PublicStaticReadOnlyTokenList, CSEX.EnumTypeMdName, ReflectionExtensions.ThisMetadataNameStr,
            //    CS.NewObjExpr(CSEX.EnumTypeMdName, CSEX.Literal(FullName),
            //    CS.NewArrOrNullExpr(CSEX.NameValuePairArrayType, 
            //        memberMap.Select(i => CS.NewObjExpr(CSEX.NameValuePairName, CS.Literal(i.Name), CS.IdName(i.Name.EscapeId()))))
            //    )));
            //>public static partial class XX { }
            list.Add(CS.Class(null, CS.PublicStaticPartialTokenList, CS.UnescapedId(DottedName.LastName), null, memberSyntaxList));
        }
        protected override ExpressionSyntax GetMetadataRefSyntax() {
            return CS.MemberAccessExpr(FullExprSyntax, ReflectionExtensions.ThisMetadataNameStr);
        }
    }
    internal sealed class ClassTypeInfo : GlobalTypeInfo {
        public ClassTypeInfo(NamespaceInfo ns, string name, bool isAbstract, bool isSealed, ClassTypeInfo baseClass,
            Dictionary<string, ClassTypePropertyInfo> propertyMap)
            : base(TypeKind.Class, ns, name) {
            IsAbstract = isAbstract;
            IsSealed = isSealed;
            BaseClass = baseClass;
            PropertyMap = propertyMap;
        }
        public readonly bool IsAbstract;
        public readonly bool IsSealed;
        public readonly ClassTypeInfo BaseClass;//opt
        public readonly Dictionary<string, ClassTypePropertyInfo> PropertyMap;//only includes this class's props 
        public ClassTypePropertyInfo TryGetProperty(string name) {
            ClassTypePropertyInfo p;
            PropertyMap.TryGetValue(name, out p);
            return p;
        }
        public ClassTypePropertyInfo TryGetPropertyInHierarchy(string name) {
            for (var info = this; info != null; info = info.BaseClass) {
                var p = info.TryGetProperty(name);
                if (p != null) {
                    return p;
                }
            }
            return null;
        }
        public override bool TrySetMd(MdGlobalType mdGlobalType) {
            var mdCls = mdGlobalType as MdClassType;
            if (mdCls == null) {
                return false;
            }
            var mdPropMap = mdCls.PropertyMap;
            if (PropertyMap.Count != mdPropMap.Count) {
                return false;
            }
            foreach (var prop in PropertyMap.Values) {
                MdClassTypeProperty mdProp;
                if (!mdPropMap.TryGetValue(prop.Name, out mdProp)) {
                    return false;
                }
                prop.CSName = mdProp.CSName;
                prop.IsCSProperty = mdProp.IsCSProperty;
            }
            return true;
        }
        public override MdGlobalType GetMd() {
            var mdPropertyMap = new Dictionary<string, MdClassTypeProperty>();
            foreach (var prop in PropertyMap.Values) {
                mdPropertyMap.Add(prop.Name, new MdClassTypeProperty(prop.Name, prop.CSName, prop.IsCSProperty));
            }
            return new MdClassType(Name, DottedName.LastName, mdPropertyMap);
        }
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
        public override void GetSyntax(List<MemberDeclarationSyntax> list, ExpressionSyntax assMdExpr) {
            var memberList = new List<MemberDeclarationSyntax>();
            var propMap = PropertyMap;
            foreach (var prop in propMap.Values) {
                prop.GetSyntax(memberList);
            }
            var baseClass = BaseClass;
            if (baseClass == null) {
                //>public TextSpan __TextSpan {get; private set;}
                //memberList.Add(CS.Property(null, CS.PublicTokenList, EX.TextSpanName, CS.Id(ReflectionExtensions.TextSpanNameStr), CS.GetPrivateSetAccessorList));
            }
            //>public static bool TryLoad(string filePath, System.IO.TextReader reader, DiagContext context, out XXX result) {
            //  return CData.Serializer.TryLoad<XXX>(filePath, reader, context, assemblyMetadata, __ThisMetadata, out result);
            //}
            memberList.Add(CS.Method(CS.PublicStaticTokenList, CS.BoolType, "TryLoad", new[] {
                    CS.Parameter(CS.StringType, "filePath"),
                    CS.Parameter(CS.TextReaderName, "reader"),
                    CS.Parameter(CSEX.LoadingContextName, "context"),
                    CS.OutParameter(FullNameSyntax, "result")
                },
                CS.ReturnStm(CS.InvoExpr(CS.MemberAccessExpr(CSEX.SerializerExpr, CS.GenericName("TryLoad", FullNameSyntax)),
                    SyntaxFactory.Argument(CS.IdName("filePath")), SyntaxFactory.Argument(CS.IdName("reader")), SyntaxFactory.Argument(CS.IdName("context")),
                    SyntaxFactory.Argument(assMdExpr), SyntaxFactory.Argument(CS.IdName(ReflectionExtensions.ThisMetadataNameStr)), CS.OutArgument("result")))));
            if (baseClass == null) {
                //>public void Save(TextWriter writer, string indentString = "\t", string newLineString = "\n") {
                //>  CData.Serializer.Save(this, __Metadata, writer, indentString, newLineString);
                //>}
                memberList.Add(CS.Method(CS.PublicTokenList, CS.VoidType, "Save", new[] { 
                        CS.Parameter(CS.TextWriterName, "writer"),
                        CS.Parameter(CS.StringType, "indentString", CS.Literal("\t")),
                        CS.Parameter(CS.StringType, "newLineString", CS.Literal("\n"))
                    },
                    CS.ExprStm(CS.InvoExpr(CS.MemberAccessExpr(CSEX.SerializerExpr, "Save"), SyntaxFactory.ThisExpression(),
                        CS.IdName(ReflectionExtensions.MetadataNameStr), CS.IdName("writer"), CS.IdName("indentString"), CS.IdName("newLineString")
                    ))));
                //>public void Save(StringBuilder stringBuilder, string indentString = "\t", string newLineString = "\n") {
                //>  CData.Serializer.Save(this, __Metadata, stringBuilder, indentString, newLineString);
                //>}
                memberList.Add(CS.Method(CS.PublicTokenList, CS.VoidType, "Save", new[] { 
                        CS.Parameter(CS.StringBuilderName, "stringBuilder"),
                        CS.Parameter(CS.StringType, "indentString", CS.Literal("\t")),
                        CS.Parameter(CS.StringType, "newLineString", CS.Literal("\n"))
                    },
                    CS.ExprStm(CS.InvoExpr(CS.MemberAccessExpr(CSEX.SerializerExpr, "Save"), SyntaxFactory.ThisExpression(),
                        CS.IdName(ReflectionExtensions.MetadataNameStr), CS.IdName("stringBuilder"), CS.IdName("indentString"), CS.IdName("newLineString")
                    ))));
            }
            //>new public static readonly ClassMetadata __ThisMetadata =
            //>  new ClassMetadata(FullName fullName, bool isAbstract, ClassMetadata baseClass, PropertyMetadata[] properties, Type clrType);
            memberList.Add(CS.Field(baseClass == null ? CS.PublicStaticReadOnlyTokenList : CS.NewPublicStaticReadOnlyTokenList,
                CSEX.ClassTypeMdName, ReflectionExtensions.ThisMetadataNameStr,
                CS.NewObjExpr(CSEX.ClassTypeMdName, CSEX.Literal(FullName), CS.Literal(IsAbstract),
                    baseClass == null ? CS.NullLiteral : baseClass.MetadataRefSyntax,
                    CS.NewArrOrNullExpr(CSEX.ClassTypePropertyMdArrayType, propMap.Values.Select(i => i.GetMetadataSyntax())),
                    CS.TypeOfExpr(FullNameSyntax)
                )));
            //>public virtual/override ClassMetadata __Metadata {
            //>    get { return __ThisMetadata; }
            //>}
            memberList.Add(CS.Property(baseClass == null ? CS.PublicVirtualTokenList : CS.PublicOverrideTokenList,
                CSEX.ClassTypeMdName, ReflectionExtensions.MetadataNameStr, true, default(SyntaxTokenList),
                new StatementSyntax[] { 
                    CS.ReturnStm(CS.IdName(ReflectionExtensions.ThisMetadataNameStr))
                }));

            list.Add(SyntaxFactory.ClassDeclaration(
                attributeLists: default(SyntaxList<AttributeListSyntax>),
                modifiers: IsAbstract ? CS.PublicAbstractPartialTokenList : CS.PublicPartialTokenList,
                identifier: CS.UnescapedId(DottedName.LastName),
                typeParameterList: null,
                baseList: baseClass == null ? null : CS.BaseList(baseClass.FullNameSyntax),
                constraintClauses: default(SyntaxList<TypeParameterConstraintClauseSyntax>),
                members: SyntaxFactory.List(memberList)
                ));
        }
        protected override ExpressionSyntax GetMetadataRefSyntax() {
            return CS.MemberAccessExpr(FullExprSyntax, ReflectionExtensions.ThisMetadataNameStr);
        }
    }
    internal sealed class ClassTypePropertyInfo {
        public ClassTypePropertyInfo(string name, LocalTypeInfo type) {
            Name = name;
            Type = type;
        }
        public readonly string Name;
        public readonly LocalTypeInfo Type;
        public string CSName;
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
                list.Add(CS.Property(null, CS.PublicTokenList, Type.GetTypeSyntax(), CS.UnescapedId(CSName), CS.GetSetAccessorList));
            }
        }
        public ExpressionSyntax GetMetadataSyntax() {
            //>new PropertyMetadata(string name, LocalTypeMetadata type, string clrName, bool isClrProperty)
            return CS.NewObjExpr(CSEX.ClassTypePropertyMdName, CS.Literal(Name), Type.GetMetadataSyntax(),
                CS.Literal(CSName), CS.Literal(IsCSProperty));
        }
    }

    internal abstract class LocalTypeInfo : TypeInfo {
        protected LocalTypeInfo(TypeKind kind)
            : base(kind) {
        }
        public bool IsNullable {
            get {
                return Kind == TypeKind.Nullable;
            }
        }
        public NonNullableTypeInfo NonNullableType {
            get {
                var nnt = this as NonNullableTypeInfo;
                if (nnt != null) {
                    return nnt;
                }
                return (this as NullableTypeInfo).ElementType;
            }
        }
        public abstract void CheckSymbol(ISymbol propSymbol, ITypeSymbol typeSymbol, string parentTypeName);
        public abstract TypeSyntax GetTypeSyntax();
        public abstract ExpressionSyntax GetMetadataSyntax();
    }
    internal sealed class NullableTypeInfo : LocalTypeInfo {
        public NullableTypeInfo(NonNullableTypeInfo elementType)
            : base(TypeKind.Nullable) {
            ElementType = elementType;
        }
        public readonly NonNullableTypeInfo ElementType;
        public override void CheckSymbol(ISymbol propSymbol, ITypeSymbol typeSymbol, string parentTypeName) {
            var gtr = ElementType as GlobalTypeRefInfo;
            if (gtr != null) {
                var effAtomType = gtr.EffectiveAtomType;
                if (effAtomType != null) {
                    if (!effAtomType.Kind.IsClrRefAtom()) {
                        if (typeSymbol.SpecialType != SpecialType.System_Nullable_T) {
                            CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.InvalidSchemaPropertyType,
                                parentTypeName, "System.Nullable<T>"), CSEX.GetTextSpan(propSymbol));
                        }
                        typeSymbol = ((INamedTypeSymbol)typeSymbol).TypeArguments[0];
                    }
                }
            }
            ElementType.CheckSymbol(propSymbol, typeSymbol, parentTypeName);
        }
        public override TypeSyntax GetTypeSyntax() {
            throw new NotImplementedException();
        }
        public override ExpressionSyntax GetMetadataSyntax() {
            throw new NotImplementedException();
        }
    }
    internal abstract class NonNullableTypeInfo : LocalTypeInfo {
        protected NonNullableTypeInfo(TypeKind kind)
            : base(kind) {
        }
        public T TryGetGlobalType<T>() where T : GlobalTypeInfo {
            var gtr = this as GlobalTypeRefInfo;
            if (gtr != null) {
                return gtr.GlobalType as T;
            }
            return null;
        }

    }
    internal sealed class GlobalTypeRefInfo : NonNullableTypeInfo {
        public GlobalTypeRefInfo(GlobalTypeInfo globalType)
            : base(globalType.Kind) {
            GlobalType = globalType;
        }
        public readonly GlobalTypeInfo GlobalType;
        public bool IsAtom {
            get { return Kind.IsAtom(); }
        }
        public bool IsEnum {
            get { return Kind == TypeKind.Enum; }
        }
        public bool IsClass {
            get { return Kind == TypeKind.Class; }
        }
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
                var atomType = AtomType;
                if (atomType == null && IsEnum) {
                    atomType = EnumType.UnderlyingType;
                }
                return atomType;
            }
        }
        public override void CheckSymbol(ISymbol propSymbol, ITypeSymbol typeSymbol, string parentTypeName) {
            if (IsClass) {
                if (!typeSymbol.FullNameEquals(GlobalType.DottedName.NameParts)) {
                    CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.InvalidSchemaPropertyTypeOrExplicitTypeExpected,
                        parentTypeName, GetTypeSyntax().ToString()), CSEX.GetTextSpan(propSymbol));
                }
            }
            else {
                var effAtomType = EffectiveAtomType;
                if (!CSEX.IsAtomType(effAtomType.Kind, typeSymbol)) {
                    CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.InvalidSchemaPropertyType,
                        parentTypeName, effAtomType.TypeDisplayName), CSEX.GetTextSpan(propSymbol));
                }
            }
        }
        public override TypeSyntax GetTypeSyntax() {
            if (IsClass) {
                return GlobalType.FullNameSyntax;
            }
            var effAtom = EffectiveAtomType;
            return IsNullable ? effAtom.NullableTypeSyntax : effAtom.TypeSyntax;
        }
        public override ExpressionSyntax GetMetadataSyntax() {
            if (IsAtom) {
                //>GlobalTypeRefMetadata.GetAtom(kind, isNullable)
                return CS.InvoExpr(CS.MemberAccessExpr(CSEX.GlobalTypeRefMdExpr, "GetAtom"), CSEX.Literal(Kind), CS.Literal(IsNullable));
            }
            //>new GlobalTypeRefMetadata(XXX.__ThisMetadata, isNullable)
            return CS.NewObjExpr(CSEX.GlobalTypeRefMdName, GlobalType.MetadataRefSyntax, CS.Literal(IsNullable));
        }
    }
    internal sealed class ObjectSetKeySelectorInfo : List<ClassTypePropertyInfo> {
        public GlobalTypeRefInfo KeyType {
            get { return this[Count - 1].Type as GlobalTypeRefInfo; }
        }
    }
    internal sealed class CollectionInfo : NonNullableTypeInfo {
        public CollectionInfo(TypeKind kind, LocalTypeInfo itemOrValueType, GlobalTypeRefInfo mapKeyType,
            ObjectSetKeySelectorInfo objectSetKeySelector)
            : base(kind) {
            ItemOrValueType = itemOrValueType;
            MapKeyType = mapKeyType;
            ObjectSetKeySelector = objectSetKeySelector;
        }
        public readonly LocalTypeInfo ItemOrValueType;
        public readonly GlobalTypeRefInfo MapKeyType;//opt, for map
        public readonly ObjectSetKeySelectorInfo ObjectSetKeySelector;//opt
        public INamedTypeSymbol CollectionSymbol;//opt
        public bool IsList {
            get { return Kind == TypeKind.List; }
        }
        public bool IsMap {
            get { return Kind == TypeKind.Map; }
        }
        public bool IsObjectSet {
            get { return Kind == TypeKind.ObjectSet; }
        }
        public bool IsSimpleSet {
            get { return Kind == TypeKind.SimpleSet; }
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
            else if (IsObjectSet) {
                var itfSymbol = typeSymbol.GetSelfOrInterface(CSEX.IObjectSet2NameParts);
                if (itfSymbol == null) {
                    CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.InvalidSchemaPropertyType,
                        parentTypeName, "SData.IObjectSet<TKey, TObject> or implementing class"),
                        CSEX.GetTextSpan(propSymbol));
                }
                var typeArgs = itfSymbol.TypeArguments;
                ObjectSetKeySelector.KeyType.CheckSymbol(propSymbol, typeArgs[0], parentTypeName + " <object set key>");
                ItemOrValueType.CheckSymbol(propSymbol, typeArgs[1], parentTypeName + " <object set item>");
            }
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
            if (symbolTypeKind == Microsoft.CodeAnalysis.TypeKind.Interface) {
            }
            else {
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
                CollectionSymbol = (INamedTypeSymbol)typeSymbol;
            }
        }
        public override TypeSyntax GetTypeSyntax() {
            if (CollectionSymbol != null) {
                return CollectionSymbol.ToNameSyntax();
            }
            if (IsList) {
                return CS.ListOf(ItemOrValueType.GetTypeSyntax());
            }
            else if (IsMap) {
                return CS.DictionaryOf(MapKeyType.GetTypeSyntax(), ItemOrValueType.GetTypeSyntax());
            }
            else if (IsObjectSet) {
                return CSEX.ObjectSetOf(ObjectSetKeySelector.KeyType.GetTypeSyntax(), ItemOrValueType.GetTypeSyntax());
            }
            else {//SimpleSet
                return CS.HashSetOf(ItemOrValueType.GetTypeSyntax());
            }
        }
        public override ExpressionSyntax GetMetadataSyntax() {
            var kind = Kind;
            ExpressionSyntax objectSetKeySelectorExpr = null;
            if (kind == TypeKind.ObjectSet) {
                //>(Func<ObjType, KeyType>)(obj => obj.Prop1.Prop2)
                ExpressionSyntax bodyExpr = CS.IdName("obj");
                foreach (var propInfo in ObjectSetKeySelector) {
                    bodyExpr = CS.MemberAccessExpr(bodyExpr, CS.EscapeId(propInfo.CSName));
                }
                objectSetKeySelectorExpr = CS.CastExpr(
                    CS.FuncOf(ItemOrValueType.GetTypeSyntax(), ObjectSetKeySelector.KeyType.GetTypeSyntax()),
                    CS.ParedExpr(CS.SimpleLambdaExpr("obj", bodyExpr)));
            }
            //>new CollectionMetadata(kind, isNullable, itemOrValueType, mapKeyType, objectSetKeySelector, clrType)
            return CS.NewObjExpr(CSEX.CollectionMdName,
                CSEX.Literal(Kind), CS.Literal(IsNullable), ItemOrValueType.GetMetadataSyntax(),
                kind == TypeKind.Map ? MapKeyType.GetMetadataSyntax() : CS.NullLiteral,
                objectSetKeySelectorExpr ?? CS.NullLiteral,
                CS.TypeOfExpr(GetTypeSyntax())
                );
        }
    }


}
