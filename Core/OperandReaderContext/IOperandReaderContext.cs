namespace ILReader.Context {
    using System.Reflection;

    interface IOperandReaderContext {
        Module Module { get; }
        LocalVariableInfo[] Variables { get; }
    }
}