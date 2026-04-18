namespace ILReader.DebuggerVisualizer {
    using System.Runtime.Serialization;
    // Data transfer object representing an IL method body for the debugger visualizer.</summary>
    [DataContract]
    public class ILMethodData {
        [DataMember] public string Name { get; set; }
        [DataMember] public ILMetadataData[] Metadata { get; set; }
        [DataMember] public ILInstructionData[] Instructions { get; set; }
        [DataMember] public ILExceptionHandlerData[] ExceptionHandlers { get; set; }
    }
    [DataContract]
    public class ILMetadataData {
        [DataMember] public string Name { get; set; }
        [DataMember] public string Value { get; set; }
        [DataMember] public bool HasChildren { get; set; }
        [DataMember] public ILMetadataData[] Children { get; set; }
    }
    [DataContract]
    public class ILInstructionData {
        [DataMember] public int Index { get; set; }
        [DataMember] public int Depth { get; set; }
        [DataMember] public string Text { get; set; }
        [DataMember] public string OpCode { get; set; }
        [DataMember] public string Operand { get; set; }
        [DataMember] public byte[] Bytes { get; set; }
        [DataMember] public bool IsLdstr { get; set; }
    }
    [DataContract]
    public class ILExceptionHandlerData {
        [DataMember] public int TryStartIndex { get; set; }
        [DataMember] public int TryEndIndex { get; set; }
        [DataMember] public int HandlerStartIndex { get; set; }
        [DataMember] public int HandlerEndIndex { get; set; }
        [DataMember] public int FilterStartIndex { get; set; }
        [DataMember] public bool IsFinally { get; set; }
        [DataMember] public bool IsFault { get; set; }
        [DataMember] public bool IsCatch { get; set; }
        [DataMember] public bool IsFilter { get; set; }
        [DataMember] public string CatchType { get; set; }
    }
}