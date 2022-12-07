namespace ILReader.Readers {
    using System.Collections.Generic;
    using System.Reflection.Emit;

    public interface IILReader : IEnumerable<IInstruction> {
        void CopyTo(IInstruction[] array, int index);
        IInstruction this[int index] { get; }
        int Count { get; }
        string Name { get; }
        ExceptionHandler[] ExceptionHandlers { get; }
        IEnumerable<IMetadataItem> Metadata { get; }
    }
    //
    public interface IInstruction {
        int Index { get; }
        int Offset { get; }
        string Text { get; }
        OpCode OpCode { get; }
        object Operand { get; }
        int Depth { get; }
        byte[] Bytes { get; }
    }
    //
    public interface IMetadataItem {
        string Name { get; }
        object Value { get; }
        bool HasChildren { get; }
        IEnumerable<IMetadataItem> Children { get; }
    }
}