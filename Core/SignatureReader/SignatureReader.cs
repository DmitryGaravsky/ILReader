namespace ILReader.Readers {
    using System;
    using System.Linq;
    using System.Collections.Generic;

    class SignatureReader {
        IBinaryReader binaryReader;
        public SignatureReader(IBinaryReader binaryReader) {
            this.binaryReader = binaryReader;
        }
        protected bool CanRead() {
            return binaryReader.CanRead();
        }
        protected byte Current() {
            return binaryReader.Current;
        }
        protected byte ReadByte() {
            return binaryReader.ReadByte();
        }
        protected uint ReadCompressedUInt32() {
            byte current = ReadByte();
            if((current & 0x80) == 0)
                return (uint)current;
            if((current & 0x40) == 0)
                return ((uint)current & 0xffffff7fu) << 8 | (uint)ReadByte();
            return (uint)(((int)current & -0xc1) << 24 | (int)ReadByte() << 16 | (int)ReadByte() << 8 | (int)ReadByte());
        }
    }
    class LocalVarSig {
        public readonly Type Type;
        readonly byte data;
        public LocalVarSig(Type type, bool typedByRef, bool pinned, bool byRef) {
            this.Type = type;
            if(typedByRef) data |= 0x01;
            if(pinned) data |= 0x02;
            if(byRef) data |= 0x04;
        }
        public bool IsPinned {
            get { return (data & 0x02) == 0x02; }
        }
        public override string ToString() {
            return Type.ToString();
        }
    }
    class LocalSignatureReader : SignatureReader {
        LazyRef<LocalVarSig[]> locals;
        public LocalVarSig[] Locals {
            get { return locals.Value; }
        }
        public LocalSignatureReader(byte[] signature)
            : this(new BinaryReader(signature)) {
        }
        public LocalSignatureReader(IBinaryReader binaryReader)
            : base(binaryReader) {
            this.locals = new LazyRef<LocalVarSig[]>(() => ParseLocals().ToArray());
        }
        IEnumerable<LocalVarSig> ParseLocals() {
            int localsSigCode = ReadByte(); //0x07
            uint localsCount = ReadCompressedUInt32();
            for(int i = 0; i < localsCount; i++) {
                Type type;
                bool typedByRef, pinned, byRef;
                if(!ParseLocal(out typedByRef, out pinned, out byRef, out type))
                    yield break;
                yield return new LocalVarSig(type, typedByRef, pinned, byRef);
            }
        }
        const byte ELEMENT_TYPE_TYPEDBYREF = 0x16;
        const byte ELEMENT_TYPE_BYREF = 0x10;
        const byte ELEMENT_TYPE_CMOD_REQD = 0x1f;
        const byte ELEMENT_TYPE_CMOD_OPT = 0x20;
        const byte ELEMENT_TYPE_PINNED = 0x45;
        bool ParseLocal(out bool typedByRef, out bool pinned, out bool byRef, out Type type) {
            typedByRef = false;
            pinned = false;
            byRef = false;
            type = null;
            // TYPEDBYREF | ([CustomMod] [Constraint])* [BYREF] Type
            if(!CanRead())
                return false;
            if(Current() == ELEMENT_TYPE_TYPEDBYREF) {
                ReadByte();
                typedByRef = true;
                return true;
            }
            if(!ParseOptionalCustomModsOrConstraint(out pinned))
                return false;
            if(!CanRead())
                return false;
            if(Current() == ELEMENT_TYPE_BYREF) {
                ReadByte();
                byRef = true;
            }
            // TODO
            type = ParseSimpleType();
            return true;
        }
        bool ParseOptionalCustomModsOrConstraint(out bool pinned) {
            pinned = false;
            while(CanRead()) {
                switch(Current()) {
                    case ELEMENT_TYPE_CMOD_OPT:
                    case ELEMENT_TYPE_CMOD_REQD:
                        if(!ParseCustomMod(ReadByte()))
                            return false;
                        return false;
                    case ELEMENT_TYPE_PINNED:
                        ReadByte();
                        pinned = true;
                        return false;
                    default:
                        return true;
                }
            }
            return true;
        }
        bool ParseOptionalCustomMods() {
            while(CanRead()) {
                byte cmod = Current();
                if(cmod == ELEMENT_TYPE_CMOD_OPT || cmod == ELEMENT_TYPE_CMOD_REQD) {
                    if(!ParseCustomMod(ReadByte()))
                        return false;
                    return false;
                }
                break;
            }
            return true;
        }
        bool ParseCustomMod(byte cmod) {
            if(cmod == ELEMENT_TYPE_CMOD_OPT || cmod == ELEMENT_TYPE_CMOD_REQD) {
                // TODO
                //if(!ParseTypeDefOrRefEncoded(&indexType, &index))
                //return false;
                return true;
            }
            return false;
        }
        const byte ELEMENT_TYPE_VOID = 0x01;
        const byte ELEMENT_TYPE_BOOLEAN = 0x02;
        const byte ELEMENT_TYPE_CHAR = 0x03;
        const byte ELEMENT_TYPE_I1 = 0x04;
        const byte ELEMENT_TYPE_U1 = 0x05;
        const byte ELEMENT_TYPE_I2 = 0x06;
        const byte ELEMENT_TYPE_U2 = 0x07;
        const byte ELEMENT_TYPE_I4 = 0x08;
        const byte ELEMENT_TYPE_U4 = 0x09;
        const byte ELEMENT_TYPE_I8 = 0x0a;
        const byte ELEMENT_TYPE_U8 = 0x0b;
        const byte ELEMENT_TYPE_R4 = 0x0c;
        const byte ELEMENT_TYPE_R8 = 0x0d;
        const byte ELEMENT_TYPE_STRING = 0x0e;
        const byte ELEMENT_TYPE_I = 0x18;
        const byte ELEMENT_TYPE_U = 0x19;
        const byte ELEMENT_TYPE_OBJECT = 0x1c;
        //
        System.Type ParseSimpleType() {
            byte elem_type = ReadByte();
            switch(elem_type) {
                case ELEMENT_TYPE_BOOLEAN:
                    return typeof(bool);
                case ELEMENT_TYPE_CHAR:
                    return typeof(char);
                case ELEMENT_TYPE_I1:
                    return typeof(sbyte);
                case ELEMENT_TYPE_U1:
                    return typeof(byte);
                case ELEMENT_TYPE_U2:
                    return typeof(ushort);
                case ELEMENT_TYPE_I2:
                    return typeof(short);
                case ELEMENT_TYPE_I4:
                    return typeof(int);
                case ELEMENT_TYPE_U4:
                    return typeof(uint);
                case ELEMENT_TYPE_I8:
                    return typeof(long);
                case ELEMENT_TYPE_U8:
                    return typeof(ulong);
                case ELEMENT_TYPE_R4:
                    return typeof(float);
                case ELEMENT_TYPE_R8:
                    return typeof(double);
                case ELEMENT_TYPE_I:
                    return typeof(System.IntPtr);
                case ELEMENT_TYPE_U:
                    return typeof(System.UIntPtr);
                case ELEMENT_TYPE_STRING:
                    return typeof(string);
                case ELEMENT_TYPE_OBJECT:
                    return typeof(object);
            }
            return null;
        }
    }
}