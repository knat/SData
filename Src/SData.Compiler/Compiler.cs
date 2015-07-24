using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SData.Internal;

namespace SData.Compiler {
    public static class SDataCompiler {
        public static bool Compile(IReadOnlyList<string> schemaFileList,
            IReadOnlyList<string> csFileList, IReadOnlyList<string> csPpList, IReadOnlyList<MetadataReference> csRefList, string csAssemblyName,
            out LoadingContext context, out string csCode) {
            context = null;
            csCode = _csGeneratedFileBanner;
            if (schemaFileList == null || schemaFileList.Count == 0) {
                return true;
            }
            try {
                context = CompilerContext.Current = new CompilerContext();
                var cuList = new List<CompilationUnitNode>();
                foreach (var schemaFile in schemaFileList) {
                    using (var reader = new StreamReader(schemaFile)) {
                        CompilationUnitNode cuNode;
                        if (Parser.Parse(schemaFile, reader, context, out cuNode)) {
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
                if (csFileList == null || csFileList.Count == 0) {
                    return true;
                }
                var parseOpts = new CSharpParseOptions(preprocessorSymbols: csPpList, documentationMode: DocumentationMode.None);
                var compilation = CSharpCompilation.Create(
                    assemblyName: "__TEMP__",
                    syntaxTrees: csFileList.Select(csFile => CSharpSyntaxTree.ParseText(text: File.ReadAllText(csFile), options: parseOpts, path: csFile)),
                    references: csRefList,
                    options: _csCompilationOptions);
                if (csRefList != null) {
                    foreach (var csRef in csRefList) {
                        if (csRef.Properties.Kind == MetadataImageKind.Assembly) {
                            var assSymbol = compilation.GetAssemblyOrModuleSymbol(csRef) as IAssemblySymbol;
                            if (assSymbol != null) {
                                CSEX.MapNamespaces(nsInfoMap, assSymbol, true);
                            }
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
                var sdataProgramName = CSEX.SDataProgramName(csAssemblyName);
                //>public sealed class SData_XX : ProgramMd {
                //>  public static void Initialize() {
                //>
                //>  }
                //>  private static readonly ProgramMd Instance = new SData_XX();
                //>  private SData_XX() : base(new GlobalTypeMd[]{ ... }) { }
                //>}
                cuMemberSyntaxList.Add(CS.Class(null, CS.PublicSealedTokenList, sdataProgramName, new[] { CSEX.ProgramMdName },
                    CS.Method(CS.PublicStaticTokenList, CS.VoidType, "Initialize", null, new StatementSyntax[] {
                        CS.LocalDeclStm(CS.VarIdName, "instance", CS.IdName("Instance"))
                    }),
                    CS.Field(CS.PrivateStaticReadOnlyTokenList, CSEX.ProgramMdName, "Instance",
                        CS.NewObjExpr(CS.IdName(sdataProgramName))),
                    CS.Constructor(CS.PrivateTokenList, sdataProgramName, null,
                        CS.ConstructorInitializer(true, CS.NewArrOrNullExpr(CSEX.GlobalTypeMdArrayType, globalTypeMdRefSyntaxList)))
                    ));
                csCode = _csGeneratedFileBanner +
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

        private const string _csGeneratedFileBanner = @"//
//Auto-generated, DO NOT EDIT.
//Visit https://github.com/knat/SData for more information.
//

";
        private static readonly CSharpCompilationOptions _csCompilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);


    }
}
