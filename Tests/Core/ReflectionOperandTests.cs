namespace ILReader.Core.Tests {
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using ILReader.Readers;
    using NUnit.Framework;

    [TestFixture]
    public class ReflectionOperand_Tests {
        readonly IILReaderConfiguration cfg = StandardConfiguration.Default;
        public class Victims {
            int _field = 42;
            public string GetTypeName(object o) => o.GetType().Name;
            public int GetField() => _field;
            public void SetField(int v) { _field = v; }
            public string Cast(object o) => (string)o;
            public bool IsStr(object o) => o is string;
            public int[] NewArr(int n) => new int[n];
            public string Hello() => "Hello, World!";
            public Type GetStringType() => typeof(string);
            public string Classify(int n) {
                switch(n) {
                    case 0: return "zero";
                    case 1: return "one";
                    case 2: return "two";
                    case 3: return "three";
                    case 4: return "four";
                    default:
                        return "other";
                }
            }
            public int WithFilter(int a, int b) {
                try {
                    return a / b;
                }
                catch(DivideByZeroException e) when(e != null) { return -1; }
            }
            public int GetLargeConst() => 1000;
            public int GetSmallConst() => 42;
            public long GetLongConst() => 100_000_000_000L;
            public double GetDoubleConst() => 3.14;
        }
        [Test]
        public void InlineMethod_Operand_IsIMetadataSymbol_KindMethod() {
            var reader = cfg.GetReader(typeof(Victims).GetMethod(nameof(Victims.GetTypeName)));
            var instr = reader.FirstOrDefault(i =>
                i.OpCode.OperandType == OperandType.InlineMethod && i.Operand != null);
            Assert.IsNotNull(instr, "GetTypeName must contain at least one InlineMethod instruction");
            var sym = (IMetadataSymbol)instr.Operand;
            Assert.AreEqual(MetadataSymbolKind.Method, sym.Kind);
            Assert.IsNotEmpty(sym.Name);
        }
        [Test]
        public void InlineMethod_GetSource_MethodBase_NonNull() {
            var reader = cfg.GetReader(typeof(Victims).GetMethod(nameof(Victims.GetTypeName)));
            var instr = reader.First(i =>
                i.OpCode.OperandType == OperandType.InlineMethod && i.Operand != null);
            var sym = (IMetadataSymbol)instr.Operand;
            Assert.IsTrue(sym.TryGetSource<MethodBase>(out var mb));
            Assert.IsNotNull(mb);
        }
        [Test]
        public void InlineField_Operand_IsIMetadataSymbol_KindField() {
            var reader = cfg.GetReader(typeof(Victims).GetMethod(nameof(Victims.GetField)));
            var instr = reader.FirstOrDefault(i =>
                i.OpCode.OperandType == OperandType.InlineField && i.Operand != null);
            Assert.IsNotNull(instr, "GetField must contain an InlineField instruction");
            var sym = (IMetadataSymbol)instr.Operand;
            Assert.AreEqual(MetadataSymbolKind.Field, sym.Kind);
            Assert.AreEqual("_field", sym.Name);
        }
        [Test]
        public void InlineType_Operand_IsIMetadataSymbol_KindType() {
            var reader = cfg.GetReader(typeof(Victims).GetMethod(nameof(Victims.Cast)));
            var instr = reader.FirstOrDefault(i =>
                i.OpCode.OperandType == OperandType.InlineType && i.Operand != null);
            Assert.IsNotNull(instr, "Cast must contain an InlineType instruction (castclass)");
            var sym = (IMetadataSymbol)instr.Operand;
            Assert.AreEqual(MetadataSymbolKind.Type, sym.Kind);
            Assert.AreEqual("String", sym.Name);
        }
        [Test]
        public void InlineString_Operand_Is_Correct_String() {
            var reader = cfg.GetReader(typeof(Victims).GetMethod(nameof(Victims.Hello)));
            var instr = reader.FirstOrDefault(i => i.OpCode == OpCodes.Ldstr);
            Assert.IsNotNull(instr, "Hello must contain ldstr");
            Assert.AreEqual("Hello, World!", (string)instr.Operand);
        }
        [Test]
        public void InlineTok_Operand_IsIMetadataSymbol() {
            var reader = cfg.GetReader(typeof(Victims).GetMethod(nameof(Victims.GetStringType)));
            var instr = reader.FirstOrDefault(i =>
                i.OpCode.OperandType == OperandType.InlineTok && i.Operand != null);
            Assert.IsNotNull(instr, "GetStringType must contain ldtoken (InlineTok)");
            Assert.IsInstanceOf<IMetadataSymbol>(instr.Operand);
        }
        [Test]
        public void InlineSwitch_Operand_IsIntArray() {
            var reader = cfg.GetReader(typeof(Victims).GetMethod(nameof(Victims.Classify)));
            var instr = reader.FirstOrDefault(i =>
                i.OpCode.OperandType == OperandType.InlineSwitch);
            Assert.IsNotNull(instr, "Classify must contain a switch instruction");
            Assert.IsInstanceOf<int[]>(instr.Operand);
            Assert.GreaterOrEqual(((int[])instr.Operand).Length, 5);
        }
        [Test]
        public void FilterHandler_HandlerType_And_FilterStart_NonNull() {
            var reader = cfg.GetReader(typeof(Victims).GetMethod(nameof(Victims.WithFilter)));
            var handlers = reader.ExceptionHandlers;
            var filterHandler = Array.Find(handlers,
                h => (h.HandlerType & ExceptionHandlerType.Filter) == ExceptionHandlerType.Filter);
            Assert.IsNotNull(filterHandler, "WithFilter must produce a Filter exception handler");
            Assert.IsNotNull(filterHandler.FilterStart, "FilterStart must not be null for Filter handler");
        }
        [Test]
        public void InlineI_Operand_IsInt32_WithCorrectValue() {
            var reader = cfg.GetReader(typeof(Victims).GetMethod(nameof(Victims.GetLargeConst)));
            var instr = reader.FirstOrDefault(i => i.OpCode.OperandType == OperandType.InlineI);
            Assert.IsNotNull(instr, "GetLargeConst must contain an InlineI instruction (ldc.i4)");
            Assert.IsInstanceOf<int>(instr.Operand);
            Assert.AreEqual(1000, (int)instr.Operand);
        }
        [Test]
        public void ShortInlineI_Operand_IsByte_WithCorrectValue() {
            var reader = cfg.GetReader(typeof(Victims).GetMethod(nameof(Victims.GetSmallConst)));
            var instr = reader.FirstOrDefault(i => i.OpCode.OperandType == OperandType.ShortInlineI);
            Assert.IsNotNull(instr, "GetSmallConst must contain a ShortInlineI instruction (ldc.i4.s)");
            Assert.IsInstanceOf<byte>(instr.Operand);
            Assert.AreEqual((byte)42, (byte)instr.Operand);
        }
        [Test]
        public void InlineI8_Operand_IsInt64_WithCorrectValue() {
            var reader = cfg.GetReader(typeof(Victims).GetMethod(nameof(Victims.GetLongConst)));
            var instr = reader.FirstOrDefault(i => i.OpCode.OperandType == OperandType.InlineI8);
            Assert.IsNotNull(instr, "GetLongConst must contain an InlineI8 instruction (ldc.i8)");
            Assert.IsInstanceOf<long>(instr.Operand);
            Assert.AreEqual(100_000_000_000L, (long)instr.Operand);
        }
        [Test]
        public void InlineR_Operand_IsDouble_WithCorrectValue() {
            var reader = cfg.GetReader(typeof(Victims).GetMethod(nameof(Victims.GetDoubleConst)));
            var instr = reader.FirstOrDefault(i => i.OpCode.OperandType == OperandType.InlineR);
            Assert.IsNotNull(instr, "GetDoubleConst must contain an InlineR instruction (ldc.r8)");
            Assert.IsInstanceOf<double>(instr.Operand);
            Assert.AreEqual(3.14, (double)instr.Operand);
        }
    }
}
