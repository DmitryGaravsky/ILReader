namespace ILReader.Readers {
    using ILReader.Context;

    // The operand is a 32-bit integer.
    sealed class InlineIOperandReader : IOperandReader {
        object IOperandReader.Read(IBinaryReader reader, IOperandReaderContext context) {
            return reader.ReadInt();
        }
    }
}