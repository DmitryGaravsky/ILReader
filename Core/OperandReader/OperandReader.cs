namespace ILReader.Readers {
    using System.Collections.Generic;
    using System.Reflection.Emit;

    static class OperandReader {
        static Dictionary<OperandType, IOperandReader> operandReadersCache = new Dictionary<OperandType, IOperandReader>();
        static Dictionary<OperandType, IOperandReader> argumentReadersCache = new Dictionary<OperandType, IOperandReader>();
        static OperandReader() {
            // Operand Readers
            operandReadersCache.Add(OperandType.InlineNone, new InlineNoneOperandReader());
            // 8bit
            operandReadersCache.Add(OperandType.ShortInlineBrTarget, new ShortInlineBrTargetOperandReader());
            operandReadersCache.Add(OperandType.ShortInlineI, new ShortInlineIOperandReader());
            operandReadersCache.Add(OperandType.ShortInlineVar, new ShortInlineVarOperandReader());
            // 16bit
            operandReadersCache.Add(OperandType.InlineVar, new InlineVarOperandReader());
            // 32bit
            operandReadersCache.Add(OperandType.InlineBrTarget, new InlineBrTargetOperandReader());
            operandReadersCache.Add(OperandType.InlineField, new InlineFieldOperandReader());
            operandReadersCache.Add(OperandType.InlineI, new InlineIOperandReader());
            operandReadersCache.Add(OperandType.InlineMethod, new InlineMethodOperandReader());
            operandReadersCache.Add(OperandType.InlineSig, new InlineSigOperandReader());
            operandReadersCache.Add(OperandType.InlineString, new InlineStringOperandReader());
            operandReadersCache.Add(OperandType.InlineSwitch, new InlineSwitchOperandReader());
            operandReadersCache.Add(OperandType.InlineTok, new InlineTokOperandReader());
            operandReadersCache.Add(OperandType.InlineType, new InlineTypeOperandReader());
            operandReadersCache.Add(OperandType.ShortInlineR, new ShortInlineROperandReader());
            // 64bit
            operandReadersCache.Add(OperandType.InlineI8, new InlineI8OperandReader());
            operandReadersCache.Add(OperandType.InlineR, new InlineROperandReader());
            
            // Argument Readers
            argumentReadersCache.Add(OperandType.InlineNone, new InlineNoneOperandReader());
            // 8bit
            argumentReadersCache.Add(OperandType.ShortInlineVar, new ShortInlineVarArgReader());
            // 16bit
            argumentReadersCache.Add(OperandType.InlineVar, new InlineVarArgReader());
        }
        public static object Read(IBinaryReader binaryReader, Context.IOperandReaderContext context, OperandType operandType) {
            IOperandReader reader;
            if(operandReadersCache.TryGetValue(operandType, out reader))
                return reader.Read(binaryReader, context);
            throw new System.NotSupportedException(operandType.ToString());
        }
        public static object ReadArg(IBinaryReader binaryReader, Context.IOperandReaderContext context, OperandType operandType) {
            IOperandReader reader;
            if(argumentReadersCache.TryGetValue(operandType, out reader))
                return reader.Read(binaryReader, context);
            throw new System.NotSupportedException(operandType.ToString());
        }
        public static bool IsArgumentAware(OpCode opCode) {
            return System.Array.IndexOf(argumentAwareOpcodeValues, opCode.Value) != -1;
        }
        public static short GetArgIndex(OpCode opCode, IBinaryReader binaryReader) {
            short opcodeValue = opCode.Value;
            if(opcodeValue == ldarg_0_value)
                return 0; // this
            if(opcodeValue == ldarg_1_value)
                return 1;
            if(opcodeValue == ldarg_2_value)
                return 2;
            if(opcodeValue == ldarg_3_value)
                return 3;
            if(opcodeValue == ldarg_value || opcodeValue == starg_value || opcodeValue == ldarga_value)
                return System.BitConverter.ToInt16(binaryReader.Read(binaryReader.Offset - 2, 2), 0);
            return binaryReader.Read(binaryReader.Offset - 1, 1)[0];
        }
        public static bool IsLocalAware(OpCode opCode) {
            return System.Array.IndexOf(localAwareOpcodeValues, opCode.Value) != -1;
        }
        public static short GetLocalIndex(OpCode opCode, IBinaryReader binaryReader) {
            short opcodeValue = opCode.Value;
            if(opcodeValue == ldloc_0_value || opcodeValue == stloc_0_value)
                return 0;
            if(opcodeValue == ldloc_1_value || opcodeValue == stloc_1_value)
                return 1;
            if(opcodeValue == ldloc_2_value || opcodeValue == stloc_2_value)
                return 2;
            if(opcodeValue == ldloc_3_value || opcodeValue == stloc_3_value)
                return 3;
            if(opcodeValue == ldloc_value || opcodeValue == stloc_value || opcodeValue == ldloca_value)
                return System.BitConverter.ToInt16(binaryReader.Read(binaryReader.Offset - 2, 2), 0);
            return binaryReader.Read(binaryReader.Offset - 1, 1)[0];
        }
        #region OpCodes Data
        // Argument aware
        readonly static short[] argumentAwareOpcodeValues = new short[] {
            OpCodes.Ldarg.Value, OpCodes.Ldarg_0.Value, OpCodes.Ldarg_1.Value, OpCodes.Ldarg_2.Value, OpCodes.Ldarg_3.Value,
            OpCodes.Ldarg_S.Value, OpCodes.Ldarga.Value, OpCodes.Ldarga_S.Value,
            OpCodes.Starg.Value, OpCodes.Starg_S.Value,
        };
        // Local aware
        readonly static short[] localAwareOpcodeValues = new short[] {
            OpCodes.Ldloc.Value, OpCodes.Ldloc_0.Value, OpCodes.Ldloc_1.Value, OpCodes.Ldloc_2.Value, OpCodes.Ldloc_3.Value,
            OpCodes.Ldloc_S.Value, OpCodes.Ldloca.Value, OpCodes.Ldloca_S.Value,
            OpCodes.Stloc.Value, OpCodes.Stloc_0.Value, OpCodes.Stloc_1.Value, OpCodes.Stloc_2.Value, OpCodes.Stloc_3.Value, OpCodes.Stloc_S.Value,
        };
        // Arg Indices
        readonly static short ldarg_value = OpCodes.Ldarg.Value;
        readonly static short starg_value = OpCodes.Starg.Value;
        readonly static short ldarga_value = OpCodes.Ldarga.Value;
        readonly static short ldarg_0_value = OpCodes.Ldarg_0.Value;
        readonly static short ldarg_1_value = OpCodes.Ldarg_1.Value;
        readonly static short ldarg_2_value = OpCodes.Ldarg_2.Value;
        readonly static short ldarg_3_value = OpCodes.Ldarg_3.Value;
        // Local Indices
        readonly static short ldloc_value = OpCodes.Ldloc.Value;
        readonly static short stloc_value = OpCodes.Stloc.Value;
        readonly static short ldloca_value = OpCodes.Ldloca.Value;
        readonly static short ldloc_0_value = OpCodes.Ldloc_0.Value;
        readonly static short stloc_0_value = OpCodes.Stloc_0.Value;
        readonly static short ldloc_1_value = OpCodes.Ldloc_1.Value;
        readonly static short stloc_1_value = OpCodes.Stloc_1.Value;
        readonly static short ldloc_2_value = OpCodes.Ldloc_2.Value;
        readonly static short stloc_2_value = OpCodes.Stloc_2.Value;
        readonly static short ldloc_3_value = OpCodes.Ldloc_3.Value;
        readonly static short stloc_3_value = OpCodes.Stloc_3.Value;
        #endregion
    }
}