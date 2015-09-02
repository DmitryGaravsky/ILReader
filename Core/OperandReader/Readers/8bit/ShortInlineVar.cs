namespace ILReader.Readers {
    // The operand is an 8-bit integer containing the ordinal of a local variable or an argument.
    sealed class ShortInlineVarOperandReader : IOperandReader {
        object IOperandReader.Read(IBinaryReader reader, Context.IOperandReaderContext context) {
            byte variableIndex = reader.ReadByte();
            return context.Variables[variableIndex];
        }
    }
}