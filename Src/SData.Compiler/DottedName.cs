using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SData.Compiler {
    internal sealed class DottedName : IEquatable<DottedName> {
        public static bool TryParse(string dottedNameStr, out DottedName result) {
            if (dottedNameStr == null) throw new ArgumentNullException("dottedNameStr");
            result = null;
            List<string> partList = null;
            foreach (var i in dottedNameStr.Split(_dotCharArray)) {
                if (i.Length == 0) {
                    return false;
                }
                var part = i.UnescapeId();
                if (!SyntaxFacts.IsValidIdentifier(part)) {
                    return false;
                }
                if (partList == null) {
                    partList = new List<string>();
                }
                partList.Add(part);
            }
            partList.Reverse();
            result = new DottedName(partList.ToArray());
            return true;
        }
        private static readonly char[] _dotCharArray = new char[] { '.' };
        public DottedName(string[] nameParts) {
            if (nameParts == null || nameParts.Length == 0) throw new ArgumentNullException("nameParts");
            NameParts = nameParts;
        }
        public DottedName(DottedName parent, string name) {
            var parentNameParts = parent.NameParts;
            var nameParts = new string[parentNameParts.Length + 1];
            nameParts[0] = name;
            Array.Copy(parentNameParts, 0, nameParts, 1, parentNameParts.Length);
            NameParts = nameParts;
        }
        public DottedName Clone() {
            return new DottedName((string[])NameParts.Clone());
        }
        public readonly string[] NameParts;//eg: {"List`1", "Generic", "Collections", "System"}
        public string LastName {
            get { return NameParts[0]; }
        }
        public IEnumerable<string> Names {
            get {
                for (var i = NameParts.Length - 1; i >= 0; --i) {
                    yield return NameParts[i];
                }
            }
        }
        public override string ToString() {
            return string.Join(".", Names);
        }
        private NameSyntax _nonGlobalFullNameSyntax;//@NS1.NS2.Type
        internal NameSyntax NonGlobalFullNameSyntax {
            get {
                if (_nonGlobalFullNameSyntax == null) {
                    foreach (var name in Names) {
                        if (_nonGlobalFullNameSyntax == null) {
                            _nonGlobalFullNameSyntax = CS.IdName(name.EscapeId());
                        }
                        else {
                            _nonGlobalFullNameSyntax = CS.QualifiedName(_nonGlobalFullNameSyntax, name.EscapeId());
                        }
                    }
                }
                return _nonGlobalFullNameSyntax;
            }
        }

        private NameSyntax _fullNameSyntax;//global::@NS1.NS2.Type
        internal NameSyntax FullNameSyntax {
            get {
                if (_fullNameSyntax == null) {
                    foreach (var name in Names) {
                        if (_fullNameSyntax == null) {
                            _fullNameSyntax = CS.GlobalAliasQualifiedName(name.EscapeId());
                        }
                        else {
                            _fullNameSyntax = CS.QualifiedName(_fullNameSyntax, name.EscapeId());
                        }
                    }
                }
                return _fullNameSyntax;
            }
        }
        private ExpressionSyntax _fullExprSyntax;
        internal ExpressionSyntax FullExprSyntax {
            get {
                if (_fullExprSyntax == null) {
                    foreach (var name in Names) {
                        if (_fullExprSyntax == null) {
                            _fullExprSyntax = CS.GlobalAliasQualifiedName(name.EscapeId());
                        }
                        else {
                            _fullExprSyntax = CS.MemberAccessExpr(_fullExprSyntax, name.EscapeId());
                        }
                    }
                }
                return _fullExprSyntax;
            }
        }


        //
        public bool Equals(DottedName other) {
            if ((object)this == (object)other) return true;
            if ((object)other == null) return false;
            var xParts = NameParts;
            var yParts = other.NameParts;
            if (xParts == yParts) return true;
            var xCount = xParts.Length;
            if (xCount != yParts.Length) return false;
            for (var i = 0; i < xCount; ++i) {
                if (xParts[i] != yParts[i]) return false;
            }
            return true;
        }
        public override bool Equals(object obj) {
            return Equals(obj as DottedName);
        }
        public override int GetHashCode() {
            var parts = NameParts;
            var count = Math.Min(parts.Length, 7);
            var hash = 17;
            for (var i = 0; i < count; ++i) {
                hash = Extensions.AggregateHash(hash, parts[i].GetHashCode());
            }
            return hash;
        }
        public static bool operator ==(DottedName left, DottedName right) {
            if ((object)left == null) {
                return (object)right == null;
            }
            return left.Equals(right);
        }
        public static bool operator !=(DottedName left, DottedName right) {
            return !(left == right);
        }

    }
}
