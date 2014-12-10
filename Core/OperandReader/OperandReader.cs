namespace ILReader.Readers {
    using System.Collections.Generic;
    using System.Reflection.Emit;
    using ILReader.Context;

    static class OperandReader {
        static IDictionary<OperandType, IOperandReader> cache = new Dictionary<OperandType, IOperandReader>();
        static OperandReader() {
            cache.Add(OperandType.InlineNone, new InlineNoneOperandReader());
            cache.Add(OperandType.InlineMethod, new InlineMethodOperandReader());
            cache.Add(OperandType.InlineTok, new InlineTokOperandReader());
            cache.Add(OperandType.InlineString, new InlineStringOperandReader());
            cache.Add(OperandType.InlineType, new InlineTypeOperandReader());
            cache.Add(OperandType.InlineI, new InlineIOperandReader());
            cache.Add(OperandType.InlineSig, new InlineSigOperandReader());

            cache.Add(OperandType.ShortInlineVar, new ShortInlineVarOperandReader());
            cache.Add(OperandType.ShortInlineI, new ShortInlineIOperandReader());
            cache.Add(OperandType.ShortInlineR, new ShortInlineIOperandReader());
            cache.Add(OperandType.ShortInlineBrTarget, new ShortInlineIOperandReader());
        }
        public static object Read(IBinaryReader binaryReader, IOperandReaderContext context, OperandType operandType) {
            IOperandReader reader;
            if(cache.TryGetValue(operandType, out reader))
                return reader.Read(binaryReader, context);
            throw new System.NotSupportedException(operandType.ToString());
        }
    }
}