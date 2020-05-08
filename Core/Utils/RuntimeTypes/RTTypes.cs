namespace ILReader {
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.CompilerServices;
    using FastAccessors.Monads;
    using BF = System.Reflection.BindingFlags;

    static class RTTypes {
        internal static readonly Type DynamicMethodType = typeof(DynamicMethod);
        internal static readonly Type RTDynamicMethodType = DynamicMethodType.GetNestedType("RTDynamicMethod", BF.NonPublic);
        internal static readonly Type ILGeneratorType = typeof(ILGenerator);
        //
        static readonly int fld_m_owner = "m_owner".@ƒRegister(RTDynamicMethodType);
        internal static DynamicMethod GetOwnerDynamicMethod(object methodBase) {
            return methodBase.@ƒ(fld_m_owner) as DynamicMethod;
        }
        //
        static readonly int fld_m_resolver = "m_resolver".@ƒRegister(DynamicMethodType);
        internal static object GetResolver(object dynamicMethod) {
            return dynamicMethod.@ƒ(fld_m_resolver);
        }
        //
        static readonly int fld_m_ILStream = "m_ILStream".@ƒRegister(ILGeneratorType);
        internal static void CopyILStream(object ILGenerator, byte[] bytes) {
            Array.Copy((byte[])ILGenerator.@ƒ(fld_m_ILStream), bytes, bytes.Length);
        }
        static readonly int fld_m_localSignature = "m_localSignature".@ƒRegister(ILGeneratorType);
        internal static byte[] GetLocalSignature(object ILGenerator) {
            return ((SignatureHelper)ILGenerator.@ƒ(fld_m_localSignature)).GetSignature();
        }
        static readonly int fld_m_maxStackSize = "m_maxStackSize".@ƒRegister(ILGeneratorType);
        internal static int GetMaxStackSize(object ILGenerator) {
            return (int)ILGenerator.@ƒ(fld_m_maxStackSize);
        }
        //
        internal static readonly Type DynamicResolverType = Type.GetType("System.Reflection.Emit.DynamicResolver");
        static readonly int fld_m_Code = "m_code".@ƒRegister(DynamicResolverType);
        internal static byte[] GetIL(object resolver) {
            return (byte[])resolver.@ƒ(fld_m_Code);
        }
        static readonly MethodInfo mInfo_ResolveToken = DynamicResolverType.GetMethod("ResolveToken", BF.Instance | BF.NonPublic);
        static readonly MethodInfo mInfo_GetStringLiteral = DynamicResolverType.GetMethod("GetStringLiteral", BF.Instance | BF.NonPublic);
        static readonly MethodInfo mInfo_ResolveSignature = DynamicResolverType.GetMethod("ResolveSignature", BF.Instance | BF.NonPublic);
        static readonly MethodInfo mInfo_CreateDelegate = typeof(Delegate).GetMethod("CreateDelegate",
            new Type[] { typeof(Type), typeof(object), typeof(MethodInfo) });

        internal delegate void TokenResolver(int token, out IntPtr typeHandle, out IntPtr methodHandle, out IntPtr fieldHandle);
        internal delegate string StringResolver(int token);
        internal delegate byte[] SignatureResolver(int token, int fromMethod);
        //
        static Func<object, TokenResolver> getTokenResolver = null;
        internal static TokenResolver GetTokenResolver(object resolver) {
            if(getTokenResolver == null) {
                var pResolver = Expression.Parameter(typeof(object), "resolver");
                getTokenResolver = Expression.Lambda<Func<object, TokenResolver>>(
                    Expression.Convert(
                        Expression.Call(
                            mInfo_CreateDelegate,
                                Expression.Constant(typeof(TokenResolver), typeof(Type)),
                                Expression.Convert(pResolver, DynamicResolverType),
                                Expression.Constant(mInfo_ResolveToken, typeof(MethodInfo))),
                        typeof(TokenResolver)),
                    pResolver).Compile();
            }
            return getTokenResolver(resolver);
        }
        static Func<object, StringResolver> getStringResolver = null;
        internal static StringResolver GetStringResolver(object resolver) {
            if(getStringResolver == null) {
                var pResolver = Expression.Parameter(typeof(object), "resolver");
                getStringResolver = Expression.Lambda<Func<object, StringResolver>>(
                    Expression.Convert(
                        Expression.Call(
                            mInfo_CreateDelegate,
                                Expression.Constant(typeof(StringResolver), typeof(Type)),
                                Expression.Convert(pResolver, DynamicResolverType),
                                Expression.Constant(mInfo_GetStringLiteral, typeof(MethodInfo))),
                        typeof(StringResolver)),
                    pResolver).Compile();
            }
            return getStringResolver(resolver);
        }
        static Func<object, SignatureResolver> getSignatureResolver = null;
        internal static SignatureResolver GetSignatureResolver(object resolver) {
            if(getSignatureResolver == null) {
                var pResolver = Expression.Parameter(typeof(object), "resolver");
                getSignatureResolver = Expression.Lambda<Func<object, SignatureResolver>>(
                    Expression.Convert(
                        Expression.Call(
                            mInfo_CreateDelegate,
                                Expression.Constant(typeof(SignatureResolver), typeof(Type)),
                                Expression.Convert(pResolver, DynamicResolverType),
                                Expression.Constant(mInfo_ResolveSignature, typeof(MethodInfo))),
                        typeof(SignatureResolver)),
                    pResolver).Compile();
            }
            return getSignatureResolver(resolver);
        }
        //
        internal delegate Type TypeFromHandleUnsafe(IntPtr handle);
        static readonly MethodInfo mInfo_GetTypeFromHandleUnsafe = typeof(Type).GetMethod("GetTypeFromHandleUnsafe", BF.Static | BF.NonPublic);
        static Func<TypeFromHandleUnsafe> getTypeFromHandle = null;
        internal static TypeFromHandleUnsafe GetTypeFromHandleUnsafe() {
            if(getTypeFromHandle == null) {
                var createDelegateMethod = typeof(Delegate).GetMethod("CreateDelegate",
                    new Type[] { typeof(Type), typeof(MethodInfo) });
                getTypeFromHandle = Expression.Lambda<Func<TypeFromHandleUnsafe>>(
                    Expression.Convert(
                        Expression.Call(
                            createDelegateMethod,
                                Expression.Constant(typeof(TypeFromHandleUnsafe), typeof(Type)),
                                Expression.Constant(mInfo_GetTypeFromHandleUnsafe, typeof(MethodInfo))),
                        typeof(TypeFromHandleUnsafe))
                    ).Compile();
            }
            return getTypeFromHandle();
        }
        //
        static readonly Assembly RuntimeTypeAssembly = typeof(RuntimeTypeHandle).Assembly;
        static readonly Type RuntimeTypeType = RuntimeTypeAssembly.GetType("System.RuntimeType");
        //
        static readonly Type RuntimeMethodHandleInternalType = RuntimeTypeAssembly.GetType("System.RuntimeMethodHandleInternal");
        static readonly MethodInfo mInfo_GetMethodBase = RuntimeTypeType.GetMethod("GetMethodBase", BF.Static | BF.NonPublic,
            null, new[] { RuntimeTypeType, RuntimeMethodHandleInternalType }, null);
        static readonly ConstructorInfo ctor_RuntimeMethodHandleInternal = RuntimeMethodHandleInternalType.GetConstructor(BF.Instance | BF.NonPublic,
            null, new[] { typeof(IntPtr) }, null);
        //
        static readonly Type RuntimeFieldInfoStubType = RuntimeTypeAssembly.GetType("System.RuntimeFieldInfoStub");
        static readonly MethodInfo mInfo_GetFieldInfo = RuntimeTypeType.GetMethod("GetFieldInfo", BF.Static | BF.NonPublic,
            null, new[] { RuntimeTypeType, RuntimeTypeAssembly.GetType("System.IRuntimeFieldInfo") }, null);
        static readonly ConstructorInfo ctor_RuntimeFieldInfoStub = RuntimeFieldInfoStubType.GetConstructor(BF.Instance | BF.Public,
            null, new[] { typeof(IntPtr), typeof(object) }, null);

        //
        internal delegate MethodBase MethodFromHandles(Type type, IntPtr methodHandle);
        static Func<Type, IntPtr, MethodBase> getMethodFromHandles = null;
        internal static MethodFromHandles GetMethodFromHandles() {
            if(getMethodFromHandles == null) {
                var pType = Expression.Parameter(typeof(Type), "type");
                var pMethodHandle = Expression.Parameter(typeof(IntPtr), "methodHandle");
                getMethodFromHandles = Expression.Lambda<Func<Type, IntPtr, MethodBase>>(
                                Expression.Call(
                                    mInfo_GetMethodBase,
                                    Expression.Convert(pType, RuntimeTypeType),
                                    Expression.New(ctor_RuntimeMethodHandleInternal, pMethodHandle)),
                                pType, pMethodHandle
                           ).Compile();
            }
            return (type, mHandle) => getMethodFromHandles(type, mHandle);
        }
        internal delegate FieldInfo FieldFromHandles(Type type, IntPtr fieldHandle);
        static Func<Type, IntPtr, FieldInfo> getFieldFromHandles = null;
        internal static FieldFromHandles GetFieldFromHandles() {
            if(getFieldFromHandles == null) {
                var pType = Expression.Parameter(typeof(Type), "type");
                var pFieldHandle = Expression.Parameter(typeof(IntPtr), "fieldHandle");
                getFieldFromHandles = Expression.Lambda<Func<Type, IntPtr, FieldInfo>>(
                                Expression.Call(
                                    mInfo_GetFieldInfo,
                                    Expression.Convert(pType, RuntimeTypeType),
                                    Expression.New(ctor_RuntimeFieldInfoStub, pFieldHandle, Expression.Constant(null, typeof(object)))),
                                pType, pFieldHandle
                           ).Compile();
            }
            return (type, fHandle) => getFieldFromHandles(type, fHandle);
        }
        //
        public static void TryPrepareMethod(MethodBase method) {
            try {
                if(method.IsGenericMethod || method.IsGenericMethodDefinition)
                    return;
                var mInfo = method as MethodInfo;
                if(mInfo != null) {
                    if(mInfo.ReturnType.IsGenericParameter || mInfo.ReturnType.IsGenericTypeDefinition)
                        return;
                    var methodParams = method.GetParameters();
                    for(int i = 0; i < methodParams.Length; i++) {
                        if(methodParams[i].ParameterType.IsGenericParameter ||
                            methodParams[i].ParameterType.IsGenericTypeDefinition)
                            return;
                    }
                }
                RuntimeHelpers.PrepareMethod(method.MethodHandle);
            }
            catch { }
        }
    }
}