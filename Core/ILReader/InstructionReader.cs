namespace ILReader.Readers {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection.Emit;

    class InstructionReader : IILReader, Dump.ISupportDump {
        readonly LazyRef<string> name;
        readonly LazyRef<IEnumerable<IMetadataItem>> metadata;
        readonly LazyRef<IInstruction[]> instructions;
        readonly LazyRef<Action<Stream>> writeDump;
        public InstructionReader(IBinaryReader binaryReader, Context.IOperandReaderContext context) {
            name = new LazyRef<string>(() => GetName(context));
            metadata = new LazyRef<IEnumerable<IMetadataItem>>(() => GetMetadata(context));
            instructions = new LazyRef<IInstruction[]>(() => GetInstructions(binaryReader, context).ToArray());
            writeDump = new LazyRef<Action<Stream>>(() => (stream) => WriteDump(context, stream));
        }
        string IILReader.Name {
            get { return name.Value; }
        }
        IEnumerable<IMetadataItem> IILReader.Metadata {
            get { return metadata.Value; }
        }
        protected virtual string GetName(Context.IOperandReaderContext context) {
            return (context.Type == Context.OperandReaderContextType.Method) ? context.Name :
                context.Name + "(" + context.Type.ToString() + ")";
        }
        protected virtual IEnumerable<IMetadataItem> GetMetadata(Context.IOperandReaderContext context) {
            return context.GetMetadata();
        }
        void IILReader.CopyTo(IInstruction[] array, int index) {
            instructions.Value.CopyTo(array, index);
        }
        IInstruction IILReader.this[int index] {
            get { return GetInstruction(instructions.Value, index); }
        }
        int IILReader.Count {
            get { return instructions.Value.Length; }
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
        protected virtual void WriteDump(Context.IOperandReaderContext context, Stream stream) {
            Dump.InstructionReaderDump.Write(stream, context);
        }
        //
        sealed class Instruction : IInstruction {
            readonly LazyRef<byte[]> bytes;
            readonly object rawOperand;
            readonly short? argIndex, locIndex;
            internal Instruction(int index, IBinaryReader binaryReader, Context.IOperandReaderContext context) {
                this.Index = index;
                this.Offset = binaryReader.Offset;
                this.OpCode = OpCodeReader.ReadOpCode(binaryReader);
                // Operand
                bool argumentAware = OperandReader.IsArgumentAware(OpCode);
                if(argumentAware) {
                    this.Operand = OperandReader.ReadArg(binaryReader, context, OpCode.OperandType);
                    argIndex = OperandReader.GetArgIndex(OpCode, binaryReader);
                    if(argIndex.Value > 0)
                        this.rawOperand = context[(short)(argIndex.Value - 1), true];
                    else
                        this.rawOperand = context.This;
                }
                bool localAware = OperandReader.IsLocalAware(OpCode);
                if(localAware) {
                    this.Operand = OperandReader.Read(binaryReader, context, OpCode.OperandType);
                    locIndex = OperandReader.GetLocalIndex(OpCode, binaryReader);
                    if(Operand == null)
                        this.rawOperand = context[locIndex.Value, false];
                }
                if(!localAware && !argumentAware)
                    this.Operand = OperandReader.Read(binaryReader, context, OpCode.OperandType);
                //
                int size = binaryReader.Offset - Offset;
                this.bytes = new LazyRef<byte[]>(() => binaryReader.Read(Offset, size));
            }
            public int Index {
                get;
                private set;
            }
            public int Offset {
                get;
                private set;
            }
            public OpCode OpCode {
                get;
                private set;
            }
            public object Operand {
                get;
                private set;
            }
            public byte[] Bytes {
                get { return bytes.Value; }
            }
            public string Text {
                get { return ToString(); }
            }
            public sealed override string ToString() {
                if(object.ReferenceEquals(Operand, null)) {
                    if(rawOperand != null) {
                        if(argIndex.HasValue) {
                            if(argIndex.Value > 0)
                                return string.Format("IL_{0:X4}: {1}   (@arg.{2} {3})", Offset, OpCode.ToString(), argIndex.Value.ToString(), rawOperand.ToString());
                            else
                                return string.Format("IL_{0:X4}: {1}   (@this {2})", Offset, OpCode.ToString(), rawOperand.ToString());
                        }
                        return string.Format("IL_{0:X4}: {1} ({2})", Offset, OpCode.ToString(), rawOperand.ToString());
                    }
                    else return string.Format("IL_{0:X4}: {1}", Offset, OpCode.ToString());
                }
                else {
                    string suffix = string.Empty;
                    if(argIndex.HasValue)
                        suffix = string.Format(" (@arg.{0} {1})", argIndex.Value.ToString(), (rawOperand ?? Operand).ToString());
                    if(locIndex.HasValue)
                        suffix = string.Format(" (@loc.{0} {1})", locIndex.Value.ToString(), (rawOperand ?? Operand).ToString());
                    //
                    return string.Format("IL_{0:X4}: {1} {2}", Offset, OpCode.ToString(), Operand.ToString()) + suffix;
                }
            }
        }
        #region Empty
        internal static readonly IILReader Empty = new InstructionReaderEmpty();
        sealed class InstructionReaderEmpty : InstructionReader {
            internal InstructionReaderEmpty()
                : base(null, null) {
            }
            protected override string GetName(Context.IOperandReaderContext context) {
                return string.Empty;
            }
            protected override IEnumerable<IMetadataItem> GetMetadata(Context.IOperandReaderContext context) {
                yield break;
            }
            protected override IEnumerable<IInstruction> GetInstructions(IBinaryReader binaryReader, Context.IOperandReaderContext context) {
                yield break;
            }
            protected override void WriteDump(Context.IOperandReaderContext context, Stream stream) {
                /* do nothing */
            }
        }
        #endregion
        #region Dump
        void Dump.ISupportDump.Dump(Stream stream) {
            IInstruction[] arr = instructions.Value;
            if(arr != null && arr.Length >= 0)
                writeDump.Value(stream);
        }
        #endregion
    }
}