namespace ILReader.Readers {
    interface IOperandReader {
        object Read(IBinaryReader reader, Context.IOperandReaderContext context);
    }
}