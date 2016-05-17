namespace ILReader.Readers {
    // The operand is 16-bit integer containing the ordinal of a local variable or an argument.
    sealed class InlineVarOperandReader : IOperandReader {
        object IOperandReader.Read(IBinaryReader reader, Context.IOperandReaderContext context) {
            return context[reader.ReadShort()];
        }
    }
    sealed class InlineVarArgReader : IOperandReader {
        object IOperandReader.Read(IBinaryReader reader, Context.IOperandReaderContext context) {
            return reader.ReadShort();
        }
    }
}