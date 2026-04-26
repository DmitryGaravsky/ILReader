namespace ILReader.Core.Tests {
    using System.IO;
    using System.Linq;
    using System.Reflection.Emit;
    using ILReader.Dump;
    using NUnit.Framework;
    using EH = ILReader.Readers.ExceptionHandler;
    using EHType = ILReader.Readers.ExceptionHandlerType;
    [TestFixture]
    public class BugFix_Tests {
        readonly IILReaderConfiguration cfg = StandardConfiguration.Default;
        class Victim {
            public int Divide(int a, int b) {
                try { return a / b; }
                catch(System.DivideByZeroException) { return 0; }
            }
            public int BackwardLoop(int n) {
                int s = 0;
                // do-while guarantees a backward conditional short branch (bgt.s / blt.s)
                do { s += n; n--; }
                while(n > 0);
                return s;
            }
            public int MultiCatch(int a, int b) {
                try { return a / b; }
                catch(System.DivideByZeroException) { return -1; }
                catch(System.OverflowException) { return -2; }
            }
            public int Nested(int a, int b) {
                try {
                    try { return a / b; }
                    catch(System.DivideByZeroException) { return 0; }
                }
                catch(System.Exception) { return -1; }
            }
            public int SequentialCatches(int a, int b) {
                int r = 0;
                try { r = a / b; }
                catch(System.DivideByZeroException) { r = -1; }
                try { r += a / (b + 1); }
                catch(System.DivideByZeroException) { }
                return r;
            }
            public int WithFinally(int a, int b) {
                int r = 0;
                try { r = a / b; }
                finally { r += 1; }
                return r;
            }
            public int WithCatchAndFinally(int a, int b) {
                int r = 0;
                try { r = a / b; }
                catch(System.DivideByZeroException) { r = -1; }
                finally { r += 1; }
                return r;
            }
        }
        [Test]
        public void Dump_MethodWithInstructions_WritesContent() {
            var method = typeof(Victim).GetMethod(nameof(Victim.Divide));
            var reader = cfg.GetReader(method);
            Assert.Greater(reader.Count, 0, "Precondition: method should have instructions");
            using(var stream = new MemoryStream()) {
                ((ISupportDump)reader).Dump(stream);
                Assert.Greater(stream.Length, 0L,
                    "Dump for a method with instructions must write content");
            }
        }
        [Test]
        public void Dump_MethodWithNoInstructions_WritesNothing() {
            // object.GetType() is an InternalCall — no managed IL body → 0 instructions
            var method = typeof(object).GetMethod("GetType");
            var reader = cfg.GetReader(method);
            Assert.AreEqual(0, reader.Count, "Precondition: InternalCall method must have 0 instructions");
            using(var stream = new MemoryStream()) {
                ((ISupportDump)reader).Dump(stream);
                Assert.AreEqual(0L, stream.Length,
                    "Dump for a 0-instruction method must write nothing (arr.Length > 0 guard)");
            }
        }
        [Test]
        public void ExceptionHandler_Advance_InstructionAtHandlerEnd_HasZeroDepth() {
            var method = typeof(Victim).GetMethod(nameof(Victim.Divide));
            var reader = cfg.GetReader(method);
            var handlers = reader.ExceptionHandlers;
            Assert.AreEqual(1, handlers.Length, "Precondition: Divide must have exactly 1 exception handler");
            var eh = handlers[0];
            Assert.AreEqual(0, reader[eh.HandlerEnd.Index].Depth,
                "Instruction at HandlerEnd is outside the handler — its Depth must remain 0");
        }
        [Test]
        public void ExceptionHandler_Advance_InstructionsInsideHandler_HavePositiveDepth() {
            var method = typeof(Victim).GetMethod(nameof(Victim.Divide));
            var reader = cfg.GetReader(method);
            var eh = reader.ExceptionHandlers[0];
            for(int i = eh.HandlerStart.Index; i < eh.HandlerEnd.Index; i++) {
                Assert.Greater(reader[i].Depth, 0,
                    $"Instruction [{i}] is inside the catch handler and must have Depth > 0");
            }
        }
        [Test]
        public void ExceptionHandler_Advance_InstructionsInsideTry_HavePositiveDepth() {
            var method = typeof(Victim).GetMethod(nameof(Victim.Divide));
            var reader = cfg.GetReader(method);
            var eh = reader.ExceptionHandlers[0];
            for(int i = eh.TryStart.Index; i < eh.TryEnd.Index; i++) {
                Assert.Greater(reader[i].Depth, 0,
                    $"Instruction [{i}] is inside the try block and must have Depth > 0");
            }
        }
        [Test]
        public void ShortInlineBrTarget_Operand_IsSByte() {
            // Divide() contains leave.s — always present in both Debug and Release
            var method = typeof(Victim).GetMethod(nameof(Victim.Divide));
            var reader = cfg.GetReader(method);
            var shortBranch = reader.FirstOrDefault(
                i => i.OpCode.OperandType == OperandType.ShortInlineBrTarget);
            Assert.IsNotNull(shortBranch, "Divide must contain at least one ShortInlineBrTarget instruction");
            Assert.IsInstanceOf<sbyte>(shortBranch.Operand,
                "ShortInlineBrTarget operand must be sbyte, not byte");
        }
        [Test]
        public void ShortInlineBrTarget_BackwardBranch_IsNegative() {
            var method = typeof(Victim).GetMethod(nameof(Victim.BackwardLoop));
            var reader = cfg.GetReader(method);
            var backwardBranch = reader.FirstOrDefault(
                i => i.OpCode.OperandType == OperandType.ShortInlineBrTarget
                  && i.Operand is sbyte s && s < 0);
            Assert.IsNotNull(backwardBranch,
                "BackwardLoop must contain a backward ShortInlineBrTarget instruction");
            Assert.Less((sbyte)backwardBranch.Operand, 0,
                "Backward branch operand must be a negative sbyte");
        }
        static void AssertHandlerBoundaries(EH eh, string label) {
            Assert.IsNotNull(eh.TryStart, $"{label}: TryStart must not be null");
            Assert.IsNotNull(eh.TryEnd, $"{label}: TryEnd must not be null");
            Assert.IsNotNull(eh.HandlerStart, $"{label}: HandlerStart must not be null");
            Assert.IsNotNull(eh.HandlerEnd, $"{label}: HandlerEnd must not be null");
            Assert.LessOrEqual(eh.TryStart.Offset, eh.TryEnd.Offset,
                $"{label}: TryStart.Offset must be <= TryEnd.Offset");
            Assert.LessOrEqual(eh.TryEnd.Offset, eh.HandlerStart.Offset,
                $"{label}: TryEnd.Offset must be <= HandlerStart.Offset");
            Assert.Less(eh.HandlerStart.Offset, eh.HandlerEnd.Offset,
                $"{label}: HandlerStart.Offset must be < HandlerEnd.Offset");
        }
        [Test]
        public void ExceptionHandlerCursor_SingleCatch_ResolvedCorrectly() {
            var reader = cfg.GetReader(typeof(Victim).GetMethod(nameof(Victim.Divide)));
            var handlers = reader.ExceptionHandlers;
            Assert.AreEqual(1, handlers.Length);
            AssertHandlerBoundaries(handlers[0], "catch");
            Assert.AreEqual(EHType.Catch, handlers[0].HandlerType);
            Assert.AreEqual(typeof(System.DivideByZeroException), handlers[0].CatchType);
        }
        [Test]
        public void ExceptionHandlerCursor_TryFinally_ResolvedCorrectly() {
            var reader = cfg.GetReader(typeof(Victim).GetMethod(nameof(Victim.WithFinally)));
            var handlers = reader.ExceptionHandlers;
            Assert.AreEqual(1, handlers.Length);
            AssertHandlerBoundaries(handlers[0], "finally");
            Assert.AreEqual(EHType.Finally, handlers[0].HandlerType);
            Assert.IsNull(handlers[0].CatchType, "Finally handler must have no CatchType");
        }
        [Test]
        public void ExceptionHandlerCursor_MultiCatch_AllHandlersResolvedCorrectly() {
            var reader = cfg.GetReader(typeof(Victim).GetMethod(nameof(Victim.MultiCatch)));
            var handlers = reader.ExceptionHandlers;
            Assert.AreEqual(2, handlers.Length, "MultiCatch must produce 2 handlers");
            AssertHandlerBoundaries(handlers[0], "catch[0]");
            AssertHandlerBoundaries(handlers[1], "catch[1]");
            Assert.AreEqual(handlers[0].TryStart.Offset, handlers[1].TryStart.Offset,
                "Both catch blocks protect the same try body — TryStart offsets must match");
            Assert.AreEqual(handlers[0].TryEnd.Offset, handlers[1].TryEnd.Offset,
                "Both catch blocks protect the same try body — TryEnd offsets must match");
            Assert.LessOrEqual(handlers[0].HandlerEnd.Offset, handlers[1].HandlerStart.Offset,
                "Handlers must be laid out sequentially in IL");
        }
        [Test]
        public void ExceptionHandlerCursor_NestedTryCatch_AllHandlersResolvedCorrectly() {
            var reader = cfg.GetReader(typeof(Victim).GetMethod(nameof(Victim.Nested)));
            var handlers = reader.ExceptionHandlers;
            Assert.AreEqual(2, handlers.Length, "Nested must produce 2 handlers (inner + outer)");
            AssertHandlerBoundaries(handlers[0], "inner");
            AssertHandlerBoundaries(handlers[1], "outer");
            Assert.LessOrEqual(handlers[1].TryStart.Offset, handlers[0].TryStart.Offset,
                "Outer TryStart must be <= inner TryStart");
            Assert.GreaterOrEqual(handlers[1].TryEnd.Offset, handlers[0].HandlerEnd.Offset,
                "Outer TryEnd must be >= inner HandlerEnd");
        }
        [Test]
        public void ExceptionHandlerCursor_SequentialTryCatch_AllHandlersResolvedCorrectly() {
            var reader = cfg.GetReader(typeof(Victim).GetMethod(nameof(Victim.SequentialCatches)));
            var handlers = reader.ExceptionHandlers;
            Assert.AreEqual(2, handlers.Length, "Sequential must produce 2 handlers");
            AssertHandlerBoundaries(handlers[0], "first");
            AssertHandlerBoundaries(handlers[1], "second");
            Assert.LessOrEqual(handlers[0].HandlerEnd.Offset, handlers[1].TryStart.Offset,
                "First handler must end before the second try block starts");
        }
        [Test]
        public void ExceptionHandlerCursor_CatchAndFinally_BothHandlersResolvedCorrectly() {
            var reader = cfg.GetReader(typeof(Victim).GetMethod(nameof(Victim.WithCatchAndFinally)));
            var handlers = reader.ExceptionHandlers;
            Assert.AreEqual(2, handlers.Length, "WithCatchAndFinally must produce 2 handlers");
            var catchH = handlers.FirstOrDefault(h => h.HandlerType == EHType.Catch);
            var finallyH = handlers.FirstOrDefault(h => h.HandlerType == EHType.Finally);
            Assert.IsNotNull(catchH, "catch handler must be present");
            Assert.IsNotNull(finallyH, "finally handler must be present");
            AssertHandlerBoundaries(catchH, "catch");
            AssertHandlerBoundaries(finallyH, "finally");
            Assert.AreEqual(typeof(System.DivideByZeroException), catchH.CatchType);
            Assert.IsNull(finallyH.CatchType, "finally handler must have no CatchType");
        }
    }
}
