namespace ILReader.DebuggerVisualizer {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;
    using Microsoft.VisualStudio.Extensibility.UI;
    /// <summary>Root view model for the IL visualizer window.</summary>
    [DataContract]
    public class ILReaderViewModel : NotifyPropertyChangedObject {
        public ILReaderViewModel(ILMethodData data) {
            Title = data.Name ?? string.Empty;
            MetadataText = BuildMetadataText(data.Metadata);
            Lines = new ObservableList<InstructionLineViewModel>(BuildLines(data));
        }
        [DataMember]
        public string Title { get; }
        [DataMember]
        public string MetadataText { get; }
        [DataMember]
        public ObservableList<InstructionLineViewModel> Lines { get; }
        //
        static string BuildMetadataText(ILMetadataData[]? metadata) {
            if(metadata == null || metadata.Length == 0)
                return string.Empty;
            var sb = new StringBuilder();
            AppendMetadata(sb, metadata, 0);
            return sb.ToString().TrimEnd();
        }
        static void AppendMetadata(StringBuilder sb, ILMetadataData[]? items, int indent) {
            if(items == null)
                return;
            foreach(var item in items) {
                sb.Append(new string(' ', indent));
                if(item.HasChildren && item.Children?.Length > 0) {
                    sb.AppendLine(item.Name + " (");
                    AppendMetadata(sb, item.Children, indent + 4);
                    sb.Append(new string(' ', indent));
                    sb.AppendLine(")");
                }
                else {
                    if(item.Name != null && item.Value != null)
                        sb.AppendLine(item.Name + " " + item.Value);
                    else if(item.Name != null)
                        sb.AppendLine(item.Name);
                    else if(item.Value != null)
                        sb.AppendLine(item.Value);
                }
            }
        }
        static List<InstructionLineViewModel> BuildLines(ILMethodData data) {
            var lines = new List<InstructionLineViewModel>();
            if(data.Instructions == null || data.Instructions.Length == 0)
                return lines;
            int maxBytesLen = data.Instructions.Max(i => i.Bytes?.Length ?? 0);
            // Build exception handler markers: instructionIndex → list of marker texts
            var markers = new Dictionary<int, List<(string prefix, string text)>>();
            if(data.ExceptionHandlers != null) {
                for(int i = 0; i < data.ExceptionHandlers.Length; i++) {
                    var eh = data.ExceptionHandlers[i];
                    string id = "@" + i.ToString("X2");
                    AddMarker(markers, eh.TryStartIndex, id, ".try {");
                    AddMarker(markers, eh.TryEndIndex, id, "}  // end .try");
                    if(eh.IsFinally)
                        AddMarker(markers, eh.HandlerStartIndex, id, "finally {");
                    if(eh.IsFault)
                        AddMarker(markers, eh.HandlerStartIndex, id, "fault {");
                    if(eh.IsCatch)
                        AddMarker(markers, eh.HandlerStartIndex, id, "catch " + (eh.CatchType ?? "?") + " {");
                    if(eh.IsFilter)
                        AddMarker(markers, eh.FilterStartIndex >= 0 ? eh.FilterStartIndex : eh.HandlerStartIndex, id, "filter {");
                    AddMarker(markers, eh.HandlerEndIndex, id, "}  // end handler");
                }
            }
            foreach(var instr in data.Instructions) {
                if(markers.TryGetValue(instr.Index, out var markerList)) {
                    int depth = Math.Max(0, instr.Depth - 1);
                    foreach(var (id, text) in markerList)
                        lines.Add(InstructionLineViewModel.ForMarker(id, text, depth, maxBytesLen));
                }
                lines.Add(InstructionLineViewModel.ForInstruction(instr, maxBytesLen));
            }
            return lines;
        }
        static void AddMarker(Dictionary<int, List<(string, string)>> markers, int index, string id, string text) {
            if(!markers.TryGetValue(index, out var list))
                markers.Add(index, list = new List<(string, string)>());
            list.Add((id, text));
        }
    }
    /// <summary>View model for a single display line (instruction or exception-handler marker).</summary>
    [DataContract]
    public class InstructionLineViewModel {
        [DataMember]
        public bool IsMarker { get; private set; }
        /// <summary>Offset column text (e.g. "IL_0000: " or "@01    " for markers).</summary>
        [DataMember]
        public string OffsetText { get; private set; } = string.Empty;
        /// <summary>Bytes column text (hex bytes, empty when not applicable).</summary>
        [DataMember]
        public string BytesText { get; private set; } = string.Empty;
        /// <summary>Depth-based indentation string (spaces).</summary>
        [DataMember]
        public string IndentText { get; private set; } = string.Empty;
        /// <summary>The opcode or keyword part (blue in the original UI).</summary>
        [DataMember]
        public string OpCodeText { get; private set; } = string.Empty;
        /// <summary>Operand or continuation text.</summary>
        [DataMember]
        public string OperandText { get; private set; } = string.Empty;
        /// <summary>When true, the operand is a string literal and OperandText contains the quoted value.</summary>
        [DataMember]
        public bool IsStringOperand { get; private set; }
        //
        InstructionLineViewModel() { }
        public static InstructionLineViewModel ForMarker(string id, string markerText, int depth, int maxBytesLen) {
            int bytesPad = maxBytesLen > 0 ? maxBytesLen * 2 + 1 : 0;
            // Split marker text into keyword + rest for styling
            int spaceIdx = markerText.IndexOf(' ');
            string keyword = spaceIdx > 0 ? markerText.Substring(0, spaceIdx) : markerText;
            string rest = spaceIdx > 0 ? markerText.Substring(spaceIdx) : string.Empty;
            return new InstructionLineViewModel {
                IsMarker = true,
                OffsetText = id.PadRight(8),
                BytesText = bytesPad > 0 ? string.Empty.PadRight(bytesPad) : string.Empty,
                IndentText = new string(' ', depth * 2),
                OpCodeText = keyword,
                OperandText = rest,
                IsStringOperand = false,
            };
        }
        public static InstructionLineViewModel ForInstruction(ILInstructionData instr, int maxBytesLen) {
            // Offset: "IL_0000: "
            string offsetText = "IL_" + instr.Index.ToString("X4") + ": ";
            // Bytes: padded hex
            string bytesText = string.Empty;
            if(maxBytesLen > 0) {
                if(instr.Bytes != null && instr.Bytes.Length > 0) {
                    var sb = new StringBuilder();
                    foreach(byte b in instr.Bytes)
                        sb.Append(b.ToString("X2"));
                    bytesText = sb.ToString().PadRight(maxBytesLen * 2 + 1);
                }
                else bytesText = new string(' ', maxBytesLen * 2 + 1);
            }
            string indentText = instr.Depth > 0 ? new string(' ', instr.Depth * 2) : string.Empty;
            string opCodeText = instr.OpCode ?? string.Empty;
            string operandText;
            bool isString = instr.IsLdstr;
            if(isString)
                operandText = " \"" + (instr.Operand ?? string.Empty) + "\"";
            else
                operandText = instr.Operand != null ? " " + instr.Operand : string.Empty;
            return new InstructionLineViewModel {
                IsMarker = false,
                OffsetText = offsetText,
                BytesText = bytesText,
                IndentText = indentText,
                OpCodeText = opCodeText,
                OperandText = operandText,
                IsStringOperand = isString,
            };
        }
    }
}