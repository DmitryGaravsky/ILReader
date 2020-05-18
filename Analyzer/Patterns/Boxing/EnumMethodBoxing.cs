namespace ILReader.Analyzer {
    using System;
    using System.Reflection;
    using System.Reflection.Emit;

    public sealed class EnumMethodBoxing : ILPattern {
        public static readonly ILPattern Instance = new EnumMethodBoxing();
        //
        EnumMethodBoxing()
            : base(Box.MatchFunc,
            i => i.OpCode == OpCodes.Call && IsEnumMethod(i.Operand as MethodBase)) {
        }
        static bool IsEnumMethod(MethodBase method) {
            return (method != null) && (method.DeclaringType == typeof(Enum));
        }
    }
}