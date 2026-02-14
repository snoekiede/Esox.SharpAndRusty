# Collection Extensions Quick Reference

## Decision Tree

```
Do you have a collection of Options or Results?
│
├─ Want ALL to succeed?
│  ├─ Already have collection of Option/Result?
│  │  └─ Use: Sequence()
│  └─ Need to map first?
│     └─ Use: Traverse()
│
├─ Want to collect ONLY successes?
│  ├─ Option<T>?
│  │  └─ Use: CollectSome()
│  └─ Result<T, E>?
│     └─ Use: CollectOk()
│
├─ Want to collect ONLY failures?
│  └─ Use: CollectErr()  (Result only)
│
└─ Want BOTH successes AND failures?
   ├─ Option<T>?
   │  └─ Use: PartitionOptions()
   └─ Result<T, E>?
      └─ Use: PartitionResults()
```

---

## Method Overview

### Option<T> Extensions

| Method | Input | Output | Behavior |
|--------|-------|--------|----------|
| `Sequence()` | `IEnumerable<Option<T>>` | `Option<IEnumerable<T>>` | All Some → Some, any None → None |
| `Traverse()` | `IEnumerable<T>` + `Func<T, Option<U>>` | `Option<IEnumerable<U>>` | Map then sequence |
| `CollectSome()` | `IEnumerable<Option<T>>` | `IEnumerable<T>` | Extract all Some values |
| `PartitionOptions()` | `IEnumerable<Option<T>>` | `(List<T>, int)` | Split Some values and None count |

### Result<T, E> Extensions

| Method | Input | Output | Behavior |
|--------|-------|--------|----------|
| `Sequence()` | `IEnumerable<Result<T, E>>` | `Result<IEnumerable<T>, E>` | All Ok → Ok, any Err → first Err |
| `Traverse()` | `IEnumerable<T>` + `Func<T, Result<U, E>>` | `Result<IEnumerable<U>, E>` | Map then sequence |
| `CollectOk()` | `IEnumerable<Result<T, E>>` | `IEnumerable<T>` | Extract all Ok values |
| `CollectErr()` | `IEnumerable<Result<T, E>>` | `IEnumerable<E>` | Extract all Err values |
| `PartitionResults()` | `IEnumerable<Result<T, E>>` | `(List<T>, List<E>)` | Split Ok and Err values |

---

## Common Patterns

### ✅ **All-or-Nothing Processing**
```csharp
// Option: All users must exist
var allUsers = userIds
    .Traverse(id => GetUser(id)); // Option<IEnumerable<User>>

// Result: All must parse
var allNumbers = strings
    .Traverse<string, int, string>(s => ParseInt(s)); // Result<IEnumerable<int>, string>
```

### ✅ **Best-Effort Collection**
```csharp
// Option: Get what you can
var availableUsers = userIds
    .Select(GetUser)
    .CollectSome(); // IEnumerable<User>

// Result: Collect successes
var validData = inputs
    .Select(ProcessInput)
    .CollectOk(); // IEnumerable<ProcessedData>
```

### ✅ **Comprehensive Reporting**
```csharp
// Option: Report successes and failures
var (found, missing) = userIds
    .Select(GetUser)
    .PartitionOptions(); // (List<User>, int)

Console.WriteLine($"Found: {found.Count}, Missing: {missing}");

// Result: Detailed error reporting
var (successes, errors) = operations
    .PartitionResults(); // (List<T>, List<E>)

LogErrors(errors);
ProcessSuccesses(successes);
```

### ✅ **Validation Chains**
```csharp
// Parse → Validate → Transform
var result = inputs
    .Traverse<string, int, Error>(ParseInt)
    .Bind<IEnumerable<int>, IEnumerable<int>, Error>(values => 
        ValidateRange(values))
    .Map<IEnumerable<int>, Error, IEnumerable<string>>(values => 
        values.Select(v => $"Value: {v}"));
```

---

## When to Use Each

### Use **Sequence** when:
- ✅ You already have a collection of Option/Result
- ✅ You want all-or-nothing behavior
- ✅ You need to know if ANY failed

```csharp
var results = new[]
{
    Result<int, string>.Ok(1),
    Result<int, string>.Ok(2),
    Result<int, string>.Ok(3)
};
var combined = results.Sequence(); // Ok([1, 2, 3])
```

### Use **Traverse** when:
- ✅ You have a regular collection
- ✅ You need to map AND collect
- ✅ You want all-or-nothing behavior
- ✅ You want to avoid creating intermediate collection

```csharp
// Instead of this:
var options = strings.Select(ParseInt); // IEnumerable<Option<int>>
var result = options.Sequence();

// Do this:
var result = strings.Traverse(ParseInt); // More efficient
```

### Use **CollectSome/CollectOk** when:
- ✅ You want partial success
- ✅ Failures are acceptable
- ✅ You need "best effort" processing
- ✅ You don't care about errors

```csharp
var validUsers = userIds
    .Select(GetUser)
    .CollectSome(); // Gets what's available
```

### Use **CollectErr** when:
- ✅ You need to log/report all errors
- ✅ Successes don't matter
- ✅ You're aggregating failures

```csharp
var allErrors = operations
    .CollectErr()
    .ToList();
LogBatch(allErrors);
```

### Use **Partition** when:
- ✅ You need both successes AND failures
- ✅ You're generating reports
- ✅ You want detailed statistics
- ✅ You need to handle both cases

```csharp
var (successes, failures) = operations.PartitionResults();

return new Report
{
    Succeeded = successes.Count,
    Failed = failures.Count,
    Errors = failures,
    Data = successes
};
```

---

