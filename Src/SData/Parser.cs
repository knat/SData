using System;
using System.Collections.Generic;
using System.IO;

namespace SData {
    public sealed class Parser : ParserBase {
        [ThreadStatic]
        private static Parser _instance;
        private static Parser Instance {
            get { return _instance ?? (_instance = new Parser()); }
        }
        private Stack<List<AliasUri>> _aliasUriListStack;
        private Parser() { }
        public static bool Parse(string filePath, TextReader reader, Context context, ClassTypeMd classTypeMd, out object result) {
            if (classTypeMd == null) throw new ArgumentNullException("classTypeMd");
            return Instance.ParsingUnit(filePath, reader, context, classTypeMd, out result);
        }
        private bool ParsingUnit(string filePath, TextReader reader, Context context, ClassTypeMd classTypeMd, out object result) {
            try {
                Set(filePath, reader, context);
                if (_aliasUriListStack == null) {
                    _aliasUriListStack = new Stack<List<AliasUri>>();
                }
                else {
                    _aliasUriListStack.Clear();
                }
                object obj;
                if (ClassValue(classTypeMd, out obj)) {
                    EndOfFileExpected();
                    result = obj;
                    return true;
                }
                else {
                    ErrorAndThrow("Class value expected.");
                }
            }
            catch (ParsingException) { }
            finally {
                Clear();
            }
            result = null;
            return false;
        }



        private struct AliasUri {
            public AliasUri(string alias, string uri) {
                Alias = alias;
                Uri = uri;
            }
            public readonly string Alias;
            public readonly string Uri;
        }
        private bool AliasUriList() {
            if (Token('<')) {
                List<AliasUri> list = null;
                while (true) {
                    Token aliasToken;
                    if (Identifier(out aliasToken)) {
                        var alias = aliasToken.Value;
                        //if (alias == "sys") {

                        //}
                        if (list == null) {
                            list = new List<AliasUri>();
                        }
                        else {
                            foreach (var item in list) {
                                if (item.Alias == alias) {
                                    ErrorAndThrow(new DiagMsg(DiagCode.DuplicateAlias, alias), aliasToken.TextSpan);
                                }
                            }
                        }
                        TokenExpected('=');
                        list.Add(new AliasUri(StringExpected().Value, alias));
                        if (!Token(',')) {
                            break;
                        }
                    }
                    else {
                        break;
                    }
                }
                TokenExpected('>');
                if (list != null) {
                    _aliasUriListStack.Push(list);
                    return true;
                }
                return false;
            }
            return false;
        }
        private string GetUri(Token aliasToken) {
            var alias = aliasToken.Value;
            //if (alias == "sys") {
            //    return Extensions.SystemUri;
            //}
            foreach (var auList in _aliasUriListStack) {
                foreach (var au in auList) {
                    if (au.Alias == alias) {
                        return au.Uri;
                    }
                }
            }
            ErrorAndThrow(new DiagMsg(DiagCode.InvalidUriReference, alias), aliasToken.TextSpan);
            return null;
        }
        //private bool FullName(out FullName result) {
        //    Token aliasToken;
        //    if (Identifier(out aliasToken)) {
        //        var uri = GetUri(aliasToken);
        //        TokenExpected((int)TokenKind.ColonColon, ":: expected.");
        //        result = new FullName(uri, IdentifierExpected().Value);
        //        return true;
        //    }
        //    result = default(FullName);
        //    return false;
        //}
        private bool ClassValue(ClassTypeMd declaredClsMd, out object result) {
            var hasAliasUriList = AliasUriList();
            var hasTypeIndicator = false;
            ClassTypeMd clsMd = null;
            var fullName = default(FullName);
            var clsNameTS = default(TextSpan);
            if (Token('(')) {
                hasTypeIndicator = true;
                var aliasToken = IdentifierExpected();
                TokenExpected((int)TokenKind.ColonColon, ":: expected.");
                var nameToken = IdentifierExpected();
                TokenExpected(')');
                fullName = new FullName(GetUri(aliasToken), nameToken.Value);
                clsNameTS = nameToken.TextSpan;
                clsMd = AssemblyMd.GetGlobalType<ClassTypeMd>(fullName);
                if (clsMd == null) {
                    ErrorAndThrow(new DiagMsg(DiagCode.InvalidClassReference, fullName.ToString()), clsNameTS);
                }
                if (!clsMd.IsEqualToOrDeriveFrom(declaredClsMd)) {
                    ErrorAndThrow(new DiagMsg(DiagCode.ClassNotEqualToOrDeriveFromTheDeclared, fullName.ToString(), declaredClsMd.FullName.ToString()),
                        clsNameTS);
                }
            }
            Token openBracket;
            if (Token('{', out openBracket)) {
                if (clsMd == null) {
                    clsMd = declaredClsMd;
                    fullName = clsMd.FullName;
                    clsNameTS = openBracket.TextSpan;
                }
                if (clsMd.IsAbstract) {
                    ErrorAndThrow(new DiagMsg(DiagCode.ClassIsAbstract, fullName.ToString()), clsNameTS);
                }
                var obj = clsMd.CreateInstance();
                clsMd.SetTextSpan(obj, clsNameTS);
                if (!clsMd.InvokeOnLoad(true, obj, _context)) {
                    Throw();
                }


                TokenExpected('}');
                if (hasAliasUriList) {
                    _aliasUriListStack.Pop();
                }

            }
            else if (hasAliasUriList || hasTypeIndicator) {
                ErrorAndThrow("{ expected.");
            }
            result = null;
            return false;
        }

    }
}
