namespace ILReader.Readers {
    // The operand is a 32-bit metadata signature token.
    sealed class InlineSigOperandReader : IOperandReader {
        object IOperandReader.Read(IBinaryReader reader, Context.IOperandReaderContext context) {
            return context.ResolveSignature(reader.ReadInt());
        }
    }
}