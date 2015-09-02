namespace ILReader.Readers {
    // The operand is a 64-bit IEEE floating point number.
    sealed class InlineROperandReader : IOperandReader {
        object IOperandReader.Read(IBinaryReader reader, Context.IOperandReaderContext context) {
            return reader.ReadDouble();
        }
    }
}