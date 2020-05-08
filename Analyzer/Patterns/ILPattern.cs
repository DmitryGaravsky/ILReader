namespace ILReader.Analyzer {
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using ILReader.Readers;

    public class ILPattern : MatchPattern<IInstruction> {
        protected ILPattern(params Func<IInstruction, bool>[] matches)
            : base(matches) {
        }
        public bool Match(MethodBase method, bool skinNops = true) {
            return Match(Configuration.Resolve(method).GetReader(method), skinNops);
        }
        public bool Match(IILReader reader, bool skinNops = true) {
            if(lastReader != reader)
                ResetCore();
            return matchFunc(Reset() + 1, GetElements(reader, skinNops));
        }
        IILReader lastReader;
        IInstruction[] GetElements(IILReader reader, bool skinNops) {
            lastReader = reader;
            return (skinNops ? reader.Where(i => i.OpCode != OpCodes.Nop) : reader).ToArray();
        }
    }
}