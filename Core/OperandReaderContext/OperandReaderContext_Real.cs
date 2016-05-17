namespace ILReader.Context {
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using ILReader.Dump;

    class OperandReaderContextReal : ISupportDump {
        protected string name;
        public string Name {
            get { return name; }
        }
        // Meta
        protected int maxStackSize;
        protected object methodSpec;
        protected object @this;
        protected object[] variables;
        protected object[] arguments;
        protected bool initLocals;
        //
        public object This {
            get { return @this; }
        }
        public object this[byte index, bool argument = false] {
            get { return argument ? arguments[index] : variables[index]; }
        }
        public object this[short index, bool argument = false] {
            get { return argument ? arguments[index] : variables[index]; }
        }
        public IEnumerable<Readers.IMetadataItem> GetMetadata() {
            if(arguments == null || arguments.Length == 0 && methodSpec != null)
                yield return new Readers.MetadataItem(methodSpec.ToString(), null);
            if(arguments != null && arguments.Length > 0 && methodSpec != null)
                yield return new Readers.MetadataItem(methodSpec.ToString(), GetArgs());
            yield return new Readers.MetadataItem(".codeSize", GetIL().Length);
            yield return new Readers.MetadataItem(".maxStack", maxStackSize);
            if(variables != null && variables.Length > 0) {
                if(initLocals)
                    yield return new Readers.MetadataItem(".locals init", GetLocals());
                else
                    yield return new Readers.MetadataItem(".locals", GetLocals());
            }
        }
        protected IEnumerable<Readers.IMetadataItem> GetArgs() {
            for(int i = 0; i < arguments.Length; i++)
                yield return new Readers.MetadataItem("[" + i.ToString() + "]", arguments[i].ToString());
        }
        protected IEnumerable<Readers.IMetadataItem> GetLocals() {
            for(int i = 0; i < variables.Length; i++)
                yield return new Readers.MetadataItem("[" + i.ToString() + "]", variables[i].ToString());
        }
        // Body
        readonly static byte[] EmptyIL = new byte[] { };
        protected byte[] ILBytes;
        public byte[] GetIL() {
            return ILBytes ?? EmptyIL; 
        }
        // Tokens
        protected readonly IDictionary<int, MethodBase> methodTokens = new Dictionary<int, MethodBase>();
        protected readonly IDictionary<int, FieldInfo> fieldTokens = new Dictionary<int, FieldInfo>();
        protected readonly IDictionary<int, Type> typeTokens = new Dictionary<int, Type>();
        protected readonly IDictionary<int, MemberInfo> memberTokens = new Dictionary<int, MemberInfo>();
        protected readonly IDictionary<int, string> stringTokens = new Dictionary<int, string>();
        protected readonly IDictionary<int, byte[]> signatureTokens = new Dictionary<int, byte[]>();
        protected static T GetOrCache<T>(IDictionary<int, T> tokens, int token, Func<int, T> getFunc) {
            T result;
            if(!tokens.TryGetValue(token, out result)) {
                result = getFunc(token);
                tokens.Add(token, result);
            }
            return result;
        }
        // Dump
        public void Dump(System.IO.Stream stream) {
            ILReader.Dump.DumpHelper.Write((@this ?? String.Empty).ToString(), stream);
            ILReader.Dump.DumpHelper.Write(arguments, stream);
            ILReader.Dump.DumpHelper.Write(variables, stream);
            ILReader.Dump.DumpHelper.Write(methodTokens, stream);
            ILReader.Dump.DumpHelper.Write(fieldTokens, stream);
            ILReader.Dump.DumpHelper.Write(typeTokens, stream);
            ILReader.Dump.DumpHelper.Write(memberTokens, stream);
            ILReader.Dump.DumpHelper.Write(stringTokens, stream);
            ILReader.Dump.DumpHelper.Write(signatureTokens, stream);
        }
    }
}