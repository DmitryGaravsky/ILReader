#if DEBUGTEST
namespace ILReader.Analyzer.Tests {
    using System;
    using System.Reflection.Emit;
    using ILReader.Readers;
    using NUnit.Framework;

    [TestFixture]
    public class ILPattern_Tests {
#region Test Classes
        Action<Foo, Action> subscribe = (foo, execute) => foo.Click += (s, e) => execute();
        class Foo {
            public event EventHandler Click {
                add { }
                remove { }
            }
        }
        class Bar {
            public void Stub() {
                throw new NotImplementedException("Stub");
            }
        }
        static IILReader GetReader(System.Reflection.MethodBase method) {
            return ILReader.Configuration.Resolve(method).GetReader(method);
        }
#endregion Test Classes
        [Test]
        public void Test_SubscribePattern() {
            var pattern = Analyzer.Subscribe.Instance;
            var reader = GetReader(subscribe.Method);
            Assert.IsTrue(pattern.Match(reader));
            Assert.AreEqual(7, pattern.StartIndex);
            Assert.AreEqual(3, pattern.Result.Length);
        }
        [Test]
        public void Test_NotImplementedPattern() {
            var pattern = Analyzer.NotImplemented.Instance;
            var reader = GetReader(typeof(Bar).GetMethod("Stub"));
            Assert.IsTrue(pattern.Match(reader));
            Assert.AreEqual(0, pattern.StartIndex);
            Assert.AreEqual(3, pattern.Result.Length);
        }
        [Test]
        public void Test_NotSupportedPattern() {
            var pattern = Analyzer.NotSupported.Instance;
            DynamicMethod dm = new DynamicMethod("m", null, null);
            var dmGenType = dm.GetILGenerator().GetType();
            var reader = GetReader(dmGenType.GetMethod("BeginExceptFilterBlock"));
            Assert.IsTrue(pattern.Match(reader));
            Assert.AreEqual(0, pattern.StartIndex);
            Assert.AreEqual(3, pattern.Result.Length);
        }
    }
}
#endif