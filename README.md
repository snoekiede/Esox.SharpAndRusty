# Esox.SharpAndRusty

A C# library that brings Rust-inspired `Result<T, E>` type to .NET, providing a type-safe way to handle operations that can succeed or fail without relying on exceptions for control flow.

## Features

- **Type-Safe Error Handling**: Explicitly represent success and failure states in your type signatures
- **Rust-Inspired API**: Familiar patterns for developers coming from Rust or functional programming
- **Zero Overhead**: Implemented as a `readonly struct` for optimal performance
- **Functional Composition**: Chain operations with `Map` and `Bind` extensions
- **Pattern Matching**: Use the `Match` method for elegant success/failure handling
- **Implicit Conversions**: Seamlessly create results from values or errors
- **.NET 10 Compatible**: Built for the latest .NET platform

## Installation

```bash
# Clone the repository
git clone https://github.com/yourusername/Esox.SharpAndRusty.git

# Build the project
dotnet build
```

## Usage

### Basic Example

```csharp
using Esox.SharpAndRusty.Types;
using Esox.SharpAndRusty.Extensions;

// Create a successful result
Result<int, string> success = Result<int, string>.Ok(42);

// Create a failed result
Result<int, string> failure = Result<int, string>.Err("Something went wrong");

// Implicit conversions work too!
Result<int, string> implicitSuccess = 42;
Result<int, string> implicitFailure = "Error message";
```

### Pattern Matching

```csharp
public string ProcessResult(Result<int, string> result)
{
    return result.Match(
        success: value => $"Success: {value}",
        failure: error => $"Error: {error}"
    );
}
```

### Functional Composition with Map

Transform success values while preserving error states:

```csharp
Result<int, string> result = Result<int, string>.Ok(10);

Result<string, string> mapped = result.Map(x => $"The number is {x}");
// Result: Ok("The number is 10")

Result<int, string> failed = Result<int, string>.Err("Invalid input");
Result<string, string> mappedFailed = failed.Map(x => $"The number is {x}");
// Result: Err("Invalid input") - error propagated
```

### Chaining Operations with Bind

Chain multiple operations that can fail:

```csharp
Result<int, string> ParseInt(string input)
{
    if (int.TryParse(input, out int value))
        return Result<int, string>.Ok(value);
    return Result<int, string>.Err("Invalid integer");
}

Result<int, string> Divide(int numerator, int denominator)
{
    if (denominator == 0)
        return Result<int, string>.Err("Division by zero");
    return Result<int, string>.Ok(numerator / denominator);
}

// Chain operations
Result<int, string> result = ParseInt("10")
    .Bind(value => Divide(value, 2));
// Result: Ok(5)

Result<int, string> failedResult = ParseInt("abc")
    .Bind(value => Divide(value, 2));
// Result: Err("Invalid integer")
```

### Checking Result State

```csharp
Result<int, string> result = GetSomeResult();

if (result.IsSuccess)
{
    // Handle success case
}

if (result.IsFailure)
{
    // Handle failure case
}
```

### Unwrapping (Use with Caution)

```csharp
Result<int, string> result = Result<int, string>.Ok(42);

// This will return 42
int value = result.Unwrap();

// This will throw InvalidOperationException
Result<int, string> failed = Result<int, string>.Err("Error");
int willThrow = failed.Unwrap(); // Throws!
```

## API Reference

### `Result<T, E>` Type

#### Properties
- `IsSuccess` - Returns `true` if the result represents success
- `IsFailure` - Returns `true` if the result represents failure

#### Static Methods
- `Ok(T value)` - Creates a successful result
- `Err(E error)` - Creates a failed result

#### Instance Methods
- `Match<R>(Func<T, R> success, Func<E, R> failure)` - Pattern match on the result

### Extension Methods

#### `Map<T, E, U>`
Transforms the success value while propagating errors:
```csharp
Result<U, E> Map<T, E, U>(this Result<T, E> result, Func<T, U> mapper)
```

#### `Bind<T, E, U>`
Chains operations that return results (also known as `flatMap` or `andThen`):
```csharp
Result<U, E> Bind<T, E, U>(this Result<T, E> result, Func<T, Result<U, E>> binder)
```

#### `Unwrap<T, E>`
Extracts the success value or throws an exception:
```csharp
T Unwrap<T, E>(this Result<T, E> result)
```

## Why Use Result Types?

### Traditional Exception-Based Approach
```csharp
public int Divide(int a, int b)
{
    if (b == 0)
        throw new DivideByZeroException();
    return a / b;
}

// Caller has no indication this method can fail
int result = Divide(10, 0); // Runtime exception!
```

### Result-Based Approach
```csharp
public Result<int, string> Divide(int a, int b)
{
    if (b == 0)
        return Result<int, string>.Err("Cannot divide by zero");
    return Result<int, string>.Ok(a / b);
}

// Failure is explicit in the type signature
Result<int, string> result = Divide(10, 0);
// Compiler encourages handling both cases
```

## Benefits

- **Explicit Error Handling**: Method signatures clearly communicate potential failures
- **Type Safety**: Compile-time guarantees about error handling
- **Performance**: Avoid exception overhead for expected failure cases
- **Composability**: Easily chain operations with functional combinators
- **Testability**: Easier to test both success and failure paths

## License

[Your License Here]

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Acknowledgments

Inspired by Rust's `Result<T, E>` type and functional programming principles.
