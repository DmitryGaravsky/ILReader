namespace ILReader.Readers {
    // The operand is a 32-bit metadata string token.
    sealed class InlineStringOperandReader : OperandReader {
        public sealed override object Read(ILBytesReader reader, Context.IOperandReaderContext context) {
            return context.ResolveString(reader.ReadInt());
        }
    }
}