namespace ILReader.Readers {
    using ILReader.Context;
    
    // The operand is a 32-bit integer branch target.
    sealed class InlineBrTargetOperandReader : IOperandReader {
        object IOperandReader.Read(IBinaryReader reader, IOperandReaderContext context) {
            return reader.ReadInt();
        }
    }
}