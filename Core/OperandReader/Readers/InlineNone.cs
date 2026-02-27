namespace ILReader.Readers {
    // No operand.
    sealed class InlineNoneOperandReader : OperandReader {
        public sealed override object Read(ILBytesReader reader, Context.IOperandReaderContext context) {
            return null;
        }
    }
}