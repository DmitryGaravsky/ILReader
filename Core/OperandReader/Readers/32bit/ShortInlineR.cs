namespace ILReader.Readers {
    // The operand is a 32-bit IEEE floating point number.
    sealed class ShortInlineROperandReader : IOperandReader {
        object IOperandReader.Read(IBinaryReader reader, Context.IOperandReaderContext context) {
            return reader.ReadFloat();
        }
    }
}