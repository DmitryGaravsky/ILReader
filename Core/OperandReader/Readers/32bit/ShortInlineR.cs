namespace ILReader.Readers {
    // The operand is a 32-bit IEEE floating point number.
    sealed class ShortInlineROperandReader : OperandReader {
        public sealed override object Read(ILBytesReader reader, Context.IOperandReaderContext context) {
            return reader.ReadFloat();
        }
    }
}