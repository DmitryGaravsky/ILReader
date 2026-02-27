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
        public InstructionReader(ILBytesReader bytesReader, IOperandReaderContext context) {
            name = new LazyRef<string>(() => GetName(context));
            metadata = new LazyRef<IEnumerable<IMetadataItem>>(() => GetMetadata(context));
            instructions = new LazyRef<IInstruction[]>(() => GetInstructions(bytesReader, context).ToArray());
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
        protected virtual IEnumerable<IInstruction> GetInstructions(ILBytesReader bytesReader, IOperandReaderContext context) {
            int index = 0;
            while(bytesReader.CanRead())
                yield return new Instruction(index++, bytesReader, context);
        }
        protected virtual IEnumerable<ExceptionHandler> GetExceptionHandlers(IOperandReaderContext context) {
            //   Within one handler the CLR exception table guarantees that the five boundary
            //   offsets are queried in non-decreasing order:
            //     TryStart <= TryEnd <= FilterStart <= HandlerStart <= HandlerEnd
            //   A forward-only cursor therefore finds each offset in a single pass (O(n) total
            //   instead of O(n) per offset), which is correct and intentional.
            ExceptionHandler current;
            while(context.ResolveExceptionHandler(GetGetInstruction(instructions.Value), out current))
                yield return current.Advance(instructions.Value, x => ((Instruction)x).IncreaseDepth());
        }
        static Func<int, IInstruction> GetGetInstruction(IInstruction[] instructionsArray) {
            // Returns a single-pass forward cursor over the instructions array.
            //
            // Design rationale:
            //   This method is called INSIDE the while-condition of GetExceptionHandlers, so a
            //   fresh cursor (index = 0) is created for every exception handler. There is no
            //   shared state between handlers.
            // Caveat:
            //   If the IL is malformed and an offset is out of the expected order, the cursor
            //   will not backtrack and will return null. The caller (ExceptionHandler.Advance)
            //   must tolerate null boundary instructions produced in such degenerate cases.
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
            internal Instruction(int index, ILBytesReader bytesReader, IOperandReaderContext context) {
                this.Index = index;
                this.Offset = bytesReader.Offset;
                this.opCodeInfo = OpCodeReader.ReadOpCode(bytesReader);
                // Operand
                bool argumentAware = OperandReader.IsArgumentAware(OpCode);
                if(argumentAware) {
                    this.Operand = OperandReader.ReadArg(bytesReader, context, OpCode.OperandType);
                    argIndex = OperandReader.GetArgIndex(OpCode, bytesReader);
                    if(argIndex.Value > 0)
                        this.rawOperand = context[(short)(argIndex.Value - 1), true];
                    else
                        this.rawOperand = context.This;
                }
                // Local
                bool localAware = OperandReader.IsLocalAware(OpCode);
                if(localAware) {
                    this.Operand = OperandReader.Read(bytesReader, context, OpCode.OperandType);
                    locIndex = OperandReader.GetLocalIndex(OpCode, bytesReader);
                    if(Operand == null)
                        this.rawOperand = context[locIndex.Value, false];
                }
                if(!localAware && !argumentAware)
                    this.Operand = OperandReader.Read(bytesReader, context, OpCode.OperandType);
                // bytes
                int size = bytesReader.Offset - Offset;
                this.bytes = new LazyRef<byte[]>(() => bytesReader.Read(Offset, size));
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
            protected override IEnumerable<IInstruction> GetInstructions(ILBytesReader bytesReader, IOperandReaderContext context) {
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
            if(arr != null && arr.Length > 0)
                writeDump.Value(stream);
        }
        #endregion
    }
}