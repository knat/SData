using System;
using System.Reflection;

namespace SData {
    public static class ReflectionExtensions {
        public const string ThisMetadataNameStr = "__ThisMetadata";
        public const string MetadataNameStr = "__Metadata";
        public const string TextSpanNameStr = "__TextSpan";
        public const string UnknownPropertiesNameStr = "__UnknownProperties";
        public const string OnLoadingNameStr = "OnLoading";
        public const string OnLoadedNameStr = "OnLoaded";
        //
        internal static ConstructorInfo TryGetParameterlessConstructor(TypeInfo ti) {
            foreach (var ci in ti.DeclaredConstructors) {
                if (ci.GetParameters().Length == 0) {
                    return ci;
                }
            }
            return null;
        }
        internal static ConstructorInfo GetParameterlessConstructor(TypeInfo ti) {
            var r = TryGetParameterlessConstructor(ti);
            if (r != null) return r;
            throw new ArgumentException("Cannot get parameterless constructor: " + ti.FullName);
        }
        internal static PropertyInfo TryGetPropertyInHierarchy(TypeInfo ti, string name) {
            while (true) {
                var pi = ti.GetDeclaredProperty(name);
                if (pi != null) {
                    return pi;
                }
                var baseType = ti.BaseType;
                if (baseType == null) {
                    return null;
                }
                ti = baseType.GetTypeInfo();
            }
        }
        internal static PropertyInfo GetPropertyInHierarchy(TypeInfo ti, string name) {
            var r = TryGetPropertyInHierarchy(ti, name);
            if (r != null) return r;
            throw new ArgumentException("Cannot get property: " + name);
        }
        internal static PropertyInfo GetProperty(TypeInfo ti, string name) {
            var r = ti.GetDeclaredProperty(name);
            if (r != null) return r;
            throw new ArgumentException("Cannot get property: " + name);
        }
        internal static FieldInfo TryGetFieldInHierarchy(TypeInfo ti, string name) {
            while (true) {
                var fi = ti.GetDeclaredField(name);
                if (fi != null) {
                    return fi;
                }
                var baseType = ti.BaseType;
                if (baseType == null) {
                    return null;
                }
                ti = baseType.GetTypeInfo();
            }
        }
        internal static FieldInfo GetFieldInHierarchy(TypeInfo ti, string name) {
            var r = TryGetFieldInHierarchy(ti, name);
            if (r != null) return r;
            throw new ArgumentException("Cannot get field: " + name);
        }
        internal static FieldInfo GetField(TypeInfo ti, string name) {
            var r = ti.GetDeclaredField(name);
            if (r != null) return r;
            throw new ArgumentException("Cannot get field: " + name);
        }
        internal static MethodInfo TryGetMethodInHierarchy(TypeInfo ti, string name) {
            while (true) {
                var mi = ti.GetDeclaredMethod(name);
                if (mi != null) {
                    return mi;
                }
                var baseType = ti.BaseType;
                if (baseType == null) {
                    return null;
                }
                ti = baseType.GetTypeInfo();
            }
        }
        internal static MethodInfo GetMethodInHierarchy(TypeInfo ti, string name) {
            var r = TryGetMethodInHierarchy(ti, name);
            if (r != null) return r;
            throw new ArgumentException("Cannot get method: " + name);
        }
        internal static MethodInfo GetMethod(TypeInfo ti, string name) {
            var r = ti.GetDeclaredMethod(name);
            if (r != null) return r;
            throw new ArgumentException("Cannot get method: " + name);
        }

    }
}
