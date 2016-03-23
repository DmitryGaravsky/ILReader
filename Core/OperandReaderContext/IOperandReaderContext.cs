namespace ILReader.Context {
    using System;
    using System.Reflection;

    public interface IOperandReaderContext {
        LocalVariableInfo this[byte variableIndex] { get; }
        LocalVariableInfo this[short variableIndex] { get; }
        FieldInfo ResolveField(int metadataToken);
        MethodBase ResolveMethod(int metadataToken);
        MemberInfo ResolveMember(int metadataToken);
        Type ResolveType(int metadataToken);
        byte[] ResolveSignature(int metadataToken);
        string ResolveString(int metadataToken);
    }
}