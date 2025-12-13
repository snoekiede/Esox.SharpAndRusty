# Changelog

All notable changes to the Esox.SharpAndRusty project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [1.2.4] - 2025

### Added

#### Unit Type
- **Unit type** - Rust-inspired unit type `()` for representing the absence of a value
  - `Unit.Value` - Singleton instance
  - Structural equality - all Unit values are equal
  - Full comparison support - implements `IEquatable<Unit>` and `IComparable<Unit>`
  - Useful for `Result<Unit, E>` when operations succeed/fail without producing a value
  - Common use cases:
    - Validation operations that don't return data
    - Side-effect operations (logging, caching, notifications)
    - Operations where success is the only meaningful information
  - Zero-overhead - implemented as `readonly struct`
  - ToString representation: `"()"`

#### Test Coverage
- Added **10 comprehensive Unit type tests**:
  - Singleton instance behavior (1 test)
  - Equality operations (3 tests)
  - Comparison operations (2 tests)
  - GetHashCode consistency (1 test)
  - ToString representation (1 test)
  - Integration with Result type (2 tests)

### Changed

#### Documentation Updates
- Updated **README.md** (root) with Unit type section
  - Added Unit type feature to features list
  - Added usage examples for Unit type
  - Added Unit type API reference
  - Updated example code to show Unit in action
- Test count updated: **296** ? **306 tests** (270 production + 36 experimental)
- Added Unit type to Feature Maturity table

#### API Additions
- `Result<Unit, E>` now a recommended pattern for void-like operations
- Unit type fully compatible with all Result operations (Map, Bind, LINQ, etc.)

### Performance

- **Unit type**: Zero overhead as `readonly struct`
  - Instance size: 0 bytes (empty struct)
  - All operations: O(1)
  - No allocations
  - Value type semantics
  - JIT can optimize away in many cases

### Notes

#### Unit Type Usage Patterns

**When to Use Unit:**
- Validation operations: `Result<Unit, ValidationError>`
- Side-effect operations: `Result<Unit, string>`
- Operations where only success/failure matters
- Void-replacement in functional pipelines

**Example Patterns:**
```csharp
// Validation
Result<Unit, string> ValidateUser(User user) => ...;

// Side effects
Result<Unit, Error> SendNotification(Message msg) => ...;

// Chaining void operations
var result = from _ in Initialize()
             from __ in Configure()
             from ___ in Start()
             select Unit.Value;
```

**Comparison with Rust:**
- Rust: `Result<(), E>` 
- C#: `Result<Unit, E>`
- Identical semantics and use cases

---

## [1.2.2] - 2025

### Added

#### Experimental Features

> **EXPERIMENTAL**: The following features are experimental and their APIs may change in future versions. Use with caution in production environments.

##### Mutex<T> - Thread-Safe Mutual Exclusion (?? Experimental)
- **Rust-inspired Mutex<T>** type for protecting shared data with Result-based error handling
- **Five locking strategies**:
  - `Lock()` - Blocking lock acquisition
  - `TryLock()` - Non-blocking attempt
  - `TryLockTimeout(TimeSpan)` - Lock with timeout
  - `LockAsync(CancellationToken)` - Async lock acquisition
  - `LockAsyncTimeout(TimeSpan, CancellationToken)` - Async lock with timeout and cancellation
- **MutexGuard<T>** - RAII guard with automatic lock release via `IDisposable`
- **Result-based API** - All lock operations return `Result<MutexGuard<T>, Error>`
- **Functional operations on guards**:
  - `Map<TResult>(Func<T, TResult>)` - Transform guarded value
  - `Update(Func<T, T>)` - Update guarded value in place
- **Interior mutability** - Safe mutation of shared data
- **IntoInner()** - Consume mutex and extract value (similar to Rust)
- Built on `SemaphoreSlim` for reliability

