namespace ILReader.Core.Tests {
    using ILReader.Readers;
    using NUnit.Framework;

    [TestFixture]
    public class LocalSignatureReader_Tests {
        readonly static byte[] sig1 = new byte[] { 0x7, 1, 0x8 };
        readonly static byte[] sig2 = new byte[] { 0x7, 2, 0x8, 0x8 };
        [Test]
        public void Test_SingleIntParameter() {
            ILBytesReader reader = new ILBytesReader(sig1);
            var sigReader = new LocalSignatureReader(reader);
            Assert.AreEqual(1, sigReader.Locals.Length);
            Assert.AreEqual(typeof(int), sigReader.Locals[0].Type);
            Assert.IsFalse(sigReader.Locals[0].IsPinned);
        }
        [Test]
        public void Test_TwoIntParameters() {
            ILBytesReader reader = new ILBytesReader(sig2);
            var sigReader = new LocalSignatureReader(reader);
            Assert.AreEqual(2, sigReader.Locals.Length);
            Assert.AreEqual(typeof(int), sigReader.Locals[0].Type);
            Assert.AreEqual(typeof(int), sigReader.Locals[1].Type);
            Assert.IsFalse(sigReader.Locals[0].IsPinned);
            Assert.IsFalse(sigReader.Locals[1].IsPinned);
        }
        [Test]
        public void Test_Empty_ZeroLocals() {
            var sigReader = new LocalSignatureReader(new byte[] { 0x07, 0 });
            Assert.AreEqual(0, sigReader.Locals.Length);
        }
        [Test]
        public void Test_BoolLocal() {
            var sigReader = new LocalSignatureReader(new byte[] { 0x07, 1, 0x02 });
            Assert.AreEqual(1, sigReader.Locals.Length);
            Assert.AreEqual(typeof(bool), sigReader.Locals[0].Type);
        }
        [Test]
        public void Test_CharLocal() {
            var sigReader = new LocalSignatureReader(new byte[] { 0x07, 1, 0x03 });
            Assert.AreEqual(1, sigReader.Locals.Length);
            Assert.AreEqual(typeof(char), sigReader.Locals[0].Type);
        }
        [Test]
        public void Test_SByteLocal() {
            var sigReader = new LocalSignatureReader(new byte[] { 0x07, 1, 0x04 });
            Assert.AreEqual(1, sigReader.Locals.Length);
            Assert.AreEqual(typeof(sbyte), sigReader.Locals[0].Type);
        }
        [Test]
        public void Test_ByteLocal() {
            var sigReader = new LocalSignatureReader(new byte[] { 0x07, 1, 0x05 });
            Assert.AreEqual(1, sigReader.Locals.Length);
            Assert.AreEqual(typeof(byte), sigReader.Locals[0].Type);
        }
        [Test]
        public void Test_ShortLocal() {
            var sigReader = new LocalSignatureReader(new byte[] { 0x07, 1, 0x06 });
            Assert.AreEqual(1, sigReader.Locals.Length);
            Assert.AreEqual(typeof(short), sigReader.Locals[0].Type);
        }
        [Test]
        public void Test_UShortLocal() {
            var sigReader = new LocalSignatureReader(new byte[] { 0x07, 1, 0x07 });
            Assert.AreEqual(1, sigReader.Locals.Length);
            Assert.AreEqual(typeof(ushort), sigReader.Locals[0].Type);
        }
        [Test]
        public void Test_UIntLocal() {
            var sigReader = new LocalSignatureReader(new byte[] { 0x07, 1, 0x09 });
            Assert.AreEqual(1, sigReader.Locals.Length);
            Assert.AreEqual(typeof(uint), sigReader.Locals[0].Type);
        }
        [Test]
        public void Test_LongLocal() {
            var sigReader = new LocalSignatureReader(new byte[] { 0x07, 1, 0x0a });
            Assert.AreEqual(1, sigReader.Locals.Length);
            Assert.AreEqual(typeof(long), sigReader.Locals[0].Type);
        }
        [Test]
        public void Test_ULongLocal() {
            var sigReader = new LocalSignatureReader(new byte[] { 0x07, 1, 0x0b });
            Assert.AreEqual(1, sigReader.Locals.Length);
            Assert.AreEqual(typeof(ulong), sigReader.Locals[0].Type);
        }
        [Test]
        public void Test_FloatLocal() {
            var sigReader = new LocalSignatureReader(new byte[] { 0x07, 1, 0x0c });
            Assert.AreEqual(1, sigReader.Locals.Length);
            Assert.AreEqual(typeof(float), sigReader.Locals[0].Type);
        }
        [Test]
        public void Test_DoubleLocal() {
            var sigReader = new LocalSignatureReader(new byte[] { 0x07, 1, 0x0d });
            Assert.AreEqual(1, sigReader.Locals.Length);
            Assert.AreEqual(typeof(double), sigReader.Locals[0].Type);
        }
        [Test]
        public void Test_StringLocal() {
            var sigReader = new LocalSignatureReader(new byte[] { 0x07, 1, 0x0e });
            Assert.AreEqual(1, sigReader.Locals.Length);
            Assert.AreEqual(typeof(string), sigReader.Locals[0].Type);
        }
        [Test]
        public void Test_ObjectLocal() {
            var sigReader = new LocalSignatureReader(new byte[] { 0x07, 1, 0x1c });
            Assert.AreEqual(1, sigReader.Locals.Length);
            Assert.AreEqual(typeof(object), sigReader.Locals[0].Type);
        }
        [Test]
        public void Test_IntPtrLocal() {
            var sigReader = new LocalSignatureReader(new byte[] { 0x07, 1, 0x18 });
            Assert.AreEqual(1, sigReader.Locals.Length);
            Assert.AreEqual(typeof(System.IntPtr), sigReader.Locals[0].Type);
        }
        [Test]
        public void Test_UIntPtrLocal() {
            var sigReader = new LocalSignatureReader(new byte[] { 0x07, 1, 0x19 });
            Assert.AreEqual(1, sigReader.Locals.Length);
            Assert.AreEqual(typeof(System.UIntPtr), sigReader.Locals[0].Type);
        }
        [Test]
        public void Test_MixedLocals_IntStringDouble() {
            var sigReader = new LocalSignatureReader(new byte[] { 0x07, 3, 0x08, 0x0e, 0x0d });
            Assert.AreEqual(3, sigReader.Locals.Length);
            Assert.AreEqual(typeof(int), sigReader.Locals[0].Type);
            Assert.AreEqual(typeof(string), sigReader.Locals[1].Type);
            Assert.AreEqual(typeof(double), sigReader.Locals[2].Type);
        }
    }
}
