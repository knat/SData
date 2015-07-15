
namespace SData.Internal {
    public struct AliasUri {
        public AliasUri(string alias, string uri) {
            Alias = alias;
            Uri = uri;
        }
        public readonly string Alias;
        public readonly string Uri;
    }

}
