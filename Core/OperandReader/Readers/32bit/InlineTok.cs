namespace ILReader.Readers {
    // The operand is a FieldRef, MethodRef, or TypeRef token.
    sealed class InlineTokOperandReader : OperandReader {
        public sealed override object Read(ILBytesReader reader, Context.IOperandReaderContext context) {
            return context.ResolveMember(reader.ReadInt());
        }
    }
}