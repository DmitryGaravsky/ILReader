namespace ILReader {
    using System.IO;
    using System.Reflection;
    using ILReader.Context;
    using ILReader.Readers;

    public interface IILReaderConfiguration {
        IILReader GetReader(Stream dump);
        IILReader GetReader(MethodBase methodBase);
        IBinaryReader CreateBinaryReader(byte[] bytes);
        IOperandReaderContext CreateOperandReaderContext(Stream dump);
        IOperandReaderContext CreateOperandReaderContext(MethodBase methodBase);
        void Reset(MethodBase methodBase);
        void Reset();
    }
}