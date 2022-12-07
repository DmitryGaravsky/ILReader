namespace ILReader.Dump {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using ILReader.Readers;
    using ILReader.Context;

    sealed class InstructionReaderDump : IILReaderDump {
        public InstructionReaderDump(Stream dump) {
            // header(type, name.Length, il.Length)
            byte[] header = new byte[sizeof(byte) + sizeof(int) * 2];
            dump.Read(header, 0, header.Length);
            IBinaryReader headerReader = new Readers.BinaryReader(header);
            Type = (OperandReaderContextType)headerReader.ReadByte();
            byte[] nameBytes = new byte[headerReader.ReadInt()];
            ilBytes = new byte[headerReader.ReadInt()];
            // data
            dump.Read(nameBytes, 0, nameBytes.Length);
            dump.Read(ilBytes, 0, ilBytes.Length);
            // name and meta
            Name = DumpHelper.GetString(nameBytes);
            metadata = DumpHelper.ReadMedatataItems(dump).ToArray();
            // tokens
            DumpHelper.Read(ref @this, dump);
            DumpHelper.Read(ref arguments, dump);
            DumpHelper.Read(ref variables, dump);
            DumpHelper.Read(methods.tokens, dump);
            DumpHelper.Read(fields.tokens, dump);
            DumpHelper.Read(types.tokens, dump);
            DumpHelper.Read(members.tokens, dump);
            DumpHelper.Read(strings.tokens, dump);
            DumpHelper.Read(signatures.tokens, dump);
            // exceptions
            exceptionHandlers = DumpHelper.ReadExceptionHandlers(dump).ToArray();
        }
        public OperandReaderContextType Type {
            get;
            private set;
        }
        //
        public static void Write(Stream stream, IOperandReaderContext context, ExceptionHandler[] handlers) {
            // type
            stream.Write(new byte[] { (byte)context.Type }, 0, 1);
            // name
            byte[] nameBytes = null;
            DumpHelper.GetBytes(ref nameBytes, context.Name);
            DumpHelper.Write(nameBytes.Length, stream);
            // IL
            byte[] ILBytes = context.GetIL();
            DumpHelper.Write(ILBytes.Length, stream);
            // context data
            stream.Write(nameBytes, 0, nameBytes.Length);
            stream.Write(ILBytes, 0, ILBytes.Length);
            // meta
            DumpHelper.WriteMedatataItems(stream, context.GetMetadata());
            // body
            context.Dump(stream);
            // handlers
            DumpHelper.WriteExceptionHandlers(stream, handlers);
        }
        #region Meta
        public string Name {
            get;
            private set;
        }
        readonly IMetadataItem[] metadata;
        public IEnumerable<IMetadataItem> Metadata {
            get { return metadata; }
        }
        readonly string @this;
        public string This {
            get { return @this; }
        }
        readonly string[] variables;
        public string[] Variables {
            get { return variables; }
        }
        readonly string[] arguments;
        public string[] Arguments {
            get { return arguments; }
        }
        #endregion
        #region Body
        readonly byte[] ilBytes;
        public byte[] IL {
            get { return ilBytes; }
        }
        readonly IReadOnlyCollection<Tuple<ExceptionHandlerType, string, int[]>> exceptionHandlers;
        public IReadOnlyCollection<Tuple<ExceptionHandlerType, string, int[]>> ExceptionHandlers {
            get { return exceptionHandlers; }
        }
        #endregion Body
        #region Tokens
        readonly TokenLookup<string> methods = new TokenLookup<string>();
        public ITokenLookup<string> Methods {
            get { return methods; }
        }
        readonly TokenLookup<string> fields = new TokenLookup<string>();
        public ITokenLookup<string> Fields {
            get { return fields; }
        }
        readonly TokenLookup<string> types = new TokenLookup<string>();
        public ITokenLookup<string> Types {
            get { return types; }
        }
        readonly TokenLookup<string> members = new TokenLookup<string>();
        public ITokenLookup<string> Members {
            get { return members; }
        }
        readonly TokenLookup<string> strings = new TokenLookup<string>();
        public ITokenLookup<string> Strings {
            get { return strings; }
        }
        readonly TokenLookup<byte[]> signatures = new TokenLookup<byte[]>();
        public ITokenLookup<byte[]> Signatures {
            get { return signatures; }
        }
        #endregion Tokens
        sealed class TokenLookup<T> : ITokenLookup<T> {
            internal readonly Dictionary<int, T> tokens = new Dictionary<int, T>();
            public T this[int token] {
                get {
                    T value;
                    return tokens.TryGetValue(token, out value) ? value : default(T);
                }
            }
        }
    }
}