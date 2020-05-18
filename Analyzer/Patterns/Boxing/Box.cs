namespace ILReader.Analyzer {
    using System;
    using System.Reflection.Emit;

    public sealed class Box : ILPattern {
        internal readonly static Func<Readers.IInstruction, bool> MatchFunc =
            new Func<Readers.IInstruction, bool>(IsValueTypeBox);
        //
        public readonly static ILPattern Instance = new Box();
        //
        Box() : base(MatchFunc) { }
        //
        static bool IsValueTypeBox(Readers.IInstruction instruction) {
            return (instruction.OpCode == OpCodes.Box) && IsValueType(instruction.Operand as Type);
        }
        static bool IsValueType(Type type) {
            return (type != null) && type.IsValueType;
        }
    }
}