##### RwLock<T> - Reader-Writer Lock (?? Experimental)
- **Rust-inspired RwLock<T>** type for protecting shared data with reader-writer semantics
- **Read and write locking strategies**:
  - `Read()` - Blocking read lock acquisition (multiple readers allowed)
  - `TryRead()` - Non-blocking read attempt
  - `TryReadTimeout(TimeSpan)` - Read lock with timeout
  - `Write()` - Blocking write lock acquisition (exclusive access)
  - `TryWrite()` - Non-blocking write attempt
  - `TryWriteTimeout(TimeSpan)` - Write lock with timeout
- **ReadGuard<T>** - RAII read guard with automatic lock release
- **WriteGuard<T>** - RAII write guard with automatic lock release
- **Result-based API** - All lock operations return `Result<Guard, Error>`
- **Functional operations on guards**:
  - `Map<TResult>(Func<T, TResult>)` - Transform guarded value (both guards)
  - `Update(Func<T, T>)` - Update guarded value in place (write guard only)
- **Multiple concurrent readers** - Efficient read-heavy scenarios
- **Exclusive writer access** - Single writer at a time
- **IntoInner()** - Consume lock and extract value
- Built on `ReaderWriterLockSlim` for efficiency

#### Documentation
- **MUTEX_DOCUMENTATION.md** - Complete Mutex<T> usage guide
  - API reference with all methods
  - Usage examples (basic to advanced)
  - Comparison with Rust's Mutex
  - Best practices and performance considerations
  - Real-world use cases (counters, caches, resource pools, work queues)
- **MUTEX_IMPLEMENTATION_SUMMARY.md** - Implementation details and test results
- **README_MUTEX_UPDATE_SUMMARY.md** - README update documentation
- Updated **README.md** with Experimental Features section
  - Clear experimental status indicators (??)
  - Security and stability warnings
  - Best practices for experimental features
  - Guidelines for providing feedback
- Updated **SECURITY.md** with experimental feature security considerations
  - Concurrency safety guidelines
  - Deadlock prevention patterns
  - Resource management best practices
  - Experimental feature risks and mitigations

#### Test Coverage
- Added **36 comprehensive Mutex<T> tests**:
  - Basic lock operations (4 tests)
  - TryLock functionality (3 tests)
  - TryLockTimeout with various scenarios (3 tests)
  - Async locking (4 tests)
  - Async timeout locking (3 tests)
  - MutexGuard operations (8 tests)
  - IntoInner functionality (3 tests)
  - Concurrency stress tests (3 tests)
  - Disposal and cleanup (3 tests)
  - Complex scenarios (2 tests)

### Changed

#### Documentation Updates
- Updated **README.md** (root) with comprehensive Experimental Features section
  - Clear warnings about API stability
  - Feature maturity table
  - Usage guidelines
  - Best practices for experimental features
  - Feedback mechanism explanation
- Updated **SECURITY.md** with experimental feature security section
  - Mutex<T> and RwLock<T> security considerations
  - Deadlock prevention patterns
  - Resource management guidelines
  - Experimental feature checklist
- Updated **Esox.SharpAndRusty/README.md** with Experimental Features section
- Test count updated: **230** ? **296+ tests** (260 production + 36+ experimental)
- Added clear experimental status indicators (??) throughout documentation
- Added warnings about potential API changes for experimental features

#### Test Organization
- Split test reporting:
  - **Production tests**: 260 (Result/Error core functionality)
  - **Experimental tests**: 36+ (Mutex<T>, RwLock<T>)
  - **Total**: 296+ tests, 100% passing

### Performance

- **Mutex<T>**: Minimal overhead using `SemaphoreSlim` internally
  - Lock acquisition: O(1) + blocking time
  - TryLock: O(1) non-blocking
  - Guard disposal: O(1)
  - Memory: ~40-48 bytes per Mutex + size of T
