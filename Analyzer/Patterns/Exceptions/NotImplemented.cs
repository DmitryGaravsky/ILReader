namespace ILReader.Analyzer {
    using System.Reflection;
    using System.Reflection.Emit;

    public sealed class NotImplemented : ILPattern {
        public readonly static ILPattern Instance = new NotImplemented();
        //
        NotImplemented()
            : base(
            i => i.OpCode == OpCodes.Ldstr,
            i => i.OpCode == OpCodes.Newobj && ExceptionAnalizer.IsException<System.NotImplementedException>(i.Operand as ConstructorInfo),
            i => i.OpCode == OpCodes.Throw) {
        }
    }
}