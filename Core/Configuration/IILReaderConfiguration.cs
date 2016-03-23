namespace ILReader {
    using System.Reflection;
    using ILReader.Context;
    using ILReader.Readers;

    public interface IILReaderConfiguration {
        IILReader GetReader(MethodBase methodBase);
        // Services
        IBinaryReader CreateBinaryReader(byte[] bytes);
        IOperandReaderContext CreateOperandReaderContext(MethodBase methodBase, MethodBody methodBody);
        // Reset
        void Reset(MethodBase methodBase);
        void Reset();
    }
}