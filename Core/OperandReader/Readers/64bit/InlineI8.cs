namespace ILReader.Readers {
    // The operand is a 64-bit integer.
    sealed class InlineI8OperandReader : OperandReader {
        public sealed override object Read(ILBytesReader reader, Context.IOperandReaderContext context) {
            return reader.ReadLong();
        }
    }
}