using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SData.Compiler {
    internal static class CS {
        internal static string EscapeId(this string text) {
            return SyntaxFacts.GetKeywordKind(text) == SyntaxKind.None ? text : "@" + text;
        }
        internal static string UnescapeId(this string text) {
            return text[0] == '@' ? text.Substring(1) : text;
        }
        //e.g: Id("@class")
        internal static SyntaxToken Id(string escapedId) {
            return SyntaxFactory.Identifier(default(SyntaxTriviaList), SyntaxKind.IdentifierToken,
                escapedId, escapedId.UnescapeId(), default(SyntaxTriviaList));
        }
        //e.g: Id("class")
        internal static SyntaxToken UnescapedId(string unescapedId) {
            return SyntaxFactory.Identifier(default(SyntaxTriviaList), SyntaxKind.IdentifierToken,
               unescapedId.EscapeId(), unescapedId, default(SyntaxTriviaList));
        }
        internal static IdentifierNameSyntax IdName(string escapedId) {
            return SyntaxFactory.IdentifierName(Id(escapedId));
        }
        internal static IdentifierNameSyntax UnescapedIdName(string unescapedId) {
            return SyntaxFactory.IdentifierName(UnescapedId(unescapedId));
        }
        internal static IdentifierNameSyntax IdName(SyntaxToken identifier) {
            return SyntaxFactory.IdentifierName(identifier);
        }
        //
        internal static SyntaxToken PrivateToken {
            get { return SyntaxFactory.Token(SyntaxKind.PrivateKeyword); }
        }
        internal static SyntaxToken ProtectedToken {
            get { return SyntaxFactory.Token(SyntaxKind.ProtectedKeyword); }
        }
        internal static SyntaxToken InternalToken {
            get { return SyntaxFactory.Token(SyntaxKind.InternalKeyword); }
        }
        internal static SyntaxToken PublicToken {
            get { return SyntaxFactory.Token(SyntaxKind.PublicKeyword); }
        }
        internal static SyntaxToken AbstractToken {
            get { return SyntaxFactory.Token(SyntaxKind.AbstractKeyword); }
        }
        internal static SyntaxToken SealedToken {
            get { return SyntaxFactory.Token(SyntaxKind.SealedKeyword); }
        }
        internal static SyntaxToken StaticToken {
            get { return SyntaxFactory.Token(SyntaxKind.StaticKeyword); }
        }
        internal static SyntaxToken PartialToken {
            get { return SyntaxFactory.Token(SyntaxKind.PartialKeyword); }
        }
        internal static SyntaxToken NewToken {
            get { return SyntaxFactory.Token(SyntaxKind.NewKeyword); }
        }
        internal static SyntaxToken ConstToken {
            get { return SyntaxFactory.Token(SyntaxKind.ConstKeyword); }
        }
        internal static SyntaxToken ReadOnlyToken {
            get { return SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword); }
        }
        internal static SyntaxToken VirtualToken {
            get { return SyntaxFactory.Token(SyntaxKind.VirtualKeyword); }
        }
        internal static SyntaxToken OverrideToken {
            get { return SyntaxFactory.Token(SyntaxKind.OverrideKeyword); }
        }
        internal static SyntaxToken VolatileToken {
            get { return SyntaxFactory.Token(SyntaxKind.VolatileKeyword); }
        }
        internal static SyntaxToken InToken {
            get { return SyntaxFactory.Token(SyntaxKind.InKeyword); }
        }
        internal static SyntaxToken RefToken {
            get { return SyntaxFactory.Token(SyntaxKind.RefKeyword); }
        }
        internal static SyntaxToken OutToken {
            get { return SyntaxFactory.Token(SyntaxKind.OutKeyword); }
        }
        internal static SyntaxToken ThisToken {
            get { return SyntaxFactory.Token(SyntaxKind.ThisKeyword); }
        }
        internal static SyntaxToken GetToken {
            get { return SyntaxFactory.Token(SyntaxKind.GetKeyword); }
        }
        internal static SyntaxToken SetToken {
            get { return SyntaxFactory.Token(SyntaxKind.SetKeyword); }
        }
        internal static SyntaxToken AddToken {
            get { return SyntaxFactory.Token(SyntaxKind.AddKeyword); }
        }
        internal static SyntaxToken RemoveToken {
            get { return SyntaxFactory.Token(SyntaxKind.RemoveKeyword); }
        }
        internal static SyntaxToken ParamsToken {
            get { return SyntaxFactory.Token(SyntaxKind.ParamsKeyword); }
        }
        internal static SyntaxToken ImplictToken {
            get { return SyntaxFactory.Token(SyntaxKind.ImplicitKeyword); }
        }
        internal static SyntaxToken ExplictToken {
            get { return SyntaxFactory.Token(SyntaxKind.ExplicitKeyword); }
        }
        internal static SyntaxToken SemicolonToken {
            get { return SyntaxFactory.Token(SyntaxKind.SemicolonToken); }
        }
        internal static SyntaxToken CommaToken {
            get { return SyntaxFactory.Token(SyntaxKind.CommaToken); }
        }
        internal static SyntaxToken EqualsEqualsToken {
            get { return SyntaxFactory.Token(SyntaxKind.EqualsEqualsToken); }
        }
        internal static SyntaxToken ExclamationEqualsToken {
            get { return SyntaxFactory.Token(SyntaxKind.ExclamationEqualsToken); }
        }
        internal static SyntaxTokenList PublicTokenList {
            get { return SyntaxFactory.TokenList(PublicToken); }
        }
        internal static SyntaxTokenList NewPublicTokenList {
            get { return SyntaxFactory.TokenList(NewToken, PublicToken); }
        }
        internal static SyntaxTokenList NewPublicPartialTokenList {
            get { return SyntaxFactory.TokenList(NewToken, PublicToken, PartialToken); }
        }
        internal static SyntaxTokenList PublicPartialTokenList {
            get { return SyntaxFactory.TokenList(PublicToken, PartialToken); }
        }
        internal static SyntaxTokenList PublicAbstractPartialTokenList {
            get { return SyntaxFactory.TokenList(PublicToken, AbstractToken, PartialToken); }
        }
        internal static SyntaxTokenList NewPublicAbstractPartialTokenList {
            get { return SyntaxFactory.TokenList(NewToken, PublicToken, AbstractToken, PartialToken); }
        }
        internal static SyntaxTokenList PublicVirtualTokenList {
            get { return SyntaxFactory.TokenList(PublicToken, VirtualToken); }
        }
        internal static SyntaxTokenList PublicOverrideTokenList {
            get { return SyntaxFactory.TokenList(PublicToken, OverrideToken); }
        }
        internal static SyntaxTokenList PublicSealedTokenList {
            get { return SyntaxFactory.TokenList(PublicToken, SealedToken); }
        }
        internal static SyntaxTokenList PublicStaticTokenList {
            get { return SyntaxFactory.TokenList(PublicToken, StaticToken); }
        }
        internal static SyntaxTokenList PublicStaticPartialTokenList {
            get { return SyntaxFactory.TokenList(PublicToken, StaticToken, PartialToken); }
        }
        internal static SyntaxTokenList PublicStaticReadOnlyTokenList {
            get { return SyntaxFactory.TokenList(PublicToken, StaticToken, ReadOnlyToken); }
        }
        internal static SyntaxTokenList NewPublicStaticReadOnlyTokenList {
            get { return SyntaxFactory.TokenList(NewToken, PublicToken, StaticToken, ReadOnlyToken); }
        }
        internal static SyntaxTokenList PublicConstTokenList {
            get { return SyntaxFactory.TokenList(PublicToken, ConstToken); }
        }
        internal static SyntaxTokenList ConstTokenList {
            get { return SyntaxFactory.TokenList(ConstToken); }
        }
        internal static SyntaxTokenList NewPublicConstTokenList {
            get { return SyntaxFactory.TokenList(NewToken, PublicToken, ConstToken); }
        }
        internal static SyntaxTokenList ProtectedTokenList {
            get { return SyntaxFactory.TokenList(ProtectedToken); }
        }
        internal static SyntaxTokenList ProtectedOverrideTokenList {
            get { return SyntaxFactory.TokenList(ProtectedToken, OverrideToken); }
        }
        internal static SyntaxTokenList InternalTokenList {
            get { return SyntaxFactory.TokenList(InternalToken); }
        }
        internal static SyntaxTokenList InternalReadOnlyTokenList {
            get { return SyntaxFactory.TokenList(InternalToken, ReadOnlyToken); }
        }
        internal static SyntaxTokenList InternalSealedTokenList {
            get { return SyntaxFactory.TokenList(InternalToken, SealedToken); }
        }
        internal static SyntaxTokenList InternalStaticTokenList {
            get { return SyntaxFactory.TokenList(InternalToken, StaticToken); }
        }
        internal static SyntaxTokenList ProtectedInternalTokenList {
            get { return SyntaxFactory.TokenList(ProtectedToken, InternalToken); }
        }
        internal static SyntaxTokenList PrivateTokenList {
            get { return SyntaxFactory.TokenList(PrivateToken); }
        }
        internal static SyntaxTokenList PrivateStaticTokenList {
            get { return SyntaxFactory.TokenList(PrivateToken, StaticToken); }
        }
        internal static SyntaxTokenList PrivateStaticReadOnlyTokenList {
            get { return SyntaxFactory.TokenList(PrivateToken, StaticToken, ReadOnlyToken); }
        }
        internal static SyntaxTokenList PrivateStaticVolatileTokenList {
            get { return SyntaxFactory.TokenList(PrivateToken, StaticToken, VolatileToken); }
        }
        internal static SyntaxTokenList PrivateReadOnlyTokenList {
            get { return SyntaxFactory.TokenList(PrivateToken, ReadOnlyToken); }
        }
        internal static SyntaxTokenList PrivateVolatileTokenList {
            get { return SyntaxFactory.TokenList(PrivateToken, VolatileToken); }
        }
        internal static SyntaxTokenList PartialTokenList {
            get { return SyntaxFactory.TokenList(PartialToken); }
        }
        internal static SyntaxTokenList RefTokenList {
            get { return SyntaxFactory.TokenList(RefToken); }
        }
        internal static SyntaxTokenList OutTokenList {
            get { return SyntaxFactory.TokenList(OutToken); }
        }
        internal static SyntaxTokenList ThisTokenList {
            get { return SyntaxFactory.TokenList(ThisToken); }
        }
        internal static SyntaxTokenList ParamsTokenList {
            get { return SyntaxFactory.TokenList(ParamsToken); }
        }
        //
        //
        private static SyntaxList<ArrayRankSpecifierSyntax> OneDimArrayTypeRankSpecifiers {
            get {
                return SyntaxFactory.SingletonList(SyntaxFactory.ArrayRankSpecifier(
                    SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(SyntaxFactory.OmittedArraySizeExpression())));
            }
        }
        internal static ArrayTypeSyntax OneDimArrayType(TypeSyntax elementType) {
            return SyntaxFactory.ArrayType(elementType, OneDimArrayTypeRankSpecifiers);
        }
        //
        internal static IdentifierNameSyntax VarIdName {
            get { return IdName("var"); }
        }
        internal static IdentifierNameSyntax GlobalIdName {
            get { return IdName(SyntaxFactory.Token(SyntaxKind.GlobalKeyword)); }
        }
        internal static PredefinedTypeSyntax VoidType {
            get { return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)); }
        }
        internal static PredefinedTypeSyntax ObjectType {
            get { return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword)); }
        }
        internal static ArrayTypeSyntax ObjectArrayType {
            get { return OneDimArrayType(ObjectType); }
        }
        internal static LiteralExpressionSyntax NullLiteral {
            get { return SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression); }
        }
        //
        internal static PredefinedTypeSyntax BoolType {
            get { return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword)); }
        }
        internal static NullableTypeSyntax BoolNullableType {
            get { return SyntaxFactory.NullableType(BoolType); }
        }
        internal static LiteralExpressionSyntax TrueLiteral {
            get { return SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression); }
        }
        internal static LiteralExpressionSyntax FalseLiteral {
            get { return SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression); }
        }
        internal static LiteralExpressionSyntax Literal(bool value) {
            return value ? TrueLiteral : FalseLiteral;
        }
        internal static ExpressionSyntax Literal(bool? value) {
            if (value == null) return NullLiteral;
            return Literal(value.Value);
        }
        //
        internal static PredefinedTypeSyntax StringType {
            get { return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)); }
        }
        internal static ArrayTypeSyntax StringArrayType {
            get { return OneDimArrayType(StringType); }
        }
        internal static ExpressionSyntax Literal(string value) {
            if (value == null) return NullLiteral;
            return SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(value));
        }
        internal static PredefinedTypeSyntax CharType {
            get { return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.CharKeyword)); }
        }
        internal static NullableTypeSyntax CharNullableType {
            get { return SyntaxFactory.NullableType(CharType); }
        }
        internal static ArrayTypeSyntax CharArrayType {
            get { return OneDimArrayType(CharType); }
        }
        internal static ExpressionSyntax Literal(char value) {
            return SyntaxFactory.LiteralExpression(SyntaxKind.CharacterLiteralExpression, SyntaxFactory.Literal(value));
        }


        //
        internal static PredefinedTypeSyntax IntType {
            get { return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)); }
        }
        internal static NullableTypeSyntax IntNullableType {
            get { return SyntaxFactory.NullableType(IntType); }
        }
        internal static LiteralExpressionSyntax Literal(int value) {
            return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(value));
        }
        internal static ExpressionSyntax Literal(int? value) {
            if (value == null) return NullLiteral;
            return Literal(value.Value);
        }
        //
        internal static PredefinedTypeSyntax UIntType {
            get { return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.UIntKeyword)); }
        }
        internal static NullableTypeSyntax UIntNullableType {
            get { return SyntaxFactory.NullableType(UIntType); }
        }
        internal static LiteralExpressionSyntax Literal(uint value) {
            return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(value));
        }
        internal static ExpressionSyntax Literal(uint? value) {
            if (value == null) return NullLiteral;
            return Literal(value.Value);
        }
        //
        internal static PredefinedTypeSyntax LongType {
            get { return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.LongKeyword)); }
        }
        internal static NullableTypeSyntax LongNullableType {
            get { return SyntaxFactory.NullableType(LongType); }
        }
        internal static LiteralExpressionSyntax Literal(long value) {
            return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(value));
        }
        internal static ExpressionSyntax Literal(long? value) {
            if (value == null) return NullLiteral;
            return Literal(value.Value);
        }
        //
        internal static PredefinedTypeSyntax ULongType {
            get { return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ULongKeyword)); }
        }
        internal static NullableTypeSyntax ULongNullableType {
            get { return SyntaxFactory.NullableType(ULongType); }
        }
        internal static LiteralExpressionSyntax Literal(ulong value) {
            return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(value));
        }
        internal static ExpressionSyntax Literal(ulong? value) {
            if (value == null) return NullLiteral;
            return Literal(value.Value);
        }
        //
        internal static PredefinedTypeSyntax ShortType {
            get { return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ShortKeyword)); }
        }
        internal static NullableTypeSyntax ShortNullableType {
            get { return SyntaxFactory.NullableType(ShortType); }
        }
        internal static ExpressionSyntax Literal(short value) {
            return SyntaxFactory.CastExpression(ShortType, Literal((int)value));
        }
        internal static ExpressionSyntax Literal(short? value) {
            if (value == null) return NullLiteral;
            return Literal(value.Value);
        }
        //
        internal static PredefinedTypeSyntax UShortType {
            get { return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.UShortKeyword)); }
        }
        internal static NullableTypeSyntax UShortNullableType {
            get { return SyntaxFactory.NullableType(UShortType); }
        }
        internal static ExpressionSyntax Literal(ushort value) {
            return SyntaxFactory.CastExpression(UShortType, Literal((int)value));
        }
        internal static ExpressionSyntax Literal(ushort? value) {
            if (value == null) return NullLiteral;
            return Literal(value.Value);
        }
        //
        internal static PredefinedTypeSyntax ByteType {
            get { return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ByteKeyword)); }
        }
        internal static NullableTypeSyntax ByteNullableType {
            get { return SyntaxFactory.NullableType(ByteType); }
        }
        internal static ArrayTypeSyntax ByteArrayType {
            get { return OneDimArrayType(ByteType); }
        }
        internal static ExpressionSyntax Literal(byte value) {
            return SyntaxFactory.CastExpression(ByteType, Literal((int)value));
        }
        internal static ExpressionSyntax Literal(byte? value) {
            if (value == null) return NullLiteral;
            return Literal(value.Value);
        }
        internal static ExpressionSyntax Literal(byte[] value) {
            if (value == null) return NullLiteral;
            return NewArrExpr(ByteArrayType, value.Select(i => Literal((int)i)));
        }
        //
        internal static PredefinedTypeSyntax SByteType {
            get { return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.SByteKeyword)); }
        }
        internal static NullableTypeSyntax SByteNullableType {
            get { return SyntaxFactory.NullableType(SByteType); }
        }
        internal static ExpressionSyntax Literal(sbyte value) {
            return SyntaxFactory.CastExpression(SByteType, Literal((int)value));
        }
        internal static ExpressionSyntax Literal(sbyte? value) {
            if (value == null) return NullLiteral;
            return Literal(value.Value);
        }
        //
        internal static PredefinedTypeSyntax DecimalType {
            get { return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.DecimalKeyword)); }
        }
        internal static NullableTypeSyntax DecimalNullableType {
            get { return SyntaxFactory.NullableType(DecimalType); }
        }
        internal static LiteralExpressionSyntax Literal(decimal value) {
            return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(value));
        }
        internal static ExpressionSyntax Literal(decimal? value) {
            if (value == null) return NullLiteral;
            return Literal(value.Value);
        }
        //
        internal static PredefinedTypeSyntax FloatType {
            get { return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.FloatKeyword)); }
        }
        internal static NullableTypeSyntax FloatNullableType {
            get { return SyntaxFactory.NullableType(FloatType); }
        }
        internal static LiteralExpressionSyntax Literal(float value) {
            return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(value));
        }
        internal static ExpressionSyntax Literal(float? value) {
            if (value == null) return NullLiteral;
            return Literal(value.Value);
        }
        //
        internal static PredefinedTypeSyntax DoubleType {
            get { return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.DoubleKeyword)); }
        }
        internal static NullableTypeSyntax DoubleNullableType {
            get { return SyntaxFactory.NullableType(DoubleType); }
        }
        internal static CastExpressionSyntax Literal(double value) {
            return SyntaxFactory.CastExpression(DoubleType, SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(value)));
        }
        internal static ExpressionSyntax Literal(double? value) {
            if (value == null) return NullLiteral;
            return Literal(value.Value);
        }
        //
        //global::XXX
        internal static AliasQualifiedNameSyntax GlobalAliasQualifiedName(SimpleNameSyntax name) {
            return SyntaxFactory.AliasQualifiedName(GlobalIdName, name);
        }
        internal static AliasQualifiedNameSyntax GlobalAliasQualifiedName(string name) {
            return GlobalAliasQualifiedName(IdName(name));
        }
        internal static QualifiedNameSyntax QualifiedName(NameSyntax left, string right) {
            return SyntaxFactory.QualifiedName(left, IdName(right));
        }
        internal static GenericNameSyntax GenericName(string identifier, IEnumerable<TypeSyntax> types) {
            return SyntaxFactory.GenericName(Id(identifier), SyntaxFactory.TypeArgumentList(SyntaxFactory.SeparatedList(types)));
        }
        internal static GenericNameSyntax GenericName(string identifier, params TypeSyntax[] types) {
            return GenericName(identifier, (IEnumerable<TypeSyntax>)types);
        }
        internal static GenericNameSyntax GenericName(string identifier, TypeSyntax type) {
            return SyntaxFactory.GenericName(Id(identifier), SyntaxFactory.TypeArgumentList(SyntaxFactory.SingletonSeparatedList(type)));
        }
        //private static TypeArgumentListSyntax TypeArgumentList(IEnumerable<TypeSyntax> types) {
        //    return SyntaxFactory.TypeArgumentList(SyntaxFactory.SeparatedList(types));
        //}
        //internal static TypeArgumentListSyntax TypeArgumentList(params TypeSyntax[] types) {
        //    return TypeArgumentList((IEnumerable<TypeSyntax>)types);
        //}
        //
        //global::System
        internal static AliasQualifiedNameSyntax GlobalSystemName {
            get { return GlobalAliasQualifiedName("System"); }
        }
        //global::System.DateTime
        internal static QualifiedNameSyntax DateTimeName {
            get { return QualifiedName(GlobalSystemName, "DateTime"); }
        }
        internal static NullableTypeSyntax DateTimeNullableType {
            get { return SyntaxFactory.NullableType(DateTimeName); }
        }
        //global::System.DateTimeKind
        internal static QualifiedNameSyntax DateTimeKindName {
            get { return QualifiedName(GlobalSystemName, "DateTimeKind"); }
        }
        internal static ExpressionSyntax Literal(DateTimeKind value) {
            return SyntaxFactory.CastExpression(DateTimeKindName, Literal((int)value));
        }
        internal static ExpressionSyntax Literal(DateTime value) {
            return NewObjExpr(DateTimeName, Literal(value.Ticks), Literal(value.Kind));
        }
        internal static ExpressionSyntax Literal(DateTime? value) {
            if (value == null) return NullLiteral;
            return Literal(value.Value);
        }
        //global::System.DateTimeOffset
        internal static QualifiedNameSyntax DateTimeOffsetName {
            get { return QualifiedName(GlobalSystemName, "DateTimeOffset"); }
        }
        internal static NullableTypeSyntax DateTimeOffsetNullableType {
            get { return SyntaxFactory.NullableType(DateTimeOffsetName); }
        }
        internal static ExpressionSyntax Literal(DateTimeOffset value) {
            return NewObjExpr(DateTimeOffsetName, Literal(value.Ticks), Literal(value.Offset));
        }
        internal static ExpressionSyntax Literal(DateTimeOffset? value) {
            if (value == null) return NullLiteral;
            return Literal(value.Value);
        }
        //global::System.TimeSpan
        internal static QualifiedNameSyntax TimeSpanName {
            get { return QualifiedName(GlobalSystemName, "TimeSpan"); }
        }
        internal static NullableTypeSyntax TimeSpanNullableType {
            get { return SyntaxFactory.NullableType(TimeSpanName); }
        }
        internal static ExpressionSyntax Literal(TimeSpan value) {
            return NewObjExpr(TimeSpanName, Literal(value.Ticks));
        }
        internal static ExpressionSyntax Literal(TimeSpan? value) {
            if (value == null) return NullLiteral;
            return Literal(value.Value);
        }
        //global::System.Guid
        internal static QualifiedNameSyntax GuidName {
            get { return QualifiedName(GlobalSystemName, "Guid"); }
        }
        internal static NullableTypeSyntax GuidNullableType {
            get { return SyntaxFactory.NullableType(GuidName); }
        }
        internal static ExpressionSyntax Literal(Guid value) {
            return NewObjExpr(GuidName, Literal(value.ToByteArray()));
        }
        internal static ExpressionSyntax Literal(Guid? value) {
            if (value == null) return NullLiteral;
            return Literal(value.Value);
        }
        //global::System.Uri
        internal static QualifiedNameSyntax UriName {
            get { return QualifiedName(GlobalSystemName, "Uri"); }
        }
        internal static ExpressionSyntax Literal(Uri value) {
            if (value == null) return NullLiteral;
            return NewObjExpr(UriName, Literal(value.ToString()));
        }
        //global::System.Type
        internal static QualifiedNameSyntax SystemTypeName {
            get { return QualifiedName(GlobalSystemName, "Type"); }
        }
        //global::System.Type[]
        internal static ArrayTypeSyntax SystemTypeArrayType {
            get { return OneDimArrayType(SystemTypeName); }
        }

        //global::System.IDisposable
        internal static QualifiedNameSyntax IDisposableName {
            get { return QualifiedName(GlobalSystemName, "IDisposable"); }
        }
        //global::System.Nullable<T>
        internal static QualifiedNameSyntax NullableOf(TypeSyntax type) {
            return SyntaxFactory.QualifiedName(GlobalSystemName, GenericName("Nullable", type));
        }
        //global::System.IEquatable<T>
        internal static QualifiedNameSyntax IEquatableOf(TypeSyntax type) {
            return SyntaxFactory.QualifiedName(GlobalSystemName, GenericName("IEquatable", type));
        }
        //global::System.Action
        internal static QualifiedNameSyntax ActionName {
            get { return QualifiedName(GlobalSystemName, "Action"); }
        }
        //global::System.Action<...>
        internal static QualifiedNameSyntax ActionOf(IEnumerable<TypeSyntax> types) {
            return SyntaxFactory.QualifiedName(GlobalSystemName, GenericName("Action", types));
        }
        internal static QualifiedNameSyntax ActionOf(params TypeSyntax[] types) {
            return ActionOf((IEnumerable<TypeSyntax>)types);
        }
        internal static QualifiedNameSyntax ActionOf(TypeSyntax type) {
            return SyntaxFactory.QualifiedName(GlobalSystemName, GenericName("Action", type));
        }
        //global::System.Func<...>
        internal static QualifiedNameSyntax FuncOf(IEnumerable<TypeSyntax> types) {
            return SyntaxFactory.QualifiedName(GlobalSystemName, GenericName("Func", types));
        }
        internal static QualifiedNameSyntax FuncOf(params TypeSyntax[] types) {
            return FuncOf((IEnumerable<TypeSyntax>)types);
        }
        internal static QualifiedNameSyntax FuncOf(TypeSyntax type) {
            return SyntaxFactory.QualifiedName(GlobalSystemName, GenericName("Func", type));
        }

        //global::System.Exception
        internal static QualifiedNameSyntax ExceptionName {
            get { return QualifiedName(GlobalSystemName, "Exception"); }
        }
        //global::System.SerializableAttribute
        internal static QualifiedNameSyntax SerializableAttributeName {
            get { return QualifiedName(GlobalSystemName, "SerializableAttribute"); }
        }
        internal static AttributeListSyntax SerializableAttributeList {
            get { return AttributeList(SerializableAttributeName); }
        }
        //
        //global::System.Attribute
        internal static QualifiedNameSyntax AttributeName {
            get { return QualifiedName(GlobalSystemName, "Attribute"); }
        }
        //global::System.AttributeUsageAttribute
        internal static QualifiedNameSyntax AttributeUsageAttributeName {
            get { return QualifiedName(GlobalSystemName, "AttributeUsageAttribute"); }
        }
        //global::System.AttributeTargets
        internal static QualifiedNameSyntax AttributeTargetsName {
            get { return QualifiedName(GlobalSystemName, "AttributeTargets"); }
        }
        internal static ExpressionSyntax Literal(AttributeTargets value) {
            return SyntaxFactory.CastExpression(AttributeTargetsName, Literal((int)value));
        }
        //global::System.Reflection
        internal static QualifiedNameSyntax GlobalSystemReflectionName {
            get { return QualifiedName(GlobalSystemName, "Reflection"); }
        }
        //global::System.Reflection.PropertyInfo
        internal static QualifiedNameSyntax PropertyInfoName {
            get { return QualifiedName(GlobalSystemReflectionName, "PropertyInfo"); }
        }
        //global::System.Reflection.BindingFlags
        internal static QualifiedNameSyntax BindingFlagsName {
            get { return QualifiedName(GlobalSystemReflectionName, "BindingFlags"); }
        }
        internal static ExpressionSyntax Literal(System.Reflection.BindingFlags value) {
            return SyntaxFactory.CastExpression(BindingFlagsName, Literal((int)value));
        }

        //global::System.Collections
        internal static QualifiedNameSyntax GlobalSystemCollectionName {
            get { return QualifiedName(GlobalSystemName, "Collections"); }
        }
        //global::System.Collections.IEnumerable
        internal static QualifiedNameSyntax IEnumerableName {
            get { return QualifiedName(GlobalSystemCollectionName, "IEnumerable"); }
        }
        //global::System.Collections.IEnumerator
        internal static QualifiedNameSyntax IEnumeratorName {
            get { return QualifiedName(GlobalSystemCollectionName, "IEnumerator"); }
        }
        //
        //global::System.Collections.Generic
        internal static QualifiedNameSyntax GlobalSystemCollectionGenericName {
            get { return QualifiedName(GlobalSystemCollectionName, "Generic"); }
        }
        //global::System.Collection.Generic.IEnumerable<T>
        internal static QualifiedNameSyntax IEnumerableOf(TypeSyntax type) {
            return SyntaxFactory.QualifiedName(GlobalSystemCollectionGenericName, GenericName("IEnumerable", type));
        }
        //global::System.Collection.Generic.IEnumerator<T>
        internal static QualifiedNameSyntax IEnumeratorOf(TypeSyntax type) {
            return SyntaxFactory.QualifiedName(GlobalSystemCollectionGenericName, GenericName("IEnumerator", type));
        }
        //global::System.Collection.Generic.ICollection<T>
        internal static QualifiedNameSyntax ICollectionOf(TypeSyntax type) {
            return SyntaxFactory.QualifiedName(GlobalSystemCollectionGenericName, GenericName("ICollection", type));
        }
        //global::System.Collection.Generic.IList<T>
        internal static QualifiedNameSyntax IListOf(TypeSyntax type) {
            return SyntaxFactory.QualifiedName(GlobalSystemCollectionGenericName, GenericName("IList", type));
        }
        //global::System.Collection.Generic.List<T>
        internal static QualifiedNameSyntax ListOf(TypeSyntax type) {
            return SyntaxFactory.QualifiedName(GlobalSystemCollectionGenericName, GenericName("List", type));
        }
        //global::System.Collection.Generic.ISet<T>
        internal static QualifiedNameSyntax ISetOf(TypeSyntax type) {
            return SyntaxFactory.QualifiedName(GlobalSystemCollectionGenericName, GenericName("ISet", type));
        }
        //global::System.Collection.Generic.HashSet<T>
        internal static QualifiedNameSyntax HashSetOf(TypeSyntax type) {
            return SyntaxFactory.QualifiedName(GlobalSystemCollectionGenericName, GenericName("HashSet", type));
        }
        //global::System.Collection.Generic.IReadOnlyList<T>
        internal static QualifiedNameSyntax IReadOnlyListOf(TypeSyntax type) {
            return SyntaxFactory.QualifiedName(GlobalSystemCollectionGenericName, GenericName("IReadOnlyList", type));
        }
        //global::System.Collection.Generic.IDictionary<TKey,TValue>
        internal static QualifiedNameSyntax IDictionaryOf(TypeSyntax keyType, TypeSyntax valueType) {
            return SyntaxFactory.QualifiedName(GlobalSystemCollectionGenericName, GenericName("IDictionary", keyType, valueType));
        }
        //global::System.Collection.Generic.IReadOnlyDictionary<TKey,TValue>
        internal static QualifiedNameSyntax IReadOnlyDictionaryOf(TypeSyntax keyType, TypeSyntax valueType) {
            return SyntaxFactory.QualifiedName(GlobalSystemCollectionGenericName, GenericName("IReadOnlyDictionary", keyType, valueType));
        }
        //global::System.Collection.Generic.Dictionary<TKey,TValue>
        internal static QualifiedNameSyntax DictionaryOf(TypeSyntax keyType, TypeSyntax valueType) {
            return SyntaxFactory.QualifiedName(GlobalSystemCollectionGenericName, GenericName("Dictionary", keyType, valueType));
        }
        //global::System.Collection.Generic.KeyValuePair<TKey,TValue>
        internal static QualifiedNameSyntax KeyValuePairOf(TypeSyntax keyType, TypeSyntax valueType) {
            return SyntaxFactory.QualifiedName(GlobalSystemCollectionGenericName, GenericName("KeyValuePair", keyType, valueType));
        }

        //global::System.Collections.ObjectModel
        internal static QualifiedNameSyntax GlobalSystemCollectionObjectModelName {
            get { return QualifiedName(GlobalSystemCollectionName, "ObjectModel"); }
        }
        //global::System.Collection.ObjectModel.Collection<T>
        internal static QualifiedNameSyntax CollectionOf(TypeSyntax type) {
            return SyntaxFactory.QualifiedName(GlobalSystemCollectionObjectModelName, GenericName("Collection", type));
        }
        //
        //global::System.IO
        internal static QualifiedNameSyntax GlobalSystemIOName {
            get { return QualifiedName(GlobalSystemName, "IO"); }
        }
        internal static QualifiedNameSyntax TextReaderName {
            get { return QualifiedName(GlobalSystemIOName, "TextReader"); }
        }
        internal static QualifiedNameSyntax TextWriterName {
            get { return QualifiedName(GlobalSystemIOName, "TextWriter"); }
        }

        //
        //global::System.Text
        internal static QualifiedNameSyntax GlobalSystemTextName {
            get { return QualifiedName(GlobalSystemName, "Text"); }
        }
        //global::System.Text.StringBuilder
        internal static QualifiedNameSyntax StringBuilderName {
            get { return QualifiedName(GlobalSystemTextName, "StringBuilder"); }
        }

        //global::System.Text.RegularExpressions
        internal static QualifiedNameSyntax GlobalSystemTextRegularExpressionsName {
            get { return QualifiedName(GlobalSystemTextName, "RegularExpressions"); }
        }
        //global::System.Text.RegularExpressions.Regex
        internal static QualifiedNameSyntax RegexName {
            get { return QualifiedName(GlobalSystemTextRegularExpressionsName, "Regex"); }
        }
        //global::System.Text.RegularExpressions.RegexOptions
        internal static QualifiedNameSyntax RegexOptionsName {
            get { return QualifiedName(GlobalSystemTextRegularExpressionsName, "RegexOptions"); }
        }
        internal static ExpressionSyntax Literal(System.Text.RegularExpressions.RegexOptions value) {
            return SyntaxFactory.CastExpression(RegexOptionsName, Literal((int)value));
        }
        //
        //global::System.Xml
        internal static QualifiedNameSyntax GlobalSystemXmlName {
            get { return QualifiedName(GlobalSystemName, "Xml"); }
        }
        internal static QualifiedNameSyntax XmlReaderName {
            get { return QualifiedName(GlobalSystemXmlName, "XmlReader"); }
        }
        //global::System.Xml.Linq
        internal static QualifiedNameSyntax GlobalSystemXmlLinqName {
            get { return QualifiedName(GlobalSystemXmlName, "Linq"); }
        }
        //global::System.Xml.Linq.XName
        internal static QualifiedNameSyntax XNameName {
            get { return QualifiedName(GlobalSystemXmlLinqName, "XName"); }
        }
        //internal static ExpressionSyntax Literal(System.Xml.Linq.XName value) {
        //    if (value == null) return SyntaxFactory.CastExpression(XNameName, NullLiteral);
        //    return InvoExpr(MemberAccessExpr(XNameName, "Get"), SyntaxFactory.Argument(Literal(value.LocalName)), SyntaxFactory.Argument(Literal(value.NamespaceName)));
        //}
        //global::System.Xml.Linq.XNamespace
        internal static QualifiedNameSyntax XNamespaceName {
            get { return QualifiedName(GlobalSystemXmlLinqName, "XNamespace"); }
        }
        //internal static ExpressionSyntax Literal(System.Xml.Linq.XNamespace value) {
        //    if (value == null) return SyntaxFactory.CastExpression(XNamespaceName, NullLiteral);
        //    return InvoExpr(MemberAccessExpr(XNamespaceName, "Get"), SyntaxFactory.Argument(Literal(value.NamespaceName)));
        //}
        //
        //global::System.Data
        internal static QualifiedNameSyntax GlobalSystemDataName {
            get { return QualifiedName(GlobalSystemName, "Data"); }
        }
        //global::System.Data.Spatial
        internal static QualifiedNameSyntax GlobalSystemDataSpatialName {
            get { return QualifiedName(GlobalSystemDataName, "Spatial"); }
        }
        //global::System.Data.Spatial.DbGeography
        internal static QualifiedNameSyntax DbGeographyName {
            get { return QualifiedName(GlobalSystemDataSpatialName, "DbGeography"); }
        }
        //global::System.Data.Spatial.DbGeometry
        internal static QualifiedNameSyntax DbGeometryName {
            get { return QualifiedName(GlobalSystemDataSpatialName, "DbGeometry"); }
        }
        //
        //global::System.Transactions
        internal static QualifiedNameSyntax GlobalSystemTransactionsName {
            get { return QualifiedName(GlobalSystemName, "Transactions"); }
        }
        //global::System.Transactions.IsolationLevel
        internal static QualifiedNameSyntax IsolationLevelName {
            get { return QualifiedName(GlobalSystemTransactionsName, "IsolationLevel"); }
        }
        //
        //global::System.Runtime
        internal static QualifiedNameSyntax GlobalSystemRuntimeName {
            get { return QualifiedName(GlobalSystemName, "Runtime"); }
        }
        //global::System.Runtime.Serialization
        internal static QualifiedNameSyntax GlobalSystemRuntimeSerializationName {
            get { return QualifiedName(GlobalSystemRuntimeName, "Serialization"); }
        }
        //global::System.Runtime.Serialization.ISerializable
        internal static QualifiedNameSyntax ISerializableName {
            get { return QualifiedName(GlobalSystemRuntimeSerializationName, "ISerializable"); }
        }
        //global::System.Runtime.Serialization.SerializationInfo
        internal static QualifiedNameSyntax SerializationInfoName {
            get { return QualifiedName(GlobalSystemRuntimeSerializationName, "SerializationInfo"); }
        }
        //global::System.Runtime.Serialization.StreamingContext
        internal static QualifiedNameSyntax StreamingContextName {
            get { return QualifiedName(GlobalSystemRuntimeSerializationName, "StreamingContext"); }
        }
        //global::System.Runtime.Serialization.DataContractAttribute
        internal static QualifiedNameSyntax DataContractAttributeName {
            get { return QualifiedName(GlobalSystemRuntimeSerializationName, "DataContractAttribute"); }
        }
        //global::System.Runtime.Serialization.DataMemberAttribute
        internal static QualifiedNameSyntax DataMemberAttributeName {
            get { return QualifiedName(GlobalSystemRuntimeSerializationName, "DataMemberAttribute"); }
        }
        //global::System.Runtime.Serialization.KnownTypeAttribute
        internal static QualifiedNameSyntax KnownTypeAttributeName {
            get { return QualifiedName(GlobalSystemRuntimeSerializationName, "KnownTypeAttribute"); }
        }
        //global::System.Runtime.Serialization.OnSerializingAttribute
        internal static QualifiedNameSyntax OnSerializingAttributeName {
            get { return QualifiedName(GlobalSystemRuntimeSerializationName, "OnSerializingAttribute"); }
        }
        //global::System.Runtime.Serialization.OnSerializedAttribute
        internal static QualifiedNameSyntax OnSerializedAttributeName {
            get { return QualifiedName(GlobalSystemRuntimeSerializationName, "OnSerializedAttribute"); }
        }
        //global::System.Runtime.Serialization.OnDeserializedAttribute
        internal static QualifiedNameSyntax OnDeserializedAttributeName {
            get { return QualifiedName(GlobalSystemRuntimeSerializationName, "OnDeserializedAttribute"); }
        }
        //
        //
        //
        //global::System.NotImplementedException
        internal static QualifiedNameSyntax NotImplementedExceptionName {
            get { return QualifiedName(GlobalSystemName, "NotImplementedException"); }
        }
        //throw new global::System.NotImplementedException();
        internal static ThrowStatementSyntax ThrowNotImplemented {
            get { return SyntaxFactory.ThrowStatement(NewObjExpr(NotImplementedExceptionName)); }
        }
        //{throw new global::System.NotImplementedException();}
        internal static BlockSyntax ThrowNotImplementedBlock {
            get { return SyntaxFactory.Block(SyntaxFactory.SingletonList<StatementSyntax>(ThrowNotImplemented)); }
        }
        //global::System.NotSupportedException
        internal static QualifiedNameSyntax NotSupportedExceptionName {
            get { return QualifiedName(GlobalSystemName, "NotSupportedException"); }
        }
        //throw new global::System.NotSupportedException();
        internal static ThrowStatementSyntax ThrowNotSupported {
            get { return SyntaxFactory.ThrowStatement(NewObjExpr(NotSupportedExceptionName)); }
        }
        //{throw new global::System.NotSupportedException();}
        internal static BlockSyntax ThrowNotSupportedBlock {
            get { return SyntaxFactory.Block(SyntaxFactory.SingletonList<StatementSyntax>(ThrowNotSupported)); }
        }
        //global::System.ArgumentNullException
        internal static QualifiedNameSyntax ArgumentNullExceptionName {
            get { return QualifiedName(GlobalSystemName, "ArgumentNullException"); }
        }
        //throw new global::System.ArgumentNullException("paramName");
        internal static ThrowStatementSyntax ThrowArgumentNull(string paramName) {
            return SyntaxFactory.ThrowStatement(NewObjExpr(ArgumentNullExceptionName, Literal(paramName)));
        }
        //if(condition) throw new global::System.ArgumentNullException("paramName");
        internal static IfStatementSyntax IfThrowArgumentNull(ExpressionSyntax condition, string paramName) {
            return SyntaxFactory.IfStatement(condition, ThrowArgumentNull(paramName));
        }
        //if(paramName == null) throw new global::System.ArgumentNullException("paramName");
        internal static IfStatementSyntax IfNullThrowArgumentNull(string paramName) {
            return IfThrowArgumentNull(EqualsExpr(IdName(paramName), NullLiteral), paramName);
        }
        //global::System.InvalidOperationException
        internal static QualifiedNameSyntax InvalidOperationExceptionName {
            get { return QualifiedName(GlobalSystemName, "InvalidOperationException"); }
        }
        //throw new global::System.InvalidOperationException("message");
        internal static ThrowStatementSyntax ThrowInvalidOperation(string message) {
            return SyntaxFactory.ThrowStatement(NewObjExpr(InvalidOperationExceptionName, Literal(message)));
        }
        //if(condition) throw new global::System.InvalidOperationException("message");
        internal static IfStatementSyntax IfThrowInvalidOperation(ExpressionSyntax condition, string message) {
            return SyntaxFactory.IfStatement(condition, ThrowInvalidOperation(message));
        }
        //
        internal static VariableDeclarationSyntax VarDecl(TypeSyntax type, SyntaxToken identifier, ExpressionSyntax initializer = null) {
            return SyntaxFactory.VariableDeclaration(
                type: type,
                variables: SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.VariableDeclarator(
                        identifier: identifier,
                        argumentList: null,
                        initializer: initializer == null ? null : SyntaxFactory.EqualsValueClause(initializer))));
        }
        internal static VariableDeclarationSyntax VarDecl(TypeSyntax type, string identifier, ExpressionSyntax initializer = null) {
            return VarDecl(type, Id(identifier), initializer);
        }
        internal static LocalDeclarationStatementSyntax LocalDeclStm(TypeSyntax type, SyntaxToken identifier, ExpressionSyntax initializer = null) {
            return SyntaxFactory.LocalDeclarationStatement(VarDecl(type, identifier, initializer));
        }
        internal static LocalDeclarationStatementSyntax LocalDeclStm(TypeSyntax type, string identifier, ExpressionSyntax initializer = null) {
            return SyntaxFactory.LocalDeclarationStatement(VarDecl(type, identifier, initializer));
        }
        internal static LocalDeclarationStatementSyntax LocalConstDeclStm(TypeSyntax type, SyntaxToken identifier, ExpressionSyntax initializer) {
            return SyntaxFactory.LocalDeclarationStatement(ConstTokenList, VarDecl(type, identifier, initializer));
        }
        internal static TryStatementSyntax TryFinallyStm(IEnumerable<StatementSyntax> bodyStms, IEnumerable<StatementSyntax> finallyStms) {
            return SyntaxFactory.TryStatement(SyntaxFactory.Block(bodyStms),
                default(SyntaxList<CatchClauseSyntax>),
                SyntaxFactory.FinallyClause(SyntaxFactory.Block(finallyStms)));
        }
        internal static IfStatementSyntax IfStm(ExpressionSyntax condition, StatementSyntax then) {
            return SyntaxFactory.IfStatement(condition, then);
        }
        internal static IfStatementSyntax IfStm(ExpressionSyntax condition, StatementSyntax then, StatementSyntax @else) {
            return SyntaxFactory.IfStatement(condition, then, SyntaxFactory.ElseClause(@else));
        }
        internal static ReturnStatementSyntax ReturnStm(ExpressionSyntax expr) {
            return SyntaxFactory.ReturnStatement(expr);
        }
        internal static ExpressionStatementSyntax ExprStm(ExpressionSyntax expr) {
            return SyntaxFactory.ExpressionStatement(expr);
        }
        //>(..) => body
        internal static ParenthesizedLambdaExpressionSyntax ParedLambdaExpr(IEnumerable<ParameterSyntax> parameters, CSharpSyntaxNode body) {
            return SyntaxFactory.ParenthesizedLambdaExpression(ParameterList(parameters), body);
        }
        //>para => body
        internal static SimpleLambdaExpressionSyntax SimpleLambdaExpr(string para, CSharpSyntaxNode body) {
            return SyntaxFactory.SimpleLambdaExpression(Parameter(para), body);
        }
        //>para => { ... }
        internal static SimpleLambdaExpressionSyntax SimpleLambdaExpr(string para, IEnumerable<StatementSyntax> stms) {
            return SimpleLambdaExpr(para, SyntaxFactory.Block(stms));
        }
        //>((lambdaType)(() => block))();
        internal static InvocationExpressionSyntax InvoLambdaExpr(TypeSyntax lambdaType, BlockSyntax block) {
            return InvoExpr(ParedExpr(CastExpr(lambdaType, ParedExpr(ParedLambdaExpr(null, block)))));
        }
        //>obj.Method(para => body)
        internal static InvocationExpressionSyntax InvoWithLambdaArgExpr(ExpressionSyntax obj, string method, string para, CSharpSyntaxNode body) {
            return InvoExpr(MemberAccessExpr(obj, method), SimpleLambdaExpr(para, body));
        }
        //>obj.Method(para => { ... })
        internal static InvocationExpressionSyntax InvoWithLambdaArgExpr(ExpressionSyntax obj, string method, string para, IEnumerable<StatementSyntax> stms) {
            return InvoWithLambdaArgExpr(obj, method, para, SyntaxFactory.Block(stms));
        }
        //>new type(para => body)
        internal static ObjectCreationExpressionSyntax NewObjWithLambdaArgExpr(TypeSyntax type, string para, CSharpSyntaxNode body) {
            return NewObjExpr(type, SimpleLambdaExpr(para, body));
        }
        //>new type(para => { ... })
        internal static ObjectCreationExpressionSyntax NewObjWithLambdaArgExpr(TypeSyntax type, string para, IEnumerable<StatementSyntax> stms) {
            return NewObjWithLambdaArgExpr(type, para, SyntaxFactory.Block(stms));
        }
        //>obj.property = value;
        internal static ExpressionStatementSyntax AssignStm(ExpressionSyntax obj, string property, ExpressionSyntax value) {
            return AssignStm(MemberAccessExpr(obj, property), value);
        }
        //>obj.Method(...)
        internal static InvocationExpressionSyntax InvoExpr(ExpressionSyntax obj, string method, params ExpressionSyntax[] argExprs) {
            return InvoExpr(MemberAccessExpr(obj, method), argExprs);
        }
        internal static InvocationExpressionSyntax InvoExpr(string obj, string method, params ExpressionSyntax[] argExprs) {
            return InvoExpr(IdName(obj), method, argExprs);
        }
        //>obj.Add(value);
        internal static ExpressionStatementSyntax AddInvoStm(ExpressionSyntax obj, ExpressionSyntax value) {
            return ExprStm(InvoExpr(obj, "Add", value));
        }
        //>obj.property.Add(value);
        internal static ExpressionStatementSyntax AddInvoStm(ExpressionSyntax obj, string property, ExpressionSyntax value) {
            return AddInvoStm(MemberAccessExpr(obj, property), value);
        }
        //>obj.property.Add(key, value);
        internal static ExpressionStatementSyntax AddInvoStm(ExpressionSyntax obj, string property, ExpressionSyntax key, ExpressionSyntax value) {
            return ExprStm(InvoExpr(MemberAccessExpr(MemberAccessExpr(obj, property), "Add"), key, value));
        }

        //Parentheses
        internal static ParenthesizedExpressionSyntax ParedExpr(ExpressionSyntax expr) {
            return SyntaxFactory.ParenthesizedExpression(expr);
        }
        internal static SyntaxNode GetNonPareParent(this SyntaxNode node) {
            var parent = node.Parent;
            if (parent is ParenthesizedExpressionSyntax) return GetNonPareParent(parent);
            return parent;
        }
        internal static ExpressionSyntax StripPareExpr(this ExpressionSyntax expr) {
            var paredExpr = expr as ParenthesizedExpressionSyntax;
            if (paredExpr != null) return StripPareExpr(paredExpr.Expression);
            return expr;
        }
        internal static T AsNonPareExpr<T>(this ExpressionSyntax expr) where T : ExpressionSyntax {
            var t = expr as T;
            if (t != null) return t;
            var paredExpr = expr as ParenthesizedExpressionSyntax;
            if (paredExpr != null) return AsNonPareExpr<T>(paredExpr.Expression);
            return null;
        }
        internal static CastExpressionSyntax CastExpr(TypeSyntax type, ExpressionSyntax expr) {
            return SyntaxFactory.CastExpression(type, expr);
        }
        internal static AssignmentExpressionSyntax AssignExpr(ExpressionSyntax left, ExpressionSyntax right) {
            return SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, left, right);
        }
        internal static AssignmentExpressionSyntax AssignExpr(string left, ExpressionSyntax right) {
            return AssignExpr(IdName(left), right);
        }
        internal static ExpressionStatementSyntax AssignStm(ExpressionSyntax left, ExpressionSyntax right) {
            return ExprStm(AssignExpr(left, right));
        }
        internal static ExpressionStatementSyntax AssignStm(string left, ExpressionSyntax right) {
            return AssignStm(IdName(left), right);
        }
        //
        internal static BinaryExpressionSyntax AddExpr(ExpressionSyntax left, ExpressionSyntax right) {
            return SyntaxFactory.BinaryExpression(SyntaxKind.AddExpression, left, right);
        }
        internal static BinaryExpressionSyntax SubtractExpr(ExpressionSyntax left, ExpressionSyntax right) {
            return SyntaxFactory.BinaryExpression(SyntaxKind.SubtractExpression, left, right);
        }
        internal static BinaryExpressionSyntax MultiplyExpr(ExpressionSyntax left, ExpressionSyntax right) {
            return SyntaxFactory.BinaryExpression(SyntaxKind.MultiplyExpression, left, right);
        }
        internal static BinaryExpressionSyntax DivideExpr(ExpressionSyntax left, ExpressionSyntax right) {
            return SyntaxFactory.BinaryExpression(SyntaxKind.DivideExpression, left, right);
        }
        internal static BinaryExpressionSyntax ModuloExpr(ExpressionSyntax left, ExpressionSyntax right) {
            return SyntaxFactory.BinaryExpression(SyntaxKind.ModuloExpression, left, right);
        }
        internal static BinaryExpressionSyntax BitwiseAndExpr(ExpressionSyntax left, ExpressionSyntax right) {
            return SyntaxFactory.BinaryExpression(SyntaxKind.BitwiseAndExpression, left, right);
        }
        internal static BinaryExpressionSyntax LogicalAndExpr(ExpressionSyntax left, ExpressionSyntax right) {
            return SyntaxFactory.BinaryExpression(SyntaxKind.LogicalAndExpression, left, right);
        }
        internal static BinaryExpressionSyntax BitwiseOrExpr(ExpressionSyntax left, ExpressionSyntax right) {
            return SyntaxFactory.BinaryExpression(SyntaxKind.BitwiseOrExpression, left, right);
        }
        internal static BinaryExpressionSyntax LogicalOrExpr(ExpressionSyntax left, ExpressionSyntax right) {
            return SyntaxFactory.BinaryExpression(SyntaxKind.LogicalOrExpression, left, right);
        }
        internal static BinaryExpressionSyntax ExclusiveOrExpr(ExpressionSyntax left, ExpressionSyntax right) {
            return SyntaxFactory.BinaryExpression(SyntaxKind.ExclusiveOrExpression, left, right);
        }
        internal static BinaryExpressionSyntax LeftShiftExpr(ExpressionSyntax left, ExpressionSyntax right) {
            return SyntaxFactory.BinaryExpression(SyntaxKind.LeftShiftExpression, left, right);
        }
        internal static BinaryExpressionSyntax RightShiftExpr(ExpressionSyntax left, ExpressionSyntax right) {
            return SyntaxFactory.BinaryExpression(SyntaxKind.RightShiftExpression, left, right);
        }
        internal static PrefixUnaryExpressionSyntax PreIncrementExpr(ExpressionSyntax operand) {
            return SyntaxFactory.PrefixUnaryExpression(SyntaxKind.PreIncrementExpression, operand);
        }
        internal static PrefixUnaryExpressionSyntax LogicalNotExpr(ExpressionSyntax operand) {
            return SyntaxFactory.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, operand);
        }
        internal static PrefixUnaryExpressionSyntax PreDecrementExpr(ExpressionSyntax operand) {
            return SyntaxFactory.PrefixUnaryExpression(SyntaxKind.PreDecrementExpression, operand);
        }
        internal static BinaryExpressionSyntax AsExpr(ExpressionSyntax left, TypeSyntax right) {
            return SyntaxFactory.BinaryExpression(SyntaxKind.AsExpression, left, right);
        }
        internal static BinaryExpressionSyntax EqualsExpr(ExpressionSyntax left, ExpressionSyntax right) {
            return SyntaxFactory.BinaryExpression(SyntaxKind.EqualsExpression, left, right);
        }
        internal static BinaryExpressionSyntax NotEqualsExpr(ExpressionSyntax left, ExpressionSyntax right) {
            return SyntaxFactory.BinaryExpression(SyntaxKind.NotEqualsExpression, left, right);
        }
        internal static BinaryExpressionSyntax LessThanExpr(ExpressionSyntax left, ExpressionSyntax right) {
            return SyntaxFactory.BinaryExpression(SyntaxKind.LessThanExpression, left, right);
        }
        internal static BinaryExpressionSyntax GreaterThanExpr(ExpressionSyntax left, ExpressionSyntax right) {
            return SyntaxFactory.BinaryExpression(SyntaxKind.GreaterThanExpression, left, right);
        }
        internal static BinaryExpressionSyntax CoalesceExpr(ExpressionSyntax left, ExpressionSyntax right) {
            return SyntaxFactory.BinaryExpression(SyntaxKind.CoalesceExpression, left, right);
        }
        internal static TypeOfExpressionSyntax TypeOfExpr(TypeSyntax type) {
            return SyntaxFactory.TypeOfExpression(type);
        }
        internal static ThisExpressionSyntax ThisExpr() {
            return SyntaxFactory.ThisExpression();
        }
        internal static BaseExpressionSyntax BaseExpr() {
            return SyntaxFactory.BaseExpression();
        }
        //>.x
        internal static MemberBindingExpressionSyntax MemberBindingExpr(SimpleNameSyntax name) {
            return SyntaxFactory.MemberBindingExpression(name);
        }
        internal static ConditionalAccessExpressionSyntax ConditionalAccessExpr(ExpressionSyntax left, ExpressionSyntax whenNotNull) {
            return SyntaxFactory.ConditionalAccessExpression(left, whenNotNull);
        }
        //>a.b
        internal static MemberAccessExpressionSyntax MemberAccessExpr(ExpressionSyntax expression, SimpleNameSyntax name) {
            return SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, expression, name);
        }
        internal static MemberAccessExpressionSyntax MemberAccessExpr(ExpressionSyntax expression, string name) {
            return MemberAccessExpr(expression, IdName(name));
        }
        internal static ElementAccessExpressionSyntax ElementAccessExpr(ExpressionSyntax expression, IEnumerable<ExpressionSyntax> argExprs) {
            return SyntaxFactory.ElementAccessExpression(expression, BracketedArgumentList(argExprs));
        }
        internal static ElementAccessExpressionSyntax ElementAccessExpr(ExpressionSyntax expression, params ExpressionSyntax[] argExprs) {
            return ElementAccessExpr(expression, (IEnumerable<ExpressionSyntax>)argExprs);
        }
        internal static MemberAccessExpressionSyntax BaseMemberAccessExpr(SimpleNameSyntax name) {
            return MemberAccessExpr(BaseExpr(), name);
        }
        internal static MemberAccessExpressionSyntax BaseMemberAccessExpr(string name) {
            return MemberAccessExpr(BaseExpr(), name);
        }
        internal static ElementAccessExpressionSyntax BaseElementAccessExpr(params ExpressionSyntax[] arguments) {
            return ElementAccessExpr(BaseExpr(), arguments);
        }
        internal static InvocationExpressionSyntax InvoExpr(ExpressionSyntax expression) {
            return SyntaxFactory.InvocationExpression(expression, SyntaxFactory.ArgumentList());
        }
        internal static InvocationExpressionSyntax InvoExpr(ExpressionSyntax expression, IEnumerable<ArgumentSyntax> arguments) {
            return SyntaxFactory.InvocationExpression(expression, ArgumentList(arguments));
        }
        internal static InvocationExpressionSyntax InvoExpr(ExpressionSyntax expression, params ArgumentSyntax[] arguments) {
            return InvoExpr(expression, (IEnumerable<ArgumentSyntax>)arguments);
        }
        internal static InvocationExpressionSyntax InvoExpr(ExpressionSyntax expression, IEnumerable<ExpressionSyntax> argExprs) {
            return SyntaxFactory.InvocationExpression(expression, ArgumentList(argExprs));
        }
        internal static InvocationExpressionSyntax InvoExpr(ExpressionSyntax expression, params ExpressionSyntax[] argExprs) {
            return InvoExpr(expression, (IEnumerable<ExpressionSyntax>)argExprs);
        }

        //
        internal static ObjectCreationExpressionSyntax NewObjExpr(TypeSyntax type, IEnumerable<ExpressionSyntax> argExprs, InitializerExpressionSyntax initializer) {
            return SyntaxFactory.ObjectCreationExpression(type, ArgumentList(argExprs), initializer);
        }
        internal static ObjectCreationExpressionSyntax NewObjExpr(TypeSyntax type, IEnumerable<ExpressionSyntax> argExprs, IEnumerable<ExpressionSyntax> initExprs) {
            return NewObjExpr(type, argExprs, ObjectInitializer(initExprs));
        }
        internal static ObjectCreationExpressionSyntax NewObjExpr(TypeSyntax type, params ExpressionSyntax[] argExprs) {
            return NewObjExpr(type, (IEnumerable<ExpressionSyntax>)argExprs, (InitializerExpressionSyntax)null);
        }
        internal static ObjectCreationExpressionSyntax NewObjExpr(TypeSyntax type) {
            return SyntaxFactory.ObjectCreationExpression(type, SyntaxFactory.ArgumentList(), null);
        }
        internal static ObjectCreationExpressionSyntax NewObjWithCollInitExpr(TypeSyntax type, IEnumerable<ExpressionSyntax> initExprs) {
            return SyntaxFactory.ObjectCreationExpression(type, SyntaxFactory.ArgumentList(), CollectionInitializer(initExprs));
        }
        internal static ExpressionSyntax NewObjWithCollInitOrNullExpr(TypeSyntax type, IEnumerable<ExpressionSyntax> initExprs) {
            var initer = CollectionInitializer(initExprs);
            if (initer == null) return NullLiteral;
            return SyntaxFactory.ObjectCreationExpression(type, SyntaxFactory.ArgumentList(), initer);
        }
        internal static ObjectCreationExpressionSyntax NewObjWithCollInitExpr(TypeSyntax type, IEnumerable<IEnumerable<ExpressionSyntax>> initExprs) {
            return SyntaxFactory.ObjectCreationExpression(type, SyntaxFactory.ArgumentList(), CollectionInitializer(initExprs));
        }
        internal static ExpressionSyntax NewObjWithCollInitOrNullExpr(TypeSyntax type, IEnumerable<IEnumerable<ExpressionSyntax>> initExprs) {
            var initer = CollectionInitializer(initExprs);
            if (initer == null) return NullLiteral;
            return SyntaxFactory.ObjectCreationExpression(type, SyntaxFactory.ArgumentList(), initer);
        }
        private static InitializerExpressionSyntax ObjectInitializer(IEnumerable<ExpressionSyntax> exprs) {
            var exprList = SyntaxFactory.SeparatedList(exprs);
            if (exprList.Count == 0) return null;
            return SyntaxFactory.InitializerExpression(SyntaxKind.ObjectInitializerExpression, exprList);
        }
        private static InitializerExpressionSyntax CollectionInitializer(IEnumerable<ExpressionSyntax> exprs) {
            var exprList = SyntaxFactory.SeparatedList(exprs);
            if (exprList.Count == 0) return null;
            return SyntaxFactory.InitializerExpression(SyntaxKind.CollectionInitializerExpression, exprList);
        }
        private static InitializerExpressionSyntax CollectionInitializer(IEnumerable<IEnumerable<ExpressionSyntax>> exprs) {
            if (exprs == null) return null;
            var exprList = SyntaxFactory.SeparatedList<ExpressionSyntax>(exprs.Select(i =>
                SyntaxFactory.InitializerExpression(SyntaxKind.ComplexElementInitializerExpression, SyntaxFactory.SeparatedList(i))));
            if (exprList.Count == 0) return null;
            return SyntaxFactory.InitializerExpression(SyntaxKind.CollectionInitializerExpression, exprList);
        }
        internal static ArrayCreationExpressionSyntax NewArrExpr(ArrayTypeSyntax type, IEnumerable<ExpressionSyntax> initExprs) {
            return SyntaxFactory.ArrayCreationExpression(type, ArrayInitializer(SyntaxFactory.SeparatedList(initExprs)));
        }
        internal static ArrayCreationExpressionSyntax NewArrExpr(ArrayTypeSyntax type, params ExpressionSyntax[] initExprs) {
            return NewArrExpr(type, (IEnumerable<ExpressionSyntax>)initExprs);
        }
        internal static ExpressionSyntax NewArrOrNullExpr(ArrayTypeSyntax type, IEnumerable<ExpressionSyntax> initExprs) {
            var exprList = SyntaxFactory.SeparatedList(initExprs);
            if (exprList.Count == 0) return NullLiteral;
            return SyntaxFactory.ArrayCreationExpression(type, ArrayInitializer(exprList));
        }
        internal static ExpressionSyntax NewArrOrNullExpr(ArrayTypeSyntax type, params ExpressionSyntax[] initExprs) {
            return NewArrOrNullExpr(type, (IEnumerable<ExpressionSyntax>)initExprs);
        }
        private static InitializerExpressionSyntax ArrayInitializer(SeparatedSyntaxList<ExpressionSyntax> exprList) {
            return SyntaxFactory.InitializerExpression(SyntaxKind.ArrayInitializerExpression, exprList);
        }
        //
        //
        internal static ParameterSyntax Parameter(SyntaxTokenList modifiers, TypeSyntax type, SyntaxToken identifier, ExpressionSyntax @default = null) {
            return SyntaxFactory.Parameter(
                attributeLists: default(SyntaxList<AttributeListSyntax>),
                modifiers: modifiers,
                type: type,
                identifier: identifier,
                @default: @default == null ? null : SyntaxFactory.EqualsValueClause(@default));
        }
        internal static ParameterSyntax Parameter(SyntaxTokenList modifiers, TypeSyntax type, string identifier, ExpressionSyntax @default = null) {
            return Parameter(modifiers, type, Id(identifier), @default);
        }
        internal static ParameterSyntax Parameter(TypeSyntax type, string identifier, ExpressionSyntax @default = null) {
            return Parameter(default(SyntaxTokenList), type, identifier, @default);
        }
        internal static ParameterSyntax Parameter(string identifier) {
            return Parameter(null, identifier);
        }
        internal static ParameterSyntax OutParameter(TypeSyntax type, string identifier) {
            return Parameter(OutTokenList, type, identifier);
        }
        internal static ParameterSyntax RefParameter(TypeSyntax type, string identifier) {
            return Parameter(RefTokenList, type, identifier);
        }
        internal static ParameterSyntax ThisParameter(TypeSyntax type, string identifier) {
            return Parameter(ThisTokenList, type, identifier);
        }
        internal static ParameterListSyntax ParameterList(IEnumerable<ParameterSyntax> parameters) {
            return SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(parameters));
        }
        internal static ParameterListSyntax ParameterList(params ParameterSyntax[] parameters) {
            return ParameterList((IEnumerable<ParameterSyntax>)parameters);
        }
        internal static BracketedParameterListSyntax BracketedParameterList(IEnumerable<ParameterSyntax> parameters) {
            return SyntaxFactory.BracketedParameterList(SyntaxFactory.SeparatedList(parameters));
        }
        internal static BracketedParameterListSyntax BracketedParameterList(params ParameterSyntax[] parameters) {
            return BracketedParameterList((IEnumerable<ParameterSyntax>)parameters);
        }
        internal static ArgumentSyntax Argument(ExpressionSyntax expr) {
            return SyntaxFactory.Argument(expr);
        }
        internal static ArgumentSyntax OutArgument(string identifier) {
            return SyntaxFactory.Argument(null, OutToken, IdName(identifier));
        }
        internal static ArgumentSyntax RefArgument(string identifier) {
            return SyntaxFactory.Argument(null, RefToken, IdName(identifier));
        }
        internal static ArgumentListSyntax ArgumentList(IEnumerable<ArgumentSyntax> arguments) {
            return SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(arguments));
        }
        internal static ArgumentListSyntax ArgumentList(params ArgumentSyntax[] arguments) {
            return ArgumentList((IEnumerable<ArgumentSyntax>)arguments);
        }
        internal static ArgumentListSyntax ArgumentList(IEnumerable<ExpressionSyntax> argExprs) {
            return ArgumentList(argExprs == null ? null : argExprs.Select(i => SyntaxFactory.Argument(i)));
        }
        internal static ArgumentListSyntax ArgumentList(params ExpressionSyntax[] argExprs) {
            return ArgumentList((IEnumerable<ExpressionSyntax>)argExprs);
        }
        internal static BracketedArgumentListSyntax BracketedArgumentList(IEnumerable<ExpressionSyntax> argExprs) {
            return SyntaxFactory.BracketedArgumentList(SyntaxFactory.SeparatedList(argExprs == null ? null : argExprs.Select(i => SyntaxFactory.Argument(i))));
        }
        internal static BracketedArgumentListSyntax BracketedArgumentList(params ExpressionSyntax[] argExprs) {
            return BracketedArgumentList((IEnumerable<ExpressionSyntax>)argExprs);
        }
        internal static TypeParameterListSyntax TypeParameterList(IEnumerable<TypeParameterSyntax> parameters) {
            var parameterList = SyntaxFactory.SeparatedList(parameters);
            if (parameterList.Count == 0) return null;
            return SyntaxFactory.TypeParameterList(parameterList);
        }
        internal static TypeParameterConstraintClauseSyntax TypeParameterConstraintClause(IdentifierNameSyntax name, IEnumerable<TypeParameterConstraintSyntax> constraints) {
            return SyntaxFactory.TypeParameterConstraintClause(name, SyntaxFactory.SeparatedList(constraints));
        }
        internal static TypeParameterConstraintClauseSyntax TypeParameterConstraintClause(IdentifierNameSyntax name, IEnumerable<TypeSyntax> types) {
            return TypeParameterConstraintClause(name, types.Select(type => SyntaxFactory.TypeConstraint(type)));
        }
        internal static TypeParameterConstraintClauseSyntax TypeParameterConstraintClause(IdentifierNameSyntax name, TypeParameterConstraintSyntax constraint) {
            return SyntaxFactory.TypeParameterConstraintClause(name, SyntaxFactory.SingletonSeparatedList(constraint));
        }
        internal static TypeParameterConstraintClauseSyntax TypeParameterConstraintClause(IdentifierNameSyntax name, TypeSyntax type) {
            return TypeParameterConstraintClause(name, SyntaxFactory.TypeConstraint(type));
        }
        internal static BaseListSyntax BaseList(TypeSyntax type) {
            return SyntaxFactory.BaseList(SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(SyntaxFactory.SimpleBaseType(type)));
        }
        internal static BaseListSyntax BaseList(IEnumerable<TypeSyntax> types) {
            if (types == null) return null;
            var baseTypeList = SyntaxFactory.SeparatedList<BaseTypeSyntax>(types.Select(type => SyntaxFactory.SimpleBaseType(type)));
            if (baseTypeList.Count == 0) return null;
            return SyntaxFactory.BaseList(baseTypeList);
        }
        internal static BaseListSyntax BaseList(params TypeSyntax[] types) {
            return BaseList((IEnumerable<TypeSyntax>)types);
        }
        internal static AttributeListSyntax AttributeList(NameSyntax name, params AttributeArgumentSyntax[] arguments) {
            return SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(
                SyntaxFactory.Attribute(name, SyntaxFactory.AttributeArgumentList(SyntaxFactory.SeparatedList(arguments)))));
        }
        internal static AttributeListSyntax AttributeList(string target, NameSyntax name, params AttributeArgumentSyntax[] arguments) {
            return SyntaxFactory.AttributeList(SyntaxFactory.AttributeTargetSpecifier(Id(target)),
                SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.Attribute(name, SyntaxFactory.AttributeArgumentList(SyntaxFactory.SeparatedList(arguments)))));
        }
        internal static AttributeArgumentSyntax AttributeArgument(IdentifierNameSyntax name, ExpressionSyntax expr) {
            return SyntaxFactory.AttributeArgument(
                nameEquals: SyntaxFactory.NameEquals(name),
                nameColon: null,
                expression: expr);
        }
        internal static AttributeArgumentSyntax AttributeArgument(string name, ExpressionSyntax value) {
            return AttributeArgument(IdName(name), value);
        }
        private static AttributeListSyntax AttributeUsageAttributeList(AttributeTargets validOn, bool allowMultiple = false, bool inherited = true) {
            return AttributeList(AttributeUsageAttributeName,
                SyntaxFactory.AttributeArgument(Literal(validOn)),
                AttributeArgument("AllowMultiple", Literal(allowMultiple)),
                AttributeArgument("Inherited", Literal(inherited)));
        }
        internal static ClassDeclarationSyntax AttributeDecl(SyntaxTokenList modifiers, SyntaxToken identifier,
            IEnumerable<MemberDeclarationSyntax> members, AttributeTargets validOn, bool allowMultiple = false, bool inherited = true) {
            return Class(new[] { AttributeUsageAttributeList(validOn, allowMultiple, inherited) }, modifiers, identifier, new[] { AttributeName }, members);
        }

        //
        //
        internal static ClassDeclarationSyntax Class(IEnumerable<AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, SyntaxToken identifier,
            IEnumerable<TypeParameterSyntax> typeParameters, IEnumerable<TypeSyntax> baseTypes, IEnumerable<TypeParameterConstraintClauseSyntax> constraintClauses,
            IEnumerable<MemberDeclarationSyntax> members) {
            return SyntaxFactory.ClassDeclaration(
                attributeLists: SyntaxFactory.List(attributeLists),
                modifiers: modifiers,
                identifier: identifier,
                typeParameterList: TypeParameterList(typeParameters),
                //parameterList: null,
                baseList: BaseList(baseTypes),
                constraintClauses: SyntaxFactory.List(constraintClauses),
                members: SyntaxFactory.List(members));
        }
        internal static ClassDeclarationSyntax Class(IEnumerable<AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, SyntaxToken identifier,
            IEnumerable<TypeSyntax> baseTypes, IEnumerable<MemberDeclarationSyntax> members) {
            return Class(attributeLists, modifiers, identifier, null, baseTypes, null, members);
        }
        internal static ClassDeclarationSyntax Class(IEnumerable<AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, string identifier,
            IEnumerable<TypeSyntax> baseTypes, IEnumerable<MemberDeclarationSyntax> members) {
            return Class(attributeLists, modifiers, Id(identifier), baseTypes, members);
        }
        internal static ClassDeclarationSyntax Class(IEnumerable<AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, string identifier,
            IEnumerable<TypeSyntax> baseTypes, params MemberDeclarationSyntax[] members) {
            return Class(attributeLists, modifiers, identifier, baseTypes, (IEnumerable<MemberDeclarationSyntax>)members);
        }
        internal static PropertyDeclarationSyntax Property(IEnumerable<AttributeListSyntax> attributeLists, SyntaxTokenList modifiers,
            TypeSyntax type, SyntaxToken identifier, AccessorListSyntax accessorList) {
            return SyntaxFactory.PropertyDeclaration(
                attributeLists: SyntaxFactory.List(attributeLists),
                modifiers: modifiers,
                type: type,
                explicitInterfaceSpecifier: null,
                identifier: identifier,
                accessorList: accessorList,
                expressionBody: null,
                initializer: null);
        }
        internal static PropertyDeclarationSyntax Property(IEnumerable<AttributeListSyntax> attributeLists, SyntaxTokenList modifiers,
            TypeSyntax type, SyntaxToken identifier, bool getterOnly,
            SyntaxTokenList getterModifiers, IEnumerable<StatementSyntax> getterStatements,
            SyntaxTokenList setterModifiers = default(SyntaxTokenList), IEnumerable<StatementSyntax> setterStatements = null) {
            var getter = AccessorDecl(SyntaxKind.GetAccessorDeclaration, getterModifiers, getterStatements);
            AccessorDeclarationSyntax setter = null;
            if (!getterOnly) {
                setter = AccessorDecl(SyntaxKind.SetAccessorDeclaration, setterModifiers, setterStatements);
            }
            return Property(attributeLists, modifiers, type, identifier,
                SyntaxFactory.AccessorList(setter == null ? SyntaxFactory.SingletonList(getter) : SyntaxFactory.List(new[] { getter, setter }))
                );
        }
        internal static PropertyDeclarationSyntax Property(SyntaxTokenList modifiers, TypeSyntax type, SyntaxToken identifier, bool getterOnly,
            SyntaxTokenList getterModifiers, IEnumerable<StatementSyntax> getterStatements,
            SyntaxTokenList setterModifiers = default(SyntaxTokenList), IEnumerable<StatementSyntax> setterStatements = null) {
            return Property(null, modifiers, type, identifier, getterOnly, getterModifiers, getterStatements, setterModifiers, setterStatements);
        }
        internal static PropertyDeclarationSyntax Property(SyntaxTokenList modifiers, TypeSyntax type, string identifier, bool getterOnly,
            SyntaxTokenList getterModifiers, IEnumerable<StatementSyntax> getterStatements,
            SyntaxTokenList setterModifiers = default(SyntaxTokenList), IEnumerable<StatementSyntax> setterStatements = null) {
            return Property(modifiers, type, Id(identifier), getterOnly, getterModifiers, getterStatements, setterModifiers, setterStatements);
        }
        private static AccessorDeclarationSyntax AccessorDecl(SyntaxKind kind, SyntaxTokenList modifiers, IEnumerable<StatementSyntax> statements) {
            return SyntaxFactory.AccessorDeclaration(
                kind: kind,
                attributeLists: default(SyntaxList<AttributeListSyntax>),
                modifiers: modifiers,
                body: SyntaxFactory.Block(statements));
        }
        //{ get; set; }
        internal static AccessorListSyntax GetSetAccessorList {
            get {
                return SyntaxFactory.AccessorList(SyntaxFactory.List(new[] {
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration, default(SyntaxList<AttributeListSyntax>), default(SyntaxTokenList), 
                        SyntaxFactory.Token(SyntaxKind.GetKeyword), null, SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration, default(SyntaxList<AttributeListSyntax>), default(SyntaxTokenList),
                        SyntaxFactory.Token(SyntaxKind.SetKeyword), null, SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                }));
            }
        }
        //{ get; private set; }
        internal static AccessorListSyntax GetPrivateSetAccessorList {
            get {
                return SyntaxFactory.AccessorList(SyntaxFactory.List(new[] {
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration, default(SyntaxList<AttributeListSyntax>), default(SyntaxTokenList), 
                        SyntaxFactory.Token(SyntaxKind.GetKeyword), null, SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration, default(SyntaxList<AttributeListSyntax>), PrivateTokenList,
                        SyntaxFactory.Token(SyntaxKind.SetKeyword), null, SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                }));
            }
        }

        internal static IndexerDeclarationSyntax Indexer(SyntaxTokenList modifiers, TypeSyntax type, IEnumerable<ParameterSyntax> parameters, bool getterOnly,
            SyntaxTokenList getterModifiers, IEnumerable<StatementSyntax> getterStatements,
            SyntaxTokenList setterModifiers = default(SyntaxTokenList), IEnumerable<StatementSyntax> setterStatements = null) {
            var getter = AccessorDecl(SyntaxKind.GetAccessorDeclaration, getterModifiers, getterStatements);
            AccessorDeclarationSyntax setter = null;
            if (!getterOnly) setter = AccessorDecl(SyntaxKind.SetAccessorDeclaration, setterModifiers, setterStatements);
            return SyntaxFactory.IndexerDeclaration(
                attributeLists: default(SyntaxList<AttributeListSyntax>),
                modifiers: modifiers,
                type: type,
                explicitInterfaceSpecifier: null,
                parameterList: BracketedParameterList(parameters),
                accessorList: SyntaxFactory.AccessorList(setter == null ? SyntaxFactory.SingletonList(getter) : SyntaxFactory.List(new[] { getter, setter })));
        }
        internal static MethodDeclarationSyntax Method(IEnumerable<AttributeListSyntax> attributeLists, SyntaxTokenList modifiers,
            TypeSyntax returnType, SyntaxToken identifier, IEnumerable<TypeParameterSyntax> typeParameters,
            IEnumerable<ParameterSyntax> parameters, IEnumerable<TypeParameterConstraintClauseSyntax> constraintClauses,
            IEnumerable<StatementSyntax> statements) {
            return SyntaxFactory.MethodDeclaration(
                attributeLists: SyntaxFactory.List(attributeLists),
                modifiers: modifiers,
                returnType: returnType,
                explicitInterfaceSpecifier: null,
                identifier: identifier,
                typeParameterList: TypeParameterList(typeParameters),
                parameterList: ParameterList(parameters),
                constraintClauses: SyntaxFactory.List(constraintClauses),
                body: SyntaxFactory.Block(statements),
                expressionBody: null);
        }
        internal static MethodDeclarationSyntax Method(IEnumerable<AttributeListSyntax> attributeLists, SyntaxTokenList modifiers,
            TypeSyntax returnType, SyntaxToken identifier, IEnumerable<ParameterSyntax> parameters, IEnumerable<StatementSyntax> statements) {
            return Method(attributeLists, modifiers, returnType, identifier, null, parameters, null, statements);
        }
        internal static MethodDeclarationSyntax Method(SyntaxTokenList modifiers, TypeSyntax returnType, SyntaxToken identifier,
            IEnumerable<ParameterSyntax> parameters, IEnumerable<StatementSyntax> statements) {
            return Method(null, modifiers, returnType, identifier, parameters, statements);
        }
        internal static MethodDeclarationSyntax Method(SyntaxTokenList modifiers, TypeSyntax returnType, string identifier,
            IEnumerable<ParameterSyntax> parameters, params StatementSyntax[] statements) {
            return Method(modifiers, returnType, Id(identifier), parameters, statements);
        }
        internal static ConstructorDeclarationSyntax Constructor(SyntaxTokenList modifiers, SyntaxToken identifier, IEnumerable<ParameterSyntax> parameters,
            ConstructorInitializerSyntax initializer, IEnumerable<StatementSyntax> statements) {
            return SyntaxFactory.ConstructorDeclaration(
                attributeLists: default(SyntaxList<AttributeListSyntax>),
                modifiers: modifiers,
                identifier: identifier,
                parameterList: ParameterList(parameters),
                initializer: initializer,
                body: SyntaxFactory.Block(statements));
        }
        internal static ConstructorDeclarationSyntax Constructor(SyntaxTokenList modifiers, string identifier, IEnumerable<ParameterSyntax> parameters,
            ConstructorInitializerSyntax initializer, params StatementSyntax[] statements) {
            return Constructor(modifiers, Id(identifier), parameters, initializer, statements);
        }
        internal static ConstructorInitializerSyntax ConstructorInitializer(bool isBase, IEnumerable<ExpressionSyntax> argExprs) {
            return SyntaxFactory.ConstructorInitializer(
                kind: isBase ? SyntaxKind.BaseConstructorInitializer : SyntaxKind.ThisConstructorInitializer,
                argumentList: ArgumentList(argExprs));
        }
        internal static ConstructorInitializerSyntax ConstructorInitializer(bool isBase, params ExpressionSyntax[] argExprs) {
            return ConstructorInitializer(isBase, (IEnumerable<ExpressionSyntax>)argExprs);
        }
        internal static ConversionOperatorDeclarationSyntax ConversionOperator(bool isImplict, TypeSyntax type,
            IEnumerable<ParameterSyntax> parameters, IEnumerable<StatementSyntax> statements) {
            return SyntaxFactory.ConversionOperatorDeclaration(
                attributeLists: default(SyntaxList<AttributeListSyntax>),
                modifiers: PublicStaticTokenList,
                implicitOrExplicitKeyword: isImplict ? ImplictToken : ExplictToken,
                type: type,
                parameterList: ParameterList(parameters),
                body: SyntaxFactory.Block(statements),
                expressionBody: null);
        }
        internal static ConversionOperatorDeclarationSyntax ConversionOperator(bool isImplict, TypeSyntax type,
            IEnumerable<ParameterSyntax> parameters, params StatementSyntax[] statements) {
            return ConversionOperator(isImplict, type, parameters, (IEnumerable<StatementSyntax>)statements);
        }
        //>public static bool operator ==(TYPE left, TYPE right) { ... }
        internal static OperatorDeclarationSyntax OperatorDecl(TypeSyntax returnType, SyntaxToken operatorToken,
            IEnumerable<ParameterSyntax> parameters, IEnumerable<StatementSyntax> statements) {
            return SyntaxFactory.OperatorDeclaration(
                attributeLists: default(SyntaxList<AttributeListSyntax>),
                modifiers: PublicStaticTokenList,
                returnType: returnType,
                operatorToken: operatorToken,
                parameterList: ParameterList(parameters),
                body: SyntaxFactory.Block(statements),
                expressionBody: null);
        }
        internal static OperatorDeclarationSyntax OperatorDecl(TypeSyntax returnType, SyntaxToken operatorToken,
            IEnumerable<ParameterSyntax> parameters, params StatementSyntax[] statements) {
            return OperatorDecl(returnType, operatorToken, parameters, (IEnumerable<StatementSyntax>)statements);
        }
        internal static FieldDeclarationSyntax Field(SyntaxTokenList modifiers, TypeSyntax type, SyntaxToken identifier, ExpressionSyntax initializer = null) {
            return SyntaxFactory.FieldDeclaration(
                attributeLists: default(SyntaxList<AttributeListSyntax>),
                modifiers: modifiers,
                declaration: VarDecl(type, identifier, initializer));
        }
        internal static FieldDeclarationSyntax Field(SyntaxTokenList modifiers, TypeSyntax type, string identifier, ExpressionSyntax initializer = null) {
            return Field(modifiers, type, Id(identifier), initializer);
        }



        //
        //
        //symbols
        //
        //
        internal static readonly string[] GuidNameParts = new string[] { "Guid", "System" };
        internal static readonly string[] TimeSpanNameParts = new string[] { "TimeSpan", "System" };
        internal static readonly string[] DateTimeOffsetNameParts = new string[] { "DateTimeOffset", "System" };
        //internal static readonly string[] Func2NameParts = new string[] { "Func`2", "System" };

        internal static readonly string[] ICollection1NameParts = new string[] { "ICollection`1", "Generic", "Collections", "System" };
        internal static readonly string[] IDictionary2NameParts = new string[] { "IDictionary`2", "Generic", "Collections", "System" };
        internal static readonly string[] ISet1NameParts = new string[] { "ISet`1", "Generic", "Collections", "System" };

        //eg: symbol.FullNameEquals(new string[]{"List`1", "Generic", "Collections", "System"})
        internal static bool FullNameEquals(this ISymbol symbol, string[] nameParts) {
            if (symbol == null) throw new ArgumentNullException("symbol");
            if (nameParts == null || nameParts.Length == 0) throw new ArgumentNullException("nameParts");
            var idx = 0;
            for (; symbol != null; symbol = symbol.ContainingSymbol) {
                var name = symbol.MetadataName;
                if (string.IsNullOrEmpty(name)) break;
                if (idx == nameParts.Length) return false;
                if (name != nameParts[idx]) return false;
                idx++;
            }
            return idx == nameParts.Length;
        }
        internal static bool FullNameEquals(this ITypeSymbol typeSymbol, string[] nameParts, bool isNullable) {
            if (isNullable) {
                if (typeSymbol.SpecialType == SpecialType.System_Nullable_T) {
                    return FullNameEquals(((INamedTypeSymbol)typeSymbol).TypeArguments[0], nameParts);
                }
                return false;
            }
            return FullNameEquals(typeSymbol, nameParts);
        }
        internal static bool FullNameEquals(this ISymbol symbol, string[] genericTypeNameParts, string[] typeArgNameParts) {
            if (symbol.FullNameEquals(genericTypeNameParts)) {
                return ((INamedTypeSymbol)symbol).TypeArguments[0].FullNameEquals(typeArgNameParts);
            }
            return false;
        }
        internal static bool FullNameEquals(this ISymbol symbol, string[] genericTypeNameParts, string[] typeArg1NameParts, string[] typeArg2NameParts) {
            if (symbol.FullNameEquals(genericTypeNameParts)) {
                var typeArgs = ((INamedTypeSymbol)symbol).TypeArguments;
                return typeArgs[0].FullNameEquals(typeArg1NameParts) && typeArgs[1].FullNameEquals(typeArg2NameParts);
            }
            return false;
        }

        //eg: var idx = symbol.MatchFullNames(new []{"List`1", "Dictionary`2"}, new []{"Generic", "Collections", "System"});
        //idx: -1: none; 0: symbol is List`1; 1: symbol is Dictionary`2 
        internal static int MatchFullNames(this ISymbol symbol, string[] typeNames, string[] outerNameParts) {
            if (symbol == null) throw new ArgumentNullException("symbol");
            if (typeNames == null || typeNames.Length == 0) throw new ArgumentNullException("typeNames");
            var fullLength = 1 + (outerNameParts != null ? outerNameParts.Length : 0);
            int idx = 0, result = -1;
            for (; symbol != null; symbol = symbol.ContainingSymbol) {
                var name = symbol.MetadataName;
                if (string.IsNullOrEmpty(name)) break;
                if (idx == fullLength) return -1;
                if (idx == 0) {
                    for (var i = 0; i < typeNames.Length; i++) {
                        if (name == typeNames[i]) {
                            result = i;
                            break;
                        }
                    }
                    if (result == -1) return -1;
                }
                else {
                    if (name != outerNameParts[idx - 1]) return -1;
                }
                idx++;
            }
            if (idx == fullLength) return result;
            return -1;
        }

        internal static string[] GetFullName(this ISymbol symbol) {
            if (symbol == null) throw new ArgumentNullException("symbol");
            var list = new List<string>();
            for (; symbol != null; symbol = symbol.ContainingSymbol) {
                var name = symbol.MetadataName;
                if (string.IsNullOrEmpty(name)) break;
                list.Add(name);
            }
            if (list.Count == 0) throw new ArgumentException("symbol");
            return list.ToArray();
        }
        internal static NameSyntax ToNameSyntax(this ISymbol symbol) {
            return SyntaxFactory.ParseName(symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
        }
        internal static TypeSyntax ToTypeSyntax(this ISymbol symbol) {
            return SyntaxFactory.ParseTypeName(symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
        }

        internal static bool ThisOrBaseFullNameEquals(this INamedTypeSymbol symbol, string[] nameParts) {
            for (; symbol != null; symbol = symbol.BaseType) {
                if (symbol.FullNameEquals(nameParts)) {
                    return true;
                }
            }
            return false;
        }

        internal static AttributeData GetAttributeData(this ISymbol symbol, string[] nameParts) {
            foreach (var attData in symbol.GetAttributes()) {
                if (attData.AttributeClass.FullNameEquals(nameParts)) {
                    return attData;
                }
            }
            return null;
        }
        internal static INamedTypeSymbol GetInterface(this ITypeSymbol typeSymbol, string[] nameParts) {
            foreach (var itf in typeSymbol.AllInterfaces) {
                if (itf.FullNameEquals(nameParts)) {
                    return itf;
                }
            }
            return null;
        }
        internal static INamedTypeSymbol GetSelfOrInterface(this ITypeSymbol typeSymbol, string[] nameParts) {
            if (typeSymbol.FullNameEquals(nameParts)) {
                return (INamedTypeSymbol)typeSymbol;
            }
            return GetInterface(typeSymbol, nameParts);
        }
        internal static bool HasParameterlessConstructor(this INamedTypeSymbol typeSymbol) {
            foreach (var methodSymbol in typeSymbol.InstanceConstructors) {
                if (methodSymbol.Parameters.Length == 0) {
                    return true;
                }
            }
            return false;
        }



    }

}
