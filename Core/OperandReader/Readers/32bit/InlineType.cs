namespace ILReader.Readers {
    // The operand is a 32-bit metadata token.
    sealed class InlineTypeOperandReader : OperandReader {
        public sealed override object Read(ILBytesReader reader, Context.IOperandReaderContext context) {
            return context.ResolveType(reader.ReadInt());
        }
    }
}