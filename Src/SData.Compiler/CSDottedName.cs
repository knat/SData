using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SData.Compiler
{
    internal sealed class CSDottedName : IEquatable<CSDottedName>
    {
        public static bool TryParse(string dottedString, out CSDottedName result)
        {
            result = null;
            if (string.IsNullOrEmpty(dottedString))
            {
                return false;
            }
            List<string> partList = null;
            foreach (var i in dottedString.Split(_dotCharArray))
            {
                if (i.Length == 0)
                {
                    return false;
                }
                var part = i.UnescapeId();
                if (!SyntaxFacts.IsValidIdentifier(part))
                {
                    return false;
                }
                if (partList == null)
                {
                    partList = new List<string>();
                }
                partList.Add(part);
            }
            partList.Reverse();
            result = new CSDottedName(partList.ToArray());
            return true;
        }
        public static bool TrySplit(string dottedString, out string first, out string second)
        {
            if (!string.IsNullOrEmpty(dottedString))
            {
                var arr = dottedString.Split(_dotCharArray);
                if (arr.Length == 2)
                {
                    first = arr[0];
                    second = arr[1];
                    if (SyntaxFacts.IsValidIdentifier(first) && SyntaxFacts.IsValidIdentifier(second))
                    {
                        return true;
                    }
                }
            }
            first = null;
            second = null;
            return false;
        }
        public static bool TrySplit(string dottedString, out string first, out string second, out string third)
        {
            if (!string.IsNullOrEmpty(dottedString))
            {
                var arr = dottedString.Split(_dotCharArray);
                if (arr.Length == 3)
                {
                    first = arr[0];
                    second = arr[1];
                    third = arr[2];
                    if (SyntaxFacts.IsValidIdentifier(first) && SyntaxFacts.IsValidIdentifier(second) && SyntaxFacts.IsValidIdentifier(third))
                    {
                        return true;
                    }
                }
            }
            first = null;
            second = null;
            third = null;
            return false;
        }

        private static readonly char[] _dotCharArray = new char[] { '.' };
        private CSDottedName(string[] nameParts)
        {
            if (nameParts == null || nameParts.Length == 0) throw new ArgumentNullException("nameParts");
            NameParts = nameParts;
        }
        public CSDottedName(CSDottedName parent, string name)
        {
            if (parent == null) throw new ArgumentNullException("parent");
            if (!SyntaxFacts.IsValidIdentifier(name)) throw new ArgumentException("Invalid name.");
            var parentNameParts = parent.NameParts;
            var nameParts = new string[parentNameParts.Length + 1];
            nameParts[0] = name;
            Array.Copy(parentNameParts, 0, nameParts, 1, parentNameParts.Length);
            NameParts = nameParts;
        }
        public CSDottedName Clone()
        {
            return new CSDottedName((string[])NameParts.Clone());
        }
        public readonly string[] NameParts;//eg: {"List`1", "Generic", "Collections", "System"}
        public string LastName
        {
            get { return NameParts[0]; }
        }
        public IEnumerable<string> Names
        {
            get
            {
                for (var i = NameParts.Length - 1; i >= 0; --i)
                {
                    yield return NameParts[i];
                }
            }
        }
        private string _string;
        public override string ToString()
        {
            return _string ?? (_string = string.Join(".", Names));
        }
        private NameSyntax _nonGlobalFullNameSyntax;//@NS1.NS2.Type
        internal NameSyntax NonGlobalFullNameSyntax
        {
            get
            {
                if (_nonGlobalFullNameSyntax == null)
                {
                    NameSyntax result = null;
                    foreach (var name in Names)
                    {
                        if (result == null)
                        {
                            result = CS.UnescapedIdName(name);
                        }
                        else
                        {
                            result = CS.QualifiedName(result, name.EscapeId());
                        }
                    }
                    _nonGlobalFullNameSyntax = result;
                }
                return _nonGlobalFullNameSyntax;
            }
        }

        private NameSyntax _fullNameSyntax;//global::@NS1.NS2.Type
        internal NameSyntax FullNameSyntax
        {
            get
            {
                if (_fullNameSyntax == null)
                {
                    NameSyntax result = null;
                    foreach (var name in Names)
                    {
                        if (result == null)
                        {
                            result = CS.GlobalAliasQualifiedName(name.EscapeId());
                        }
                        else
                        {
                            result = CS.QualifiedName(result, name.EscapeId());
                        }
                    }
                    _fullNameSyntax = result;
                }
                return _fullNameSyntax;
            }
        }
        private ExpressionSyntax _fullExprSyntax;
        internal ExpressionSyntax FullExprSyntax
        {
            get
            {
                if (_fullExprSyntax == null)
                {
                    ExpressionSyntax result = null;
                    foreach (var name in Names)
                    {
                        if (result == null)
                        {
                            result = CS.GlobalAliasQualifiedName(name.EscapeId());
                        }
                        else
                        {
                            result = CS.MemberAccessExpr(result, name.EscapeId());
                        }
                    }
                    _fullExprSyntax = result;
                }
                return _fullExprSyntax;
            }
        }
        //
        public bool Equals(CSDottedName other)
        {
            if ((object)this == (object)other) return true;
            if ((object)other == null) return false;
            var xParts = NameParts;
            var yParts = other.NameParts;
            if (xParts == yParts) return true;
            var xCount = xParts.Length;
            if (xCount != yParts.Length) return false;
            for (var i = 0; i < xCount; ++i)
            {
                if (xParts[i] != yParts[i]) return false;
            }
            return true;
        }
        public override bool Equals(object obj)
        {
            return Equals(obj as CSDottedName);
        }
        public override int GetHashCode()
        {
            var parts = NameParts;
            var count = Math.Min(parts.Length, 7);
            var hash = 17;
            for (var i = 0; i < count; ++i)
            {
                hash = Extensions.AggregateHash(hash, parts[i].GetHashCode());
            }
            return hash;
        }
        public static bool operator ==(CSDottedName left, CSDottedName right)
        {
            if ((object)left == null)
            {
                return (object)right == null;
            }
            return left.Equals(right);
        }
        public static bool operator !=(CSDottedName left, CSDottedName right)
        {
            return !(left == right);
        }

    }
}
