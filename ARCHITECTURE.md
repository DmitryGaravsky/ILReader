# ILReader Architecture

## Overview

ILReader is a .NET library for reading, analyzing, and pattern-matching CIL (Common Intermediate Language) bytecode at runtime. It supports standard reflection-based methods, dynamic methods, pre-serialized IL dumps, and assembly-level analysis via `System.Reflection.Metadata` (SRM).

The solution is organized into three projects with a clear separation of concerns:

| Project | Role |
|---|---|
| `ILReader.Core` | IL decoding engine and public API |
| `ILReader.Analyzer` | Semantic pattern detection built on Core |
| `ILReader.Core.Tests` | NUnit test suite covering both libraries |

---

## Solution Structure

```
ILReader/
├── Core/                        # Core IL reading library
│   ├── Configuration/           # Entry-point factories and configuration
│   ├── ILReader/                # Factory for IILReader instances
│   ├── MetadataSymbol/          # Abstraction over resolved metadata tokens
│   ├── OperandReaderContext/    # Source-specific operand resolution strategies
│   ├── Readers/                 # Instruction reader, IL byte parser, opcode reader
│   └── Utils/                   # LazyRef, RuntimeTypes, helpers
├── Analyzer/
│   └── Patterns/                # Pattern matching engine + built-in patterns
│       ├── Boxing/
│       ├── Call/
│       ├── Events/
│       └── Exceptions/
└── Tests/
    └── Core/                    # Unit and integration tests
```

---

## Core Library

### Layered Architecture

The Core library is structured in eight layers, each with a single responsibility.

```
Consumer code
      │
      ▼
[Configuration]          ← Entry point: selects the right reader strategy
      │
      ▼
[IILReaderFactory]       ← Creates and caches IILReader instances
      │
      ▼
[InstructionReader]      ← Lazily enumerates IInstruction sequence
      │
      ├── [ILBytesReader]        ← Raw byte stream over IL body
      ├── [OpCodeReader]         ← Decodes OpCode from byte(s)
      ├── [OperandReader]        ← Dispatches to type-specific operand readers
      └── [IOperandReaderContext]← Resolves metadata tokens to symbols
                │
                └── [IMetadataSymbol] ← Unified token abstraction
```

### Key Public Interfaces

#### `IILReader`
The primary consumer-facing API. Implements `IEnumerable<IInstruction>`.

```csharp
interface IILReader : IEnumerable<IInstruction> {
    int Count { get; }
    string Name { get; }
    ExceptionHandler[] ExceptionHandlers { get; }
    IEnumerable<IMetadataItem> Metadata { get; }
    IInstruction this[int index] { get; }
}
```

#### `IInstruction`
Represents a single decoded CIL instruction.

```csharp
interface IInstruction {
    int Index { get; }      // Sequential position
    int Offset { get; }     // IL byte offset
    OpCode OpCode { get; }
    object Operand { get; } // Resolved metadata symbol or primitive
    byte[] Bytes { get; }
    string Text { get; }
    int Depth { get; }      // Exception handler nesting depth
}
```

#### `IILReaderConfiguration`
Provides reader creation and caching. Token- and name-based overloads are only supported by `SrmConfiguration` — other configurations throw `NotSupportedException`.

```csharp
interface IILReaderConfiguration {
    IILReader GetReader(MethodBase method);
    IILReader GetReader(int metadataToken);          // SRM only
    IILReader GetReader(string typeName, string methodName); // SRM only
    IILReader GetReader(Stream dump);
    void Reset(MethodBase method);
    void Reset();
}
```

#### `IMetadataSymbol`
Unified abstraction over metadata tokens, regardless of source.

```csharp
interface IMetadataSymbol {
    MetadataSymbolKind Kind { get; }  // Method, Field, Type, Member, String, Signature
    string Name { get; }
    string DeclaringType { get; }
    TSource GetSource<TSource>();
    bool TryGetSource<TSource>(out TSource source);
}
```

---

### Configuration System

`Configuration` is the main entry point. It selects the appropriate strategy based on the IL source type and caches readers per `MethodBase`.

```csharp
// Auto-detect and resolve
IILReaderConfiguration cfg = Configuration.Resolve(methodBase);
IILReader reader = cfg.GetReader(methodBase);

// For PE-stream based assembly analysis
IILReaderConfiguration cfg = Configuration.ForAssembly(peStream);
IILReader reader = cfg.GetReader(metadataToken);
// or by name
IILReader reader = cfg.GetReader("My.Namespace.MyClass", "MyMethod");
```

**Configuration variants:**

| Class | IL Source |
|---|---|
| `StandardConfiguration` | Regular .NET methods via `System.Reflection` |
| `DynamicMethodConfiguration` | `System.Reflection.Emit.DynamicMethod` |
| `RTDynamicMethodConfiguration` | Runtime-generated dynamic methods |
| `SrmConfiguration` | PE streams via `System.Reflection.Metadata` |
| `DumpConfiguration` | Pre-serialized IL dumps |

All variants derive from `ConfigurationBase`, which provides thread-safe reader caching.

---

### Operand Resolution

`IOperandReaderContext` encapsulates the source-specific strategy for resolving metadata tokens to typed symbols. Each `Configuration` variant creates its own context implementation:

