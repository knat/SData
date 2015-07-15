using System;
using System.Collections.Generic;
using System.Text;

namespace SData.Internal {

    public sealed class SavingContext : IndentedStringBuilder {
        public SavingContext(StringBuilder stringBuilder, string indentString, string newLineString)
            : base(stringBuilder, indentString, newLineString) {
            _aliasUriList = new List<AliasUri>();
        }
        private readonly List<AliasUri> _aliasUriList;
        private string AddUri(string uri) {
            var list = _aliasUriList;
            foreach (var au in list) {
                if (au.Uri == uri) {
                    return au.Alias;
                }
            }
            var alias = "a" + list.Count.ToInvString();
            list.Add(new AliasUri(alias, uri));
            return alias;
        }
        public void AppendFullName(FullName fullName) {
            Append(AddUri(fullName.Uri));
            var sb = StringBuilder;
            sb.Append("::");
            sb.Append(fullName.Name);
        }
        public void AppendTypeIndicator(FullName fullName) {
            Append('(');
            AppendFullName(fullName);
            StringBuilder.Append(") ");
        }

        public void InsertAliasUriList() {
            var list = _aliasUriList;
            var cnt = list.Count;
            if (cnt > 0) {
                var sb = StringBuilderBuffer.Acquire();
                sb.Append('<');
                for (var i = 0; i < cnt; ++i) {
                    if (i > 0) {
                        sb.Append(", ");
                    }
                    var au = list[i];
                    sb.Append(au.Alias);
                    sb.Append(" = ");
                    sb.Append(au.Uri.ToLiteral());
                }
                sb.Append("> ");
                StringBuilder.Insert(StartIndex, sb.ToStringAndRelease());
            }
        }

    }

}
