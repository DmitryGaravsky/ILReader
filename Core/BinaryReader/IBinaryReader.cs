namespace ILReader.Readers {
    public interface IBinaryReader {
        bool CanRead();
        //
        byte ReadByte();
        bool ReadBoolean();
        short ReadShort();
        int ReadInt();
        float ReadFloat();
        long ReadLong();
        double ReadDouble();
        //
        int Offset { get; }
    }
}