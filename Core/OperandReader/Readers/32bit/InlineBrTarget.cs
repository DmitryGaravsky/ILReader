namespace ILReader.Readers {
    // The operand is a 32-bit integer branch target.
    sealed class InlineBrTargetOperandReader : OperandReader {
        public sealed override object Read(ILBytesReader reader, Context.IOperandReaderContext context) {
            return reader.ReadInt();
        }
    }
}