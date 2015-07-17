using System;
using System.Collections.Generic;
using SData.Internal;

namespace SData.Compiler {
    internal sealed class CompilationUnitNode {
        public CompilationUnitNode() {
            NamespaceList = new List<NamespaceNode>();
        }
        public readonly List<NamespaceNode> NamespaceList;
    }
    //internal sealed class LogicalNamespaceMap : Dictionary<string, LogicalNamespace> {
    //}
    //internal sealed class LogicalNamespace {
    //    public LogicalNamespace() {
    //        NamespaceNodeList = new List<NamespaceNode>();
    //    }
    //    public readonly List<NamespaceNode> NamespaceNodeList;
    //    public string Uri {
    //        get { return NamespaceNodeList[0].UriValue; }
    //    }
    //    public NamespaceInfo NamespaceInfo;
    //    public DottedName DottedName {
    //        get { return NamespaceInfo.DottedName; }
    //        set { NamespaceInfo.DottedName = value; }
    //    }
    //    public bool IsRef {
    //        get { return NamespaceInfo.IsRef; }
    //        set { NamespaceInfo.IsRef = value; }
    //    }
    //    public void CheckDuplicateGlobalTypes() {
    //        var list = NamespaceNodeList;
    //        var count = list.Count;
    //        for (var i = 0; i < count - 1; ++i) {
    //            var thisGlobalTypeMap = list[i].GlobalTypeMap;
    //            for (var j = i + 1; j < count; ++j) {
    //                foreach (var otherGlobalTypeName in list[j].GlobalTypeMap.Keys) {
    //                    if (thisGlobalTypeMap.ContainsKey(otherGlobalTypeName)) {
    //                        CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.DuplicateGlobalTypeName, otherGlobalTypeName.Value),
    //                            otherGlobalTypeName.TextSpan);
    //                    }
    //                }
    //                //list[i].CheckDuplicateGlobalTypes(list[j]);
    //            }
    //        }
    //    }
    //    public GlobalTypeNode TryGetGlobalTypeNode(Token name) {
    //        foreach (var ns in NamespaceNodeList) {
    //            GlobalTypeNode gt;
    //            if (ns.GlobalTypeMap.TryGetValue(name, out gt)) {
    //                return gt;
    //            }
    //        }
    //        return null;
    //    }
    //}
    internal sealed class NamespaceNode {
        internal NamespaceNode(Token uri) {
            Uri = uri;
            ImportList = new List<ImportNode>();
            GlobalTypeMap = new Dictionary<Token, GlobalTypeNode>();
        }
        public readonly Token Uri;
        public string UriValue {
            get {
                return Uri.Value;
            }
        }
        public readonly List<ImportNode> ImportList;
        public readonly Dictionary<Token, GlobalTypeNode> GlobalTypeMap;
        public NamespaceInfo NamespaceInfo;
        //
        public void ResolveImports(NamespaceInfoMap nsInfoMap) {
            foreach (var import in ImportList) {
                if (!nsInfoMap.TryGetValue(import.Uri.Value, out import.NamespaceInfo)) {
                    CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.InvalidNamespaceReference, import.Uri.Value),
                        import.Uri.TextSpan);
                }
            }
        }
        //public void CheckDuplicateGlobalTypes(NamespaceNode other) {
        //    foreach (var otherGlobalTypeName in other.GlobalTypeMap.Keys) {
        //        if (GlobalTypeMap.ContainsKey(otherGlobalTypeName)) {
        //            CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.DuplicateGlobalTypeName, otherGlobalTypeName.Value),
        //                otherGlobalTypeName.TextSpan);
        //        }
        //    }
        //}
        public void Resolve() {
            foreach (var globalType in GlobalTypeMap.Values) {
                globalType.Resolve();
            }
        }
        public void CreateInfos() {
            foreach (var globalType in GlobalTypeMap.Values) {
                globalType.CreateInfo();
            }
        }
        public GlobalTypeNode ResolveQName(QualifiableNameNode qName) {
            GlobalTypeNode result = null;
            var name = qName.Name;
            if (qName.IsQualified) {
                var alias = qName.Alias;
                if (alias.Value == "sys") {
                    result = AtomTypeNode.TryGet(name.Value);
                }
                else {
                    ImportNode import = null;
                    foreach (var item in ImportList) {
                        if (item.Alias == alias) {
                            import = item;
                            break;
                        }
                    }
                    if (import == null) {
                        CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.InvalidNamespaceAliasReference, alias.Value),
                            alias.TextSpan);
                    }
                    result = import.NamespaceInfo.TryGetGlobalTypeNode(name);
                }
            }
            else {
                result = NamespaceInfo.TryGetGlobalTypeNode(name);
                if (result == null) {
                    result = AtomTypeNode.TryGet(name.Value);
                    foreach (var item in ImportList) {
                        var globalType = item.NamespaceInfo.TryGetGlobalTypeNode(name);
                        if (globalType != null) {
                            if (result != null) {
                                CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.AmbiguousGlobalTypeReference, name.Value),
                                    name.TextSpan);
                            }
                            result = globalType;
                        }
                    }
                }
            }
            if (result == null) {
                CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.InvalidGlobalTypeReference, name.Value), name.TextSpan);
            }
            return result;
        }
        public ClassTypeNode ResolveQNameAsClass(QualifiableNameNode qName) {
            var result = ResolveQName(qName) as ClassTypeNode;
            if (result == null) {
                CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.InvalidClassReference, qName.ToString()),
                    qName.TextSpan);
            }
            return result;
        }
        public AtomTypeNode ResolveQNameAsAtom(QualifiableNameNode qName) {
            var result = ResolveQName(qName) as AtomTypeNode;
            if (result == null) {
                CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.InvalidAtomReference, qName.ToString()),
                    qName.TextSpan);
            }
            return result;
        }
        public SimpleGlobalTypeNode ResolveQNameAsSimpleGlobalType(QualifiableNameNode qName) {
            var result = ResolveQName(qName) as SimpleGlobalTypeNode;
            if (result == null) {
                CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.InvalidSimpleGlobalTypeReference, qName.ToString()),
                    qName.TextSpan);
            }
            return result;
        }

    }
    internal sealed class ImportNode {
        public ImportNode(Token uri, Token alias) {
            Uri = uri;
            Alias = alias;
            NamespaceInfo = null;
        }
        public readonly Token Uri;
        public readonly Token Alias;//opt
        public NamespaceInfo NamespaceInfo;
    }
    internal struct QualifiableNameNode {
        public QualifiableNameNode(Token alias, Token name) {
            Alias = alias;
            Name = name;
        }
        public readonly Token Alias;//opt
        public readonly Token Name;
        public bool IsQualified {
            get {
                return Alias.IsValid;
            }
        }
        public bool IsValid {
            get {
                return Name.IsValid;
            }
        }
        public TextSpan TextSpan {
            get {
                return Name.TextSpan;
            }
        }
        public override string ToString() {
            if (IsQualified) {
                return Alias.Value + "::" + Name.Value;
            }
            return Name.Value;
        }
    }
    internal abstract class NamespaceDescendantNode {
        protected NamespaceDescendantNode(NamespaceNode ns) {
            Namespace = ns;
        }
        public readonly NamespaceNode Namespace;
    }

    internal abstract class GlobalTypeNode : NamespaceDescendantNode {
        public GlobalTypeNode(NamespaceNode ns, Token name)
            : base(ns) {
            Name = name;
        }
        public readonly Token Name;
        public abstract void Resolve();
        protected GlobalTypeInfo _info;
        private bool _isProcessing;
        public GlobalTypeInfo CreateInfo() {
            if (_info == null) {
                if (_isProcessing) {
                    CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.CircularReferenceNotAllowed), Name.TextSpan);
                }
                _isProcessing = true;
                var nsInfo = Namespace.NamespaceInfo;
                var info = CreateInfoCore(nsInfo);
                nsInfo.GlobalTypeMap.Add(info.Name, info);
                _info = info;
                _isProcessing = false;
            }
            return _info;
        }
        protected abstract GlobalTypeInfo CreateInfoCore(NamespaceInfo nsInfo);
    }
    internal abstract class SimpleGlobalTypeNode : GlobalTypeNode {
        protected SimpleGlobalTypeNode(NamespaceNode ns, Token name) : base(ns, name) { }
    }

    internal sealed class AtomTypeNode : SimpleGlobalTypeNode {
        private static readonly Dictionary<string, AtomTypeNode> _map;
        static AtomTypeNode() {
            _map = new Dictionary<string, AtomTypeNode>();
            for (var kind = AtomExtensionsEx.AtomTypeStart; kind <= AtomExtensionsEx.AtomTypeEnd; ++kind) {
                _map.Add(kind.ToString(), new AtomTypeNode(AtomTypeInfo.Get(kind)));
            }
        }
        public static AtomTypeNode TryGet(string name) {
            AtomTypeNode result;
            _map.TryGetValue(name, out result);
            return result;
        }
        private AtomTypeNode(AtomTypeInfo info)
            : base(null, default(Token)) {
            _info = info;
        }
        public override void Resolve() {
            throw new NotImplementedException();
        }
        protected override GlobalTypeInfo CreateInfoCore(NamespaceInfo nsInfo) {
            throw new NotImplementedException();
        }
    }
    internal sealed class EnumTypeNode : SimpleGlobalTypeNode {
        public EnumTypeNode(NamespaceNode ns, Token name, QualifiableNameNode underlyingTypeQName)
            : base(ns, name) {
            UnderlyingTypeQName = underlyingTypeQName;
            MemberMap = new Dictionary<Token, Token>();
        }
        public readonly QualifiableNameNode UnderlyingTypeQName;
        public AtomTypeNode UnderlyingType;
        public readonly Dictionary<Token, Token> MemberMap;
        public override void Resolve() {
            UnderlyingType = Namespace.ResolveQNameAsAtom(UnderlyingTypeQName);
        }
        protected override GlobalTypeInfo CreateInfoCore(NamespaceInfo nsInfo) {
            var underlyingTypeInfo = (AtomTypeInfo)UnderlyingType.CreateInfo();
            var memberInfoMap = new Dictionary<string, object>();
            var typeKind = underlyingTypeInfo.Kind;
            foreach (var kv in MemberMap) {
                var valueToken = kv.Value;
                var value = AtomExtensions.TryParse(typeKind, valueToken.Value, true);
                if (value == null) {
                    CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.InvalidAtomValue, typeKind.ToString(), valueToken.Value),
                        valueToken.TextSpan);
                }
                memberInfoMap.Add(kv.Key.Value, value);
            }
            return new EnumTypeInfo(nsInfo, Name.Value, underlyingTypeInfo, memberInfoMap);
        }
    }
    //internal sealed class EnumTypeMemberNode {
    //    public EnumTypeMemberNode(Token name, Token value) {
    //        Name = name;
    //        Value = value;
    //    }
    //    public readonly Token Name;
    //    public readonly Token Value;
    //    public NameValuePair CreateInfo(TypeKind typeKind) {
    //        var avNode = Value;
    //        var value = AtomExtensions.TryParse(typeKind, avNode.Value, true);
    //        if (value == null) {
    //            CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.InvalidAtomValue, typeKind.ToString(), avNode.Value),
    //                avNode.TextSpan);
    //        }
    //        return new NameValuePair(Name.Value, value);
    //    }
    //}

    internal sealed class ClassTypeNode : GlobalTypeNode {
        public ClassTypeNode(NamespaceNode ns, Token name, Token abstractOrSealed, QualifiableNameNode baseClassQName)
            : base(ns, name) {
            AbstractOrSealed = abstractOrSealed;
            BaseClassQName = baseClassQName;
            PropertyMap = new Dictionary<Token, LocalTypeNode>();
        }
        public readonly Token AbstractOrSealed;
        public bool IsAbstract {
            get { return AbstractOrSealed.Value == ParserConstants.AbstractKeyword; }
        }
        public bool IsSealed {
            get { return AbstractOrSealed.Value == ParserConstants.SealedKeyword; }
        }
        public readonly QualifiableNameNode BaseClassQName;//opt
        public ClassTypeNode BaseClass;
        public readonly Dictionary<Token, LocalTypeNode> PropertyMap;
        public override void Resolve() {
            if (BaseClassQName.IsValid) {
                BaseClass = Namespace.ResolveQNameAsClass(BaseClassQName);
            }
            foreach (var type in PropertyMap.Values) {
                type.Resolve();
            }
        }
        protected override GlobalTypeInfo CreateInfoCore(NamespaceInfo nsInfo) {
            ClassTypeInfo baseClassInfo = null;
            if (BaseClass != null) {
                baseClassInfo = (ClassTypeInfo)BaseClass.CreateInfo();
                if (baseClassInfo.IsSealed) {
                    CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.BaseClassIsSealed), BaseClassQName.TextSpan);
                }
            }
            var propInfoMap = new Dictionary<string, ClassTypePropertyInfo>();
            foreach (var kv in PropertyMap) {
                var nameToken = kv.Key;
                if (baseClassInfo != null && baseClassInfo.TryGetPropertyInHierarchy(nameToken.Value) != null) {
                    CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.DuplicatePropertyName, nameToken.Value),
                        nameToken.TextSpan);
                }
                propInfoMap.Add(nameToken.Value, new ClassTypePropertyInfo(nameToken.Value, kv.Value.CreateInfo()));
            }
            return new ClassTypeInfo(nsInfo, Name.Value, IsAbstract, IsSealed, baseClassInfo, propInfoMap);
        }
    }
    //internal sealed class PropertyNode : NamespaceDescendantNode {
    //    public PropertyNode(NamespaceNode ns, Token name, LocalTypeNode type)
    //        : base(ns) {
    //        Name = name;
    //        Type = type;
    //    }
    //    public readonly Token Name;
    //    public readonly LocalTypeNode Type;
    //    public void Resolve() {
    //        Type.Resolve();
    //    }
    //    public PropertyInfo CreateInfo() {
    //        return new PropertyInfo(Name.Value, Type.CreateInfo());
    //    }
    //}

    internal abstract class LocalTypeNode : NamespaceDescendantNode {
        protected LocalTypeNode(NamespaceNode ns, TextSpan textSpan)
            : base(ns) {
            TextSpan = textSpan;
        }
        public readonly TextSpan TextSpan;
        public abstract void Resolve();
        public abstract LocalTypeInfo CreateInfo();
    }
    //nullable<element>
    internal sealed class NullableTypeNode : LocalTypeNode {
        public NullableTypeNode(NamespaceNode ns, TextSpan textSpan, NonNullableTypeNode element)
            : base(ns, textSpan) {
            Element = element;
        }
        public readonly NonNullableTypeNode Element;
        public override void Resolve() {
            Element.Resolve();
        }
        public override LocalTypeInfo CreateInfo() {
            return new NullableTypeInfo((NonNullableTypeInfo)Element.CreateInfo());
        }
    }
    internal abstract class NonNullableTypeNode : LocalTypeNode {
        protected NonNullableTypeNode(NamespaceNode ns, TextSpan textSpan)
            : base(ns, textSpan) {
        }
    }
    internal sealed class GlobalTypeRefNode : NonNullableTypeNode {
        public GlobalTypeRefNode(NamespaceNode ns, TextSpan textSpan, QualifiableNameNode globalTypeQName)
            : base(ns, textSpan) {
            GlobalTypeQName = globalTypeQName;
        }
        public readonly QualifiableNameNode GlobalTypeQName;
        public GlobalTypeNode GlobalType;
        public bool IsClass {
            get {
                return GlobalType is ClassTypeNode;
            }
        }
        public bool IsSimpleGlobalType {
            get {
                return GlobalType is SimpleGlobalTypeNode;
            }
        }
        public override void Resolve() {
            GlobalType = Namespace.ResolveQName(GlobalTypeQName);
        }
        public override LocalTypeInfo CreateInfo() {
            return new GlobalTypeRefInfo(GlobalType.CreateInfo());
        }
    }
    //list<item>
    internal sealed class ListTypeNode : NonNullableTypeNode {
        public ListTypeNode(NamespaceNode ns, TextSpan textSpan, LocalTypeNode item)
            : base(ns, textSpan) {
            Item = item;
        }
        public readonly LocalTypeNode Item;
        public override void Resolve() {
            Item.Resolve();
        }
        public override LocalTypeInfo CreateInfo() {
            return new CollectionInfo(TypeKind.List, Item.CreateInfo(), null, null);
        }
    }
    //map<key, value>
    internal sealed class MapTypeNode : NonNullableTypeNode {
        public MapTypeNode(NamespaceNode ns, TextSpan textSpan, GlobalTypeRefNode key, LocalTypeNode value)
            : base(ns, textSpan) {
            Key = key;
            Value = value;
        }
        public readonly GlobalTypeRefNode Key;
        public readonly LocalTypeNode Value;
        public override void Resolve() {
            Key.Resolve();
            if (!Key.IsSimpleGlobalType) {
                CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.InvalidSimpleGlobalTypeReference, Key.GlobalTypeQName.ToString()),
                    Key.GlobalTypeQName.TextSpan);
            }
            Value.Resolve();
        }
        public override LocalTypeInfo CreateInfo() {
            return new CollectionInfo(TypeKind.Map, Value.CreateInfo(), (GlobalTypeRefInfo)Key.CreateInfo(), null);
        }
    }
    //set<Class1 \ Prop1.Prop2>
    //set<Int32>
    internal sealed class SetTypeNode : NonNullableTypeNode {
        public SetTypeNode(NamespaceNode ns, TextSpan textSpan, GlobalTypeRefNode item,
            List<Token> keyNameList, TextSpan closeTextSpan)
            : base(ns, textSpan) {
            Item = item;
            KeyNameList = keyNameList;
            CloseTextSpan = closeTextSpan;
        }
        public readonly GlobalTypeRefNode Item;
        public readonly List<Token> KeyNameList;//opt
        public readonly TextSpan CloseTextSpan;
        public bool IsObjectSet {
            get {
                return KeyNameList != null;
            }
        }
        public override void Resolve() {
            Item.Resolve();
            if (Item.IsSimpleGlobalType) {
                if (KeyNameList != null) {
                    CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.KeySelectorNotAllowedForSimpleSet), KeyNameList[0].TextSpan);
                }
            }
            else if (KeyNameList == null) {
                CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.KeySelectorRequiredForObjectSet), CloseTextSpan);
            }
        }
        public override LocalTypeInfo CreateInfo() {
            var itemTypeInfo = Item.CreateInfo();
            ObjectSetKeySelectorInfo selector = null;
            var isObjectSet = IsObjectSet;
            if (isObjectSet) {
                selector = new ObjectSetKeySelectorInfo();
                var globalTypeRefInfo = (GlobalTypeRefInfo)itemTypeInfo;
                var keyCount = KeyNameList.Count;
                for (var i = 0; i < keyCount; ++i) {
                    var keyName = KeyNameList[i];
                    var propInfo = globalTypeRefInfo.ClassType.TryGetPropertyInHierarchy(keyName.Value);
                    if (propInfo == null) {
                        CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.InvalidPropertyReference, keyName.Value), keyName.TextSpan);
                    }
                    var propTypeInfo = propInfo.Type;
                    if (propTypeInfo.IsNullable) {
                        CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.ObjectSetKeyCannotBeNullable), keyName.TextSpan);
                    }
                    if (propTypeInfo.Kind.IsSimple()) {
                        if (i < keyCount - 1) {
                            CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.InvalidObjectSetKey), KeyNameList[i + 1].TextSpan);
                        }
                        selector.Add(propInfo);
                    }
                    else if (propTypeInfo.Kind == TypeKind.Class) {
                        if (i == keyCount - 1) {
                            CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.ObjectSetKeyMustBeSimpleType), keyName.TextSpan);
                        }
                        selector.Add(propInfo);
                        globalTypeRefInfo = (GlobalTypeRefInfo)propTypeInfo;
                    }
                    else {
                        CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.ObjectSetKeyMustBeSimpleType), keyName.TextSpan);
                    }
                }
            }
            return new CollectionInfo(isObjectSet ? TypeKind.ObjectSet : TypeKind.SimpleSet, itemTypeInfo, null, selector);
        }
    }


}
