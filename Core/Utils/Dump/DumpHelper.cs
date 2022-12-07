namespace ILReader.Dump {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using ILReader.Readers;

    static class DumpHelper {
        static byte[] GetBytes(string str) {
            byte[] bytes = null;
            GetBytes(ref bytes, str);
            return bytes;
        }
        static byte[] GetBytes(int value) {
            return BitConverter.GetBytes(value);
        }
        //
        internal static void GetBytes(ref byte[] bytes, string str) {
            bytes = new byte[str.Length * sizeof(char)];
            Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
        }
        internal static string GetString(byte[] bytes) {
            char[] chars = new char[bytes.Length / sizeof(char)];
            Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }
        //
        internal static void Write(int value, Stream dump) {
            dump.Write(GetBytes(value), 0, sizeof(int));
        }
        internal static void Write(byte[] value, Stream dump) {
            dump.Write(GetBytes(value.Length), 0, sizeof(int));
            dump.Write(value, 0, value.Length);
        }
        //
        internal static void Write(string value, Stream dump) {
            Write(GetBytes(value), dump);
        }
        //
        internal static void Write<T>(T[] cache, Stream dump, Func<T, string> toString) {
            Write(cache.Length, dump);
            for(int i = 0; i < cache.Length; i++)
                Write(toString(cache[i]), dump);
        }
        //
        internal static void Write<T>(Dictionary<int, T> cache, Stream dump, Func<T, string> toString) {
            Write(cache.Count, dump);
            foreach(var item in cache) {
                Write(item.Key, dump);
                Write(toString(item.Value), dump);
            }
        }
        internal static void Write(Dictionary<int, string> cache, Stream dump) {
            Write(cache.Count, dump);
            foreach(var item in cache) {
                Write(item.Key, dump);
                Write(item.Value, dump);
            }
        }
        internal static void Write(Dictionary<int, byte[]> cache, Stream dump) {
            Write(cache.Count, dump);
            foreach(var item in cache) {
                Write(item.Key, dump);
                Write(item.Value, dump);
            }
        }
        //
        static int ReadInt(Stream dump) {
            byte[] intBuffer = new byte[4];
            dump.Read(intBuffer, 0, intBuffer.Length);
            return BitConverter.ToInt32(intBuffer, 0);
        }
        static void ReadInts(int[] array, Stream stream) {
            byte[] intBuffer = new byte[array.Length * 4];
            stream.Read(intBuffer, 0, intBuffer.Length);
            for(int i = 0; i < array.Length; i++)
                array[i] = BitConverter.ToInt32(intBuffer, i * 4);
        }
        static byte[] ReadBytes(Stream dump) {
            byte[] bytes = new byte[ReadInt(dump)];
            dump.Read(bytes, 0, bytes.Length);
            return bytes;
        }
        internal static void Read(ref string str, Stream dump) {
            str = GetString(ReadBytes(dump));
        }
        internal static void Read(ref string[] array, Stream dump) {
            array = new string[ReadInt(dump)];
            for(int i = 0; i < array.Length; i++)
                array[i] = GetString(ReadBytes(dump));
        }
        internal static void Read(Dictionary<int, string> cache, Stream dump) {
            int count = ReadInt(dump);
            for(int i = 0; i < count; i++)
                cache.Add(ReadInt(dump), GetString(ReadBytes(dump)));
        }
        internal static void Read(Dictionary<int, byte[]> cache, Stream dump) {
            int count = ReadInt(dump);
            for(int i = 0; i < count; i++)
                cache.Add(ReadInt(dump), ReadBytes(dump));
        }
        //
        internal static void WriteMedatataItems(Stream stream, IEnumerable<IMetadataItem> metadata) {
            var items = metadata.ToArray();
            DumpHelper.Write(items.Length, stream); // count
            for(int i = 0; i < items.Length; i++)
                WriteMedatataItem(stream, items[i]);
        }
        static void WriteMedatataItem(Stream stream, IMetadataItem item) {
            DumpHelper.Write(item.HasChildren ? 1 : 0, stream); // hasChildren
            DumpHelper.Write(item.Name, stream); // name
            if(!item.HasChildren)
                WriteMetadataItemValue(stream, item);
            else
                WriteMedatataItems(stream, item.Children);
        }
        static void WriteMetadataItemValue(Stream stream, IMetadataItem item) {
            DumpHelper.Write(item.Value is int ? 1 : 0, stream);
            if(item.Value is int)
                DumpHelper.Write((int)item.Value, stream);
            else
                DumpHelper.Write((item.Value ?? string.Empty).ToString(), stream);
        }
        internal static void WriteExceptionHandlers(Stream stream, ExceptionHandler[] handlers) {
            DumpHelper.Write(handlers.Length, stream); // count
            foreach(ISupportDump item in handlers)
                item.Dump(stream);
        }
        //
        internal static IEnumerable<IMetadataItem> ReadMedatataItems(Stream stream) {
            int count = DumpHelper.ReadInt(stream);
            for(int i = 0; i < count; i++)
                yield return ReadMedatataItem(stream);
        }
        static IMetadataItem ReadMedatataItem(Stream stream) {
            bool hasChildren = ReadInt(stream) == 1;
            string name = GetString(ReadBytes(stream));
            object value = hasChildren ?
                ReadMedatataItems(stream).ToArray() :
                ReadMetadataItemValue(stream);
            return new MetadataItem(name, value);
        }
        static object ReadMetadataItemValue(Stream stream) {
            bool isInt = ReadInt(stream) == 1;
            return isInt ? (object)ReadInt(stream) : GetString(ReadBytes(stream));
        }
        internal static IEnumerable<Tuple<ExceptionHandlerType, string, int[]>> ReadExceptionHandlers(Stream stream) {
            int count = ReadInt(stream);
            for(int i = 0; i < count; i++)
                yield return ReadExceptionHandler(stream);
        }
        static Tuple<ExceptionHandlerType, string, int[]> ReadExceptionHandler(Stream stream) {
            var type = (ExceptionHandlerType)ReadInt(stream);
            var catchType = GetString(ReadBytes(stream));
            int[] offsets = new int[5];
            ReadInts(offsets, stream);
            return new Tuple<ExceptionHandlerType, string, int[]>(type, catchType, offsets);
        }
    }
}