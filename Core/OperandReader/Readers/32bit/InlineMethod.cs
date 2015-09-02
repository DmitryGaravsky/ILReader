namespace ILReader.Readers {
    // The operand is a 32-bit metadata token.
    sealed class InlineMethodOperandReader : IOperandReader {
        object IOperandReader.Read(IBinaryReader reader, Context.IOperandReaderContext context) {
            int methodToken = reader.ReadInt();
            return context.Module.ResolveMethod(methodToken);
        }
    }
}