# Option<T> Quick Reference Guide

## When to Use Each Method

### 🔍 **Checking State**
```csharp
option.IsSome()      // Has a value?
option.IsNone()      // No value?
```

### 📦 **Getting Values**
```csharp
option.GetValueOrDefault(42)           // Provide default immediately
option.GetValueOrElse(() => Compute()) // Lazy default computation
option.ToNullable()                    // Convert to T? (value types only)
```

### 🔄 **Transforming**
```csharp
option.Map(x => x * 2)                 // Transform value, keep in Option
option.Bind(x => GetOption(x))         // Chain operations returning Options
option.Filter(x => x > 10)             // Keep only if predicate passes
```

### 🔗 **Combining**
```csharp
option1.Zip(option2)                   // Combine into tuple: Option<(T, U)>
option1.ZipWith(option2, (a,b) => a+b) // Combine with function
nested.Flatten()                       // Option<Option<T>> → Option<T>
```

### ⚡ **Boolean Logic**
```csharp
option1.And(option2)                   // Both must be Some → returns option2
option1.Or(option2)                    // First Some wins → fallback chain
option1.Xor(option2)                   // Exactly one must be Some
```

### 🔍 **Debugging/Logging**
```csharp
option.Inspect(x => Log(x))            // Log if Some, pass through
option.InspectNone(() => Log("None"))  // Log if None, pass through
```

### 🔀 **Converting**
```csharp
option.OkOr("error")                   // Option<T> → Result<T, E>
option.OkOrElse(() => Error.New())     // Lazy error creation
```

### 🎯 **Pattern Matching**
```csharp
option.Match(
    onSome: value => DoSomething(value),
    onNone: () => DoNothing()
)
```

---

## Common Patterns

### ✅ **Validation Chain**
```csharp
GetInput()
    .Filter(x => x > 0)
    .Filter(x => x < 100)
    .Bind(ValidateRange)
    .OkOr("Invalid input");
```

### ✅ **Fallback Chain**
```csharp
GetFromCache()
    .Or(GetFromDatabase())
    .Or(GetFromRemote())
    .GetValueOrElse(() => DefaultValue);
```

### ✅ **Data Aggregation**
```csharp
firstName.Zip(lastName)
    .Map(names => $"{names.Item1} {names.Item2}");
```

### ✅ **Debug Pipeline**
```csharp
GetValue()
    .Inspect(v => Debug.WriteLine($"Got: {v}"))
    .Filter(v => v > 10)
    .Inspect(v => Debug.WriteLine($"Passed: {v}"))
    .InspectNone(() => Debug.WriteLine("Filtered out"));
```

### ✅ **Conditional Execution**
```csharp
GetUser()
    .And(GetPermissions())  // Both must exist
    .Map(perms => CheckAccess(perms));
```

---

## Decision Tree

```
Do you have an optional value?
│
├─ Need to check if present?
│  └─ Use: IsSome() / IsNone()
│
├─ Need to extract value?
│  ├─ With immediate default?
│  │  └─ Use: GetValueOrDefault(value)
│  ├─ With computed default?
│  │  └─ Use: GetValueOrElse(() => compute)
│  └─ As nullable?
│     └─ Use: ToNullable()
│
├─ Need to transform value?
│  ├─ Simple transformation?
│  │  └─ Use: Map(x => transform(x))
│  ├─ Returns another Option?
│  │  └─ Use: Bind(x => getOption(x))
│  └─ Conditional keep?
│     └─ Use: Filter(x => predicate(x))
│
├─ Need to combine with another Option?
│  ├─ Both required?
│  │  └─ Use: Zip() or ZipWith()
│  ├─ Either required?
│  │  └─ Use: Or()
│  ├─ Sequential dependency?
│  │  └─ Use: And()
│  └─ Exactly one required?
│     └─ Use: Xor()
│
├─ Need to convert to Result?
│  ├─ Static error?
│  │  └─ Use: OkOr(error)
│  └─ Dynamic error?
│     └─ Use: OkOrElse(() => error)
│
└─ Need to log/debug?
   ├─ When present?
   │  └─ Use: Inspect(x => log(x))
   └─ When absent?
      └─ Use: InspectNone(() => log())
```

---

## Anti-Patterns ❌

### ❌ **Don't use TryGetValue pattern**
```csharp
// Bad
if (option.IsSome())
{
    var value = ((Option<int>.Some)option).Value;
    Process(value);
}

// Good
option.Match(
    onSome: value => Process(value),
    onNone: () => { }
);
```

### ❌ **Don't chain multiple GetValueOrDefault**
```csharp
// Bad
var value = option1.GetValueOrDefault(
    option2.GetValueOrDefault(
        option3.GetValueOrDefault(0)));

// Good
var value = option1
    .Or(option2)
    .Or(option3)
    .GetValueOrDefault(0);
```

