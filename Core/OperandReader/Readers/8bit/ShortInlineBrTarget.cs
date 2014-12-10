namespace ILReader.Readers {
    using ILReader.Context;

    // The operand is an 8-bit integer branch target.
    sealed class ShortInlineBrTargetOperandReader : IOperandReader {
        object IOperandReader.Read(IBinaryReader reader, IOperandReaderContext context) {
            return reader.ReadByte();
        }
    }
}