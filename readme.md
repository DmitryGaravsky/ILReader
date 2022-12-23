# CIL-Reader for Methods(e.g. dynamic) and Delegates

`Common Intermediate Language` reader, analyzer and visualizer tool.  
Allows you to access CIL-instructions of .Net method's bodies.

<a href="https://www.nuget.org/packages/ILReader.Core/"><img alt="Nuget Version" src="https://img.shields.io/nuget/v/ILReader.Core.svg" data-canonical-src="https://img.shields.io/nuget/v/ILReader.Core.svg" style="max-width:100%;" /></a>

## Core Library (ILReader.Core)

Provides a way to read and interprete CIL-bytes of methods and delegates bodies.

Usage:

```cs
var method = typeof(...).GetMethod(...);
// or
var method = delegate.Method;
// or
var method = (DynamicMethod)(/* create or get method */);
```
Get Reader:
```cs
IILReader GetReader(MethodBase method) {
    IILReaderConfiguration cfg = Configuration.Resolve(method);
    return reader = cfg.GetReader(method);
}
```
Iterate instructions one-by-one:
```cs
foreach(IInstruction instruction in reader) {
    var opCode = instruction.OpCode
    object operand = instruction.Operand;
    int ILOffset = instruction.Offset;
    // ...
}
```

## Visualizer Library (ILReader.Visualizer)

Debugger Visualizer for Visual Studio. 
Provides a way to view CIL-bytes of methods and delegates bodies.

Usage:

```cs
Func<...> someFunc = ...; // Just use it while debugging
```

## Analyzer Library (ILReader.Analyzer)

Provides a way to detect typical patterns in  CIL-bytes.

Usage:

```cs
class Bar {
    public void Stub() {
        throw new NotImplementedException("Stub");
    }
}
```
Check whether or not the method's body match pattern:
```cs
var method = typeof(Bar).GetMethod("Stub");
var reader = GetReader(method);
// use default pattern or create you own
var pattern = Analyzer.NotImplemented.Instance;
if (pattern.Match(reader)) {
    // do something
}
```

## Real usage

[MSIL-level boxing detector tool for .Net](https://github.com/DmitryGaravsky/MSIL-Boxing-Detector)


### NuGet

To install [ILReader.Core](https://www.nuget.org/packages/ILReader.Core), run the following command in the Package Manager Console:

    Install-Package ILReader.Core


### License

The ILReader.Core library is licensed under the [MIT](https://github.com/DmitryGaravsky/ILReader.Core/blob/master/LICENSE.TXT) license.
