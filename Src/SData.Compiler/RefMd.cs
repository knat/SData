//namespace "urn:SData:Compiler"
//{
//    class RefMdNamespace
//    {
//        GlobalTypeMap as map<String, MdGlobalType>
//    }

//    class RefMdGlobalType[abstract]
//    {
//        CSName as String
//    }

//    class RefMdEnumType extends RefMdGlobalType
//    {
//    }

//    class RefMdClassType extends RefMdGlobalType
//    {
//        PropertyMap as map<String, MdProperty>
//    }

//    class RefMdClassTypeProperty
//    {
//        Name as String
//        CSName as String
//        IsCSProperty as Boolean
//    }
//}

using System;
using System.Collections.Generic;
using SData;

[assembly: SchemaNamespace("urn:SData:Compiler", "SData.Compiler")]

namespace SData.Compiler {
    partial class RefMdNamespace {
        private RefMdNamespace() { }
        internal RefMdNamespace(Dictionary<string, RefMdGlobalType> globalTypeMap) {
            GlobalTypeMap = globalTypeMap;
        }
        public readonly Dictionary<string, RefMdGlobalType> GlobalTypeMap;
    }
    partial class RefMdGlobalType {
        protected RefMdGlobalType() { }
        protected RefMdGlobalType(string csName) {
            CSName = csName;
        }
        public readonly string CSName;
    }
    partial class RefMdEnumType : RefMdGlobalType {
        private RefMdEnumType() { }
        internal RefMdEnumType(string csName) : base(csName) { }
    }
    partial class RefMdClassType : RefMdGlobalType {
        private RefMdClassType() { }
        internal RefMdClassType(string csName, Dictionary<string, RefMdClassTypeProperty> propertyMap)
            : base(csName) {
            PropertyMap = propertyMap;
        }
        public readonly Dictionary<string, RefMdClassTypeProperty> PropertyMap;
    }
    partial class RefMdClassTypeProperty {
        private RefMdClassTypeProperty() { }
        internal RefMdClassTypeProperty(string csName, bool isCSProperty) {
            CSName = csName;
            IsCSProperty = isCSProperty;
        }
        public readonly string CSName;
        public readonly bool IsCSProperty;
    }
}
