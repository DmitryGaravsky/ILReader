namespace ILReader.Analyzer {
    using System.Reflection;
    using System.Reflection.Emit;

    public sealed class Subscribe : ILPattern {
        public readonly static ILPattern Instance = new Subscribe();
        //
        Subscribe()
            : base(
            i => i.OpCode == OpCodes.Ldftn,
            i => i.OpCode == OpCodes.Newobj && EventAnalyzer.IsDelegate(i.Operand as ConstructorInfo),
            i => i.OpCode == OpCodes.Callvirt && EventAnalyzer.IsAddEvent(i.Operand as MethodInfo)) {
        }
    }
}