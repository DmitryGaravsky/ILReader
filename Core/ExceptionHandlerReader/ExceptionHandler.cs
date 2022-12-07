namespace ILReader.Readers {
    using System;
    using System.IO;
    using System.Reflection;
    using ILReader.Dump;

    [Flags]
    public enum ExceptionHandlerType {
        Catch = 0x0000,
        Filter = 0x0001,
        Finally = 0x0002,
        Fault = 0x0004,
    }
    public sealed class ExceptionHandler : ISupportDump {
        public ExceptionHandlerType HandlerType;
        public readonly IInstruction TryStart;
        public readonly IInstruction TryEnd;
        public readonly IInstruction FilterStart;
        public readonly IInstruction HandlerStart;
        public readonly IInstruction HandlerEnd;
        public object CatchType;
        public ExceptionHandler(Func<int, IInstruction> getInstruction, ExceptionHandlingClause clause) {
            this.HandlerType = (ExceptionHandlerType)(int)clause.Flags;
            this.CatchType = IsCatch ? clause.CatchType : null;
            this.TryStart = getInstruction(clause.TryOffset);
            this.TryEnd = getInstruction(clause.TryOffset + clause.TryLength);
            this.FilterStart = IsFilter ? getInstruction(clause.FilterOffset) : null;
            this.HandlerStart = getInstruction(clause.HandlerOffset);
            this.HandlerEnd = getInstruction(clause.HandlerOffset + clause.HandlerLength);
        }
        public ExceptionHandler(Func<int, IInstruction> getInstruction, ExceptionHandlerType type, string catchType, int[] offsets) {
            this.HandlerType = type;
            this.CatchType = IsCatch ? catchType : null;
            this.TryStart = getInstruction(offsets[0]);
            this.TryEnd = getInstruction(offsets[1]);
            this.FilterStart = IsFilter ? getInstruction(offsets[2]) : null;
            this.HandlerStart = getInstruction(offsets[3]);
            this.HandlerEnd = getInstruction(offsets[4]);
        }
        public bool IsCatch {
            get { return (HandlerType & (ExceptionHandlerType.Filter | ExceptionHandlerType.Finally | ExceptionHandlerType.Fault)) == ExceptionHandlerType.Catch; }
        }
        public bool IsFilter {
            get { return (HandlerType & ExceptionHandlerType.Filter) == ExceptionHandlerType.Filter; }
        }
        public bool IsFinally {
            get { return (HandlerType & ExceptionHandlerType.Finally) == ExceptionHandlerType.Finally; }
        }
        public bool IsFault {
            get { return (HandlerType & ExceptionHandlerType.Fault) == ExceptionHandlerType.Fault; }
        }
        public ExceptionHandler Advance(IInstruction[] instructions, Action<IInstruction> advance) {
            for(int i = TryStart.Index; i < TryEnd.Index; i++)
                advance(instructions[i]);
            for(int i = IsFilter ? FilterStart.Index : HandlerStart.Index; i <= HandlerEnd.Index; i++)
                advance(instructions[i]);
            return this;
        }
        void ISupportDump.Dump(Stream stream) {
            DumpHelper.Write((int)HandlerType, stream);
            string catchType = CatchType is Type ? ((Type)CatchType).FullName : (CatchType ?? string.Empty).ToString();
            DumpHelper.Write(catchType, stream);
            DumpHelper.Write(TryStart.Offset, stream);
            DumpHelper.Write(TryEnd.Offset, stream);
            DumpHelper.Write(IsFilter ? FilterStart.Offset : -1, stream);
            DumpHelper.Write(HandlerStart.Offset, stream);
            DumpHelper.Write(HandlerEnd.Offset, stream);
        }
    }
}