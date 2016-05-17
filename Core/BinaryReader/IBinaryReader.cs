namespace ILReader.Readers {
    public interface IBinaryReader {
        bool CanRead();
        byte Current { get; }
        byte ReadByte();
        bool ReadBoolean();
        short ReadShort();
        int ReadInt();
        float ReadFloat();
        long ReadLong();
        double ReadDouble();
        int Offset { get; }
        byte[] Read(int offset, int size);
    }
}