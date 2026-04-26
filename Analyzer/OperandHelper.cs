namespace ILReader.Analyzer {
    using ILReader.Readers;

    static class OperandHelper {
        internal static T GetSource<T>(this object operand)
            where T : class {
            if(operand is T direct)
                return direct;
            if(operand is IMetadataSymbol sym && sym.TryGetSource<T>(out var result))
                return result;
            return null;
        }
    }
}