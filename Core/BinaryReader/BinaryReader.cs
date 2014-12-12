namespace ILReader.Readers {
    sealed class BinaryReader : IBinaryReader {
        readonly byte[] bytes;
        int ptr;
        public BinaryReader(byte[] bytes) {
            this.bytes = bytes;
        }
        bool IBinaryReader.CanRead() {
            return ptr < bytes.Length;
        }
        byte IBinaryReader.ReadByte() {
            return bytes[ptr++];
        }
        bool IBinaryReader.ReadBoolean() {
            return bytes[ptr++] != 0;
        }
        short IBinaryReader.ReadShort() {
            ptr += 2;
            return System.BitConverter.ToInt16(bytes, ptr - 2);
        }
        int IBinaryReader.ReadInt() {
            ptr += 4;
            return System.BitConverter.ToInt32(bytes, ptr - 4);
        }
        float IBinaryReader.ReadFloat() {
            ptr += 4;
            return System.BitConverter.ToSingle(bytes, ptr - 4);
        }
        long IBinaryReader.ReadLong() {
            ptr += 8;
            return System.BitConverter.ToInt64(bytes, ptr - 8);
        }
        double IBinaryReader.ReadDouble() {
            ptr += 8;
            return System.BitConverter.ToDouble(bytes, ptr - 8);
        }
        int IBinaryReader.Offset {
            get { return ptr; }
        }
    }
}