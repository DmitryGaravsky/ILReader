namespace ILReader.Readers {
    using ILReader.Context;

    // The operand is a 32-bit IEEE floating point number.
    sealed class ShortInlineROperandReader : IOperandReader {
        object IOperandReader.Read(IBinaryReader reader, IOperandReaderContext context) {
            return reader.ReadFloat();
        }
    }
}