namespace ILReader.Readers {
    using System.IO;
    using System.Reflection;

    sealed class ILReaderFactory : IILReaderFactory {
        readonly LazyRef<IILReader> reader;
        public ILReaderFactory(MethodBase method, IILReaderConfiguration configuration) {
            reader = method.IsAbstract ?
                new LazyRef<IILReader>(() => InstructionReader.Empty) :
                new LazyRef<IILReader>(() => CreateInstructionReader(method, configuration));
        }
        public ILReaderFactory(Stream dump, IILReaderConfiguration configuration) {
            reader = (dump == null || dump.Length < (sizeof(byte) + sizeof(int) * 2)) ?
                new LazyRef<IILReader>(() => InstructionReader.Empty) :
                new LazyRef<IILReader>(() => CreateInstructionReader(dump, configuration));
        }
        IILReader CreateInstructionReader(MethodBase method, IILReaderConfiguration configuration) {
            var context = configuration.CreateOperandReaderContext(method);
            var binaryReader = configuration.CreateBinaryReader(context.GetIL());
            return CreateInstructionReader(binaryReader, context);
        }
        IILReader CreateInstructionReader(Stream dump, IILReaderConfiguration configuration) {
            var context = configuration.CreateOperandReaderContext(dump);
            var binaryReader = configuration.CreateBinaryReader(context.GetIL());
            return CreateInstructionReader(binaryReader, context);
        }
        IILReader CreateInstructionReader(IBinaryReader binaryReader, Context.IOperandReaderContext context) {
            return new InstructionReader(binaryReader, context);
        }
        //
        IILReader IILReaderFactory.CreateReader() {
            return reader.Value;
        }
    }
}