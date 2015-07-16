//namespace "urn:SData:Compiler"
//{
//    class MdNamespace
//    {
//        GlobalTypeList as list<MdGlobalType>
//    }

//    class MdGlobalType[abstract]
//    {
//        Name as String,
//        CSName as String
//    }

//    class MdEnum extends MdGlobalType
//    {
//    }

//    class MdClass extends MdGlobalType
//    {
//        PropertyList as list<MdProperty>
//    }

//    class MdProperty
//    {
//        Name as String,
//        CSName as String,
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
        internal MdNamespace(List<MdGlobalType> globalTypeList) {
            GlobalTypeList = globalTypeList;
        }
        public readonly List<MdGlobalType> GlobalTypeList;
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
    partial class MdEnum : MdGlobalType {
        private MdEnum() { }
        internal MdEnum(string name, string csName) : base(name, csName) { }
    }
    partial class MdClass : MdGlobalType {
        private MdClass() { }
        internal MdClass(string name, string csName, List<MdProperty> propertyList)
            : base(name, csName) {
            PropertyList = propertyList;
        }
        public readonly List<MdProperty> PropertyList;
    }
    partial class MdProperty {
        private MdProperty() { }
        internal MdProperty(string name, string csName, bool isCSProperty) {
            Name = name;
            CSName = csName;
            IsCSProperty = isCSProperty;
        }
        public readonly string Name;
        public readonly string CSName;
        public readonly bool IsCSProperty;
    }
}
