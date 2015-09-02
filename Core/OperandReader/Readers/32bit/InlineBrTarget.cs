namespace ILReader.Readers {
    // The operand is a 32-bit integer branch target.
    sealed class InlineBrTargetOperandReader : IOperandReader {
        object IOperandReader.Read(IBinaryReader reader, Context.IOperandReaderContext context) {
            return reader.ReadInt();
        }
    }
}