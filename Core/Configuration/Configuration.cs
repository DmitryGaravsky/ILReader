namespace ILReader {
    using System.Collections.Generic;
    using System.Reflection;
    using ILReader.Readers;

    public static class Configuration {
        public static IILReaderConfiguration Standard {
            get { return StandardConfiguration.Default; }
        }
    }
    //
    class StandardConfiguration : IILReaderConfiguration {
        static Dictionary<MethodBase, IILReader> readers = new Dictionary<MethodBase, IILReader>();
        protected virtual void ResetReader(MethodBase methodBase) {
            readers.Remove(methodBase);
        }
        protected virtual void ResetReaders() {
            readers.Clear();
        }
        protected virtual IILReader GetOrCreateReader(MethodBase methodBase) {
            IILReader reader;
            if(!readers.TryGetValue(methodBase, out reader)) {
                var factory = CreateILReaderFactory(methodBase);
                reader = factory.CreateReader();
                readers.Add(methodBase, reader);
            }
            return reader;
        }
        protected virtual IILReaderFactory CreateILReaderFactory(MethodBase methodBase) {
            return new MethodBaseILReaderFactory(methodBase, this);
        }
        protected virtual IBinaryReader CreateBinaryReader(byte[] bytes) {
            return new BinaryReader(bytes);
        }
        protected virtual Context.IOperandReaderContext CreateOperandReaderContext(MethodBase methodBase, MethodBody methodBody) {
            return new Context.OperandReaderContext(methodBase, methodBody);
        }
        #region IILReaderConfiguration
        readonly static object syncObj = new object();
        IILReader IILReaderConfiguration.GetReader(MethodBase methodBase) {
            lock(syncObj) return GetOrCreateReader(methodBase);
        }
        void IILReaderConfiguration.Reset(MethodBase methodBase) {
            lock(syncObj) ResetReader(methodBase);
        }
        void IILReaderConfiguration.Reset() {
            lock(syncObj) ResetReaders();
        }
        IBinaryReader IILReaderConfiguration.CreateBinaryReader(byte[] bytes) {
            return CreateBinaryReader(bytes);
        }
        Context.IOperandReaderContext IILReaderConfiguration.CreateOperandReaderContext(MethodBase methodBase, MethodBody methodBody) {
            return CreateOperandReaderContext(methodBase, methodBody);
        }
        #endregion
        #region Default
        static readonly internal IILReaderConfiguration Default = new StandardConfiguration();
        #endregion Default
    }
}