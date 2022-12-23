namespace ILReader.Core.Tests {
    using ILReader.Readers;
    using NUnit.Framework;

    [TestFixture]
    public class BinaryReader_Tests {
        readonly static byte[] bytes = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 };
        [Test]
        public void Test_ReadByte() {
            IBinaryReader reader = new BinaryReader(bytes);
            Assert.AreEqual(0, reader.ReadByte());
            Assert.AreEqual(1, reader.ReadByte());
            Assert.AreEqual(2, reader.Offset);
        }
        [Test]
        public void Test_ReadBoolean() {
            IBinaryReader reader = new BinaryReader(bytes);
            Assert.IsFalse(reader.ReadBoolean());
            Assert.IsTrue(reader.ReadBoolean());
            Assert.AreEqual(2, reader.Offset);
        }
        [Test]
        public void Test_ReadShort() {
            IBinaryReader reader = new BinaryReader(bytes);
            Assert.AreEqual(0x0100, reader.ReadShort());
            Assert.AreEqual(0x0302, reader.ReadShort());
            Assert.AreEqual(4, reader.Offset);
        }
        [Test]
        public void Test_ReadInt() {
            IBinaryReader reader = new BinaryReader(bytes);
            Assert.AreEqual(0x03020100, reader.ReadInt());
            Assert.AreEqual(0x07060504, reader.ReadInt());
            Assert.AreEqual(8, reader.Offset);
            Assert.IsFalse(reader.CanRead());
        }
        [Test]
        public void Test_ReadLong() {
            IBinaryReader reader = new BinaryReader(bytes);
            Assert.AreEqual(0x0706050403020100, reader.ReadLong());
            Assert.AreEqual(8, reader.Offset);
            Assert.IsFalse(reader.CanRead());
        }
        [Test]
        public void Test_ReadFloat() {
            byte[] bytes = new byte[] { 0, 0, 0x80, 0x3F };
            IBinaryReader reader = new BinaryReader(bytes);
            Assert.AreEqual(1.0f, reader.ReadFloat());
            Assert.AreEqual(4, reader.Offset);
        }
        [Test]
        public void Test_ReadDouble() {
            byte[] bytes = new byte[] { 0, 0, 0, 0, 0, 0, 0xF0, 0x3F };
            IBinaryReader reader = new BinaryReader(bytes);
            Assert.AreEqual(1.0, reader.ReadDouble());
            Assert.AreEqual(8, reader.Offset);
        }
    }
}