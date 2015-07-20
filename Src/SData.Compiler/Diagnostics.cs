using System;
using SData.Internal;

namespace SData.Compiler {
    internal enum DiagCodeEx {
        None = 0,
        InternalCompilerError = -2000,
        //
        AliasSysReserved,
        UriSystemReserved,
        DuplicateNamespaceAlias,
        InvalidNamespaceReference,
        DuplicateGlobalTypeName,
        DuplicateEnumMemberName,
        DuplicatePropertyName,
        SpecificTypeExpected,
        InvalidNamespaceAliasReference,
        AmbiguousGlobalTypeReference,
        InvalidGlobalTypeReference,
        InvalidClassReference,
        InvalidAtomReference,
        InvalidSimpleGlobalTypeReference,
        CircularReferenceNotAllowed,
        InvalidAtomValue,
        BaseClassIsSealed,
        KeySelectorNotAllowedForSimpleSet,
        KeySelectorRequiredForObjectSet,
        InvalidPropertyReference,
        ObjectSetKeyCannotBeNullable,
        InvalidObjectSetKey,
        ObjectSetKeyMustBeSimpleType,

        //
        InvalidSchemaNamespaceAttribute,
        InvalidSchemaNamespaceAttributeUri,
        DuplicateSchemaNamespaceAttributeUri,
        InvalidSchemaNamespaceAttributeNamespaceName,
        SchemaNamespaceAttributeRequired,
        Invalid__CompilerSchemaNamespaceAttribute,
        //
        InvalidSchemaClassAttribute,
        InvalidSchemaClassAttributeName,
        DuplicateSchemaClassAttributeName,
        SchemaClassCannotBeGeneric,
        SchemaClassCannotBeStatic,
        NonAbstractSchemaClassRequired,
        ParameterlessConstructorRequired,
        //
        InvalidSchemaPropertyAttribute,
        InvalidSchemaPropertyAttributeName,
        DuplicateSchemaPropertyAttributeName,
        SchemaPropertyCannotBeStatic,
        SchemaPropertyMustHaveGetterAndSetter,
        SchemaPropertyCannotBeIndexer,
        SchemaFieldCannotBeConst,
        InvalidSchemaPropertyType,
        InvalidSchemaPropertyTypeOrExplicitTypeExpected,


    }
    internal struct DiagMsgEx {
        public DiagMsgEx(DiagCodeEx code) {
            Code = code;
            _msgArgs = null;
        }
        public DiagMsgEx(DiagCodeEx code, params string[] msgArgs) {
            Code = code;
            _msgArgs = msgArgs;
        }
        public readonly DiagCodeEx Code;
        private readonly string[] _msgArgs;
        public string GetMessage() {
            switch (Code) {
                //
                case DiagCodeEx.AliasSysReserved:
                    return "Alias 'sys' is reserved.";
                case DiagCodeEx.UriSystemReserved:
                    return "Uri '" + Extensions.SystemUri + "' is reserved.";
                case DiagCodeEx.DuplicateNamespaceAlias:
                    return "Duplicate namespace alias '{0}'.".InvFormat(_msgArgs);
                case DiagCodeEx.InvalidNamespaceReference:
                    return "Invalid namespace reference '{0}'.".InvFormat(_msgArgs);
                case DiagCodeEx.DuplicateGlobalTypeName:
                    return "Duplicate global type name '{0}'.".InvFormat(_msgArgs);
                case DiagCodeEx.DuplicateEnumMemberName:
                    return "Duplicate enum member name '{0}'.".InvFormat(_msgArgs);
                case DiagCodeEx.DuplicatePropertyName:
                    return "Duplicate property name '{0}'.".InvFormat(_msgArgs);
                case DiagCodeEx.SpecificTypeExpected:
                    return "{0} type expected.".InvFormat(_msgArgs);
                case DiagCodeEx.InvalidNamespaceAliasReference:
                    return "Invalid namespace alias reference '{0}'.".InvFormat(_msgArgs);
                case DiagCodeEx.AmbiguousGlobalTypeReference:
                    return "Ambiguous global type reference '{0}'.".InvFormat(_msgArgs);
                case DiagCodeEx.InvalidGlobalTypeReference:
                    return "Invalid global type reference '{0}'.".InvFormat(_msgArgs);
                case DiagCodeEx.InvalidClassReference:
                    return "Invalid class reference '{0}'.".InvFormat(_msgArgs);
                case DiagCodeEx.InvalidAtomReference:
                    return "Invalid atom reference '{0}'.".InvFormat(_msgArgs);
                case DiagCodeEx.InvalidSimpleGlobalTypeReference:
                    return "Invalid simple global type(atom or enum) reference '{0}'.".InvFormat(_msgArgs);
                case DiagCodeEx.CircularReferenceNotAllowed:
                    return "Circular reference not allowed.";
                case DiagCodeEx.InvalidAtomValue:
                    return "Invalid atom '{0}' value '{1}'.".InvFormat(_msgArgs);
                case DiagCodeEx.BaseClassIsSealed:
                    return "Base class is sealed.";
                case DiagCodeEx.KeySelectorNotAllowedForSimpleSet:
                    return "Key selector not allowed for simple set.";
                case DiagCodeEx.KeySelectorRequiredForObjectSet:
                    return "Key selector required for object set.";
                case DiagCodeEx.InvalidPropertyReference:
                    return "Invalid property reference '{0}'.".InvFormat(_msgArgs);
                case DiagCodeEx.ObjectSetKeyCannotBeNullable:
                    return "Object set key cannot be nullable.";
                case DiagCodeEx.InvalidObjectSetKey:
                    return "Invalid object set key.";
                case DiagCodeEx.ObjectSetKeyMustBeSimpleType:
                    return "Object set key must be simple type.";

                //
                case DiagCodeEx.InvalidSchemaNamespaceAttribute:
                    return "Invalid SchemaNamespaceAttribute.";
                case DiagCodeEx.InvalidSchemaNamespaceAttributeUri:
                    return "Invalid SchemaNamespaceAttribute uri '{0}'.".InvFormat(_msgArgs);
                case DiagCodeEx.DuplicateSchemaNamespaceAttributeUri:
                    return "Duplicate SchemaNamespaceAttribute uri '{0}'.".InvFormat(_msgArgs);
                case DiagCodeEx.InvalidSchemaNamespaceAttributeNamespaceName:
                    return "Invalid SchemaNamespaceAttribute namespaceName '{0}'.".InvFormat(_msgArgs);
                case DiagCodeEx.SchemaNamespaceAttributeRequired:
                    return "SchemaNamespaceAttribute required for uri '{0}'.".InvFormat(_msgArgs);
                case DiagCodeEx.Invalid__CompilerSchemaNamespaceAttribute:
                    return "Invalid __CompilerSchemaNamespaceAttribute. assembly: '{0}', uri argument: '{1}', namespaceName argument: '{2}'. Make sure one schema namespace is implemented in only one assembly, or you should rebuild that assembly.".InvFormat(_msgArgs);
                //
                case DiagCodeEx.InvalidSchemaClassAttribute:
                    return "Invalid SchemaClassAttribute.";
                case DiagCodeEx.InvalidSchemaClassAttributeName:
                    return "Invalid SchemaClassAttribute name '{0}'.".InvFormat(_msgArgs);
                case DiagCodeEx.DuplicateSchemaClassAttributeName:
                    return "Duplicate SchemaClassAttribute name '{0}'.".InvFormat(_msgArgs);
                case DiagCodeEx.SchemaClassCannotBeGeneric:
                    return "Schema class cannot be generic.";
                case DiagCodeEx.SchemaClassCannotBeStatic:
                    return "Schema class cannot be static.";
                case DiagCodeEx.NonAbstractSchemaClassRequired:
                    return "Non-abstract class required.";
                case DiagCodeEx.ParameterlessConstructorRequired:
                    return "Parameterless constructor required.";
                //
                case DiagCodeEx.InvalidSchemaPropertyAttribute:
                    return "Invalid SchemaPropertyAttribute.";
                case DiagCodeEx.InvalidSchemaPropertyAttributeName:
                    return "Invalid SchemaPropertyAttribute name '{0}'.".InvFormat(_msgArgs);
                case DiagCodeEx.DuplicateSchemaPropertyAttributeName:
                    return "Duplicate SchemaPropertyAttribute name '{0}'.".InvFormat(_msgArgs);
                case DiagCodeEx.SchemaPropertyCannotBeStatic:
                    return "Schema property/field cannot be static.";
                case DiagCodeEx.SchemaPropertyMustHaveGetterAndSetter:
                    return "Schema property must have getter and setter.";
                case DiagCodeEx.SchemaPropertyCannotBeIndexer:
                    return "Schema property cannot be indexer.";
                case DiagCodeEx.SchemaFieldCannotBeConst:
                    return "Schema field cannot be const.";
                case DiagCodeEx.InvalidSchemaPropertyType:
                    return "Invalid schema property/field{0} type. {1} expected.".InvFormat(_msgArgs);
                case DiagCodeEx.InvalidSchemaPropertyTypeOrExplicitTypeExpected:
                    return "Invalid schema property/field{0} type. {1} expected, or you should declare the C# partial class explicitly.".InvFormat(_msgArgs);

                default:
                    throw new InvalidOperationException("Invalid code: " + Code.ToString());
            }
        }
    }

    

}
