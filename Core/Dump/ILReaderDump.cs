namespace ILReader.Dump {
    using System;
    using System.Collections.Generic;
    using ILReader.Context;
    using ILReader.Readers;

    public interface ITokenLookup<T> {
        T this[int token] { get; }
    }
    //
    public interface IILReaderDump {
        OperandReaderContextType Type { get; }
        // Meta
        string Name { get; }
        IEnumerable<IMetadataItem> Metadata { get; }
        string This { get; }
        string[] Arguments { get; }
        string[] Variables { get; }
        // Body
        byte[] IL { get; }
        IReadOnlyCollection<Tuple<ExceptionHandlerType, string, int[]>> ExceptionHandlers { get; }
        // Tokens
        ITokenLookup<string> Methods { get; }
        ITokenLookup<string> Types { get; }
        ITokenLookup<string> Fields { get; }
        ITokenLookup<string> Members { get; }
        ITokenLookup<string> Strings { get; }
        ITokenLookup<byte[]> Signatures { get; }
    }
}