# Esox.SharpAndRusty

A production-ready C# library that brings Rust-inspired patterns to .NET, including `Result<T, E>` for type-safe error handling and `Option<T>` for representing optional values without null references.

## ⚠️ Disclaimer

This library is provided "as is" without warranty of any kind, either express or implied, including but not limited to the warranties of merchantability, fitness for a particular purpose, and non-infringement. In no event shall the authors or copyright holders be liable for any claim, damages, or other liability, whether in an action of contract, tort, or otherwise, arising from, out of, or in connection with the software or the use or other dealings in the software.

**Use at your own risk.** While this library has been designed to be production-ready with comprehensive test coverage, it is your responsibility to evaluate its suitability for your specific use case and to test it thoroughly in your environment before deploying to production.

## Features

- ✅ **Type-Safe Error Handling**: Explicitly represent success and failure states in your type signatures
- ✅ **Option Type**: Rust-inspired `Option<T>` for representing optional values without null references
  - 30 extension methods including `Filter`, `Zip`, `ZipWith`, `Flatten`, `And`, `Or`, `Xor`, `Inspect`, and more
  - 9 async methods: `MapAsync`, `BindAsync`, `FilterAsync`, `InspectAsync`, `MatchAsync`, and more
  - Full Rust parity for core operations
  - Complete async/await support with cancellation tokens
  - See [OPTION_QUICK_REFERENCE.md](OPTION_QUICK_REFERENCE.md) for complete guide
- ✅ **Result Type**: Comprehensive error handling
  - ~33 synchronous methods for transforming, chaining, and matching results
  - ~10 async methods: `MapAsync`, `BindAsync`, `MapErrorAsync`, `TapAsync`, and more
  - Full async/await integration
- ✅ **Collection Extensions**: Powerful collection processing for Options and Results
  - 11 synchronous methods: `Sequence`, `Traverse`, `CollectSome/Ok`, `Partition` for functional collection patterns
  - 11 asynchronous methods: `SequenceAsync`, `TraverseAsync`, `TraverseParallelAsync`, `CollectSomeAsync`, `PartitionResultsAsync`
  - Fail-fast and best-effort processing modes
  - Sequential and parallel async execution with cancellation support
  - See [COLLECTION_QUICK_REFERENCE.md](COLLECTION_QUICK_REFERENCE.md) for complete guide
- ✅ **Complete Async Support**: Every operation has an async version
  - Full `CancellationToken` support
  - `ConfigureAwait(false)` for performance
  - No blocking calls, fully async/await
- ✅ **Rust-Inspired API**: Familiar patterns for developers coming from Rust or functional programming
- ✅ **Rich Error Type**: Rust-inspired `Error` type with context chaining, metadata, and error categorization
- ✅ **Zero Overhead**: Implemented as a `readonly struct` for optimal performance
- ✅ **Functional Composition**: Chain operations with `Map`, `Bind`, `MapError`, and `OrElse`
- ✅ **Pattern Matching**: Use the `Match` method for elegant success/failure handling
- ✅ **Full Equality Support**: Implements `IEquatable<T>` with proper `==`, `!=`, and `GetHashCode()`
- ✅ **Safe Value Extraction**: `TryGetValue`, `UnwrapOr`, `UnwrapOrElse`, `Expect`, and `Contains` methods
- ✅ **Exception Handling Helpers**: Built-in `Try` and `TryAsync` for wrapping operations
- ✅ **Inspection Methods**: Execute side effects with `Inspect`, `InspectErr`, and `Tap`
- ✅ **LINQ Query Syntax**: Full support for C# LINQ query comprehension with `from`, `select`, and more
- ✅ **Collection Operations**: `Combine` and `Partition` for batch processing
- ✅ **Full Async Support**: Complete async/await integration with `MapAsync`, `BindAsync`, `TapAsync`, and more
- ✅ **Cancellation Support**: All async methods support `CancellationToken` for graceful operation cancellation
- ✅ **.NET 10 Compatible**: Built for the latest .NET platform with C# 14
- 🧪 **Experimental: Mutex<T>**: Rust-inspired mutual exclusion primitive with Result-based locking (works in both sync and async contexts)
- 🧪 **Experimental: RwLock<T>**: Rust-inspired reader-writer lock for shared data access (works in both sync and async contexts)
- ✅ **Alternative Result DU**: `ExtendedResult<T, E>` — a record-based discriminated union with pattern matching and LINQ support

## Installation

```bash
# Clone the repository
git clone https://github.com/snoekiede/Esox.SharpAndRusty.git

# Build the project
dotnet build

# Run tests
dotnet test
```

## Quick Start

