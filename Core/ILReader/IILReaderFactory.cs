using System.Reflection;
namespace ILReader.Readers {
    public interface IILReaderFactory {
        IILReader CreateReader();
    }
    //
    class MethodILReaderFactory : IILReaderFactory {
        readonly IILReader reader;
        public MethodILReaderFactory(MethodInfo method) {
            var mBody = method.GetMethodBody();
            if(mBody != null) {
                var binaryReader = new BinaryReader(mBody.GetILAsByteArray());
                var context = new Context.OperandReaderContext(method);
                reader = new InstructionReader(binaryReader, context);
            }
            else reader = InstructionReader.Empty;
        }
        IILReader IILReaderFactory.CreateReader() {
            return reader;
        }
    }
}