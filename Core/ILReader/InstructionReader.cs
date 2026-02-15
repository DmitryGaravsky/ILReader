namespace ILReader.Readers {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection.Emit;
    using ILReader.Context;
    using ILReader.Dump;

    class InstructionReader : IILReader, ISupportDump {
        readonly LazyRef<string> name;
        readonly LazyRef<IEnumerable<IMetadataItem>> metadata;
        readonly LazyRef<IInstruction[]> instructions;
        readonly LazyRef<ExceptionHandler[]> exceptionHandlers;
        readonly LazyRef<Action<Stream>> writeDump;
        public InstructionReader(IBinaryReader binaryReader, IOperandReaderContext context) {
            name = new LazyRef<string>(() => GetName(context));
            metadata = new LazyRef<IEnumerable<IMetadataItem>>(() => GetMetadata(context));
            instructions = new LazyRef<IInstruction[]>(() => GetInstructions(binaryReader, context).ToArray());
            exceptionHandlers = new LazyRef<ExceptionHandler[]>(() => GetExceptionHandlers(context).ToArray());
            writeDump = new LazyRef<Action<Stream>>(() => (stream) => WriteDump(context, stream));
        }
        string IILReader.Name {
            get { return name.Value; }
        }
        IEnumerable<IMetadataItem> IILReader.Metadata {
            get { return metadata.Value; }
        }
        protected virtual string GetName(IOperandReaderContext context) {
            return (context.Type == OperandReaderContextType.Method) ? context.Name :
                context.Name + "(" + context.Type.ToString() + ")";
        }
        protected virtual IEnumerable<IMetadataItem> GetMetadata(IOperandReaderContext context) {
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
        ExceptionHandler[] IILReader.ExceptionHandlers {
            get { return exceptionHandlers.Value; }
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
        protected virtual IEnumerable<IInstruction> GetInstructions(IBinaryReader binaryReader, IOperandReaderContext context) {
            int index = 0;
            while(binaryReader.CanRead())
                yield return new Instruction(index++, binaryReader, context);
        }
        protected virtual IEnumerable<ExceptionHandler> GetExceptionHandlers(IOperandReaderContext context) {
            ExceptionHandler current;
            while(context.ResolveExceptionHandler(GetGetInstruction(instructions.Value), out current))
                yield return current.Advance(instructions.Value, x => ((Instruction)x).IncreaseDepth());
        }
        static Func<int, IInstruction> GetGetInstruction(IInstruction[] instructionsArray) {
            int index = 0;
            return offset => {
                for(; index < instructionsArray.Length; index++) {
                    if(instructionsArray[index].Offset == offset)
                        return instructionsArray[index];
                }
                return null;
            };
        }
        protected virtual void WriteDump(IOperandReaderContext context, Stream stream) {
            InstructionReaderDump.Write(stream, context, exceptionHandlers.Value);
        }
        sealed class Instruction : IInstruction {
            readonly LazyRef<byte[]> bytes;
            readonly object rawOperand;
            readonly short? argIndex, locIndex;
            readonly OpCodeInfo opCodeInfo;
            internal Instruction(int index, IBinaryReader binaryReader, IOperandReaderContext context) {
                this.Index = index;
                this.Offset = binaryReader.Offset;
                this.opCodeInfo = OpCodeReader.ReadOpCode(binaryReader);
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
                // Local
                bool localAware = OperandReader.IsLocalAware(OpCode);
                if(localAware) {
                    this.Operand = OperandReader.Read(binaryReader, context, OpCode.OperandType);
                    locIndex = OperandReader.GetLocalIndex(OpCode, binaryReader);
                    if(Operand == null)
                        this.rawOperand = context[locIndex.Value, false];
                }
                if(!localAware && !argumentAware)
                    this.Operand = OperandReader.Read(binaryReader, context, OpCode.OperandType);
                // bytes
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
            public int Depth {
                get;
                private set;
            }
            internal void IncreaseDepth() {
                Depth++;
            }
            public OpCode OpCode {
                get { return opCodeInfo.OpCode; }
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
                if(ReferenceEquals(Operand, null)) {
                    if(rawOperand != null) {
                        if(argIndex.HasValue) {
                            if(argIndex.Value > 0)
                                return string.Format("IL_{0}: {1}   (@arg.{2} {3})", Offset.ToString("X4"), OpCode.Name, argIndex.Value.ToString(), GetRawOperandString(OpCode));
                            return string.Format("IL_{0}: {1}   (@this {2})", Offset.ToString("X4"), OpCode.Name, GetRawOperandString(OpCode));
                        }
                        return string.Format("IL_{0}: {1} ({2})", Offset.ToString("X4"), OpCode.Name, GetRawOperandString(OpCode));
                    }
                    return string.Format("IL_{0}: {1}", Offset.ToString("X4"), OpCode.Name);
                }
                else {
                    string suffix = string.Empty;
                    if(argIndex.HasValue)
                        suffix = string.Format(" (@arg.{0} {1})", argIndex.Value.ToString(), GetOperandString(OpCode));
                    if(locIndex.HasValue)
                        suffix = string.Format(" (@loc.{0} {1})", locIndex.Value.ToString(), GetOperandString(OpCode));
                    return string.Format("IL_{0}: {1} {2}", Offset.ToString("X4"), OpCode.Name, GetOperandString(OpCode)) + suffix;
                }
            }
            string GetRawOperandString(OpCode opCode) {
                return GetOperandString(opCode.Value, rawOperand);
            }
            string GetOperandString(OpCode opCode) {
                return GetOperandString(opCode.Value, Operand ?? rawOperand);
            }
            readonly static short ldstr_value = OpCodes.Ldstr.Value;
            static string GetOperandString(short opCodeValue, object value) {
                return (opCodeValue == ldstr_value) ? "\"" + value.ToString() + "\"" : value.ToString().TrimEnd();
            }
        }
        #region Empty
        internal static readonly IILReader Empty = new InstructionReaderEmpty();
        sealed class InstructionReaderEmpty : InstructionReader {
            internal InstructionReaderEmpty()
                : base(null, null) {
            }
            protected override string GetName(IOperandReaderContext context) {
                return string.Empty;
            }
            protected override IEnumerable<IMetadataItem> GetMetadata(IOperandReaderContext context) {
                yield break;
            }
            protected override IEnumerable<IInstruction> GetInstructions(IBinaryReader binaryReader, IOperandReaderContext context) {
                yield break;
            }
            protected override IEnumerable<ExceptionHandler> GetExceptionHandlers(IOperandReaderContext context) {
                yield break;
            }
            protected override void WriteDump(IOperandReaderContext context, Stream stream) {
                /* do nothing */
            }
        }
        #endregion
        #region Dump
        void ISupportDump.Dump(Stream stream) {
            IInstruction[] arr = instructions.Value;
            if(arr != null && arr.Length >= 0)
                writeDump.Value(stream);
        }
        #endregion
    }
}