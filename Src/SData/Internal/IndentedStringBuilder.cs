using System;
using System.Text;

namespace SData.Internal
{
    public class IndentedStringBuilder
    {
        public IndentedStringBuilder(StringBuilder stringBuilder, string indentString, string newLineString)
        {
            if (stringBuilder == null) throw new ArgumentNullException("stringBuilder");
            if (string.IsNullOrEmpty(indentString)) throw new ArgumentNullException("indentString");
            if (string.IsNullOrEmpty(newLineString)) throw new ArgumentNullException("newLineString");
            StringBuilder = stringBuilder;
            StartIndex = stringBuilder.Length;
            IndentString = indentString;
            NewLineString = newLineString;
            _atNewLine = true;
        }
        public readonly StringBuilder StringBuilder;
        public readonly int StartIndex;
        public readonly string IndentString;
        public readonly string NewLineString;
        private int _indentCount;
        private bool _atNewLine;
        public int IndentCount
        {
            get
            {
                return _indentCount;
            }
        }
        public bool AtNewLine
        {
            get
            {
                return _atNewLine;
            }
        }
        public void PushIndent(int count = 1)
        {
            if ((_indentCount += count) < 0) throw new ArgumentOutOfRangeException("count");
        }
        public void PopIndent(int count = 1)
        {
            if ((_indentCount -= count) < 0) throw new ArgumentOutOfRangeException("count");
        }
        public void AppendIndents()
        {
            if (_atNewLine)
            {
                var count = _indentCount;
                var sb = StringBuilder;
                var s = IndentString;
                for (var i = 0; i < count; ++i)
                {
                    sb.Append(s);
                }
                _atNewLine = false;
            }
        }
        public void Append(string s)
        {
            AppendIndents();
            StringBuilder.Append(s);
        }
        public void Append(char ch)
        {
            AppendIndents();
            StringBuilder.Append(ch);
        }
        public void AppendLine()
        {
            StringBuilder.Append(NewLineString);
            _atNewLine = true;
        }
        //public void AppendLine(string s) {
        //    Append(s);
        //    AppendLine();
        //}
        //public void AppendLine(char ch) {
        //    Append(ch);
        //    AppendLine();
        //}
    }
}
