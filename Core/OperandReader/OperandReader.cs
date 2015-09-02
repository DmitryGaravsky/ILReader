namespace ILReader.Readers {
    using System.Collections.Generic;
    using System.Reflection.Emit;

    static class OperandReader {
        static IDictionary<OperandType, IOperandReader> cache = new Dictionary<OperandType, IOperandReader>();
        static OperandReader() {
            cache.Add(OperandType.InlineNone, new InlineNoneOperandReader());
            // 8bit
            cache.Add(OperandType.ShortInlineBrTarget, new ShortInlineBrTargetOperandReader());
            cache.Add(OperandType.ShortInlineI, new ShortInlineIOperandReader());
            cache.Add(OperandType.ShortInlineVar, new ShortInlineVarOperandReader());
            // 16bit
            cache.Add(OperandType.InlineVar, new InlineVarOperandReader());
            // 32bit
            cache.Add(OperandType.InlineBrTarget, new InlineBrTargetOperandReader());
            cache.Add(OperandType.InlineField, new InlineFieldOperandReader());
            cache.Add(OperandType.InlineI, new InlineIOperandReader());
            cache.Add(OperandType.InlineMethod, new InlineMethodOperandReader());
            cache.Add(OperandType.InlineSig, new InlineSigOperandReader());
            cache.Add(OperandType.InlineString, new InlineStringOperandReader());
            cache.Add(OperandType.InlineSwitch, new InlineSwitchOperandReader());
            cache.Add(OperandType.InlineTok, new InlineTokOperandReader());
            cache.Add(OperandType.InlineType, new InlineTypeOperandReader());
            cache.Add(OperandType.ShortInlineR, new ShortInlineROperandReader());
            // 64bit
            cache.Add(OperandType.InlineI8, new InlineI8OperandReader());
            cache.Add(OperandType.InlineR, new InlineROperandReader());
        }
        public static object Read(IBinaryReader binaryReader, Context.IOperandReaderContext context, OperandType operandType) {
            IOperandReader reader;
            if(cache.TryGetValue(operandType, out reader))
                return reader.Read(binaryReader, context);
            throw new System.NotSupportedException(operandType.ToString());
        }
    }
}