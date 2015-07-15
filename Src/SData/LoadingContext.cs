using System;
using System.Collections.Generic;

namespace SData {
    public class LoadingContext {
        public LoadingContext() {
            DiagList = new List<Diag>();
            //_tokenList = new List<Token>();
        }
        public readonly List<Diag> DiagList;
        //private readonly List<Token> _tokenList;
        public virtual void Reset() {
            DiagList.Clear();
            //_tokenList.Clear();
        }

        public void AddDiag(DiagSeverity severity, int code, string message, TextSpan textSpan) {
            DiagList.Add(new Diag(severity, code, message, textSpan));
        }
        public void AddDiag(DiagSeverity severity, DiagMsg diagMsg, TextSpan textSpan) {
            DiagList.Add(new Diag(severity, diagMsg, textSpan));
        }
        public bool HasDiags {
            get {
                return DiagList.Count > 0;
            }
        }
        public bool HasErrorDiags {
            get {
                return HasErrorDiagsCore(0);
            }
        }
        private bool HasErrorDiagsCore(int startIndex) {
            var list = DiagList;
            var count = list.Count;
            for (; startIndex < count; ++startIndex) {
                if (list[startIndex].IsError) {
                    return true;
                }
            }
            return false;
        }
        public struct Marker {
            internal Marker(LoadingContext context) {
                Context = context;
                StartIndex = context.DiagList.Count;
            }
            internal readonly LoadingContext Context;
            public readonly int StartIndex;
            public int Count {
                get {
                    return Context.DiagList.Count - StartIndex;
                }
            }
            public bool HasErrorDiags {
                get {
                    return Context.HasErrorDiagsCore(StartIndex);
                }
            }
            public void Restore() {
                Context.DiagList.RemoveRange(StartIndex, Context.DiagList.Count - StartIndex);
            }
        }
        public Marker Mark() {
            return new Marker(this);
        }
        //
        //internal string AddToken(Token token) {
        //    var list = _tokenList;
        //    var idxStr = list.Count.ToInvString();
        //    list.Add(token);
        //    return idxStr;
        //}

    }


}
