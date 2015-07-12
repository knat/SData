//using System;

//namespace SData {
//    internal struct IdNode : IEquatable<IdNode> {
//        public IdNode(string value, TextSpan textSpan) {
//            Value = value;
//            TextSpan = textSpan;
//        }
//        public readonly string Value;
//        public readonly TextSpan TextSpan;
//        public bool IsValid {
//            get {
//                return Value != null;
//            }
//        }
//        public override string ToString() {
//            return Value;
//        }
//        public bool Equals(IdNode other) {
//            return Value == other.Value;
//        }
//        public override bool Equals(object obj) {
//            return obj is IdNode && Equals((IdNode)obj);
//        }
//        public override int GetHashCode() {
//            return Value != null ? Value.GetHashCode() : 0;
//        }
//        public static bool operator ==(IdNode left, IdNode right) {
//            return left.Equals(right);
//        }
//        public static bool operator !=(IdNode left, IdNode right) {
//            return !left.Equals(right);
//        }
//    }
//    internal enum AtomValueKind : byte {
//        None = 0,
//        String,
//        Char,
//        Boolean,
//        Null,
//        Integer,
//        Decimal,
//        Real,
//    }
//    internal struct AtomValueNode {
//        public AtomValueNode(AtomValueKind kind, string value, TextSpan textSpan) {
//            Kind = kind;
//            Value = value;
//            TextSpan = textSpan;
//        }
//        public readonly AtomValueKind Kind;
//        public readonly string Value;
//        public readonly TextSpan TextSpan;
//        public bool IsValid {
//            get {
//                return Kind != AtomValueKind.None;
//            }
//        }
//        public bool IsNull {
//            get {
//                return Kind == AtomValueKind.Null;
//            }
//        }
//    }
//}
