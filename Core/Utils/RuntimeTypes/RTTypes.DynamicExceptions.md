# RTTypes — DynamicMethod Exception Handler Support

Implementation notes, assumptions, and known compatibility constraints for
`RTTypes.DynamicExceptions.cs` / `OperandReaderContext_DynamicMethod`.

---

## How it works

`DynamicResolver` (an internal CLR type) stores exception-handling metadata in a field
`m_exceptions` of type `__ExceptionInfo[]`. Each `__ExceptionInfo` describes **one try-block**
with **multiple handler clauses**. The code reads this array via `FieldInfo` reflection,
flattens it into a sequence of `DynamicExceptionClause` records, and feeds them to
`ExceptionHandler` one by one through the same `IEnumerator` pattern used by the regular
`OperandReaderContext`.

---

## `__ExceptionInfo` field contract

| Field | Type | Meaning |
|-------|------|---------|
| `m_startAddr` | `int` | TryOffset — shared by all clauses of this try-block |
| `m_endAddr` | `int` | TryOffset + TryLength (exclusive end) — shared by all clauses |
| `m_catchAddr[i]` | `int[]` | HandlerOffset for the i-th clause |
| `m_catchEndAddr[i]` | `int[]` | HandlerOffset + HandlerLength for the i-th clause |
| `m_filterAddr[i]` | `int[]` | FilterOffset for the i-th clause (**Filter type only** — see below) |
| `m_type[i]` | `int[]` | Handler kind: Catch=0, Filter=1, Finally=2, Fault=4 |
| `m_catchClass[i]` | `Type[]` | Exception type (non-null for Catch only) |
| `m_currentCatch` | `int` | Number of active clauses; arrays may be larger |

### Array pre-allocation
Arrays are pre-allocated with an initial capacity (currently 4 in the CLR source).
Only the first `m_currentCatch` elements are valid; the rest contain zeros or sentinel values.
`m_currentCatch` is the authoritative count.

### `m_filterAddr` sentinel values for non-Filter clauses
For Catch / Finally / Fault clauses, `m_filterAddr[i]` holds an **unresolved `Label`
integer** (e.g. `0x2000002`), not a real IL offset. The implementation reads this field
**only** when `m_type[i] == (int)ExceptionHandlerType.Filter`.

### `m_endFinally` is not used
`__ExceptionInfo.m_endFinally` tracks the end of the finally block in certain internal
ILGenerator paths. The implementation ignores it and uses `m_catchEndAddr[i]` for all
handler types — consistent with how `ILGenerator.BakeByteArray` itself builds the
exception table headers. Correctness is confirmed by tests.

---

## Assumptions

1. **Method must be "baked" before the reader is created.**
   `DynamicResolver` is created only when `CreateDelegate` or `GetMethodDescriptor` is
   called on the `DynamicMethod`. If `ILReader` is invoked before baking, `resolver == null`
   and the exception-handler list will be empty — no error is thrown, but no data is returned.

2. **`m_type` values are bit-compatible with `ExceptionHandlerType`.**
   The cast `(ExceptionHandlerType)m_type[i]` assumes the CLR's internal numeric codes
   match the public `ExceptionHandlingClauseOptions` flags, which in turn match
   `ExceptionHandlerType` (Catch=0x0000, Filter=0x0001, Finally=0x0002, Fault=0x0004).

---

## Known compatibility constraints

### 1. .NET Framework only (critical)

The core DynamicMethod context reads the IL byte array by looking up the field
`m_resolver` in `DynamicMethod`:

```csharp
static readonly int fld_m_resolver = "m_resolver".@ƒRegister(DynamicMethodType);
```

On **.NET 5+** this field was renamed to `_resolver`, so `GetResolver(method)` returns
`null`. As a result, **both IL reading and exception-handler resolution are unavailable
for `DynamicMethod` on .NET 5+**. All `BugFix_DynamicMethod_EH_Tests` are guarded with
`#if !NET`.

**Quick fix** — resolve by probing both names:

```csharp
static readonly int fld_m_resolver =
    DynamicMethodType.GetField("m_resolver", BF.Instance | BF.NonPublic) != null
        ? "m_resolver".@ƒRegister(DynamicMethodType)
        : "_resolver".@ƒRegister(DynamicMethodType);
```

### 2. Filter blocks not supported in `DynamicILGenerator` on .NET Framework

`DynamicILGenerator.BeginExceptFilterBlock()` throws `NotSupportedException` on
.NET Framework — filter handlers cannot be added to dynamic methods. The test
`DynamicMethod_TryFilter_HandlerResolved` calls `Assert.Ignore` when this exception
is caught during method construction.

### 3. Internal CLR types may change between runtime versions

`DynamicResolver`, `__ExceptionInfo`, and their field names are **not part of any public
contract**. They can be renamed, restructured, or removed in any runtime patch or future
.NET release.

The implementation degrades gracefully: if `DynamicResolverType` is `null`, or
`fi_m_exceptions` is `null`, `GetDynamicExceptionClauses` returns `null` and the handler
list stays empty — no exception is thrown. However, this failure is **silent**.

> **Recommendation:** log a diagnostic warning when `fi_m_exceptions == null && resolver != null`
> so that a breakage is detected promptly when upgrading the runtime.
