using System;
using System.Collections.Generic;

namespace SData {
    public interface IObjectSet<TKey, TObject> : ICollection<TObject> {
        Func<TObject, TKey> KeySelector { get; set; }
        new bool Add(TObject obj);
        TObject this[TKey key] { get; }
        ICollection<TKey> Keys { get; }
        bool ContainsKey(TKey key);
        bool TryGetValue(TKey key, out TObject obj);
        bool Remove(TKey key);
    }
    public class ObjectSet<TKey, TObject> : IObjectSet<TKey, TObject> {
        public ObjectSet() : this(null, null, null) { }
        public ObjectSet(Func<TObject, TKey> keySelector, IEnumerable<TObject> objs = null) : this(keySelector, null, objs) { }
        public ObjectSet(Func<TObject, TKey> keySelector, IEqualityComparer<TKey> keyComparer, IEnumerable<TObject> objs = null) {
            _keySelector = keySelector;
            _dict = new Dictionary<TKey, TObject>(keyComparer);
            AddRange(objs);
        }
        private readonly Dictionary<TKey, TObject> _dict;
        private Func<TObject, TKey> _keySelector;
        public Func<TObject, TKey> KeySelector {
            get {
                return _keySelector;
            }
            set {
                _keySelector = value;
            }
        }
        public IEqualityComparer<TKey> KeyComparer {
            get {
                return _dict.Comparer;
            }
        }
        public int Count {
            get {
                return _dict.Count;
            }
        }
        private TKey GetObjectKey(TObject obj) {
            var keySelector = _keySelector;
            if (keySelector == null) throw new InvalidOperationException("KeySelector is null.");
            return keySelector(obj);
        }
        public bool Add(TObject obj) {
            var key = GetObjectKey(obj);
            if (_dict.ContainsKey(key)) {
                return false;
            }
            _dict.Add(key, obj);
            return true;
        }
        void ICollection<TObject>.Add(TObject obj) {
            Add(obj);
        }
        public void AddRange(IEnumerable<TObject> objs) {
            if (objs != null) {
                foreach (var obj in objs) {
                    Add(obj);
                }
            }
        }
        public TObject this[TKey key] {
            get {
                return _dict[key];
            }
        }
        public ICollection<TKey> Keys {
            get {
                return _dict.Keys;
            }
        }
        public bool ContainsKey(TKey key) {
            return _dict.ContainsKey(key);
        }
        public bool Contains(TObject obj) {
            return _dict.ContainsKey(GetObjectKey(obj));
        }
        public bool TryGetValue(TKey key, out TObject obj) {
            return _dict.TryGetValue(key, out obj);
        }
        public bool Remove(TKey key) {
            return _dict.Remove(key);
        }
        public bool Remove(TObject obj) {
            return _dict.Remove(GetObjectKey(obj));
        }
        public void Clear() {
            _dict.Clear();
        }
        public Dictionary<TKey, TObject>.ValueCollection.Enumerator GetEnumerator() {
            return _dict.Values.GetEnumerator();
        }
        IEnumerator<TObject> IEnumerable<TObject>.GetEnumerator() {
            return GetEnumerator();
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
        public void CopyTo(TObject[] array, int arrayIndex) {
            _dict.Values.CopyTo(array, arrayIndex);
        }
        bool ICollection<TObject>.IsReadOnly {
            get { return false; }
        }
    }
}
