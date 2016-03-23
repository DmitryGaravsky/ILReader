namespace ILReader.Readers {
    using System;
    using System.Collections.Generic;

    public interface IILReader : IEnumerable<IInstruction> {
        IInstruction this[int index] { get; }
        IInstruction this[IInstruction instruction, int offset] { get; }
        IInstruction FindPrev(IInstruction instruction, Predicate<IInstruction> match);
        IInstruction FindNext(IInstruction instruction, Predicate<IInstruction> match);
    }
    //
    public interface IInstruction {
        int Index { get; }
        int Offset { get; }
        string Text { get; }
        //
        System.Reflection.Emit.OpCode OpCode { get; }
        object Operand { get; }
    }
}