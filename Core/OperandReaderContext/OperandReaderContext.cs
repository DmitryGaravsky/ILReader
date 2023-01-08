namespace ILReader.Context {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using ILReader.Readers;

    sealed class OperandReaderContext : OperandReaderContextReal, IOperandReaderContext {
        readonly static object[] NoVariables = new object[0];
        readonly Module module;
        readonly Type[] methodArguments;
        readonly Type[] typeArguments;
        IEnumerator<ExceptionHandlingClause> exceptionHandlingClauses;
        public OperandReaderContext(MethodBase method, MethodBody methodBody) {
            RTTypes.TryPrepareMethod(method);
            this.module = method.Module;
            this.name = method.Name;
            // meta
            var implFlags = method.GetMethodImplementationFlags();
            this.initLocals = (methodBody != null) && methodBody.InitLocals;
            this.maxStackSize = (methodBody != null) ? methodBody.MaxStackSize : 0;
            var declaringType = method.DeclaringType;
            this.@this = declaringType;
            this.variables = (methodBody != null) ? methodBody.LocalVariables.ToArray() : NoVariables;
            this.arguments = method.GetParameters();
            // method
            ConstructorInfo cInfo = method as ConstructorInfo;
            if(cInfo != null) {
                this.methodSpec =
                    (cInfo.IsStatic ? "static " : string.Empty) + ".ctor " +
                    declaringType.ToString() + " " + GetImplFlagString(implFlags);
            }
            MethodInfo mInfo = method as MethodInfo;
            if(mInfo != null) {
                var retType = mInfo.ReturnType;
                this.methodSpec =
                    (mInfo.IsStatic ? "static " : "instance ") +
                    ((retType != null && retType != typeof(void)) ? retType.ToString() + " " : "void ") +
                    mInfo.Name + " " + GetImplFlagString(implFlags);
            }
            methodArguments = method.IsGenericMethod ? method.GetGenericArguments() : null;
            typeArguments = (declaringType != null) && declaringType.IsGenericType ? declaringType.GetGenericArguments() : null;
            //
            this.ILBytes = methodBody?.GetILAsByteArray();
            this.exceptionHandlingClauses = methodBody?.ExceptionHandlingClauses.GetEnumerator();
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
        public bool ResolveExceptionHandler(Func<int, IInstruction> getInstruction, out ExceptionHandler handler) {
            handler = null;
            if(exceptionHandlingClauses != null) {
                if(exceptionHandlingClauses.MoveNext()) {
                    handler = new ExceptionHandler(getInstruction, exceptionHandlingClauses.Current);
                    return true;
                }
                exceptionHandlingClauses.Dispose();
                exceptionHandlingClauses = null;
            }
            return false;
        }
        #endregion Resolve
        protected override string VariableToString(object variable) {
            var variableInfo = (LocalVariableInfo)variable;
            string variableStr = TypeToString(variableInfo.LocalType) +
                " (" + variableInfo.LocalIndex.ToString() + ")";
            return variableInfo.IsPinned ? variableStr + " (pinned)" : variableStr;
        }
    }
}