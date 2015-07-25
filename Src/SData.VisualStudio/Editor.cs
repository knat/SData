using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Language.StandardClassification;
using SData.Compiler;
using SData.MSBuild;

namespace SData.VisualStudio.Editors
{
    internal static class ContentTypeDefinitions
    {
        //
        internal const string SDataSchemaContentType = "SDataSchema";
        internal const string SDataSchemaFileExtension = ".sds";
        [Export, BaseDefinition("code"), Name(SDataSchemaContentType)]
        internal static ContentTypeDefinition SDataSchemaContentTypeDefinition = null;
        [Export, ContentType(SDataSchemaContentType), FileExtension(SDataSchemaFileExtension)]
        internal static FileExtensionToContentTypeDefinition SDataSchemaFileExtensionDefinition = null;
    }

    [Export(typeof(IClassifierProvider)),
        ContentType(ContentTypeDefinitions.SDataSchemaContentType)]
    internal sealed class LanguageClassifierProvider : IClassifierProvider
    {
        [Import]
        internal IStandardClassificationService StandardService = null;
        public IClassifier GetClassifier(ITextBuffer textBuffer)
        {
            return textBuffer.Properties.GetOrCreateSingletonProperty<LanguageClassifier>(
                () => new LanguageClassifier(textBuffer, StandardService));
        }
    }
    internal sealed class LanguageClassifier : LanguageClassifierBase
    {
        internal LanguageClassifier(ITextBuffer textBuffer, IStandardClassificationService standardService)
            : base(textBuffer, standardService, ParserConstants.KeywordSet)
        {
        }
    }
    //
    //
    [Export(typeof(ITaggerProvider)), TagType(typeof(IErrorTag)),
        ContentType(ContentTypeDefinitions.SDataSchemaContentType)]
    internal sealed class LanguageErrorTaggerProvider : LanguageErrorTaggerProviderBase
    {
        internal LanguageErrorTaggerProvider()
            : base(DiagStore.FileName, DiagStore.TryLoad)
        {
        }
    }

}
