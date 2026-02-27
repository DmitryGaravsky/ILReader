namespace ILReader.Readers {
    // The operand is an 8-bit integer containing the ordinal of a local variable or an argument.
    sealed class ShortInlineVarOperandReader : OperandReader {
        public sealed override object Read(ILBytesReader reader, Context.IOperandReaderContext context) {
            return context[reader.ReadByte()];
        }
    }
    sealed class ShortInlineVarArgReader : OperandReader {
        public sealed override object Read(ILBytesReader reader, Context.IOperandReaderContext context) {
            return reader.ReadByte();
        }
    }
}