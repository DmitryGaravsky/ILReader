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
            return (short)(bytes[ptr++] | bytes[ptr++] << 8);
        }
        int IBinaryReader.ReadInt() {
            return ((bytes[ptr++] | (bytes[ptr++] << 8)) | (bytes[ptr++] << 0x10)) | (bytes[ptr++] << 0x18);
        }
        float IBinaryReader.ReadFloat() {
            return ((bytes[ptr++] | (bytes[ptr++] << 8)) | (bytes[ptr++] << 0x10)) | (bytes[ptr++] << 0x18); // TODO
        }
        long IBinaryReader.ReadLong() {
            return ((bytes[ptr++] | (bytes[ptr++] << 8)) | (bytes[ptr++] << 0x10)) | (bytes[ptr++] << 0x18); // TODO
        }
        double IBinaryReader.ReadDouble() {
            return ((bytes[ptr++] | (bytes[ptr++] << 8)) | (bytes[ptr++] << 0x10)) | (bytes[ptr++] << 0x18); // TODO
        }
        int IBinaryReader.Offset {
            get { return ptr; }
        }
    }
}