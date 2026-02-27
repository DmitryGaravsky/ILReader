namespace ILReader.Readers {
    // The operand is 16-bit integer containing the ordinal of a local variable or an argument.
    sealed class InlineVarOperandReader : OperandReader {
        public sealed override object Read(ILBytesReader reader, Context.IOperandReaderContext context) {
            return context[reader.ReadShort()];
        }
    }
    sealed class InlineVarArgReader : OperandReader {
        public sealed override object Read(ILBytesReader reader, Context.IOperandReaderContext context) {
            return reader.ReadShort();
        }
    }
}