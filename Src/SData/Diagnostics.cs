using System.Runtime.Serialization;
using SData.Internal;

namespace SData {
    public enum DiagnosticSeverity : byte {
        None = 0,
        Error = 1,
        Warning = 2,
        Info = 3
    }

    public enum DiagnosticCode {
        None = 0,
        Parsing = -300,
        DuplicateUriAlias,
        InvalidUriReference,
        InvalidClassReference,
        ClassNotEqualToOrDeriveFromTheDeclared,
        ClassIsAbstract,
        DuplicatePropertyName,
        PropertyMissing,
        NullNotAllowed,
        ValueExpected,
        SpecificValueExpected,
        InvalidAtomValue,
        InvalidEnumReference,
        EnumNotEqualToTheDeclared,
        InvalidEnumMemberName,
        DuplicateSetItem,
        DuplicateMapKey,

    }


    [DataContract(Namespace = Extensions.SystemUri)]
    public struct Diagnostic {
        public Diagnostic(DiagnosticSeverity severity, int code, string message, TextSpan textSpan) {
            Severity = severity;
            Code = code;
            Message = message;
            TextSpan = textSpan;
        }
        [DataMember]
        public readonly DiagnosticSeverity Severity;
        [DataMember]
        public readonly int Code;
        [DataMember]
        public readonly string Message;
        [DataMember]
        public readonly TextSpan TextSpan;//opt
        public bool IsError {
            get {
                return Severity == DiagnosticSeverity.Error;
            }
        }
        public bool IsWarning {
            get {
                return Severity == DiagnosticSeverity.Warning;
            }
        }
        public bool IsInfo {
            get {
                return Severity == DiagnosticSeverity.Info;
            }
        }
        public bool HasTextSpan {
            get {
                return TextSpan.IsValid;
            }
        }
        public bool IsValid {
            get {
                return Severity != DiagnosticSeverity.None;
            }
        }
        public override string ToString() {
            if (IsValid) {
                var sb = StringBuilderBuffer.Acquire();
                sb.Append(Severity.ToString());
                sb.Append(' ');
                sb.Append(Code.ToInvString());
                sb.Append(": ");
                sb.Append(Message);
                if (HasTextSpan) {
                    sb.Append("\r\n    ");
                    sb.Append(TextSpan.ToString());
                }
                return sb.ToStringAndRelease();
            }
            return null;
        }
    }


}
