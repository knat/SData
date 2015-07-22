using System.Collections.Generic;

namespace SData {
    public class LoadingContext {
        public LoadingContext() {
            DiagnosticList = new List<Diagnostic>();
        }
        public readonly List<Diagnostic> DiagnosticList;
        public virtual void Reset() {
            DiagnosticList.Clear();
        }
        public void AddDiagnostic(DiagnosticSeverity severity, int code, string message, TextSpan textSpan) {
            DiagnosticList.Add(new Diagnostic(severity, code, message, textSpan));
        }
        public bool HasDiagnostics {
            get {
                return DiagnosticList.Count > 0;
            }
        }
        public bool HasErrorDiagnostics {
            get {
                return HasErrorDiagnosticsCore(0);
            }
        }
        private bool HasErrorDiagnosticsCore(int startIndex) {
            var list = DiagnosticList;
            var count = list.Count;
            for (; startIndex < count; ++startIndex) {
                if (list[startIndex].IsError) {
                    return true;
                }
            }
            return false;
        }
        //public struct Marker {
        //    internal Marker(LoadingContext context) {
        //        Context = context;
        //        StartIndex = context.DiagnosticList.Count;
        //    }
        //    public readonly LoadingContext Context;
        //    public readonly int StartIndex;
        //    public int Count {
        //        get {
        //            return Context.DiagnosticList.Count - StartIndex;
        //        }
        //    }
        //    public bool HasErrorDiagnostics {
        //        get {
        //            return Context.HasErrorDiagnosticsCore(StartIndex);
        //        }
        //    }
        //    public void Restore() {
        //        Context.DiagnosticList.RemoveRange(StartIndex, Context.DiagnosticList.Count - StartIndex);
        //    }
        //}
        //public Marker Mark() {
        //    return new Marker(this);
        //}

    }


}
