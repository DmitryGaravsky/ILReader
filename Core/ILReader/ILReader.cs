namespace ILReader.Readers {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection.Emit;

    class InstructionReader : IILReader {
        internal static readonly IILReader Empty = new InstructionReaderEmpty();
        readonly Lazy<IInstruction[]> instructions;
        public InstructionReader(IBinaryReader binaryReader, Context.IOperandReaderContext context) {
            instructions = new Lazy<IInstruction[]>(() => GetInstructions(binaryReader, context).ToArray());
        }
        IInstruction IILReader.this[int index] {
            get { return GetInstruction(instructions.Value, index); }
        }
        IInstruction IILReader.this[IInstruction instruction, int offset] {
            get { return GetInstruction(instructions.Value, instruction.Index + offset); }
        }
        IInstruction IILReader.FindPrev(IInstruction instruction, Predicate<IInstruction> match) {
            IInstruction[] arr = instructions.Value;
            int index = (instruction == null) ? arr.Length : instruction.Index;
            return GetInstruction(arr, Array.FindLastIndex(arr, index - 1, index, match));
        }
        IInstruction IILReader.FindNext(IInstruction instruction, Predicate<IInstruction> match) {
            IInstruction[] arr = instructions.Value;
            int index = (instruction == null) ? 0 : instruction.Index;
            return GetInstruction(arr, Array.FindIndex(arr, index + 1, arr.Length - index - 1, match));
        }
        IInstruction GetInstruction(IInstruction[] array, int index) {
            return (index >= 0 && index < array.Length) ? array[index] : null;
        }
        IEnumerator<IInstruction> IEnumerable<IInstruction>.GetEnumerator() {
            return ((IEnumerable<IInstruction>)instructions.Value).GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator() {
            return instructions.Value.GetEnumerator();
        }
        protected virtual IEnumerable<IInstruction> GetInstructions(IBinaryReader binaryReader, Context.IOperandReaderContext context) {
            int index = 0;
            while(binaryReader.CanRead())
                yield return new Instruction(index++, binaryReader, context);
        }
        sealed class Instruction : IInstruction {
            internal Instruction(int index, IBinaryReader binaryReader, Context.IOperandReaderContext context) {
                this.Index = index;
                this.Offset = binaryReader.Offset;
                this.OpCode = OpCodeReader.ReadOpCode(binaryReader);
                this.Operand = OperandReader.Read(binaryReader, context, OpCode.OperandType);
            }
            public int Index { get; private set; }
            public int Offset { get; private set; }
            public OpCode OpCode { get; private set; }
            public object Operand { get; private set; }
            public override string ToString() {
                if(object.ReferenceEquals(Operand, null))
                    return string.Format("{0:X5}:  {1}", Offset, OpCode);
                else
                    return string.Format("{0:X5}:  {1} {2}", Offset, OpCode, Operand);
            }
        }
        sealed class InstructionReaderEmpty : InstructionReader {
            public InstructionReaderEmpty()
                : base(null, null) {
            }
            protected override IEnumerable<IInstruction> GetInstructions(IBinaryReader binaryReader, Context.IOperandReaderContext context) {
                yield break;
            }        }
    }
}