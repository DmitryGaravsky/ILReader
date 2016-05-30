namespace ILReader.Analyzer {
    using System.Reflection;
    using System.Reflection.Emit;

    public sealed class Call : ILPattern {
        public readonly static Call Instance = new Call();
        //
        Call() : base(i => i.OpCode == OpCodes.Call || i.OpCode == OpCodes.Callvirt) { }
        //
        public bool? IsVirtualCall {
            get { return Success ? new bool?(Result[0].OpCode == OpCodes.Callvirt) : null; }
        }
        public MethodBase Method {
            get { return Success ? Result[0].Operand as MethodBase : null; }
        }
    }
}