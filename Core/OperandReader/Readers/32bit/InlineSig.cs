namespace ILReader.Readers {
    using ILReader.Context;

    // The operand is a 32-bit metadata signature token.
    sealed class InlineSigOperandReader : IOperandReader {
        object IOperandReader.Read(IBinaryReader reader, IOperandReaderContext context) {
            int sigToken = reader.ReadInt();
            return context.Module.ResolveSignature(sigToken);
        }
    }
}