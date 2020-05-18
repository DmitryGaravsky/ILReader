namespace ILReader.Analyzer {
    using System.Reflection;
    using System.Reflection.Emit;

    public sealed class StringConcatBoxing : ILPattern {
        public static readonly ILPattern Instance = new StringConcatBoxing();
        //
        StringConcatBoxing() 
            : base(Box.MatchFunc,
            i => i.OpCode == OpCodes.Call && IsConcatMethod(i.Operand as MethodBase)) {
        }
        static bool IsConcatMethod(MethodBase method) {
            return (method != null) && (method.DeclaringType == typeof(string) && method.Name == "Concat");
        }
    }
}