using System;

namespace SData.Internal {

    public struct DiagMsg {
        public DiagMsg(DiagnosticCode code) {
            Code = code;
            _msgArgs = null;
        }
        public DiagMsg(DiagnosticCode code, params string[] msgArgs) {
            Code = code;
            _msgArgs = msgArgs;
        }
        public readonly DiagnosticCode Code;
        private readonly string[] _msgArgs;
        public string GetMessage() {
            switch (Code) {
                case DiagnosticCode.DuplicateUriAlias:
                    return "Duplicate uri alias '{0}'.".InvFormat(_msgArgs);
                case DiagnosticCode.InvalidUriReference:
                    return "Invalid uri reference '{0}'.".InvFormat(_msgArgs);
                case DiagnosticCode.InvalidClassReference:
                    return "Invalid class reference '{0}'.".InvFormat(_msgArgs);
                case DiagnosticCode.ClassNotEqualToOrDeriveFromTheDeclared:
                    return "Class '{0}' not equal to or derive from the declared class '{1}'.".InvFormat(_msgArgs);
                case DiagnosticCode.ClassIsAbstract:
                    return "Class '{0}' is abstract.".InvFormat(_msgArgs);
                case DiagnosticCode.InvalidPropertyName:
                    return "Invalid property name '{0}'.".InvFormat(_msgArgs);
                case DiagnosticCode.PropertyMissing:
                    return "Property '{0}' missing.".InvFormat(_msgArgs);
                case DiagnosticCode.NullNotAllowed:
                    return "Null not allowed.";
                case DiagnosticCode.ValueExpected:
                    return "Value expetced.";
                case DiagnosticCode.SpecificValueExpected:
                    return "{0} value expetced.".InvFormat(_msgArgs);
                case DiagnosticCode.InvalidAtomValue:
                    return "Invalid atom '{0}' value '{1}'.".InvFormat(_msgArgs);
                case DiagnosticCode.InvalidEnumReference:
                    return "Invalid enum reference '{0}'.".InvFormat(_msgArgs);
                case DiagnosticCode.EnumNotEqualToTheDeclared:
                    return "Enum '{0}' not equal to the declared enum '{1}'.".InvFormat(_msgArgs);
                case DiagnosticCode.InvalidEnumMemberName:
                    return "Invalid enum member name '{0}'.".InvFormat(_msgArgs);
                case DiagnosticCode.DuplicateSetItem:
                    return "Duplicate set item.";
                case DiagnosticCode.DuplicateMapKey:
                    return "Duplicate map key.";

                default:
                    throw new InvalidOperationException("Invalid code: " + Code.ToString());
            }
        }
    }
}