### ❌ **Don't ignore Filter results**
```csharp
// Bad - loses the filtered state
option.Filter(x => x > 10);
// ... continue with original option

// Good - use the filtered result
var filtered = option.Filter(x => x > 10);
// ... use filtered
```

### ❌ **Don't use Map when you need Bind**
```csharp
// Bad - creates Option<Option<T>>
var nested = option.Map(x => GetAnotherOption(x));

// Good - flattens automatically
var flat = option.Bind(x => GetAnotherOption(x));
```

---

## Cheat Sheet

| I want to...                          | Use this method                  |
|---------------------------------------|----------------------------------|
| Check if value exists                 | `IsSome()` / `IsNone()`         |
| Get value with default                | `GetValueOrDefault()`           |
| Get value with lazy default           | `GetValueOrElse()`              |
| Transform value                       | `Map()`                         |
| Chain optional operations             | `Bind()`                        |
| Keep only if condition met            | `Filter()`                      |
| Combine two options                   | `Zip()` / `ZipWith()`           |
| Flatten nested option                 | `Flatten()`                     |
| Use first available                   | `Or()`                          |
| Require both                          | `Zip()` or `And()`              |
| Require exactly one                   | `Xor()`                         |
| Log when present                      | `Inspect()`                     |
| Log when absent                       | `InspectNone()`                 |
| Convert to Result                     | `OkOr()` / `OkOrElse()`         |
| Convert to nullable                   | `ToNullable()`                  |
| Execute different code paths          | `Match()`                       |

---

## Performance Tips

### ⚡ **Lazy Evaluation**
```csharp
// Prefer lazy evaluation for expensive operations
option.GetValueOrElse(() => ExpensiveComputation())  // ✅ Only if None
option.GetValueOrDefault(ExpensiveComputation())     // ❌ Always called
```

### ⚡ **Early Returns**
```csharp
// Filter short-circuits the chain
option
    .Filter(x => x > 10)        // If false, rest doesn't execute
    .Map(x => ExpensiveOperation(x));
```

### ⚡ **Avoid Unnecessary Allocations**
```csharp
// Reuse options when possible
static readonly Option<int> NoneInt = new Option<int>.None();

// Instead of creating new None repeatedly
return condition ? new Option<int>.Some(value) : NoneInt;
```

---

## Testing Patterns

### ✅ **Test Both Branches**
```csharp
[Fact]
public void Method_WithSome_ReturnsExpectedResult()
{
    var option = new Option<int>.Some(42);
    var result = option.Map(x => x * 2);
    Assert.True(result.IsSome());
}

[Fact]
public void Method_WithNone_PropagatesNone()
{
    var option = new Option<int>.None();
    var result = option.Map(x => x * 2);
    Assert.True(result.IsNone());
}
```

### ✅ **Test Side Effects**
```csharp
[Fact]
public void Inspect_WithSome_CallsAction()
{
    var called = false;
    new Option<int>.Some(42).Inspect(_ => called = true);
    Assert.True(called);
}
```

---

## Real-World Examples

### Example: User Lookup with Caching
```csharp
public Option<User> GetUser(int id)
{
    return GetFromCache(id)
        .Inspect(u => Metrics.RecordCacheHit())
        .InspectNone(() => Metrics.RecordCacheMiss())
        .Or(GetFromDatabase(id))
        .Inspect(u => AddToCache(id, u))
        .InspectNone(() => Logger.Warn($"User {id} not found"));
}
```

### Example: Form Validation
```csharp
public Result<Form, ValidationError> ValidateForm(FormData data)
{
    var name = ValidateName(data.Name);
    var email = ValidateEmail(data.Email);
    var age = ValidateAge(data.Age);
    
    return name
        .Zip(email)
        .ZipWith(age, (nameEmail, validAge) => new Form
        {
            Name = nameEmail.Item1,
            Email = nameEmail.Item2,
            Age = validAge
        })
        .OkOrElse(() => new ValidationError("Invalid form data"));
}
```

### Example: Configuration Loading
```csharp
public Option<Config> LoadConfig()
{
    return ReadEnvironmentVariables()
        .Or(ReadConfigFile())
        .Or(ReadRemoteConfig())
        .Filter(IsValidVersion)
        .Inspect(cfg => Logger.Info($"Loaded config v{cfg.Version}"))
        .InspectNone(() => Logger.Warn("No config found, using defaults"));
}
```

---

## See Also
- [Option<T> Implementation Summary](OPTION_EXTENSIONS_SUMMARY.md)
- [Result<T, E> Documentation](README.md)
- [Test Examples](Esox.SharpAndRust.Tests/Extensions/OptionExtensionsAdvancedTests.cs)
