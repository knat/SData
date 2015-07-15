using System.Collections.Generic;

namespace SData {
    //public struct NameValuePair {
    //    public NameValuePair(string name, object value) {
    //        Name = name;
    //        Value = value;
    //    }
    //    public readonly string Name;
    //    public readonly object Value;
    //}
    public class UntypedObject {
        public UntypedObject() { }
        public UntypedObject(FullName? classFullName, Dictionary<string, object> properties) {
            ClassFullName = classFullName;
            Properties = properties;
        }
        public FullName? ClassFullName { get; set; }
        public Dictionary<string, object> Properties { get; set; }
        //public TextSpan __TextSpan { get; set; }
    }
    public class UntypedEnumMember {
        public UntypedEnumMember() { }
        public UntypedEnumMember(FullName enumFullName, string memberName) {
            EnumFullName = enumFullName;
            MemberName = memberName;
        }
        public FullName EnumFullName { get; set; }
        public string MemberName { get; set; }
    }
    //public sealed class PropertyValue {
    //    public PropertyValue() { }
    //    public PropertyValue(string name, object value) {
    //        Name = name;
    //        Value = value;
    //    }
    //    public string Name { get; set; }
    //    public object Value { get; set; }
    //}

}
