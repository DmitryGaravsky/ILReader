namespace ILReader.Readers {
    using ILReader.Context;
    
    // The operand is a 32-bit metadata token.
    sealed class InlineFieldOperandReader : IOperandReader {
        object IOperandReader.Read(IBinaryReader reader, IOperandReaderContext context) {
            return reader.ReadInt();
        }
    }
}