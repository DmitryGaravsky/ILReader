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
    [TestFixture]
    public class Boxing_Patterns_Tests {
        #region Test Classes
        class Format {
            public object Stub1(int a, int b) {
                return a == b;
            }
            public string Stub2(int a, int b) {
                return string.Format("{0}{1}", a, b);
            }
            public string Stub3(int a, int b) {
                return string.Format("{0}{1}", a.ToString(), b.ToString());
            }
        }
        class Concat {
            public object Stub1(int a, int b) {
                return a == b;
            }
            public string Stub2(int a, int b) {
                return string.Concat(a, b);
            }
            public string Stub3(int a, int b) {
                return string.Concat(a.ToString(), b.ToString());
            }
        }
        class EnumHasFlag {
            [Flags]
            public enum State { One, Two }
            public object Stub1(State a, State b) {
                return a == b;
            }
            public bool Stub2(State state) {
                return state.HasFlag(State.One);
            }
            public bool Stub3(State state) {
                return (state & State.One) == State.One;
            }
        }
        #endregion Test Classes
        [Test]
        public void Test_Box() {
            var pattern = Analyzer.Box.Instance;
            var reader = GetReader(typeof(Format).GetMethod("Stub1"));
            Assert.IsTrue(pattern.Match(reader));
            Assert.AreEqual(3, pattern.StartIndex);
            Assert.AreEqual(1, pattern.Result.Length);
            reader = GetReader(typeof(Format).GetMethod("Stub2"));
            Assert.IsTrue(pattern.Match(reader));
            Assert.AreEqual(2, pattern.StartIndex);
            Assert.AreEqual(1, pattern.Result.Length);
            reader = GetReader(typeof(Format).GetMethod("Stub3"));
            Assert.IsFalse(pattern.Match(reader));
            Assert.AreEqual(-1, pattern.StartIndex);
            Assert.AreEqual(0, pattern.Result.Length);
        }
        [Test]
        public void Test_StringFormatBoxing() {
            var pattern = Analyzer.StringFormatBoxing.Instance;
            var reader = GetReader(typeof(Format).GetMethod("Stub1"));
            Assert.IsFalse(pattern.Match(reader));
            Assert.IsFalse(pattern.Success);
            Assert.AreEqual(3, pattern.StartIndex);
            Assert.AreEqual(0, pattern.Result.Length);
            reader = GetReader(typeof(Format).GetMethod("Stub2"));
            Assert.IsTrue(pattern.Match(reader));
            Assert.AreEqual(2, pattern.StartIndex);
            Assert.AreEqual(2, pattern.Result.Length);
            reader = GetReader(typeof(Format).GetMethod("Stub3"));
            Assert.IsFalse(pattern.Match(reader));
            Assert.AreEqual(-1, pattern.StartIndex);
            Assert.AreEqual(0, pattern.Result.Length);
        }
        [Test]
        public void Test_StringConcatBoxing() {
            var pattern = Analyzer.StringConcatBoxing.Instance;
            var reader = GetReader(typeof(Concat).GetMethod("Stub1"));
            Assert.IsFalse(pattern.Match(reader));
            Assert.IsFalse(pattern.Success);
            Assert.AreEqual(3, pattern.StartIndex);
            Assert.AreEqual(0, pattern.Result.Length);
            reader = GetReader(typeof(Concat).GetMethod("Stub2"));
            Assert.IsTrue(pattern.Match(reader));
            Assert.AreEqual(1, pattern.StartIndex);
            Assert.AreEqual(2, pattern.Result.Length);
            reader = GetReader(typeof(Concat).GetMethod("Stub3"));
            Assert.IsFalse(pattern.Match(reader));
            Assert.AreEqual(-1, pattern.StartIndex);
            Assert.AreEqual(0, pattern.Result.Length);
        }
        [Test]
        public void Test_EnumHasFlagBoxing() {
            var pattern = Analyzer.EnumHasFlagBoxing.Instance;
            var reader = GetReader(typeof(EnumHasFlag).GetMethod("Stub1"));
            Assert.IsFalse(pattern.Match(reader));
            Assert.IsFalse(pattern.Success);
            Assert.AreEqual(3, pattern.StartIndex);
            Assert.AreEqual(0, pattern.Result.Length);
            reader = GetReader(typeof(EnumHasFlag).GetMethod("Stub2"));
            Assert.IsTrue(pattern.Match(reader));
            Assert.AreEqual(1, pattern.StartIndex);
            Assert.AreEqual(2, pattern.Result.Length);
            reader = GetReader(typeof(EnumHasFlag).GetMethod("Stub3"));
            Assert.IsFalse(pattern.Match(reader));
            Assert.AreEqual(-1, pattern.StartIndex);
            Assert.AreEqual(0, pattern.Result.Length);
        }
        static IILReader GetReader(System.Reflection.MethodBase method) {
            return ILReader.Configuration.Standard.GetReader(method);
        }
    }
}
#endif