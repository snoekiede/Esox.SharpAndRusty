# Esox.SharpAndRusty.Analyzers

A Roslyn analyzer that enforces proper handling of `Result<T, E>` and `Option<T>` types from the Esox.SharpAndRusty library, similar to Rust's `#[must_use]` attribute.

## Features

### ESOX1001: Result value must be used

This analyzer warns when a method returning `Result<T, E>` is called but its return value is ignored. This prevents developers from accidentally ignoring potential errors.

**❌ Bad:**
```csharp
public Result<int, string> GetValue() => Result<int, string>.Ok(42);

public void Process()
{
    GetValue(); // Warning ESOX1001
}
```

**✅ Good:**
```csharp
public void Process()
{
    var result = GetValue();
    
    // Or use it directly
    result.Match(
        ok => Console.WriteLine($"Success: {ok}"),
        err => Console.WriteLine($"Error: {err}"));
        
    // Or explicitly discard if you really don't care (not recommended)
    _ = GetValue();
}
```

### ESOX1002: Option value must be used

This analyzer warns when a method returning `Option<T>` is called but its return value is ignored.

**❌ Bad:**
```csharp
public Option<string> GetName() => new Option<string>.Some("John");

public void Process()
{
    GetName(); // Warning ESOX1002
}
```

**✅ Good:**
```csharp
public void Process()
{
    var nameOption = GetName();
    
    // Handle it properly
    if (nameOption is Option<string>.Some(var name))
    {
        Console.WriteLine($"Name: {name}");
    }
}
```

## Installation

The analyzer is available as a separate optional package:

```bash
# First, install the core library (required)
dotnet add package Esox.SharpAndRusty

# Then, optionally install the analyzer for compile-time warnings
dotnet add package Esox.SharpAndRusty.Analyzers
```

Once installed, the analyzer will start warning about unhandled Result and Option types immediately in your IDE and during builds.

## Rationale

In Rust, the `Result` type is annotated with `#[must_use]`, which causes the compiler to emit a warning if a Result is not used. This is a powerful safety feature that prevents developers from accidentally ignoring errors.

This analyzer brings the same safety to C# code using the Esox.SharpAndRusty library, helping create more robust applications.

## Configuration

The analyzers are enabled by default with **Warning** severity. You can configure them in your `.editorconfig` or `globalconfig` file:

```ini
# Promote to error
dotnet_diagnostic.ESOX1001.severity = error
dotnet_diagnostic.ESOX1002.severity = error

# Or disable (not recommended)
dotnet_diagnostic.ESOX1001.severity = none
dotnet_diagnostic.ESOX1002.severity = none
```

## Suppression

If you have a legitimate reason to not handle a Result or Option (rare!), you can:

1. **Explicitly discard it:**
   ```csharp
   _ = GetValue(); // Shows intent
   ```

2. **Suppress the warning:**
   ```csharp
   #pragma warning disable ESOX1001
   GetValue();
   #pragma warning restore ESOX1001
   ```

## How It Works

The analyzer detects when:
- A method call returns `Result<T, E>` or `Option<T>` 
- The return value is not:
  - Assigned to a variable
  - Returned from the calling method
  - Used as an argument
  - Used in pattern matching
  - Used in any other meaningful way

## Contributing

Contributions are welcome! Please see the [main repository](https://github.com/snoekiede/Esox.SharpAndRusty) for contribution guidelines.

## License

This analyzer is part of the Esox.SharpAndRusty project and is licensed under the same terms.

