namespace ILReader.Analyzer {
    using System.Reflection;
    using System.Reflection.Emit;

    public sealed class NotSupported : ILPattern {
        public readonly static ILPattern Instance = new NotSupported();
        //
        NotSupported()
            : base(
            i => i.OpCode == OpCodes.Ldstr,
            i => i.OpCode == OpCodes.Newobj && ExceptionAnalizer.IsException<System.NotSupportedException>(i.Operand as ConstructorInfo),
            i => i.OpCode == OpCodes.Throw) {
        }
    }
}