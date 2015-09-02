namespace ILReader.Readers {
    // The operand is a 64-bit integer.
    sealed class InlineI8OperandReader : IOperandReader {
        object IOperandReader.Read(IBinaryReader reader, Context.IOperandReaderContext context) {
            return reader.ReadLong();
        }
    }
}