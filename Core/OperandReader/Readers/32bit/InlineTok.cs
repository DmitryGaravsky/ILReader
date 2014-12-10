namespace ILReader.Readers {
    using ILReader.Context;

    // The operand is a FieldRef, MethodRef, or TypeRef token.
    sealed class InlineTokOperandReader : IOperandReader {
        object IOperandReader.Read(IBinaryReader reader, IOperandReaderContext context) {
            int refToken = reader.ReadInt();
            return context.Module.ResolveMember(refToken);
        }
    }
}