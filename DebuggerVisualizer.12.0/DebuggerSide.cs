namespace ILReader.DebuggerVisualizer {
    using ILReader.Readers;
    using ILReader.Visualizer.UI;
    using Microsoft.VisualStudio.DebuggerVisualizers;

    public class DebuggerSide : DialogDebuggerVisualizer {
        protected override void Show(IDialogVisualizerService windowService, IVisualizerObjectProvider objectProvider) {
            ShowInstructionsWindow(windowService, objectProvider.GetData());
        }
        protected void ShowInstructionsWindow(IDialogVisualizerService windowService, System.IO.Stream data) {
            using(data) {
                var reader = CreateReader(data);
                using(var form = new InstructionsWindow(reader) { Text = reader.Name })
                    windowService.ShowDialog(form);
            }
        }
        IILReader CreateReader(System.IO.Stream data) {
            var cfg = ILReader.Configuration.Resolve(data);
            return cfg.GetReader(data);
        }
    }
    public class ILDumpObjectSource : VisualizerObjectSource {
        public override void GetData(object source, System.IO.Stream data) {
            var methodBase =
                (source as System.Reflection.MethodBase) ??
                ((source is System.Delegate) ? ((System.Delegate)source).Method : null);
            if(methodBase != null) {
                try {
                    var reader = CreateReader(methodBase) as Dump.ISupportDump;
                    if(reader != null) reader.Dump(data);
                }
                catch { }
            }
        }
        IILReader CreateReader(System.Reflection.MethodBase methodBase) {
            var cfg = ILReader.Configuration.Resolve(methodBase);
            return cfg.GetReader(methodBase);
        }
    }
}