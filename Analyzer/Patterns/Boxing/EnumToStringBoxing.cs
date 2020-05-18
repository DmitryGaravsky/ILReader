namespace ILReader.Analyzer {
    using System;
    using System.Reflection;
    using System.Reflection.Emit;

    public sealed class EnumToStringBoxing : ILPattern {
        public static readonly ILPattern Instance = new EnumToStringBoxing();
        //
        EnumToStringBoxing()
            : base(
            i => i.OpCode == OpCodes.Constrained && IsEnum(i.Operand as Type),
            i => i.OpCode == OpCodes.Callvirt && IsToStringMethod(i.Operand as MethodBase)) {
        }
        static bool IsEnum(Type type) {
            return (type != null) && type.IsEnum;
        }
        static bool IsToStringMethod(MethodBase method) {
            return (method != null) && (method.Name == "ToString");
        }
    }
}