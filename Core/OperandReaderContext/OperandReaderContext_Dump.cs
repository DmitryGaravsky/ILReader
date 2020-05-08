using System.Collections.Generic;
namespace ILReader.Context {
    sealed class OperandReaderContextDump : IOperandReaderContext {
        readonly Dump.IILReaderDump dump;
        public OperandReaderContextDump(Dump.IILReaderDump dump) {
            this.dump = dump;
        }
        public OperandReaderContextType Type {
            get { return dump.Type; }
        }
        // Meta
        public string Name {
            get { return dump.Name; }
        }
        public IEnumerable<Readers.IMetadataItem> GetMetadata() {
            return dump.Metadata;
        }
        // Body
        public byte[] GetIL() {
            return dump.IL;
        }
        public object This {
            get { return dump.This; }
        }
        public object this[byte index, bool argument = false] {
            get { return argument ? dump.Arguments[index] : dump.Variables[index]; }
        }
        public object this[short index, bool argument = false] {
            get { return argument ? dump.Arguments[index] : dump.Variables[index]; }
        }
        // Tokens
        public object ResolveMethod(int methodToken) {
            return dump.Methods[methodToken];
        }
        public object ResolveField(int fieldToken) {
            return dump.Fields[fieldToken];
        }
        public object ResolveType(int typeToken) {
            return dump.Types[typeToken];
        }
        public object ResolveMember(int memberToken) {
            return dump.Members[memberToken];
        }
        public string ResolveString(int stringToken) {
            return dump.Strings[stringToken];
        }
        public byte[] ResolveSignature(int signatureToken) {
            return dump.Signatures[signatureToken];
        }
        // Dump
        public void Dump(System.IO.Stream stream) { 
            /* do nothing */
        }
    }
}