using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace SData {
    public abstract class AssemblyMd {
        private static readonly Dictionary<FullName, GlobalTypeMd> _globalTypeMap = new Dictionary<FullName, GlobalTypeMd>();
        //private static readonly HashSet<string> _uriSet = new HashSet<string>();
        public static T GetGlobalType<T>(FullName fullName) where T : GlobalTypeMd {
            GlobalTypeMd globalType;
            _globalTypeMap.TryGetValue(fullName, out globalType);
            return globalType as T;
        }
        //internal static bool IsUriDefined(string uri) {
        //    return _uriSet.Contains(uri);
        //}
        protected void AddGlobalTypes(GlobalTypeMd[] globalTypes) {
            if (globalTypes != null) {
                lock (_globalTypeMap) {
                    foreach (var globalType in globalTypes) {
                        var fullName = globalType.FullName;
                        _globalTypeMap.Add(fullName, globalType);
                        //_uriSet.Add(fullName.Uri);
                    }
                }
            }
        }
    }

    public enum TypeKind : byte {
        None = 0,
        String = 1,
        IgnoreCaseString = 2,
        Char = 3,
        Decimal = 4,
        Int64 = 5,
        Int32 = 6,
        Int16 = 7,
        SByte = 8,
        UInt64 = 9,
        UInt32 = 10,
        UInt16 = 11,
        Byte = 12,
        Double = 13,
        Single = 14,
        Boolean = 15,
        Binary = 16,
        Guid = 17,
        TimeSpan = 18,
        DateTimeOffset = 19,
        //Null = 30,
        //
        Class = 50,
        Enum = 51,
        //
        Nullable = 70,
        //Enumerable = 71,
        List = 72,
        SimpleSet = 73,
        ObjectSet = 74,
        Map = 75,
        //AnonymousClass = 76,
        //Void = 77,

    }

    public abstract class TypeMd {
        protected TypeMd(TypeKind kind) {
            Kind = kind;
        }
        public readonly TypeKind Kind;
    }
    public abstract class LocalTypeMd : TypeMd {
        protected LocalTypeMd(TypeKind kind)
            : base(kind) {
        }
    }
    public sealed class NullableTypeMd : LocalTypeMd {
        public NullableTypeMd(LocalTypeMd elementType)
            : base(TypeKind.Nullable) {
            ElementType = elementType;
        }
        public readonly LocalTypeMd ElementType;
    }
    //public sealed class AnonymousClassTypeMd : LocalTypeMd {
    //    public AnonymousClassTypeMd(AnonymousClassPropertyMd[] properties)
    //        : base(TypeKind.AnonymousClass) {
    //        _properties = properties;
    //    }
    //    private readonly AnonymousClassPropertyMd[] _properties;//opt
    //    public AnonymousClassPropertyMd GetProperty(string name) {
    //        var props = _properties;
    //        if (props != null) {
    //            var length = props.Length;
    //            for (var i = 0; i < length; ++i) {
    //                if (props[i].Name == name) {
    //                    return props[i];
    //                }
    //            }
    //        }
    //        return null;
    //    }
    //}
    //public abstract class NameTypePairMd : ITypeProviderMd {
    //    protected NameTypePairMd(string name, LocalTypeMd type) {
    //        Name = name;
    //        Type = type;
    //    }
    //    public readonly string Name;
    //    public readonly LocalTypeMd Type;
    //    TypeMd ITypeProviderMd.Type {
    //        get { return Type; }
    //    }
    //}
    //public sealed class AnonymousClassPropertyMd : NameTypePairMd {
    //    public AnonymousClassPropertyMd(string name, LocalTypeMd type)
    //        : base(name, type) {
    //    }
    //}
    //public class EnumerableTypeMd : LocalTypeMd {
    //    public EnumerableTypeMd(LocalTypeMd itemType)
    //        : base(TypeKind.Enumerable) {
    //        ItemType = itemType;
    //    }
    //    protected EnumerableTypeMd(TypeKind kind, LocalTypeMd itemType)
    //        : base(kind) {
    //        ItemType = itemType;
    //    }
    //    public readonly LocalTypeMd ItemType;
    //}

    //for List, SimpleSet, ObjectSet, Map
    public sealed class CollectionTypeMd : LocalTypeMd {
        public CollectionTypeMd(TypeKind kind, LocalTypeMd itemOrValueType,
            GlobalTypeRefMd mapKeyType, object objectSetKeySelector, Type clrType)
            : base(kind) {
            ItemOrValueType = itemOrValueType;
            MapKeyType = mapKeyType;
            //MapValueType = mapValueType;
            ObjectSetKeySelector = objectSetKeySelector;
            ClrType = clrType;
            var ti = clrType.GetTypeInfo();
            ClrConstructor = ReflectionExtensions.GetParameterlessConstructor(ti);
            ClrAddMethod = ReflectionExtensions.GetMethodInHierarchy(ti, "Add");
            if (kind == TypeKind.Map) {
                ClrContainsKeyMethod = ReflectionExtensions.GetMethodInHierarchy(ti, "ContainsKey");
                ClrGetEnumeratorMethod = ReflectionExtensions.GetMethodInHierarchy(ti, "GetEnumerator");
            }
            else if (kind == TypeKind.ObjectSet) {
                ClrKeySelectorProperty = ReflectionExtensions.GetPropertyInHierarchy(ti, "KeySelector");
            }
        }
        public readonly LocalTypeMd ItemOrValueType;
        public readonly GlobalTypeRefMd MapKeyType;//opt
        //public readonly LocalTypeMd MapValueType;//opt
        public readonly object ObjectSetKeySelector;//opt
        public readonly Type ClrType;
        public readonly ConstructorInfo ClrConstructor;
        public readonly MethodInfo ClrAddMethod;
        public readonly MethodInfo ClrContainsKeyMethod;//for map
        public readonly MethodInfo ClrGetEnumeratorMethod;//for map
        public readonly PropertyInfo ClrKeySelectorProperty;//for object set
        public object CreateInstance() {
            var obj = ClrConstructor.Invoke(null);
            if (Kind == TypeKind.ObjectSet) {
                ClrKeySelectorProperty.SetValue(obj, ObjectSetKeySelector);
            }
            return obj;
        }
        public void InvokeAdd(object obj, object item) {
            ClrAddMethod.Invoke(obj, new object[] { item });
        }
        public bool InvokeBoolAdd(object obj, object item) {
            return (bool)ClrAddMethod.Invoke(obj, new object[] { item });
        }
        public bool InvokeContainsKey(object obj, object key) {
            return (bool)ClrContainsKeyMethod.Invoke(obj, new object[] { key });
        }
        public void InvokeAdd(object obj, object key, object value) {
            ClrAddMethod.Invoke(obj, new object[] { key, value });
        }
        public IDictionaryEnumerator GetMapEnumerator(object obj) {
            return ClrGetEnumeratorMethod.Invoke(obj, null) as IDictionaryEnumerator;
        }
    }
    public sealed class GlobalTypeRefMd : LocalTypeMd {
        public GlobalTypeRefMd(GlobalTypeMd globalType)
            : base(globalType.Kind) {
            GlobalType = globalType;
        }
        public readonly GlobalTypeMd GlobalType;
        public static GlobalTypeRefMd GetAtom(TypeKind kind) {
            return _atomMap[kind];
        }
        public static NullableTypeMd GetNullableAtom(TypeKind kind) {
            return _nullableAtomMap[kind];
        }
        private static readonly Dictionary<TypeKind, GlobalTypeRefMd> _atomMap = new Dictionary<TypeKind, GlobalTypeRefMd> {
            { TypeKind.String, new GlobalTypeRefMd(AtomTypeMd.Get(TypeKind.String)) },
            { TypeKind.IgnoreCaseString, new GlobalTypeRefMd(AtomTypeMd.Get(TypeKind.IgnoreCaseString)) },
            { TypeKind.Char, new GlobalTypeRefMd(AtomTypeMd.Get(TypeKind.Char)) },
            { TypeKind.Decimal, new GlobalTypeRefMd(AtomTypeMd.Get(TypeKind.Decimal)) },
            { TypeKind.Int64, new GlobalTypeRefMd(AtomTypeMd.Get(TypeKind.Int64)) },
            { TypeKind.Int32, new GlobalTypeRefMd(AtomTypeMd.Get(TypeKind.Int32)) },
            { TypeKind.Int16, new GlobalTypeRefMd(AtomTypeMd.Get(TypeKind.Int16)) },
            { TypeKind.SByte, new GlobalTypeRefMd(AtomTypeMd.Get(TypeKind.SByte)) },
            { TypeKind.UInt64, new GlobalTypeRefMd(AtomTypeMd.Get(TypeKind.UInt64)) },
            { TypeKind.UInt32, new GlobalTypeRefMd(AtomTypeMd.Get(TypeKind.UInt32)) },
            { TypeKind.UInt16, new GlobalTypeRefMd(AtomTypeMd.Get(TypeKind.UInt16)) },
            { TypeKind.Byte, new GlobalTypeRefMd(AtomTypeMd.Get(TypeKind.Byte)) },
            { TypeKind.Double, new GlobalTypeRefMd(AtomTypeMd.Get(TypeKind.Double)) },
            { TypeKind.Single, new GlobalTypeRefMd(AtomTypeMd.Get(TypeKind.Single)) },
            { TypeKind.Boolean, new GlobalTypeRefMd(AtomTypeMd.Get(TypeKind.Boolean)) },
            { TypeKind.Binary, new GlobalTypeRefMd(AtomTypeMd.Get(TypeKind.Binary)) },
            { TypeKind.Guid, new GlobalTypeRefMd(AtomTypeMd.Get(TypeKind.Guid)) },
            { TypeKind.TimeSpan, new GlobalTypeRefMd(AtomTypeMd.Get(TypeKind.TimeSpan)) },
            { TypeKind.DateTimeOffset, new GlobalTypeRefMd(AtomTypeMd.Get(TypeKind.DateTimeOffset)) },
        };
        private static readonly Dictionary<TypeKind, NullableTypeMd> _nullableAtomMap = new Dictionary<TypeKind, NullableTypeMd> {
            { TypeKind.String, new NullableTypeMd(_atomMap[TypeKind.String]) },
            { TypeKind.IgnoreCaseString, new NullableTypeMd(_atomMap[TypeKind.IgnoreCaseString]) },
            { TypeKind.Char, new NullableTypeMd(_atomMap[TypeKind.Char]) },
            { TypeKind.Decimal, new NullableTypeMd(_atomMap[TypeKind.Decimal]) },
            { TypeKind.Int64, new NullableTypeMd(_atomMap[TypeKind.Int64]) },
            { TypeKind.Int32, new NullableTypeMd(_atomMap[TypeKind.Int32]) },
            { TypeKind.Int16, new NullableTypeMd(_atomMap[TypeKind.Int16]) },
            { TypeKind.SByte, new NullableTypeMd(_atomMap[TypeKind.SByte]) },
            { TypeKind.UInt64, new NullableTypeMd(_atomMap[TypeKind.UInt64]) },
            { TypeKind.UInt32, new NullableTypeMd(_atomMap[TypeKind.UInt32]) },
            { TypeKind.UInt16, new NullableTypeMd(_atomMap[TypeKind.UInt16]) },
            { TypeKind.Byte, new NullableTypeMd(_atomMap[TypeKind.Byte]) },
            { TypeKind.Double, new NullableTypeMd(_atomMap[TypeKind.Double]) },
            { TypeKind.Single, new NullableTypeMd(_atomMap[TypeKind.Single]) },
            { TypeKind.Boolean, new NullableTypeMd(_atomMap[TypeKind.Boolean]) },
            { TypeKind.Binary, new NullableTypeMd(_atomMap[TypeKind.Binary]) },
            { TypeKind.Guid, new NullableTypeMd(_atomMap[TypeKind.Guid]) },
            { TypeKind.TimeSpan, new NullableTypeMd(_atomMap[TypeKind.TimeSpan]) },
            { TypeKind.DateTimeOffset, new NullableTypeMd(_atomMap[TypeKind.DateTimeOffset]) },
        };
    }
    public abstract class GlobalTypeMd : TypeMd {
        protected GlobalTypeMd(TypeKind kind, FullName fullName)
            : base(kind) {
            FullName = fullName;
            //_properties = properties;
            //_functions = functions;
        }
        public readonly FullName FullName;
        //protected PropertyMd[] _properties;//opt
        //protected FunctionMd[] _functions;//opt
    }




    //[Flags]
    //public enum PropertyFlags {
    //    None = 0,
    //    Index = 1,
    //    Static = 2,
    //    ReadOnly = 4,
    //    Extension = 8,
    //}
    //public class PropertyMd : NameTypePairMd {
    //    public PropertyMd(string name, LocalTypeMd type, PropertyFlags flags, ParameterMd[] parameters = null)
    //        : base(name, type) {
    //        Flags = flags;
    //        _parameters = parameters;
    //    }
    //    public readonly PropertyFlags Flags;
    //    private readonly ParameterMd[] _parameters;//opt
    //    public bool IsIndex {
    //        get { return (Flags & PropertyFlags.Index) == PropertyFlags.Index; }
    //    }
    //    public bool IsStatic {
    //        get { return (Flags & PropertyFlags.Static) == PropertyFlags.Static; }
    //    }
    //    public bool IsReadOnly {
    //        get { return (Flags & PropertyFlags.ReadOnly) == PropertyFlags.ReadOnly; }
    //    }
    //    public int ParameterCount {
    //        get { return _parameters == null ? 0 : _parameters.Length; }
    //    }

    //}

    //public sealed class ParameterMd : NameTypePairMd {
    //    public ParameterMd(string name, LocalTypeMd type)
    //        : base(name, type) {
    //    }
    //}


    //[Flags]
    //public enum FunctionFlags {
    //    None = 0,
    //    Idempotent = 1,
    //    Safe = Idempotent | 2,
    //    Static = 4,
    //    Extension = 8,
    //}

    //public sealed class FunctionMd {
    //    public FunctionMd(string name, FunctionFlags flags, LocalTypeMd returnType, ParameterMd[] parameters) {
    //        Name = name;
    //        Flags = flags;
    //        ReturnType = returnType;
    //        _parameters = parameters;
    //    }
    //    public readonly string Name;
    //    public readonly FunctionFlags Flags;
    //    public readonly LocalTypeMd ReturnType;
    //    private readonly ParameterMd[] _parameters;//opt
    //    public bool IsIdempotent {
    //        get { return (Flags & FunctionFlags.Idempotent) == FunctionFlags.Idempotent; }
    //    }
    //    public bool IsSafe {
    //        get { return (Flags & FunctionFlags.Safe) == FunctionFlags.Safe; }
    //    }
    //    public bool IsStatic {
    //        get { return (Flags & FunctionFlags.Static) == FunctionFlags.Static; }
    //    }
    //    public int ParameterCount {
    //        get { return _parameters == null ? 0 : _parameters.Length; }
    //    }

    //}

    public sealed class AtomTypeMd : GlobalTypeMd {
        public static AtomTypeMd Get(TypeKind kind) {
            return _map[kind];
        }
        public static AtomTypeMd Get(string name) {
            TypeKind kind;
            if (_nameMap.TryGetValue(name, out kind)) {
                return _map[kind];
            }
            return null;
        }
        public static TypeKind GetTypeKind(string name) {
            TypeKind kind;
            if (_nameMap.TryGetValue(name, out kind)) {
                return kind;
            }
            return TypeKind.None;
        }
        private AtomTypeMd(TypeKind kind)
            : base(kind, AtomExtensions.GetFullName(kind)) {
        }
        private static readonly Dictionary<string, TypeKind> _nameMap = new Dictionary<string, TypeKind> {
                { TypeKind.String.ToString(), TypeKind.String },
                { TypeKind.IgnoreCaseString.ToString(), TypeKind.IgnoreCaseString },
                { TypeKind.Char.ToString(), TypeKind.Char },
                { TypeKind.Decimal.ToString(), TypeKind.Decimal },
                { TypeKind.Int64.ToString(), TypeKind.Int64 },
                { TypeKind.Int32.ToString(), TypeKind.Int32 },
                { TypeKind.Int16.ToString(), TypeKind.Int16 },
                { TypeKind.SByte.ToString(), TypeKind.SByte },
                { TypeKind.UInt64.ToString(), TypeKind.UInt64 },
                { TypeKind.UInt32.ToString(), TypeKind.UInt32 },
                { TypeKind.UInt16.ToString(), TypeKind.UInt16 },
                { TypeKind.Byte.ToString(), TypeKind.Byte },
                { TypeKind.Double.ToString(), TypeKind.Double },
                { TypeKind.Single.ToString(), TypeKind.Single },
                { TypeKind.Boolean.ToString(), TypeKind.Boolean },
                { TypeKind.Binary.ToString(), TypeKind.Binary },
                { TypeKind.Guid.ToString(), TypeKind.Guid },
                { TypeKind.TimeSpan.ToString(), TypeKind.TimeSpan },
                { TypeKind.DateTimeOffset.ToString(), TypeKind.DateTimeOffset },
            };
        private static readonly Dictionary<TypeKind, AtomTypeMd> _map = new Dictionary<TypeKind, AtomTypeMd> {
                { TypeKind.String, new AtomTypeMd(TypeKind.String) },
                { TypeKind.IgnoreCaseString, new AtomTypeMd(TypeKind.IgnoreCaseString) },
                { TypeKind.Char, new AtomTypeMd(TypeKind.Char) },
                { TypeKind.Decimal, new AtomTypeMd(TypeKind.Decimal) },
                { TypeKind.Int64, new AtomTypeMd(TypeKind.Int64) },
                { TypeKind.Int32, new AtomTypeMd(TypeKind.Int32) },
                { TypeKind.Int16, new AtomTypeMd(TypeKind.Int16) },
                { TypeKind.SByte, new AtomTypeMd(TypeKind.SByte) },
                { TypeKind.UInt64, new AtomTypeMd(TypeKind.UInt64) },
                { TypeKind.UInt32, new AtomTypeMd(TypeKind.UInt32) },
                { TypeKind.UInt16, new AtomTypeMd(TypeKind.UInt16) },
                { TypeKind.Byte, new AtomTypeMd(TypeKind.Byte) },
                { TypeKind.Double, new AtomTypeMd(TypeKind.Double) },
                { TypeKind.Single, new AtomTypeMd(TypeKind.Single) },
                { TypeKind.Boolean, new AtomTypeMd(TypeKind.Boolean) },
                { TypeKind.Binary, new AtomTypeMd(TypeKind.Binary) },
                { TypeKind.Guid, new AtomTypeMd(TypeKind.Guid) },
                { TypeKind.TimeSpan, new AtomTypeMd(TypeKind.TimeSpan) },
                { TypeKind.DateTimeOffset, new AtomTypeMd(TypeKind.DateTimeOffset) },
            };

    }
    //public abstract class NamedTypePairMd {
    //    protected NamedTypePairMd(string name, LocalTypeMd type) {
    //        Name = name;
    //        Type = type;
    //    }
    //    public readonly string Name;
    //    public readonly LocalTypeMd Type;
    //}

    public struct EnumTypeMemberMd {
        public EnumTypeMemberMd(string name, object value) {
            Name = name;
            Value = value;
        }
        public readonly string Name;
        public readonly object Value;
    }
    public sealed class EnumTypeMd : GlobalTypeMd {
        public EnumTypeMd(FullName fullName, AtomTypeMd underlyingType, EnumTypeMemberMd[] members)
            : base(TypeKind.Enum, fullName) {
            UnderlyingType = underlyingType;
            _members = members;
        }
        public readonly AtomTypeMd UnderlyingType;
        private readonly EnumTypeMemberMd[] _members;
        //public EnumTypeMemberMd GetProperty(string name) {
        //    var props = _members;
        //    if (props != null) {
        //        var length = props.Length;
        //        for (var i = 0; i < length; ++i) {
        //            if (props[i].Name == name) {
        //                return props[i];
        //            }
        //        }
        //    }
        //    return null;
        //}
        //public object GetPropertyValue(string name) {
        //    var prop = GetProperty(name);
        //    if (prop != null) {
        //        return prop.Value;
        //    }
        //    return null;
        //}
        public string GetPropertyName(object value) {
            var props = _members;
            if (props != null) {
                var length = props.Length;
                for (var i = 0; i < length; ++i) {
                    if (props[i].Value.Equals(value)) {
                        return props[i].Name;
                    }
                }
            }
            return null;
        }

    }
    public sealed class ClassTypePropertyMd {
        public ClassTypePropertyMd(string name, LocalTypeMd type, string clrName, bool isClrProperty) {
            Name = name;
            Type = type;
            ClrName = clrName;
            IsClrProperty = isClrProperty;
        }
        public readonly string Name;
        public readonly LocalTypeMd Type;
        public readonly string ClrName;
        public readonly bool IsClrProperty;
        public PropertyInfo ClrProperty { get; private set; }
        public FieldInfo ClrField { get; private set; }
        internal void GetClrPropertyOrField(TypeInfo ti) {
            if (IsClrProperty) {
                ClrProperty = ReflectionExtensions.GetProperty(ti, ClrName);
            }
            else {
                ClrField = ReflectionExtensions.GetField(ti, ClrName);
            }
        }
        public object GetValue(object obj) {
            if (IsClrProperty) {
                return ClrProperty.GetValue(obj);
            }
            else {
                return ClrField.GetValue(obj);
            }
        }
        public void SetValue(object obj, object value) {
            if (IsClrProperty) {
                ClrProperty.SetValue(obj, value);
            }
            else {
                ClrField.SetValue(obj, value);
            }
        }
    }

    public sealed class ClassTypeMd : GlobalTypeMd {
        public ClassTypeMd(FullName fullName, bool isAbstract, ClassTypeMd baseClass,
            ClassTypePropertyMd[] properties, Type clrType)
            : base(TypeKind.Class, fullName) {
            IsAbstract = isAbstract;
            BaseClass = baseClass;
            _properties = properties;
            ClrType = clrType;
            TypeInfo ti = clrType.GetTypeInfo();
            if (properties != null) {
                foreach (var prop in properties) {
                    prop.GetClrPropertyOrField(ti);
                }
            }
            if (!isAbstract) {
                ClrConstructor = ReflectionExtensions.GetParameterlessConstructor(ti);
            }
            if (baseClass == null) {
                ClrMetadataProperty = ReflectionExtensions.GetProperty(ti, ReflectionExtensions.MetadataNameStr);
                ClrTextSpanProperty = ReflectionExtensions.GetProperty(ti, ReflectionExtensions.TextSpanNameStr);
                ClrUnknownPropertiesProperty = ReflectionExtensions.GetProperty(ti, ReflectionExtensions.UnknownPropertiesNameStr);
            }
            ClrOnLoadingMethod = ti.GetDeclaredMethod(ReflectionExtensions.OnLoadingNameStr);
            ClrOnLoadedMethod = ti.GetDeclaredMethod(ReflectionExtensions.OnLoadedNameStr);
        }
        public readonly bool IsAbstract;
        public readonly ClassTypeMd BaseClass;
        private readonly ClassTypePropertyMd[] _properties;
        public readonly Type ClrType;
        public readonly ConstructorInfo ClrConstructor;//for non abstract class
        public readonly PropertyInfo ClrMetadataProperty;//for top class
        public readonly PropertyInfo ClrTextSpanProperty;//for top class
        public readonly PropertyInfo ClrUnknownPropertiesProperty;//for top class
        public readonly MethodInfo ClrOnLoadingMethod;//opt
        public readonly MethodInfo ClrOnLoadedMethod;//opt
        public bool IsEqualToOrDeriveFrom(ClassTypeMd other) {
            for (var cls = this; cls != null; cls = cls.BaseClass) {
                if (cls == other) {
                    return true;
                }
            }
            return false;
        }
        public ClassTypePropertyMd GetPropertyInHierarchy(string name) {
            var props = _properties;
            if (props != null) {
                var length = props.Length;
                for (var i = 0; i < length; ++i) {
                    if (props[i].Name == name) {
                        return props[i];
                    }
                }
            }
            if (BaseClass != null) {
                return BaseClass.GetPropertyInHierarchy(name);
            }
            return null;
        }
        public void GetPropertiesInHierarchy(ref List<ClassTypePropertyMd> propList) {
            if (BaseClass != null) {
                BaseClass.GetPropertiesInHierarchy(ref propList);
            }
            if (_properties != null) {
                if (propList == null) {
                    propList = new List<ClassTypePropertyMd>(_properties);
                }
                else {
                    propList.AddRange(_properties);
                }
            }
        }
        public IEnumerable<ClassTypePropertyMd> GetPropertiesInHierarchy() {
            if (BaseClass == null) {
                return _properties;
            }
            if (_properties == null) {
                return BaseClass.GetPropertiesInHierarchy();
            }
            List<ClassTypePropertyMd> propList = null;
            GetPropertiesInHierarchy(ref propList);
            return propList;
        }
        public object CreateInstance() {
            //FormatterServices.GetUninitializedObject()
            return ClrConstructor.Invoke(null);
        }
        public ClassTypeMd GetMetadata(object obj) {
            var md = this;
            for (; md.BaseClass != null; md = md.BaseClass) ;
            return (ClassTypeMd)md.ClrMetadataProperty.GetValue(obj);
        }
        public void SetTextSpan(object obj, TextSpan value) {
            var md = this;
            for (; md.BaseClass != null; md = md.BaseClass) ;
            md.ClrTextSpanProperty.SetValue(obj, value);
        }
        public bool InvokeOnLoad(bool isLoading, object obj, Context context) {
            if (BaseClass != null) {
                if (!BaseClass.InvokeOnLoad(isLoading, obj, context)) {
                    return false;
                }
            }
            var mi = isLoading ? ClrOnLoadingMethod : ClrOnLoadedMethod;
            if (mi != null) {
                if (!(bool)mi.Invoke(obj, new object[] { context })) {
                    return false;
                }
            }
            return true;
        }
    }

    public struct NameValuePair222 {
        public NameValuePair222(string name, object value) {
            Name = name;
            Value = value;
        }
        public readonly string Name;
        public readonly object Value;
    }

}
