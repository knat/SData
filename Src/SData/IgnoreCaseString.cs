using System;

namespace SData
{
    public sealed class IgnoreCaseString : IEquatable<IgnoreCaseString>, IComparable<IgnoreCaseString>
    {
        private string _value;
        private bool _isReadOnly;
        public IgnoreCaseString(string value, bool isReadOnly = false)
        {
            Value = value;
            _isReadOnly = isReadOnly;
        }
        public static implicit operator IgnoreCaseString(string value)
        {
            if (value == null) return null;
            return new IgnoreCaseString(value);
        }
        public static implicit operator string (IgnoreCaseString obj)
        {
            if ((object)obj == null) return null;
            return obj._value;
        }
        public string Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (_isReadOnly)
                {
                    throw new InvalidOperationException("The object is readonly.");
                }
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                _value = value;
            }
        }
        public bool IsReadOnly
        {
            get { return _isReadOnly; }
        }
        public void AsReadOnly()
        {
            _isReadOnly = true;
        }
        private static readonly StringComparer _stringComparer = StringComparer.OrdinalIgnoreCase;
        public bool Equals(IgnoreCaseString other)
        {
            if ((object)this == (object)other) return true;
            if ((object)other == null) return false;
            return _stringComparer.Equals(_value, other._value);
        }
        public override bool Equals(object obj)
        {
            return Equals(obj as IgnoreCaseString);
        }
        public override int GetHashCode()
        {
            return _stringComparer.GetHashCode(_value);
        }
        public static bool operator ==(IgnoreCaseString left, IgnoreCaseString right)
        {
            if ((object)left == null)
            {
                return (object)right == null;
            }
            return left.Equals(right);
        }
        public static bool operator !=(IgnoreCaseString left, IgnoreCaseString right)
        {
            return !(left == right);
        }
        public int CompareTo(IgnoreCaseString other)
        {
            if ((object)this == (object)other) return 0;
            if ((object)other == null) return 1;
            return _stringComparer.Compare(_value, other._value);
        }
        public override string ToString()
        {
            return _value;
        }
    }
}
