namespace ILReader.Readers {
    using ILReader.Context;

    // The operand is an 8-bit integer.
    sealed class ShortInlineIOperandReader : IOperandReader {
        object IOperandReader.Read(IBinaryReader reader, IOperandReaderContext context) {
            return reader.ReadByte();
        }
    }
}