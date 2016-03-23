namespace ILReader.Readers {
    using System;
    using System.Reflection;

    class MethodBaseILReaderFactory : IILReaderFactory {
        readonly Lazy<IILReader> reader;
        public MethodBaseILReaderFactory(MethodBase method, IILReaderConfiguration configuration) {
            var mBody = method.GetMethodBody();
            reader = (mBody == null) ?
                new Lazy<IILReader>(() => InstructionReader.Empty) :
                new Lazy<IILReader>(() => CreateInstructionReader(method, mBody, configuration));
        }
        IILReader CreateInstructionReader(MethodBase method, MethodBody mBody, IILReaderConfiguration configuration) {
            var binaryReader = configuration.CreateBinaryReader(mBody.GetILAsByteArray());
            var context = configuration.CreateOperandReaderContext(method, mBody);
            return CreateInstructionReader(binaryReader, context);
        }
        IILReader CreateInstructionReader(IBinaryReader binaryReader, Context.IOperandReaderContext context) {
            return new InstructionReader(binaryReader, context);
        }
        IILReader IILReaderFactory.CreateReader() {
            return reader.Value;
        }
    }
}