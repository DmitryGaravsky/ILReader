namespace ILReader.Readers {
    // The operand is an 8-bit integer branch target.
    sealed class ShortInlineBrTargetOperandReader : IOperandReader {
        object IOperandReader.Read(IBinaryReader reader, Context.IOperandReaderContext context) {
            return reader.ReadByte();
        }
    }
}