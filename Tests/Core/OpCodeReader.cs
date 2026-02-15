namespace ILReader.Core.Tests {
    using System.Reflection;
    using System.Reflection.Emit;
    using ILReader.Readers;
    using NUnit.Framework;

    [TestFixture]
    public class OpCodeReader_Tests {
        [Test]
        public void KnownInstruction_ShouldBeMarkedAsKnown() {
            var info = new OpCodeInfo(OpCodes.Nop, 1);
            Assert.IsFalse(info.IsUnknown);
            Assert.AreEqual(1, info.Size);
            Assert.IsNull(info.RawByte);
        }
        [Test]
        public void UnknownInstruction_ShouldBeMarkedAsUnknown() {
            var unknown = new OpCodeInfo(1, 0xFF);
            Assert.IsTrue(unknown.IsUnknown);
            Assert.AreEqual(OpCodeCategory.Unknown, unknown.Category);
            Assert.AreEqual(1, unknown.Size);
            Assert.AreEqual(0xFF, unknown.RawByte);
        }
        [Test]
        public void UnknownInstruction_ShouldPreserveRawByte() {
            var unknown1 = new OpCodeInfo(1, 0xAB);
            var unknown2 = new OpCodeInfo(2, 0xCD);
            Assert.AreEqual(0xAB, unknown1.RawByte);
            Assert.AreEqual(0xCD, unknown2.RawByte);
        }
        [Test]
        public void LoadInstructions_ShouldHaveLoadCategory() {
            TestCategory(OpCodes.Ldarg, OpCodeCategory.Load);
            TestCategory(OpCodes.Ldarg_0, OpCodeCategory.Load);
            TestCategory(OpCodes.Ldc_I4, OpCodeCategory.Load);
            TestCategory(OpCodes.Ldfld, OpCodeCategory.Load);
            TestCategory(OpCodes.Ldloc, OpCodeCategory.Load);
        }
        [Test]
        public void StoreInstructions_ShouldHaveStoreCategory() {
            TestCategory(OpCodes.Starg, OpCodeCategory.Store);
            TestCategory(OpCodes.Stfld, OpCodeCategory.Store);
            TestCategory(OpCodes.Stloc, OpCodeCategory.Store);
            TestCategory(OpCodes.Stloc_0, OpCodeCategory.Store);
        }
        [Test]
        public void CallInstructions_ShouldHaveCallCategory() {
            TestCategory(OpCodes.Call, OpCodeCategory.Call);
            TestCategory(OpCodes.Callvirt, OpCodeCategory.Call);
            TestCategory(OpCodes.Calli, OpCodeCategory.Call);
        }
        [Test]
        public void BranchInstructions_ShouldHaveBranchCategory() {
            TestCategory(OpCodes.Br, OpCodeCategory.Branch);
            TestCategory(OpCodes.Br_S, OpCodeCategory.Branch);
            TestCategory(OpCodes.Beq, OpCodeCategory.Branch);
            TestCategory(OpCodes.Bne_Un, OpCodeCategory.Branch);
            TestCategory(OpCodes.Bge, OpCodeCategory.Branch);
            TestCategory(OpCodes.Bgt, OpCodeCategory.Branch);
            TestCategory(OpCodes.Ble, OpCodeCategory.Branch);
            TestCategory(OpCodes.Blt, OpCodeCategory.Branch);
            TestCategory(OpCodes.Switch, OpCodeCategory.Branch);
        }
        [Test]
        public void ComparisonInstructions_ShouldHaveComparisonCategory() {
            TestCategory(OpCodes.Ceq, OpCodeCategory.Comparison);
            TestCategory(OpCodes.Cgt, OpCodeCategory.Comparison);
            TestCategory(OpCodes.Cgt_Un, OpCodeCategory.Comparison);
            TestCategory(OpCodes.Clt, OpCodeCategory.Comparison);
            TestCategory(OpCodes.Clt_Un, OpCodeCategory.Comparison);
        }
        [Test]
        public void ArithmeticInstructions_ShouldHaveArithmeticCategory() {
            TestCategory(OpCodes.Add, OpCodeCategory.Arithmetic);
            TestCategory(OpCodes.Add_Ovf, OpCodeCategory.Arithmetic);
            TestCategory(OpCodes.Sub, OpCodeCategory.Arithmetic);
            TestCategory(OpCodes.Sub_Ovf_Un, OpCodeCategory.Arithmetic);
            TestCategory(OpCodes.Mul, OpCodeCategory.Arithmetic);
            TestCategory(OpCodes.Div, OpCodeCategory.Arithmetic);
            TestCategory(OpCodes.Div_Un, OpCodeCategory.Arithmetic);
            TestCategory(OpCodes.Rem, OpCodeCategory.Arithmetic);
            TestCategory(OpCodes.Neg, OpCodeCategory.Arithmetic);
        }
        [Test]
        public void UnboxInstruction_ShouldNotHaveArithmeticCategory() {
            var info = new OpCodeInfo(OpCodes.Unbox, OpCodes.Unbox.Size);
            Assert.IsFalse(info.Category.HasFlag(OpCodeCategory.Arithmetic));
            Assert.IsTrue(info.Category.HasFlag(OpCodeCategory.Conversion));
        }
        [Test]
        public void BitwiseInstructions_ShouldHaveBitwiseCategory() {
            TestCategory(OpCodes.And, OpCodeCategory.Bitwise);
            TestCategory(OpCodes.Or, OpCodeCategory.Bitwise);
            TestCategory(OpCodes.Xor, OpCodeCategory.Bitwise);
            TestCategory(OpCodes.Not, OpCodeCategory.Bitwise);
            TestCategory(OpCodes.Shl, OpCodeCategory.Bitwise);
            TestCategory(OpCodes.Shr, OpCodeCategory.Bitwise);
            TestCategory(OpCodes.Shr_Un, OpCodeCategory.Bitwise);
        }
        [Test]
        public void ConversionInstructions_ShouldHaveConversionCategory() {
            TestCategory(OpCodes.Conv_I4, OpCodeCategory.Conversion);
            TestCategory(OpCodes.Conv_U8, OpCodeCategory.Conversion);
            TestCategory(OpCodes.Conv_R8, OpCodeCategory.Conversion);
            TestCategory(OpCodes.Castclass, OpCodeCategory.Conversion);
            TestCategory(OpCodes.Isinst, OpCodeCategory.Conversion);
            TestCategory(OpCodes.Box, OpCodeCategory.Conversion);
            TestCategory(OpCodes.Unbox, OpCodeCategory.Conversion);
            TestCategory(OpCodes.Unbox_Any, OpCodeCategory.Conversion);
        }
        [Test]
        public void ArrayInstructions_ShouldHaveArrayCategory() {
            TestCategory(OpCodes.Newarr, OpCodeCategory.Array);
            TestCategory(OpCodes.Ldlen, OpCodeCategory.Array | OpCodeCategory.Load);
            TestCategory(OpCodes.Ldelem, OpCodeCategory.Array | OpCodeCategory.Load);
            TestCategory(OpCodes.Ldelem_I4, OpCodeCategory.Array | OpCodeCategory.Load);
            TestCategory(OpCodes.Stelem, OpCodeCategory.Array | OpCodeCategory.Store);
            TestCategory(OpCodes.Stelem_I4, OpCodeCategory.Array | OpCodeCategory.Store);
        }
        [Test]
        public void ObjectInstructions_ShouldHaveObjectCategory() {
            TestCategory(OpCodes.Newobj, OpCodeCategory.Object);
            TestCategory(OpCodes.Initobj, OpCodeCategory.Object);
            TestCategory(OpCodes.Cpobj, OpCodeCategory.Object);
            TestCategory(OpCodes.Ldobj, OpCodeCategory.Object | OpCodeCategory.Load);
            TestCategory(OpCodes.Stobj, OpCodeCategory.Object | OpCodeCategory.Store);
        }
        [Test]
        public void StackInstructions_ShouldHaveStackCategory() {
            TestCategory(OpCodes.Pop, OpCodeCategory.Stack);
            TestCategory(OpCodes.Dup, OpCodeCategory.Stack);
        }
        [Test]
        public void ReturnInstruction_ShouldHaveReturnCategory() {
            TestCategory(OpCodes.Ret, OpCodeCategory.Return);
        }
        [Test]
        public void ExceptionInstructions_ShouldHaveExceptionCategory() {
            TestCategory(OpCodes.Throw, OpCodeCategory.Exception);
            TestCategory(OpCodes.Rethrow, OpCodeCategory.Exception);
            TestCategory(OpCodes.Leave, OpCodeCategory.Exception);
            TestCategory(OpCodes.Leave_S, OpCodeCategory.Exception);
            TestCategory(OpCodes.Endfinally, OpCodeCategory.Exception);
            TestCategory(OpCodes.Endfilter, OpCodeCategory.Exception);
        }
        [Test]
        public void EndFinallyInstruction_ShouldNotHavePointerCategory() {
            var info = new OpCodeInfo(OpCodes.Endfinally, OpCodes.Endfinally.Size);
            Assert.IsFalse(info.Category.HasFlag(OpCodeCategory.Pointer));
            Assert.IsTrue(info.Category.HasFlag(OpCodeCategory.Exception));
        }
        [Test]
        public void PointerInstructions_ShouldHavePointerCategory() {
            TestCategory(OpCodes.Ldind_I, OpCodeCategory.Pointer | OpCodeCategory.Load);
            TestCategory(OpCodes.Ldind_I4, OpCodeCategory.Pointer | OpCodeCategory.Load);
            TestCategory(OpCodes.Stind_I, OpCodeCategory.Pointer | OpCodeCategory.Store);
            TestCategory(OpCodes.Stind_I4, OpCodeCategory.Pointer | OpCodeCategory.Store);
            TestCategory(OpCodes.Localloc, OpCodeCategory.Pointer);
            TestCategory(OpCodes.Cpblk, OpCodeCategory.Pointer);
            TestCategory(OpCodes.Initblk, OpCodeCategory.Pointer);
        }
        [Test]
        public void PrefixInstructions_ShouldHavePrefixCategory() {
            TestCategory(OpCodes.Tailcall, OpCodeCategory.Prefix);
            TestCategory(OpCodes.Volatile, OpCodeCategory.Prefix);
            TestCategory(OpCodes.Readonly, OpCodeCategory.Prefix);
            TestCategory(OpCodes.Unaligned, OpCodeCategory.Prefix);
            TestCategory(OpCodes.Constrained, OpCodeCategory.Prefix);
        }
        [Test]
        public void UtilityInstructions_ShouldHaveUtilityCategory() {
            TestCategory(OpCodes.Nop, OpCodeCategory.Utility);
            TestCategory(OpCodes.Break, OpCodeCategory.Utility);
        }
        [Test]
        public void InternalInstructions_ShouldBeMarkedAsInternal() {
            var allOpCodes = typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static);
            int internalCount = 0;
            foreach(var field in allOpCodes) {
                OpCode opCode = (OpCode)field.GetValue(null);
                if(opCode.OpCodeType == OpCodeType.Nternal) {
                    var info = new OpCodeInfo(opCode, opCode.Size);
                    Assert.IsTrue(info.IsInternal, $"{opCode.Name} should be marked as Internal");
                    Assert.IsTrue(info.Category.HasFlag(OpCodeCategory.Internal));
                    internalCount++;
                }
            }
            Assert.Greater(internalCount, 0, "Should have at least one internal OpCode");
        }
        [Test]
        public void UnknownInstruction_ToString_ShouldShowHexValue() {
            var unknown = new OpCodeInfo(1, 0xAB);
            string str = unknown.ToString();
            Assert.IsTrue(str.Contains("unknown"));
            Assert.IsTrue(str.Contains("AB"));
        }
        [Test]
        public void KnownInstruction_ToString_ShouldShowName() {
            var info = new OpCodeInfo(OpCodes.Add, OpCodes.Add.Size);
            string str = info.ToString();
            Assert.IsTrue(str.Contains("add"));
        }
        //
        static void TestCategory(OpCode opCode, OpCodeCategory expectedCategory) {
            OpCodeInfo info = new OpCodeInfo(opCode, opCode.Size);
            OpCodeCategory missing = expectedCategory & ~info.Category;
            Assert.AreEqual(OpCodeCategory.None, missing,
                $"{opCode.Name}: Missing categories: {missing}. " +
                $"Expected: {expectedCategory}, Got: {info.Category}");
        }
    }
}