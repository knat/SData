using System;
using System.Text;

namespace SData.Internal {
    public static class StringBuilderBuffer {
        private const int _count = 4;
        [ThreadStatic]
        private static StringBuilder[] _buf;
        private static StringBuilder[] Buf {
            get { return _buf ?? (_buf = new StringBuilder[_count]); }
        }
        public static StringBuilder Acquire() {
            var buf = Buf;
            StringBuilder sb = null;
            for (var i = 0; i < _count; ++i) {
                sb = buf[i];
                if (sb != null) {
                    buf[i] = null;
                    break;
                }
            }
            if (sb != null) {
                sb.Clear();
                return sb;
            }
            return new StringBuilder(128);
        }
        public static void Release(StringBuilder sb) {
            if (sb != null && sb.Capacity <= 1024 * 8) {
                var buf = Buf;
                for (var i = 0; i < _count; ++i) {
                    if (buf[i] == null) {
                        buf[i] = sb;
                        return;
                    }
                }
            }
        }
        public static string ToStringAndRelease(this StringBuilder sb) {
            var str = sb.ToString();
            Release(sb);
            return str;
        }

    }
}
