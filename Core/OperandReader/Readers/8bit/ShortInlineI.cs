namespace ILReader.Readers {
    // The operand is an 8-bit integer.
    sealed class ShortInlineIOperandReader : OperandReader {
        public sealed override object Read(ILBytesReader reader, Context.IOperandReaderContext context) {
            return reader.ReadByte();
        }
    }
}