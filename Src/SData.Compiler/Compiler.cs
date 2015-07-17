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
                if (csFileList.Count > 0) {
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
                    if (CSEX.MapNamespaces(nsInfoMap, compilationAssSymbol, false) > 0) {
                        foreach (var nsInfo in nsInfoMap.Values) {
                            if (nsInfo.DottedName == null) {
                                CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.SchemaNamespaceAttributeRequired, nsInfo.Uri), default(TextSpan));
                            }
                        }
                        CSEX.MapClasses(nsInfoMap, compilationAssSymbol.GlobalNamespace);
                        foreach (var nsInfo in nsInfoMap.Values) {
                            nsInfo.SetGlobalTypeDottedNames();
                        }
                        foreach (var nsInfo in nsInfoMap.Values) {
                            nsInfo.MapGlobalTypeMembers();
                        }
                        var cuCompierAttList = new List<AttributeListSyntax>();
                        var cuMemberSyntaxList = new List<MemberDeclarationSyntax>();
                        var globalTypeMdSyntaxList = new List<ExpressionSyntax>();
                        var userAssemblyMetadataName = CSEX.UserAssemblyMetadataName(assemblyName);
                        var assMdExpr = CS.MemberAccessExpr(CS.GlobalAliasQualifiedName(userAssemblyMetadataName), "Instance");
                        System.Text.StringBuilder sb = null;
                        foreach (var logicalNs in nsInfoMap.Values) {
                            var nsInfo = logicalNs;
                            string uri, csns;
                            var mdns = nsInfo.GetMd(out uri, out csns);
                            if (mdns != null) {
                                if (sb == null) {
                                    sb = new System.Text.StringBuilder(1024 * 2);
                                }
                                else {
                                    sb.Clear();
                                }
                                //mdns.Save(sb);
                                var data = sb.ToString();
                                cuCompierAttList.Add(CS.AttributeList("assembly", CSEX.__CompilerSchemaNamespaceAttributeName,
                                    SyntaxFactory.AttributeArgument(CS.Literal(uri)),
                                    SyntaxFactory.AttributeArgument(CS.Literal(csns)),
                                    SyntaxFactory.AttributeArgument(CS.Literal(data))));
                            }
                            nsInfo.GetSyntax(cuMemberSyntaxList, assMdExpr, globalTypeMdSyntaxList);
                        }
                        if (globalTypeMdSyntaxList.Count > 0) {
                            //>public sealed class AssemblyMetadata_XX : AssemblyMetadata {
                            //>  public static readonly AssemblyMetadata Instance = new AssemblyMetadata_XX(new GlobalTypeMetadata[]{ ... });
                            //>  private AssemblyMetadata_XX(GlobalTypeMetadata[] globalTypes):base(globalTypes) { }
                            //>}
                            cuMemberSyntaxList.Add(CS.Class(null, CS.PublicSealedTokenList, userAssemblyMetadataName, new[] { CSEX.AssemblyMdName },
                                CS.Field(CS.PublicStaticReadOnlyTokenList, CSEX.AssemblyMdName, "Instance",
                                    CS.NewObjExpr(CS.IdName(userAssemblyMetadataName), CS.NewArrExpr(CSEX.GlobalTypeMdArrayType, globalTypeMdSyntaxList))),
                                CS.Constructor(CS.PrivateTokenList, userAssemblyMetadataName,
                                    new[] { CS.Parameter(CSEX.GlobalTypeMdArrayType, "globalTypes") },
                                    CS.ConstructorInitializer(true, CS.IdName("globalTypes")))
                                ));
                        }
                        code = GeneratedFileBanner +
                            SyntaxFactory.CompilationUnit(default(SyntaxList<ExternAliasDirectiveSyntax>), default(SyntaxList<UsingDirectiveSyntax>),
                                SyntaxFactory.List(cuCompierAttList), SyntaxFactory.List(cuMemberSyntaxList)).NormalizeWhitespace().ToString();
                    }
                }
                return true;
            }
            catch (LoadingException) { }
            catch (Exception ex) {
                context.AddDiagnostic(DiagnosticSeverity.Error, (int)DiagCodeEx.InternalCompilerError, "Internal compiler error: " + ex.ToString(), default(TextSpan));
            }
            return false;
        }
        //private static bool CompileCore(CompilerContext context, List<CompilationUnitNode> cuList,
        //    List<string> csFileList, List<string> csPpList, List<MetadataReference> csRefList, string assemblyName, ref string code) {
        //    try {
        //        CompilerContext.Current = context;
        //        var nsList = new List<NamespaceNode>();
        //        foreach (var cu in cuList) {
        //            nsList.AddRange(cu.NamespaceList);
        //        }
        //        if (nsList.Count == 0) {
        //            return true;
        //        }
        //        var nsMap = new LogicalNamespaceMap();
        //        foreach (var ns in nsList) {
        //            var uri = ns.UriValue;
        //            LogicalNamespace logicalNS;
        //            if (!nsMap.TryGetValue(uri, out logicalNS)) {
        //                logicalNS = new LogicalNamespace();
        //                nsMap.Add(uri, logicalNS);
        //            }
        //            logicalNS.NamespaceList.Add(ns);
        //            ns.LogicalNamespace = logicalNS;
        //        }
        //        foreach (var ns in nsList) {
        //            ns.ResolveImports(nsMap);
        //        }
        //        foreach (var logicalNs in nsMap.Values) {
        //            logicalNs.CheckDuplicateGlobalTypes();
        //        }
        //        foreach (var ns in nsList) {
        //            ns.Resolve();
        //        }
        //        foreach (var logicalNs in nsMap.Values) {
        //            logicalNs.NamespaceInfo = new NamespaceInfo(logicalNs.Uri);
        //        }
        //        foreach (var ns in nsList) {
        //            ns.CreateInfos();
        //        }
        //        //
        //        if (csFileList.Count > 0) {
        //            var parseOpts = new CSharpParseOptions(preprocessorSymbols: csPpList, documentationMode: DocumentationMode.None);
        //            var compilation = CSharpCompilation.Create(
        //                assemblyName: "__TEMP__",
        //                syntaxTrees: csFileList.Select(csFile => CSharpSyntaxTree.ParseText(text: File.ReadAllText(csFile), options: parseOpts, path: csFile)),
        //                references: csRefList,
        //                options: _compilationOptions);
        //            foreach (var csRef in csRefList) {
        //                if (csRef.Properties.Kind == MetadataImageKind.Assembly) {
        //                    var assSymbol = compilation.GetAssemblyOrModuleSymbol(csRef) as IAssemblySymbol;
        //                    if (assSymbol != null) {
        //                        CSEX.MapNamespaces(nsMap, assSymbol, true);
        //                    }
        //                }
        //            }
        //            var compilationAssSymbol = compilation.Assembly;
        //            if (CSEX.MapNamespaces(nsMap, compilationAssSymbol, false) > 0) {
        //                foreach (var logicalNs in nsMap.Values) {
        //                    if (logicalNs.DottedName == null) {
        //                        CompilerContext.ErrorAndThrow(new DiagMsgEx(DiagCodeEx.ContractNamespaceAttributeRequired, logicalNs.Uri), default(TextSpan));
        //                    }
        //                }
        //                CSEX.MapClasses(nsMap, compilationAssSymbol.GlobalNamespace);
        //                foreach (var logicalNs in nsMap.Values) {
        //                    logicalNs.NamespaceInfo.SetGlobalTypeDottedNames();
        //                }
        //                foreach (var logicalNs in nsMap.Values) {
        //                    logicalNs.NamespaceInfo.MapGlobalTypeMembers();
        //                }
        //                List<AttributeListSyntax> cuCompierAttList = new List<AttributeListSyntax>();
        //                List<MemberDeclarationSyntax> cuMemberSyntaxList = new List<MemberDeclarationSyntax>();
        //                List<ExpressionSyntax> globalTypeMdSyntaxList = new List<ExpressionSyntax>();
        //                var userAssemblyMetadataName = CSEX.UserAssemblyMetadataName(assemblyName);
        //                var assMdExpr = CS.MemberAccessExpr(CS.GlobalAliasQualifiedName(userAssemblyMetadataName), "Instance");
        //                System.Text.StringBuilder sb = null;
        //                foreach (var logicalNs in nsMap.Values) {
        //                    var nsInfo = logicalNs.NamespaceInfo;
        //                    string uri, csns;
        //                    var mdns = nsInfo.GetMdNamespace(out uri, out csns);
        //                    if (mdns != null) {
        //                        if (sb == null) {
        //                            sb = new System.Text.StringBuilder(1024 * 2);
        //                        }
        //                        else {
        //                            sb.Clear();
        //                        }
        //                        //mdns.Save(sb);
        //                        var data = sb.ToString();
        //                        cuCompierAttList.Add(CS.AttributeList("assembly", CSEX.__CompilerSchemaNamespaceAttributeName,
        //                            SyntaxFactory.AttributeArgument(CS.Literal(uri)),
        //                            SyntaxFactory.AttributeArgument(CS.Literal(csns)),
        //                            SyntaxFactory.AttributeArgument(CS.Literal(data))));
        //                    }
        //                    nsInfo.GetSyntax(cuMemberSyntaxList, assMdExpr, globalTypeMdSyntaxList);
        //                }
        //                if (globalTypeMdSyntaxList.Count > 0) {
        //                    //>public sealed class AssemblyMetadata_XX : AssemblyMetadata {
        //                    //>  public static readonly AssemblyMetadata Instance = new AssemblyMetadata_XX(new GlobalTypeMetadata[]{ ... });
        //                    //>  private AssemblyMetadata_XX(GlobalTypeMetadata[] globalTypes):base(globalTypes) { }
        //                    //>}
        //                    cuMemberSyntaxList.Add(CS.Class(null, CS.PublicSealedTokenList, userAssemblyMetadataName, new[] { CSEX.AssemblyMdName },
        //                        CS.Field(CS.PublicStaticReadOnlyTokenList, CSEX.AssemblyMdName, "Instance",
        //                            CS.NewObjExpr(CS.IdName(userAssemblyMetadataName), CS.NewArrExpr(CSEX.GlobalTypeMdArrayType, globalTypeMdSyntaxList))),
        //                        CS.Constructor(CS.PrivateTokenList, userAssemblyMetadataName,
        //                            new[] { CS.Parameter(CSEX.GlobalTypeMdArrayType, "globalTypes") },
        //                            CS.ConstructorInitializer(true, CS.IdName("globalTypes")))
        //                        ));
        //                }
        //                code = GeneratedFileBanner +
        //                    SyntaxFactory.CompilationUnit(default(SyntaxList<ExternAliasDirectiveSyntax>), default(SyntaxList<UsingDirectiveSyntax>),
        //                        SyntaxFactory.List(cuCompierAttList), SyntaxFactory.List(cuMemberSyntaxList)).NormalizeWhitespace().ToString();
        //            }
        //        }
        //        return true;
        //    }
        //    catch (LoadingException) { }
        //    return false;
        //}


    }
}
