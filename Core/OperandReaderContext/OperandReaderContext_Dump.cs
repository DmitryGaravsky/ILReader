using System;
using System.Collections.Generic;
using ILReader.Readers;

namespace ILReader.Context {
    sealed class OperandReaderContext_Dump : IOperandReaderContext {
        readonly Dump.IILReaderDump dump;
        IEnumerator<Tuple<ExceptionHandlerType, string, int[]>> exceptionHandlingClauses;
        public OperandReaderContext_Dump(Dump.IILReaderDump dump) {
            this.dump = dump;
            this.exceptionHandlingClauses = dump.ExceptionHandlers.GetEnumerator();
        }
        public OperandReaderContextType Type {
            get { return dump.Type; }
        }
        // Meta
        public string Name {
            get { return dump.Name; }
        }
        public IEnumerable<IMetadataItem> GetMetadata() {
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
        public IMetadataSymbol ResolveMethod(int methodToken) =>
            new StringSymbol(MetadataSymbolKind.Method, dump.Methods[methodToken]);
        public IMetadataSymbol ResolveField(int fieldToken) =>
            new StringSymbol(MetadataSymbolKind.Field, dump.Fields[fieldToken]);
        public IMetadataSymbol ResolveType(int typeToken) =>
            new StringSymbol(MetadataSymbolKind.Type, dump.Types[typeToken]);
        public IMetadataSymbol ResolveMember(int memberToken) =>
            new StringSymbol(MetadataSymbolKind.Member, dump.Members[memberToken]);
        public string ResolveString(int stringToken) {
            return dump.Strings[stringToken];
        }
        public byte[] ResolveSignature(int signatureToken) {
            return dump.Signatures[signatureToken];
        }
        public bool ResolveExceptionHandler(Func<int, IInstruction> getInstruction, out ExceptionHandler handler) {
            handler = null;
            if(exceptionHandlingClauses != null) {
                if(exceptionHandlingClauses.MoveNext()) {
                    var t = exceptionHandlingClauses.Current;
                    handler = new ExceptionHandler(getInstruction, t.Item1, t.Item2, t.Item3);
                    return true;
                }
                exceptionHandlingClauses.Dispose();
                exceptionHandlingClauses = null;
            }
            return false;
        }
        // Dump
        public void Dump(System.IO.Stream stream) {
            /* do nothing */
        }
    }
}