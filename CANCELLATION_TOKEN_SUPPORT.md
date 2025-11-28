# CancellationToken Support for Async Result Extensions

## Summary

Added comprehensive `CancellationToken` support to all async extension methods in the `ResultAsyncExtensions` class. This enables proper cancellation of long-running async operations when working with Result types.

## Changes Made

### 1. Updated Async Extension Methods

All async extension methods in `ResultAsyncExtensions.cs` now include an optional `CancellationToken` parameter:

- **MapAsync** (both overloads)
- **BindAsync** (all overloads)
- **MapErrorAsync**
- **TapAsync**
- **OrElseAsync**
- **CombineAsync**

### 2. Cancellation Points

Cancellation tokens are checked at strategic points:
- Before executing async operations
- After awaiting tasks
- After executing mapper/binder functions
- After executing side-effect actions (Tap)

### 3. Behavior

- All `CancellationToken` parameters are **optional** with `default` values to maintain backward compatibility
- When cancelled, methods throw `OperationCanceledException` or `TaskCanceledException` (see Exception Behavior section)
- Existing code without cancellation tokens continues to work without any changes

## Exception Behavior: TaskCanceledException vs OperationCanceledException

Understanding which exception type is thrown is important for proper exception handling in your applications.

### TaskCanceledException

**When it's thrown:**
- When a `Task` operation itself is cancelled (e.g., `Task.Run`, `Task.Delay`)
- When awaiting a task that was created with a cancellation token and that token is cancelled
- When the underlying async operation throws `TaskCanceledException`

**Examples:**

```csharp
var cts = new CancellationTokenSource();
var resultTask = Task.Run(async () =>
{
    await Task.Delay(100, cts.Token); // This can throw TaskCanceledException
    return Result<int, string>.Ok(42);
}, cts.Token);

cts.Cancel();

// Throws TaskCanceledException because the Task.Run operation was cancelled
await resultTask.MapAsync(x => x * 2, cts.Token);
```

**Affected methods when task infrastructure is cancelled:**
- `MapAsync` (when awaiting `resultTask`)
- `BindAsync` (when awaiting `resultTask`)
- `MapErrorAsync` (when awaiting `resultTask`)
- `TapAsync` (when awaiting `resultTask`)
- `OrElseAsync` (when awaiting `resultTask`)
- `CombineAsync` (when awaiting multiple `resultTasks`)

### OperationCanceledException

**When it's thrown:**
- When `cancellationToken.ThrowIfCancellationRequested()` is called in our extension methods
- When cancellation is detected between operations (not during a Task operation)
- When the user's async mapper/binder/action throws `OperationCanceledException`

**Examples:**

```csharp
var cts = new CancellationTokenSource();
var result = Result<int, string>.Ok(42);

cts.Cancel(); // Cancel before starting

// Throws OperationCanceledException because cancellation is checked 
// before the async mapper runs
await result.MapAsync(async x =>
{
    await Task.Delay(100);
    return x * 2;
}, cts.Token);
```

```csharp
var cts = new CancellationTokenSource();
var result = Result<int, string>.Ok(42);

// Throws OperationCanceledException because cancellation is detected
// after the mapper completes
await result.MapAsync(async x =>
{
    await Task.Delay(10);
    cts.Cancel(); // Cancel after work completes
    return x * 2;
}, cts.Token);
```

**Affected scenarios:**
- Cancellation token already cancelled before method is called
- Cancellation detected after async operation completes but before returning
- Side-effect actions (Tap) cancel during execution

### Inheritance Relationship

```
Exception
  ?? SystemException
      ?? OperationCanceledException
          ?? TaskCanceledException
```

`TaskCanceledException` **inherits from** `OperationCanceledException`, so:

```csharp
// ? This catches both types
try
{
    await resultTask.MapAsync(x => x * 2, cts.Token);
}
catch (OperationCanceledException ex)
{
    // Handles both TaskCanceledException and OperationCanceledException
    Console.WriteLine("Operation was cancelled");
}

// ?? This only catches TaskCanceledException, not OperationCanceledException
try
{
    await resultTask.MapAsync(x => x * 2, cts.Token);
}
catch (TaskCanceledException ex)
{
    // Only handles TaskCanceledException
    // OperationCanceledException will not be caught here
    Console.WriteLine("Task was cancelled");
}
```

### Best Practice Recommendations

**For Application Code:**

```csharp
// ? Recommended: Catch the base type to handle all cancellation scenarios
try
{
    var result = await GetUserAsync(userId, cancellationToken)
        .MapAsync(user => user.Email, cancellationToken)
        .BindAsync(email => ValidateEmailAsync(email, cancellationToken), cancellationToken);
}
catch (OperationCanceledException)
{
    // User cancelled the operation - this is expected behavior
    _logger.LogInformation("User operation was cancelled");
    return Result<string, string>.Err("Operation cancelled");
}
catch (Exception ex)
{
    // Unexpected error
    _logger.LogError(ex, "Unexpected error during user operation");
    return Result<string, string>.Err($"Unexpected error: {ex.Message}");
}
```

