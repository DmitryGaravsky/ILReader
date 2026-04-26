namespace ILReader.Readers {
    public enum MetadataSymbolKind {
        Method,
        Field,
        Type,
        Member,
        Signature,
        String
    }
    public interface IMetadataSymbol {
        MetadataSymbolKind Kind { get; }
        string Name { get; }
        string FullName { get; }
        string DeclaringType { get; }
        string AssemblyName { get; }
        TSource GetSource<TSource>();
        bool TryGetSource<TSource>(out TSource source);
    }
    public interface IMethodSymbol : IMetadataSymbol {
        string ReturnTypeName { get; }
        ParameterSymbol[] Parameters { get; }
        bool IsDeclaringTypeNested { get; }
        string[] EnclosingTypeNames { get; }
    }
    public interface IFieldSymbol : IMetadataSymbol {
        string FieldTypeName { get; }
    }
    public interface ITypeSymbol : IMetadataSymbol {
        bool IsNested { get; }
        string[] EnclosingTypeNames { get; }
    }
    public readonly record struct ParameterSymbol(
        int Index, string Name, string TypeName);
    public readonly record struct LocalVariableSymbol(
        int Index, string TypeName);
}