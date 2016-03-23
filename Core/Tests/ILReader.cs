#if DEBUGTEST
namespace ILReader.Tests {
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using ILReader.Readers;
    using NUnit.Framework;

    [TestFixture]
    public class ILReader_Tests {
        IILReaderConfiguration cfg = Configuration.Standard;
        #region Test Classes
        Action<Foo, Action> subscribe = (foo, execute) => foo.Click += (s, e) => execute();
        class Foo {
            public event EventHandler Click {
                add { }
                remove { }
            }
        }
        #endregion Test Classes
        [Test]
        public void Test_ReadInstructions() {
            var reader = cfg.GetReader(subscribe.Method);
            Assert.AreEqual(12, reader.Count());
            Assert.AreEqual(OpCodes.Newobj, reader.First().OpCode);
            Assert.AreEqual(OpCodes.Ret, reader[reader.First(), 11].OpCode);
            Assert.AreEqual(OpCodes.Nop, reader[reader.Last(), -1].OpCode);
            //
            var newDelegate = reader[8];
            Assert.AreEqual(newDelegate,
                reader.FindPrev(null, i => i.OpCode == OpCodes.Newobj));
            Assert.AreEqual(
                reader.FindNext(null, i => i.OpCode == OpCodes.Ldftn),
                reader.FindPrev(null, i => i.OpCode == OpCodes.Ldftn));
            //
            var ldftn = reader.FindPrev(newDelegate, i => i.OpCode == OpCodes.Ldftn);
            Assert.IsNotNull(ldftn);
            Assert.AreEqual(ldftn.Index, newDelegate.Index - 1);
            //
            var callVirt = reader.FindNext(newDelegate, i => i.OpCode == OpCodes.Callvirt);
            Assert.IsNotNull(newDelegate);
            Assert.AreEqual(callVirt.Index, newDelegate.Index + 1);
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
                            Assert.IsTrue(methods[m].IsAbstract ||
                               ((methods[m].GetMethodImplementationFlags() & MethodImplAttributes.InternalCall) == MethodImplAttributes.InternalCall));
                        }
                    }
                }
            }
            catch { Assert.Fail(); }
        }
    }
}
#endif