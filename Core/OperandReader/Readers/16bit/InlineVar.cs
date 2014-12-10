namespace ILReader.Readers {
    using ILReader.Context;

    // The operand is 16-bit integer containing the ordinal of a local variable or an argument.
    sealed class InlineVarOperandReader : IOperandReader {
        object IOperandReader.Read(IBinaryReader reader, IOperandReaderContext context) {
            short variableIndex = reader.ReadShort(); // TODO
            return context.Variables[variableIndex];
        }
    }
}