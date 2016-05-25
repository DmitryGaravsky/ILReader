namespace ILReader.Analyzer {
    using System.Reflection.Emit;

    public sealed class Nop : ILPattern {
        public readonly static ILPattern Instance = new Nop();
        //
        Nop() : base(i => i.OpCode == OpCodes.Nop) { }
    }
}