namespace ILReader.Dump {
    using System.Linq;
    using System.Collections.Generic;
    using ILReader.Readers;

    sealed class InstructionReaderDump : IILReaderDump {
        public InstructionReaderDump(System.IO.Stream dump) {
            // header(type, name.Length, il.Length)
            byte[] header = new byte[sizeof(byte) + sizeof(int) * 2];
            dump.Read(header, 0, header.Length);
            IBinaryReader headerReader = new BinaryReader(header);
            Type = (Context.OperandReaderContextType)headerReader.ReadByte();
            byte[] nameBytes = new byte[headerReader.ReadInt()];
            ilBytes = new byte[headerReader.ReadInt()];
            // data
            dump.Read(nameBytes, 0, nameBytes.Length);
            dump.Read(ilBytes, 0, ilBytes.Length);
            //
            Name = DumpHelper.GetString(nameBytes);
            metadata = DumpHelper.ReadMedatataItems(dump).ToArray();
            DumpHelper.Read(ref @this, dump);
            DumpHelper.Read(ref arguments, dump);
            DumpHelper.Read(ref variables, dump);
            DumpHelper.Read(methods.tokens, dump);
            DumpHelper.Read(fields.tokens, dump);
            DumpHelper.Read(types.tokens, dump);
            DumpHelper.Read(members.tokens, dump);
            DumpHelper.Read(strings.tokens, dump);
            DumpHelper.Read(signatures.tokens, dump);
        }
        public Context.OperandReaderContextType Type { get; private set; }
        //
        public static void Write(System.IO.Stream stream, Context.IOperandReaderContext context) {
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
        }
        #region Meta
        public string Name {
            get;
            private set;
        }
        Readers.IMetadataItem[] metadata;
        public IEnumerable<Readers.IMetadataItem> Metadata {
            get { return metadata; }
        }
        string @this;
        public string This {
            get { return @this; }
        }
        string[] variables;
        public string[] Variables {
            get { return variables; }
        }
        string[] arguments;
        public string[] Arguments {
            get { return arguments; }
        }
        #endregion
        #region Body
        byte[] ilBytes;
        public byte[] IL {
            get { return ilBytes; }
        }
        #endregion Body
        #region Tokens
        TokenLookup<string> methods = new TokenLookup<string>();
        public ITokenLookup<string> Methods {
            get { return methods; }
        }
        TokenLookup<string> fields = new TokenLookup<string>();
        public ITokenLookup<string> Fields {
            get { return fields; }
        }
        TokenLookup<string> types = new TokenLookup<string>();
        public ITokenLookup<string> Types {
            get { return types; }
        }
        TokenLookup<string> members = new TokenLookup<string>();
        public ITokenLookup<string> Members {
            get { return members; }
        }
        TokenLookup<string> strings = new TokenLookup<string>();
        public ITokenLookup<string> Strings {
            get { return strings; }
        }
        TokenLookup<byte[]> signatures = new TokenLookup<byte[]>();
        public ITokenLookup<byte[]> Signatures {
            get { return signatures; }
        }
        #endregion Properties
        class TokenLookup<T> : ITokenLookup<T> {
            internal readonly IDictionary<int, T> tokens = new Dictionary<int, T>();
            public T this[int token] {
                get {
                    T value;
                    return tokens.TryGetValue(token, out value) ? value : default(T);
                }
            }
        }
    }
}