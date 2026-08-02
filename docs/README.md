# Feature Documentation

This directory contains detailed documentation for Esox.SharpAndRusty features.

## Documentation Structure

- **Main Documentation**: See the [main README](../README.md) for getting started and core concepts
- **Advanced Features**: See [ADVANCED_FEATURES.md](../Esox.SharpAndRusty/ADVANCED_FEATURES.md) in the library folder
- **API Reference**: Generated XML documentation in the library
- **Analyzers**: See [Analyzers README](../Esox.SharpAndRusty.Analyzers/README.md)

## Quick Links

### Core Types
- `Result<T, E>` - For operations that can fail
- `ExtendedResult<T, TE>` - Extended result with additional functionality
- `Option<T>` - For optional values
- `Error` - Rich error type with context and metadata

### Synchronization Primitives
- `RwLock<T>` - Reader-writer lock inspired by Rust
- `Mutex<T>` - Mutual exclusion lock
- `Arc<T>` - Atomic reference counting

### Extensions
- Collection extensions for Result and Option types
- Async extensions for asynchronous operations
- LINQ-style query syntax support

## Key Features

### Error Handling
- **Result Pattern**: Type-safe error handling without exceptions
- **Rich Error Type**: Context, metadata, and error chaining
- **Circular Reference Protection**: Prevents infinite loops in error chains

### Thread Safety
- **Interior Mutability**: Safe shared mutable state with RwLock and Mutex
- **Lock Guards**: RAII-style automatic lock release
- **Configurable Recursion**: Optional support for recursive locks

### Functional Programming
- **Pattern Matching**: Match, Map, Bind operations
- **Implicit Conversions**: Ergonomic API with implicit operators
- **Monadic Operations**: Functor and monad patterns

### Production Features
- **CancellationToken Support**: Cancellation across all async operations
- **Validation Patterns**: Fluent validation without LINQ where clauses
- **Error Transformation**: MapError for cross-boundary error conversion

## Migration Notes

All historical implementation summaries and temporary documentation files have been removed
to keep documentation concise. The essential information has been preserved in:

1. Main README.md - Project overview and getting started
2. CHANGELOG.md - Version history and breaking changes  
3. Esox.SharpAndRusty/ADVANCED_FEATURES.md - Detailed feature documentation
4. Project-specific READMEs in their respective folders

For historical context, refer to git history or previous releases.
