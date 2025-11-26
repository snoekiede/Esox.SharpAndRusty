# Esox.SharpAndRusty

A production-ready C# library that brings Rust-inspired `Result<T, E>` type to .NET, providing a type-safe way to handle operations that can succeed or fail without relying on exceptions for control flow.

## ⚠️ Disclaimer

This library is provided "as is" without warranty of any kind, either express or implied, including but not limited to the warranties of merchantability, fitness for a particular purpose, and non-infringement. In no event shall the authors or copyright holders be liable for any claim, damages, or other liability, whether in an action of contract, tort, or otherwise, arising from, out of, or in connection with the software or the use or other dealings in the software.

**Use at your own risk.** While this library has been designed to be production-ready with comprehensive test coverage, it is your responsibility to evaluate its suitability for your specific use case and to test it thoroughly in your environment before deploying to production.

## Features

- ✅ **Type-Safe Error Handling**: Explicitly represent success and failure states in your type signatures
- ✅ **Rust-Inspired API**: Familiar patterns for developers coming from Rust or functional programming
- ✅ **Zero Overhead**: Implemented as a `readonly struct` for optimal performance
- ✅ **Functional Composition**: Chain operations with `Map`, `Bind`, and `OrElse`
- ✅ **Pattern Matching**: Use the `Match` method for elegant success/failure handling
- ✅ **Full Equality Support**: Implements `IEquatable<T>` with proper `==`, `!=`, and `GetHashCode()`
- ✅ **Safe Value Extraction**: `TryGetValue`, `UnwrapOr`, and `UnwrapOrElse` methods
- ✅ **Exception Handling Helpers**: Built-in `Try` and `TryAsync` for wrapping operations
- ✅ **Inspection Methods**: Execute side effects with `Inspect` and `InspectErr`
- ✅ **.NET 10 Compatible**: Built for the latest .NET platform with C# 14

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
```

### Complex Real-World Example

```csharp
public async Task<Result<OrderConfirmation, string>> ProcessOrderAsync(OrderRequest request)
{
    // Validate, process payment, create order, send email - all in a chain
    return await ValidateOrder(request)
        .Bind(order => ProcessPayment(order))
        .Inspect(payment => Logger.Info($"Payment processed: {payment.TransactionId}"))
        .Bind(payment => CreateOrder(payment))
        .Bind(async order => await SendConfirmationEmail(order))
        .InspectErr(error => Logger.Error($"Order processing failed: {error}"))
        .OrElse(error => 
        {
            // Fallback: create pending order for manual review
            return CreatePendingOrder(request, error);
        });
}

Result<Order, string> ValidateOrder(OrderRequest request)
{
    if (request.Items.Count == 0)
        return Result<Order, string>.Err("Order must contain at least one item");
    if (request.Total <= 0)
        return Result<Order, string>.Err("Order total must be positive");
    
    return Result<Order, string>.Ok(new Order(request));
}

Result<Payment, string> ProcessPayment(Order order)
{
    // Payment processing logic
    return paymentService.Charge(order.Total)
        ? Result<Payment, string>.Ok(new Payment { Amount = order.Total })
        : Result<Payment, string>.Err("Payment declined");
}
```

## API Reference

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

### Extension Methods (ResultExtensions)

#### `Map<T, E, U>`
Transforms the success value while propagating errors:
```csharp
Result<U, E> Map<T, E, U>(this Result<T, E> result, Func<T, U> mapper)
```

**Example:**
```csharp
var result = Result<int, string>.Ok(5);
var mapped = result.Map<int, string, string>(x => $"Value: {x}");
// Result: Ok("Value: 5")
```

#### `Bind<T, E, U>`
Chains operations that return results (also known as `flatMap` or `andThen`):
```csharp
Result<U, E> Bind<T, E, U>(this Result<T, E> result, Func<T, Result<U, E>> binder)
```

**Example:**
```csharp
var result = Result<int, string>.Ok(10)
    .Bind(x => x > 0 
        ? Result<int, string>.Ok(x * 2) 
        : Result<int, string>.Err("Must be positive"));
// Result: Ok(20)
```

#### `Unwrap<T, E>`
Extracts the success value or throws an exception (use with caution):
```csharp
T Unwrap<T, E>(this Result<T, E> result)
```

**Example:**
```csharp
var result = Result<int, string>.Ok(42);
var value = result.Unwrap(); // Returns 42

var failed = Result<int, string>.Err("Error");
var willThrow = failed.Unwrap(); // Throws InvalidOperationException
```

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
- ✅ **Type Safety**: Compile-time guarantees about error handling
- ✅ **Performance**: Avoid exception overhead for expected failure cases
- ✅ **Composability**: Easily chain operations with functional combinators
- ✅ **Testability**: Easier to test both success and failure paths
- ✅ **No Null References**: Avoid `NullReferenceException` by making errors explicit
- ✅ **Better Code Flow**: Failures don't break the natural flow of your code

## Testing

The library includes comprehensive test coverage with 52+ unit tests covering:
- Basic creation and inspection
- Pattern matching
- Equality and hash code
- Map and Bind operations
- Exception handling (Try/TryAsync)
- Side effects (Inspect/InspectErr)
- Value extraction methods
- Null handling for nullable types

Run tests:
```bash
dotnet test
```

## Production Readiness

This library is production-ready with:
- ✅ Full equality implementation
- ✅ Comprehensive API surface
- ✅ Exception handling helpers
- ✅ Extensive test coverage
- ✅ Proper null handling
- ✅ Argument validation
- ✅ Clear documentation

See [RESULT_TYPE_IMPROVEMENTS.md](RESULT_TYPE_IMPROVEMENTS.md) for detailed information about production-ready features.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

MIT License - see LICENSE file for details

## Acknowledgments

Inspired by Rust's `Result<T, E>` type and functional programming principles. This library brings idiomatic Rust error handling patterns to the C# ecosystem while respecting .NET conventions and best practices.
