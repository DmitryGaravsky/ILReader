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
        public override string ToString() {
            if(Success) {
                var builder = CreateResultsBuilder(lastReader.Name);
                for(int j = 0; j < Result.Length; j++)
                    builder.AppendLine("    " + Result[j].ToString());
                return builder.ToString();
            }
            return "(Empty)";
        }
        [ThreadStatic]
        static System.Text.StringBuilder resultsBuilder;
        static System.Text.StringBuilder CreateResultsBuilder(string method) {
            if(resultsBuilder == null)
                resultsBuilder = new System.Text.StringBuilder(128);
            else
                resultsBuilder.Clear();
            resultsBuilder.AppendLine(method + ":");
            return resultsBuilder;
        }
    }
}