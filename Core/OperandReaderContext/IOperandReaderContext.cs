namespace ILReader.Context {
    using System.Collections.Generic;

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
        //
        object ResolveField(int metadataToken);
        object ResolveMethod(int metadataToken);
        object ResolveMember(int metadataToken);
        object ResolveType(int metadataToken);
        //
        byte[] ResolveSignature(int metadataToken);
        string ResolveString(int metadataToken);
        //
        IEnumerable<Readers.IMetadataItem> GetMetadata();
    }
}