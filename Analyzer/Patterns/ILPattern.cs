namespace ILReader.Analyzer {
    using System;
    using System.Linq;
    using System.Reflection.Emit;
    using ILReader.Readers;

    public class ILPattern : MatchPattern<IInstruction> {
        protected ILPattern(params Func<IInstruction, bool>[] matches)
            : base(matches) {
        }
        public bool Match(IILReader reader) {
            return Match(reader.Where(i => i.OpCode != OpCodes.Nop).ToArray());
        }
    }
}