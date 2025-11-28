# Advanced Features Guide

This document covers the advanced features added to `Esox.SharpAndRusty` for production-ready error handling.

## Table of Contents

- [Validation Patterns](#validation-patterns-no-where-clause-support)
- [Error Type Transformation](#error-type-transformation-with-maperror)
- [Better Error Messages](#better-error-messages-with-expect)
- [Unified Side Effects](#unified-side-effects-with-tap)
- [Value Checking](#value-checking-with-contains)
- [Collection Operations](#collection-operations)
- [Async/Await Support](#asyncawait-support)

---

## Error Type Transformation with MapError

Transform error types while preserving success values:

```csharp
// Convert string errors to error codes
Result<User, string> userResult = GetUser(id);
Result<User, ErrorCode> converted = userResult.MapError(msg => 
{
    return msg.Contains("404") ? ErrorCode.NotFound : ErrorCode.ServerError;
});

// Chain error transformations
var result = GetUserData(id)
    .MapError(int.Parse)  // string -> int
    .MapError(code => $"Error code: {code}")  // int -> string
    .OrElse(error => FallbackOperation());
```

### Use Cases
- Converting between different error representations
- Integrating with different error handling strategies
- Maintaining type safety across API boundaries

---

## Better Error Messages with Expect

Unwrap values with contextual error messages:

```csharp
// Instead of generic Unwrap
var user = userResult.Expect("User must exist for dashboard access");
// Throws: "User must exist for dashboard access: User 123 not found"

// Use in chains for better debugging
var email = GetUser(id)
    .Expect("User lookup failed")
    .Email;

// Perfect for required values
var config = LoadConfig()
    .Expect("Application configuration is required");
```

### Benefits
- More informative error messages
- Clear intent in code
- Better debugging experience

---

## Unified Side Effects with Tap

Execute actions on both success and failure:

```csharp
var result = GetUser(userId)
    .Tap(
        onSuccess: user => Logger.Info($"Found user: {user.Name}"),
        onFailure: error => Logger.Error($"User lookup failed: {error}")
    )
    .Map(user => user.Email);

// Combine with async operations
await ProcessOrder(orderId)
    .Tap(
        onSuccess: order => Metrics.RecordSuccess(),
        onFailure: error => Metrics.RecordFailure(error)
    );
```

### Benefits
- Cleaner than separate `Inspect`/`InspectErr` calls
- Consistent API for side effects
- Easier to reason about logging/monitoring

---

## Value Checking with Contains

Check if a result contains a specific value:

```csharp
var result = GetUserAge(userId);
if (result.Contains(18))
{
    Console.WriteLine("User is exactly 18 years old");
}

// Works with reference types
var nameResult = GetUserName(userId);
if (nameResult.Contains("John"))
{
    Console.WriteLine("Found John!");
}

// Handles nulls correctly
var nullableResult = Result<string?, int>.Ok(null);
bool containsNull = nullableResult.Contains(null); // true
```

---

## Collection Operations

### Combine - Aggregate Multiple Results

Combine multiple results into one, stopping at the first error:

```csharp
var userIds = new[] { 1, 2, 3, 4, 5 };
var results = userIds.Select(id => GetUser(id));

// All succeed -> Ok([User1, User2, User3, User4, User5])
// Any fail -> Err("User 3 not found")
Result<IEnumerable<User>, string> combined = results.Combine();

combined.Match(
    success: users => Console.WriteLine($"Loaded {users.Count()} users"),
    failure: error => Console.WriteLine($"Failed: {error}")
);

// Empty collection is success
var empty = Array.Empty<Result<int, string>>();
var emptyResult = empty.Combine(); // Ok([])
```

### Partition - Separate Successes and Failures

Split results into successful and failed operations:

```csharp
var results = userIds.Select(id => GetUser(id));
var (successes, failures) = results.Partition();

Console.WriteLine($"Successfully loaded {successes.Count} users");
Console.WriteLine($"Failed to load {failures.Count} users");

// Process successes
foreach (var user in successes)
{
    ProcessUser(user);
}

// Log failures
foreach (var error in failures)
{
    Logger.Error($"User load failed: {error}")
}

// Generate report
var report = new LoadReport
{
    SuccessCount = successes.Count,
    FailureCount = failures.Count,
    Errors = failures
};
```

### Use Cases for Collection Operations
- Batch processing of multiple items
- Reporting on success/failure rates
- Partial success handling
- ETL operations
- API batch requests

---

## Async/Await Support

Complete async integration for modern C# applications:

### MapAsync - Transform Async Results

```csharp
// Await result, then map
var email = await GetUserAsync(userId)
    .MapAsync(user => user.Email);

// Map with async function
var details = await GetUserAsync(userId)
    .MapAsync(async user => await LoadUserDetailsAsync(user));

// Chain multiple async maps
var result = await GetUserAsync(userId)
    .MapAsync(user => user.Profile)
    .MapAsync(async profile => await EnrichProfileAsync(profile));
```

### BindAsync - Chain Async Operations

```csharp
// Chain async operations
var result = await GetUserAsync(userId)
    .BindAsync(async user => await ValidateUserAsync(user))
    .BindAsync(async user => await SaveUserAsync(user));

// Mix sync and async
var result = await GetUserAsync(userId)
    .MapAsync(user => user.Email)  // Sync
    .BindAsync(async email => await SendEmailAsync(email));  // Async

// Complex async chain
var orderResult = await GetOrderAsync(orderId)
    .BindAsync(async order => await ProcessPaymentAsync(order))
    .MapAsync(payment => payment.TransactionId)
    .BindAsync(async txnId => await CreateReceiptAsync(txnId));
```

### TapAsync - Async Side Effects

```csharp
var result = await GetUserAsync(userId)
    .TapAsync(
        onSuccess: async user => await LogSuccessAsync(user),
        onFailure: async error => await LogErrorAsync(error)
    );

// Use for async logging/monitoring
await ProcessDataAsync(data)
    .TapAsync(
        onSuccess: async result => await Metrics.RecordSuccessAsync(),
        onFailure: async error => await Metrics.RecordFailureAsync(error)
    );
```

### OrElseAsync - Async Fallback

```csharp
// Try cache, fallback to database
var user = await GetUserFromCacheAsync(userId)
    .OrElseAsync(async error =>
    {
        Logger.Info($"Cache miss: {error}. Loading from database...");
        return await GetUserFromDatabaseAsync(userId);
    });

// Multiple fallbacks
var data = await GetFromPrimaryAsync()
    .OrElseAsync(async _ => await GetFromSecondaryAsync())
    .OrElseAsync(async _ => await GetFromBackupAsync());
```

### CombineAsync - Concurrent Operations

```csharp
// Process multiple users concurrently
var userTasks = userIds.Select(id => GetUserAsync(id));
Result<IEnumerable<User>, string> allUsers = await userTasks.CombineAsync();

// All async operations run concurrently
allUsers.Match(
    success: users => Console.WriteLine($"Loaded {users.Count()} users"),
    failure: error => Console.WriteLine($"Failed: {error}")
);

// Real-world example: Load related data
var relatedDataTasks = new[]
{
    GetUserProfileAsync(userId),
    GetUserOrdersAsync(userId),
    GetUserPreferencesAsync(userId)
};

var combinedData = await relatedDataTasks.CombineAsync();
```

### MapErrorAsync - Async Error Transformation

```csharp
var result = await GetUserAsync(userId)
    .MapErrorAsync(async error =>
    {
        await LogErrorAsync(error);
        return new UserNotFoundException(error);
    });
```

---

## Validation Patterns (No `where` Clause Support)

The `where` clause is **intentionally not supported** for Result types because:

1. **Predicates return bool, not errors**: LINQ `where` filters based on boolean conditions, but Result types need meaningful error messages
2. **Explicit is better**: Using `Bind` with validation functions makes error handling explicit and testable
3. **Type safety**: Validation functions can enforce type constraints and provide rich error information

### Recommended Pattern: Validation Functions

Instead of `where`, create explicit validation functions:

```csharp
// Pattern 1: Inline validation with Bind
var result = GetValue()
    .Bind(x => x > 0
        ? Result<int, string>.Ok(x)
        : Result<int, string>.Err($"Value {x} must be positive"));

// Pattern 2: Reusable validation functions
Result<int, string> ValidatePositive(int value) =>
    value > 0
        ? Result<int, string>.Ok(value)
        : Result<int, string>.Err($"Value {value} must be positive");

Result<int, string> ValidateRange(int value, int min, int max) =>
    value >= min && value <= max
        ? Result<int, string>.Ok(value)
        : Result<int, string>.Err($"Value {value} must be between {min} and {max}");

// Usage in LINQ queries
var result = from x in ParseInt("42")
             from validated in ValidatePositive(x)
             from inRange in ValidateRange(validated, 1, 100)
             select inRange * 2;

// Pattern 3: Extension methods for validation
public static class ResultValidators
{
    public static Result<int, string> MustBePositive(this Result<int, string> result)
    {
        return result.Bind(value => value > 0
            ? Result<int, string>.Ok(value)
            : Result<int, string>.Err($"Value {value} must be positive"));
    }
    
    public static Result<int, string> MustBeInRange(
        this Result<int, string> result, 
        int min, 
        int max)
    {
        return result.Bind(value => value >= min && value <= max
            ? Result<int, string>.Ok(value)
            : Result<int, string>.Err($"Value {value} must be between {min} and {max}"));
    }
}

// Clean usage
var result = ParseInt("42")
    .MustBePositive()
    .MustBeInRange(1, 100)
    .Map(x => x * 2);
```

### Benefits of Explicit Validation

- ? **Clear error messages**: Every validation provides specific feedback
- ? **Testable**: Validation functions can be unit tested independently
- ? **Reusable**: Create a library of validation functions
- ? **Type-safe**: Compiler enforces proper error handling
- ? **Composable**: Chain validations naturally with Bind
- ? **Self-documenting**: Code clearly shows what validations are performed
