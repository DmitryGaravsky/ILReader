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
        //
        readonly static OpCode[] argumentAwareOpcodes = new OpCode[] { 
            OpCodes.Ldarg, OpCodes.Ldarg_0, OpCodes.Ldarg_1, OpCodes.Ldarg_2, OpCodes.Ldarg_3, OpCodes.Ldarg_S, OpCodes.Ldarga, OpCodes.Ldarga_S,
            OpCodes.Starg, OpCodes.Starg_S, 
        };
        public static bool IsArgumentAware(OpCode opCode) {
            return System.Array.IndexOf(argumentAwareOpcodes, opCode) != -1;
        }
        public static short GetArgIndex(OpCode opCode, IBinaryReader binaryReader) {
            if(opCode == OpCodes.Ldarg || opCode == OpCodes.Starg || opCode == OpCodes.Ldarga)
                return System.BitConverter.ToInt16(binaryReader.Read(binaryReader.Offset - 2, 2), 0);
            if(opCode == OpCodes.Ldarg_0)
                return 0; // this
            if(opCode == OpCodes.Ldarg_1)
                return 1;
            if(opCode == OpCodes.Ldarg_2)
                return 2;
            if(opCode == OpCodes.Ldarg_3)
                return 3;
            return binaryReader.Read(binaryReader.Offset - 1, 1)[0];
        }
        //
        readonly static OpCode[] localAwareOpcodes = new OpCode[] { 
            OpCodes.Ldloc, OpCodes.Ldloc_0, OpCodes.Ldloc_1, OpCodes.Ldloc_2, OpCodes.Ldloc_3, OpCodes.Ldloc_S, OpCodes.Ldloca, OpCodes.Ldloca_S, 
            OpCodes.Stloc, OpCodes.Stloc_0, OpCodes.Stloc_1, OpCodes.Stloc_2, OpCodes.Stloc_3, OpCodes.Stloc_S,
        };
        public static bool IsLocalAware(OpCode opCode) {
            return System.Array.IndexOf(localAwareOpcodes, opCode) != -1;
        }
        public static short GetLocalIndex(OpCode opCode, IBinaryReader binaryReader) {
            if(opCode == OpCodes.Ldloc || opCode == OpCodes.Stloc || opCode == OpCodes.Ldloca)
                return System.BitConverter.ToInt16(binaryReader.Read(binaryReader.Offset - 2, 2), 0);
            if(opCode == OpCodes.Ldloc_0 || opCode == OpCodes.Stloc_0)
                return 0;
            if(opCode == OpCodes.Ldloc_1 || opCode == OpCodes.Stloc_1)
                return 1;
            if(opCode == OpCodes.Ldloc_2 || opCode == OpCodes.Stloc_2)
                return 2;
            if(opCode == OpCodes.Ldloc_3 || opCode == OpCodes.Stloc_3)
                return 3;
            return binaryReader.Read(binaryReader.Offset - 1, 1)[0];
        }
    }
}