using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("ILReader Debugger Visualizer")]
[assembly: AssemblyDescription("Debugger Visualizer for Visual Studio 2015")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("ILReader")]
[assembly: AssemblyCopyright("Copyright ©  2016")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]


[assembly: System.Diagnostics.DebuggerVisualizer(
    typeof(ILReader.DebuggerVisualizer.DebuggerSide), typeof(ILReader.DebuggerVisualizer.ILDumpObjectSource),
    Target = typeof(System.Reflection.MethodBase), Description = "MSIL Visualizer(Method)")]

[assembly: System.Diagnostics.DebuggerVisualizer(
    typeof(ILReader.DebuggerVisualizer.DebuggerSide), typeof(ILReader.DebuggerVisualizer.ILDumpObjectSource),
    Target = typeof(System.Delegate), Description = "MSIL Visualizer(Delegate)")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("a4e3772e-1cef-4a6e-9daa-c54698ffcadc")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]