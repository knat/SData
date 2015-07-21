using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using SData.Internal;

namespace SData {
    public abstract class ProgramMd {
        protected ProgramMd(GlobalTypeMd[] globalTypes) {
            if (globalTypes != null) {
                lock (_globalTypeMap) {
                    foreach (var globalType in globalTypes) {
                        _globalTypeMap.Add(globalType.FullName, globalType);
                    }
                }
            }
        }
        private static readonly Dictionary<FullName, GlobalTypeMd> _globalTypeMap = new Dictionary<FullName, GlobalTypeMd>();
        public static T TryGetGlobalType<T>(FullName fullName) where T : GlobalTypeMd {
            GlobalTypeMd globalType;
            _globalTypeMap.TryGetValue(fullName, out globalType);
            return globalType as T;
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
        //
        Class = 50,
        Enum = 51,
        //
        Nullable = 70,
        List = 71,
        Set = 72,
        Map = 73,

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
        public bool IsNullable {
            get {
                return Kind == TypeKind.Nullable;
            }
        }
        public NonNullableTypeMd NonNullableType {
            get {
                var nnt = this as NonNullableTypeMd;
                if (nnt != null) {
                    return nnt;
                }
                return (this as NullableTypeMd).ElementType;
            }
        }
    }
    public sealed class NullableTypeMd : LocalTypeMd {
        public NullableTypeMd(NonNullableTypeMd elementType)
            : base(TypeKind.Nullable) {
            ElementType = elementType;
        }
        public readonly NonNullableTypeMd ElementType;
    }

    public abstract class NonNullableTypeMd : LocalTypeMd {
        protected NonNullableTypeMd(TypeKind kind)
            : base(kind) {
        }
        public T TryGetGlobalType<T>() where T : GlobalTypeMd {
            var gtr = this as GlobalTypeRefMd;
            if (gtr != null) {
                return gtr.GlobalType as T;
            }
            return null;
        }
    }

    public sealed class CollectionTypeMd : NonNullableTypeMd {
        public CollectionTypeMd(TypeKind kind, LocalTypeMd itemOrValueType, GlobalTypeRefMd mapKeyType, Type clrType)
            : base(kind) {
            ItemOrValueType = itemOrValueType;
            MapKeyType = mapKeyType;
            ClrType = clrType;
            var ti = clrType.GetTypeInfo();
            ClrConstructor = ReflectionExtensions.GetParameterlessConstructor(ti);
            ClrAddMethod = ReflectionExtensions.GetMethodInHierarchy(ti, "Add");
            if (kind == TypeKind.Map) {
                ClrContainsKeyMethod = ReflectionExtensions.GetMethodInHierarchy(ti, "ContainsKey");
                ClrGetEnumeratorMethod = ReflectionExtensions.GetMethodInHierarchy(ti, "GetEnumerator");
            }
        }
        public readonly LocalTypeMd ItemOrValueType;
        public readonly GlobalTypeRefMd MapKeyType;//opt
        public readonly Type ClrType;
        public readonly ConstructorInfo ClrConstructor;
        public readonly MethodInfo ClrAddMethod;
        public readonly MethodInfo ClrContainsKeyMethod;//for map
        public readonly MethodInfo ClrGetEnumeratorMethod;//for map
        public object CreateInstance() {
            return ClrConstructor.Invoke(null);
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
    public sealed class GlobalTypeRefMd : NonNullableTypeMd {
        public GlobalTypeRefMd(GlobalTypeMd globalType)
            : base(globalType.Kind) {
            GlobalType = globalType;
        }
        public readonly GlobalTypeMd GlobalType;
    }