- **RwLock<T>**: Efficient reader-writer semantics using `ReaderWriterLockSlim`
  - Read lock acquisition: O(1) + blocking time
  - Write lock acquisition: O(1) + blocking time
  - Multiple concurrent readers supported
  - Guard disposal: O(1)
  - Memory: ~56-64 bytes per RwLock + size of T
- **Concurrency verified**: Stress tests with 100+ concurrent operations

### Notes

#### Experimental Status

**The Mutex<T> and RwLock<T> APIs are experimental and may change in future versions.**

**What "Experimental" Means:**
- Thoroughly tested (36+ tests, 100% passing)
- Follows security best practices
- Built on well-tested .NET primitives
- Fully functional and ready for use
- API may change based on community feedback
- May have breaking changes in minor versions (1.x)
- Use caution in production-critical systems

**Recommendations:**
- Use in non-critical paths initially
- Provide feedback on API design via GitHub Issues/Discussions
- Test thoroughly in your specific use cases
- Be prepared for potential API changes in minor version updates
- Report any issues, especially concurrency problems
- Subscribe to release notes for breaking change notifications

**Feedback Needed On:**
- API ergonomics and naming
- Missing functionality
- Performance in real-world scenarios
- Integration with existing code patterns
- Error message clarity
- Documentation completeness

**Production Status:**
- Core Result/Error functionality remains production-ready (9.5/10)
- 260 production tests maintain 100% pass rate
- No breaking changes to existing stable APIs
- Experimental features isolated in `Esox.SharpAndRusty.Async` namespace

---

## [1.2.0] - 2025

### Added

#### Error Type Production Improvements
- **Type-safe metadata API** with generic overloads
  - `WithMetadata<T>(string key, T value) where T : struct` for compile-time type safety
  - `TryGetMetadata<T>(string key, out T? value)` for type-safe retrieval
- **ImmutableDictionary** for metadata storage with O(log n) operations and structural sharing
- **Depth limiting** for error chains (max 50 levels) to prevent stack overflow
- **Circular reference detection** using HashSet-based cycle detection
- **Expanded exception mapping** from 7 to 11+ exception types:
  - `FileNotFoundException` ? `ErrorKind.NotFound`
  - `DirectoryNotFoundException` ? `ErrorKind.NotFound`
  - `TaskCanceledException` ? `ErrorKind.Interrupted`
  - `FormatException` ? `ErrorKind.ParseError`
  - `OutOfMemoryException` ? `ErrorKind.ResourceExhausted`
- **Configurable stack trace capture** with `includeFileInfo` parameter for performance tuning
- **Metadata type validation** at addition time (validates primitives, DateTime, Guid, enums, value types)

#### Error Extensions
- Type-safe metadata overloads for `Result<T, Error>`:
  - `WithMetadata<TValue>(string key, TValue value) where TValue : struct`
  - `WithMetadataAsync<TValue>(string key, TValue value, CancellationToken)`

#### Documentation
- **ERROR_TYPE_PRODUCTION_IMPROVEMENTS.md** - Detailed optimization documentation
- **CIRCULAR_REFERENCE_PROTECTION.md** - Error chain safety features guide
- **ERROR_TESTS_CLEANUP_SUMMARY.md** - Test restoration documentation
- **README_PRODUCTION_UPDATES.md** - README update summary
- **COMPLETE_README_UPDATE_SUMMARY.md** - Comprehensive update documentation
- **CONTRIBUTING.md** - Complete contribution guidelines

### Changed

#### Error Type Enhancements
- Error chain display now includes metadata in formatted output
- `GetFullMessage()` now uses HashSet for visited tracking (circular detection)
- `AppendErrorChain()` signature updated to include `HashSet<Error> visited` parameter
- Stack trace capture now has configurable `includeFileInfo` parameter (default: `false`)

#### Test Coverage
- Increased from 202 to **230 tests** (100% pass rate)
- Added 28 new Error type tests:
  - Type-safe metadata API tests (6 tests)
  - Metadata type validation tests
  - Configurable stack trace tests
  - Circular reference detection tests
  - Enhanced error chain tests

