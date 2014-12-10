﻿namespace ILReader.Readers {
    using ILReader.Context;

    // The operand is a 32-bit metadata token.
    sealed class InlineMethodOperandReader : IOperandReader {
        object IOperandReader.Read(IBinaryReader reader, IOperandReaderContext context) {
            int methodToken = reader.ReadInt();
            return context.Module.ResolveMethod(methodToken);
        }
    }
}