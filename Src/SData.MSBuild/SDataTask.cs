using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System.Runtime.Serialization;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using SData.Compiler;

namespace SData.MSBuild {
    public sealed class SDataTask : Task {
        [Required]
        public string ProjectDirectory { get; set; }
        [Required]
        public ITaskItem[] SchemaFiles { get; set; }
        [Required]
        public ITaskItem[] CSFiles { get; set; }
        [Required]
        public string CSPpString { get; set; }
        [Required]
        public ITaskItem[] CSRefs { get; set; }
        [Required]
        public string CSAssemblyName { get; set; }
        //
        private const string _generatedCSFileName = "__SDataGenerated.cs";
        private static readonly char[] _csPpSeparators = new char[] { ';', ',' };
        private static readonly char[] _csAliasSeparators = new char[] { ',' };

        public override bool Execute() {
            try {
                var schemaFiles = SchemaFiles;
                if (schemaFiles.Length == 0) {
                    return true;
                }
                List<string> schemaFileList = new List<string>();
                List<string> csFileList = new List<string>();
                List<string> csPpList = new List<string>();
                List<MetadataReference> csRefList = new List<MetadataReference>();
                foreach (var item in schemaFiles) {
                    schemaFileList.Add(item.GetMetadata("FullPath"));
                }
                foreach (var item in CSFiles) {
                    var path = item.GetMetadata("FullPath");
                    if (!path.EndsWith(_generatedCSFileName, StringComparison.OrdinalIgnoreCase)) {
                        csFileList.Add(path);
                    }
                }
                foreach (var s in CSPpString.Split(_csPpSeparators, StringSplitOptions.RemoveEmptyEntries)) {
                    var s2 = s.Trim();
                    if (s2.Length > 0) {
                        csPpList.Add(s2);
                    }
                }
                foreach (var item in CSRefs) {
                    var path = item.ItemSpec;
                    var aliasesStr = item.GetMetadata("Aliases");
                    var aliasArray = default(ImmutableArray<string>);
                    if (!string.IsNullOrEmpty(aliasesStr)) {
                        var builder = ImmutableArray.CreateBuilder<string>();
                        foreach (var alias in aliasesStr.Split(_csAliasSeparators, StringSplitOptions.RemoveEmptyEntries)) {
                            var alias2 = alias.Trim();
                            if (alias2.Length > 0) {
                                builder.Add(alias2);
                            }
                        }
                        aliasArray = builder.ToImmutable();
                    }
                    var embedInteropTypes = string.Equals("True", item.GetMetadata("EmbedInteropTypes"), StringComparison.OrdinalIgnoreCase);
                    csRefList.Add(MetadataReference.CreateFromFile(path: path,
                        properties: new MetadataReferenceProperties(
                            kind: MetadataImageKind.Assembly,
                            aliases: aliasArray,
                            embedInteropTypes: embedInteropTypes
                        )));
                }
                //
                LoadingContext context;
                string csCode;
                var res = SDataCompiler.Compile(schemaFileList, csFileList, csPpList, csRefList, CSAssemblyName, out context, out csCode);
                var diagStore = new DiagStore();
                if (context != null) {
                    foreach (var diag in context.DiagnosticList) {
                        LogDiagnostic(diag, diagStore);
                    }
                }
                diagStore.Save(ProjectDirectory);
                if (csCode != null) {
                    File.WriteAllText(Path.Combine(ProjectDirectory, _generatedCSFileName), csCode, System.Text.Encoding.UTF8);
                }
                return res;
            }
            catch (Exception ex) {
                Log.LogErrorFromException(ex, true, true, null);
                return false;
            }
            //C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild.exe xxx.csproj
            //  v:detailed
        }
        private void LogDiagnostic(Diagnostic diag, DiagStore diagStore) {
            const string subCategory = "SData";
            var codeString = subCategory + diag.Code.ToString(System.Globalization.CultureInfo.InvariantCulture);
            string helpKeyword = null, filePath = null;
            int startLine = 0, startCol = 0, endLine = 0, endCol = 0;
            var textSpan = diag.TextSpan;
            if (textSpan.IsValid) {
                filePath = textSpan.FilePath;
                startLine = textSpan.StartPosition.Line;
                startCol = textSpan.StartPosition.Column;
                endLine = textSpan.EndPosition.Line;
                endCol = textSpan.EndPosition.Column;
            }
            var message = diag.Message;
            switch (diag.Severity) {
                case DiagnosticSeverity.Error:
                    Log.LogError(subCategory, codeString, helpKeyword, filePath, startLine, startCol, endLine, endCol, message);
                    break;
                case DiagnosticSeverity.Warning:
                    Log.LogWarning(subCategory, codeString, helpKeyword, filePath, startLine, startCol, endLine, endCol, message);
                    break;
                case DiagnosticSeverity.Info:
                    Log.LogMessage(subCategory, codeString, helpKeyword, filePath, startLine, startCol, endLine, endCol, MessageImportance.Normal, message);
                    break;
            }
            if (filePath != null && !filePath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)) {
                DiagUnit diagUnit;
                if (!diagStore.TryGetUnit(filePath, out diagUnit)) {
                    diagUnit = new DiagUnit(filePath, File.GetLastWriteTime(filePath));
                    diagStore.Add(diagUnit);
                }
                diagUnit.DiagList.Add(diag);
            }
        }
    }

    [DataContract(Namespace = Extensions.SystemUri)]
    public sealed class DiagUnit {
        internal DiagUnit(string filePath, DateTime lastWriteTime) {
            FilePath = filePath;
            LastWriteTime = lastWriteTime;
            DiagList = new List<Diagnostic>();
        }
        [DataMember]
        public readonly string FilePath;
        [DataMember]
        public readonly DateTime LastWriteTime;
        [DataMember]
        public readonly List<Diagnostic> DiagList;
    }

    [CollectionDataContract(Namespace = Extensions.SystemUri)]
    public sealed class DiagStore : List<DiagUnit> {
        public bool TryGetUnit(string filePath, out DiagUnit result) {
            foreach (var item in this) {
                if (item.FilePath == filePath) {
                    result = item;
                    return true;
                }
            }
            result = null;
            return false;
        }
        public DiagUnit TryGetUnit(string filePath, DateTime lastWriteTime) {
            foreach (var item in this) {
                if (item.FilePath == filePath) {
                    if (item.LastWriteTime == lastWriteTime) {
                        return item;
                    }
                    return null;
                }
            }
            return null;
        }
        public const string FileName = "SDataBuildDiags.xml";
        private static readonly DataContractSerializer _dcs = new DataContractSerializer(typeof(DiagStore));
        internal void Save(string projectDirectory) {
            var filePath = Path.Combine(projectDirectory, "obj", FileName);
            File.Delete(filePath);
            using (var fs = File.Create(filePath)) {
                _dcs.WriteObject(fs, this);
            }
        }
        public static DiagStore TryLoad(string filePath) {
            try {
                if (File.Exists(filePath)) {
                    using (var fs = File.OpenRead(filePath)) {
                        return (DiagStore)_dcs.ReadObject(fs);
                    }
                }
            }
            catch (Exception) { }
            return null;
        }

    }

}
