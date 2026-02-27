namespace ILReader.Readers {
    // The operand is an 8-bit integer branch target.
    sealed class ShortInlineBrTargetOperandReader : OperandReader {
        public sealed override object Read(ILBytesReader reader, Context.IOperandReaderContext context) {
            return reader.ReadSByte();
        }
    }
}