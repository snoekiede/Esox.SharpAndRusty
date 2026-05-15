# Quick Start: Analyzer Setup

## For Library Users

The analyzer is available as an **optional separate package**. First install the core library, then optionally add the analyzer:

```bash
# Install the core library (required)
dotnet add package Esox.SharpAndRusty

# Optionally install the analyzer for compile-time warnings
dotnet add package Esox.SharpAndRusty.Analyzers
```

Once installed, you'll immediately start seeing warnings when Result or Option types are not handled.

## For Library Developers

If you're working with the source code:

### Building the Analyzer

```bash
cd Esox.SharpAndRusty.Analyzers
dotnet build
```

### Testing the Analyzer Locally

Reference the analyzer directly in your project:

```xml
<ItemGroup>
  <ProjectReference Include="..\Esox.SharpAndRusty.Analyzers\Esox.SharpAndRusty.Analyzers.csproj" 
                    OutputItemType="Analyzer" 
                    ReferenceOutputAssembly="false" />
</ItemGroup>
```

### Demo Project

Run the demo to see the analyzer in action:

```bash
cd AnalyzerDemo
dotnet build   # See warnings
dotnet run     # Run the demo
```

## Configuring Warnings

### Promote to Errors (.editorconfig)

```ini
[*.cs]
# Make unhandled Result an error
dotnet_diagnostic.ESOX1001.severity = error

# Make unhandled Option an error
dotnet_diagnostic.ESOX1002.severity = error
```

### Suppress in Code

If you have a legitimate reason to ignore a Result/Option:

```csharp
// Option 1: Explicit discard (recommended)
_ = GetResult();

// Option 2: Pragma directive
#pragma warning disable ESOX1001
GetResult();
#pragma warning restore ESOX1001

// Option 3: Attribute (on method)
[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "ESOX1001")]
public void MyMethod()
{
    GetResult();
}
```

### Disable Globally (Not Recommended)

In `.editorconfig`:
```ini
dotnet_diagnostic.ESOX1001.severity = none
dotnet_diagnostic.ESOX1002.severity = none
```

## What Gets Flagged

❌ **Triggers Warning:**
```csharp
GetResult();              // Standalone call
SomeProperty;             // Unused property
obj.GetValue();          // Ignored return value
```

✅ **No Warning:**
```csharp
var x = GetResult();                    // Assigned
return GetResult();                     // Returned
DoSomething(GetResult());              // Used as argument
GetResult().Match(...);                // Chained method call
if (GetResult().TryGetValue(...))      // Pattern matching
_ = GetResult();                       // Explicitly discarded
```

## IDE Support

The analyzer works in:
- ✅ Visual Studio 2022+
- ✅ VS Code with C# extension
- ✅ JetBrains Rider
- ✅ Command-line builds (`dotnet build`)
- ✅ CI/CD pipelines

## Troubleshooting

### Analyzer Not Loading

If you don't see warnings:

1. **Verify analyzer package is installed:**
   ```bash
   # The analyzer is a separate optional package
   dotnet add package Esox.SharpAndRusty.Analyzers
   ```

2. **Clean and rebuild:**
   ```bash
   dotnet clean
   dotnet build
   ```

3. **Check analyzer is included:**
   ```bash
   dotnet build /p:ReportAnalyzer=true
   ```

4. **Restart IDE** (especially VS Code)

5. **Verify package versions:**
   Ensure you have both packages:
   - `Esox.SharpAndRusty` (core library)
   - `Esox.SharpAndRusty.Analyzers` (analyzer - optional)

### Too Many Warnings

If you're adding the analyzer to existing code:

1. **Gradual adoption:** Start with warning severity, fix incrementally
2. **Suppress legacy code:** Use `#pragma warning disable` in old files
3. **Configure by directory:** Use `.editorconfig` with different rules per folder

## Example: Gradual Adoption

```ini
# .editorconfig
# Root config
root = true

[*.cs]
# Default: warnings
dotnet_diagnostic.ESOX1001.severity = warning
dotnet_diagnostic.ESOX1002.severity = warning

# New code: errors
[src/NewFeature/**/*.cs]
dotnet_diagnostic.ESOX1001.severity = error
dotnet_diagnostic.ESOX1002.severity = error

# Legacy code: info (or none)
[src/Legacy/**/*.cs]
dotnet_diagnostic.ESOX1001.severity = suggestion
dotnet_diagnostic.ESOX1002.severity = suggestion
```

## Learn More

- Full documentation: `ROSLYN_ANALYZER_SUMMARY.md`
- Analyzer README: `Esox.SharpAndRusty.Analyzers/README.md`
- Demo code: `AnalyzerDemo/`

