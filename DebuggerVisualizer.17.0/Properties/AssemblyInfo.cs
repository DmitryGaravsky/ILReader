using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("ILReader Debugger Visualizer")]
[assembly: AssemblyDescription("Debugger Visualizer for Visual Studio 2022")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("ILReader")]
[assembly: AssemblyCopyright("Copyright © 2026, Dmitry Garavsky")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: System.Diagnostics.DebuggerVisualizer(
    typeof(ILReader.DebuggerVisualizer.DebuggerSide), typeof(ILReader.DebuggerVisualizer.ILDumpObjectSource),
    Target = typeof(System.Reflection.MethodBase), Description = "MSIL Visualizer(Method)")]
[assembly: System.Diagnostics.DebuggerVisualizer(
    typeof(ILReader.DebuggerVisualizer.DebuggerSide), typeof(ILReader.DebuggerVisualizer.ILDumpObjectSource),
    Target = typeof(System.Delegate), Description = "MSIL Visualizer(Delegate)")]

[assembly: ComVisible(false)]
[assembly: Guid("a4e3772e-1cef-4a6e-9daa-c54698ffcadc")]
[assembly: AssemblyVersion("1.0.0.5")]
[assembly: AssemblyFileVersion("1.0.0.0")]