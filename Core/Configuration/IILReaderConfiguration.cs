namespace ILReader {
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using ILReader.Context;
    using ILReader.Readers;

    public interface IILReaderConfiguration {
        IILReader GetReader(Stream dump);
        IILReader GetReader(MethodBase methodBase);
        IILReader GetReader(int metadataToken);
        IILReader GetReader(string typeName, string methodName);
        IILReader GetReader(string typeName, string methodName, string signature);
        IEnumerable<IMethodSymbol> FindMethods(string typeName, string methodName);
        ILBytesReader CreateBytesReader(byte[] bytes);
        IOperandReaderContext CreateOperandReaderContext(Stream dump);
        IOperandReaderContext CreateOperandReaderContext(MethodBase methodBase);
        void Reset(MethodBase methodBase);
        void Reset();
    }
}