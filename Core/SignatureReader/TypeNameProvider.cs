namespace ILReader.Readers {
    using System.Collections.Immutable;
    using System.Reflection.Metadata;
    // Decodes SRM binary signature blobs into fully-qualified type-name strings.
    // All primitive types use the "System.XXX" full name format (matches Type.FullName).
    // Generic instantiations use "Generic`1[Arg]" format (matches Type.FullName).
    internal struct Unit { }
    internal sealed class TypeNameProvider : ISignatureTypeProvider<string, Unit> {
        public static readonly TypeNameProvider Instance = new TypeNameProvider();
        TypeNameProvider() { }
        public string GetPrimitiveType(PrimitiveTypeCode typeCode) => typeCode switch {
            PrimitiveTypeCode.Void => "System.Void",
            PrimitiveTypeCode.Boolean => "System.Boolean",
            PrimitiveTypeCode.Char => "System.Char",
            PrimitiveTypeCode.SByte => "System.SByte",
            PrimitiveTypeCode.Byte => "System.Byte",
            PrimitiveTypeCode.Int16 => "System.Int16",
            PrimitiveTypeCode.UInt16 => "System.UInt16",
            PrimitiveTypeCode.Int32 => "System.Int32",
            PrimitiveTypeCode.UInt32 => "System.UInt32",
            PrimitiveTypeCode.Int64 => "System.Int64",
            PrimitiveTypeCode.UInt64 => "System.UInt64",
            PrimitiveTypeCode.Single => "System.Single",
            PrimitiveTypeCode.Double => "System.Double",
            PrimitiveTypeCode.String => "System.String",
            PrimitiveTypeCode.Object => "System.Object",
            PrimitiveTypeCode.IntPtr => "System.IntPtr",
            PrimitiveTypeCode.UIntPtr => "System.UIntPtr",
            PrimitiveTypeCode.TypedReference => "System.TypedReference",
            _ => typeCode.ToString()
        };
        public string GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte rawTypeKind) {
            var typeRef = reader.GetTypeReference(handle);
            string ns = reader.GetString(typeRef.Namespace);
            string name = reader.GetString(typeRef.Name);
            return string.IsNullOrEmpty(ns) ? name : ns + "." + name;
        }
        public string GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, byte rawTypeKind) {
            var typeDef = reader.GetTypeDefinition(handle);
            string ns = reader.GetString(typeDef.Namespace);
            string name = reader.GetString(typeDef.Name);
            return string.IsNullOrEmpty(ns) ? name : ns + "." + name;
        }
        public string GetTypeFromSpecification(MetadataReader reader, Unit genericContext,
            TypeSpecificationHandle handle, byte rawTypeKind) =>
            reader.GetTypeSpecification(handle).DecodeSignature(this, genericContext);
        // "Generic`1[System.String]" — matches Type.FullName format
        public string GetGenericInstantiation(string genericType, ImmutableArray<string> typeArguments) =>
            genericType + "[" + string.Join(", ", typeArguments) + "]";
        public string GetSZArrayType(string elementType) => elementType + "[]";
        public string GetArrayType(string elementType, ArrayShape shape) =>
            shape.Rank == 1 ? elementType + "[]" : elementType + "[" + new string(',', shape.Rank - 1) + "]";
        public string GetByReferenceType(string elementType) => elementType + "&";
        public string GetPointerType(string elementType) => elementType + "*";
        public string GetPinnedType(string elementType) => elementType;
        public string GetModifiedType(string modifier, string unmodifiedType, bool isRequired) => unmodifiedType;
        public string GetFunctionPointerType(MethodSignature<string> signature) => "method*";
        public string GetGenericMethodParameter(Unit genericContext, int index) => "!!" + index;
        public string GetGenericTypeParameter(Unit genericContext, int index) => "!" + index;
    }
}