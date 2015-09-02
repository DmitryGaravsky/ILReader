namespace ILReader.Readers {
    // The operand is a 32-bit integer.
    sealed class InlineIOperandReader : IOperandReader {
        object IOperandReader.Read(IBinaryReader reader, Context.IOperandReaderContext context) {
            return reader.ReadInt();
        }
    }
}