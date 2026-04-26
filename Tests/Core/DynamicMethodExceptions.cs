#if !NET
namespace ILReader.Core.Tests {
    using System;
    using System.Reflection.Emit;
    using NUnit.Framework;
    using EHType = ILReader.Readers.ExceptionHandlerType;
    [TestFixture]
    public class BugFix_DynamicMethod_EH_Tests {
        static IILReaderConfiguration Cfg(DynamicMethod m) => Configuration.Resolve(m);
        static DynamicMethod MakeMethod(Action<ILGenerator> body) {
            var dm = new DynamicMethod("DM_EH_Test", typeof(int), new[] { typeof(int) });
            body(dm.GetILGenerator());
            // Bake: CreateDelegate triggers DynamicResolver creation which stores m_exceptions
            ((Func<int, int>)dm.CreateDelegate(typeof(Func<int, int>)))(0);
            return dm;
        }
        [Test]
        public void DynamicMethod_SingleCatch_HandlerResolved() {
            var dm = MakeMethod(il => {
                var loc = il.DeclareLocal(typeof(int));
                il.BeginExceptionBlock();
                il.Emit(OpCodes.Ldarg_0); il.Emit(OpCodes.Stloc, loc);
                il.BeginCatchBlock(typeof(Exception));
                il.Emit(OpCodes.Pop); il.Emit(OpCodes.Ldc_I4_M1); il.Emit(OpCodes.Stloc, loc);
                il.EndExceptionBlock();
                il.Emit(OpCodes.Ldloc, loc); il.Emit(OpCodes.Ret);
            });
            var handlers = Cfg(dm).GetReader(dm).ExceptionHandlers;
            Assert.AreEqual(1, handlers.Length);
            Assert.AreEqual(EHType.Catch, handlers[0].HandlerType);
            Assert.AreEqual(typeof(Exception), handlers[0].CatchType);
            Assert.IsNotNull(handlers[0].TryStart);
            Assert.IsNotNull(handlers[0].TryEnd);
            Assert.IsNotNull(handlers[0].HandlerStart);
            Assert.IsNotNull(handlers[0].HandlerEnd);
        }
        [Test]
        public void DynamicMethod_MultiCatch_AllHandlersResolved() {
            var dm = MakeMethod(il => {
                var loc = il.DeclareLocal(typeof(int));
                il.BeginExceptionBlock();
                il.Emit(OpCodes.Ldarg_0); il.Emit(OpCodes.Stloc, loc);
                il.BeginCatchBlock(typeof(DivideByZeroException));
                il.Emit(OpCodes.Pop); il.Emit(OpCodes.Ldc_I4_M1); il.Emit(OpCodes.Stloc, loc);
                il.BeginCatchBlock(typeof(OverflowException));
                il.Emit(OpCodes.Pop); il.Emit(OpCodes.Ldc_I4_S, (sbyte)-2); il.Emit(OpCodes.Stloc, loc);
                il.EndExceptionBlock();
                il.Emit(OpCodes.Ldloc, loc); il.Emit(OpCodes.Ret);
            });
            var handlers = Cfg(dm).GetReader(dm).ExceptionHandlers;
            Assert.AreEqual(2, handlers.Length);
            Assert.AreEqual(typeof(DivideByZeroException), handlers[0].CatchType);
            Assert.AreEqual(typeof(OverflowException), handlers[1].CatchType);
            Assert.AreEqual(handlers[0].TryStart.Offset, handlers[1].TryStart.Offset,
                "Both handlers protect the same try body");
            Assert.LessOrEqual(handlers[0].HandlerEnd.Offset, handlers[1].HandlerStart.Offset,
                "Handlers must be sequential");
        }
        [Test]
        public void DynamicMethod_TryFinally_HandlerResolved() {
            var dm = MakeMethod(il => {
                var loc = il.DeclareLocal(typeof(int));
                il.BeginExceptionBlock();
                il.Emit(OpCodes.Ldarg_0); il.Emit(OpCodes.Stloc, loc);
                il.BeginFinallyBlock();
                il.Emit(OpCodes.Nop);
                il.EndExceptionBlock();
                il.Emit(OpCodes.Ldloc, loc); il.Emit(OpCodes.Ret);
            });
            var handlers = Cfg(dm).GetReader(dm).ExceptionHandlers;
            Assert.AreEqual(1, handlers.Length);
            Assert.AreEqual(EHType.Finally, handlers[0].HandlerType);
            Assert.IsNull(handlers[0].CatchType, "Finally handler must have no CatchType");
            Assert.IsNotNull(handlers[0].TryStart);
            Assert.IsNotNull(handlers[0].HandlerStart);
            Assert.IsNotNull(handlers[0].HandlerEnd);
        }
        [Test]
        public void DynamicMethod_TryFilter_HandlerResolved() {
            DynamicMethod dm;
            try {
                dm = MakeMethod(il => {
                    var loc = il.DeclareLocal(typeof(int));
                    il.BeginExceptionBlock();
                    il.Emit(OpCodes.Ldarg_0); il.Emit(OpCodes.Stloc, loc);
                    il.BeginExceptFilterBlock();
                    il.Emit(OpCodes.Isinst, typeof(Exception));
                    il.Emit(OpCodes.Ldnull); il.Emit(OpCodes.Cgt_Un);
                    il.BeginCatchBlock(null);   // filtered handler body
                    il.Emit(OpCodes.Pop); il.Emit(OpCodes.Ldc_I4_M1); il.Emit(OpCodes.Stloc, loc);
                    il.EndExceptionBlock();
                    il.Emit(OpCodes.Ldloc, loc); il.Emit(OpCodes.Ret);
                });
            }
            catch(NotSupportedException) {
                Assert.Ignore("Filter blocks are not supported in DynamicILGenerator on this runtime");
                return;
            }
            var handlers = Cfg(dm).GetReader(dm).ExceptionHandlers;
            Assert.AreEqual(1, handlers.Length);
            Assert.AreEqual(EHType.Filter, handlers[0].HandlerType);
            Assert.IsNull(handlers[0].CatchType, "Filter handler must have no CatchType");
            Assert.IsNotNull(handlers[0].FilterStart, "Filter handler must have a FilterStart");
            Assert.IsNotNull(handlers[0].HandlerStart);
            Assert.IsNotNull(handlers[0].HandlerEnd);
        }
    }
}
#endif
