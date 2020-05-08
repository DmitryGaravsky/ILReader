namespace ILReader.Analyzer {
    using System.Reflection;
    using System.Reflection.Emit;

    public sealed class EnumHasFlagBoxing : ILPattern {
        public static readonly ILPattern Instance = new EnumHasFlagBoxing();
        //
        EnumHasFlagBoxing()
            : base(
            i => i.OpCode == OpCodes.Box,
            i => i.OpCode == OpCodes.Call && IsHasFlagMethod(i.Operand as MethodBase)) {
        }
        static bool IsHasFlagMethod(MethodBase method) {
            return (method != null) && (method.DeclaringType == typeof(System.Enum) && method.Name == "HasFlag");
        }
        public sealed override string ToString() {
            return "Boxing of Enum.HasFlag() argument";
        }
    }
}