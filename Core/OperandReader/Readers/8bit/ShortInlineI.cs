namespace ILReader.Readers {
    // The operand is an 8-bit integer.
    sealed class ShortInlineIOperandReader : IOperandReader {
        object IOperandReader.Read(IBinaryReader reader, Context.IOperandReaderContext context) {
            return reader.ReadByte();
        }
    }
}