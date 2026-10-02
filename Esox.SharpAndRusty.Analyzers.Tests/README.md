# Analyzer Testing

## Status

The analyzer is covered by Roslyn analyzer tests and integration testing with the `AnalyzerDemo` project.

## Unit Tests

The test suite uses `Microsoft.CodeAnalysis.CSharp.Analyzer.Testing.XUnit` with Roslyn 4.8.0-compatible references and
the framework's `DefaultVerifier`. It validates diagnostic reporting and the supported handled-value patterns.

## Integration Verification

The `AnalyzerDemo` project contains intentionally unhandled Result/Option calls that trigger analyzer warnings:

**Build Output:**

```
warning ESOX1001: Result<int, string> returned by 'GetValue' must be used. 
Ignoring a Result may hide errors.

warning ESOX1002: Option<string> returned by 'GetName' must be used. 
Ignoring an Option may hide missing values.
```

### Test Cases Verified

✅ **Triggers Warning:**

- Standalone method calls: `GetResult();`
- Property access: `someProperty;`

✅ **No Warning (Properly Handled):**

- Variable assignment: `var x = GetResult();`
- Return statements: `return GetResult();`
- Method arguments: `DoSomething(GetResult());`
- Chained calls: `GetResult().Match(...);`
- Pattern matching: `if (GetResult().TryGetValue(out var x))`
- Explicit discard: `_ = GetResult();`

### Quick Verification

To verify the analyzer works:

```bash
cd AnalyzerDemo
dotnet clean
dotnet build
```

Expected output: 2 warnings (ESOX1001 and ESOX1002)

## Conclusion

**The analyzer is covered by automated unit and integration tests.** Run the analyzer test project and `AnalyzerDemo`
build to validate diagnostic behavior before publishing.

---

*Last Updated: Implementation verified via AnalyzerDemo project*

