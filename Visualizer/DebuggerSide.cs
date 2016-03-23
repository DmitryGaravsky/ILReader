namespace ILReader.Visualizer {
    using System.Collections.Generic;
    using ILReader.Readers;
    using Microsoft.VisualStudio.DebuggerVisualizers;

    public abstract class DebuggerSideBase : Microsoft.VisualStudio.DebuggerVisualizers.DialogDebuggerVisualizer {
        protected virtual IILReader CreateReader(System.Reflection.MethodBase methodBase) {
            var cfg = ILReader.Configuration.Standard;
            return cfg.GetReader(methodBase);
        }
        protected void ShowInstructions(IDialogVisualizerService windowService, System.Reflection.MethodBase methodBase) {
            if(methodBase != null)
                ShowInstructions(windowService, methodBase.Name, CreateReader(methodBase));
        }
        protected void ShowInstructions(IDialogVisualizerService windowService, string caption, IEnumerable<IInstruction> instructions) {
            using(var form = new UI.InstructionsWindow(instructions)) {
                form.Text = caption;
                windowService.ShowDialog(form);
            }
        }
    }
}
namespace ILReader.MethodVisualizer {
    using Microsoft.VisualStudio.DebuggerVisualizers;

    public class DebuggerSide : Visualizer.DebuggerSideBase {
        protected override void Show(IDialogVisualizerService windowService, IVisualizerObjectProvider objectProvider) {
            ShowInstructions(windowService, objectProvider.GetObject() as System.Reflection.MethodBase);
        }
    }
}
namespace ILReader.DelegateVisualizer {
    using Microsoft.VisualStudio.DebuggerVisualizers;

    public class DebuggerSide : Visualizer.DebuggerSideBase {
        protected override void Show(IDialogVisualizerService windowService, IVisualizerObjectProvider objectProvider) {
            var dgt = objectProvider.GetObject() as System.Delegate;
            ShowInstructions(windowService, dgt != null ? dgt.Method : null);
        }
    }
}