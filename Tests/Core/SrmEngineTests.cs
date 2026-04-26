namespace ILReader.Core.Tests {
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using ILReader.Readers;
    using NUnit.Framework;

    [TestFixture]
    public class SrmEngine_Tests {
        static System.Type VictimType => typeof(ReflectionOperand_Tests.Victims);
        static IILReaderConfiguration ReflectionCfg => StandardConfiguration.Default;
        static IILReaderConfiguration SrmCfg() {
            string dllPath = typeof(SrmEngine_Tests).Assembly.Location;
            return Configuration.ForAssembly(File.OpenRead(dllPath));
        }
        static IILReader SrmByToken(int token) {
            string dllPath = typeof(SrmEngine_Tests).Assembly.Location;
            var cfg = Configuration.ForAssembly(File.OpenRead(dllPath));
            return ((IILReaderConfiguration)cfg).GetReader(token);
        }
        static void GetBothReaders(string methodName, out IILReader reflection, out IILReader srm) {
            var method = VictimType.GetMethod(methodName);
            reflection = ReflectionCfg.GetReader(method);
            srm = ((IILReaderConfiguration)SrmCfg()).GetReader(method.MetadataToken);
        }
#pragma warning disable CS0649
        class ParamVictims {
            public void InstanceMethod(int a, string b, bool c) { _ = a; _ = b; _ = c; }
            public static void StaticMethod(int x, string y) { _ = x; _ = y; }
            public void NoParams() { }
        }
        class InstrVictims {
            public static bool CallVictim(string s) => string.IsNullOrEmpty(s);
            public static string LdstrVictim() => "hello world";
            public static int LdcVictim() => 1000;
            public int LdargVictim(int x) => x;
        }
        class FieldVictim {
            static int staticField = 42;
            int instanceField;
            public static int ReadStaticField() => staticField;
            public int ReadInstanceField() => instanceField;
        }
        class OuterVictim {
            public class InnerVictim {
                public void InnerMethod(int x) { _ = x; }
            }
        }
        class ReturnTypeVictim {
            public static string ReturnsString() => "x";
            public static void ReturnsVoid() { }
        }
        class GenericCallVictim {
            public static string[] CallGenericMethod() => System.Array.Empty<string>();
        }
        static class LocalVarVictim {
            [System.Runtime.CompilerServices.MethodImpl(
                System.Runtime.CompilerServices.MethodImplOptions.NoOptimization |
                System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
            public static int LocalVarMethod(int x) {
                // 5 user-visible locals force the compiler to emit ldloc.s (index >= 4)
                // ensuring that the InstructionReader sees a non-null Operand for one ldloc.
                int v0 = x;
                int v1 = v0 + 1;
                int v2 = v1 + 1;
                int v3 = v2 + 1;
                int v4 = v3 + 1;
                return v0 + v1 + v2 + v3 + v4;
            }
        }
        class InternalCallVictim {
            static void InternalTarget(int a, string b) { _ = a; _ = b; }
            public static void Caller() => InternalTarget(1, "hello");
        }
#pragma warning restore CS0649
        //
        [Test]
        public void InstructionCount_Matches_GetTypeName() {
            GetBothReaders(nameof(ReflectionOperand_Tests.Victims.GetTypeName), out var r, out var s);
            Assert.AreEqual(r.Count, s.Count);
        }
        [Test]
        public void InstructionOffsets_Match_GetField() {
            GetBothReaders(nameof(ReflectionOperand_Tests.Victims.GetField), out var r, out var s);
            var rOffsets = r.Select(i => i.Offset).ToArray();
            var sOffsets = s.Select(i => i.Offset).ToArray();
            CollectionAssert.AreEqual(rOffsets, sOffsets);
        }
        [Test]
        public void OpCodes_Match_Classify() {
            GetBothReaders(nameof(ReflectionOperand_Tests.Victims.Classify), out var r, out var s);
            var rCodes = r.Select(i => i.OpCode.Value).ToArray();
            var sCodes = s.Select(i => i.OpCode.Value).ToArray();
            CollectionAssert.AreEqual(rCodes, sCodes);
        }
        [Test]
        public void InlineMethod_SymbolName_Matches_GetTypeName() {
            GetBothReaders(nameof(ReflectionOperand_Tests.Victims.GetTypeName), out var r, out var s);
            var rSym = r.First(i => i.OpCode.OperandType == OperandType.InlineMethod && i.Operand != null);
            var sSym = s.First(i => i.OpCode.OperandType == OperandType.InlineMethod && i.Operand != null);
            Assert.AreEqual(
                ((IMetadataSymbol)rSym.Operand).Name,
                ((IMetadataSymbol)sSym.Operand).Name);
        }
        [Test]
        public void InlineField_SymbolName_Matches_GetField() {
            GetBothReaders(nameof(ReflectionOperand_Tests.Victims.GetField), out var r, out var s);
            var rSym = r.First(i => i.OpCode.OperandType == OperandType.InlineField && i.Operand != null);
            var sSym = s.First(i => i.OpCode.OperandType == OperandType.InlineField && i.Operand != null);
            Assert.AreEqual(
                ((IMetadataSymbol)rSym.Operand).Name,
                ((IMetadataSymbol)sSym.Operand).Name);
        }
        [Test]
        public void InlineString_Matches_Hello() {
            GetBothReaders(nameof(ReflectionOperand_Tests.Victims.Hello), out var r, out var s);
            var rStr = r.First(i => i.OpCode == OpCodes.Ldstr).Operand as string;
            var sStr = s.First(i => i.OpCode == OpCodes.Ldstr).Operand as string;
            Assert.AreEqual(rStr, sStr);
        }
        [Test]
        public void Reflection_GetSource_MethodBase_NonNull() {
            GetBothReaders(nameof(ReflectionOperand_Tests.Victims.GetTypeName), out var r, out _);
            var sym = (IMetadataSymbol)r.First(i =>
                i.OpCode.OperandType == OperandType.InlineMethod && i.Operand != null).Operand;
            Assert.IsTrue(sym.TryGetSource<MethodBase>(out var mb));
            Assert.IsNotNull(mb);
        }
        [Test]
        public void Srm_GetSource_EntityHandle_Valid() {
            GetBothReaders(nameof(ReflectionOperand_Tests.Victims.GetTypeName), out _, out var s);
            var sym = (IMetadataSymbol)s.First(i =>
                i.OpCode.OperandType == OperandType.InlineMethod && i.Operand != null).Operand;
            Assert.IsTrue(sym.TryGetSource<System.Reflection.Metadata.EntityHandle>(out var handle));
            Assert.IsFalse(handle.IsNil);
        }
        [Test]
        public void ExceptionHandlers_Count_Matches_WithFilter() {
            GetBothReaders(nameof(ReflectionOperand_Tests.Victims.WithFilter), out var r, out var s);
            Assert.AreEqual(r.ExceptionHandlers.Length, s.ExceptionHandlers.Length);
        }
        [Test]
        public void ExceptionHandler_Type_Matches_WithFilter() {
            GetBothReaders(nameof(ReflectionOperand_Tests.Victims.WithFilter), out var r, out var s);
            Assert.AreEqual(r.ExceptionHandlers[0].HandlerType, s.ExceptionHandlers[0].HandlerType);
        }
        [Test]
        public void ExceptionHandler_Offsets_Match_WithFilter() {
            GetBothReaders(nameof(ReflectionOperand_Tests.Victims.WithFilter), out var r, out var s);
            var rh = r.ExceptionHandlers[0];
            var sh = s.ExceptionHandlers[0];
            Assert.AreEqual(rh.TryStart.Offset, sh.TryStart.Offset);
            Assert.AreEqual(rh.TryEnd.Offset, sh.TryEnd.Offset);
            Assert.AreEqual(rh.HandlerStart.Offset, sh.HandlerStart.Offset);
            Assert.AreEqual(rh.HandlerEnd.Offset, sh.HandlerEnd.Offset);
        }
        [Test]
        public void Stream_Can_Be_Closed_After_GetReader_Returns() {
            string dllPath = typeof(SrmEngine_Tests).Assembly.Location;
            IILReader reader;
            using(var stream = File.OpenRead(dllPath)) {
                var cfg = Configuration.ForAssembly(stream);
                int token = VictimType.GetMethod(nameof(ReflectionOperand_Tests.Victims.Hello)).MetadataToken;
                reader = ((IILReaderConfiguration)cfg).GetReader(token);
            }
            Assert.Greater(reader.Count, 0);
        }
        [Test]
        public void NameBased_InstructionCount_Matches_Reflection() {
            var method = VictimType.GetMethod(nameof(ReflectionOperand_Tests.Victims.Hello));
            var reflection = ReflectionCfg.GetReader(method);
            string dllPath = typeof(SrmEngine_Tests).Assembly.Location;
            var cfg = Configuration.ForAssembly(File.OpenRead(dllPath));
            // SRM builds type name from Namespace + "." + Name; nested types have no namespace in metadata
            var srm = ((IILReaderConfiguration)cfg).GetReader(VictimType.Name, nameof(ReflectionOperand_Tests.Victims.Hello));
            Assert.AreEqual(reflection.Count, srm.Count);
        }
        [Test]
        public void Srm_InlineField_Kind_Name_And_DeclaringType_Match_Reflection() {
            GetBothReaders(nameof(ReflectionOperand_Tests.Victims.GetField), out var r, out var s);
            var rSym = (IMetadataSymbol)r.First(i =>
                i.OpCode.OperandType == OperandType.InlineField && i.Operand != null).Operand;
            var sSym = (IMetadataSymbol)s.First(i =>
                i.OpCode.OperandType == OperandType.InlineField && i.Operand != null).Operand;
            Assert.AreEqual(MetadataSymbolKind.Field, sSym.Kind);
            Assert.AreEqual(rSym.Name, sSym.Name);
            Assert.AreEqual(rSym.DeclaringType, sSym.DeclaringType);
        }
        // ── Group A: Parameter resolution ──────────────────────────────────────
        [Test]
        public void A_InstanceMethod_Parameters_Count() {
            var cfg = SrmCfg();
            var srmSym = ((IILReaderConfiguration)cfg).FindMethods(
                typeof(ParamVictims).FullName, nameof(ParamVictims.InstanceMethod)).First();
            Assert.AreEqual(3, srmSym.Parameters.Length);
        }
        [Test]
        public void A_InstanceMethod_Parameters_Types() {
            var cfg = SrmCfg();
            var srmSym = ((IILReaderConfiguration)cfg).FindMethods(
                typeof(ParamVictims).FullName, nameof(ParamVictims.InstanceMethod)).First();
            Assert.AreEqual("System.Int32", srmSym.Parameters[0].TypeName);
            Assert.AreEqual("System.String", srmSym.Parameters[1].TypeName);
            Assert.AreEqual("System.Boolean", srmSym.Parameters[2].TypeName);
        }
        [Test]
        public void A_StaticMethod_Parameters_Count() {
            var cfg = SrmCfg();
            var srmSym = ((IILReaderConfiguration)cfg).FindMethods(
                typeof(ParamVictims).FullName, nameof(ParamVictims.StaticMethod)).First();
            Assert.AreEqual(2, srmSym.Parameters.Length);
        }
        [Test]
        public void A_NoParams_Parameters_Empty() {
            var cfg = SrmCfg();
            var srmSym = ((IILReaderConfiguration)cfg).FindMethods(
                typeof(ParamVictims).FullName, nameof(ParamVictims.NoParams)).First();
            Assert.IsEmpty(srmSym.Parameters);
        }
        // ── Group B: Instruction operands ─────────────────────────────────────
        [Test]
        public void B_Call_Instruction_HasIMethodSymbolOperand() {
            var method = typeof(InstrVictims).GetMethod(nameof(InstrVictims.CallVictim));
            var srm = SrmByToken(method.MetadataToken);
            var instr = srm.First(i => i.OpCode.OperandType == OperandType.InlineMethod
                                        && i.Operand is IMethodSymbol m
                                        && m.Name == "IsNullOrEmpty");
            Assert.IsNotNull(instr);
            Assert.AreEqual("System.String", ((IMethodSymbol)instr.Operand).DeclaringType);
        }
        [Test]
        public void B_Ldstr_HasStringOperand() {
            var method = typeof(InstrVictims).GetMethod(nameof(InstrVictims.LdstrVictim));
            var srm = SrmByToken(method.MetadataToken);
            var instr = srm.First(i => i.OpCode == OpCodes.Ldstr && i.Operand is string);
            Assert.AreEqual("hello world", (string)instr.Operand);
        }
        [Test]
        public void B_Ldc_I4_HasIntOperand_1000() {
            var method = typeof(InstrVictims).GetMethod(nameof(InstrVictims.LdcVictim));
            var srm = SrmByToken(method.MetadataToken);
            Assert.IsTrue(srm.Any(i => i.Operand is int v && v == 1000));
        }
        // ── Group C: Local variable resolution ────────────────────────────────
        [Test]
        public void C_Ldloc_ResolvesToLocalVariableSymbol() {
            var method = typeof(LocalVarVictim).GetMethod(nameof(LocalVarVictim.LocalVarMethod));
            var srm = SrmByToken(method.MetadataToken);
            Assert.IsTrue(srm.Any(i => i.Operand is LocalVariableSymbol));
        }
        [Test]
        public void C_Ldloc_LocalVariableSymbol_TypeName_IsInt32() {
            var method = typeof(LocalVarVictim).GetMethod(nameof(LocalVarVictim.LocalVarMethod));
            var srm = SrmByToken(method.MetadataToken);
            var lv = (LocalVariableSymbol)srm.First(i => i.Operand is LocalVariableSymbol).Operand;
            Assert.AreEqual("System.Int32", lv.TypeName);
        }
        // ── Group D: Field symbol resolution ──────────────────────────────────
        [Test]
        public void D_Ldsfld_HasIFieldSymbolOperand() {
            var method = typeof(FieldVictim).GetMethod(nameof(FieldVictim.ReadStaticField));
            var srm = SrmByToken(method.MetadataToken);
            var instr = srm.First(i => i.OpCode == OpCodes.Ldsfld && i.Operand is IFieldSymbol);
            Assert.AreEqual("staticField", ((IFieldSymbol)instr.Operand).Name);
        }
        [Test]
        public void D_Ldfld_HasIFieldSymbolOperand() {
            var method = typeof(FieldVictim).GetMethod(nameof(FieldVictim.ReadInstanceField));
            var srm = SrmByToken(method.MetadataToken);
            var instr = srm.First(i => i.OpCode == OpCodes.Ldfld && i.Operand is IFieldSymbol);
            Assert.AreEqual("instanceField", ((IFieldSymbol)instr.Operand).Name);
        }
        [Test]
        public void D_IFieldSymbol_FieldTypeName_IsInt32() {
            var method = typeof(FieldVictim).GetMethod(nameof(FieldVictim.ReadStaticField));
            var srm = SrmByToken(method.MetadataToken);
            var sym = (IFieldSymbol)srm.First(i => i.Operand is IFieldSymbol).Operand;
            Assert.AreEqual("System.Int32", sym.FieldTypeName);
        }
        // ── Group E: Nested type symbol properties ─────────────────────────────
        [Test]
        public void E_NestedType_IsDeclaringTypeNested_IsTrue() {
            var cfg = SrmCfg();
            var srmSym = ((IILReaderConfiguration)cfg).FindMethods(
                typeof(OuterVictim.InnerVictim).FullName, nameof(OuterVictim.InnerVictim.InnerMethod)).First();
            Assert.IsTrue(srmSym.IsDeclaringTypeNested);
        }
        [Test]
        public void E_NestedType_DeclaringTypeName_UsesPlusNotation() {
            var cfg = SrmCfg();
            var srmSym = ((IILReaderConfiguration)cfg).FindMethods(
                typeof(OuterVictim.InnerVictim).FullName, nameof(OuterVictim.InnerVictim.InnerMethod)).First();
            Assert.IsTrue(srmSym.DeclaringType.Contains("+"));
        }
        [Test]
        public void E_NestedType_EnclosingTypeNames_NotEmpty() {
            var cfg = SrmCfg();
            var srmSym = ((IILReaderConfiguration)cfg).FindMethods(
                typeof(OuterVictim.InnerVictim).FullName, nameof(OuterVictim.InnerVictim.InnerMethod)).First();
            Assert.IsNotEmpty(srmSym.EnclosingTypeNames);
        }
        // ── Group F: Method symbol properties ─────────────────────────────────
        [Test]
        public void F_ReturnTypeName_IsString() {
            var cfg = SrmCfg();
            var srmSym = ((IILReaderConfiguration)cfg).FindMethods(
                typeof(ReturnTypeVictim).FullName, nameof(ReturnTypeVictim.ReturnsString)).First();
            Assert.AreEqual("System.String", srmSym.ReturnTypeName);
        }
        [Test]
        public void F_ReturnTypeName_IsVoid() {
            var cfg = SrmCfg();
            var srmSym = ((IILReaderConfiguration)cfg).FindMethods(
                typeof(ReturnTypeVictim).FullName, nameof(ReturnTypeVictim.ReturnsVoid)).First();
            Assert.AreEqual("System.Void", srmSym.ReturnTypeName);
        }
        [Test]
        public void F_FullName_ContainsReturnTypeAndMethodName() {
            var cfg = SrmCfg();
            var srmSym = ((IILReaderConfiguration)cfg).FindMethods(
                typeof(ReturnTypeVictim).FullName, nameof(ReturnTypeVictim.ReturnsString)).First();
            Assert.IsTrue(srmSym.FullName.Contains("System.String"));
            Assert.IsTrue(srmSym.FullName.Contains(nameof(ReturnTypeVictim.ReturnsString)));
        }
        // ── Group G: MethodSpecification (generic calls) ───────────────────────
        [Test]
        public void G_GenericMethodCall_InstructionFound_ViaMethodSpecPath() {
            var method = typeof(GenericCallVictim).GetMethod(nameof(GenericCallVictim.CallGenericMethod));
            var srm = SrmByToken(method.MetadataToken);
            Assert.IsTrue(srm.Any(i =>
                i.OpCode.OperandType == OperandType.InlineMethod
                && i.Operand is IMethodSymbol m && m.Name == "Empty"));
        }
        [Test]
        public void G_GenericMethodCall_DeclaringType_IsArray() {
            var method = typeof(GenericCallVictim).GetMethod(nameof(GenericCallVictim.CallGenericMethod));
            var srm = SrmByToken(method.MetadataToken);
            var sym = (IMethodSymbol)srm.First(i =>
                i.OpCode.OperandType == OperandType.InlineMethod
                && i.Operand is IMethodSymbol m && m.Name == "Empty").Operand;
            Assert.AreEqual("System.Array", sym.DeclaringType);
        }
        // ── Group H: FindMethods + signature-based lookup ─────────────────────
        [Test]
        public void H_FindMethods_Returns_Overloads() {
            var cfg = SrmCfg();
            var methods = ((IILReaderConfiguration)cfg)
                .FindMethods(typeof(ParamVictims).FullName, nameof(ParamVictims.InstanceMethod))
                .ToList();
            Assert.GreaterOrEqual(methods.Count, 1);
        }
        [Test]
        public void H_GetReader_BySignature_ReturnsCorrectReader() {
            var reflMethod = typeof(ReturnTypeVictim).GetMethod(nameof(ReturnTypeVictim.ReturnsString));
            var reflReader = ReflectionCfg.GetReader(reflMethod);
            string sig = "System.String ReturnsString()";
            string dllPath = typeof(SrmEngine_Tests).Assembly.Location;
            var cfg = Configuration.ForAssembly(File.OpenRead(dllPath));
            var srmReader = ((IILReaderConfiguration)cfg).GetReader(
                typeof(ReturnTypeVictim).FullName, nameof(ReturnTypeVictim.ReturnsString), sig);
            Assert.AreEqual(reflReader.Count, srmReader.Count);
        }
        // ── Group I: Internal MethodDef call ──────────────────────────────────
        [Test]
        public void I_InternalMethodDef_MethodSymbol_Parameters_CountAndTypes() {
            var method = typeof(InternalCallVictim).GetMethod(nameof(InternalCallVictim.Caller));
            var srm = SrmByToken(method.MetadataToken);
            var sym = (IMethodSymbol)srm.First(i =>
                i.OpCode.OperandType == OperandType.InlineMethod
                && i.Operand is IMethodSymbol ms && ms.Name == "InternalTarget").Operand;
            Assert.AreEqual(2, sym.Parameters.Length);
            Assert.AreEqual("System.Int32", sym.Parameters[0].TypeName);
            Assert.AreEqual("System.String", sym.Parameters[1].TypeName);
        }
        // ── Group J: Dual-backend FullName + DeclaringType ─────────────────────
        [Test]
        public void J_InlineMethod_SymbolName_Matches_BothBackends() {
            GetBothReaders(nameof(ReflectionOperand_Tests.Victims.GetTypeName), out var r, out var s);
            var rSym = (IMetadataSymbol)r.First(i => i.OpCode.OperandType == OperandType.InlineMethod && i.Operand != null).Operand;
            var sSym = (IMetadataSymbol)s.First(i => i.OpCode.OperandType == OperandType.InlineMethod && i.Operand != null).Operand;
            Assert.AreEqual(rSym.Name, sSym.Name);
            Assert.AreEqual(rSym.DeclaringType, sSym.DeclaringType);
        }
        [Test]
        public void J_InlineMethod_FullName_NotEmpty_SrmBackend() {
            GetBothReaders(nameof(ReflectionOperand_Tests.Victims.GetTypeName), out _, out var s);
            var sSym = (IMetadataSymbol)s.First(i => i.OpCode.OperandType == OperandType.InlineMethod && i.Operand != null).Operand;
            Assert.IsNotEmpty(sSym.FullName);
        }
        [Test]
        public void J_IFieldSymbol_BothBackends_Name_Match() {
            GetBothReaders(nameof(ReflectionOperand_Tests.Victims.GetField), out var r, out var s);
            var rSym = (IFieldSymbol)r.First(i => i.OpCode.OperandType == OperandType.InlineField && i.Operand != null).Operand;
            var sSym = (IFieldSymbol)s.First(i => i.OpCode.OperandType == OperandType.InlineField && i.Operand != null).Operand;
            Assert.AreEqual(rSym.Name, sSym.Name);
            Assert.AreEqual(rSym.FieldTypeName, sSym.FieldTypeName);
        }
        // ── Group K: Namespace-qualified type names ────────────────────────────
        [Test]
        public void K_DeclaringType_IncludesNamespace() {
            var method = typeof(InstrVictims).GetMethod(nameof(InstrVictims.CallVictim));
            var srm = SrmByToken(method.MetadataToken);
            var sym = (IMethodSymbol)srm.First(i =>
                i.OpCode.OperandType == OperandType.InlineMethod
                && i.Operand is IMethodSymbol m && m.Name == "IsNullOrEmpty").Operand;
            Assert.AreEqual("System.String", sym.DeclaringType);
        }
        [Test]
        public void K_AssemblyName_NotEmpty_ForExternalType() {
            var method = typeof(InstrVictims).GetMethod(nameof(InstrVictims.CallVictim));
            var srm = SrmByToken(method.MetadataToken);
            var sym = (IMetadataSymbol)srm.First(i =>
                i.OpCode.OperandType == OperandType.InlineMethod && i.Operand != null).Operand;
            Assert.IsNotEmpty(sym.AssemblyName);
        }
        sealed class TraceableStream : MemoryStream {
            public bool StreamDisposed { get; private set; }
            public TraceableStream(byte[] buffer) : base(buffer, writable: false) { }
            protected override void Dispose(bool disposing) {
                StreamDisposed = true;
                base.Dispose(disposing);
            }
        }
        // ── Group Lifecycle: C1/C2 stream and configuration disposal ──────────
        [Test]
        public void ResourceLifeCycle_Stream_IsDisposed_When_SrmConfiguration_IsDisposed() {
            byte[] bytes = File.ReadAllBytes(typeof(SrmEngine_Tests).Assembly.Location);
            var stream = new TraceableStream(bytes);
            var cfg = Configuration.ForAssembly(stream);
            Assert.IsFalse(stream.StreamDisposed, "stream should be open while cfg is alive");
            ((IDisposable)cfg).Dispose();
            Assert.IsTrue(stream.StreamDisposed, "stream must be disposed after cfg.Dispose()");
        }
        [Test]
        public void ResourceLifeCycle_Reader_WorksBeforeDispose() {
            byte[] bytes = File.ReadAllBytes(typeof(SrmEngine_Tests).Assembly.Location);
            var cfg = Configuration.ForAssembly(new MemoryStream(bytes));
            var sym = ((IILReaderConfiguration)cfg).FindMethods(
                typeof(ReturnTypeVictim).FullName, nameof(ReturnTypeVictim.ReturnsString)).First();
            Assert.AreEqual("System.String", sym.ReturnTypeName);
            ((IDisposable)cfg).Dispose();
        }
#pragma warning disable CS0649
        class TypeTokenVictim {
            [System.Runtime.CompilerServices.MethodImpl(
                System.Runtime.CompilerServices.MethodImplOptions.NoInlining |
                System.Runtime.CompilerServices.MethodImplOptions.NoOptimization)]
            public static bool IsInnerVictim(object o) => o is OuterVictim.InnerVictim;
        }
#pragma warning restore CS0649
        // ── Group M: SrmTypeSymbol nested DeclaringType ────────────────────────
        [Test]
        public void M_NestedType_SrmTypeSymbol_DeclaringType_NotNull() {
            var method = typeof(TypeTokenVictim).GetMethod(nameof(TypeTokenVictim.IsInnerVictim));
            var srm = SrmByToken(method.MetadataToken);
            var sym = srm.Select(i => i.Operand as ITypeSymbol)
                         .FirstOrDefault(s => s != null && s.IsNested);
            Assert.IsNotNull(sym, "expected at least one nested ITypeSymbol operand");
            Assert.IsNotNull(sym.DeclaringType, "DeclaringType must not be null for nested types");
        }
        [Test]
        public void M_NestedType_SrmTypeSymbol_DeclaringType_ContainsEnclosingType() {
            var method = typeof(TypeTokenVictim).GetMethod(nameof(TypeTokenVictim.IsInnerVictim));
            var srm = SrmByToken(method.MetadataToken);
            var sym = srm.Select(i => i.Operand as ITypeSymbol)
                         .First(s => s != null && s.IsNested);
            Assert.IsTrue(sym.DeclaringType.Contains("OuterVictim"),
                $"Expected 'OuterVictim' in DeclaringType but got: {sym.DeclaringType}");
        }
        class GenericParamVictim {
            [System.Runtime.CompilerServices.MethodImpl(
                System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
            public static void TakeDict(
                System.Collections.Generic.Dictionary<string, int> d) { _ = d; }
        }
        // ── Group M4: generic type argument comma spacing ──────────────────────
        [Test]
        public void M4_GenericParam_TypeName_HasSpaceAfterComma() {
            var cfg = SrmCfg();
            var sym = ((IILReaderConfiguration)cfg).FindMethods(
                typeof(GenericParamVictim).FullName,
                nameof(GenericParamVictim.TakeDict)).First();
            string typeName = sym.Parameters[0].TypeName;
            Assert.IsTrue(typeName.Contains(", "),
                $"Expected ', ' in generic type name but got: {typeName}");
        }
    }
}