**For Testing:**

```csharp
// Use the specific exception type based on what you're testing

// When testing Task infrastructure cancellation:
await Assert.ThrowsAsync<TaskCanceledException>(async () =>
    await Task.Run(() => Result<int, string>.Ok(42), cts.Token)
        .MapAsync(x => x * 2, cts.Token));

// When testing our extension method cancellation checks:
await Assert.ThrowsAsync<OperationCanceledException>(async () =>
{
    cts.Cancel();
    await Result<int, string>.Ok(42)
        .MapAsync(async x => await DoWorkAsync(x), cts.Token);
});
```

### Summary Table

| Scenario | Exception Type | Reason |
|----------|---------------|--------|
| Task.Run/Task.Delay cancelled | `TaskCanceledException` | Task infrastructure handles cancellation |
| Cancellation token already cancelled | `OperationCanceledException` | Our code calls `ThrowIfCancellationRequested()` |
| Cancellation detected after mapper/binder | `OperationCanceledException` | Our code calls `ThrowIfCancellationRequested()` |
| User's async operation throws cancellation | Depends on user code | Usually `OperationCanceledException` |
| Awaiting cancelled Task<Result<T,E>> | `TaskCanceledException` | Task was cancelled before completion |

## Usage Examples

### Basic Cancellation

```csharp
var cts = new CancellationTokenSource();
cts.CancelAfter(TimeSpan.FromSeconds(5));

try
{
    var result = await GetUserAsync(userId)
        .MapAsync(user => user.Email, cts.Token)
        .BindAsync(email => ValidateEmailAsync(email, cts.Token), cts.Token);
}
catch (OperationCanceledException)
{
    // Handle cancellation gracefully
    Console.WriteLine("Operation was cancelled");
}
```

### Complex Chains

```csharp
using var cts = new CancellationTokenSource();

try
{
    var result = await GetUserAsync(userId, cts.Token)
        .MapAsync(user => user.Profile, cts.Token)
        .BindAsync(
            async profile => await EnrichProfileAsync(profile, cts.Token), 
            cts.Token
        )
        .TapAsync(
            onSuccess: async profile => await LogSuccessAsync(profile, cts.Token),
            onFailure: async error => await LogErrorAsync(error, cts.Token),
            cts.Token
        );
}
catch (OperationCanceledException)
{
    // Cancellation is expected and handled gracefully
    _logger.LogInformation("User profile enrichment was cancelled");
}
```

### Combining Multiple Results

```csharp
var cts = new CancellationTokenSource();

try
{
    var userTasks = userIds.Select(id => GetUserAsync(id, cts.Token));
    var combined = await userTasks.CombineAsync(cts.Token);
    
    if (combined.IsSuccess)
    {
        combined.TryGetValue(out var users);
        Console.WriteLine($"Loaded {users.Count()} users");
    }
}
catch (OperationCanceledException)
{
    Console.WriteLine("Batch user load was cancelled");
}
```

### Timeout Pattern

```csharp
// Cancel after 5 seconds
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

try
{
    var result = await GetUserAsync(userId, cts.Token)
        .BindAsync(user => ProcessUserAsync(user, cts.Token), cts.Token);
}
catch (OperationCanceledException)
{
    Console.WriteLine("Operation timed out after 5 seconds");
    return Result<User, string>.Err("Operation timed out");
}
```

## Test Coverage

Added comprehensive tests in `ResultAsyncExtensionsTests.cs`:

- **Cancellation before awaiting**: Verifies cancellation is respected before async operations start
- **Cancellation after operations**: Verifies cancellation is checked after completing async work
- **Cancellation in different scenarios**: Success, failure, and alternative paths
- **Normal operation**: Confirms cancellation tokens don't affect normal execution when not cancelled
- **Exception type verification**: Tests verify correct exception types (TaskCanceledException vs OperationCanceledException)

### Test Results

All 157 tests pass, including:
- 20 new cancellation token tests
- All existing tests remain passing (backward compatibility confirmed)

## Backward Compatibility

? **100% Backward Compatible**

- All existing code continues to work without modification
- CancellationToken parameters are optional with default values
- No breaking changes to method signatures or behavior

## Benefits

1. **Graceful Cancellation**: Long-running async operations can be cancelled properly
2. **Resource Management**: Prevents unnecessary work and improves resource utilization
3. **User Experience**: Enables responsive UI by allowing operation cancellation
4. **Best Practices**: Aligns with .NET async/await patterns and conventions
5. **Production Ready**: Properly handles cancellation in complex async chains
6. **Clear Exception Semantics**: Well-defined behavior for different cancellation scenarios

## Documentation Updates

All XML documentation comments have been updated to include:
- `<param name="cancellationToken">` descriptions
- `<exception cref="OperationCanceledException">` documentation
- Updated code examples showing cancellation token usage
- Clear guidance on exception handling best practices
