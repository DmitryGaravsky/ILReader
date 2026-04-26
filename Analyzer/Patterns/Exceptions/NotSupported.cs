namespace ILReader.Analyzer {
    using System.Reflection;
    using System.Reflection.Emit;

    public sealed class NotSupported : ILPattern {
        public readonly static ILPattern Instance = new NotSupported();
        //
        NotSupported()
            : base(
            i => i.OpCode == OpCodes.Ldstr,
            i => i.OpCode == OpCodes.Newobj && ExceptionAnalyzer.IsException<System.NotSupportedException>(i.Operand.GetSource<ConstructorInfo>()),
            i => i.OpCode == OpCodes.Throw) {
        }
    }
}