| Context Class | Resolution Strategy |
|---|---|
| `OperandReaderContext` | `Module.ResolveMethod/Field/Type/String` |
| `OperandReaderContext_DynamicMethod` | Private IL extraction from `DynamicMethod` internals |
| `OperandReaderContext_Srm` | `System.Reflection.Metadata` `EntityHandle` navigation |
| `OperandReaderContext_Dump` | Deserialized metadata from IL dump |

`OperandReader` is a static dispatcher that routes each `OperandType` (e.g. `InlineMethod`, `InlineField`, `InlineString`, `InlineSwitch`) to a specialized reader. Results are cached per operand type.

---

### Metadata Symbols

`IMetadataSymbol` implementations unify the token resolved by different sources:

| Implementation | Wraps |
|---|---|
| `ReflectionSymbol` | `MethodBase`, `FieldInfo`, `Type`, `MemberInfo` |
| `SrmSymbol` | `System.Reflection.Metadata.EntityHandle` |
| `StringSymbol` | Literal `string` |

Consumers retrieve the underlying object via `GetSource<T>()` or `TryGetSource<T>()`.

---

### Exception Handling

`ExceptionHandler` maps exception clauses (Catch, Filter, Finally, Fault) to instruction ranges. It exposes `TryStart`, `TryEnd`, `HandlerStart`, `HandlerEnd`, and `CatchType`. The `Advance()` method marks instructions as exception-protected during enumeration, updating `IInstruction.Depth`.

---

### IL Parsing Data Flow

```
MethodBase
    → Configuration.Resolve(method)          # select strategy
    → StandardConfiguration.GetReader(method)
    → ILReaderFactory creates OperandReaderContext
    → InstructionReader (lazy)
        → ILBytesReader reads raw bytes
        → OpCodeReader decodes opcode
        → OperandReader dispatches on OperandType
        → OperandReaderContext resolves token
        → Instruction wraps opcode + resolved operand
        → ExceptionHandler.Advance() sets nesting depth
```

---

## Analyzer Library

The Analyzer library provides pattern matching over `IILReader` sequences, built on top of Core.

### Pattern Matching Engine

**`MatchPattern<T>`** — generic sequential pattern matcher.

- Accepts an array of predicate functions.
- Scans elements in order, matching predicates one by one.
- Exposes `Success`, `Result[]` (matched elements), `Indexes[]`, and `StartIndex`.

**`ILPattern`** — extends `MatchPattern<IInstruction>` for IL-specific use.

```csharp
ILPattern pattern = NotImplemented.Instance;

if (pattern.Match(methodBase)) {
    IInstruction[] matched = pattern.Result;
    int start = pattern.StartIndex;
}

// Or match against an existing reader
if (pattern.Match(reader, skipNops: true)) { ... }
```

NOP skipping is optional — when enabled, NOP instructions are filtered before matching.

---

### Built-in Patterns

#### Boxing Detection

| Pattern | Detects |
|---|---|
| `Box` | Any value-type boxing (`box` opcode) |
| `Unbox` | Any unboxing operation |
| `StringFormatBoxing` | Value types passed to `string.Format` |
| `StringConcatBoxing` | Value types passed to `string.Concat` |
| `EnumMethodBoxing` | Enum values boxed inside method calls |
| `EnumToStringBoxing` | `enum.ToString()` triggering boxing |

#### Call Detection

| Pattern | Detects |
|---|---|
| `Call` | `call` / `callvirt` instructions with resolved target method |

#### Event Detection

| Pattern | Detects |
|---|---|
| `Subscribe` | Event subscription (`add_EventName`) |
| `Unsubscribe` | Event unsubscription (`remove_EventName`) |

#### Exception Pattern Detection

| Pattern | Detects |
|---|---|
| `NotImplemented` | `throw new NotImplementedException(...)` |
| `NotSupported` | `throw new NotSupportedException(...)` |

---

### Operand Utilities

`OperandHelper` provides extension methods for safely extracting typed sources from `IInstruction.Operand`, keeping pattern implementations concise and null-safe.

---

## Design Patterns

| Pattern | Usage |
|---|---|
| Factory | `Configuration.Resolve()`, `ILReaderFactory`, `OperandReader` dispatch table |
| Strategy | Interchangeable `IOperandReaderContext` implementations per IL source |
| Template Method | `ConfigurationBase` and `OperandReaderContextReal` define extension points for subclasses |
| Lazy Initialization | `LazyRef<T>` defers instruction list, metadata, and exception handler computation |
| Null Object | `InstructionReader.Empty` for abstract methods or missing IL bodies |
| Adapter | `IMetadataSymbol` bridges `System.Reflection`, SRM, and string literals |

---

## Multi-Target Framework Support

| Target | Purpose |
|---|---|
| `net472` | Legacy Windows / .NET Framework applications |
| `netstandard2.0` | Cross-platform compatibility (Core only) |
| `net8.0` | Modern .NET applications |

Build outputs are separated by target:
- `bin.NetFW/` — .NET Framework 4.7.2
- `bin.NET/` — .NET Standard 2.0 and .NET 8.0

---

## Extension Points

- **Custom patterns**: Subclass `ILPattern` and define predicate arrays to detect new IL sequences.
- **Custom IL sources**: Implement `IOperandReaderContext` and a matching `ConfigurationBase` subclass to support novel IL sources (e.g., compiled expression trees, third-party bytecode).
- **Custom operand readers**: Register specialized `OperandReader` subclasses for non-standard operand types.
