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
- When cancelled, methods throw `OperationCanceledException`
- Existing code without cancellation tokens continues to work without any changes

## Usage Examples

### Basic Cancellation

```csharp
var cts = new CancellationTokenSource();
cts.CancelAfter(TimeSpan.FromSeconds(5));

var result = await GetUserAsync(userId)
    .MapAsync(user => user.Email, cts.Token)
    .BindAsync(email => ValidateEmailAsync(email, cts.Token), cts.Token);
```

### Complex Chains

```csharp
using var cts = new CancellationTokenSource();

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
```

### Combining Multiple Results

```csharp
var cts = new CancellationTokenSource();

var userTasks = userIds.Select(id => GetUserAsync(id, cts.Token));
var combined = await userTasks.CombineAsync(cts.Token);
```

## Test Coverage

Added comprehensive tests in `ResultAsyncExtensionsTests.cs`:

- **Cancellation before awaiting**: Verifies cancellation is respected before async operations start
- **Cancellation after operations**: Verifies cancellation is checked after completing async work
- **Cancellation in different scenarios**: Success, failure, and alternative paths
- **Normal operation**: Confirms cancellation tokens don't affect normal execution when not cancelled

### Test Results

All 137 tests pass, including:
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

## Documentation Updates

All XML documentation comments have been updated to include:
- `<param name="cancellationToken">` descriptions
- `<exception cref="OperationCanceledException">` documentation
- Updated code examples showing cancellation token usage
