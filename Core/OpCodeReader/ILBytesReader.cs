namespace ILReader.Readers {
    public sealed class ILBytesReader {
        readonly byte[] bytes; int ptr;
        public ILBytesReader(byte[] bytes) {
            this.bytes = bytes;
        }
        public bool CanRead() {
            return ptr < bytes.Length;
        }
        public byte ReadByte() {
            return bytes[ptr++];
        }
        public sbyte ReadSByte() {
            return (sbyte)bytes[ptr++];
        }
        public bool ReadBoolean() {
            return bytes[ptr++] != 0;
        }
        public short ReadShort() {
            ptr += 2;
            return System.BitConverter.ToInt16(bytes, ptr - 2);
        }
        public int ReadInt() {
            ptr += 4;
            return System.BitConverter.ToInt32(bytes, ptr - 4);
        }
        public float ReadFloat() {
            ptr += 4;
            return System.BitConverter.ToSingle(bytes, ptr - 4);
        }
        public long ReadLong() {
            ptr += 8;
            return System.BitConverter.ToInt64(bytes, ptr - 8);
        }
        public double ReadDouble() {
            ptr += 8;
            return System.BitConverter.ToDouble(bytes, ptr - 8);
        }
        //
        public int Offset {
            get { return ptr; }
        }
        public byte Current {
            get { return bytes[ptr]; }
        }
        //
        public byte[] Read(int offset, int size) {
            byte[] result = new byte[size];
            System.Array.Copy(bytes, offset, result, 0, size);
            return result;
        }
    }
}