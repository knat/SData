using System;
using System.IO;

namespace SData.Internal {
    public sealed class SimpleStringReader : TextReader {
        public SimpleStringReader() { }
        public SimpleStringReader(string s) {
            SetString(s);
        }
        private string _s;
        private int _length;
        private int _pos;
        public void SetString(string s) {
            _s = s;
            _length = s == null ? 0 : s.Length;
            _pos = 0;
        }
        public override int Read(char[] buffer, int index, int count) {
            var num = _length - _pos;
            if (num > 0) {
                if (num > count) {
                    num = count;
                }
                _s.CopyTo(_pos, buffer, index, num);
                _pos += num;
            }
            return num;
        }
        public override int Peek() {
            throw new NotImplementedException();
        }
        public override int Read() {
            throw new NotImplementedException();
        }
        public override System.Threading.Tasks.Task<int> ReadAsync(char[] buffer, int index, int count) {
            throw new NotImplementedException();
        }
        public override int ReadBlock(char[] buffer, int index, int count) {
            throw new NotImplementedException();
        }
        public override System.Threading.Tasks.Task<int> ReadBlockAsync(char[] buffer, int index, int count) {
            throw new NotImplementedException();
        }
        public override string ReadLine() {
            throw new NotImplementedException();
        }
        public override System.Threading.Tasks.Task<string> ReadLineAsync() {
            throw new NotImplementedException();
        }
        public override string ReadToEnd() {
            throw new NotImplementedException();
        }
        public override System.Threading.Tasks.Task<string> ReadToEndAsync() {
            throw new NotImplementedException();
        }
    }
}
