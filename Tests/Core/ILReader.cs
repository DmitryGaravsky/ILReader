namespace ILReader.Core.Tests {
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using ILReader.Readers;
    using NUnit.Framework;

    [TestFixture]
    public class ILReader_Tests {
        readonly IILReaderConfiguration cfg = StandardConfiguration.Default;
        #region Test Classes
        readonly Action<Foo, Action> subscribe = (foo, execute) => foo.Click += (s, e) => execute();
        class Foo {
            public event EventHandler Click {
                add { }
                remove { }
            }
        }
        class FooBar {
            readonly int val = 42;
            public int Sum(int a, int b, int c, int d, int e) {
                return val + a + b + c + d + e;
            }
            public int Divide(int a, int b) {
                try { return a / b; }
                catch(DivideByZeroException) { return 0; }
            }
        }
        #endregion Test Classes
        [Test]
        public void Test_ReadInstructions() {
            var reader = cfg.GetReader(subscribe.Method);
            Assert.AreEqual(12, reader.Count());
            Assert.AreEqual(OpCodes.Newobj, reader.First().OpCode);
        }
        [Test]
        public void Test_ReadMetadata() {
            var reader = cfg.GetReader(typeof(FooBar).GetMethod("Sum"));
            Assert.AreEqual(17, reader.Count());
            var args = reader.Metadata.First();
            Assert.IsTrue(args.HasChildren);
            Assert.AreEqual(5, args.Children.Count());
            var codeSize = reader.Metadata.ElementAt(1);
            Assert.AreEqual(24, codeSize.Value);
            var maxStackSize = reader.Metadata.ElementAt(2);
            Assert.AreEqual(2, maxStackSize.Value);
            var locals = reader.Metadata.ElementAt(3);
            Assert.IsTrue(locals.HasChildren);
            Assert.AreEqual(1, locals.Children.Count());
        }
        [Test]
        public void Test_ReadExceptionBlocks() {
            var reader = cfg.GetReader(typeof(FooBar).GetMethod("Divide"));
            Assert.AreEqual(14, reader.Count());
            var args = reader.Metadata.First();
            Assert.IsTrue(args.HasChildren);
            Assert.AreEqual(2, args.Children.Count());
            var codeSize = reader.Metadata.ElementAt(1);
            Assert.AreEqual(16, codeSize.Value);
            var maxStackSize = reader.Metadata.ElementAt(2);
            Assert.AreEqual(2, maxStackSize.Value);
            Assert.AreEqual(1, reader.ExceptionHandlers.Length);
            var ex = reader.ExceptionHandlers[0];
            Assert.AreEqual(ExceptionHandlerType.Catch, ex.HandlerType);
            Assert.AreEqual(typeof(DivideByZeroException), ex.CatchType);
        }
        [Test]
        public void Test_ReadInstructions_Stress() {
            try {
                var types = typeof(IILReaderFactory).Assembly.GetTypes();
                for(int i = 0; i < types.Length; i++) {
                    var methods = types[i].GetMethods();
                    for(int m = 0; m < methods.Length; m++) {
                        var reader = cfg.GetReader(subscribe.Method);
                        var results = reader.ToArray();
                        if(results.Length == 0) {
                            Assert.IsTrue(methods[m].IsAbstract || ((methods[m].GetMethodImplementationFlags() & MethodImplAttributes.InternalCall) == MethodImplAttributes.InternalCall));
                        }
                    }
                }
            }
            catch { Assert.Fail(); }
        }
        [Test]
        public void Test_ReadInstructions_External() {
            var m = typeof(object).GetMethod("GetType");
            var reader = cfg.GetReader(m);
            Assert.AreEqual(0, reader.Count());
        }
    }
    [TestFixture]
    public class ILReader_Tests_DynamicMethod {
        static DynamicMethod method;
        static Func<int> Sum;
        [SetUp]
        public void SetUp() {
            if(method == null) {
                method = new DynamicMethod("Sum", typeof(int), null);
                var ilGen = method.GetILGenerator();
                ilGen.Emit(OpCodes.Ldc_I4, 2);
                ilGen.Emit(OpCodes.Ldc_I4, 2);
                ilGen.Emit(OpCodes.Add);
                ilGen.Emit(OpCodes.Ret);
                Sum = method.CreateDelegate(typeof(Func<int>)) as Func<int>;
            }
        }
        [Test]
        public void Test_ReadInstructions() {
            IILReaderConfiguration cfg = Configuration.Resolve(method);
            Assert.IsTrue(cfg is DynamicMethodConfiguration);
            var reader = cfg.GetReader(method);
            Assert.AreEqual(4, reader.Count());
        }
        [Test]
        public void Test_ReadInstructions_RTDynamicMethod() {
            IILReaderConfiguration cfg = Configuration.Resolve(Sum.Method);
            Assert.IsTrue(cfg is RTDynamicMethodConfiguration);
            var reader = cfg.GetReader(Sum.Method);
            Assert.AreEqual(4, reader.Count());
        }
    }
}