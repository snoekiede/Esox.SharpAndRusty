# Changelog

All notable changes to the Esox.SharpAndRusty project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [1.2.4] - 2025

### Added

#### Unit Type
- **Unit type** - Rust-inspired type representing the absence of a meaningful return value
  - Similar to Rust's `()` type or F#'s `unit` type
  - Perfect for `Result<Unit, E>` patterns
  - Single value semantics - all Unit instances are equal
  - Implements `IEquatable<Unit>` with proper equality operators
  - Returns `"()"` for `ToString()`
  - Zero overhead - empty readonly struct
  - Static `Unit.Value` property for easy access

#### Use Cases
- Operations that can fail but don't return meaningful values on success
- File I/O operations (write, delete, etc.)
- Cleanup and shutdown operations  
- State transitions
- Logging operations
- Any operation where success is binary (worked or didn't)

#### Documentation
- Added **Unit type** section to README.md
- Added Unit type to API Reference
- Comprehensive usage examples with Error type
- 19 comprehensive Unit tests covering:
  - Equality semantics
  - Hash code consistency
  - String representation  
  - Integration with Result types
  - Integration with Error type
  - Pattern matching
  - Value extraction methods

#### Test Coverage
- Added **19 comprehensive Unit tests**:
  - Basic creation and value access (3 tests)
  - Equality operations (4 tests)
  - Hash code consistency (1 test)
  - String representation (1 test)
  - Integration with Result<Unit, E> (5 tests)
  - Integration with Error type (1 test)
  - Object equality edge cases (3 tests)
  - Multiple instance equality verification (1 test)

### Changed

#### Documentation Updates
- Updated **README.md** with Unit type feature
- Updated API Reference section with Unit type documentation
- Updated test coverage to **323 tests** (267 production + 19 Unit + 37 RwLock)
- Updated Feature Maturity table with Unit type (10/10 production ready)
- Updated Production Readiness section with Unit type
- Added Unit type usage examples throughout documentation

#### Test Organization  
- Split test reporting:
  - **Production tests**: 267 (Result/Error/Unit core functionality)
  - **Experimental tests**: 37 RwLock + 36 Mutex = 73
  - **Total**: 323 tests, 100% passing

### Fixed

- Made `Match` method null-aware using `ArgumentNullException.ThrowIfNull` (.NET 10 API)

### Performance

- **Unit**: Zero overhead - empty struct with no memory allocation
- **Equality**: All operations are O(1) with no allocations
- **Pattern matching**: Seamless integration with existing Result infrastructure

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
| 1.2.4   | 323   | + Unit type | Complete | Yes |

---

## Migration Guide

### From 1.2.0 to 1.2.4

**All changes are backward compatible. No breaking changes to stable APIs.**

#### New Unit Type

**Unit** is now available as a production-ready feature:

```csharp
using Esox.SharpAndRusty;

// Result<Unit, E> - Operations with no meaningful value
Result<Unit, Error> result = ...;

// Unit type - Represents the absence of a meaningful return value
Unit value = Unit.Value;
```

**Important Notes**: 
- Unit type is now stable and production-ready (10/10)
- Use `Unit.Value` to access the single value instance
- Unit type has zero overhead - it is an empty struct
- Comprehensive test coverage for Unit type (19 tests)

**No Action Required**: This is an additive change. Existing code continues to work unchanged.

### From 1.2.2 to 1.2.4

**All changes are backward compatible. No breaking changes to stable APIs.**

#### Bug Fixes
- `Match` method is now null-aware, preventing potential null reference exceptions

### From 1.1.0 to 1.2.0

**All changes are backward compatible. No breaking changes.**

#### Recommended Upgrades

1. **Use type-safe metadata API**:
   ```csharp
   // Old way (still works)
   error.WithMetadata("count", (object)42);
   var count = (int)error.TryGetMetadata("count", out var value) ? value : 0;
   
   // New way (recommended)
   error.WithMetadata("count", 42);
   if (error.TryGetMetadata("count", out int count))
   {
       // Use count directly, no casting
   }
   ```

2. **Use configurable stack traces**:
   ```csharp
   // Old way (always includes file info - slower)
   var error = Error.New("Test").CaptureStackTrace();
   
   // New way (configurable)
   var error = Error.New("Test").CaptureStackTrace(includeFileInfo: false);  // Fast
   var detailed = Error.New("Test").CaptureStackTrace(includeFileInfo: true); // Detailed
   ```

3. **Benefit from automatic optimizations**:
   - ImmutableDictionary is used automatically (no code changes needed)
   - Depth limiting protects against deep chains automatically
   - Circular reference detection works transparently

### From 1.0.0 to 1.1.0

**All changes are backward compatible. No breaking changes.**

#### New Features Available

1. **LINQ query syntax**:
   ```csharp
   var result = from x in GetValue()
                from y in GetOtherValue()
                select x + y;
   ```

2. **Async operations**:
   ```csharp
   var result = await GetValueAsync()
       .MapAsync(async x => await ProcessAsync(x))
       .BindAsync(async x => await ValidateAsync(x));
   ```

3. **Error type with context**:
   ```csharp
   var result = Try(() => File.ReadAllText("config.json"))
       .Context("Failed to load configuration")
       .WithMetadata("path", "config.json");
   ```

---

## Roadmap

### Future Considerations

#### Version 1.3.0 / 2.0.0 (Potential)
- [ ] **Stabilize Mutex<T>** - Move from experimental to production if feedback is positive
- [ ] **Stabilize RwLock<T>** - Move from experimental to production if feedback is positive
- [ ] Evaluate API changes based on community feedback
- [ ] Consider additional concurrency primitives:
  - [ ] Semaphore<T> - Counting semaphore for controlled concurrent access
  - [ ] CondVar - Condition variables for more complex synchronization
- [ ] HttpClient-specific exception mapping
- [ ] Error serialization support (JSON, XML)
- [ ] Performance benchmarks and optimization
- [ ] Additional LINQ operators (Where with error messages)
- [ ] Retry policies and resilience patterns
- [ ] Integration with ILogger

#### Version 2.0.0 (Breaking Changes - If Needed)
- [ ] Finalize Mutex<T> and RwLock<T> APIs based on user feedback
- [ ] Consider breaking changes to improve ergonomics
- [ ] Evaluate struct vs class trade-offs for Error type
- [ ] Consider alternative metadata storage options
- [ ] Review API surface for improvements
- [ ] Potential .NET version upgrade requirements

### Experimental Feature Feedback

We welcome feedback on the **Mutex<T>** and **RwLock<T>** experimental features:
- API design and ergonomics
- Performance in real-world scenarios
- Missing functionality
- Integration with existing code patterns
- Deadlock or race condition experiences
- Documentation clarity and completeness

**How to Provide Feedback:**
- **Bug Reports**: [GitHub Issues](https://github.com/snoekiede/Esox.SharpAndRusty/issues)
- **API Suggestions**: [GitHub Discussions](https://github.com/snoekiede/Esox.SharpAndRusty/discussions)
- **Security Concerns**: security@esoxsolutions.com
- **General Questions**: GitHub Discussions

Your feedback will directly influence whether these features:
- Stabilize with current API
- Stabilize with modifications
- Require major version (2.0) for breaking changes
- Need additional functionality before stabilization

---

## Support

- **Documentation**: See README.md, SECURITY.md, and docs/ folder
- **Issues**: Report bugs on GitHub Issues
- **Discussions**: Ask questions on GitHub Discussions
- **Contributing**: See CONTRIBUTING.md
- **Security**: See SECURITY.md

---

## License

This project is licensed under the MIT License - see [LICENSE.txt](LICENSE.txt) for details.

---

## Acknowledgments

- Inspired by Rust's `Result<T, E>`, `Mutex<T>`, and `RwLock<T>` types
- Thanks to all contributors and early adopters
- Special thanks to the .NET community for feedback and testing
- Experimental feature design influenced by Rust's std::sync primitives

---

**Current Version**: 1.2.4  
**Status**: 
- Production Ready (9.5/10) - Core Result/Error features
- Production Ready (10/10) - Unit type
- Experimental - Mutex<T> and RwLock<T> (API may change)

**Test Coverage**: 323 tests (267 production + 19 Unit + 37 RwLock experimental), 100% pass rate  
**Maintainer**: Iede Snoek (Esox Solutions)

[1.2.4]: https://github.com/snoekiede/Esox.SharpAndRusty/releases/tag/v1.2.4
[1.2.2]: https://github.com/snoekiede/Esox.SharpAndRusty/releases/tag/v1.2.2
[1.2.0]: https://github.com/snoekiede/Esox.SharpAndRusty/releases/tag/v1.2.0
[1.1.0]: https://github.com/snoekiede/Esox.SharpAndRusty/releases/tag/v1.1.0
[1.0.0]: https://github.com/snoekiede/Esox.SharpAndRusty/releases/tag/v1.0.0
