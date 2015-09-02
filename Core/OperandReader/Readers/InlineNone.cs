namespace ILReader.Readers {
    // No operand.
    sealed class InlineNoneOperandReader : IOperandReader {
        object IOperandReader.Read(IBinaryReader reader, Context.IOperandReaderContext context) {
            return null;
        }
    }
}