namespace ILReader.Readers {
    using System.Reflection;
    using System.Reflection.Emit;

    static class OpCodeReader {
        #region Initialization
        static OpCodeReader() {
            CreateOpCodes();
        }
        static OpCode[] singleByteOpCode;
        static OpCode[] doubleByteOpCode;
        static void CreateOpCodes() {
            singleByteOpCode = new OpCode[225];
            doubleByteOpCode = new OpCode[31];
            //
            FieldInfo[] fields = GetOpCodeFields();
            for(int i = 0; i < fields.Length; i++) {
                OpCode code = (OpCode)fields[i].GetValue(null);
                if(code.OpCodeType == OpCodeType.Nternal)
                    continue;
                if(code.Size == 1)
                    singleByteOpCode[code.Value] = code;
                else
                    doubleByteOpCode[code.Value & 0xff] = code;
            }
        }
        static FieldInfo[] GetOpCodeFields() {
            return typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static);
        }
        #endregion Initialization
        const int DoubleByteInstructionPrefix = 254;
        public static OpCode ReadOpCode(IBinaryReader binaryReader) {
            byte instruction = binaryReader.ReadByte();
            if(instruction != DoubleByteInstructionPrefix)
                return singleByteOpCode[instruction];
            else
                return doubleByteOpCode[binaryReader.ReadByte()];
        }
    }
}