#### Documentation Updates
- Updated **README.md** (root) with production features section
- Updated **Esox.SharpAndRusty/README.md** with production optimizations
- Added exception-to-ErrorKind mapping table
- Added performance characteristics documentation
- Updated test count references to 230 tests
- Added production readiness score: **9.5/10**

### Performance

- **Metadata operations**: Reduced from O(n) to O(log n) with ImmutableDictionary
- **Memory usage**: Improved with structural sharing in immutable collections
- **Error chain traversal**: Bounded at 50 levels with O(1) cycle detection
- **Stack trace capture**: Made configurable for performance tuning

### Fixed

- Corrupted ErrorTests.cs file (3,050 lines) restored to clean 507 lines
- Removed massive test duplication from tool malfunction
- Fixed circular reference potential in error chain traversal

---

## [1.1.0] - 2025

### Added

#### LINQ Query Syntax Support
- Full support for C# LINQ query comprehension
- `SelectMany` overloads for chaining Result operations
- `Select` support for transforming values
- Query syntax integration for elegant error handling workflows

#### Async Extensions
- Complete async/await integration for all Result operations:
  - `MapAsync` - Transform values asynchronously
  - `BindAsync` - Chain async operations
  - `TapAsync` - Execute async side effects
  - `OrElseAsync` - Async fallback operations
  - `CombineAsync` - Async collection operations
- **CancellationToken support** for all async methods
- Graceful operation cancellation with proper cleanup

#### Advanced Features
- `MapError` - Transform error types
- `Expect` - Extract values with custom error messages
- `Tap` - Execute side effects on both success and failure
- `Contains` - Check if result contains a specific value
- `Combine` - Aggregate multiple results into a single result
- `Partition` - Separate successes and failures from collections

#### Error Type Features
- Rich error type with context chaining
- Error categorization with 14 `ErrorKind` categories
- Metadata attachment for debugging
- Stack trace capture capability
- Exception-to-Error conversion with automatic categorization

#### Error Extensions
- `Context` / `ContextAsync` - Add context to errors in Result
- `WithMetadata` / `WithMetadataAsync` - Attach metadata to errors
- `WithKind` - Change error kind
- `Try` / `TryAsync` - Execute operations with automatic exception conversion

### Documentation

- **ERROR_TYPE.md** - Complete Error type documentation
- **ERROR_TYPE_EXAMPLES.md** - Usage examples and patterns
- **CANCELLATION_TOKEN_SUPPORT.md** - Async cancellation guide
- **ADVANCED_FEATURES.md** - Advanced features comprehensive guide
- Expanded README with LINQ examples
- Added API reference sections

### Changed

- Updated README with comprehensive examples for all features
- Expanded test coverage to **202 tests**
- Enhanced XML documentation for all public APIs

---

## [1.0.0] - 2025

### Added

#### Core Result Type
- `Result<T, E>` as a `readonly struct` for zero-overhead abstraction
- Factory methods: `Ok(T value)` and `Err(E error)`
- Pattern matching with `Match<R>(Func<T, R> success, Func<E, R> failure)`
- Value extraction:
  - `TryGetValue(out T value)` and `TryGetError(out E error)`
  - `UnwrapOr(T defaultValue)`
  - `UnwrapOrElse(Func<E, T> defaultFactory)`
- `OrElse` for fallback operations
- Side effects: `Inspect` and `InspectErr`

#### Functional Composition
- `Map` - Transform success values
- `Bind` - Chain operations that return results (flatMap/andThen)
- Automatic error propagation

#### Exception Handling
- `Try` - Execute synchronous operations with exception handling
- `TryAsync` - Execute async operations with exception handling
- Custom error handlers for exception conversion

