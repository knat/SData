using System;
using System.Collections.Generic;
using System.IO;
using SData.Internal;

namespace SData.Compiler {
    public static class ParserConstants {
        public const string AbstractKeyword = "abstract";
        public const string AsKeyword = "as";
        public const string ClassKeyword = "class";
        public const string EnumKeyword = "enum";
        public const string ExtendsKeyword = "extends";
        public const string ImportKeyword = "import";
        public const string ListKeyword = "list";
        public const string MapKeyword = "map";
        public const string NamespaceKeyword = "namespace";
        public const string NullableKeyword = "nullable";
        public const string SealedKeyword = "sealed";
        public const string SetKeyword = "set";
        public static readonly HashSet<string> KeywordSet = new HashSet<string> {
            AbstractKeyword,
            AsKeyword,
            ClassKeyword,
            EnumKeyword,
            ExtendsKeyword,
            ImportKeyword,
            ListKeyword,
            MapKeyword,
            NamespaceKeyword,
            NullableKeyword,
            SealedKeyword,
            SetKeyword,
            //"true",
            //"false"
        };
    }
    internal sealed class Parser : ParserBase {
        [ThreadStatic]
        private static Parser _instance;
        private static Parser Instance {
            get { return _instance ?? (_instance = new Parser()); }
        }

