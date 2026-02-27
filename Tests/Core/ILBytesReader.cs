namespace ILReader.Core.Tests {
    using ILReader.Readers;
    using NUnit.Framework;

    [TestFixture]
    public class ILBytesReader_Tests {
        readonly static byte[] bytes = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 };
        [Test]
        public void Test_ReadByte() {
            ILBytesReader reader = new ILBytesReader(bytes);
            Assert.AreEqual(0, reader.ReadByte());
            Assert.AreEqual(1, reader.ReadByte());
            Assert.AreEqual(2, reader.Offset);
        }
        [Test]
        public void Test_ReadBoolean() {
            ILBytesReader reader = new ILBytesReader(bytes);
            Assert.IsFalse(reader.ReadBoolean());
            Assert.IsTrue(reader.ReadBoolean());
            Assert.AreEqual(2, reader.Offset);
        }
        [Test]
        public void Test_ReadShort() {
            ILBytesReader reader = new ILBytesReader(bytes);
            Assert.AreEqual(0x0100, reader.ReadShort());
            Assert.AreEqual(0x0302, reader.ReadShort());
            Assert.AreEqual(4, reader.Offset);
        }
        [Test]
        public void Test_ReadInt() {
            ILBytesReader reader = new ILBytesReader(bytes);
            Assert.AreEqual(0x03020100, reader.ReadInt());
            Assert.AreEqual(0x07060504, reader.ReadInt());
            Assert.AreEqual(8, reader.Offset);
            Assert.IsFalse(reader.CanRead());
        }
        [Test]
        public void Test_ReadLong() {
            ILBytesReader reader = new ILBytesReader(bytes);
            Assert.AreEqual(0x0706050403020100, reader.ReadLong());
            Assert.AreEqual(8, reader.Offset);
            Assert.IsFalse(reader.CanRead());
        }
        [Test]
        public void Test_ReadFloat() {
            byte[] bytes = new byte[] { 0, 0, 0x80, 0x3F };
            ILBytesReader reader = new ILBytesReader(bytes);
            Assert.AreEqual(1.0f, reader.ReadFloat());
            Assert.AreEqual(4, reader.Offset);
        }
        [Test]
        public void Test_ReadDouble() {
            byte[] bytes = new byte[] { 0, 0, 0, 0, 0, 0, 0xF0, 0x3F };
            ILBytesReader reader = new ILBytesReader(bytes);
            Assert.AreEqual(1.0, reader.ReadDouble());
            Assert.AreEqual(8, reader.Offset);
        }
        [Test]
        public void Test_ReadSByte_Positive() {
            ILBytesReader reader = new ILBytesReader(bytes);
            Assert.AreEqual((sbyte)0, reader.ReadSByte());
            Assert.AreEqual((sbyte)1, reader.ReadSByte());
            Assert.AreEqual(2, reader.Offset);
        }
        [Test]
        public void Test_ReadSByte_Negative() {
            // 0xFC = 252 as byte = -4 as sbyte (backward branch target)
            ILBytesReader reader = new ILBytesReader(new byte[] { 0xFC, 0xFF });
            Assert.AreEqual((sbyte)-4, reader.ReadSByte());
            Assert.AreEqual((sbyte)-1, reader.ReadSByte());
            Assert.AreEqual(2, reader.Offset);
        }
    }
}