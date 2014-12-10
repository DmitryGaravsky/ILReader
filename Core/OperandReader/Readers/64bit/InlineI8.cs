namespace ILReader.Readers {
    using ILReader.Context;

    // The operand is a 64-bit integer.
    sealed class InlineI8OperandReader : IOperandReader {
        object IOperandReader.Read(IBinaryReader reader, IOperandReaderContext context) {
            return reader.ReadLong();
        }
    }
}