    public abstract class GlobalTypeMd : TypeMd {
        protected GlobalTypeMd(TypeKind kind, FullName fullName)
            : base(kind) {
            FullName = fullName;
        }
        public readonly FullName FullName;
    }
    public abstract class SimpleGlobalTypeMd : GlobalTypeMd {
        protected SimpleGlobalTypeMd(TypeKind kind, FullName fullName)
            : base(kind, fullName) {
        }
    }
    public sealed class AtomTypeMd : SimpleGlobalTypeMd {
        public static AtomTypeMd Get(TypeKind kind) {
            return _map[kind];
        }
        private AtomTypeMd(TypeKind kind)
            : base(kind, AtomExtensionsEx.GetFullName(kind)) {
        }
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
    public sealed class EnumTypeMd : SimpleGlobalTypeMd {
        public EnumTypeMd(FullName fullName, AtomTypeMd underlyingType, Dictionary<string, object> members)
            : base(TypeKind.Enum, fullName) {
            UnderlyingType = underlyingType;
            _members = members ?? _emptyMembers;
        }
        public readonly AtomTypeMd UnderlyingType;
        private static readonly Dictionary<string, object> _emptyMembers = new Dictionary<string, object>(0);
        internal readonly Dictionary<string, object> _members;
        public IReadOnlyDictionary<string, object> Members {
            get {
                return _members;
            }
        }
        public string TryGetMemberName(object value) {
            foreach (var kv in _members) {
                if (value.Equals(kv.Value)) {
                    return kv.Key;
                }
            }
            return null;
        }
    }
    public sealed class ClassTypeMd : GlobalTypeMd {
        public ClassTypeMd(FullName fullName, bool isAbstract, ClassTypeMd baseClass,
            KeyMd[] thisKeys, Dictionary<string, PropertyMd> thisPropertyMap, Type clrType)
            : base(TypeKind.Class, fullName) {
            IsAbstract = isAbstract;
            BaseClass = baseClass;
            ClrType = clrType;
            TypeInfo ti = clrType.GetTypeInfo();
            if (thisPropertyMap != null) {
                foreach (var prop in thisPropertyMap.Values) {
                    prop.Resolve(ti, this);
                }
            }
            if (!isAbstract) {
                ClrConstructor = ReflectionExtensions.GetParameterlessConstructor(ti);
            }
            if (baseClass == null) {
                ClrMetadataProperty = ReflectionExtensions.GetProperty(ti, ReflectionExtensions.MetadataNameStr);
                ClrUnknownPropertiesProperty = ReflectionExtensions.GetProperty(ti, ReflectionExtensions.UnknownPropertiesNameStr);
                _propertyMap = thisPropertyMap ?? _emptyPropertyMap;
            }
            else if (thisPropertyMap == null) {
                _propertyMap = baseClass._propertyMap;
            }
            else {
                var pm = new Dictionary<string, PropertyMd>(baseClass._propertyMap.Count + thisPropertyMap.Count);
                foreach (var kv in baseClass._propertyMap) {
                    pm.Add(kv.Key, kv.Value);
                }
                foreach (var kv in thisPropertyMap) {
                    pm.Add(kv.Key, kv.Value);
                }
                _propertyMap = pm;
            }
            if (thisKeys != null) {
                foreach (var key in thisKeys) {
                    key.Resolve(this);
                }
                _thisKeys = thisKeys;
            }
            ClrOnLoadingMethod = ti.GetDeclaredMethod(ReflectionExtensions.OnLoadingNameStr);
            ClrOnLoadedMethod = ti.GetDeclaredMethod(ReflectionExtensions.OnLoadedNameStr);
        }
        public readonly bool IsAbstract;
        public readonly ClassTypeMd BaseClass;//opt
        private static readonly Dictionary<string, PropertyMd> _emptyPropertyMap = new Dictionary<string, PropertyMd>(0);
        internal readonly Dictionary<string, PropertyMd> _propertyMap;//includes base properties
        public IReadOnlyDictionary<string, PropertyMd> PropertyMap {
            get { return _propertyMap; }
        }
        private readonly KeyMd[] _thisKeys;
        public IReadOnlyList<KeyMd> KeyList {//opt
            get {
                for (var md = this; md != null; md = md.BaseClass) {
                    if (md._thisKeys != null) {
                        return md._thisKeys;
                    }
                }
                return null;
            }
        }
        public bool Haskey {
            get { return KeyList != null; }
        }
        public readonly Type ClrType;
        public readonly ConstructorInfo ClrConstructor;//for non abstract class
        public readonly PropertyInfo ClrMetadataProperty;//for top class
        public readonly PropertyInfo ClrUnknownPropertiesProperty;//for top class
        public readonly MethodInfo ClrOnLoadingMethod;//opt
        public readonly MethodInfo ClrOnLoadedMethod;//opt
        public bool IsEqualToOrDeriveFrom(ClassTypeMd other) {
            for (var md = this; md != null; md = md.BaseClass) {
                if (md == other) {
                    return true;
                }
            }
            return false;
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
        public void SetUnknownProperties(object obj, Dictionary<string, object> value) {
            var md = this;
            for (; md.BaseClass != null; md = md.BaseClass) ;
            md.ClrUnknownPropertiesProperty.SetValue(obj, value);
        }
        public Dictionary<string, object> GetUnknownProperties(object obj) {
            var md = this;
            for (; md.BaseClass != null; md = md.BaseClass) ;
            return (Dictionary<string, object>)md.ClrUnknownPropertiesProperty.GetValue(obj);
        }
        public bool InvokeOnLoad(bool isLoading, object obj, LoadingContext context, TextSpan textSpan) {
            if (BaseClass != null) {
                if (!BaseClass.InvokeOnLoad(isLoading, obj, context, textSpan)) {
                    return false;
                }
            }
            var mi = isLoading ? ClrOnLoadingMethod : ClrOnLoadedMethod;
            if (mi != null) {
                if (!(bool)mi.Invoke(obj, new object[] { context, textSpan })) {
                    return false;
                }
            }
            return true;
        }
    }

    public sealed class PropertyMd {
        public PropertyMd(string name, LocalTypeMd type, string clrName, bool isClrProperty) {
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
        public ClassTypeMd ClassType { get; private set; }
        internal void Resolve(TypeInfo ti, ClassTypeMd classType) {
            if (IsClrProperty) {
                ClrProperty = ReflectionExtensions.GetProperty(ti, ClrName);
            }
            else {
                ClrField = ReflectionExtensions.GetField(ti, ClrName);
            }
            ClassType = classType;
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
    public sealed class KeyMd {
        public KeyMd(string[] propertyNames) {
            _propertyNames = propertyNames;
        }
        internal void Resolve(ClassTypeMd classType) {
            ClassType = classType;
            var propNames = _propertyNames;
            var count = propNames.Length;
            var props = new PropertyMd[count];
            for (var i = 0; i < count; ++i) {
                props[i] = classType._propertyMap[propNames[i]];
                var propType = props[i].Type;
                if (propType.Kind == TypeKind.Class) {
                    classType = propType.NonNullableType.TryGetGlobalType<ClassTypeMd>();
                }
            }
            _properties = props;
        }
        private readonly string[] _propertyNames;
        public IReadOnlyList<string> PropertyNameList {
            get {
                return _propertyNames;
            }
        }
        private PropertyMd[] _properties;
        public IReadOnlyList<PropertyMd> PropertyList {
            get {
                return _properties;
            }
        }
        public ClassTypeMd ClassType { get; private set; }
    }

}
