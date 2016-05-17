namespace ILReader.Dump {
    using System.Collections.Generic;

    public interface ITokenLookup<T> {
        T this[int token] { get; }
    }
    public interface IILReaderDump {
        Context.OperandReaderContextType Type { get; }
        // Meta
        string Name { get; }
        IEnumerable<Readers.IMetadataItem> Metadata { get; }
        string This { get; }
        string[] Arguments { get; }
        string[] Variables { get; }
        // Body
        byte[] IL { get; }
        // Tokens
        ITokenLookup<string> Methods { get; }
        ITokenLookup<string> Types { get; }
        ITokenLookup<string> Fields { get; }
        ITokenLookup<string> Members { get; }
        ITokenLookup<string> Strings { get; }
        ITokenLookup<byte[]> Signatures { get; }
    }
}