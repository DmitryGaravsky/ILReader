namespace ILReader.Readers {
    // The operand is the 32-bit integer argument to a switch instruction.
    sealed class InlineSwitchOperandReader : IOperandReader {
        object IOperandReader.Read(IBinaryReader reader, Context.IOperandReaderContext context) {
            int casesCount = reader.ReadInt();
            int[] labelOffsets = new int[casesCount];
            for(int i = 0; i < labelOffsets.Length; i++)
                labelOffsets[i] = reader.ReadInt();
            return labelOffsets;
        }
    }
}