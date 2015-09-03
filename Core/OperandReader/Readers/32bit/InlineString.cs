namespace ILReader.Readers {
    // The operand is a 32-bit metadata string token.
    sealed class InlineStringOperandReader : IOperandReader {
        object IOperandReader.Read(IBinaryReader reader, Context.IOperandReaderContext context) {
            return context.ResolveString(reader.ReadInt());
        }
    }
}