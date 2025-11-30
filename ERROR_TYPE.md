# Error Type Documentation

This document describes the Rust-inspired `Error` type that integrates seamlessly with the `Result<T, E>` type in the Esox.SharpAndRusty library.

## Overview

The `Error` type provides a rich error handling mechanism inspired by Rust's error handling patterns, particularly the `anyhow` and `thiserror` crates. It supports:

- **Error Context Chaining**: Add context as errors propagate up the call stack
- **Error Categorization**: Categorize errors using `ErrorKind` enum
- **Exception Conversion**: Automatically convert .NET exceptions to errors
- **Metadata Attachment**: Attach structured metadata to errors
- **Stack Trace Capture**: Optional stack trace capture for debugging
- **Full Error Chain Display**: View the complete error chain with context

## Basic Usage

### Creating Errors

```csharp
using Esox.SharpAndRusty.Types;

// Simple error
var error = Error.New("File not found");

// Error with kind
var error = Error.New("File not found", ErrorKind.NotFound);

// From exception
try
{
    File.ReadAllText("config.json");
}
catch (Exception ex)
{
    var error = Error.FromException(ex);
}

// Implicit conversion from string
Error error = "Something went wrong";
```

## Error Context Chaining

One of the most powerful features is context chaining, similar to Rust's context pattern:

```csharp
Result<User, Error> GetUser(int userId)
{
    return database.FindUser(userId)
        .Context($"Failed to load user {userId}");
}

Result<Config, Error> LoadConfig(string path)
{
    return ReadFile(path)
        .Context("Failed to read configuration file")
        .Bind(content => ParseConfig(content)
            .Context("Failed to parse configuration"));
}

// Full error chain:
// "Failed to parse configuration"
//   Caused by: "Failed to read configuration file"
//     Caused by: "File not found: config.json"
```

## Error Kinds

The `ErrorKind` enum categorizes errors:

```csharp
public enum ErrorKind
{
    NotFound,              // Entity was not found
    PermissionDenied,      // Insufficient privileges
    ConnectionRefused,     // Connection refused
    ConnectionReset,       // Connection reset
    Timeout,               // Operation timed out
    Interrupted,           // Operation was interrupted
    InvalidInput,          // Invalid data provided
    NotSupported,          // Operation not supported
    Io,                    // I/O error
    AlreadyExists,         // Entity already exists
    InvalidOperation,      // Invalid operation for current state
    ParseError,            // Parsing error
    ResourceExhausted,     // Resource exhausted (memory, disk, etc.)
    Other                  // Unclassified error
}
```

### Using Error Kinds

```csharp
var result = ValidateUser(user)
    .WithKind(ErrorKind.InvalidInput);

if (result.TryGetError(out var error))
{
    switch (error.Kind)
    {
        case ErrorKind.NotFound:
            return NotFound();
        case ErrorKind.InvalidInput:
            return BadRequest(error.Message);
        case ErrorKind.PermissionDenied:
            return Unauthorized();
        default:
            return InternalServerError();
    }
}
```

## Metadata

Attach structured metadata to errors:

```csharp
var result = ProcessOrder(orderId)
    .WithMetadata("orderId", orderId)
    .WithMetadata("userId", userId)
    .WithMetadata("timestamp", DateTime.UtcNow);

if (result.TryGetError(out var error))
{
    if (error.TryGetMetadata("orderId", out var id))
    {
        Logger.Error($"Failed to process order {id}");
    }
}
```

## Working with Result<T, Error>

### Try Pattern

```csharp
using Esox.SharpAndRusty.Extensions;

// Synchronous
var result = ErrorExtensions.Try(() => int.Parse(input))
    .Context("Failed to parse user age");

// Asynchronous
var result = await ErrorExtensions.TryAsync(async () => 
    await httpClient.GetStringAsync(url))
    .ContextAsync("Failed to fetch data from API");
```

### Complete Example

```csharp
public class UserService
{
    public async Task<Result<User, Error>> CreateUserAsync(
        string username, 
        string email,
        CancellationToken cancellationToken = default)
    {
        return await ValidateUsername(username)
            .Context("Username validation failed")
            .WithMetadata("username", username)
            .BindAsync(async _ => 
                await ValidateEmailAsync(email, cancellationToken)
                    .ContextAsync("Email validation failed")
                    .WithMetadataAsync("email", email, cancellationToken),
                cancellationToken)
            .BindAsync(async _ => 
                await SaveUserAsync(username, email, cancellationToken)
                    .ContextAsync("Failed to save user to database"),
                cancellationToken);
    }

    private Result<Unit, Error> ValidateUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return Result<Unit, Error>.Err(
                Error.New("Username cannot be empty", ErrorKind.InvalidInput));

        if (username.Length < 3)
            return Result<Unit, Error>.Err(
                Error.New("Username must be at least 3 characters", ErrorKind.InvalidInput));

        return Result<Unit, Error>.Ok(Unit.Value);
    }

    private async Task<Result<Unit, Error>> ValidateEmailAsync(
        string email, 
        CancellationToken cancellationToken)
    {
        if (!email.Contains("@"))
            return Result<Unit, Error>.Err(
                Error.New("Invalid email format", ErrorKind.InvalidInput));

        var exists = await CheckEmailExistsAsync(email, cancellationToken);
        if (exists)
            return Result<Unit, Error>.Err(
                Error.New("Email already registered", ErrorKind.AlreadyExists));

        return Result<Unit, Error>.Ok(Unit.Value);
    }

    private async Task<Result<User, Error>> SaveUserAsync(
        string username, 
        string email,
        CancellationToken cancellationToken)
    {
        return await ErrorExtensions.TryAsync(async () =>
        {
            var user = new User { Username = username, Email = email };
            await _database.SaveAsync(user, cancellationToken);
            return user;
        }, cancellationToken);
    }
}
```

