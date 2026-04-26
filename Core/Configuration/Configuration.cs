namespace ILReader {
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Reflection;
    using System.Reflection.Emit;
    using ILReader.Context;
    using ILReader.Monads;
    using ILReader.Readers;

    public static class Configuration {
        public static IILReaderConfiguration Standard {
            get { return StandardConfiguration.Default; }
        }
        public static IILReaderConfiguration Resolve(Stream dump) {
            return DumpConfiguration.Default;
        }
        public static IILReaderConfiguration Resolve(MethodBase method) {
            if(method is DynamicMethod)
                return DynamicMethodConfiguration.Default;
            if(Equals(method.@Get(m => m.GetType()), RTTypes.RTDynamicMethodType))
                return RTDynamicMethodConfiguration.Default;
            return StandardConfiguration.Default;
        }
        public static IILReaderConfiguration ForAssembly(Stream peStream) {
            return new SrmConfiguration(peStream);
        }
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void DisableUsingRuntimeHelpersPrepareMethod() {
            RTTypes.DisableUsingRuntimeHelpersPrepareMethod();
        }
    }
    //
    abstract class ConfigurationBase : IILReaderConfiguration {
        readonly static ConcurrentDictionary<MethodBase, IILReader> readers =
            new ConcurrentDictionary<MethodBase, IILReader>();
        protected virtual IILReader ResetReader(MethodBase methodBase) {
            IILReader reader;
            return readers.TryRemove(methodBase, out reader) ? reader : null;
        }
        protected virtual void ResetReaders() {
            readers.Clear();
        }
        protected virtual IILReader GetOrCreateReader(MethodBase methodBase) {
            return readers.GetOrAdd(methodBase, m => {
                var factory = CreateILReaderFactory(methodBase);
                return factory.CreateReader();
            });
        }
        protected virtual IILReader CreateReader(Stream dump) {
            var factory = CreateILReaderFactory(dump);
            return factory.CreateReader();
        }
        protected virtual IILReaderFactory CreateILReaderFactory(Stream dump) {
            return new ILReaderFactory(dump, this);
        }
        protected virtual IILReaderFactory CreateILReaderFactory(MethodBase methodBase) {
            return new ILReaderFactory(methodBase, this);
        }
        protected virtual ILBytesReader CreateILBytesReader(byte[] bytes) {
            return new ILBytesReader(bytes);
        }
        //
        protected abstract IOperandReaderContext CreateOperandReaderContext(MethodBase methodBase);
        protected abstract IOperandReaderContext CreateOperandReaderContext(Stream dump);
        #region IILReaderConfiguration
        IILReader IILReaderConfiguration.GetReader(Stream dump) {
            return CreateReader(dump);
        }
        IILReader IILReaderConfiguration.GetReader(MethodBase methodBase) {
            return GetOrCreateReader(methodBase);
        }
        void IILReaderConfiguration.Reset(MethodBase methodBase) {
            ResetReader(methodBase);
        }
        void IILReaderConfiguration.Reset() {
            ResetReaders();
        }
        ILBytesReader IILReaderConfiguration.CreateBytesReader(byte[] bytes) {
            return CreateILBytesReader(bytes);
        }
        IOperandReaderContext IILReaderConfiguration.CreateOperandReaderContext(MethodBase methodBase) {
            return CreateOperandReaderContext(methodBase);
        }
        IOperandReaderContext IILReaderConfiguration.CreateOperandReaderContext(Stream dump) {
            return CreateOperandReaderContext(dump);
        }
        IILReader IILReaderConfiguration.GetReader(int metadataToken) {
            return GetReaderByToken(metadataToken);
        }
        IILReader IILReaderConfiguration.GetReader(string typeName, string methodName) {
            return GetReaderByName(typeName, methodName);
        }
        IILReader IILReaderConfiguration.GetReader(string typeName, string methodName, string signature) {
            return GetReaderByNameAndSignature(typeName, methodName, signature);
        }
        IEnumerable<IMethodSymbol> IILReaderConfiguration.FindMethods(string typeName, string methodName) {
            return GetFindMethods(typeName, methodName);
        }
        protected virtual IILReader GetReaderByToken(int metadataToken) {
            throw new NotSupportedException("Token-based lookup requires SrmConfiguration.");
        }
        protected virtual IILReader GetReaderByName(string typeName, string methodName) {
            throw new NotSupportedException("Name-based lookup requires SrmConfiguration.");
        }
        protected virtual IILReader GetReaderByNameAndSignature(string typeName, string methodName, string signature) {
            throw new NotSupportedException("Signature-based lookup requires SrmConfiguration.");
        }
        protected virtual IEnumerable<IMethodSymbol> GetFindMethods(string typeName, string methodName) {
            throw new NotSupportedException("FindMethods requires SrmConfiguration.");
        }
        #endregion
    }
    //
    abstract class RealConfiguration : ConfigurationBase {
        protected override IOperandReaderContext CreateOperandReaderContext(Stream dump) {
            throw new NotImplementedException();
        }
    }
    sealed class StandardConfiguration : RealConfiguration {
        protected sealed override IOperandReaderContext CreateOperandReaderContext(MethodBase methodBase) {
            return new OperandReaderContext(methodBase, methodBase.GetMethodBody());
        }
        static readonly internal IILReaderConfiguration Default = new StandardConfiguration();
    }
    sealed class DynamicMethodConfiguration : RealConfiguration {
        protected sealed override IOperandReaderContext CreateOperandReaderContext(MethodBase methodBase) {
            return new OperandReaderContext_DynamicMethod((DynamicMethod)methodBase);
        }
        static readonly internal IILReaderConfiguration Default = new DynamicMethodConfiguration();
    }
    sealed class RTDynamicMethodConfiguration : RealConfiguration {
        protected sealed override IOperandReaderContext CreateOperandReaderContext(MethodBase methodBase) {
            var ownerMethod = RTTypes.GetOwnerDynamicMethod(methodBase);
            return new OperandReaderContext_DynamicMethod(ownerMethod);
        }
        static readonly internal IILReaderConfiguration Default = new RTDynamicMethodConfiguration();
    }
    //
    sealed class SrmConfiguration : ConfigurationBase, IDisposable {
        readonly SrmModuleContext module;
        readonly ConcurrentDictionary<int, IILReader> tokenCache =
            new ConcurrentDictionary<int, IILReader>();
        readonly ConcurrentDictionary<string, IILReader> nameCache =
            new ConcurrentDictionary<string, IILReader>(StringComparer.Ordinal);
        public SrmConfiguration(Stream peStream) {
            module = new SrmModuleContext(peStream);
        }
        public void Dispose() {
            module.Dispose();
        }
        protected override IOperandReaderContext CreateOperandReaderContext(MethodBase methodBase) {
            throw new NotSupportedException("SrmConfiguration does not support MethodBase lookup.");
        }
        protected override IOperandReaderContext CreateOperandReaderContext(Stream dump) {
            throw new NotSupportedException("SrmConfiguration does not support dump streams.");
        }
        protected override IILReader GetReaderByToken(int metadataToken) {
            return tokenCache.GetOrAdd(metadataToken, t => {
                var context = new SrmOperandReaderContext(module, t);
                var bytesReader = CreateILBytesReader(context.GetIL());
                return ((IILReaderFactory)new ILReaderFactory(bytesReader, context)).CreateReader();
            });
        }
        protected override IILReader GetReaderByName(string typeName, string methodName) {
            string key = typeName + "::" + methodName;
            return nameCache.GetOrAdd(key, _ => {
                var context = new SrmOperandReaderContext(module, typeName, methodName);
                var bytesReader = CreateILBytesReader(context.GetIL());
                return ((IILReaderFactory)new ILReaderFactory(bytesReader, context)).CreateReader();
            });
        }
        protected override IILReader GetReaderByNameAndSignature(string typeName, string methodName, string signature) {
            string key = $"{typeName}::{methodName}::{signature}";
            return nameCache.GetOrAdd(key, _ => {
                var context = new SrmOperandReaderContext(module, typeName, methodName, signature);
                var bytesReader = CreateILBytesReader(context.GetIL());
                return ((IILReaderFactory)new ILReaderFactory(bytesReader, context)).CreateReader();
            });
        }
        protected override IEnumerable<IMethodSymbol> GetFindMethods(string typeName, string methodName) {
            return SrmOperandReaderContext.FindMethods(module.MetadataReader, typeName, methodName);
        }
    }
    //
    sealed class DumpConfiguration : ConfigurationBase {
        protected sealed override IOperandReaderContext CreateOperandReaderContext(MethodBase methodBase) {
            throw new NotImplementedException();
        }
        protected sealed override IOperandReaderContext CreateOperandReaderContext(Stream dump) {
            var ilReaderDump = new Dump.InstructionReaderDump(dump);
            return new OperandReaderContext_Dump(ilReaderDump);
        }
        static readonly internal IILReaderConfiguration Default = new DumpConfiguration();
    }
}