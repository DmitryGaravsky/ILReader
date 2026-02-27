namespace ILReader.Readers {
    // The operand is a 32-bit metadata signature token.
    sealed class InlineSigOperandReader : OperandReader {
        public sealed override object Read(ILBytesReader reader, Context.IOperandReaderContext context) {
            return context.ResolveSignature(reader.ReadInt());
        }
    }
}