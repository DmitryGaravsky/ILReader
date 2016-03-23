namespace ILReader.Context {
    using System;
    using System.Linq;
    using System.Reflection;

    class OperandReaderContext : IOperandReaderContext {
        public OperandReaderContext(MethodBase method)
            : this(method, method.GetMethodBody()) {
        }
        public OperandReaderContext(MethodBase method, MethodBody methodBody) {
            module = method.Module;
            variables = methodBody.LocalVariables.ToArray();
            methodArguments = method.IsGenericMethod ? method.GetGenericArguments() : null;
            typeArguments = (method.DeclaringType != null) && method.DeclaringType.IsGenericType ? method.DeclaringType.GetGenericArguments() : null;
        }
        readonly Module module;
        readonly LocalVariableInfo[] variables;
        readonly Type[] methodArguments;
        readonly Type[] typeArguments;
        public LocalVariableInfo this[byte variableIndex] {
            get { return variables[variableIndex]; } 
        }
        public LocalVariableInfo this[short variableIndex] {
            get { return variables[variableIndex]; } 
        }
        public MethodBase ResolveMethod(int methodToken) {
            return module.ResolveMethod(methodToken, typeArguments, methodArguments);
        }
        public FieldInfo ResolveField(int fieldToken) {
            return module.ResolveField(fieldToken, typeArguments, methodArguments);
        }
        public Type ResolveType(int typeToken) {
            return module.ResolveType(typeToken, typeArguments, methodArguments);
        }
        public MemberInfo ResolveMember(int memberToken) {
            return module.ResolveMember(memberToken, typeArguments, methodArguments);
        }
        public string ResolveString(int stringToken) {
            return module.ResolveString(stringToken);
        }
        public byte[] ResolveSignature(int sigToken) {
            return module.ResolveSignature(sigToken);
        }
    }
}