namespace ILReader.Readers {
    // The operand is an 8-bit integer containing the ordinal of a local variable or an argument.
    sealed class ShortInlineVarOperandReader : IOperandReader {
        object IOperandReader.Read(IBinaryReader reader, Context.IOperandReaderContext context) {
            return context[reader.ReadByte()];
        }
    }
    sealed class ShortInlineVarArgReader : IOperandReader {
        object IOperandReader.Read(IBinaryReader reader, Context.IOperandReaderContext context) {
            return reader.ReadByte();
        }
    }
}