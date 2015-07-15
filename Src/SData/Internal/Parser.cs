using System;
using System.Collections.Generic;
using System.IO;

namespace SData.Internal {
    public sealed class Parser : ParserBase {
        [ThreadStatic]
        private static Parser _instance;
        private static Parser Instance {
            get { return _instance ?? (_instance = new Parser()); }
        }
        private Stack<List<AliasUri>> _aliasUriListStack;
        private Parser() { }
        public static bool Parse(string filePath, TextReader reader, LoadingContext context, ClassTypeMd classTypeMd, out object result) {
            if (classTypeMd == null) throw new ArgumentNullException("classTypeMd");
            return Instance.ParsingUnit(filePath, reader, context, classTypeMd, out result);
        }
        private bool ParsingUnit(string filePath, TextReader reader, LoadingContext context, ClassTypeMd classTypeMd, out object result) {
            try {
                Set(filePath, reader, context);
                if (_aliasUriListStack == null) {
                    _aliasUriListStack = new Stack<List<AliasUri>>();
                }
                else {
                    _aliasUriListStack.Clear();
                }
                object obj;
                TextSpan textSpan;
                if (ClassValue(classTypeMd, out obj, out textSpan)) {
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



        //private bool AliasUriList() {
        //    if (Token('<')) {
        //        List<AliasUri> list = null;
        //        while (true) {
        //            Token aliasToken;
        //            if (Identifier(out aliasToken)) {
        //                var alias = aliasToken.Value;
        //                //if (alias == "sys") {

        //                //}
        //                if (list == null) {
        //                    list = new List<AliasUri>();
        //                }
        //                else {
        //                    foreach (var item in list) {
        //                        if (item.Alias == alias) {
        //                            ErrorAndThrow(new DiagMsg(DiagCode.DuplicateAlias, alias), aliasToken.TextSpan);
        //                        }
        //                    }
        //                }
        //                TokenExpected('=');
        //                list.Add(new AliasUri(StringExpected().Value, alias));
        //                if (!Token(',')) {
        //                    break;
        //                }
        //            }
        //            else {
        //                break;
        //            }
        //        }
        //        TokenExpected('>');
        //        if (list != null) {
        //            _aliasUriListStack.Push(list);
        //            return true;
        //        }
        //        return false;
        //    }
        //    return false;
        //}
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
        private bool ClassValue(ClassTypeMd declaredClsMd, out object result, out TextSpan textSpan) {
            var hasAliasUriList = false;
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
                    hasAliasUriList = true;
                }
            }


            textSpan = default(TextSpan);
            var hasTypeIndicator = false;
            ClassTypeMd clsMd = null;
            var fullName = default(FullName);
            //var clsNameTS = default(TextSpan);
            if (Token('(')) {
                hasTypeIndicator = true;
                var aliasToken = IdentifierExpected();
                TokenExpected((int)TokenKind.ColonColon, ":: expected.");
                var nameToken = IdentifierExpected();
                TokenExpected(')');
                fullName = new FullName(GetUri(aliasToken), nameToken.Value);
                textSpan = nameToken.TextSpan;
                if (declaredClsMd != null) {
                    clsMd = AssemblyMd.TryGetGlobalType<ClassTypeMd>(fullName);
                    if (clsMd == null) {
                        ErrorAndThrow(new DiagMsg(DiagCode.InvalidClassReference, fullName.ToString()), textSpan);
                    }
                    if (!clsMd.IsEqualToOrDeriveFrom(declaredClsMd)) {
                        ErrorAndThrow(new DiagMsg(DiagCode.ClassNotEqualToOrDeriveFromTheDeclared, fullName.ToString(), declaredClsMd.FullName.ToString()),
                            textSpan);
                    }
                }
            }
            Token openBraceToken;
            if (Token('{', out openBraceToken)) {
                if (declaredClsMd != null) {
                    if (!hasTypeIndicator) {
                        clsMd = declaredClsMd;
                        fullName = clsMd.FullName;
                        textSpan = openBraceToken.TextSpan;
                    }
                    if (clsMd.IsAbstract) {
                        ErrorAndThrow(new DiagMsg(DiagCode.ClassIsAbstract, fullName.ToString()), textSpan);
                    }
                    var obj = clsMd.CreateInstance();
                    //clsMd.SetTextSpan(obj, clsNameTS);
                    if (!clsMd.InvokeOnLoad(true, obj, _context, textSpan)) {
                        Throw();
                    }
                    var propMdMap = clsMd._propertyMap;
                    HashSet<string> propNameSet = null;
                    Dictionary<string, object> unknownPropMap = null;
                    while (true) {
                        Token propNameToken;
                        if (Identifier(out propNameToken)) {
                            var propName = propNameToken.Value;
                            if (propNameSet == null) {
                                propNameSet = new HashSet<string>();
                            }
                            else if (propNameSet.Contains(propName)) {
                                ErrorAndThrow(new DiagMsg(DiagCode.DuplicateAlias, propName), propNameToken.TextSpan);
                            }
                            propNameSet.Add(propName);
                            TokenExpected('=');
                            ClassTypePropertyMd propMd;
                            if (propMdMap.TryGetValue(propName, out propMd)) {
                                propMd.SetValue(obj, LocalValueExpected(propMd.Type));
                            }
                            else {
                                if (unknownPropMap == null) {
                                    unknownPropMap = new Dictionary<string, object>();
                                }
                                unknownPropMap.Add(propName, LocalValueExpected(null));
                            }
                            if (!Token(',')) {
                                break;
                            }
                        }
                        else {
                            break;
                        }
                    }
                    var closeBraceToken = TokenExpectedEx('}');
                    if (propMdMap.Count > 0) {
                        var needThrow = false;
                        foreach (var propMd in propMdMap.Values) {
                            if (!propMd.Type.IsNullable && (propNameSet == null || !propNameSet.Contains(propMd.Name))) {
                                Error(new DiagMsg(DiagCode.PropertyMissing, propMd.Name), closeBraceToken.TextSpan);
                                needThrow = true;
                            }
                        }
                        if (needThrow) {
                            Throw();
                        }
                    }
                    if (unknownPropMap != null) {
                        clsMd.SetUnknownProperties(obj, unknownPropMap);
                    }
                    if (!clsMd.InvokeOnLoad(false, obj, _context, textSpan)) {
                        Throw();
                    }
                    result = obj;
                }
                else {
                    Dictionary<string, object> propMap = null;
                    while (true) {
                        Token propNameToken;
                        if (Identifier(out propNameToken)) {
                            var propName = propNameToken.Value;
                            if (propMap == null) {
                                propMap = new Dictionary<string, object>();
                            }
                            else if (propMap.ContainsKey(propName)) {
                                ErrorAndThrow(new DiagMsg(DiagCode.DuplicateAlias, propName), propNameToken.TextSpan);
                            }
                            TokenExpected('=');
                            propMap.Add(propName, LocalValueExpected(null));
                            if (!Token(',')) {
                                break;
                            }
                        }
                        else {
                            break;
                        }
                    }
                    TokenExpected('}');
                    result = new UntypedObject(hasTypeIndicator ? (FullName?)fullName : null, propMap);
                }
                if (hasAliasUriList) {
                    _aliasUriListStack.Pop();
                }
                return true;
            }
            else if (hasAliasUriList || hasTypeIndicator) {
                ErrorAndThrow("{ expected.");
            }
            result = null;
            return false;
        }
        private object LocalValueExpected(LocalTypeMd typeMd) {
            object value;
            TextSpan textSpan;
            if (!LocalValue(typeMd, out value, out textSpan)) {
                ErrorAndThrow(new DiagMsg(DiagCode.ValueExpected));
            }
            return value;
        }
        private bool LocalValue(LocalTypeMd typeMd, out object result, out TextSpan textSpan) {
            Token token;
            if (Null(out token)) {
                if (typeMd != null && !typeMd.IsNullable) {
                    ErrorAndThrow(new DiagMsg(DiagCode.NullNotAllowed), token.TextSpan);
                }
                result = null;
                textSpan = token.TextSpan;
                return true;
            }
            if (Atom(out token)) {
                if (typeMd != null) {
                    var nonNullableTypeMd = typeMd.NonNullableType;
                    var typeKind = nonNullableTypeMd.Kind;
                    if (!typeKind.IsAtom()) {
                        if (typeKind == TypeKind.Enum) {
                            typeKind = nonNullableTypeMd.TryGetGlobalType<EnumTypeMd>().UnderlyingType.Kind;
                        }
                        else {
                            ErrorAndThrow(new DiagMsg(DiagCode.SpecificValueExpected, typeKind.ToString()), token.TextSpan);
                        }
                    }
                    result = AtomExtensionsEx.TryParse(typeKind, token.Value);
                    if (result == null) {
                        ErrorAndThrow(new DiagMsg(DiagCode.InvalidAtomValue, typeKind.ToString(), token.Value), token.TextSpan);
                    }
                }
                else {
                    result = token.Value;
                }
                textSpan = token.TextSpan;
                return true;
            }
            if (PeekToken('(', '{', '<')) {//class value
                if (typeMd != null) {
                    var nonNullableTypeMd = typeMd.NonNullableType;
                    var typeKind = nonNullableTypeMd.Kind;
                    if (typeKind != TypeKind.Class) {
                        ErrorAndThrow(new DiagMsg(DiagCode.SpecificValueExpected, typeKind.ToString()));
                    }
                    return ClassValue(nonNullableTypeMd.TryGetGlobalType<ClassTypeMd>(), out result, out textSpan);
                }
                else {
                    return ClassValue(null, out result, out textSpan);
                }
            }
            if (Identifier(out token)) {//enum value
                TokenExpected((int)TokenKind.ColonColon, ":: expected.");
                var nameToken = IdentifierExpected();
                var fullName = new FullName(GetUri(token), nameToken.Value);
                EnumTypeMd enumMd = null;
                if (typeMd != null) {
                    var nonNullableTypeMd = typeMd.NonNullableType;
                    var typeKind = nonNullableTypeMd.Kind;
                    if (typeKind != TypeKind.Enum) {
                        ErrorAndThrow(new DiagMsg(DiagCode.SpecificValueExpected, typeKind.ToString()), nameToken.TextSpan);
                    }
                    enumMd = AssemblyMd.TryGetGlobalType<EnumTypeMd>(fullName);
                    if (enumMd == null) {
                        ErrorAndThrow(new DiagMsg(DiagCode.InvalidEnumReference, fullName.ToString()), nameToken.TextSpan);
                    }
                    var declaredEnumMd = nonNullableTypeMd.TryGetGlobalType<EnumTypeMd>();
                    if (enumMd != declaredEnumMd) {
                        ErrorAndThrow(new DiagMsg(DiagCode.EnumNotEqualToTheDeclared, fullName.ToString(), declaredEnumMd.FullName.ToString()),
                            nameToken.TextSpan);
                    }
                }
                TokenExpected('.');
                var memberNameToken = IdentifierExpected();
                if (enumMd != null) {
                    if (!enumMd._members.TryGetValue(memberNameToken.Value, out result)) {
                        ErrorAndThrow(new DiagMsg(DiagCode.InvalidEnumMemberName, memberNameToken.Value), memberNameToken.TextSpan);
                    }
                }
                else {
                    result = new UntypedEnumMember(fullName, memberNameToken.Value);
                }
                textSpan = memberNameToken.TextSpan;
                return true;
            }
            if (Token('[', out token)) {
                if (typeMd != null) {
                    var nonNullableTypeMd = typeMd.NonNullableType;
                    var typeKind = nonNullableTypeMd.Kind;
                    var isList = typeKind == TypeKind.List;
                    var isSet = typeKind == TypeKind.SimpleSet || typeKind == TypeKind.ObjectSet;
                    if (!isList && !isSet) {
                        ErrorAndThrow(new DiagMsg(DiagCode.SpecificValueExpected, typeKind.ToString()), token.TextSpan);
                    }
                    var collTypeMd = (CollectionTypeMd)nonNullableTypeMd;
                    var collObj = collTypeMd.CreateInstance();
                    var itemMd = collTypeMd.ItemOrValueType;
                    object itemObj;
                    TextSpan ts;
                    while (true) {
                        if (LocalValue(itemMd, out itemObj, out ts)) {
                            if (isSet) {
                                if (!collTypeMd.InvokeBoolAdd(collObj, itemObj)) {
                                    ErrorAndThrow(new DiagMsg(DiagCode.DuplicateSetItem), ts);
                                }
                            }
                            else {
                                collTypeMd.InvokeAdd(collObj, itemObj);
                            }
                            if (!Token(',')) {
                                break;
                            }
                        }
                        else {
                            break;
                        }
                    }
                    TokenExpected(']');
                    result = collObj;
                }
                else {
                    var listObj = new List<object>();
                    object itemObj;
                    TextSpan ts;
                    while (true) {
                        if (LocalValue(null, out itemObj, out ts)) {
                            listObj.Add(itemObj);
                            if (!Token(',')) {
                                break;
                            }
                        }
                        else {
                            break;
                        }
                    }
                    TokenExpected(']');
                    result = listObj;
                }
                textSpan = token.TextSpan;
                return true;
            }
            if (Token((int)TokenKind.HashOpenBracket, out token)) {
                if (typeMd != null) {
                    var nonNullableTypeMd = typeMd.NonNullableType;
                    var typeKind = nonNullableTypeMd.Kind;
                    if (typeKind != TypeKind.Map) {
                        ErrorAndThrow(new DiagMsg(DiagCode.SpecificValueExpected, typeKind.ToString()), token.TextSpan);
                    }
                    var collTypeMd = (CollectionTypeMd)nonNullableTypeMd;
                    var collObj = collTypeMd.CreateInstance();
                    var keyMd = collTypeMd.MapKeyType;
                    var valueMd = collTypeMd.ItemOrValueType;
                    object keyObj;
                    TextSpan ts;
                    while (true) {
                        if (LocalValue(keyMd, out keyObj, out ts)) {
                            if (collTypeMd.InvokeContainsKey(collObj, keyObj)) {
                                ErrorAndThrow(new DiagMsg(DiagCode.DuplicateMapKey), ts);
                            }
                            TokenExpected('=');
                            collTypeMd.InvokeAdd(collObj, keyObj, LocalValueExpected(valueMd));
                            if (!Token(',')) {
                                break;
                            }
                        }
                        else {
                            break;
                        }
                    }
                    TokenExpected(']');
                    result = collObj;
                }
                else {
                    var mapObj = new Dictionary<object, object>();
                    object keyObj;
                    TextSpan ts;
                    while (true) {
                        if (LocalValue(null, out keyObj, out ts)) {
                            TokenExpected('=');
                            mapObj.Add(keyObj, LocalValueExpected(null));
                            if (!Token(',')) {
                                break;
                            }
                        }
                        else {
                            break;
                        }
                    }
                    TokenExpected(']');
                    result = mapObj;
                }
                textSpan = token.TextSpan;
                return true;
            }
            result = null;
            textSpan = default(TextSpan);
            return false;
        }

    }
}
