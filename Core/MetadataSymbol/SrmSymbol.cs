namespace ILReader.Readers {
    using System;
    using System.Reflection.Metadata;
    //
    abstract class SrmSymbol : IMetadataSymbol {
        protected readonly EntityHandle handle;
        protected SrmSymbol(EntityHandle handle) { this.handle = handle; }
        public abstract MetadataSymbolKind Kind { get; }
        public abstract string Name { get; }
        public abstract string FullName { get; }
        public abstract string DeclaringType { get; }
        public abstract string AssemblyName { get; }
        public TSource GetSource<TSource>() {
            if(typeof(TSource) == typeof(EntityHandle))
                return (TSource)(object)handle;
            throw new InvalidCastException($"SrmSymbol only exposes EntityHandle, not {typeof(TSource).Name}");
        }
        public bool TryGetSource<TSource>(out TSource source) {
            if(typeof(TSource) == typeof(EntityHandle)) {
                source = (TSource)(object)handle;
                return true;
            }
            source = default;
            return false;
        }
    }
    sealed class SrmSymbol_Method : SrmSymbol, IMethodSymbol {
        public SrmSymbol_Method(
            string name, string fullName, string declaringType,
            string assemblyName, string returnTypeName, ParameterSymbol[] parameters,
            bool isNested, string[] enclosingTypeNames, EntityHandle handle)
            : base(handle) {
            Name = name;
            FullName = fullName;
            DeclaringType = declaringType;
            AssemblyName = assemblyName;
            ReturnTypeName = returnTypeName;
            Parameters = parameters;
            IsDeclaringTypeNested = isNested;
            EnclosingTypeNames = enclosingTypeNames;
        }
        public override MetadataSymbolKind Kind => MetadataSymbolKind.Method;
        public override string Name { get; }
        public override string FullName { get; }
        public override string DeclaringType { get; }
        public override string AssemblyName { get; }
        public string ReturnTypeName { get; }
        public ParameterSymbol[] Parameters { get; }
        public bool IsDeclaringTypeNested { get; }
        public string[] EnclosingTypeNames { get; }
        public override string ToString() {
            return (DeclaringType != null) ? DeclaringType + "." + Name : Name;
        }
    }
    sealed class SrmSymbol_Field : SrmSymbol, IFieldSymbol {
        public SrmSymbol_Field(string name, string fullName, string fieldTypeName,
            string declaringType, string assemblyName, EntityHandle handle)
            : base(handle) {
            Name = name;
            FullName = fullName;
            FieldTypeName = fieldTypeName;
            DeclaringType = declaringType;
            AssemblyName = assemblyName;
        }
        public override MetadataSymbolKind Kind => MetadataSymbolKind.Field;
        public override string Name { get; }
        public override string FullName { get; }
        public override string DeclaringType { get; }
        public override string AssemblyName { get; }
        public string FieldTypeName { get; }
        public override string ToString() {
            return (DeclaringType != null) ? DeclaringType + "." + Name : Name;
        }
    }
    sealed class SrmSymbol_Type : SrmSymbol, ITypeSymbol {
        public SrmSymbol_Type(string name, string fullName, string assemblyName,
            bool isNested, string[] enclosingTypeNames, EntityHandle handle)
            : base(handle) {
            Name = name;
            FullName = fullName;
            AssemblyName = assemblyName;
            IsNested = isNested;
            EnclosingTypeNames = enclosingTypeNames;
        }
        public override MetadataSymbolKind Kind => MetadataSymbolKind.Type;
        public override string Name { get; }
        public override string FullName { get; }
        public override string DeclaringType {
            get {
                if(!IsNested || EnclosingTypeNames.Length == 0)
                    return null;
                // EnclosingTypeNames is innermost-first; join in reverse for full nested name.
                var sb = new System.Text.StringBuilder();
                for(int i = EnclosingTypeNames.Length - 1; i >= 0; i--) {
                    if(sb.Length > 0) sb.Append('+');
                    sb.Append(EnclosingTypeNames[i]);
                }
                return sb.ToString();
            }
        }
        public override string AssemblyName { get; }
        public bool IsNested { get; }
        public string[] EnclosingTypeNames { get; }
        public override string ToString() => FullName;
    }
    // Used for InlineSig operand kind and unrecognised handles.
    sealed class SrmSymbol_Signature : SrmSymbol {
        public SrmSymbol_Signature(EntityHandle handle)
            : base(handle) { }
        public override MetadataSymbolKind Kind => MetadataSymbolKind.Signature;
        public override string Name => "<signature>";
        public override string FullName => "<signature>";
        public override string DeclaringType => null;
        public override string AssemblyName => string.Empty;
    }
}