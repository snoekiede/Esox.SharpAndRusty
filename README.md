# Esox.SharpAndRusty

A production-ready C# library that brings Rust-inspired `Result<T, E>` type to .NET, providing a type-safe way to handle operations that can succeed or fail without relying on exceptions for control flow.

## ⚠️ Disclaimer

This library is provided "as is" without warranty of any kind, either express or implied, including but not limited to the warranties of merchantability, fitness for a particular purpose, and non-infringement. In no event shall the authors or copyright holders be liable for any claim, damages, or other liability, whether in an action of contract, tort, or otherwise, arising from, out of, or in connection with the software or the use or other dealings in the software.

**Use at your own risk.** While this library has been designed to be production-ready with comprehensive test coverage, it is your responsibility to evaluate its suitability for your specific use case and to test it thoroughly in your environment before deploying to production.

## Features

- ✅ **Type-Safe Error Handling**: Explicitly represent success and failure states in your type signatures
- ✅ **Rust-Inspired API**: Familiar patterns for developers coming from Rust or functional programming
- ✅ **Rich Error Type**: Rust-inspired `Error` type with context chaining, metadata, and error categorization
- ✅ **Unit Type**: Rust-inspired `Unit` type for operations that don't return meaningful values
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

### 🧪 Experimental Features

> **⚠️ EXPERIMENTAL**: The following features are experimental and may change in future versions. Use with caution in production environments.

