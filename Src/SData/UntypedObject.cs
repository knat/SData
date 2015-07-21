using System.Collections.Generic;

namespace SData {
    public class UntypedObject {
        public UntypedObject() { }
        public UntypedObject(Dictionary<string, object> properties)
            : this(default(FullName), properties) {
        }
        public UntypedObject(FullName classFullName, Dictionary<string, object> properties) {
            ClassFullName = classFullName;
            Properties = properties;
        }
        public FullName ClassFullName { get; set; }
        public bool HasClassFullName {
            get { return ClassFullName.IsValid; }
        }
        public Dictionary<string, object> Properties { get; set; }
    }
    public class UntypedEnumValue {
        public UntypedEnumValue() { }
        public UntypedEnumValue(FullName enumFullName, string memberName) {
            EnumFullName = enumFullName;
            MemberName = memberName;
        }
        public FullName EnumFullName { get; set; }
        public string MemberName { get; set; }
    }

}
