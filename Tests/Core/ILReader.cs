namespace ILReader.Core.Tests {
    using System;
    using System.Diagnostics.CodeAnalysis;
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
#if RELEASE
            Assert.AreEqual(11, reader.Count());
#else
            Assert.AreEqual(12, reader.Count());
#endif
            Assert.AreEqual(OpCodes.Newobj, reader.First().OpCode);
        }
        [Test]
        public void Test_ReadMetadata() {
            var reader = cfg.GetReader(typeof(FooBar).GetMethod("Sum"));
#if RELEASE
            Assert.AreEqual(13, reader.Count());
#else
            Assert.AreEqual(17, reader.Count());
#endif
            var args = reader.Metadata.First();
            Assert.IsTrue(args.HasChildren);
            Assert.AreEqual(5, args.Children.Count());
            var codeSize = reader.Metadata.ElementAt(1);
            var maxStackSize = reader.Metadata.ElementAt(2);
#if RELEASE
            Assert.AreEqual(19, codeSize.Value);
            Assert.AreEqual(8, maxStackSize.Value);
            Assert.IsFalse(reader.Metadata.Any(x => x.Name == ".locals"));
#else
            Assert.AreEqual(24, codeSize.Value);
            Assert.AreEqual(2, maxStackSize.Value);
            var locals = reader.Metadata.ElementAt(3);
            Assert.IsTrue(locals.HasChildren);
            Assert.AreEqual(1, locals.Children.Count());
#endif
        }
        [Test]
        public void Test_ReadExceptionBlocks() {
            AppDomain.CurrentDomain.AssemblyResolve += OnAsemblyResolve;
            var reader = cfg.GetReader(typeof(FooBar).GetMethod("Divide"));
#if RELEASE
            Assert.AreEqual(11, reader.Count());
#else
            Assert.AreEqual(14, reader.Count());
#endif
            var args = reader.Metadata.First();
            Assert.IsTrue(args.HasChildren);
            Assert.AreEqual(2, args.Children.Count());
            var codeSize = reader.Metadata.ElementAt(1);
#if RELEASE
            Assert.AreEqual(13, codeSize.Value);
#else
            Assert.AreEqual(16, codeSize.Value);
#endif
            var maxStackSize = reader.Metadata.ElementAt(2);
            Assert.AreEqual(2, maxStackSize.Value);
            Assert.AreEqual(1, reader.ExceptionHandlers.Length);
            var ex = reader.ExceptionHandlers[0];
            Assert.AreEqual(ExceptionHandlerType.Catch, ex.HandlerType);
            Assert.AreEqual(typeof(DivideByZeroException), ex.CatchType);
        }
        static Assembly OnAsemblyResolve(object sender, ResolveEventArgs args) {
            return null;
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
                            bool isAbstract = methods[m].IsAbstract;
                            bool isInternalCall = (methods[m].GetMethodImplementationFlags() & MethodImplAttributes.InternalCall)
                                == MethodImplAttributes.InternalCall;
                            Assert.IsTrue(isAbstract || isInternalCall);
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
#if !NET
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
#endif
}