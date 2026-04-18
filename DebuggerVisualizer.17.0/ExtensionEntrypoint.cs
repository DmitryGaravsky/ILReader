namespace ILReader.DebuggerVisualizer {
    using Microsoft.VisualStudio.Extensibility;
    //
    [VisualStudioContribution]
    internal class ExtensionEntrypoint : Extension {
        public override ExtensionConfiguration ExtensionConfiguration {
            get {
                return new ExtensionConfiguration() {
                    Metadata = new ExtensionMetadata(
                        id: "ILReader.DebuggerVisualizer.6ac34b34-5d31-46bd-b354-6f176378e5ab",
                        version: this.ExtensionAssemblyVersion,
                        publisherName: "Dmitry Garavsky",
                        displayName: "ILReader Debugger Visualizer",
                        description: "MSIL/CIL debugger visualizer for MethodBase and Delegate types (VS 2022/2026)"
                    ),
                };
            }
        }
    }
}