namespace ILReader.Analyzer {
    using System.Reflection.Emit;

    public sealed class Unbox : ILPattern {
        public readonly static ILPattern Instance = new Unbox();
        //
        Unbox() : base(i => i.OpCode == OpCodes.Unbox || i.OpCode == OpCodes.Unbox_Any) { }
    }
}