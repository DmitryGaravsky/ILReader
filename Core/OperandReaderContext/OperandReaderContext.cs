namespace ILReader.Context {
    using System;
    using System.Linq;
    using System.Reflection;

    sealed class OperandReaderContext : OperandReaderContextReal, IOperandReaderContext {
        readonly Module module;
        readonly Type[] methodArguments;
        readonly Type[] typeArguments;
        public OperandReaderContext(MethodBase method, MethodBody methodBody) {
            RTTypes.TryPrepareMethod(method);
            this.module = method.Module;
            this.name = method.Name;
            //
            var implFlags = method.GetMethodImplementationFlags();
            this.initLocals = (methodBody != null) && methodBody.InitLocals;
            this.maxStackSize = (methodBody != null) ? methodBody.MaxStackSize : 0;
            this.@this = method.DeclaringType;
            this.variables = (methodBody != null) ? methodBody.LocalVariables.ToArray() : new object[] { };
            this.arguments = method.GetParameters();
            ConstructorInfo cInfo = method as ConstructorInfo;
            if(cInfo != null) {
                this.methodSpec =
                    (cInfo.IsStatic ? "static " : string.Empty) + ".ctor " +
                    cInfo.DeclaringType.ToString() + " " + implFlags.ToString().ToLower();
            }
            MethodInfo mInfo = method as MethodInfo;
            if(mInfo != null) {
                this.methodSpec =
                    (mInfo.IsStatic ? "static " : "instance ") +
                    ((mInfo.ReturnType != null && mInfo.ReturnType != typeof(void)) ? mInfo.ReturnType.ToString() + " " : "void ") +
                    mInfo.Name + " " + implFlags.ToString().ToLower();
            }
            methodArguments = method.IsGenericMethod ?
                method.GetGenericArguments() : null;
            typeArguments = (method.DeclaringType != null) && method.DeclaringType.IsGenericType ?
                method.DeclaringType.GetGenericArguments() : null;
            //
            this.ILBytes = (methodBody != null) ? methodBody.GetILAsByteArray() : null;
        }
        //
        public OperandReaderContextType Type {
            get { return OperandReaderContextType.Method; }
        }
        #region Resolve
        public object ResolveMethod(int methodToken) {
            return GetOrCache(methodTokens, methodToken, t => module.ResolveMethod(t, typeArguments, methodArguments));
        }
        public object ResolveField(int fieldToken) {
            return GetOrCache(fieldTokens, fieldToken, t => module.ResolveField(t, typeArguments, methodArguments));
        }
        public object ResolveType(int typeToken) {
            return GetOrCache(typeTokens, typeToken, t => module.ResolveType(t, typeArguments, methodArguments));
        }
        public object ResolveMember(int memberToken) {
            return GetOrCache(memberTokens, memberToken, t => module.ResolveMember(t, typeArguments, methodArguments));
        }
        public string ResolveString(int stringToken) {
            return GetOrCache(stringTokens, stringToken, t => module.ResolveString(t));
        }
        public byte[] ResolveSignature(int signatureToken) {
            return GetOrCache(signatureTokens, signatureToken, t => module.ResolveSignature(t));
        }
        #endregion Resolve
        protected override string VariableToString(object variable) {
            var variableInfo = (LocalVariableInfo)variable;
            string variableStr = TypeToString(variableInfo.LocalType) + " (" + variableInfo.LocalIndex.ToString() + ")";
            return variableInfo.IsPinned ? variableStr + " (pinned)" : variableStr;
        }
    }
}