        public static bool Parse(string filePath, TextReader reader, LoadingContext context, out CompilationUnitNode result) {
            return Instance.CompilationUnit(filePath, reader, context, out result);
        }
        private Parser() {
        }
        private void ErrorAndThrow(DiagMsgEx diagMsg, TextSpan textSpan) {
            Error((int)diagMsg.Code, diagMsg.GetMessage(), textSpan);
            Throw();
        }
        private void ErrorAndThrow(DiagMsgEx diagMsg) {
            ErrorAndThrow(diagMsg, GetToken().TextSpan);
        }
        private bool CompilationUnit(string filePath, TextReader reader, LoadingContext context, out CompilationUnitNode result) {
            try {
                Set(filePath, reader, context);
                var cu = new CompilationUnitNode();
                while (Namespace(cu)) ;
                EndOfFileExpected();
                result = cu;
                return true;
            }
            catch (LoadingException) { }
            finally {
                Clear();
            }
            result = null;
            return false;
        }
        private bool Namespace(CompilationUnitNode cu) {
            if (Keyword(ParserConstants.NamespaceKeyword)) {
                var uri = UriExpected();
                TokenExpected('{');
                var ns = new NamespaceNode(uri);
                while (Import(ns)) ;
                while (GlobalType(ns)) ;
                TokenExpected('}');
                cu.NamespaceList.Add(ns);
                return true;
            }
            return false;
        }
        private Token UriExpected() {
            var uri = StringExpected();
            if (uri.Value == Extensions.SystemUri) {
                ErrorAndThrow(new DiagMsgEx(DiagCodeEx.UriSystemReserved), uri.TextSpan);
            }
            return uri;
        }
        private bool Import(NamespaceNode ns) {
            if (Keyword(ParserConstants.ImportKeyword)) {
                var uri = UriExpected();
                var alias = default(Token);
                if (Keyword(ParserConstants.AsKeyword)) {
                    alias = IdentifierExpected();
                    if (alias.Value == "sys") {
                        ErrorAndThrow(new DiagMsgEx(DiagCodeEx.AliasSysReserved), alias.TextSpan);
                    }
                    if (ns.ImportList.Count > 0) {
                        foreach (var import in ns.ImportList) {
                            if (import.Alias == alias) {
                                ErrorAndThrow(new DiagMsgEx(DiagCodeEx.DuplicateNamespaceAlias, alias.Value), alias.TextSpan);
                            }
                        }
                    }
                }
                ns.ImportList.Add(new ImportNode(uri, alias));
                return true;
            }
            return false;
        }
        private bool QualifiableName(out QualifiableNameNode result) {
            Token name;
            if (Identifier(out name)) {
                if (Token((int)TokenKind.ColonColon)) {
                    result = new QualifiableNameNode(name, IdentifierExpected());
                }
                else {
                    result = new QualifiableNameNode(default(Token), name);
                }
                return true;
            }
            result = default(QualifiableNameNode);
            return false;
        }
        private QualifiableNameNode QualifiableNameExpected() {
            QualifiableNameNode qName;
            if (!QualifiableName(out qName)) {
                ErrorAndThrow("Qualifiable name expected.");
            }
            return qName;
        }
        private void CheckDuplicateGlobalType(NamespaceNode ns, Token name) {
            if (ns.GlobalTypeList.Count > 0) {
                foreach (var globalType in ns.GlobalTypeList) {
                    if (globalType.Name == name) {
                        ErrorAndThrow(new DiagMsgEx(DiagCodeEx.DuplicateGlobalTypeName, name.Value), name.TextSpan);
                    }
                }
            }
        }
        private bool GlobalType(NamespaceNode ns) {
            if (Class(ns)) {
                return true;
            }
            return Enum(ns);
        }
        private bool Enum(NamespaceNode ns) {
            if (Keyword(ParserConstants.EnumKeyword)) {
                var name = IdentifierExpected();
                CheckDuplicateGlobalType(ns, name);
                KeywordExpected(ParserConstants.AsKeyword);
                var atomQName = QualifiableNameExpected();
                TokenExpected('{');
                var en = new EnumNode(ns, name, atomQName);
                while (EnumMember(en)) ;
                TokenExpected('}');
                ns.GlobalTypeList.Add(en);
                return true;
            }
            return false;
        }
        private bool EnumMember(EnumNode en) {
            Token name;
            if (Identifier(out name)) {
                if (en.MemberList.Count > 0) {
                    foreach (var item in en.MemberList) {
                        if (item.Name == name) {
                            ErrorAndThrow(new DiagMsgEx(DiagCodeEx.DuplicateEnumMemberName, name.Value), name.TextSpan);
                        }
                    }
                }
                TokenExpected('=');
                en.MemberList.Add(new EnumMemberNode(name, AtomExpected()));
                return true;
            }
            return false;
        }
        private bool Class(NamespaceNode ns) {
            if (Keyword(ParserConstants.ClassKeyword)) {
                var name = IdentifierExpected();
                CheckDuplicateGlobalType(ns, name);
                var abstractOrSealed = default(Token);
                if (Token('[')) {
                    if (!Keyword(ParserConstants.AbstractKeyword, out abstractOrSealed)) {
                        Keyword(ParserConstants.SealedKeyword, out abstractOrSealed);
                    }
                    TokenExpected(']');
                }
                var baseClassQName = default(QualifiableNameNode);
                if (Keyword(ParserConstants.ExtendsKeyword)) {
                    baseClassQName = QualifiableNameExpected();
                }
                TokenExpected('{');
                var cls = new ClassNode(ns, name, abstractOrSealed, baseClassQName);
                while (Property(ns, cls)) ;
                TokenExpected('}');
                ns.GlobalTypeList.Add(cls);
                return true;
            }
            return false;
        }
        private bool Property(NamespaceNode ns, ClassNode cls) {
            Token name;
            if (Identifier(out name)) {
                if (cls.PropertyList.Count > 0) {
                    foreach (var item in cls.PropertyList) {
                        if (item.Name == name) {
                            ErrorAndThrow(new DiagMsgEx(DiagCodeEx.DuplicatePropertyName, name.Value), name.TextSpan);
                        }
                    }
                }
                KeywordExpected(ParserConstants.AsKeyword);
                cls.PropertyList.Add(new PropertyNode(ns, name,
                    LocalTypeExpected(ns, LocalTypeFlags.GlobalTypeRef | LocalTypeFlags.Nullable | LocalTypeFlags.List | LocalTypeFlags.Set | LocalTypeFlags.Map)));
                return true;
            }
            return false;
        }
        [Flags]
        private enum LocalTypeFlags {
            GlobalTypeRef = 1,
            Nullable = 2,
            List = 4,
            Set = 8,
            Map = 16,
        }
        private LocalTypeNode LocalTypeExpected(NamespaceNode ns, LocalTypeFlags flags) {
            LocalTypeNode type;
            if (!LocalType(ns, flags, out type)) {
                ErrorAndThrow(new DiagMsgEx(DiagCodeEx.SpecificTypeExpected, flags.ToString()));
            }
            return type;
        }
        private bool LocalType(NamespaceNode ns, LocalTypeFlags flags, out LocalTypeNode result) {
            if ((flags & LocalTypeFlags.Nullable) != 0) {
                NullableNode r;
                if (Nullable(ns, out r)) {
                    result = r;
                    return true;
                }
            }
            if ((flags & LocalTypeFlags.List) != 0) {
                ListNode r;
                if (List(ns, out r)) {
                    result = r;
                    return true;
                }
            }
            if ((flags & LocalTypeFlags.Set) != 0) {
                SetNode r;
                if (Set(ns, out r)) {
                    result = r;
                    return true;
                }
            }
            if ((flags & LocalTypeFlags.Map) != 0) {
                MapNode r;
                if (Map(ns, out r)) {
                    result = r;
                    return true;
                }
            }
            if ((flags & LocalTypeFlags.GlobalTypeRef) != 0) {
                GlobalTypeRefNode r;
                if (GlobalTypeRef(ns, out r)) {
                    result = r;
                    return true;
                }
            }
            result = null;
            return false;
        }
        private bool GlobalTypeRef(NamespaceNode ns, out GlobalTypeRefNode result) {
            QualifiableNameNode qName;
            if (QualifiableName(out qName)) {
                result = new GlobalTypeRefNode(ns, qName.TextSpan, qName);
                return true;
            }
            result = null;
            return false;
        }
        private bool Nullable(NamespaceNode ns, out NullableNode result) {
            Token tk;
            if (Keyword(ParserConstants.NullableKeyword, out tk)) {
                TokenExpected('<');
                var element = LocalTypeExpected(ns, LocalTypeFlags.GlobalTypeRef | LocalTypeFlags.List | LocalTypeFlags.Set | LocalTypeFlags.Map);
                TokenExpected('>');
                result = new NullableNode(ns, tk.TextSpan, element);
                return true;
            }
            result = null;
            return false;
        }
        private bool List(NamespaceNode ns, out ListNode result) {
            Token tk;
            if (Keyword(ParserConstants.ListKeyword, out tk)) {
                TokenExpected('<');
                var item = LocalTypeExpected(ns, LocalTypeFlags.GlobalTypeRef | LocalTypeFlags.Nullable | LocalTypeFlags.List | LocalTypeFlags.Set | LocalTypeFlags.Map);
                TokenExpected('>');
                result = new ListNode(ns, tk.TextSpan, item);
                return true;
            }
            result = null;
            return false;
        }
        private bool Map(NamespaceNode ns, out MapNode result) {
            Token tk;
            if (Keyword(ParserConstants.MapKeyword, out tk)) {
                TokenExpected('<');
                var key = (GlobalTypeRefNode)LocalTypeExpected(ns, LocalTypeFlags.GlobalTypeRef);
                TokenExpected(',');
                var value = LocalTypeExpected(ns, LocalTypeFlags.GlobalTypeRef | LocalTypeFlags.Nullable | LocalTypeFlags.List | LocalTypeFlags.Set | LocalTypeFlags.Map);
                TokenExpected('>');
                result = new MapNode(ns, tk.TextSpan, key, value);
                return true;
            }
            result = null;
            return false;
        }
        private bool Set(NamespaceNode ns, out SetNode result) {
            Token tk;
            if (Keyword(ParserConstants.SetKeyword, out tk)) {
                TokenExpected('<');
                var item = (GlobalTypeRefNode)LocalTypeExpected(ns, LocalTypeFlags.GlobalTypeRef);
                List<Token> keyNameList = null;
                if (Token('\\')) {
                    keyNameList = new List<Token> { IdentifierExpected() };
                    while (true) {
                        if (Token('.')) {
                            keyNameList.Add(IdentifierExpected());
                        }
                        else {
                            break;
                        }
                    }
                }
                result = new SetNode(ns, tk.TextSpan, item, keyNameList, TokenExpectedEx('>').TextSpan);
                return true;
            }
            result = null;
            return false;
        }


    }
}
