namespace ILReader.Readers {
    sealed class StringSymbol : IMetadataSymbol {
        readonly MetadataSymbolKind kind;
        readonly string value;
        public StringSymbol(MetadataSymbolKind kind, string value) {
            this.kind = kind;
            this.value = value;
        }
        public MetadataSymbolKind Kind => kind;
        public string Name => value;
        public string FullName => value;
        public string DeclaringType => null;
        public string AssemblyName => string.Empty;
        public TSource GetSource<TSource>() {
            if(value is TSource s)
                return s;
            throw new System.InvalidCastException($"Source is string, not {typeof(TSource).Name}");
        }
        public bool TryGetSource<TSource>(out TSource source) {
            if(value is TSource s) {
                source = s;
                return true;
            }
            source = default;
            return false;
        }
        public override string ToString() => value;
    }
}