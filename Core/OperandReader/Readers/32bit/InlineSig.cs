namespace ILReader.Readers {
    // The operand is a 32-bit metadata signature token.
    sealed class InlineSigOperandReader : IOperandReader {
        object IOperandReader.Read(IBinaryReader reader, Context.IOperandReaderContext context) {
            int sigToken = reader.ReadInt();
            return context.Module.ResolveSignature(sigToken);
        }
    }
}