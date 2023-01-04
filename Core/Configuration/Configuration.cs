namespace ILReader {
    using System.Collections.Concurrent;
    using System.ComponentModel;
    using System.Reflection;
    using System.Reflection.Emit;
    using ILReader.Monads;
    using ILReader.Readers;

    public static class Configuration {
        public static IILReaderConfiguration Standard {
            get { return StandardConfiguration.Default; }
        }
        public static IILReaderConfiguration Resolve(System.IO.Stream dump) {
            return DumpConfiguration.Default;
        }
        public static IILReaderConfiguration Resolve(MethodBase method) {
            if(method is DynamicMethod)
                return DynamicMethodConfiguration.Default;
            if(Equals(method.@Get(m => m.GetType()), RTTypes.RTDynamicMethodType))
                return RTDynamicMethodConfiguration.Default;
            return StandardConfiguration.Default;
        }
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void DisableUsingRuntimeHelpersPrepareMethod() {
            RTTypes.DisableUsingRuntimeHelpersPrepareMethod();
        }
    }
    //
    abstract class ConfigurationBase : IILReaderConfiguration {
        readonly static ConcurrentDictionary<MethodBase, IILReader> readers = new ConcurrentDictionary<MethodBase, IILReader>();
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
        protected virtual IILReader CreateReader(System.IO.Stream dump) {
            var factory = CreateILReaderFactory(dump);
            return factory.CreateReader();
        }
        protected virtual IILReaderFactory CreateILReaderFactory(System.IO.Stream dump) {
            return new ILReaderFactory(dump, this);
        }
        protected virtual IILReaderFactory CreateILReaderFactory(MethodBase methodBase) {
            return new ILReaderFactory(methodBase, this);
        }
        protected virtual IBinaryReader CreateBinaryReader(byte[] bytes) {
            return new BinaryReader(bytes);
        }
        //
        protected abstract Context.IOperandReaderContext CreateOperandReaderContext(MethodBase methodBase);
        protected abstract Context.IOperandReaderContext CreateOperandReaderContext(System.IO.Stream dump);
        #region IILReaderConfiguration
        IILReader IILReaderConfiguration.GetReader(System.IO.Stream dump) {
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
        IBinaryReader IILReaderConfiguration.CreateBinaryReader(byte[] bytes) {
            return CreateBinaryReader(bytes);
        }
        Context.IOperandReaderContext IILReaderConfiguration.CreateOperandReaderContext(MethodBase methodBase) {
            return CreateOperandReaderContext(methodBase);
        }
        Context.IOperandReaderContext IILReaderConfiguration.CreateOperandReaderContext(System.IO.Stream dump) {
            return CreateOperandReaderContext(dump);
        }
        #endregion
    }
    //
    abstract class RealConfiguration : ConfigurationBase {
        protected override Context.IOperandReaderContext CreateOperandReaderContext(System.IO.Stream dump) {
            throw new System.NotImplementedException();
        }
    }
    sealed class StandardConfiguration : RealConfiguration {
        protected sealed override Context.IOperandReaderContext CreateOperandReaderContext(MethodBase methodBase) {
            return new Context.OperandReaderContext(methodBase, methodBase.GetMethodBody());
        }
        static readonly internal IILReaderConfiguration Default = new StandardConfiguration();
    }
    sealed class DynamicMethodConfiguration : RealConfiguration {
        protected sealed override Context.IOperandReaderContext CreateOperandReaderContext(MethodBase methodBase) {
            return new Context.OperandReaderContext_DynamicMethod((DynamicMethod)methodBase);
        }
        static readonly internal IILReaderConfiguration Default = new DynamicMethodConfiguration();
    }
    sealed class RTDynamicMethodConfiguration : RealConfiguration {
        protected sealed override Context.IOperandReaderContext CreateOperandReaderContext(MethodBase methodBase) {
            var ownerMethod = RTTypes.GetOwnerDynamicMethod(methodBase);
            return new Context.OperandReaderContext_DynamicMethod(ownerMethod);
        }
        static readonly internal IILReaderConfiguration Default = new RTDynamicMethodConfiguration();
    }
    //
    sealed class DumpConfiguration : ConfigurationBase {
        protected sealed override Context.IOperandReaderContext CreateOperandReaderContext(MethodBase methodBase) {
            throw new System.NotImplementedException();
        }
        protected sealed override Context.IOperandReaderContext CreateOperandReaderContext(System.IO.Stream dump) {
            var ilReaderDump = new Dump.InstructionReaderDump(dump);
            return new Context.OperandReaderContextDump(ilReaderDump);
        }
        static readonly internal IILReaderConfiguration Default = new DumpConfiguration();
    }
}