using System;
using System.Collections.Generic;

namespace SData
{
    public sealed class Binary : IEquatable<Binary>, IList<byte>
    {
        internal static readonly byte[] EmptyBytes = new byte[0];
        private byte[] _bytes;
        private int _count;
        private bool _isReadOnly;
        public Binary(byte[] bytes, bool isReadOnly = false)
        {
            if (bytes == null) throw new ArgumentNullException("bytes");
            _bytes = bytes;
            _count = bytes.Length;
            _isReadOnly = isReadOnly;
        }
        public Binary()
            : this(EmptyBytes, false)
        {
        }
        public static implicit operator Binary(byte[] bytes)
        {
            if (bytes == null) return null;
            return new Binary(bytes);
        }
        public static bool TryFromBase64String(string s, out Binary result, bool isReadOnly = false)
        {
            if (s == null) throw new ArgumentNullException("s");
            if (s.Length == 0)
            {
                result = new Binary(EmptyBytes, isReadOnly);
                return true;
            }
            try
            {
                var bytes = Convert.FromBase64String(s);
                result = new Binary(bytes, isReadOnly);
                return true;
            }
            catch (FormatException)
            {
                result = null;
                return false;
            }
        }
        public string ToBase64String()
        {
            if (_count == 0) return string.Empty;
            return Convert.ToBase64String(_bytes, 0, _count);
        }
        public override string ToString()
        {
            return ToBase64String();
        }
        public byte[] ToBytes()
        {
            var bytes = new byte[_count];
            Array.Copy(_bytes, 0, bytes, 0, _count);
            return bytes;
        }
        public byte[] GetBytes(out int count)
        {
            ThrowIfReadOnly();
            count = _count;
            var bytes = _bytes;
            _bytes = EmptyBytes;
            _count = 0;
            return bytes;
        }
        public bool IsReadOnly
        {
            get { return _isReadOnly; }
        }
        public void AsReadOnly()
        {
            _isReadOnly = true;
        }
        private void ThrowIfReadOnly()
        {
            if (_isReadOnly)
            {
                throw new InvalidOperationException("The object is readonly.");
            }
        }
        public int Count
        {
            get { return _count; }
        }
        public void CopyTo(byte[] array, int arrayIndex)
        {
            Array.Copy(_bytes, 0, array, arrayIndex, _count);
        }
        public void Set(byte[] bytes)
        {
            ThrowIfReadOnly();
            if (bytes == null) throw new ArgumentNullException("bytes");
            _bytes = bytes;
            _count = bytes.Length;
        }
        public byte this[int index]
        {
            get
            {
                if (index >= _count)
                {
                    throw new ArgumentOutOfRangeException("index");
                }
                return _bytes[index];
            }
            set
            {
                ThrowIfReadOnly();
                if (index >= _count)
                {
                    throw new ArgumentOutOfRangeException("index");
                }
                _bytes[index] = value;
            }
        }
        public void AddRange(byte[] array)
        {
            if (array == null) throw new ArgumentNullException("array");
            AddRange(array, 0, array.Length);
        }
        public void AddRange(byte[] array, int arrayIndex, int length)
        {
            InsertRange(_count, array, arrayIndex, length);
        }
        public void InsertRange(int index, byte[] array)
        {
            if (array == null) throw new ArgumentNullException("array");
            InsertRange(index, array, 0, array.Length);
        }
        public void InsertRange(int index, byte[] array, int arrayIndex, int length)
        {
            ThrowIfReadOnly();
            var count = _count;
            if (index < 0 || index > count)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            if (array == null) throw new ArgumentNullException("array");
            if (arrayIndex < 0)
            {
                throw new ArgumentOutOfRangeException("arrayIndex");
            }
            if (length < 0 || arrayIndex + length > array.Length)
            {
                throw new ArgumentOutOfRangeException("length");
            }
            if (length > 0)
            {
                var newCount = count + length;
                if (newCount > _bytes.Length)
                {
                    var newLength = _bytes.Length + length;
                    if (newLength <= 32)
                    {
                        newLength = 64;
                    }
                    else if (newLength < 1024 * 8)
                    {
                        newLength *= 2;
                    }
                    else
                    {
                        newLength += 1024 * 2;
                    }
                    Enlarge(newLength);
                }
                if (index < count)
                {
                    Array.Copy(_bytes, index, _bytes, index + length, count - index);
                }
                Array.Copy(array, arrayIndex, _bytes, index, length);
                _count = newCount;
            }
        }
        private void Enlarge(int newLength)
        {
            var bytes = new byte[newLength];
            Array.Copy(_bytes, 0, bytes, 0, _bytes.Length);
            _bytes = bytes;
        }
        public void RemoveRange(int index, int length)
        {
            ThrowIfReadOnly();
            var count = _count;
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            if (length < 0 || index + length > count)
            {
                throw new ArgumentOutOfRangeException("length");
            }
            if (length > 0)
            {
                count -= length;
                if (index < count)
                {
                    Array.Copy(_bytes, index + length, _bytes, index, count - index);
                }
                Array.Clear(_bytes, count, length);
                _count = count;
            }
        }
        public void Clear()
        {
            ThrowIfReadOnly();
            Array.Clear(_bytes, 0, _count);
            _count = 0;
        }
        public void Add(byte item)
        {
            Insert(_count, item);
        }
        public void Insert(int index, byte item)
        {
            ThrowIfReadOnly();
            var count = _count;
            if (index < 0 || index > count)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            if (count == _bytes.Length)
            {
                int newLength;
                if (count <= 32)
                {
                    newLength = 64;
                }
                else if (count < 1024 * 8)
                {
                    newLength = count * 2;
                }
                else
                {
                    newLength = count + 1024 * 2;
                }
                Enlarge(newLength);
            }
            if (index < count)
            {
                Array.Copy(_bytes, index, _bytes, index + 1, count - index);
            }
            _bytes[index] = item;
            ++_count;
        }
        public void RemoveAt(int index)
        {
            ThrowIfReadOnly();
            var count = _count;
            if (index < 0 || index >= count)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            --count;
            if (index < count)
            {
                Array.Copy(_bytes, index + 1, _bytes, index, count - index);
            }
            _bytes[count] = 0;
            _count = count;
        }
        public bool Remove(byte item)
        {
            ThrowIfReadOnly();
            var idx = IndexOf(item);
            if (idx >= 0)
            {
                RemoveAt(idx);
                return true;
            }
            return false;
        }
        public int IndexOf(byte item)
        {
            var count = _count;
            var bytes = _bytes;
            for (var i = 0; i < count; ++i)
            {
                if (bytes[i] == item) return i;
            }
            return -1;
        }
        public bool Contains(byte item)
        {
            return IndexOf(item) >= 0;
        }
        public struct Enumerator : IEnumerator<byte>
        {
            internal Enumerator(Binary binary)
            {
                _binary = binary;
                _index = 0;
                _current = 0;
            }
            private readonly Binary _binary;
            private int _index;
            private byte _current;
            public bool MoveNext()
            {
                var bin = _binary;
                var idx = _index;
                if (idx < bin._count)
                {
                    _current = bin._bytes[idx];
                    ++_index;
                    return true;
                }
                return false;
            }
            public byte Current
            {
                get { return _current; }
            }
            object System.Collections.IEnumerator.Current
            {
                get { return _current; }
            }
            public void Reset()
            {
                _index = 0;
                _current = 0;
            }
            public void Dispose()
            {
            }
        }
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }
        IEnumerator<byte> IEnumerable<byte>.GetEnumerator()
        {
            return GetEnumerator();
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        //
        public bool Equals(Binary other)
        {
            if ((object)this == (object)other) return true;
            if ((object)other == null) return false;
            var xBytes = _bytes;
            var yBytes = other._bytes;
            if (xBytes == yBytes) return true;
            var xCount = _count;
            if (xCount != other._count) return false;
            for (var i = 0; i < xCount; ++i)
            {
                if (xBytes[i] != yBytes[i]) return false;
            }
            return true;
        }
        public override bool Equals(object obj)
        {
            return Equals(obj as Binary);
        }
        public override int GetHashCode()
        {
            var bytes = _bytes;
            var count = Math.Min(_count, 7);
            var hash = 17;
            for (var i = 0; i < count; ++i)
            {
                hash = Extensions.AggregateHash(hash, bytes[i]);
            }
            return hash;
        }
        public static bool operator ==(Binary left, Binary right)
        {
            if ((object)left == null)
            {
                return (object)right == null;
            }
            return left.Equals(right);
        }
        public static bool operator !=(Binary left, Binary right)
        {
            return !(left == right);
        }

    }
}
