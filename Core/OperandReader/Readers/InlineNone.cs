namespace ILReader.Readers {
    using ILReader.Context;

    // No operand.
    sealed class InlineNoneOperandReader : IOperandReader {
        object IOperandReader.Read(IBinaryReader reader, IOperandReaderContext context) {
            return null;
        }
    }
}