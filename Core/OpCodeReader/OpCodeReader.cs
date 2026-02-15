namespace ILReader.Readers {
    using System;
    using System.Reflection;
    using System.Reflection.Emit;

    static class OpCodeReader {
        #region Initialization
        static readonly OpCodeInfo[] singleByteOpCode = new OpCodeInfo[0x100];
        static readonly OpCodeInfo[] doubleByteOpCode = new OpCodeInfo[0x100];
        static OpCodeReader() {
            InitializeOpCodeInfos();
        }
        static void InitializeOpCodeInfos() {
            for(int i = 0; i < singleByteOpCode.Length; i++) {
                singleByteOpCode[i] = new OpCodeInfo(1, (byte)i);
                doubleByteOpCode[i] = new OpCodeInfo(2, (byte)i);
            }
            FieldInfo[] fields = typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static);
            for(int i = 0; i < fields.Length; i++) {
                OpCode code = (OpCode)fields[i].GetValue(null);
                if(code.Size == 1)
                    singleByteOpCode[code.Value & 0xFF] = new OpCodeInfo(code, 1);
                else if(code.Size == 2)
                    doubleByteOpCode[code.Value & 0xFF] = new OpCodeInfo(code, 2);
            }
        }
        #endregion Initialization
        const int DoubleByteInstructionPrefix = 0xFE;
        public static OpCodeInfo ReadOpCode(IBinaryReader binaryReader) {
            byte instructionOrPrefix = binaryReader.ReadByte();
            if(instructionOrPrefix != DoubleByteInstructionPrefix)
                return singleByteOpCode[instructionOrPrefix];
            byte instruction = binaryReader.ReadByte();
            return doubleByteOpCode[instruction];
        }
    }
    [Flags]
    public enum OpCodeCategory {
        None = 0,
        Unknown = 1 << 31,
        /// <summary>(ld*, ldc.*)</summary>
        Load = 1 << 0, // (ld*, ldc.*)
        /// <summary>(st*)</summary>
        Store = 1 << 1,
        /// <summary>(call, callvirt, calli)</summary>
        Call = 1 << 2,
        /// <summary>(br*, beq, bne, etc.)</summary>
        Branch = 1 << 3,
        /// <summary>(ceq, cgt, clt, etc.)</summary>
        Comparison = 1 << 4,
        /// <summary>(add, sub, mul, div, rem)</summary>
        Arithmetic = 1 << 5,
        /// <summary>(and, or, xor, not, shl, shr)</summary>
        Bitwise = 1 << 6,
        /// <summary>(conv.*, castclass, isinst, box, unbox)</summary>
        Conversion = 1 << 7,
        /// <summary>(newarr, ldelem, stelem, ldlen)</summary>
        Array = 1 << 8,
        /// <summary>(newobj, initobj, cpobj, ldobj, stobj)</summary>
        Object = 1 << 9,
        /// <summary>(pop, dup)</summary>
        Stack = 1 << 10,
        /// <summary>(ret)</summary>
        Return = 1 << 11,
        /// <summary>(throw, rethrow, leave, endfinally, endfilter)</summary>
        Exception = 1 << 12,
        /// <summary>(ldind.*, stind.*, localloc, cpblk, initblk)</summary>
        Pointer = 1 << 13,
        /// <summary>(tail., constrained., volatile., unaligned., readonly.)</summary>
        Prefix = 1 << 14,
        /// <summary>(nop, break)</summary>
        Utility = 1 << 15,
        /// <summary>Internal/undocumented</summary>
        Internal = (1 << 30),
    }
    struct OpCodeInfo {
        public readonly OpCode OpCode;
        public readonly OpCodeCategory Category;
        public readonly int Size;
        public readonly byte? RawByte;
        public OpCodeInfo(OpCode opCode, int size) {
            this.OpCode = opCode;
            this.Category = opCode.GetCategory();
            this.Size = size;
            this.RawByte = null;
        }
        public OpCodeInfo(int size, byte rawByte) {
            this.OpCode = OpCodes.Nop;
            this.Category = OpCodeCategory.Unknown;
            this.Size = size;
            this.RawByte = rawByte;
        }
        public bool IsInternal {
            get { return (Category & OpCodeCategory.Internal) == OpCodeCategory.Internal; }
        }
        public bool IsUnknown {
            get { return (Category & OpCodeCategory.Unknown) == OpCodeCategory.Unknown; }
        }
        public override string ToString() {
            if(IsUnknown)
                return $"<unknown 0x{RawByte:X2} (size={Size})>";
            string categoryStr = Category != OpCodeCategory.None && Category != OpCodeCategory.Utility
                ? $" [{Category}]"
                : "";
            return $"{OpCode.Name}{categoryStr}";
        }
    }
    static class OpCodeExtension {
        internal static OpCodeCategory GetCategory(this OpCode opCode) {
            string name = opCode.Name.ToLowerInvariant();
            OpCodeCategory category = OpCodeCategory.None;
            if(name.StartsWith("ld", StringComparison.Ordinal)) {
                category |= OpCodeCategory.Load;
                if(name.StartsWith("ldelem", StringComparison.Ordinal) || name == "ldlen")
                    category |= OpCodeCategory.Array;
                else if(name.StartsWith("ldind", StringComparison.Ordinal))
                    category |= OpCodeCategory.Pointer;
            }
            if(name.StartsWith("st", StringComparison.Ordinal)) {
                category |= OpCodeCategory.Store;
                if(name.StartsWith("stelem", StringComparison.Ordinal))
                    category |= OpCodeCategory.Array;
                else if(name.StartsWith("stind", StringComparison.Ordinal))
                    category |= OpCodeCategory.Pointer;
            }
            if(name.StartsWith("call", StringComparison.Ordinal))
                category |= OpCodeCategory.Call;
            if(name.StartsWith("br", StringComparison.Ordinal) ||
               name.StartsWith("beq", StringComparison.Ordinal) || name.StartsWith("bne", StringComparison.Ordinal) ||
               name.StartsWith("bge", StringComparison.Ordinal) || name.StartsWith("bgt", StringComparison.Ordinal) ||
               name.StartsWith("ble", StringComparison.Ordinal) || name.StartsWith("blt", StringComparison.Ordinal) ||
               name == "switch") {
                category |= OpCodeCategory.Branch;
            }
            if(name.StartsWith("ceq", StringComparison.Ordinal) ||
               name.StartsWith("cgt", StringComparison.Ordinal) ||
               name.StartsWith("clt", StringComparison.Ordinal)) {
                category |= OpCodeCategory.Comparison;
            }
            if(name == "add" || name == "sub" ||
               name == "mul" || name == "div" ||
               name == "rem" || name == "neg" ||
               name.StartsWith("add.", StringComparison.Ordinal) || name.StartsWith("sub.", StringComparison.Ordinal) ||
               name.StartsWith("mul.", StringComparison.Ordinal) || name.StartsWith("div.", StringComparison.Ordinal) ||
               name.StartsWith("rem.", StringComparison.Ordinal)) {
                category |= OpCodeCategory.Arithmetic;
            }
            if(name == "and" || name == "or" || name == "xor" || name == "not" ||
               name == "shl" || name == "shr" || name.StartsWith("shr.", StringComparison.Ordinal)) {
                category |= OpCodeCategory.Bitwise;
            }
            if(name.StartsWith("conv", StringComparison.Ordinal) ||
               name == "castclass" || name == "isinst" ||
               name == "box" || name == "unbox" ||
               name.StartsWith("unbox.", StringComparison.Ordinal)) {
                category |= OpCodeCategory.Conversion;
            }
            if(name == "newarr") {
                category |= OpCodeCategory.Array;
            }
            if(name == "newobj" || name == "initobj" || name == "cpobj" ||
               (name.StartsWith("ld", StringComparison.Ordinal) && name.EndsWith("obj", StringComparison.Ordinal)) ||
               (name.StartsWith("st", StringComparison.Ordinal) && name.EndsWith("obj", StringComparison.Ordinal))) {
                category |= OpCodeCategory.Object;
            }
            if(name == "pop" || name == "dup") {
                category |= OpCodeCategory.Stack;
            }
            if(name == "ret") {
                category |= OpCodeCategory.Return;
            }
            if(name == "throw" || name == "rethrow" ||
               name.StartsWith("leave", StringComparison.Ordinal) ||
               name == "endfinally" || name == "endfilter") {
                category |= OpCodeCategory.Exception;
            }
            if(name == "localloc" ||
               name == "cpblk" || name == "initblk" ||
               name.StartsWith("ldind", StringComparison.Ordinal) ||
               name.StartsWith("stind", StringComparison.Ordinal)) {
                category |= OpCodeCategory.Pointer;
            }
            if(name == "tail." ||
               name == "constrained." ||
               name == "volatile." ||
               name == "unaligned." ||
               name == "readonly." ||
               opCode.FlowControl == FlowControl.Meta) {
                category |= OpCodeCategory.Prefix;
            }
            if(name == "nop" || name == "break") {
                category |= OpCodeCategory.Utility;
            }
            if(opCode.OpCodeType == OpCodeType.Nternal) {
                category |= OpCodeCategory.Internal;
            }
            return category == OpCodeCategory.None ? OpCodeCategory.Utility : category;
        }
    }
}