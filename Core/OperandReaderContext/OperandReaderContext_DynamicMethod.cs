namespace ILReader.Context {
    using System;
    using System.Reflection;
    using System.Reflection.Emit;
    using ILReader.Monads;

    sealed class OperandReaderContext_DynamicMethod : OperandReaderContextReal, IOperandReaderContext {
        readonly object resolver;
        RTTypes.TokenResolver resolveToken;
        RTTypes.StringResolver resolveString;
        RTTypes.SignatureResolver resolveSignature;
        //
        RTTypes.TypeFromHandleUnsafe typeFromHandle;
        RTTypes.MethodFromHandles methodFromHandles;
        RTTypes.FieldFromHandles fieldFromHandles;
        public OperandReaderContext_DynamicMethod(DynamicMethod method) {
            this.name = method.Name;
            ILGenerator ILGen = method.GetILGenerator();
            this.maxStackSize = RTTypes.GetMaxStackSize(ILGen);
            this.@this = method.DeclaringType;
            this.initLocals = method.InitLocals;
            this.arguments = method.GetParameters();
            InitVariables(ILGen);
            InitMethodSpec(method);
            //
            this.resolver = RTTypes.GetResolver(method);
            if(resolver != null) {
                this.ILBytes = RTTypes.GetIL(resolver);
                this.resolveToken = RTTypes.GetTokenResolver(resolver);
                this.resolveString = RTTypes.GetStringResolver(resolver);
                this.resolveSignature = RTTypes.GetSignatureResolver(resolver);
                this.typeFromHandle = RTTypes.GetTypeFromHandleUnsafe();
                this.methodFromHandles = RTTypes.GetMethodFromHandles();
                this.fieldFromHandles = RTTypes.GetFieldFromHandles();
            }
            else {
                this.ILBytes = new byte[ILGen.ILOffset];
                RTTypes.CopyILStream(ILGen, ILBytes);
                // TODO
            }
        }
        void InitMethodSpec(DynamicMethod method) {
            var implFlags = method.GetMethodImplementationFlags();
            this.methodSpec = "dynamic " +
                (method.IsStatic ? "static " : "instance ") +
                ((method.ReturnType != null && method.ReturnType != typeof(void)) ? method.ReturnType.ToString() + " " : "void ") +
                method.Name + " " + implFlags.ToString().ToLower();
        }
        void InitVariables(ILGenerator ILGen) {
            var localSig = RTTypes.GetLocalSignature(ILGen);
            this.variables = new Readers.LocalSignatureReader(localSig).Locals;
        }
        public OperandReaderContextType Type {
            get { return OperandReaderContextType.DynamicMethod; }
        }
        #region Resolve
        public object ResolveMethod(int methodToken) {
            return GetOrCache(methodTokens, methodToken, t => ResolveMethodCore(t));
        }
        public object ResolveField(int fieldToken) {
            return GetOrCache(fieldTokens, fieldToken, t => ResolveFieldCore(t));
        }
        public object ResolveType(int typeToken) {
            return GetOrCache(typeTokens, typeToken, t => ResolveTypeCore(t));
        }
        public object ResolveMember(int memberToken) {
            return GetOrCache(memberTokens, memberToken, t => ResolveMemberCore(t));
        }
        public string ResolveString(int stringToken) {
            return GetOrCache(stringTokens, stringToken, t => resolveString(t));
        }
        public byte[] ResolveSignature(int signatureToken) {
            return GetOrCache(signatureTokens, signatureToken, t => resolveSignature(t, 0));
        }
        //
        Type ResolveTypeCore(int typeToken) {
            IntPtr typeHandle = IntPtr.Zero, methodHanlde, fieldHandle;
            resolveToken.@Do(f => f(typeToken, out typeHandle, out methodHanlde, out fieldHandle));
            return typeFromHandle.@Get(f => f(typeHandle));
        }
        MethodBase ResolveMethodCore(int methodToken) {
            IntPtr typeHandle = IntPtr.Zero; IntPtr methodHanlde = IntPtr.Zero, fieldHandle;
            resolveToken.@Do(resolve => resolve(methodToken, out typeHandle, out methodHanlde, out fieldHandle));
            Type mType = typeFromHandle.@Get(f => typeHandle != IntPtr.Zero ? f(typeHandle) : null);
            return methodFromHandles.@Get(f => f(mType, methodHanlde));
        }
        FieldInfo ResolveFieldCore(int fieldToken) {
            IntPtr typeHandle = IntPtr.Zero; IntPtr methodHanlde; IntPtr fieldHandle = IntPtr.Zero;
            resolveToken.@Do(resolve => resolve(fieldToken, out typeHandle, out methodHanlde, out fieldHandle));
            Type mType = typeFromHandle.@Get(f => typeHandle != IntPtr.Zero ? f(typeHandle) : null);
            return fieldFromHandles.@Get(f => f(mType, fieldHandle));
        }
        MemberInfo ResolveMemberCore(int memberToken) {
            IntPtr typeHandle = IntPtr.Zero; IntPtr methodHanlde = IntPtr.Zero; IntPtr fieldHandle = IntPtr.Zero;
            resolveToken.@Do(f => f(memberToken, out typeHandle, out methodHanlde, out fieldHandle));
            Type mType = typeFromHandle.@Get(f => typeHandle != IntPtr.Zero ? f(typeHandle) : null);
            if(methodHanlde != IntPtr.Zero)
                return methodFromHandles.@Get(f => f(mType, methodHanlde));
            if(fieldHandle != IntPtr.Zero)
                return fieldFromHandles.@Get(f => f(mType, fieldHandle));
            return mType;
        }
        #endregion Resolve
        protected override string VariableToString(object variable) {
            var varSig = (Readers.LocalVarSig)variable;
            string variableStr = TypeToString(varSig.Type);
            return varSig.IsPinned ? variableStr + " (pinned)" : variableStr;
        }
    }
}