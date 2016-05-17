namespace ILReader.Analyzer {
    using System.Reflection;
    using System.Reflection.Emit;

    public sealed class Unsubscribe : ILPattern {
        public readonly static ILPattern Instance = new Unsubscribe();
        //
        Unsubscribe()
            : base(
            i => i.OpCode == OpCodes.Ldftn,
            i => i.OpCode == OpCodes.Newobj && EventAnalyzer.IsDelegate(i.Operand as ConstructorInfo),
            i => i.OpCode == OpCodes.Callvirt && EventAnalyzer.IsRemoveEvent(i.Operand as MethodInfo)) {
        }
    }
}