### Error Handling in Controllers

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly UserService _userService;

    [HttpPost]
    public async Task<IActionResult> CreateUser(
        [FromBody] CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _userService.CreateUserAsync(
            request.Username, 
            request.Email,
            cancellationToken);

        return result.Match(
            success: user => Ok(new { userId = user.Id }),
            failure: error =>
            {
                // Log full error chain
                _logger.LogError(error.GetFullMessage());

                // Return appropriate HTTP status based on error kind
                return error.Kind switch
                {
                    ErrorKind.InvalidInput => BadRequest(new { message = error.Message }),
                    ErrorKind.AlreadyExists => Conflict(new { message = error.Message }),
                    ErrorKind.NotFound => NotFound(new { message = error.Message }),
                    ErrorKind.PermissionDenied => Unauthorized(),
                    _ => StatusCode(500, new { message = "An internal error occurred" })
                };
            }
        );
    }
}
```

## Full Error Chain Display

The `GetFullMessage()` method displays the complete error chain with context and metadata:

```csharp
var error = Error.New("config.json not found", ErrorKind.NotFound)
    .WithContext("Failed to read configuration file")
    .WithMetadata("path", "/etc/myapp/config.json")
    .WithContext("Application initialization failed")
    .WithMetadata("stage", "startup");

Console.WriteLine(error.GetFullMessage());

// Output:
// Other: Application initialization failed
//   [stage=startup]
// Caused by:
//   NotFound: Failed to read configuration file
//     [path=/etc/myapp/config.json]
//   Caused by:
//     NotFound: config.json not found
```

## Exception Conversion

The `Error.FromException()` method automatically maps common .NET exceptions to appropriate error kinds:

| Exception Type | Error Kind |
|----------------|------------|
| `ArgumentException`, `ArgumentNullException` | `InvalidInput` |
| `InvalidOperationException` | `InvalidOperation` |
| `NotSupportedException` | `NotSupported` |
| `UnauthorizedAccessException` | `PermissionDenied` |
| `TimeoutException` | `Timeout` |
| `OperationCanceledException` | `Interrupted` |
| `IOException` | `Io` |
| Other exceptions | `Other` |

Inner exceptions are automatically converted to source errors, creating a full error chain.

## Best Practices

1. **Add Context as Errors Propagate**: Each layer should add meaningful context
   ```csharp
   return LoadConfig(path)
       .Context("Failed to initialize application")
       .WithMetadata("configPath", path);
   ```

2. **Use Appropriate Error Kinds**: Choose the most specific error kind
   ```csharp
   return Result<User, Error>.Err(
       Error.New("User not found", ErrorKind.NotFound));
   ```

3. **Attach Metadata for Debugging**: Include relevant data without exposing sensitive information
   ```csharp
   .WithMetadata("userId", userId)
   .WithMetadata("operation", "delete")
   .WithMetadata("timestamp", DateTime.UtcNow)
   ```

4. **Use Try/TryAsync for Exception Boundaries**: Wrap operations that might throw
   ```csharp
   var result = ErrorExtensions.Try(() => File.ReadAllText(path))
       .Context("Failed to read file");
   ```

5. **Log Full Error Chains**: Use `GetFullMessage()` for comprehensive logging
   ```csharp
   if (result.TryGetError(out var error))
   {
       _logger.LogError(error.GetFullMessage());
   }
   ```

## Comparison with Rust

| Rust Pattern | C# Equivalent |
|--------------|---------------|
| `Result<T, E>` | `Result<T, Error>` |
| `anyhow::Error` | `Error` |
| `.context("msg")` | `.Context("msg")` |
| `?` operator | `.Bind()` or LINQ query syntax |
| `Error::new()` | `Error.New()` |
| `Error::from()` | `Error.FromException()` |
| Error kinds | `ErrorKind` enum |

## See Also

- [Result Type Documentation](Result.md)
- [Result Extensions](ResultExtensions.md)
- [Async Error Handling](AsyncErrorHandling.md)
