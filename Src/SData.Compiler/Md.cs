//namespace "urn:SData:Compiler"
//{
//    class MdNamespace
//    {
//        GlobalTypeMap as map<String, MdGlobalType>
//    }

//    class MdGlobalType[abstract]
//    {
//        Name as String
//        CSName as String
//    }

//    class MdEnumType extends MdGlobalType
//    {
//    }

//    class MdClassType extends MdGlobalType
//    {
//        PropertyMap as map<String, MdProperty>
//    }

//    class MdClassTypeProperty
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
    partial class MdNamespace {
        private MdNamespace() { }
        internal MdNamespace(Dictionary<string, MdGlobalType> globalTypeMap) {
            GlobalTypeMap = globalTypeMap;
        }
        public readonly Dictionary<string, MdGlobalType> GlobalTypeMap;
    }
    partial class MdGlobalType {
        protected MdGlobalType() { }
        protected MdGlobalType(string name, string csName) {
            Name = name;
            CSName = csName;
        }
        public readonly string Name;
        public readonly string CSName;
    }
    partial class MdEnumType : MdGlobalType {
        private MdEnumType() { }
        internal MdEnumType(string name, string csName) : base(name, csName) { }
    }
    partial class MdClassType : MdGlobalType {
        private MdClassType() { }
        internal MdClassType(string name, string csName, Dictionary<string, MdClassTypeProperty> propertyMap)
            : base(name, csName) {
            PropertyMap = propertyMap;
        }
        public readonly Dictionary<string, MdClassTypeProperty> PropertyMap;
    }
    partial class MdClassTypeProperty {
        private MdClassTypeProperty() { }
        internal MdClassTypeProperty(string name, string csName, bool isCSProperty) {
            Name = name;
            CSName = csName;
            IsCSProperty = isCSProperty;
        }
        public readonly string Name;
        public readonly string CSName;
        public readonly bool IsCSProperty;
    }
}
