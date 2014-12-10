namespace ILReader.Readers {
    using ILReader.Context;

    // The operand is a 32-bit metadata string token.
    sealed class InlineStringOperandReader : IOperandReader {
        object IOperandReader.Read(IBinaryReader reader, IOperandReaderContext context) {
            int strToken = reader.ReadInt();
            return context.Module.ResolveString(strToken);
        }
    }
}