namespace ILReader.Readers {
    // The operand is a 64-bit IEEE floating point number.
    sealed class InlineROperandReader : OperandReader {
        public sealed override object Read(ILBytesReader reader, Context.IOperandReaderContext context) {
            return reader.ReadDouble();
        }
    }
}