namespace ILReader.DebuggerVisualizer {
    using System;
    using System.IO;
    using System.Reflection;
    using ILReader.Readers;
    using ILReader.Visualizer.UI;
    using Microsoft.VisualStudio.DebuggerVisualizers;

    public class DebuggerSide : DialogDebuggerVisualizer {
        public DebuggerSide()
            : base(FormatterPolicy.Json) {
        }
        protected override void Show(IDialogVisualizerService windowService, IVisualizerObjectProvider objectProvider) {
            var data = objectProvider.GetData();
            using(data) {
                IILReader reader = CreateReader(data);
                using(var form = new InstructionsWindow(reader) { Text = reader.Name }) {
                    windowService.ShowDialog(form);
                }
            }
        }
        static IILReader CreateReader(Stream data) {
            var cfg = Configuration.Resolve(data);
            return cfg.GetReader(data);
        }
    }
    //
    public class ILDumpObjectSource : VisualizerObjectSource {
        public override void GetData(object source, Stream data) {
            MethodBase methodBase = (source as MethodBase) ?? ((source is Delegate d) ? d.Method : null);
            if(methodBase != null) {
                try {
                    var reader = CreateReader(methodBase) as Dump.ISupportDump;
                    reader?.Dump(data);
                }
                catch { }
            }
        }
        static IILReader CreateReader(MethodBase methodBase) {
            var cfg = Configuration.Resolve(methodBase);
            return cfg.GetReader(methodBase);
        }
    }
}