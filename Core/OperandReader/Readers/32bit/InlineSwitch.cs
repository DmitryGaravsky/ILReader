namespace ILReader.Readers {
    // The operand is the 32-bit integer argument to a switch instruction.
    sealed class InlineSwitchOperandReader : IOperandReader {
        object IOperandReader.Read(IBinaryReader reader, Context.IOperandReaderContext context) {
            return reader.ReadInt(); // TODO
        }
    }
}