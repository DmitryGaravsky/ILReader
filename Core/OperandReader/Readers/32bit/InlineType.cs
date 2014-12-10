namespace ILReader.Readers {
    using ILReader.Context;

    // The operand is a 32-bit metadata token.
    sealed class InlineTypeOperandReader : IOperandReader {
        object IOperandReader.Read(IBinaryReader reader, IOperandReaderContext context) {
            var typeToken = reader.ReadInt();
            return context.Module.ResolveType(typeToken);
        }
    }
}