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
    internal sealed class LogicalNamespaceMap : Dictionary<string, LogicalNamespace> {
    }
    internal sealed class LogicalNamespace : List<NamespaceNode> {
        public string Uri {
            get { return this[0].UriValue; }
        }
        public NamespaceInfo NamespaceInfo;
        public DottedName DottedName {
            get { return NamespaceInfo.DottedName; }
            set { NamespaceInfo.DottedName = value; }
        }
        public bool IsRef {
            get { return NamespaceInfo.IsRef; }
            set { NamespaceInfo.IsRef = value; }
        }
        public void CheckDuplicateGlobalTypes() {
            var count = Count;
            for (var i = 0; i < count - 1; ++i) {
                for (var j = i + 1; j < count; ++j) {
                    this[i].CheckDuplicateGlobalTypes(this[j].GlobalTypeList);
                }
            }
        }
        public GlobalTypeNode TryGetGlobalType(Token name) {
            var count = Count;
            for (var i = 0; i < count; ++i) {
                foreach (var globalType in this[i].GlobalTypeList) {
                    if (globalType.Name == name) {
                        return globalType;
                    }
                }
            }
            return null;
        }
    }
    internal sealed class NamespaceNode {
        internal NamespaceNode(Token uri) {
            Uri = uri;
            ImportList = new List<ImportNode>();
            GlobalTypeList = new List<GlobalTypeNode>();
        }
        public readonly Token Uri;
        public string UriValue {
            get {
                return Uri.Value;
            }
        }
        public readonly List<ImportNode> ImportList;
        public readonly List<GlobalTypeNode> GlobalTypeList;
        public LogicalNamespace LogicalNamespace;
        //
        public void ResolveImports(LogicalNamespaceMap nsMap) {
            foreach (var import in ImportList) {
                if (!nsMap.TryGetValue(import.Uri.Value, out import.LogicalNamespace)) {
                    ParsingContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.InvalidNamespaceReference, import.Uri.Value),
                        import.Uri.TextSpan);
                }
            }
        }
        public void CheckDuplicateGlobalTypes(List<GlobalTypeNode> otherGlobalTypeList) {
            foreach (var thisGlobalType in GlobalTypeList) {
                foreach (var otherGlobalType in otherGlobalTypeList) {
                    if (thisGlobalType.Name == otherGlobalType.Name) {
                        ParsingContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.DuplicateGlobalTypeName, otherGlobalType.Name.Value),
                            otherGlobalType.Name.TextSpan);
                    }
                }
            }
        }
        public void Resolve() {
            foreach (var globalType in GlobalTypeList) {
                globalType.Resolve();
            }
        }
        public void CreateInfos() {
            foreach (var globalType in GlobalTypeList) {
                globalType.CreateInfo();
            }
        }
        public GlobalTypeNode ResolveQName(QualifiableNameNode qName) {
            GlobalTypeNode result = null;
            var name = qName.Name;
            if (qName.IsQualified) {
                var alias = qName.Alias;
                if (alias.Value == "sys") {
                    result = AtomNode.TryGet(name.Value);
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
                        ParsingContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.InvalidNamespaceAliasReference, alias.Value),
                            alias.TextSpan);
                    }
                    result = import.LogicalNamespace.TryGetGlobalType(name);
                }
            }
            else {
                result = LogicalNamespace.TryGetGlobalType(name);
                if (result == null) {
                    result = AtomNode.TryGet(name.Value);
                    foreach (var item in ImportList) {
                        var globalType = item.LogicalNamespace.TryGetGlobalType(name);
                        if (globalType != null) {
                            if (result != null) {
                                ParsingContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.AmbiguousGlobalTypeReference, name.Value),
                                    name.TextSpan);
                            }
                            result = globalType;
                        }
                    }
                }
            }
            if (result == null) {
                ParsingContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.InvalidGlobalTypeReference, name.Value), name.TextSpan);
            }
            return result;
        }
        public ClassNode ResolveQNameAsClass(QualifiableNameNode qName) {
            var result = ResolveQName(qName) as ClassNode;
            if (result == null) {
                ParsingContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.InvalidClassReference, qName.ToString()),
                    qName.TextSpan);
            }
            return result;
        }
        public AtomNode ResolveQNameAsAtom(QualifiableNameNode qName) {
            var result = ResolveQName(qName) as AtomNode;
            if (result == null) {
                ParsingContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.InvalidAtomReference, qName.ToString()),
                    qName.TextSpan);
            }
            return result;
        }
        public SimpleGlobalTypeNode ResolveQNameAsSimpleGlobalType(QualifiableNameNode qName) {
            var result = ResolveQName(qName) as SimpleGlobalTypeNode;
            if (result == null) {
                ParsingContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.InvalidSimpleGlobalTypeReference, qName.ToString()),
                    qName.TextSpan);
            }
            return result;
        }

    }
    internal sealed class ImportNode {
        public ImportNode(Token uri, Token alias) {
            Uri = uri;
            Alias = alias;
            LogicalNamespace = null;
        }
        public readonly Token Uri;
        public readonly Token Alias;//opt
        public LogicalNamespace LogicalNamespace;
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
                    ParsingContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.CircularReferenceNotAllowed), Name.TextSpan);
                }
                _isProcessing = true;
                var nsInfo = Namespace.LogicalNamespace.NamespaceInfo;
                var info = CreateInfoCore(nsInfo);
                nsInfo.GlobalTypeList.Add(info);
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

    internal sealed class AtomNode : SimpleGlobalTypeNode {
        private static readonly Dictionary<string, AtomNode> _map;
        static AtomNode() {
            _map = new Dictionary<string, AtomNode>();
            for (var kind = AtomExtensionsEx.AtomTypeStart; kind <= AtomExtensionsEx.AtomTypeEnd; ++kind) {
                _map.Add(kind.ToString(), new AtomNode(AtomInfo.Get(kind)));
            }
        }
        public static AtomNode TryGet(string name) {
            AtomNode result;
            _map.TryGetValue(name, out result);
            return result;
        }
        private AtomNode(AtomInfo info)
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
    internal sealed class EnumNode : SimpleGlobalTypeNode {
        public EnumNode(NamespaceNode ns, Token name, QualifiableNameNode atomQName)
            : base(ns, name) {
            AtomQName = atomQName;
            MemberList = new List<EnumMemberNode>();
        }
        public readonly QualifiableNameNode AtomQName;
        public AtomNode Atom;
        public readonly List<EnumMemberNode> MemberList;
        public override void Resolve() {
            Atom = Namespace.ResolveQNameAsAtom(AtomQName);
        }
        protected override GlobalTypeInfo CreateInfoCore(NamespaceInfo nsInfo) {
            var atomInfo = (AtomInfo)Atom.CreateInfo();
            var memberInfoList = new List<NameValuePair>();
            var kind = atomInfo.Kind;
            foreach (var member in MemberList) {
                memberInfoList.Add(member.CreateInfo(kind));
            }
            return new EnumInfo(nsInfo, Name.Value, atomInfo, memberInfoList);
        }
    }
    internal sealed class EnumMemberNode {
        public EnumMemberNode(Token name, Token value) {
            Name = name;
            Value = value;
        }
        public readonly Token Name;
        public readonly Token Value;
        public NameValuePair CreateInfo(TypeKind typeKind) {
            var avNode = Value;
            var value = AtomExtensionsEx.TryParse(typeKind, avNode.Value, true);
            if (value == null) {
                ParsingContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.InvalidAtomValue, typeKind.ToString(), avNode.Value),
                    avNode.TextSpan);
            }
            return new NameValuePair(Name.Value, value);
        }
    }

    internal sealed class ClassNode : GlobalTypeNode {
        public ClassNode(NamespaceNode ns, Token name, Token abstractOrSealed, QualifiableNameNode baseClassQName)
            : base(ns, name) {
            AbstractOrSealed = abstractOrSealed;
            BaseClassQName = baseClassQName;
            PropertyList = new List<PropertyNode>();
        }
        public readonly Token AbstractOrSealed;
        public bool IsAbstract {
            get { return AbstractOrSealed.Value == ParserConstants.AbstractKeyword; }
        }
        public bool IsSealed {
            get { return AbstractOrSealed.Value == ParserConstants.SealedKeyword; }
        }
        public readonly QualifiableNameNode BaseClassQName;//opt
        public ClassNode BaseClass;
        public readonly List<PropertyNode> PropertyList;
        public override void Resolve() {
            if (BaseClassQName.IsValid) {
                BaseClass = Namespace.ResolveQNameAsClass(BaseClassQName);
            }
            foreach (var prop in PropertyList) {
                prop.Resolve();
            }
        }
        protected override GlobalTypeInfo CreateInfoCore(NamespaceInfo nsInfo) {
            ClassInfo baseClassInfo = null;
            if (BaseClass != null) {
                baseClassInfo = (ClassInfo)BaseClass.CreateInfo();
                if (baseClassInfo.IsSealed) {
                    ParsingContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.BaseClassIsSealed), BaseClassQName.TextSpan);
                }
            }
            var propInfoList = new List<PropertyInfo>();
            foreach (var prop in PropertyList) {
                if (baseClassInfo != null && baseClassInfo.GetPropertyInHierarchy(prop.Name.Value) != null) {
                    ParsingContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.DuplicatePropertyName, prop.Name.Value),
                        prop.Name.TextSpan);
                }
                propInfoList.Add(prop.CreateInfo());
            }
            return new ClassInfo(nsInfo, Name.Value, IsAbstract, IsSealed, baseClassInfo, propInfoList);
        }
    }
    internal sealed class PropertyNode : NamespaceDescendantNode {
        public PropertyNode(NamespaceNode ns, Token name, LocalTypeNode type)
            : base(ns) {
            Name = name;
            Type = type;
        }
        public readonly Token Name;
        public readonly LocalTypeNode Type;
        public void Resolve() {
            Type.Resolve();
        }
        public PropertyInfo CreateInfo() {
            return new PropertyInfo(Name.Value, Type.CreateInfo());
        }
    }

    internal abstract class LocalTypeNode : NamespaceDescendantNode {
        protected LocalTypeNode(NamespaceNode ns, TextSpan textSpan)
            : base(ns) {
            TextSpan = textSpan;
        }
        public readonly TextSpan TextSpan;
        public abstract void Resolve();
        public abstract LocalTypeInfo CreateInfo();
    }
    internal sealed class GlobalTypeRefNode : LocalTypeNode {
        public GlobalTypeRefNode(NamespaceNode ns, TextSpan textSpan, QualifiableNameNode globalTypeQName)
            : base(ns, textSpan) {
            GlobalTypeQName = globalTypeQName;
        }
        public readonly QualifiableNameNode GlobalTypeQName;
        public GlobalTypeNode GlobalType;
        public bool IsClass {
            get {
                return GlobalType is ClassNode;
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
            return GlobalType.CreateInfo().CreateGlobalTypeRef();
        }
    }
    //nullable<element>
    internal sealed class NullableNode : LocalTypeNode {
        public NullableNode(NamespaceNode ns, TextSpan textSpan, LocalTypeNode element)
            : base(ns, textSpan) {
            Element = element;
        }
        public readonly LocalTypeNode Element;
        public override void Resolve() {
            Element.Resolve();
        }
        public override LocalTypeInfo CreateInfo() {
            var info = Element.CreateInfo();
            info.IsNullable = true;
            return info;
        }
    }
    //list<item>
    internal sealed class ListNode : LocalTypeNode {
        public ListNode(NamespaceNode ns, TextSpan textSpan, LocalTypeNode item)
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
    internal sealed class MapNode : LocalTypeNode {
        public MapNode(NamespaceNode ns, TextSpan textSpan, GlobalTypeRefNode key, LocalTypeNode value)
            : base(ns, textSpan) {
            Key = key;
            Value = value;
        }
        public readonly GlobalTypeRefNode Key;
        public readonly LocalTypeNode Value;
        public override void Resolve() {
            Key.Resolve();
            if (!Key.IsSimpleGlobalType) {
                ParsingContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.InvalidSimpleGlobalTypeReference, Key.GlobalTypeQName.ToString()),
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
    internal sealed class SetNode : LocalTypeNode {
        public SetNode(NamespaceNode ns, TextSpan textSpan, GlobalTypeRefNode item,
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
                    ParsingContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.KeySelectorNotAllowedForSimpleSet), KeyNameList[0].TextSpan);
                }
            }
            else if (KeyNameList == null) {
                ParsingContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.KeySelectorRequiredForObjectSet), CloseTextSpan);
            }
        }
        public override LocalTypeInfo CreateInfo() {
            var itemTypeInfo = Item.CreateInfo();
            ObjectSetKeySelector selector = null;
            var isObjectSet = IsObjectSet;
            if (isObjectSet) {
                selector = new ObjectSetKeySelector();
                var globalTypeRefInfo = (GlobalTypeRefInfo)itemTypeInfo;
                var keyCount = KeyNameList.Count;
                for (var i = 0; i < keyCount; ++i) {
                    var keyName = KeyNameList[i];
                    var propInfo = globalTypeRefInfo.Class.GetPropertyInHierarchy(keyName.Value);
                    if (propInfo == null) {
                        ParsingContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.InvalidPropertyReference, keyName.Value), keyName.TextSpan);
                    }
                    var propTypeInfo = propInfo.Type;
                    if (propTypeInfo.IsNullable) {
                        ParsingContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.ObjectSetKeyCannotBeNullable), keyName.TextSpan);
                    }
                    if (propTypeInfo.Kind.IsSimple()) {
                        if (i < keyCount - 1) {
                            ParsingContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.InvalidObjectSetKey), KeyNameList[i + 1].TextSpan);
                        }
                        selector.Add(propInfo);
                    }
                    else if (propTypeInfo.Kind == TypeKind.Class) {
                        if (i == keyCount - 1) {
                            ParsingContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.ObjectSetKeyMustBeSimpleType), keyName.TextSpan);
                        }
                        selector.Add(propInfo);
                        globalTypeRefInfo = (GlobalTypeRefInfo)propTypeInfo;
                    }
                    else {
                        ParsingContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.ObjectSetKeyMustBeSimpleType), keyName.TextSpan);
                    }
                }
            }
            return new CollectionInfo(isObjectSet ? TypeKind.ObjectSet : TypeKind.SimpleSet, itemTypeInfo, null, selector);
        }
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
}
