using System;

namespace SData
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
    public sealed class SchemaNamespaceAttribute : Attribute
    {
        public SchemaNamespaceAttribute(string uri, string namespaceName) { }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class SchemaClassAttribute : Attribute
    {
        public SchemaClassAttribute(string name) { }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class SchemaPropertyAttribute : Attribute
    {
        public SchemaPropertyAttribute(string name) { }
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
    public sealed class __CompilerSchemaNamespaceAttribute : Attribute
    {
        public __CompilerSchemaNamespaceAttribute(string uri, string namespaceName, string[] typeNames, string[] propertyNames) { }
    }

}