## Performance Tips

### ⚡ **Use Traverse instead of Select + Sequence**
```csharp
// ❌ Less efficient - creates intermediate collection
var result = items
    .Select(ProcessItem)
    .Sequence();

// ✅ More efficient - single pass
var result = items.Traverse(ProcessItem);
```

### ⚡ **Use CollectSome for partial results**
```csharp
// ❌ Might fail if any missing
var allOrNothing = ids.Traverse(GetUser);

// ✅ Gets what's available
var available = ids
    .Select(GetUser)
    .CollectSome();
```

### ⚡ **Short-circuit behavior**
```csharp
// Sequence and Traverse stop at first failure
var result = largeList.Traverse(ExpensiveOperation);
// Stops immediately on first None/Err
```

---

## Anti-Patterns ❌

### ❌ **Don't use Sequence when you need partial results**
```csharp
// Bad - fails if ANY user missing
var allUsers = userIds
    .Select(GetUser)
    .Sequence();
if (allUsers.IsNone())
    return EmptyList; // Lost all users!

// Good - collect what you can
var availableUsers = userIds
    .Select(GetUser)
    .CollectSome()
    .ToList(); // Get partial results
```

### ❌ **Don't partition then ignore half**
```csharp
// Bad - wasteful
var (successes, _) = results.PartitionResults();
ProcessSuccesses(successes);

// Good - more efficient
var successes = results.CollectOk();
ProcessSuccesses(successes);
```

### ❌ **Don't create intermediate collections unnecessarily**
```csharp
// Bad - two passes, intermediate collection
var options = inputs.Select(ParseInt).ToList();
var result = options.Sequence();

// Good - single pass
var result = inputs.Traverse(ParseInt);
```

---

## Cheat Sheet

| I want to... | Use this |
|--------------|----------|
| Get all values or fail | `Sequence()` or `Traverse()` |
| Get values, ignore failures | `CollectSome()` or `CollectOk()` |
| Get only errors | `CollectErr()` |
| Get successes AND failures | `Partition()` |
| Map and collect in one pass | `Traverse()` |
| Stop at first error | `Sequence()` or `Traverse()` |
| Process entire collection | `Collect*()` or `Partition*()` |
| Generate report | `Partition*()` |
| Best-effort processing | `Collect*()` |
| All-or-nothing | `Sequence()` or `Traverse()` |

---

## Examples by Scenario

### Scenario: User Batch Lookup

```csharp
// Fail if ANY missing
Option<IEnumerable<User>> GetAllUsers(int[] ids) =>
    ids.Traverse(GetUser);

// Get available users
IEnumerable<User> GetAvailableUsers(int[] ids) =>
    ids.Select(GetUser).CollectSome();

// Report on lookup
(List<User> found, int missing) LookupUsers(int[] ids) =>
    ids.Select(GetUser).PartitionOptions();
```

### Scenario: Form Validation

```csharp
// Stop at first error
Result<Form, Error> ValidateStopFast(FormData data) =>
    new[] 
    {
        ValidateName(data.Name),
        ValidateEmail(data.Email),
        ValidateAge(data.Age)
    }
    .Sequence()
    .Map(CreateForm);

// Collect all errors
Result<Form, List<Error>> ValidateAll(FormData data)
{
    var results = new[]
    {
        ValidateName(data.Name),
        ValidateEmail(data.Email),
        ValidateAge(data.Age)
    };
    
    var (_, errors) = results.PartitionResults();
    return errors.Any()
        ? Result<Form, List<Error>>.Err(errors)
        : Result<Form, List<Error>>.Ok(CreateForm(data));
}
```

### Scenario: File Processing

```csharp
// Process all files, report results
ProcessingReport ProcessFiles(string[] paths)
{
    var results = paths
        .Select(ProcessFile); // IEnumerable<Result<Data, Error>>
    
    var (successes, failures) = results.PartitionResults();
    
    return new ProcessingReport
    {
        Successful = successes,
        Failed = failures,
        TotalProcessed = successes.Count,
        TotalFailed = failures.Count
    };
}

// Process files, fail if any fails
Result<ProcessedData, Error> ProcessAllFiles(string[] paths) =>
    paths
        .Traverse<string, FileData, Error>(ProcessFile)
        .Map(files => AggregateData(files));
```

### Scenario: Data Pipeline

```csharp
// Parse CSV with early exit
Result<IEnumerable<Customer>, Error> LoadCustomers(string[] lines) =>
    lines
        .Skip(1) // Header
        .Traverse<string, Customer, Error>(ParseLine);

// Parse CSV with error collection
ParseReport LoadCustomersWithReport(string[] lines)
{
    var results = lines
        .Skip(1)
        .Select(ParseLine);
    
    var (customers, errors) = results.PartitionResults();
    
    return new ParseReport
    {
        Customers = customers,
        Errors = errors,
        SuccessRate = (double)customers.Count / lines.Length
    };
}
```

---

## Integration with LINQ

### Works with Standard LINQ
```csharp
var result = items
    .Where(ShouldProcess)
    .OrderBy(GetPriority)
    .Traverse(ProcessItem)
    .Map(results => results.Sum());
```

### Works with Query Syntax (where applicable)
```csharp
var userIds = from dept in departments
              from user in dept.UserIds
              select user;

var users = userIds.Traverse(GetUser);
```

---

## See Also
- [Collection Extensions Summary](COLLECTION_EXTENSIONS_SUMMARY.md)
- [Option Quick Reference](OPTION_QUICK_REFERENCE.md)
- [Test Examples](Esox.SharpAndRust.Tests/Extensions/CollectionExtensionsTests.cs)
