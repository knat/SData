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
            var auList = _aliasUriList;
            foreach (var au in auList) {
                if (au.Uri == uri) {
                    return au.Alias;
                }
            }
            var alias = "a" + auList.Count.ToInvString();
            auList.Add(new AliasUri(alias, uri));
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
            var auList = _aliasUriList;
            var count = auList.Count;
            if (count > 0) {
                var sb = StringBuilderBuffer.Acquire();
                sb.Append('<');
                for (var i = 0; i < count; ++i) {
                    if (i > 0) {
                        sb.Append(", ");
                    }
                    var au = auList[i];
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
