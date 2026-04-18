namespace ILReader.DebuggerVisualizer {
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using ILReader.Readers;
    using Microsoft.VisualStudio.DebuggerVisualizers;
    /// <summary>
    /// Visualizer object source: runs in the debuggee process, serializes the IL method data to JSON.
    /// Supports MethodBase and Delegate targets.
    /// </summary>
    public class ILDumpObjectSource : VisualizerObjectSource {
        public ILDumpObjectSource()
            : base() { }
        public override void GetData(object source, Stream outgoingData) {
            MethodBase methodBase = (source as MethodBase) ?? (source as Delegate)?.Method;
            if(methodBase == null)
                return;
            try {
                var cfg = Configuration.Resolve(methodBase);
                IILReader reader = cfg.GetReader(methodBase);
                if(reader != null)
                    SerializeAsJson(outgoingData, BuildMethodData(reader));
            }
            catch { /* swallow errors — graceful degradation */ }
        }
        static ILMethodData BuildMethodData(IILReader reader) {
            return new ILMethodData {
                Name = reader.Name,
                Metadata = reader.Metadata.Select(ConvertMetadata).ToArray(),
                Instructions = reader.Select(ConvertInstruction).ToArray(),
                ExceptionHandlers = reader.ExceptionHandlers.Select(ConvertExceptionHandler).ToArray(),
            };
        }
        static ILMetadataData ConvertMetadata(IMetadataItem m) {
            return new ILMetadataData {
                Name = m.Name,
                Value = m.Value?.ToString(),
                HasChildren = m.HasChildren,
                Children = m.HasChildren ? m.Children?.Select(ConvertMetadata).ToArray() : null,
            };
        }
        static ILInstructionData ConvertInstruction(IInstruction i) {
            bool isLdstr = i.OpCode == System.Reflection.Emit.OpCodes.Ldstr;
            return new ILInstructionData {
                Index = i.Index,
                Depth = i.Depth,
                Text = i.Text,
                OpCode = i.OpCode.ToString(),
                Operand = isLdstr ? (string)i.Operand : i.Operand?.ToString(),
                Bytes = i.Bytes,
                IsLdstr = isLdstr,
            };
        }
        static ILExceptionHandlerData ConvertExceptionHandler(ExceptionHandler eh) {
            return new ILExceptionHandlerData {
                TryStartIndex = eh.TryStart.Index,
                TryEndIndex = eh.TryEnd.Index,
                HandlerStartIndex = eh.HandlerStart.Index,
                HandlerEndIndex = eh.HandlerEnd.Index,
                FilterStartIndex = eh.IsFilter ? eh.FilterStart.Index : -1,
                IsFinally = eh.IsFinally,
                IsFault = eh.IsFault,
                IsCatch = eh.IsCatch,
                IsFilter = eh.IsFilter,
                CatchType = eh.IsCatch ? eh.CatchType?.ToString() : null,
            };
        }
    }
}