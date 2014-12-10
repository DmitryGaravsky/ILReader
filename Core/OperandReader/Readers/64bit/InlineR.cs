namespace ILReader.Readers {
    using ILReader.Context;

    // The operand is a 64-bit IEEE floating point number.
    sealed class InlineROperandReader : IOperandReader {
        object IOperandReader.Read(IBinaryReader reader, IOperandReaderContext context) {
            return reader.ReadDouble();
        }
    }
}