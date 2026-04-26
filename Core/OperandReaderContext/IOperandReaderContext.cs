namespace ILReader.Context {
    using System;
    using System.Collections.Generic;
    using ILReader.Readers;

    public enum OperandReaderContextType : byte {
        Method, DynamicMethod
    }
    public interface IOperandReaderContext : Dump.ISupportDump {
        OperandReaderContextType Type { get; }
        string Name { get; }
        byte[] GetIL();
        object This { get; }
        object this[byte index, bool argument = false] { get; }
        object this[short index, bool argument = false] { get; }
        IMetadataSymbol ResolveField(int metadataToken);
        IMetadataSymbol ResolveMethod(int metadataToken);
        IMetadataSymbol ResolveMember(int metadataToken);
        IMetadataSymbol ResolveType(int metadataToken);
        byte[] ResolveSignature(int metadataToken);
        string ResolveString(int metadataToken);
        bool ResolveExceptionHandler(Func<int, IInstruction> getInstruction, out ExceptionHandler handler);
        IEnumerable<IMetadataItem> GetMetadata();
    }
}