namespace ILReader.Dump {
    using System.Linq;
    using System.Collections.Generic;

    static class DumpHelper {
        static byte[] GetBytes(string str) {
            byte[] bytes = null;
            GetBytes(ref bytes, str);
            return bytes;
        }
        static byte[] GetBytes(int value) {
            return System.BitConverter.GetBytes(value);
        }
        //
        internal static void GetBytes(ref byte[] bytes, string str) {
            bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
        }
        internal static string GetString(byte[] bytes) {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }
        //
        internal static void Write(int value, System.IO.Stream dump) {
            dump.Write(GetBytes(value), 0, sizeof(int));
        }
        internal static void Write(byte[] value, System.IO.Stream dump) {
            dump.Write(GetBytes(value.Length), 0, sizeof(int));
            dump.Write(value, 0, value.Length);
        }
        //
        internal static void Write(string value, System.IO.Stream dump) {
            Write(GetBytes(value), dump);
        }
        //
        internal static void Write<T>(T[] cache, System.IO.Stream dump) {
            Write(cache.Length, dump);
            for(int i = 0; i < cache.Length; i++)
                Write(cache[i].ToString(), dump);
        }
        //
        internal static void Write<T>(IDictionary<int, T> cache, System.IO.Stream dump) {
            Write(cache.Count, dump);
            foreach(var item in cache) {
                Write(item.Key, dump);
                Write(item.Value.ToString(), dump);
            }
        }
        internal static void Write<T>(IDictionary<int, string> cache, System.IO.Stream dump) {
            Write(cache.Count, dump);
            foreach(var item in cache) {
                Write(item.Key, dump);
                Write(item.Value, dump);
            }
        }
        internal static void Write<T>(IDictionary<int, byte[]> cache, System.IO.Stream dump) {
            Write(cache.Count, dump);
            foreach(var item in cache) {
                Write(item.Key, dump);
                Write(item.Value, dump);
            }
        }
        //
        static int ReadInt(System.IO.Stream dump) {
            byte[] intBuffer = new byte[4];
            dump.Read(intBuffer, 0, intBuffer.Length);
            return System.BitConverter.ToInt32(intBuffer, 0);
        }
        static byte[] ReadBytes(System.IO.Stream dump) {
            byte[] bytes = new byte[ReadInt(dump)];
            dump.Read(bytes, 0, bytes.Length);
            return bytes;
        }
        internal static void Read(ref string str, System.IO.Stream dump) {
            str = GetString(ReadBytes(dump));
        }
        internal static void Read(ref string[] array, System.IO.Stream dump) {
            array = new string[ReadInt(dump)];
            for(int i = 0; i < array.Length; i++)
                array[i] = GetString(ReadBytes(dump));
        }
        internal static void Read(IDictionary<int, string> cache, System.IO.Stream dump) {
            int count = ReadInt(dump);
            for(int i = 0; i < count; i++)
                cache.Add(ReadInt(dump), GetString(ReadBytes(dump)));
        }
        internal static void Read(IDictionary<int, byte[]> cache, System.IO.Stream dump) {
            int count = ReadInt(dump);
            for(int i = 0; i < count; i++)
                cache.Add(ReadInt(dump), ReadBytes(dump));
        }
        //
        internal static void WriteMedatataItems(System.IO.Stream stream, IEnumerable<Readers.IMetadataItem> metadata) {
            var items = metadata.ToArray();
            DumpHelper.Write(items.Length, stream); // count
            for(int i = 0; i < items.Length; i++)
                WriteMedatataItem(stream, items[i]);
        }
        static void WriteMedatataItem(System.IO.Stream stream, Readers.IMetadataItem item) {
            DumpHelper.Write(item.HasChildren ? 1 : 0, stream); // hasChildren
            DumpHelper.Write(item.Name, stream); // name
            if(!item.HasChildren)
                WriteMetadataItemValue(stream, item);
            else
                WriteMedatataItems(stream, item.Children);
        }
        static void WriteMetadataItemValue(System.IO.Stream stream, Readers.IMetadataItem item) {
            DumpHelper.Write(item.Value is int ? 1 : 0, stream);
            if(item.Value is int)
                DumpHelper.Write((int)item.Value, stream);
            else {
                DumpHelper.Write((item.Value ?? string.Empty).ToString(), stream);
            }
        }
        //
        internal static IEnumerable<Readers.IMetadataItem> ReadMedatataItems(System.IO.Stream stream) {
            int count = DumpHelper.ReadInt(stream);
            for(int i = 0; i < count; i++)
                yield return ReadMedatataItem(stream);
        }
        static Readers.IMetadataItem ReadMedatataItem(System.IO.Stream stream) {
            bool hasChildren = ReadInt(stream) == 1;
            string name = GetString(ReadBytes(stream));
            object value = hasChildren ?
                ReadMedatataItems(stream).ToArray() :
                ReadMetadataItemValue(stream);
            return new Readers.MetadataItem(name, value);
        }
        static object ReadMetadataItemValue(System.IO.Stream stream) {
            bool isInt = ReadInt(stream) == 1;
            return isInt ? (object)ReadInt(stream) : GetString(ReadBytes(stream));
        }
    }
}