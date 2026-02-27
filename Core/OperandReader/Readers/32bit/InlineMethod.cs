namespace ILReader.Readers {
    // The operand is a 32-bit metadata token.
    sealed class InlineMethodOperandReader : OperandReader {
        public sealed override object Read(ILBytesReader reader, Context.IOperandReaderContext context) {
            return context.ResolveMethod(reader.ReadInt());
        }
    }
}