```csharp
using Esox.SharpAndRusty.Types;
using Esox.SharpAndRusty.Extensions;

// Create a successful result
var success = Result<int, string>.Ok(42);

// Create a failed result
var failure = Result<int, string>.Err("Something went wrong");

// Pattern match to handle both cases
var message = success.Match(
    success: value => $"Got value: {value}",
    failure: error => $"Got error: {error}"
);

// Or use LINQ query syntax!
var result = from x in Result<int, string>.Ok(10)
             from y in Result<int, string>.Ok(20)
             select x + y;
// Result: Ok(30)

// Use the Error type for rich error handling
var richResult = ErrorExtensions.Try(() => int.Parse("42"))
    .Context("Failed to parse user age")
    .WithMetadata("input", "42")
    .WithKind(ErrorKind.ParseError);

// Use Option<T> for optional values
Option<int> FindUser(int id) => id > 0 
    ? new Option<int>.Some(id) 
    : new Option<int>.None();

var userOption = FindUser(42);
var message = userOption switch
{
    Option<int>.Some(var id) => $"Found user {id}",
    Option<int>.None => "User not found",
    _ => "Unknown"
};
```

## Navigation

- [Features](#features)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Usage Examples](#usage-examples)
  - [ExtendedResult — Record Alternative](#extendedresult-record-alternative)
  - [Safe Value Extraction](#safe-value-extraction)
  - [Functional Extensions](#functional-extensions-import-namespace)
  - [Collections](#collections)
  - [Thread Safety](#thread-safety)
- [ExtendedResult Type (API)](#extendedresult-t-te-type-api)
- [Why Use Result Types?](#why-use-result-types)

## Usage Examples

### ExtendedResult<T, TE> — Record Alternative {#extendedresult-record-alternative}

`ExtendedResult<T, TE>` is an immutable discriminated union implemented as a record with two cases:
- `ExtendedResult<T, TE>.Success(T Value)`
- `ExtendedResult<T, TE>.Failure(TE Error)`

It offers ergonomic pattern matching, safe accessors, and a rich set of extension methods for functional composition and LINQ.

#### Creating and Matching

```csharp
using Esox.SharpAndRusty.Types;

var ok = ExtendedResult<int, string>.Ok(42);
var err = ExtendedResult<int, string>.Err("Not found");

// Pattern matching with C#
var text = ok switch
{
    ExtendedResult<int, string>.Success s => $"Value: {s.Value}",
    ExtendedResult<int, string>.Failure f => $"Error: {f.Error}",
    _ => "Unknown"
};
```

#### Safe Value Extraction {#safe-value-extraction}

```csharp
// Check result state with properties
if (ok.IsSuccess) { /* handle success */ }
if (err.IsFailure) { /* handle failure */ }

// Try-pattern extraction
if (ok.TryGetValue(out var value)) { /* use value */ }
if (err.TryGetError(out var error)) { /* use error */ }

var v1 = ok.UnwrapOr(0);                          // 42
var v2 = err.UnwrapOrElse(e => e.Length);         // computes from error
```

#### Functional Extensions (import namespace)

```csharp
using Esox.SharpAndRusty.Extensions;

var mapped = ok.Map(x => x * 2);                  // Success(84)
var bound = ok.Bind(x => ExtendedResult<int, string>.Ok(x + 1));
var projected = ok.Select(x => x.ToString());     // LINQ select
var composed = from x in ok
               from y in ExtendedResult<int, string>.Ok(8)
               select x + y;                      // Success(50)

var changedError = err.MapError(e => e.Length);   // Failure(int)
var tapped = ok.Tap(v => Log(v), e => LogErr(e)); // side effects only
```

#### Collections

```csharp
var list = new[]
{
    ExtendedResult<int, string>.Ok(1),
    ExtendedResult<int, string>.Ok(2),
    ExtendedResult<int, string>.Err("bad"),
};

var combined = list.Combine();                    // Failure("bad")
var (successes, failures) = list.Partition();     // ([1,2], ["bad"]) 
```

#### Thread Safety

`ExtendedResult<T, TE>` instances are immutable and safe to share across threads. Thread safety of values inside (T/TE) and delegates passed to Map/Bind/etc. depends on those objects.

---

### `ExtendedResult<T, TE>` Type (API) {#extendedresult-t-te-type-api}

#### Instance Properties
- `bool IsSuccess` - Returns true if the result is successful
- `bool IsFailure` - Returns true if the result is a failure

#### Static Methods
- `ExtendedResult<T, TE> Ok(T value)` - Creates a successful result
- `ExtendedResult<T, TE> Err(TE error)` - Creates a failed result
- `ExtendedResult<T, TE> Try(Func<T> operation, Func<Exception, TE> errorHandler)` - Execute operation with exception handling
- `Task<ExtendedResult<T, TE>> TryAsync(Func<Task<T>> operation, Func<Exception, TE> errorHandler)` - Async version of Try

#### Instance Methods
- `TR Match<TR>(Func<T, TR> success, Func<TE, TR> failure)` - Pattern match on the result
- `bool TryGetValue(out T value)` - Try to get the success value
- `bool TryGetError(out TE error)` - Try to get the error value
- `T UnwrapOr(T defaultValue)` - Get value or return default
- `T UnwrapOrElse(Func<TE, T> defaultFactory)` - Get value or compute default from error
- `ExtendedResult<T, TE> OrElse(Func<TE, ExtendedResult<T, TE>> alternative)` - Provide alternative on failure
- `ExtendedResult<T, TE> Inspect(Action<T> action)` - Execute action on success value
- `ExtendedResult<T, TE> InspectErr(Action<TE> action)` - Execute action on error value

#### Extension Methods (ExtendedResultExtensions)
- `T Unwrap()` - Extract value or throw (use with caution)
- `T Expect(string message)` - Extract value or throw with custom message
- `ExtendedResult<U, TE> Map<U>(Func<T, U> mapper)` - Transform success value
- `ExtendedResult<U, TE> Bind<U>(Func<T, ExtendedResult<U, TE>> binder)` - Chain operations (flatMap)
- `ExtendedResult<T, TE2> MapError<TE2>(Func<TE, TE2> errorMapper)` - Transform error value
- `ExtendedResult<T, TE> Tap(Action<T> onSuccess, Action<TE> onFailure)` - Side effects for both branches
- `ExtendedResult<U, TE> Select<U>(Func<T, U> selector)` - LINQ projection support
- `ExtendedResult<U, TE> SelectMany<U>(Func<T, ExtendedResult<U, TE>> selector)` - LINQ monadic bind
- `ExtendedResult<IEnumerable<T>, TE> Combine()` (on `IEnumerable<ExtendedResult<T, TE>>`) - Aggregate results
- `(List<T> successes, List<TE> failures) Partition()` (on `IEnumerable<ExtendedResult<T, TE>>`) - Split successes/failures

#### Equality & Hash Code
- Supports value-based equality for Success and Failure cases
- Null-safe hash code computation
- Implements `Equals`, `GetHashCode`, `==`, `!=`
- `ToString()` returns `"Ok(value)"` or `"Err(error)"`

---

## Why Use Result Types?

### Traditional Exception-Based Approach
```csharp
public User GetUser(int id)
{
    var user = database.FindUser(id);
    if (user == null)
        throw new NotFoundException($"User {id} not found");
    return user;
}

// Caller has no indication this method can throw
User user = GetUser(123); // Might throw at runtime!
```

### Result-Based Approach
```csharp
public Result<User, string> GetUser(int id)
{
    var user = database.FindUser(id);
    if (user == null)
        return Result<User, string>.Err($"User {id} not found");
    return Result<User, string>.Ok(user);
}

// Failure is explicit in the type signature
Result<User, string> result = GetUser(123);
var message = result.Match(
    success: user => $"Found: {user.Name}",
    failure: error => $"Error: {error}"
);
```

## Benefits

- ✅ **Explicit Error Handling**: Method signatures clearly communicate potential failures
- ✅ **Type-Safe Optional Values**: `Option<T>` eliminates null reference exceptions
- ✅ **Type Safety**: Compile-time guarantees about error handling
- ✅ **Rich Error Context**: Chain error context as failures propagate up the call stack
- ✅ **Error Categorization**: 14 predefined error kinds for appropriate handling
- ✅ **Performance**: Avoid exception overhead for expected failure cases
- ✅ **Composability**: Easily chain operations with functional combinators
- ✅ **Testability**: Easier to test both success and failure paths
- ✅ **No Null References**: Use `Option<T>` and `Result<T, E>` to avoid `NullReferenceException`
- ✅ **Better Code Flow**: Failures don't break the natural flow of your code
- ✅ **Pattern Matching**: Leverage C# pattern matching for elegant value handling
- ✅ **LINQ Integration**: Use familiar C# query syntax for error handling workflows
- ✅ **Async/Await Support**: Full integration with async patterns including cancellation
- ✅ **Cancellable Operations**: Graceful cancellation of long-running async operations
- ✅ **Debugging Support**: Metadata attachment and full error chain display for debugging

## Testing

The library includes comprehensive test coverage with **417 unit tests** covering:
- **Result<T, E>** (260 tests)
  - Basic creation and inspection
  - Pattern matching
  - Equality and hash code
  - Map and Bind operations
  - LINQ query syntax integration (SelectMany, Select, from/select)
  - Advanced features (MapError, Expect, Tap, Contains)
  - Collection operations (Combine, Partition)
  - Full async support (MapAsync, BindAsync, TapAsync, OrElseAsync, CombineAsync)
  - Cancellation token support (all async methods with cancellation scenarios)
- **ExtendedResult<T, TE>** (19 tests)
  - Basic creation and value/error extraction
  - Pattern matching with records
  - Instance methods (UnwrapOr, UnwrapOrElse, Inspect, InspectErr, OrElse)
  - Extension methods (Map, Bind, MapError, Tap, Unwrap, Expect)
  - LINQ support (Select, SelectMany)
  - Collection operations (Combine, Partition)
  - Static helpers (Try, TryAsync)
  - Edge cases (null values, null errors)
  - Equality and hash code
- **Option<T>** (43 tests)
  - Creation and value access
  - Pattern matching with switch expressions
  - Equality and hash code
  - Record functionality (with expressions, ToString)
  - Collection integration (List, HashSet, Dictionary, LINQ)
  - Edge cases (nested options, tuples, null handling)
- **Error type** (64 comprehensive tests)
  - Context chaining and error propagation
  - Type-safe metadata with generics
  - Metadata type validation
  - Exception conversion with 11 exception types
  - Error kind modification
  - Stack trace capture (configurable)
  - Depth limiting (50 levels)
  - Circular reference detection
  - Full error chain formatting
  - Equality and hash code
- **🧪 Experimental Mutex<T>** (36 tests)
  - Lock acquisition and release
  - Try-lock and timeout variants
  - Async locking with cancellation
  - Concurrency stress tests
  - RAII guard management
- Exception handling (Try/TryAsync)
- Side effects (Inspect/InspectErr)
- Value extraction methods
- Null handling for nullable types

---

**Experimental Features**

### 🧪 Mutex<T> & RwLock<T> - Thread-Safe Synchronization Primitives

**Status:** Experimental - API may change in future versions

Rust-inspired synchronization primitives for protecting shared data, suitable for both synchronous and asynchronous contexts:

#### Mutex<T> - Mutual Exclusion

```csharp
using Esox.SharpAndRusty.Sync;
using Esox.SharpAndRusty.Types;

// Create a mutex protecting shared data
var mutex = new Mutex<int>(0);

// Synchronous locking
var result = mutex.Lock();
if (result.TryGetValue(out var guard))
{
    using (guard)
    {
        guard.Value++;  // Safe mutation
    } // Lock automatically released
}

// Non-blocking try
var tryResult = mutex.TryLock();

// Async locking with cancellation
var asyncResult = await mutex.LockAsync(cancellationToken);
```

#### RwLock<T> - Reader-Writer Lock

```csharp
using Esox.SharpAndRusty.Sync;
using Esox.SharpAndRusty.Types;

// Create a reader-writer lock
var rwlock = new RwLock<int>(42);

// Multiple readers can access simultaneously
var readResult = rwlock.Read();
if (readResult.TryGetValue(out var readGuard))
{
    using (readGuard)
    {
        Console.WriteLine(readGuard.Value); // Read-only access
    }
}

// Exclusive writer access
var writeResult = rwlock.Write();
if (writeResult.TryGetValue(out var writeGuard))
{
    using (writeGuard)
    {
        writeGuard.Value = 100; // Exclusive write access
    }
}
```

**Key Features:**
- ✅ **Result-Based Locking** - All lock operations return `Result<Guard<T>, Error>`
- ✅ **RAII Lock Management** - Automatic lock release via `IDisposable`
- ✅ **Multiple Lock Strategies** - Blocking, try-lock, and timeout variants
- ✅ **Type-Safe** - Compile-time guarantees for protected data access
- ✅ **Sync & Async Support** - Works in both synchronous and asynchronous contexts
- ✅ **Reader-Writer Optimization** - `RwLock<T>` allows concurrent readers

**Mutex<T> Methods:**
- `Lock()` - Blocking lock acquisition (sync)
- `TryLock()` - Non-blocking attempt
- `TryLockTimeout(TimeSpan)` - Lock with timeout
- `LockAsync(CancellationToken)` - Async lock
- `LockAsyncTimeout(TimeSpan, CancellationToken)` - Async lock with timeout

**RwLock<T> Methods:**
- `Read()` - Acquire read lock (allows concurrent readers)
- `TryRead()` - Non-blocking read attempt
- `TryReadTimeout(TimeSpan)` - Read with timeout
- `Write()` - Acquire exclusive write lock
- `TryWrite()` - Non-blocking write attempt
- `TryWriteTimeout(TimeSpan)` - Write with timeout

**⚠️ Experimental Notice:**

The `Mutex<T>` and `RwLock<T>` APIs are currently experimental and may undergo changes based on user feedback and real-world usage patterns. While fully tested, we recommend:

- Using them in non-critical paths initially
- Providing feedback on the API design
- Testing thoroughly in your specific use cases
- Being prepared for potential API changes in minor version updates

See [MUTEX_DOCUMENTATION.md](../MUTEX_DOCUMENTATION.md) for complete `Mutex<T>` documentation and usage examples.

## Why Use Result Types?




