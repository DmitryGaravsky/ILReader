namespace ILReader.Readers {
    // The operand is a 32-bit metadata token.
    sealed class InlineFieldOperandReader : IOperandReader {
        object IOperandReader.Read(IBinaryReader reader, Context.IOperandReaderContext context) {
            int fieldToken = reader.ReadInt();
            return context.Module.ResolveField(fieldToken);
        }
    }
}