using System;
using System.Collections.Generic;
using SData.Internal;

namespace SData.Compiler
{
    internal sealed class CompilationUnitNode
    {
        public CompilationUnitNode()
        {
            NamespaceList = new List<NamespaceNode>();
        }
        public readonly List<NamespaceNode> NamespaceList;
    }
    internal sealed class NamespaceNode
    {
        internal NamespaceNode(Token uri)
        {
            Uri = uri;
            ImportList = new List<ImportNode>();
            GlobalTypeMap = new Dictionary<Token, GlobalTypeNode>();
        }
        public readonly Token Uri;
        public string UriValue
        {
            get
            {
                return Uri.Value;
            }
        }
        public readonly List<ImportNode> ImportList;
        public readonly Dictionary<Token, GlobalTypeNode> GlobalTypeMap;
        public NamespaceInfo NamespaceInfo;
        //
        public void ResolveImports(NamespaceInfoMap nsInfoMap)
        {
            foreach (var import in ImportList)
            {
                if (!nsInfoMap.TryGetValue(import.Uri.Value, out import.NamespaceInfo))
                {
                    CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.InvalidNamespaceReference, import.Uri.Value),
                        import.Uri.TextSpan);
                }
            }
        }
        public void Resolve()
        {
            foreach (var globalType in GlobalTypeMap.Values)
            {
                globalType.Resolve();
            }
        }
        public void CreateInfos()
        {
            foreach (var globalType in GlobalTypeMap.Values)
            {
                globalType.CreateInfo();
            }
        }
        public GlobalTypeNode ResolveQName(QualifiableNameNode qName)
        {
            GlobalTypeNode result = null;
            var name = qName.Name;
            if (qName.IsQualified)
            {
                var alias = qName.Alias;
                if (alias.Value == "sys")
                {
                    result = AtomTypeNode.TryGet(name.Value);
                }
                else
                {
                    ImportNode import = null;
                    foreach (var item in ImportList)
                    {
                        if (item.Alias == alias)
                        {
                            import = item;
                            break;
                        }
                    }
                    if (import == null)
                    {
                        CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.InvalidNamespaceAliasReference, alias.Value),
                            alias.TextSpan);
                    }
                    result = import.NamespaceInfo.TryGetGlobalTypeNode(name);
                }
            }
            else
            {
                result = NamespaceInfo.TryGetGlobalTypeNode(name);
                if (result == null)
                {
                    result = AtomTypeNode.TryGet(name.Value);
                    foreach (var import in ImportList)
                    {
                        var globalType = import.NamespaceInfo.TryGetGlobalTypeNode(name);
                        if (globalType != null)
                        {
                            if (result != null)
                            {
                                CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.AmbiguousGlobalTypeReference, name.Value),
                                    name.TextSpan);
                            }
                            result = globalType;
                        }
                    }
                }
            }
            if (result == null)
            {
                CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.InvalidGlobalTypeReference, name.Value), name.TextSpan);
            }
            return result;
        }
        public ClassTypeNode ResolveQNameAsClass(QualifiableNameNode qName)
        {
            var result = ResolveQName(qName) as ClassTypeNode;
            if (result == null)
            {
                CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.InvalidClassReference, qName.Name.Value),
                    qName.TextSpan);
            }
            return result;
        }
        public AtomTypeNode ResolveQNameAsAtom(QualifiableNameNode qName)
        {
            var result = ResolveQName(qName) as AtomTypeNode;
            if (result == null)
            {
                CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.InvalidAtomReference, qName.Name.Value),
                    qName.TextSpan);
            }
            return result;
        }
    }
    internal sealed class ImportNode
    {
        public ImportNode(Token uri, Token alias)
        {
            Uri = uri;
            Alias = alias;
            NamespaceInfo = null;
        }
        public readonly Token Uri;
        public readonly Token Alias;//opt
        public NamespaceInfo NamespaceInfo;
    }
    internal struct QualifiableNameNode
    {
        public QualifiableNameNode(Token alias, Token name)
        {
            Alias = alias;
            Name = name;
        }
        public readonly Token Alias;//opt
        public readonly Token Name;
        public bool IsQualified
        {
            get
            {
                return Alias.IsValid;
            }
        }
        public bool IsValid
        {
            get
            {
                return Name.IsValid;
            }
        }
        public TextSpan TextSpan
        {
            get
            {
                return Name.TextSpan;
            }
        }
        public override string ToString()
        {
            if (IsQualified)
            {
                return Alias.Value + "::" + Name.Value;
            }
            return Name.Value;
        }
    }
    internal abstract class NamespaceDescendantNode
    {
        protected NamespaceDescendantNode(NamespaceNode ns)
        {
            Namespace = ns;
        }
        public readonly NamespaceNode Namespace;
    }

    internal abstract class GlobalTypeNode : NamespaceDescendantNode
    {
        public GlobalTypeNode(NamespaceNode ns, Token name)
            : base(ns)
        {
            Name = name;
        }
        public readonly Token Name;
        public abstract void Resolve();
        protected GlobalTypeInfo _info;
        private bool _isProcessing;
        public GlobalTypeInfo CreateInfo()
        {
            if (_info == null)
            {
                if (_isProcessing)
                {
                    CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.CircularReferenceNotAllowed), Name.TextSpan);
                }
                _isProcessing = true;
                var nsInfo = Namespace.NamespaceInfo;
                var info = _info = CreateInfoCore(nsInfo);
                nsInfo.GlobalTypeMap.Add(info.Name, info);
                _isProcessing = false;
            }
            return _info;
        }
        protected abstract GlobalTypeInfo CreateInfoCore(NamespaceInfo nsInfo);
    }
    internal abstract class SimpleGlobalTypeNode : GlobalTypeNode
    {
        protected SimpleGlobalTypeNode(NamespaceNode ns, Token name) : base(ns, name) { }
    }

    internal sealed class AtomTypeNode : SimpleGlobalTypeNode
    {
        private static readonly Dictionary<string, AtomTypeNode> _map;
        static AtomTypeNode()
        {
            _map = new Dictionary<string, AtomTypeNode>();
            for (var kind = AtomExtensionsEx.AtomTypeStart; kind <= AtomExtensionsEx.AtomTypeEnd; ++kind)
            {
                _map.Add(kind.ToString(), new AtomTypeNode(AtomTypeInfo.Get(kind)));
            }
        }
        public static AtomTypeNode TryGet(string name)
        {
            AtomTypeNode result;
            _map.TryGetValue(name, out result);
            return result;
        }
        private AtomTypeNode(AtomTypeInfo info)
            : base(null, default(Token))
        {
            _info = info;
        }
        public override void Resolve()
        {
            throw new NotImplementedException();
        }
        protected override GlobalTypeInfo CreateInfoCore(NamespaceInfo nsInfo)
        {
            throw new NotImplementedException();
        }
    }
    internal sealed class EnumTypeNode : SimpleGlobalTypeNode
    {
        public EnumTypeNode(NamespaceNode ns, Token name, QualifiableNameNode underlyingTypeQName)
            : base(ns, name)
        {
            UnderlyingTypeQName = underlyingTypeQName;
            MemberMap = new Dictionary<Token, Token>();
        }
        public readonly QualifiableNameNode UnderlyingTypeQName;
        public AtomTypeNode UnderlyingType;
        public readonly Dictionary<Token, Token> MemberMap;
        public override void Resolve()
        {
            UnderlyingType = Namespace.ResolveQNameAsAtom(UnderlyingTypeQName);
        }
        protected override GlobalTypeInfo CreateInfoCore(NamespaceInfo nsInfo)
        {
            var underlyingTypeInfo = (AtomTypeInfo)UnderlyingType.CreateInfo();
            var memberInfoMap = new Dictionary<string, object>();
            var typeKind = underlyingTypeInfo.Kind;
            foreach (var kv in MemberMap)
            {
                var valueToken = kv.Value;
                var value = AtomExtensions.TryParse(typeKind, valueToken.Value, true);
                if (value == null)
                {
                    CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.InvalidAtomValue, typeKind.ToString(), valueToken.Value),
                        valueToken.TextSpan);
                }
                memberInfoMap.Add(kv.Key.Value, value);
            }
            return new EnumTypeInfo(nsInfo, Name.Value, underlyingTypeInfo, memberInfoMap);
        }
    }
    internal sealed class ClassTypeNode : GlobalTypeNode
    {
        public ClassTypeNode(NamespaceNode ns, Token name, Token abstractOrSealed, QualifiableNameNode baseClassQName, List<KeyNode> keyList)
            : base(ns, name)
        {
            AbstractOrSealed = abstractOrSealed;
            BaseClassQName = baseClassQName;
            KeyList = keyList;
            PropertyMap = new Dictionary<Token, LocalTypeNode>();
        }
        public readonly Token AbstractOrSealed;
        public bool IsAbstract
        {
            get { return AbstractOrSealed.Value == ParserConstants.AbstractKeyword; }
        }
        public bool IsSealed
        {
            get { return AbstractOrSealed.Value == ParserConstants.SealedKeyword; }
        }
        public readonly QualifiableNameNode BaseClassQName;//opt
        public ClassTypeNode BaseClass;
        public readonly List<KeyNode> KeyList;//opt
        public readonly Dictionary<Token, LocalTypeNode> PropertyMap;
        public override void Resolve()
        {
            if (BaseClassQName.IsValid)
            {
                BaseClass = Namespace.ResolveQNameAsClass(BaseClassQName);
            }
            foreach (var type in PropertyMap.Values)
            {
                type.Resolve();
            }
        }
        protected override GlobalTypeInfo CreateInfoCore(NamespaceInfo nsInfo)
        {
            ClassTypeInfo baseClassInfo = null;
            if (BaseClass != null)
            {
                baseClassInfo = (ClassTypeInfo)BaseClass.CreateInfo();
                if (baseClassInfo.IsSealed)
                {
                    CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.BaseClassIsSealed), BaseClassQName.TextSpan);
                }
            }
            var propInfoMap = new Dictionary<string, PropertyInfo>();
            foreach (var kv in PropertyMap)
            {
                var nameToken = kv.Key;
                if (baseClassInfo != null && baseClassInfo.TryGetPropertyInHierarchy(nameToken.Value) != null)
                {
                    CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.DuplicatePropertyName, nameToken.Value),
                        nameToken.TextSpan);
                }
                propInfoMap.Add(nameToken.Value, new PropertyInfo(nameToken.Value, kv.Value.CreateInfo()));
            }
            return new ClassTypeInfo(nsInfo, Name.Value, IsAbstract, IsSealed, baseClassInfo, KeyList, propInfoMap);
        }
    }
    internal sealed class KeyNode : List<Token>
    {
    }

    internal abstract class LocalTypeNode : NamespaceDescendantNode
    {
        protected LocalTypeNode(NamespaceNode ns, TextSpan textSpan)
            : base(ns)
        {
            TextSpan = textSpan;
        }
        public readonly TextSpan TextSpan;
        public abstract void Resolve();
        public abstract LocalTypeInfo CreateInfo();
    }
    //nullable<element>
    internal sealed class NullableTypeNode : LocalTypeNode
    {
        public NullableTypeNode(NamespaceNode ns, TextSpan textSpan, NonNullableTypeNode element)
            : base(ns, textSpan)
        {
            Element = element;
        }
        public readonly NonNullableTypeNode Element;
        public override void Resolve()
        {
            Element.Resolve();
        }
        public override LocalTypeInfo CreateInfo()
        {
            return new NullableTypeInfo((NonNullableTypeInfo)Element.CreateInfo());
        }
    }
    internal abstract class NonNullableTypeNode : LocalTypeNode
    {
        protected NonNullableTypeNode(NamespaceNode ns, TextSpan textSpan)
            : base(ns, textSpan)
        {
        }
    }
    internal sealed class GlobalTypeRefNode : NonNullableTypeNode
    {
        public GlobalTypeRefNode(NamespaceNode ns, TextSpan textSpan, QualifiableNameNode globalTypeQName)
            : base(ns, textSpan)
        {
            GlobalTypeQName = globalTypeQName;
        }
        public readonly QualifiableNameNode GlobalTypeQName;
        public GlobalTypeNode GlobalType;
        public override void Resolve()
        {
            GlobalType = Namespace.ResolveQName(GlobalTypeQName);
        }
        public override LocalTypeInfo CreateInfo()
        {
            return new GlobalTypeRefInfo(GlobalType.CreateInfo());
        }
    }
    //list<item>, set<item>
    internal sealed class ListOrSetTypeNode : NonNullableTypeNode
    {
        public ListOrSetTypeNode(NamespaceNode ns, TextSpan textSpan, bool isList, LocalTypeNode item)
            : base(ns, textSpan)
        {
            IsList = isList;
            Item = item;
        }
        public readonly bool IsList;
        public readonly LocalTypeNode Item;
        public override void Resolve()
        {
            Item.Resolve();
        }
        public override LocalTypeInfo CreateInfo()
        {
            var itemInfo = Item.CreateInfo();
            if (!IsList)
            {
                var clsInfo = ((GlobalTypeRefInfo)itemInfo).ClassType;
                if (clsInfo != null && !clsInfo.HasKey)
                {
                    CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.KeyRequiredForClassUsedAsSetItemOrMapKey),
                        Item.TextSpan);
                }
            }
            return new CollectionInfo(IsList ? TypeKind.List : TypeKind.Set, itemInfo, null);
        }
    }

    //map<key, value>
    internal sealed class MapTypeNode : NonNullableTypeNode
    {
        public MapTypeNode(NamespaceNode ns, TextSpan textSpan, GlobalTypeRefNode key, LocalTypeNode value)
            : base(ns, textSpan)
        {
            Key = key;
            Value = value;
        }
        public readonly GlobalTypeRefNode Key;
        public readonly LocalTypeNode Value;
        public override void Resolve()
        {
            Key.Resolve();
            Value.Resolve();
        }
        public override LocalTypeInfo CreateInfo()
        {
            var keyInfo = (GlobalTypeRefInfo)Key.CreateInfo();
            var clsInfo = keyInfo.ClassType;
            if (clsInfo != null && !clsInfo.HasKey)
            {
                CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.KeyRequiredForClassUsedAsSetItemOrMapKey),
                    Key.TextSpan);
            }
            return new CollectionInfo(TypeKind.Map, Value.CreateInfo(), keyInfo);
        }
    }


}
