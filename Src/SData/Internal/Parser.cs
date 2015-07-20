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
        private Parser() {
            _aliasUriListStack = new Stack<List<AliasUri>>();
        }
        private readonly Stack<List<AliasUri>> _aliasUriListStack;
        public static bool Parse(string filePath, TextReader reader, LoadingContext context, ClassTypeMd classTypeMd, out object result) {
            if (classTypeMd == null) throw new ArgumentNullException("classTypeMd");
            return Instance.ParsingUnit(filePath, reader, context, classTypeMd, out result);
        }
        private bool ParsingUnit(string filePath, TextReader reader, LoadingContext context, ClassTypeMd classTypeMd, out object result) {
            try {
                Set(filePath, reader, context);
                _aliasUriListStack.Clear();
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
            catch (LoadingException) { }
            finally {
                Clear();
            }
            result = null;
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
            ErrorAndThrow(new DiagMsg(DiagnosticCode.InvalidUriReference, alias), aliasToken.TextSpan);
            return null;
        }
        private bool ClassValue(ClassTypeMd declaredClsMd, out object result, out TextSpan textSpan) {
            var hasAliasUriList = false;
            if (Token('<')) {
                List<AliasUri> list = null;
                while (true) {
                    Token aliasToken;
                    if (Name(out aliasToken)) {
                        var alias = aliasToken.Value;
                        //if (alias == "sys") {
                        //}
                        if (list == null) {
                            list = new List<AliasUri>();
                        }
                        else {
                            foreach (var item in list) {
                                if (item.Alias == alias) {
                                    ErrorAndThrow(new DiagMsg(DiagnosticCode.DuplicateUriAlias, alias), aliasToken.TextSpan);
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
                var aliasToken = NameExpected();
                TokenExpected((int)TokenKind.ColonColon, ":: expected.");
                var nameToken = NameExpected();
                TokenExpected(')');
                fullName = new FullName(GetUri(aliasToken), nameToken.Value);
                textSpan = nameToken.TextSpan;
                if (declaredClsMd != null) {
                    clsMd = ProgramMd.TryGetGlobalType<ClassTypeMd>(fullName);
                    if (clsMd == null) {
                        ErrorAndThrow(new DiagMsg(DiagnosticCode.InvalidClassReference, fullName.ToString()), textSpan);
                    }
                    if (!clsMd.IsEqualToOrDeriveFrom(declaredClsMd)) {
                        ErrorAndThrow(new DiagMsg(DiagnosticCode.ClassNotEqualToOrDeriveFromTheDeclared, fullName.ToString(), declaredClsMd.FullName.ToString()),
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
                        ErrorAndThrow(new DiagMsg(DiagnosticCode.ClassIsAbstract, fullName.ToString()), textSpan);
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
                        if (Name(out propNameToken)) {
                            var propName = propNameToken.Value;
                            if (propNameSet == null) {
                                propNameSet = new HashSet<string>();
                            }
                            else if (propNameSet.Contains(propName)) {
                                ErrorAndThrow(new DiagMsg(DiagnosticCode.DuplicateUriAlias, propName), propNameToken.TextSpan);
                            }
                            propNameSet.Add(propName);
                            TokenExpected('=');
                            PropertyMd propMd;
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
                    var closeBraceToken = TokenExpected('}');
                    if (propMdMap.Count > 0) {
                        var needThrow = false;
                        foreach (var propMd in propMdMap.Values) {
                            if (!propMd.Type.IsNullable && (propNameSet == null || !propNameSet.Contains(propMd.Name))) {
                                Error(new DiagMsg(DiagnosticCode.PropertyMissing, propMd.Name), closeBraceToken.TextSpan);
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
                        if (Name(out propNameToken)) {
                            var propName = propNameToken.Value;
                            if (propMap == null) {
                                propMap = new Dictionary<string, object>();
                            }
                            else if (propMap.ContainsKey(propName)) {
                                ErrorAndThrow(new DiagMsg(DiagnosticCode.DuplicateUriAlias, propName), propNameToken.TextSpan);
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
                ErrorAndThrow(new DiagMsg(DiagnosticCode.ValueExpected));
            }
            return value;
        }
        private bool LocalValue(LocalTypeMd typeMd, out object result, out TextSpan textSpan) {
            Token token;
            if (Null(out token)) {
                if (typeMd != null && !typeMd.IsNullable) {
                    ErrorAndThrow(new DiagMsg(DiagnosticCode.NullNotAllowed), token.TextSpan);
                }
                result = null;
                textSpan = token.TextSpan;
                return true;
            }
            if (AtomValue(out token)) {
                if (typeMd != null) {
                    var nonNullableTypeMd = typeMd.NonNullableType;
                    var typeKind = nonNullableTypeMd.Kind;
                    if (!typeKind.IsAtom()) {
                        if (typeKind == TypeKind.Enum) {
                            typeKind = nonNullableTypeMd.TryGetGlobalType<EnumTypeMd>().UnderlyingType.Kind;
                        }
                        else {
                            ErrorAndThrow(new DiagMsg(DiagnosticCode.SpecificValueExpected, typeKind.ToString()), token.TextSpan);
                        }
                    }
                    result = AtomExtensions.TryParse(typeKind, token.Value);
                    if (result == null) {
                        ErrorAndThrow(new DiagMsg(DiagnosticCode.InvalidAtomValue, typeKind.ToString(), token.Value), token.TextSpan);
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
                        ErrorAndThrow(new DiagMsg(DiagnosticCode.SpecificValueExpected, typeKind.ToString()));
                    }
                    return ClassValue(nonNullableTypeMd.TryGetGlobalType<ClassTypeMd>(), out result, out textSpan);
                }
                else {
                    return ClassValue(null, out result, out textSpan);
                }
            }
            if (Name(out token)) {//enum value
                TokenExpected((int)TokenKind.ColonColon, ":: expected.");
                var nameToken = NameExpected();
                var fullName = new FullName(GetUri(token), nameToken.Value);
                EnumTypeMd enumMd = null;
                if (typeMd != null) {
                    var nonNullableTypeMd = typeMd.NonNullableType;
                    var typeKind = nonNullableTypeMd.Kind;
                    if (typeKind != TypeKind.Enum) {
                        ErrorAndThrow(new DiagMsg(DiagnosticCode.SpecificValueExpected, typeKind.ToString()), nameToken.TextSpan);
                    }
                    enumMd = ProgramMd.TryGetGlobalType<EnumTypeMd>(fullName);
                    if (enumMd == null) {
                        ErrorAndThrow(new DiagMsg(DiagnosticCode.InvalidEnumReference, fullName.ToString()), nameToken.TextSpan);
                    }
                    var declaredEnumMd = nonNullableTypeMd.TryGetGlobalType<EnumTypeMd>();
                    if (enumMd != declaredEnumMd) {
                        ErrorAndThrow(new DiagMsg(DiagnosticCode.EnumNotEqualToTheDeclared, fullName.ToString(), declaredEnumMd.FullName.ToString()),
                            nameToken.TextSpan);
                    }
                }
                TokenExpected('.');
                var memberNameToken = NameExpected();
                if (enumMd != null) {
                    if (!enumMd._members.TryGetValue(memberNameToken.Value, out result)) {
                        ErrorAndThrow(new DiagMsg(DiagnosticCode.InvalidEnumMemberName, memberNameToken.Value), memberNameToken.TextSpan);
                    }
                }
                else {
                    result = new UntypedEnumValue(fullName, memberNameToken.Value);
                }
                textSpan = memberNameToken.TextSpan;
                return true;
            }
            if (Token('[', out token)) {
                if (typeMd != null) {
                    var nonNullableTypeMd = typeMd.NonNullableType;
                    var typeKind = nonNullableTypeMd.Kind;
                    var isList = typeKind == TypeKind.List;
                    var isSet = typeKind == TypeKind.Set;
                    if (!isList && !isSet) {
                        ErrorAndThrow(new DiagMsg(DiagnosticCode.SpecificValueExpected, typeKind.ToString()), token.TextSpan);
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
                                    ErrorAndThrow(new DiagMsg(DiagnosticCode.DuplicateSetItem), ts);
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
                        ErrorAndThrow(new DiagMsg(DiagnosticCode.SpecificValueExpected, typeKind.ToString()), token.TextSpan);
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
                                ErrorAndThrow(new DiagMsg(DiagnosticCode.DuplicateMapKey), ts);
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
