# Collection Extensions - Quick Reference

## 📋 Complete Method List

### Original Methods (Already Existed)

#### Option Extensions
```csharp
.Sequence()            // All Some → Some(list), Any None → None
.Traverse(selector)    // Map + Sequence
.CollectSome()         // Extract all Some values
.PartitionOptions()    // (values, noneCount)
```

#### Result Extensions
```csharp
.Sequence()            // All Ok → Ok(list), First Err → Err (SHORT-CIRCUITS)
.Traverse(selector)    // Map + Sequence (SHORT-CIRCUITS)
.CollectOk()           // Extract all Ok values
.CollectErr()          // Extract all Err values
.PartitionResults()    // (successes, failures)
```

#### Either Extensions (in EitherExtensions.cs)
```csharp
.Lefts()               // Extract all Left values
.Rights()              // Extract all Right values
.Partition()           // (lefts, rights)
```

#### Validation Extensions (in ValidationExtensions.cs)
```csharp
.Sequence()            // All Valid → Valid(list), Errors → Invalid(all errors)
```

---

### 🆕 NEW Methods (Just Added)

#### Either Sequencing (4 methods)
```csharp
.SequenceLeft()        // All Left → Left(list), First Right → Right
.SequenceRight()       // All Right → Right(list), First Left → Left
.TraverseLeft(sel)     // Map + SequenceLeft
.TraverseRight(sel)    // Map + SequenceRight
```

#### Validation Extensions (2 methods)
```csharp
.TraverseValidation(validator)  // Map + Validate ALL (accumulates errors)
.PartitionValidations()         // (valid, invalid[])
```

#### Utility - First/Choose (3 methods)
```csharp
.FirstOk()             // First Ok or Err(all errors)
.FirstSome()           // First Some or None
.Choose(selector)      // First Some from selector (like F# choose)
```

#### Utility - Predicates (4 methods)
```csharp
.AnyOk()               // Any Result is Ok?
.AllOk()               // All Results are Ok?
.AnySome()             // Any Option is Some?
.AllSome()             // All Options are Some?
```

#### Utility - Error Accumulation (1 method)
```csharp
.SequenceAll()         // Like Sequence but accumulates ALL errors
```

#### Dictionary Conversion (2 methods)
```csharp
.ToOkDictionary()      // Collect Ok(KeyValuePair) → Dictionary
.ToSomeDictionary()    // Collect Some(KeyValuePair) → Dictionary
```

---

## 🎯 Common Patterns

### Pattern: Fail Fast (Short-Circuit)
```csharp
// Stop at first error
var result = items.Traverse(Process);  // Result.Traverse
// Use when: Pipeline processing, fail-fast needed
```

### Pattern: Collect All Errors
```csharp
// Accumulate all errors
var result = items.SequenceAll();           // For Results
var result = items.TraverseValidation(Val); // For Validation
// Use when: Form validation, want to show all errors
```

### Pattern: Find First Valid
```csharp
// Try multiple sources
var config = sources.Select(Load).FirstOk();
// Use when: Fallback scenarios, multiple attempts
```

### Pattern: Filter Valid Items
```csharp
// Extract successes/failures
var (successes, failures) = results.PartitionResults();
// Use when: Batch processing, need both sets
```

### Pattern: Choose/Filter Map
```csharp
// Find first valid transformation
var first = items.Choose(TryParse);
// Use when: Find-first with transformation
```

### Pattern: Build Dictionary
```csharp
// Convert results to dictionary
var dict = results.ToOkDictionary();
// Use when: Building lookups from Results/Options
```

---

## 📊 Decision Tree

### "I have a collection of Results/Options/etc. What should I use?"

```
Do you need error accumulation?
├─ YES → Use Validation.Sequence / TraverseValidation / SequenceAll
│         (Shows all errors at once)
│
└─ NO → Need to stop at first error?
    ├─ YES → Use Result.Sequence / Traverse
    │         (Fail fast, pipeline style)
    │
    └─ NO → What do you want?
        ├─ First valid item → FirstOk / FirstSome
        ├─ Check if any/all valid → AnyOk / AllOk / AnySome / AllSome
        ├─ Split into groups → PartitionResults / PartitionValidations
        ├─ Extract valid items → CollectOk / CollectSome
        ├─ Build dictionary → ToOkDictionary / ToSomeDictionary
        └─ Find-first with transform → Choose
```

---

## 🔧 Type-Specific Guide

### Working with Option<T>
```csharp
options.Sequence()            // → Option<IEnumerable<T>>
options.Traverse(f)           // → Option<IEnumerable<U>>
options.CollectSome()         // → IEnumerable<T>
options.PartitionOptions()    // → (List<T>, int)
options.FirstSome()           // → Option<T>        🆕
options.AnySome()             // → bool             🆕
options.AllSome()             // → bool             🆕
source.Choose(f)              // → Option<U>        🆕
options.ToSomeDictionary()    // → Dictionary       🆕
```

