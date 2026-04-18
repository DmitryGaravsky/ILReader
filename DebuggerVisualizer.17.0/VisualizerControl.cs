namespace ILReader.DebuggerVisualizer {
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.Extensibility.DebuggerVisualizers;
    using Microsoft.VisualStudio.Extensibility.UI;
    using Microsoft.VisualStudio.RpcContracts.DebuggerVisualizers;
    /// <summary>
    /// WPF RemoteUserControl that displays the IL instructions for a MethodBase or Delegate.
    /// </summary>
    internal class VisualizerControl : RemoteUserControl {
        readonly VisualizerTarget visualizerTarget;
        readonly ILReaderViewModel viewModel;
        public VisualizerControl(VisualizerTarget visualizerTarget)
            : base(new ILReaderViewModel(new ILMethodData { Name = "Loading…" })) {
            this.visualizerTarget = visualizerTarget;
            this.viewModel = (ILReaderViewModel)this.DataContext!;
            visualizerTarget.Changed += OnVisualizerTargetChangedAsync;
        }
        protected override void Dispose(bool disposing) {
            visualizerTarget.Changed -= OnVisualizerTargetChangedAsync;
            base.Dispose(disposing);
        }
        Task OnVisualizerTargetChangedAsync(VisualizerTargetStateNotification notification) {
            if(notification == VisualizerTargetStateNotification.Available ||
               notification == VisualizerTargetStateNotification.ValueUpdated) {
                return LoadDataAsync(CancellationToken.None);
            }
            return Task.CompletedTask;
        }
        async Task LoadDataAsync(CancellationToken cancellationToken) {
            try {
                var data = await visualizerTarget.ObjectSource.RequestDataAsync<ILMethodData>(
                    jsonSerializer: null, cancellationToken);
                if(data != null)
                    RefreshViewModel(data);
            }
            catch { /* Graceful degradation — debug session may have ended */ }
        }
        void RefreshViewModel(ILMethodData data) {
            // Rebuild lines from newly received data
            var newLines = new ILReaderViewModel(data);
            viewModel.Lines.Clear();
            foreach(var line in newLines.Lines)
                viewModel.Lines.Add(line);
        }
        public override async Task ControlLoadedAsync(CancellationToken cancellationToken) {
            await base.ControlLoadedAsync(cancellationToken);
            await LoadDataAsync(cancellationToken);
        }
    }
}