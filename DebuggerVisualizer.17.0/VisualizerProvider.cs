namespace ILReader.DebuggerVisualizer {
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.Extensibility;
    using Microsoft.VisualStudio.Extensibility.DebuggerVisualizers;
    using Microsoft.VisualStudio.RpcContracts.RemoteUI;

    [VisualStudioContribution]
    internal class VisualizerProvider : DebuggerVisualizerProvider {
        public VisualizerProvider(ExtensionEntrypoint extension, VisualStudioExtensibility extensibility)
            : base(extension, extensibility) {
        }
        // MethodBase and Delegate are abstract - use string overload to bypass compile-time CEE0005 check
        const string methodResourceAlias = "%ILReader.DebuggerVisualizer.VisualizerProvider.MethodName%";
        const string MethodBaseAQN_net = "System.Reflection.MethodBase, System.Runtime";
        const string MethodBaseAQN_fw = "System.Reflection.MethodBase, mscorlib";
        const string delegateResourceAlias = "%ILReader.DebuggerVisualizer.VisualizerProvider.DelegateName%";
        const string DelegateAQN_net = "System.Delegate, System.Runtime";
        const string DelegateAQN_fw = "System.Delegate, mscorlib";
        //
        public override DebuggerVisualizerProviderConfiguration DebuggerVisualizerProviderConfiguration =>
            new DebuggerVisualizerProviderConfiguration(
                new VisualizerTargetType(methodResourceAlias, MethodBaseAQN_net),
                new VisualizerTargetType(methodResourceAlias, MethodBaseAQN_fw),
                new VisualizerTargetType(delegateResourceAlias, DelegateAQN_net),
                new VisualizerTargetType(delegateResourceAlias, DelegateAQN_fw)) {
                VisualizerObjectSourceType = new VisualizerObjectSourceType(typeof(ILDumpObjectSource)),
                Style = VisualizerStyle.ToolWindow,
            };
        public override Task<IRemoteUserControl> CreateVisualizerAsync(
            VisualizerTarget visualizerTarget, CancellationToken cancellationToken) {
            return Task.FromResult<IRemoteUserControl>(new VisualizerControl(visualizerTarget));
        }
    }
}