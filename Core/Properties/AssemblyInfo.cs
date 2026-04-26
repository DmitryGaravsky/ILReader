using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("ILReader")]
[assembly: AssemblyDescription("ILReader Core Functionality")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("ILReader")]
[assembly: AssemblyCopyright("Copyright © 2026, Dmitry Garavsky")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
[assembly: Guid("97ab9619-c263-4ff8-a92f-89da0bfa8ccc")]
[assembly: AssemblyVersion("1.0.0.6")]
[assembly: AssemblyFileVersion("1.0.0.6")]
[assembly: InternalsVisibleTo("ILReader.Core.Tests")]

#if NETFRAMEWORK || NETSTANDARD
namespace System.Runtime.CompilerServices {
    // IsExternalInit is required by init-only setters and record types (C# 9+).
    // It is part of .NET 5+ but absent from .NET Framework and older targets.
    internal static class IsExternalInit { }
}
#endif