# MSIL Reader for Methods(e.g. DynamicMethods) and Delegates

Common Intermediate Language reader, analyzer and visualizer tool for .Net.  
Allows you to access CIL-instructions of .Net methods.

## Core Library (ILReader.Core)

Provides a way to read and interprete CIL-bytes of methods and delegates bodies.

Usage:

```cs
    var method = typeof(...).GetMethod(...);
    // or
    var method = delegate.Method;
    // or
    var method = (DynamicMethod)(/* create or get method */);
    //
    IILReaderConfiguration cfg = Configuration.Resolve(method);
    var reader = cfg.GetReader(method);
```

## Visualizer Library

Provides a way to view CIL-bytes of methods and delegates bodies via Debugger Visualizer.

Usage:

```cs
Func<...> someFunc = ...; // Just use it while debugging
```

## Analizer Library

Provides a way to detect some patterns in  CIL-bytes.

Usage:

```cs
    class Bar {
        public void Stub() {
            throw new NotImplementedException("Stub");
        }
    }
    //...
    var method = typeof(Bar).GetMethod("Stub");
    var pattern = Analyzer.NotImplemented.Instance;
    var reader = GetReader(method);
    if (pattern.Match(reader)) {
        // do something
    }
```