- 🧪 **Mutex<T>**: Rust-inspired mutual exclusion primitive with Result-based locking (see [Experimental Features](#experimental-features-1))
- 🧪 **RwLock<T>**: Rust-inspired reader-writer lock with Result-based locking (see [Experimental Features](#experimental-features-1))

These experimental features are thoroughly tested but their APIs may evolve based on community feedback.

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
```

## Usage Examples

### Basic Operations

```csharp
// Creating results
var success = Result<int, string>.Ok(42);
var failure = Result<int, string>.Err("Not found");

// Checking state
if (success.IsSuccess)
{
    Console.WriteLine("Operation succeeded!");
}

if (failure.IsFailure)
{
    Console.WriteLine("Operation failed!");
}

// Equality comparison
var result1 = Result<int, string>.Ok(42);
var result2 = Result<int, string>.Ok(42);
Console.WriteLine(result1 == result2); // True

// String representation for debugging
Console.WriteLine(success); // Output: Ok(42)
Console.WriteLine(failure); // Output: Err(Not found)
```

### Safe Value Extraction

```csharp
var result = GetUserAge();

// Option 1: Try pattern
if (result.TryGetValue(out var age))
{
    Console.WriteLine($"User is {age} years old");
}

// Option 2: Provide default value
var age = result.UnwrapOr(0);

// Option 3: Compute default based on error
var age = result.UnwrapOrElse(error => 
{
    Logger.Warn($"Failed to get age: {error}");
    return 0;
});

// Option 4: Try to get error
if (result.TryGetError(out var error))
{
    Console.WriteLine($"Error occurred: {error}");
}
```

### Pattern Matching

```csharp
public string ProcessResult(Result<User, string> result)
{
    return result.Match(
        success: user => $"Welcome, {user.Name}!",
        failure: error => $"Error: {error}"
    );
}

// Convert to different type
public int GetLengthOrZero(Result<string, int> result)
{
    return result.Match(
        success: text => text.Length,
        failure: errorCode => 0
    );
}
```

### Functional Composition with Map

Transform success values while automatically propagating errors:

```csharp
Result<int, string> GetUserAge() => Result<int, string>.Ok(25);

// Transform the success value
var result = GetUserAge()
    .Map(age => $"User is {age} years old");
// Result: Ok("User is 25 years old")

// Errors propagate automatically
Result<int, string> failed = Result<int, string>.Err("User not found");
var mappedFailed = failed.Map(age => $"User is {age} years old");
// Result: Err("User not found")
```

### Chaining Operations with Bind

Chain multiple operations that can fail, stopping at the first error:

```csharp
Result<int, string> ParseInt(string input)
{
    if (int.TryParse(input, out int value))
        return Result<int, string>.Ok(value);
    return Result<int, string>.Err($"Cannot parse '{input}' as integer");
}

Result<int, string> Divide(int numerator, int denominator)
{
    if (denominator == 0)
        return Result<int, string>.Err("Division by zero");
    return Result<int, string>.Ok(numerator / denominator);
}

Result<int, string> ValidatePositive(int value)
{
    if (value <= 0)
        return Result<int, string>.Err("Result must be positive");
    return Result<int, string>.Ok(value);
}

// Chain operations - stops at first error
var result = ParseInt("100")
    .Bind(value => Divide(value, 5))
    .Bind(value => ValidatePositive(value));
// Result: Ok(20)

var failedResult = ParseInt("100")
    .Bind(value => Divide(value, 0))
    .Bind(value => ValidatePositive(value));
// Result: Err("Division by zero") - ValidatePositive never executes
```

### LINQ Query Syntax

Use familiar C# LINQ query syntax for elegant error handling:

```csharp
// Simple query
var result = from x in ParseInt("10")
             from y in ParseInt("20")
             select x + y;
// Result: Ok(30)

// Complex query with validation
var result = from input in ParseInt("100")
             from divisor in ParseInt("5")
             from quotient in Divide(input, divisor)
             from validated in ValidatePositive(quotient)
             select $"Result: {validated}";
// Result: Ok("Result: 20")

// Error propagation - stops at first error
var result = from x in ParseInt("10")
             from y in ParseInt("abc") // Parse fails here
             from z in ParseInt("30")  // Never executes
             select x + y + z;
// Result: Err("Cannot parse 'abc' as integer")

// Works with different types
var result = from name in GetUserName(userId)
             from age in GetUserAge(userId)
             select $"{name} is {age} years old";
```

**Note on Validation:** LINQ `where` clauses are not supported for Result types because predicates cannot provide meaningful error messages. Instead, use `Bind` with explicit validation:

```csharp
// ❌ where is not available (by design)
// var result = from x in GetValue()
//              where x > 5  // Cannot provide error message
//              select x * 2;

// ✅ Use Bind with explicit validation instead
var result = GetValue()
    .Bind(x => x > 5 
        ? Result<int, string>.Ok(x) 
        : Result<int, string>.Err("Value must be greater than 5"))
    .Map(x => x * 2);

// ✅ Or use validation helper functions in LINQ queries
Result<int, string> ValidatePositive(int value) =>
    value > 0
        ? Result<int, string>.Ok(value)
        : Result<int, string>.Err("Value must be positive");

var result = from x in GetValue()
             from validated in ValidatePositive(x)
             select validated * 2;
```
### Combining Map and Bind

```csharp
var result = ParseInt("42")
    .Map(x => x * 2)              // Transform value: 42 -> 84
    .Bind(x => Divide(x, 2))      // Chain operation: 84 / 2 = 42
    .Map(x => $"Result: {x}");    // Transform to string
// Result: Ok("Result: 42")
```

### Fallback with OrElse

Provide alternative results when operations fail:

```csharp
Result<User, string> GetUserFromCache(int id) => 
    Result<User, string>.Err("Not in cache");

Result<User, string> GetUserFromDatabase(int id) => 
    Result<User, string>.Ok(new User { Id = id, Name = "John" });

// Try cache first, fallback to database
var user = GetUserFromCache(123)
    .OrElse(error => 
    {
        Logger.Info($"Cache miss: {error}. Trying database...");
        return GetUserFromDatabase(123);
    });
// Result: Ok(User { Id = 123, Name = "John" })
```

### Side Effects with Inspect

Execute side effects without transforming the result:

```csharp
var result = GetUser(userId)
    .Inspect(user => Logger.Info($"Found user: {user.Name}"))
    .InspectErr(error => Logger.Error($"User lookup failed: {error}"))
    .Map(user => user.Email);

// Logs are written, but result is transformed only on success
```

### Exception Handling

Wrap operations that might throw exceptions:

```csharp
// Synchronous operation
var result = Result<int, string>.Try(
    operation: () => int.Parse("42"),
    errorHandler: ex => $"Parse failed: {ex.Message}"
);
// Result: Ok(42)

// Async operation
var asyncResult = await Result<User, string>.TryAsync(
    operation: async () => await httpClient.GetUserAsync(userId),
    errorHandler: ex => $"HTTP request failed: {ex.Message}"
);

// Real-world example: File operations
var fileContent = Result<string, string>.Try(
    operation: () => File.ReadAllText("config.json"),
    errorHandler: ex => $"Failed to read config: {ex.Message}"
);

// Using the Error type for automatic exception conversion
var richResult = ErrorExtensions.Try(() => int.Parse("42"));
// Automatically converts exceptions to Error with appropriate ErrorKind

var asyncRichResult = await ErrorExtensions.TryAsync(
    async () => await File.ReadAllTextAsync("config.json"));
// Returns Result<string, Error>
```

### Rich Error Handling with Error Type

The library includes a Rust-inspired `Error` type for rich error handling with **production-grade optimizations**:

```csharp
using Esox.SharpAndRusty.Types;
using Esox.SharpAndRusty.Extensions;

// Automatic exception conversion with error kinds
var result = ErrorExtensions.Try(() => File.ReadAllText("config.json"))
    .Context("Failed to load configuration")
    .WithMetadata("path", "config.json")
    .WithKind(ErrorKind.NotFound);

// Type-safe metadata with compile-time safety
var error = Error.New("Operation failed")
    .WithMetadata("userId", 123)           // Type-safe: int
    .WithMetadata("timestamp", DateTime.UtcNow)  // Type-safe: DateTime
    .WithMetadata("isRetryable", true);    // Type-safe: bool

// Type-safe metadata retrieval
if (error.TryGetMetadata("userId", out int userId))
{
    Console.WriteLine($"Failed for user: {userId}");
}

// Context chaining for error propagation
Result<Config, Error> LoadConfig(string path)
{
    return ReadFile(path)
        .Context($"Failed to load config from {path}")
        .Bind(content => ParseConfig(content)
            .Context("Failed to parse configuration")
            .WithKind(ErrorKind.ParseError));
}

// Full error chain display with metadata
if (result.TryGetError(out var error))
{
    Console.Error.WriteLine(error.GetFullMessage());
    // Output:
    // "NotFound: Failed to load configuration"
    //   [path=/etc/app/config.json]
    //   [attemptCount=3]
    //   Caused by: "Io: File not found: config.json"
}

// Error categorization with ErrorKind
if (error.Kind == ErrorKind.NotFound)
{
    // Handle not found specifically
}

// Optional stack trace capture (performance-aware)
var errorWithTrace = error.CaptureStackTrace(includeFileInfo: false);  // Fast
var detailedError = error.CaptureStackTrace(includeFileInfo: true);    // Detailed
```

**Production Features:**
- ✅ **ImmutableDictionary** - Efficient metadata storage with structural sharing
- ✅ **Type-safe metadata API** - Generic overloads for compile-time type safety
- ✅ **Metadata type validation** - Validates types at addition time, not serialization
- ✅ **Depth Limiting** - Error chains truncated at 50 levels (prevents stack overflow)
- ✅ **Circular Reference Detection** - HashSet-based cycle detection
- ✅ **Expanded Exception Mapping** - 11 common exception types automatically mapped
- ✅ **Configurable Stack Traces** - Optional file info for performance tuning
- ✅ **Metadata Retrieval Performance** - O(log n) for adding metadata, O(1) for depth limit check

See [ERROR_TYPE.md](ERROR_TYPE.md) for comprehensive Error type documentation.
See [ERROR_TYPE_PRODUCTION_IMPROVEMENTS.md](ERROR_TYPE_PRODUCTION_IMPROVEMENTS.md) for detailed production optimization information.

### Unit Type for Void Results

The library includes a `Unit` type for operations that can fail but don't return meaningful values on success, similar to Rust's `()` type:

```csharp
using Esox.SharpAndRusty.Types;

// Operation that can fail but doesn't return a value
public Result<Unit, Error> TerminateLogger()
{
    if (_alreadyTerminated)
    {
        return Result<Unit, Error>.Err(
            Error.New("Logger has already been terminated")
                .WithKind(ErrorKind.InvalidState)
        );
    }
    
    _logger.Shutdown();
    _alreadyTerminated = true;
    
    return Result<Unit, Error>.Ok(Unit.Value);
}

// Usage with pattern matching
var result = TerminateLogger();
result.Match(
    success: _ => Console.WriteLine("Logger terminated successfully"),
    failure: error => Console.WriteLine($"Failed: {error.Message}")
);

// File operations that return Unit
public Result<Unit, Error> SaveToFile(string path, string content)
{
    try
    {
        File.WriteAllText(path, content);
        return Result<Unit, Error>.Ok(Unit.Value);
    }
    catch (Exception ex)
    {
        return Result<Unit, Error>.Err(Error.FromException(ex));
    }
}

// Check for errors
if (result.TryGetError(out var error))
{
    Console.WriteLine($"Operation failed: {error.Kind} - {error.Message}");
}
```

**Key Features:**
- ✅ **Single Value Semantics**: All `Unit` instances are equal
- ✅ **Perfect for Result Types**: Use `Result<Unit, E>` for operations without return values
- ✅ **Rust-Inspired**: Matches Rust's `()` type behavior  
- ✅ **Zero Overhead**: Readonly struct with minimal memory footprint
- ✅ **String Representation**: `ToString()` returns `"()"` for debugging

## Production Readiness

This library is production-ready with:
- ✅ Full equality implementation
- ✅ Comprehensive API surface
- ✅ Exception handling helpers
- ✅ Extensive test coverage (**323 tests total: 267 production + 19 Unit type + 37 RwLock experimental, 100% passing**)
- ✅ Proper null handling
- ✅ Argument validation
- ✅ Clear documentation
- ✅ **Full LINQ query syntax support**
- ✅ **Complete async/await integration**
- ✅ **Cancellation token support for all async operations**
- ✅ **Advanced error handling features** (MapError, Expect, Tap, etc.)
- ✅ **Rich Error type with context chaining and metadata**
- ✅ **Unit type for void operations**
- ✅ **Collection operations** (Combine, Partition)
- ✅ **100% backward compatibility**
- ✅ **Production-optimized Error type** (ImmutableDictionary, depth limits, circular detection)
- ✅ **Type-safe metadata API** with compile-time guarantees
- ✅ **Memory-efficient** with structural sharing
- ✅ **Stack-safe** with depth and cycle protection

### Feature Maturity

| Feature | Status | Tests | Production Ready |
|---------|--------|-------|------------------|
| Result<T, E> | ✅ Stable | 137 | Yes (9.5/10) |
| Error Type | ✅ Stable | 123 | Yes (9.5/10) |
| Unit Type | ✅ Stable | 19 | Yes (10/10) |
| LINQ Support | ✅ Stable | Integrated | Yes |
| Async/Await | ✅ Stable | 37 | Yes |
| Mutex<T> | 🧪 Experimental | 36 | Use with caution |
| RwLock<T> | 🧪 Experimental | 37 | Use with caution |

**Core Result/Error/Unit Functionality**: Production-ready (9.5/10)
**Experimental Mutex/RwLock**: Thoroughly tested but API may change

