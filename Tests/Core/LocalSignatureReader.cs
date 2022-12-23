namespace ILReader.Core.Tests {
    using ILReader.Readers;
    using NUnit.Framework;

    [TestFixture]
    public class LocalSignatureReader_Tests {
        readonly static byte[] sig1 = new byte[] { 0x7, 1, 0x8 };
        readonly static byte[] sig2 = new byte[] { 0x7, 2, 0x8, 0x8 };
        [Test]
        public void Test_SingleIntParameter() {
            IBinaryReader reader = new BinaryReader(sig1);
            var sigReader = new LocalSignatureReader(reader);
            Assert.AreEqual(1, sigReader.Locals.Length);
            Assert.AreEqual(typeof(int), sigReader.Locals[0].Type);
            Assert.IsFalse(sigReader.Locals[0].IsPinned);
        }
        [Test]
        public void Test_TwoIntParameters() {
            IBinaryReader reader = new BinaryReader(sig2);
            var sigReader = new LocalSignatureReader(reader);
            Assert.AreEqual(2, sigReader.Locals.Length);
            Assert.AreEqual(typeof(int), sigReader.Locals[0].Type);
            Assert.AreEqual(typeof(int), sigReader.Locals[1].Type);
            Assert.IsFalse(sigReader.Locals[0].IsPinned);
            Assert.IsFalse(sigReader.Locals[1].IsPinned);
        }
    }
}