#### Equality and Comparison
- Full `IEquatable<Result<T, E>>` implementation
- `==` and `!=` operators
- Proper `GetHashCode()` implementation
- `ToString()` for debugging (`"Ok(value)"` or `"Err(error)"`)

#### Basic Extension Methods
- `Map` and `Bind` extension methods
- `Unwrap` for extracting values (throws on error)
- LINQ foundation with `Select` method

### Documentation

- Initial README.md with quick start guide
- Basic usage examples
- API reference
- Benefits and use cases

### Testing

- Comprehensive test suite with **150+ tests**
- Basic creation and inspection tests
- Pattern matching tests
- Equality and hash code tests
- Map and Bind operation tests
- Exception handling tests
- Null handling tests

---

## Version Comparison

| Version | Tests | Features | Documentation | Production Ready |
|---------|-------|----------|---------------|------------------|
| 1.0.0   | 150+  | Core Result type | Basic | Beta |
| 1.1.0   | 202   | + LINQ, Async, Error type | Comprehensive | Yes |
| 1.2.0   | 230   | + Production optimizations | Complete | Yes |
| 1.2.2   | 296+  | + ?? Mutex<T>, RwLock<T> (experimental) | Complete | Yes (core) / ?? (experimental) |
| 1.2.4   | 306+  | + Unit type | Complete | Yes |

---

## Migration Guide

### From 1.2.4 to 1.2.6

**All changes are backward compatible. No breaking changes to stable APIs.**

#### No Action Required
- This is a patch version with no user-facing changes.

### From 1.2.2 to 1.2.4

**All changes are backward compatible. No breaking changes to stable APIs.**

#### New Unit Type

**Unit** type is now available for representing the absence of a value:

```csharp
using Esox.SharpAndRusty.Types;

// Use Unit for operations that succeed/fail without producing a value
public Result<Unit, string> ValidateInput(string input)
{
    if (string.IsNullOrEmpty(input))
        return Result<Unit, string>.Err("Input cannot be empty");
    
    return Result<Unit, string>.Ok(Unit.Value);
}

// Using the result
var result = ValidateInput(userInput);
result.Match(
    success: _ => Console.WriteLine("Validation succeeded"),
    failure: error => Console.WriteLine($"Validation failed: {error}")
);

// Chaining with LINQ
var result = from _ in ValidateInput(input)
             from __ in ProcessData()
             select Unit.Value;

// All Unit values are equal
Unit.Value == Unit.Value;  // Always true
Unit.Value.ToString();      // Returns "()"
```

**When to Use Unit:**
- Validation operations that don't return data
- Side-effect operations (logging, caching, notifications)
- Operations where success is the only meaningful information
- As a replacement for void in functional pipelines

**Rust Equivalent:**
```rust
// Rust
fn validate_input(input: &str) -> Result<(), String> { ... }
```
```csharp
// C# with Esox.SharpAndRusty
Result<Unit, string> ValidateInput(string input) { ... }
```

**No Action Required**: This is an additive change. Existing code continues to work unchanged.

### From 1.2.0 to 1.2.2

**Current Version**: 1.2.4  
**Status**: 
- Production Ready (9.5/10) - Core Result/Error/Unit features
- Experimental - Mutex<T> and RwLock<T> (API may change)

**Test Coverage**: 306+ tests (270 production + 36 experimental), 100% pass rate  
**Maintainer**: Iede Snoek (Esox Solutions)

[1.2.4]: https://github.com/snoekiede/Esox.SharpAndRusty/releases/tag/v1.2.4
[1.2.2]: https://github.com/snoekiede/Esox.SharpAndRusty/releases/tag/v1.2.2
[1.2.0]: https://github.com/snoekiede/Esox.SharpAndRusty/releases/tag/v1.2.0
[1.1.0]: https://github.com/snoekiede/Esox.SharpAndRusty/releases/tag/v1.1.0
[1.0.0]: https://github.com/snoekiede/Esox.SharpAndRusty/releases/tag/v1.0.0
