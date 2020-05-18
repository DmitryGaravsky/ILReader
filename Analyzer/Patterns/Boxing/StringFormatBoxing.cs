namespace ILReader.Analyzer {
    using System.Reflection;
    using System.Reflection.Emit;

    public sealed class StringFormatBoxing : ILPattern {
        public static readonly ILPattern Instance = new StringFormatBoxing();
        //
        StringFormatBoxing()
            : base(Box.MatchFunc,
            i => i.OpCode == OpCodes.Call && IsFormatMethod(i.Operand as MethodBase)) {
        }
        static bool IsFormatMethod(MethodBase method) {
            return (method != null) && (method.DeclaringType == typeof(string) && method.Name == "Format");
        }
    }
}