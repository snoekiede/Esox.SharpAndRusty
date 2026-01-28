﻿# Esox.SharpAndRusty

A production-ready C# library that brings Rust-inspired patterns to .NET, including `Result<T, E>` for type-safe error handling and `Option<T>` for representing optional values without null references.

## ⚠️ Disclaimer

This library is provided "as is" without warranty of any kind, either express or implied, including but not limited to the warranties of merchantability, fitness for a particular purpose, and non-infringement. In no event shall the authors or copyright holders be liable for any claim, damages, or other liability, whether in an action of contract, tort, or otherwise, arising from, out of, or in connection with the software or the use or other dealings in the software.

**Use at your own risk.** While this library has been designed to be production-ready with comprehensive test coverage, it is your responsibility to evaluate its suitability for your specific use case and to test it thoroughly in your environment before deploying to production.

## Features

- ✅ **Type-Safe Error Handling**: Explicitly represent success and failure states in your type signatures
- ✅ **Option Type**: Rust-inspired `Option<T>` for representing optional values without null references
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
- ✅ **ExtendedResult<T, TE>**: Record-based discriminated union alternative with pattern matching and LINQ support
- 🧪 **Experimental: Mutex<T>**: Rust-inspired mutual exclusion primitive with Result-based locking (works in both sync and async contexts)
- 🧪 **Experimental: RwLock<T>**: Rust-inspired reader-writer lock for shared data access (works in both sync and async contexts)

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
- [Quick Start](#quick-start)
- [Usage Examples](#usage-examples)
- [API Reference](#api-reference)
- [ExtendedResult quick tips](#extendedresult-quick-tips)
- See full ExtendedResult docs in the package README: `Esox.SharpAndRusty/README.md` (section: "ExtendedResult<T, TE> — Record Alternative")

## Usage Examples

### Option<T> - Type-Safe Optional Values

The `Option<T>` type represents an optional value - either `Some(value)` or `None`. This provides a type-safe alternative to nullable reference types and eliminates null reference exceptions.

#### Creating Options

```csharp
using Esox.SharpAndRusty.Types;

// Create Some with a value
var someOption = new Option<int>.Some(42);

// Create None (no value)
var noneOption = new Option<int>.None();

// Real-world example: Safe dictionary lookup
Option<string> GetConfigValue(Dictionary<string, string> config, string key)
{
    return config.TryGetValue(key, out var value)
        ? new Option<string>.Some(value)
        : new Option<string>.None();
}
```

#### Pattern Matching with Options

```csharp
Option<User> FindUser(int userId) => /* ... */;

var user = FindUser(123);
var greeting = user switch
{
    Option<User>.Some(var u) => $"Hello, {u.Name}!",
    Option<User>.None => "User not found",
    _ => "Unknown"
};

// Or use if pattern
if (user is Option<User>.Some(var foundUser))
{
    Console.WriteLine($"Processing user: {foundUser.Name}");
}
```

#### Using Options in Collections

```csharp
var users = new List<Option<User>>
{
    new Option<User>.Some(new User { Id = 1, Name = "Alice" }),
    new Option<User>.None(),
    new Option<User>.Some(new User { Id = 2, Name = "Bob" })
};

// Extract all valid users
var validUsers = users
    .OfType<Option<User>.Some>()
    .Select(opt => opt.Value)
    .ToList();
// Result: [User(Alice), User(Bob)]
```

#### Record Features

```csharp
// Options are records, so you get equality by value
var opt1 = new Option<int>.Some(42);
var opt2 = new Option<int>.Some(42);
Console.WriteLine(opt1 == opt2); // True

// Use with expressions to create modified copies
var updated = opt1 with { Value = 43 };
Console.WriteLine(updated); // Some { Value = 43 }

// Use in dictionaries and hash sets
var dict = new Dictionary<Option<string>, int>
{
    { new Option<string>.Some("key"), 1 },
    { new Option<string>.None(), 0 }
};
```

#### Comparison with Nullable Types

```csharp
// Traditional nullable approach - prone to null reference exceptions
string? GetName(int id) => /* might return null */;
var name = GetName(123);
Console.WriteLine(name.Length); // NullReferenceException if null!

// Option approach - compiler forces you to handle None case
Option<string> GetNameSafe(int id) => /* returns Some or None */;
var nameOption = GetNameSafe(123);
var length = nameOption switch
{
    Option<string>.Some(var n) => n.Length,
    Option<string>.None => 0,
    _ => 0
};
// No risk of NullReferenceException!
```

### Result<T, E> - Basic Operations

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

// Option 5: Expect a value or throw
var age = result.Expect("Age not available");
// Throws InvalidOperationException with message "Age not available" if error

// Option 6: Check if result contains a value
if (result.Contains(42))
{
    Console.WriteLine("User is 42 years old");
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
    .Map<int, string, string>(age => $"User is {age} years old");
// Result: Ok("User is 25 years old")

// Errors propagate automatically
Result<int, string> failed = Result<int, string>.Err("User not found");
var mappedFailed = failed.Map<int, string, string>(age => $"User is {age} years old");
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
    .Map<int, string, int>(x => x * 2)              // Transform value: 42 -> 84
    .Bind(x => Divide(x, 2))                         // Chain operation: 84 / 2 = 42
    .Map<int, string, string>(x => $"Result: {x}"); // Transform to string
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
    .Map<User, string, string>(user => user.Email);

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
- ✅ **Metadata type validation** - Validates at addition time, not serialization
- ✅ **Depth Limiting** - Error chains truncated at 50 levels (prevents stack overflow)
- ✅ **Circular Reference Detection** - HashSet-based cycle detection
- ✅ **Expanded Exception Mapping** - 11 common exception types automatically mapped
- ✅ **Configurable Stack Traces** - Optional file info for performance tuning
- ✅ **Metadata Type Validation** - Validates types at addition time, not serialization

**Error Kind Categories:**
- `NotFound` - Entity not found
- `InvalidInput` - Invalid data
- `PermissionDenied` - Insufficient privileges
- `Timeout` - Operation timed out
- `Interrupted` - Operation cancelled/interrupted
- `ParseError` - Parsing failed
- `Io` - I/O error
- `ResourceExhausted` - Out of memory, disk full, etc.
- `InvalidOperation` - Operation invalid for current state
- And more... (14 categories total)

**Exception to ErrorKind Mapping:**
- `FileNotFoundException`, `DirectoryNotFoundException` → `NotFound`
- `TaskCanceledException`, `OperationCanceledException` → `Interrupted`
- `FormatException` → `ParseError`
- `OutOfMemoryException` → `ResourceExhausted`
- `TimeoutException` → `Timeout`
- `UnauthorizedAccessException` → `PermissionDenied`
- And more...

See [ERROR_TYPE.md](../ERROR_TYPE.md) for comprehensive Error type documentation.
See [ERROR_TYPE_PRODUCTION_IMPROVEMENTS.md](../ERROR_TYPE_PRODUCTION_IMPROVEMENTS.md) for detailed production optimization information.

## API Reference

### `Option<T>` Type

A type-safe way to represent optional values, eliminating null reference exceptions.

#### Creating Options
- `new Option<T>.Some(T value)` - Creates an option containing a value
- `new Option<T>.None()` - Creates an empty option

#### Pattern Matching
```csharp
var result = option switch
{
    Option<T>.Some(var value) => /* handle value */,
    Option<T>.None => /* handle absence */,
    _ => /* fallback */
};
```

#### Type Checks
- `option is Option<T>.Some` - Check if option contains a value
- `option is Option<T>.None` - Check if option is empty
- `option is Option<T>.Some(var value)` - Extract value with pattern matching

#### Record Features
- **Equality**: Options support value-based equality
- **Hash Code**: Safe to use in collections (HashSet, Dictionary)
- **With Expressions**: Create modified copies with `with { Value = newValue }`
- **ToString**: Automatically formatted as `"Some { Value = ... }"` or `"None { }"`

#### Example Usage
```csharp
// Type-safe dictionary lookup
Option<string> GetConfig(string key)
{
    return config.TryGetValue(key, out var value)
        ? new Option<string>.Some(value)
        : new Option<string>.None();
}

// Extract values from collections
var validValues = options
    .OfType<Option<T>.Some>()
    .Select(opt => opt.Value)
    .ToList();
```

### `Result<T, E>` Type

#### Properties
- `bool IsSuccess` - Returns `true` if the result represents success
- `bool IsFailure` - Returns `true` if the result represents failure

#### Static Factory Methods
- `Result<T, E> Ok(T value)` - Creates a successful result
- `Result<T, E> Err(E error)` - Creates a failed result
- `Result<T, E> Try(Func<T> operation, Func<Exception, E> errorHandler)` - Execute operation with exception handling
- `Task<Result<T, E>> TryAsync(Func<Task<T>> operation, Func<Exception, E> errorHandler)` - Async version of Try

#### Instance Methods
- `R Match<R>(Func<T, R> success, Func<E, R> failure)` - Pattern match on the result
- `bool TryGetValue(out T value)` - Try to get the success value
- `bool TryGetError(out E error)` - Try to get the error value
- `T UnwrapOr(T defaultValue)` - Get value or return default
- `T UnwrapOrElse(Func<E, T> defaultFactory)` - Get value or compute default
- `Result<T, E> OrElse(Func<E, Result<T, E>> alternative)` - Provide alternative on failure
- `Result<T, E> Inspect(Action<T> action)` - Execute action on success value
- `Result<T, E> InspectErr(Action<E> action)` - Execute action on error value

#### Equality Methods
- `bool Equals(Result<T, E> other)` - Check equality
- `int GetHashCode()` - Get hash code
- `bool operator ==(Result<T, E> left, Result<T, E> right)` - Equality operator
- `bool operator !=(Result<T, E> left, Result<T, E> right)` - Inequality operator
- `string ToString()` - Returns `"Ok(value)"` or `"Err(error)"`

### ExtendedResult quick tips

The record-based `ExtendedResult<T, TE>` offers lightweight success/failure checks:

- `IsSuccess` — true when the result holds a value
- `IsFailure` — true when the result holds an error

Example:

```csharp
using Esox.SharpAndRusty.Types;

var ok = ExtendedResult<int, string>.Ok(42);
var err = ExtendedResult<int, string>.Err("boom");

if (ok.IsSuccess)
{
    // handle success
}

if (err.IsFailure)
{
    // handle failure
}
```

For more APIs and patterns (Match, Try/TryAsync, Inspect/InspectErr, Map/Bind, LINQ, Combine/Partition), see the package README at `Esox.SharpAndRusty/README.md` under the section "ExtendedResult<T, TE> — Record Alternative" and "ExtendedResult<T, TE> Type (API)".
