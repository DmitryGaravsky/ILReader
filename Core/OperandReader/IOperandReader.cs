namespace ILReader.Readers {
    using ILReader.Context;

    interface IOperandReader {
        object Read(IBinaryReader reader, IOperandReaderContext context);
    }
}