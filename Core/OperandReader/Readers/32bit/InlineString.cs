namespace ILReader.Readers {
    // The operand is a 32-bit metadata string token.
    sealed class InlineStringOperandReader : IOperandReader {
        object IOperandReader.Read(IBinaryReader reader, Context.IOperandReaderContext context) {
            int strToken = reader.ReadInt();
            return context.Module.ResolveString(strToken);
        }
    }
}