### Working with Result<T, E>
```csharp
results.Sequence()            // → Result<IEnumerable<T>, E>  (STOPS AT FIRST ERROR)
results.Traverse(f)           // → Result<IEnumerable<U>, E>  (STOPS AT FIRST ERROR)
results.CollectOk()           // → IEnumerable<T>
results.CollectErr()          // → IEnumerable<E>
results.PartitionResults()    // → (List<T>, List<E>)
results.SequenceAll()         // → Result<..., IEnumerable<E>> (ACCUMULATES ALL)  🆕
results.FirstOk()             // → Result<T, IEnumerable<E>>                      🆕
results.AnyOk()               // → bool                                           🆕
results.AllOk()               // → bool                                           🆕
results.ToOkDictionary()      // → Dictionary                                     🆕
```

### Working with Either<L, R>
```csharp
eithers.Lefts()               // → IEnumerable<L>              (existing)
eithers.Rights()              // → IEnumerable<R>              (existing)
eithers.Partition()           // → (List<L>, List<R>)          (existing)
eithers.SequenceLeft()        // → Either<IEnumerable<L>, R>   🆕
eithers.SequenceRight()       // → Either<L, IEnumerable<R>>   🆕
source.TraverseLeft(f)        // → Either<IEnumerable<L>, R>   🆕
source.TraverseRight(f)       // → Either<L, IEnumerable<R>>   🆕
```

### Working with Validation<T, E>
```csharp
validations.Sequence()           // → Validation<IEnumerable<T>, E>  (existing)
source.TraverseValidation(f)     // → Validation<IEnumerable<U>, E>  🆕
validations.PartitionValidations() // → (List<T>, List<IReadOnlyList<E>>) 🆕
```

---

## 🎓 Learning Path

### Level 1: Basic Operations
```csharp
// Extract values
options.CollectSome()         // Get all Some values
results.CollectOk()           // Get all Ok values

// Check predicates
results.AnyOk()               // Is any successful?
options.AllSome()             // Are all present?
```

### Level 2: Sequencing
```csharp
// Transform collections
options.Sequence()            // Option<IEnumerable<T>>
results.Sequence()            // Result<IEnumerable<T>, E>
```

### Level 3: Traversal (Map + Sequence)
```csharp
// Parse and collect
strings.Traverse(TryParse)    // Parse all or first error
inputs.TraverseValidation(Validate) // Validate all, accumulate errors
```

### Level 4: Advanced Patterns
```csharp
// Find-first patterns
sources.Select(Load).FirstOk()      // First success or all errors
items.Choose(TryTransform)          // First valid transformation

// Error accumulation
results.SequenceAll()               // All results, all errors
```

---

## 💡 Tips & Best Practices

### ✅ DO

```csharp
// Use specific predicates
if (results.AllOk()) { /* all valid */ }

// Chain operations
results
    .SequenceAll()
    .Map(Transform)
    .Context("Operation failed");

// Use partition for batch processing
var (successes, failures) = results.PartitionResults();
await SaveAsync(successes);
await LogAsync(failures);
```

### ❌ DON'T

```csharp
// Don't manually loop when extensions exist
foreach (var r in results)  // ❌
{
    if (r.IsSuccess) { /* ... */ }
}
// Use: results.CollectOk() ✅

// Don't check collection.Any() before operations
if (results.Any())          // ❌
    results.Sequence();
// Extensions handle empty collections ✅

// Don't use Sequence when you need all errors
results.Sequence()          // ❌ Stops at first error
// Use: results.SequenceAll() ✅ When you need all errors
```

---

## 🚀 Performance Notes

### Short-Circuiting Methods (Stop at first failure)
- `Sequence()` (Result)
- `Traverse()` (Result)
- `SequenceLeft/Right()`
- `TraverseLeft/Right()`
- `FirstOk()`, `FirstSome()`
- `Choose()`
- `AnyOk()`, `AnySome()`

### Full-Scan Methods (Process entire collection)
- `SequenceAll()` ← Accumulates all errors
- `Sequence()` (Validation)
- `TraverseValidation()` ← Accumulates all errors
- `CollectOk()`, `CollectSome()`, `CollectErr()`
- `PartitionResults()`, `PartitionValidations()`
- `AllOk()`, `AllSome()`
- `ToOkDictionary()`, `ToSomeDictionary()`

---

## 📚 Related Documentation

- `OPTION_QUICK_REFERENCE.md` - Option type basics
- `COLLECTION_EXTENSIONS_SUMMARY.md` - Original collection extensions
- `COLLECTION_ENHANCEMENTS_COMPLETE.md` - Full implementation details
- `ERROR_TYPE.md` - Error handling patterns
- `VALIDATION_DOCUMENTATION.md` - Validation type guide

---

**Quick Reminder:**
- Result → Short-circuits (stops at first error)
- Validation → Accumulates (collects all errors)
- SequenceAll → Result with error accumulation
- Choose → Like LINQ FirstOrDefault but returns Option
