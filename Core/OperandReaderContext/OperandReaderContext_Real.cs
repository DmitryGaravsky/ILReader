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
                yield return new Readers.MetadataItem("[" + i.ToString() + "]", ArgumentToString(arguments[i]));
        }
        protected IEnumerable<Readers.IMetadataItem> GetLocals() {
            for(int i = 0; i < variables.Length; i++)
                yield return new Readers.MetadataItem("[" + i.ToString() + "]", VariableToString(variables[i]));
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
            ILReader.Dump.DumpHelper.Write(arguments, stream, ArgumentToString);
            ILReader.Dump.DumpHelper.Write(variables, stream, VariableToString);
            ILReader.Dump.DumpHelper.Write(methodTokens, stream, MethodToString);
            ILReader.Dump.DumpHelper.Write(fieldTokens, stream, FieldToString);
            ILReader.Dump.DumpHelper.Write(typeTokens, stream, TypeToString);
            ILReader.Dump.DumpHelper.Write(memberTokens, stream, MemberToString);
            ILReader.Dump.DumpHelper.Write(stringTokens, stream);
            ILReader.Dump.DumpHelper.Write(signatureTokens, stream);
        }
        protected virtual string ArgumentToString(object argument) {
            var parameterInfo = (ParameterInfo)argument;
            return TypeToString(parameterInfo.ParameterType) + " " + parameterInfo.Name;
        }
        protected virtual string VariableToString(object variable) {
            return variable.ToString();
        }
        static IDictionary<Type, string> typeAliases = new Dictionary<Type, string> { 
            { typeof(void), "void" },
            { typeof(object), "object" },
            { typeof(string), "string" },
            { typeof(bool), "bool" },
            { typeof(char), "char" },
            { typeof(byte), "byte" },
            { typeof(int), "int" },
            { typeof(long), "long" },
            { typeof(decimal), "decimal" },
            { typeof(float), "float" },
            { typeof(double), "double" },
        };
        protected static string TypeToString(Type type) {
            string alias;
            return typeAliases.TryGetValue(type, out alias) ? alias :
                (type.Namespace == "System" || type.Namespace == "System.Reflection" ? type.Name : type.ToString());
        }
        static string MethodToString(MethodBase method) {
            string returnType, declaringType = null;
            if(method.DeclaringType != null)
                declaringType = TypeToString(method.DeclaringType);
            if(!method.IsConstructor) {
                var mInfo = method as MethodInfo;
                if(mInfo != null) {
                    returnType = TypeToString(mInfo.ReturnType);
                    string name = method.Name;
                    if(!string.IsNullOrEmpty(declaringType))
                        name = declaringType + "." + name;
                    return returnType + " " + name;
                }
            }
            return method.ToString();
        }
        static string FieldToString(FieldInfo field) {
            return field.ToString();
        }
        static string MemberToString(MemberInfo member) {
            var method = member as MethodBase;
            if(method != null)
                return MethodToString(method);
            return member.ToString();
        }
    }
}