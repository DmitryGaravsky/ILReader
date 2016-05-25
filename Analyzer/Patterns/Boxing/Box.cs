namespace ILReader.Analyzer {
    using System.Reflection.Emit;

    public sealed class Box : ILPattern {
        public readonly static ILPattern Instance = new Box();
        //
        Box() : base(i => i.OpCode == OpCodes.Box) { }
    }
}