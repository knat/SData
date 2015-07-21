using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SData.Internal;

namespace SData.Compiler {
    public static class CDataCompiler {
        private const string GeneratedFileBanner = @"//
//Auto-generated, DO NOT EDIT.
//Visit https://github.com/knat/SData for more information.
//

";
        private static readonly CSharpCompilationOptions _compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);

        public static bool Compile(List<string> schemaFileList,
            List<string> csFileList, List<string> csPpList, List<MetadataReference> csRefList, string assemblyName,
            out LoadingContext context, out string code) {
            if (schemaFileList == null) throw new ArgumentNullException("schemaFileList");
            if (csFileList == null) throw new ArgumentNullException("csFileList");
            if (csPpList == null) throw new ArgumentNullException("csPpList");
            if (csRefList == null) throw new ArgumentNullException("csRefList");
            if (string.IsNullOrEmpty(assemblyName)) throw new ArgumentNullException("assemblyName");
            //
            context = null;
            code = GeneratedFileBanner;
            if (schemaFileList.Count == 0) {
                return true;
            }
            try {
                context = CompilerContext.Current = new CompilerContext();
                var cuList = new List<CompilationUnitNode>();
                foreach (var filePath in schemaFileList) {
                    using (var reader = new StreamReader(filePath)) {
                        CompilationUnitNode cuNode;
                        if (Parser.Parse(filePath, reader, context, out cuNode)) {
                            cuList.Add(cuNode);
                        }
                        else {
                            return false;
                        }
                    }
                }
                var nsList = new List<NamespaceNode>();
                foreach (var cu in cuList) {
                    nsList.AddRange(cu.NamespaceList);
                }
                if (nsList.Count == 0) {
                    return true;
                }
                var nsInfoMap = new NamespaceInfoMap();
                foreach (var ns in nsList) {
                    var uri = ns.UriValue;
                    NamespaceInfo nsInfo;
                    if (!nsInfoMap.TryGetValue(uri, out nsInfo)) {
                        nsInfo = new NamespaceInfo(uri);
                        nsInfoMap.Add(uri, nsInfo);
                    }
                    nsInfo.NamespaceNodeList.Add(ns);
                    ns.NamespaceInfo = nsInfo;
                }
                foreach (var ns in nsList) {
                    ns.ResolveImports(nsInfoMap);
                }
                foreach (var nsInfo in nsInfoMap.Values) {
                    nsInfo.CheckDuplicateGlobalTypeNodes();
                }
                foreach (var ns in nsList) {
                    ns.Resolve();
                }
                foreach (var ns in nsList) {
                    ns.CreateInfos();
                }
                //
                if (csFileList.Count == 0) {
                    return true;
                }
                var parseOpts = new CSharpParseOptions(preprocessorSymbols: csPpList, documentationMode: DocumentationMode.None);
                var compilation = CSharpCompilation.Create(
                    assemblyName: "__TEMP__",
                    syntaxTrees: csFileList.Select(csFile => CSharpSyntaxTree.ParseText(text: File.ReadAllText(csFile), options: parseOpts, path: csFile)),
                    references: csRefList,
                    options: _compilationOptions);
                foreach (var csRef in csRefList) {
                    if (csRef.Properties.Kind == MetadataImageKind.Assembly) {
                        var assSymbol = compilation.GetAssemblyOrModuleSymbol(csRef) as IAssemblySymbol;
                        if (assSymbol != null) {
                            CSEX.MapNamespaces(nsInfoMap, assSymbol, true);
                        }
                    }
                }
                var compilationAssSymbol = compilation.Assembly;
                if (CSEX.MapNamespaces(nsInfoMap, compilationAssSymbol, false) == 0) {
                    return true;
                }
                foreach (var nsInfo in nsInfoMap.Values) {
                    if (nsInfo.DottedName == null) {
                        CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.SchemaNamespaceAttributeRequired, nsInfo.Uri), default(TextSpan));
                    }
                }
                CSEX.MapGlobalTypes(nsInfoMap, compilationAssSymbol.GlobalNamespace);
                foreach (var nsInfo in nsInfoMap.Values) {
                    nsInfo.SetGlobalTypeDottedNames();
                }
                foreach (var nsInfo in nsInfoMap.Values) {
                    nsInfo.MapGlobalTypeMembers();
                }
                var cuAttListSyntaxList = new List<AttributeListSyntax>();
                var cuMemberSyntaxList = new List<MemberDeclarationSyntax>();
                var globalTypeMdRefSyntaxList = new List<ExpressionSyntax>();
                foreach (var nsInfo in nsInfoMap.Values) {
                    if (!nsInfo.IsRef) {
                        List<string> dottedPropertyNames;
                        var dottedTypeNames = nsInfo.GetRefData(out dottedPropertyNames);
                        cuAttListSyntaxList.Add(CS.AttributeList("assembly", CSEX.__CompilerSchemaNamespaceAttributeName,
                            SyntaxFactory.AttributeArgument(CS.Literal(nsInfo.Uri)),
                            SyntaxFactory.AttributeArgument(CS.Literal(nsInfo.DottedName.ToString())),
                            SyntaxFactory.AttributeArgument(CS.NewArrOrNullExpr(CS.StringArrayType, dottedTypeNames.Select(i => CS.Literal(i)))),
                            SyntaxFactory.AttributeArgument(CS.NewArrOrNullExpr(CS.StringArrayType, dottedPropertyNames.Select(i => CS.Literal(i))))
                            ));
                        nsInfo.GetSyntax(cuMemberSyntaxList, globalTypeMdRefSyntaxList);
                    }
                }
                var sdataProgramName = CSEX.SDataProgramName(assemblyName);
                //>public sealed class SData_XX : ProgramMd {
                //>  public static void Initialize() { }
                //>  private static readonly ProgramMd Instance = new SData_XX();
                //>  private SData_XX() : base(new GlobalTypeMd[]{ ... }) { }
                //>}
                cuMemberSyntaxList.Add(CS.Class(null, CS.PublicSealedTokenList, sdataProgramName, new[] { CSEX.ProgramMdName },
                    CS.Method(CS.PublicStaticTokenList, CS.VoidType, "Initialize", null),
                    CS.Field(CS.PrivateStaticReadOnlyTokenList, CSEX.ProgramMdName, "Instance",
                        CS.NewObjExpr(CS.IdName(sdataProgramName))),
                    CS.Constructor(CS.PrivateTokenList, sdataProgramName, null,
                        CS.ConstructorInitializer(true, CS.NewArrOrNullExpr(CSEX.GlobalTypeMdArrayType, globalTypeMdRefSyntaxList)))
                    ));
                code = GeneratedFileBanner +
                    SyntaxFactory.CompilationUnit(default(SyntaxList<ExternAliasDirectiveSyntax>), default(SyntaxList<UsingDirectiveSyntax>),
                        SyntaxFactory.List(cuAttListSyntaxList), SyntaxFactory.List(cuMemberSyntaxList)).NormalizeWhitespace().ToString();

                return true;
            }
            catch (LoadingException) { }
            catch (Exception ex) {
                context.AddDiagnostic(DiagnosticSeverity.Error, (int)DiagCodeEx.InternalCompilerError, "Internal compiler error: " + ex.ToString(), default(TextSpan));
            }
            return false;
        }

    }
}
