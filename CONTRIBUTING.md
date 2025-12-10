# Contributing to Esox.SharpAndRusty

Thank you for your interest in contributing to Esox.SharpAndRusty! This document provides guidelines and instructions for contributing to this project.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Setup](#development-setup)
- [How to Contribute](#how-to-contribute)
- [Coding Standards](#coding-standards)
- [Testing Guidelines](#testing-guidelines)
- [Pull Request Process](#pull-request-process)
- [Documentation](#documentation)
- [Project Structure](#project-structure)
- [Release Process](#release-process)

---

## Code of Conduct

This project adheres to a code of conduct that we expect all contributors to follow. Please be respectful, inclusive, and considerate in all interactions.

### Our Standards

- **Be Respectful**: Treat everyone with respect and kindness
- **Be Inclusive**: Welcome contributors of all backgrounds and experience levels
- **Be Collaborative**: Work together and help each other
- **Be Professional**: Keep discussions focused and constructive

---

## Getting Started

### Prerequisites

- **.NET 10 SDK** or later
- **Visual Studio 2022** (17.12 or later) or **Visual Studio Code** with C# extension
- **Git** for version control
- **C# 14** language features knowledge

### Repository Structure

```
Esox.SharpAndRusty/
    Esox.SharpAndRusty/           # Main library project
        Types/                     # Core types (Result, Error)
        Extensions/                # Extension methods
        README.md                  # Project documentation
    Esox.SharpAndRust.Tests/      # Test project
        Types/                     # Type tests
        Extensions/                # Extension tests


    README.md                      # Main repository documentation
    CONTRIBUTING.md                # This file
    LICENSE.txt                    # MIT License
```

---

## Development Setup

### 1. Fork and Clone

```bash
# Fork the repository on GitHub first, then clone your fork
git clone https://github.com/YOUR-USERNAME/Esox.SharpAndRusty.git
cd Esox.SharpAndRusty

# Add upstream remote
git remote add upstream https://github.com/snoekiede/Esox.SharpAndRusty.git
```

### 2. Build the Project

```bash
# Restore dependencies and build
dotnet restore
dotnet build

# Build in Release mode
dotnet build -c Release
```

### 3. Run Tests

```bash
# Run all tests
dotnet test

# Run tests with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run tests with coverage (if configured)
dotnet test --collect:"XPlat Code Coverage"
```

### 4. Create a Branch

```bash
# Create a feature branch
git checkout -b feature/your-feature-name

# Or for bug fixes
git checkout -b fix/bug-description
```

---

## How to Contribute

### Types of Contributions

We welcome various types of contributions:

1. **Bug Reports** - Found a bug? Please report it!
2. **Feature Requests** - Have an idea? We'd love to hear it!
3. **Code Contributions** - Fix bugs or implement features
4. **Documentation** - Improve or add documentation
5. **Tests** - Add or improve test coverage
6. **Examples** - Create usage examples

### Reporting Bugs

When reporting bugs, please include:

- **Clear Title**: Descriptive summary of the issue
- **Description**: Detailed explanation of the problem
- **Steps to Reproduce**: Step-by-step instructions
- **Expected Behavior**: What you expected to happen
- **Actual Behavior**: What actually happened
- **Environment**: .NET version, OS, etc.
- **Code Sample**: Minimal reproducible example

**Template:**

```markdown
## Bug Description
A clear description of the bug.

## Steps to Reproduce
1. Step one
2. Step two
3. ...

## Expected Behavior
What should happen.

## Actual Behavior
What actually happens.

## Environment
- .NET Version: 10.0
- OS: Windows 11 / macOS / Linux
- Library Version: 1.2.0

## Code Sample
```csharp
// Minimal reproducible example
var result = Result<int, string>.Ok(42);
// ...
```
```

### Suggesting Features

For feature requests, please provide:

- **Clear Title**: Descriptive feature name
- **Problem Statement**: What problem does this solve?
- **Proposed Solution**: How should it work?
- **Alternatives**: Other solutions you've considered
- **Additional Context**: Examples, use cases, etc.

---

## Coding Standards

### C# Style Guidelines

This project follows standard C# conventions with these specifics:

#### Naming Conventions

```csharp
// Classes, interfaces, structs: PascalCase
public class Result<T, E> { }
public interface IEquatable<T> { }

// Methods, properties: PascalCase
public bool IsSuccess { get; }
public void DoSomething() { }

// Private fields: _camelCase with underscore prefix
private readonly string _message;
private readonly Error? _source;

// Parameters, local variables: camelCase
public void Process(int userId, string userName) { }

// Constants: PascalCase
private const int MaxErrorChainDepth = 50;
```

#### Code Style

```csharp
// Use expression-bodied members for simple properties
public bool IsSuccess => _isSuccess;
public bool IsFailure => !IsSuccess;

// Use pattern matching
var kind = exception switch
{
    ArgumentException => ErrorKind.InvalidInput,
    TimeoutException => ErrorKind.Timeout,
    _ => ErrorKind.Other
};

// Use null-conditional operators
var source = exception.InnerException is not null
    ? FromException(exception.InnerException)
    : null;

// Prefer readonly structs for value types
public readonly struct Result<T, E> : IEquatable<Result<T, E>>

// Use nullable reference types
public Error? Source { get; }
```

#### File Organization

```csharp
// 1. Using statements
using System;
using System.Collections.Immutable;

// 2. Namespace
namespace Esox.SharpAndRusty.Types
{
    // 3. XML documentation
    /// <summary>
    /// Brief description.
    /// </summary>
    public sealed class Error
    {
        // 4. Constants
        private const int MaxDepth = 50;
        
        // 5. Fields
        private readonly string _message;
        
        // 6. Constructors
        private Error(string message) { }
        
        // 7. Properties
        public string Message => _message;
        
        // 8. Static factory methods
        public static Error New(string message) { }
        
        // 9. Instance methods
        public Error WithContext(string context) { }
        
        // 10. Private helper methods
        private static void Helper() { }
    }
}
```

### Documentation

All public APIs must have XML documentation:

```csharp
/// <summary>
/// Creates a new error with the specified message.
/// </summary>
/// <param name="message">The error message.</param>
/// <returns>A new <see cref="Error"/> instance.</returns>
/// <exception cref="ArgumentNullException">Thrown when message is null.</exception>
/// <example>
/// <code>
/// var error = Error.New("Something went wrong");
/// </code>
/// </example>
public static Error New(string message)
{
    if (message is null) throw new ArgumentNullException(nameof(message));
    return new Error(message, ErrorKind.Other, null, null, null);
}
```

### Argument Validation

Always validate public API arguments:

```csharp
public Error WithMetadata(string key, object value)
{
    // Validate null arguments
    if (key is null) throw new ArgumentNullException(nameof(key));
    if (value is null) throw new ArgumentNullException(nameof(value));
    
    // Validate business rules
    if (!IsMetadataTypeValid(value))
    {
        throw new ArgumentException(
            $"Type {value.GetType().Name} is not suitable for metadata.",
            nameof(value));
    }
    
    // Implementation
    // ...
}
```

---

## Testing Guidelines

### Test Requirements

- **All new features must have tests**
- **All bug fixes must have regression tests**
- **Maintain or improve code coverage**
- **Tests must be isolated and repeatable**

### Test Structure

Follow the **Arrange-Act-Assert** pattern:

```csharp
[Fact]
public void Method_Condition_ExpectedBehavior()
{
    // Arrange: Set up test data and dependencies
    var input = "test";
    var expected = "TEST";
    
    // Act: Execute the method being tested
    var actual = input.ToUpper();
    
    // Assert: Verify the result
    Assert.Equal(expected, actual);
}
```

### Test Naming

Use descriptive test names:

```csharp
// Good
[Fact]
public void WithMetadata_WithNullKey_ThrowsArgumentNullException()

// Good
[Fact]
public void TryGetMetadata_TypeSafe_WithCorrectType_ReturnsValue()

// Bad
[Fact]
public void Test1()

// Bad
[Fact]
public void MetadataTest()
```

### Test Categories

Organize tests into logical categories:

```csharp
public class ErrorTests
{
    #region Basic Creation Tests
    
    [Fact]
    public void New_CreatesErrorWithMessage() { }
    
    #endregion
    
    #region WithMetadata Tests
    
    [Fact]
    public void WithMetadata_AttachesMetadataToError() { }
    
    #endregion
}
```

### Test Data

Use `[Theory]` for parameterized tests:

```csharp
[Theory]
[InlineData(42)]
[InlineData("test")]
[InlineData(3.14)]
[InlineData(true)]
public void WithMetadata_WithValidPrimitiveTypes_Succeeds(object value)
{
    var error = Error.New("Test").WithMetadata("key", value);
    
    Assert.True(error.TryGetMetadata("key", out var retrievedValue));
    Assert.Equal(value, retrievedValue);
}
```

### Current Test Coverage

The project maintains **230 tests with 100% pass rate**:

- Result type: 166 tests
- Error type: 64 tests
- Extensions: Comprehensive coverage

Aim to maintain or exceed this level of coverage.

---

## Pull Request Process

### Before Submitting

1. **Update from main**:
   ```bash
   git fetch upstream
   git rebase upstream/main
   ```

2. **Ensure all tests pass**:
   ```bash
   dotnet test
   ```

3. **Build in Release mode**:
   ```bash
   dotnet build -c Release
   ```

4. **Update documentation** if needed

5. **Add yourself to contributors** (if first contribution)

### PR Checklist

- [ ] Code follows project style guidelines
- [ ] All tests pass locally
- [ ] New tests added for new functionality
- [ ] XML documentation added for public APIs
- [ ] README updated if needed
- [ ] No breaking changes (or clearly documented)
- [ ] Commit messages are clear and descriptive

### PR Template

```markdown
## Description
Brief description of the changes.

## Type of Change
- [ ] Bug fix (non-breaking change fixing an issue)
- [ ] New feature (non-breaking change adding functionality)
- [ ] Breaking change (fix or feature causing existing functionality to not work as expected)
- [ ] Documentation update

## Related Issues
Fixes #(issue number)

## Testing
Describe the tests you ran and how to reproduce them.

## Checklist
- [ ] My code follows the style guidelines
- [ ] I have performed a self-review
- [ ] I have commented my code where necessary
- [ ] I have updated the documentation
- [ ] My changes generate no new warnings
- [ ] I have added tests that prove my fix/feature works
- [ ] New and existing unit tests pass locally

## Screenshots (if applicable)
Add screenshots to help explain your changes.
```

### Review Process

1. **Automated Checks**: CI/CD must pass
2. **Code Review**: At least one maintainer approval required
3. **Testing**: All tests must pass
4. **Documentation**: Must be complete and accurate

---

## Documentation

### Types of Documentation

1. **XML Documentation**: For all public APIs
2. **README Files**: Project and feature overviews
3. **Markdown Docs**: Detailed guides and examples
4. **Code Comments**: For complex logic only

### Documentation Standards

- **Clear and Concise**: Use simple, direct language
- **Examples**: Provide code examples where helpful
- **Up-to-Date**: Keep docs in sync with code
- **Comprehensive**: Cover all use cases

### Updating Documentation

When adding features, update:

- XML documentation in code
- README.md (if public API changes)
- Relevant .md files in root directory
- Examples and usage guides

---

## Project Structure

### Core Components

#### `Esox.SharpAndRusty/Types/`

**Result.cs**
- Core `Result<T, E>` type
- Factory methods (`Ok`, `Err`)
- Pattern matching (`Match`)
- Value extraction (`UnwrapOr`, `TryGetValue`)

**Error.cs**
- Rich error type with context chaining
- Metadata attachment (type-safe and untyped)
- Error categorization (`ErrorKind`)
- Stack trace capture
- Exception conversion

#### `Esox.SharpAndRusty/Extensions/`

**ResultExtensions.cs**
- Functional operations (`Map`, `Bind`)
- LINQ support (`Select`, `SelectMany`)
- Advanced features (`MapError`, `Expect`, `Tap`)
- Collection operations (`Combine`, `Partition`)

**ResultAsyncExtensions.cs**
- Async versions of all operations
- Cancellation token support
- Async LINQ integration

**ErrorExtensions.cs**
- Error-specific extensions
- Context and metadata helpers
- Try/TryAsync wrappers

### Test Organization

Tests mirror the main project structure:

```
Esox.SharpAndRust.Tests/
    Types/
        ResultTests.cs          (core Result tests)
        ErrorTests.cs           (core Error tests)
    Extensions/
        ResultExtensionsTests.cs
        ResultAsyncExtensionsTests.cs
        ErrorExtensionsTests.cs
```

---

## Release Process

### Versioning

This project follows [Semantic Versioning](https://semver.org/):

- **MAJOR** (1.x.x): Breaking changes
- **MINOR** (x.2.x): New features, backward compatible
- **PATCH** (x.x.1): Bug fixes, backward compatible

### Current Version

**v1.2.0** - Production-ready with:
- Result type with LINQ support
- Rich Error type with production optimizations
- Full async/await integration
- 230 comprehensive tests

### Release Checklist

When releasing a new version:

1. **Update Version**:
   - Update `<Version>` in `Esox.SharpAndRusty.csproj`
   - Update version references in documentation

2. **Update Changelog**:
   - Document all changes since last release
   - Categorize: Added, Changed, Deprecated, Removed, Fixed, Security

3. **Run Full Test Suite**:
   ```bash
   dotnet test -c Release
   ```

4. **Build Release Package**:
   ```bash
   dotnet pack -c Release
   ```

5. **Tag Release**:
   ```bash
   git tag -a v1.2.0 -m "Release v1.2.0"
   git push origin v1.2.0
   ```

6. **Create GitHub Release**:
   - Write release notes
   - Attach compiled package
   - Highlight breaking changes

---

## Best Practices

### Performance

- Use `readonly struct` for value types
- Use `ImmutableDictionary` for collections
- Avoid allocations in hot paths
- Profile before optimizing

### Memory Safety

- Always validate null arguments
- Use nullable reference types
- Document null behavior
- Avoid circular references

### Error Handling

- Use `Result<T, E>` for expected failures
- Use exceptions for unexpected failures
- Provide detailed error messages
- Chain error context appropriately

### Backward Compatibility

- Avoid breaking changes when possible
- Use method overloading for new signatures
- Deprecate before removing
- Document breaking changes clearly

---

## Getting Help

### Resources

- **README.md**: Project overview and quick start
- **ERROR_TYPE.md**: Complete Error type guide
- **ERROR_TYPE_EXAMPLES.md**: Usage examples
- **ERROR_TYPE_PRODUCTION_IMPROVEMENTS.md**: Optimization details
- **CANCELLATION_TOKEN_SUPPORT.md**: Async cancellation guide

### Communication

- **GitHub Issues**: Bug reports and feature requests
- **GitHub Discussions**: Questions and general discussion
- **Pull Requests**: Code contributions and reviews

### Questions?

If you have questions:

1. Check existing documentation
2. Search closed issues
3. Open a new issue with the `question` label
4. Be specific and provide context

---

## Recognition

### Contributors

All contributors will be recognized in:
- GitHub contributors list
- Release notes for their contributions
- Project documentation (when significant)

### Types of Recognition

- **Code Contributors**: Listed in CONTRIBUTORS.md
- **Documentation Writers**: Credited in relevant docs
- **Issue Reporters**: Thanked in release notes
- **Reviewers**: Acknowledged in merged PRs

---

## License

By contributing to Esox.SharpAndRusty, you agree that your contributions will be licensed under the [MIT License](LICENSE.txt).

---

## Thank You!

Thank you for taking the time to contribute to Esox.SharpAndRusty! Your contributions help make this library better for everyone.

**Happy Coding!** ??

---

## Appendix: Quick Reference

### Common Git Commands

```bash
# Update your fork
git fetch upstream
git rebase upstream/main

# Create feature branch
git checkout -b feature/my-feature

# Commit changes
git add .
git commit -m "Add feature: description"

# Push to your fork
git push origin feature/my-feature

# Update branch with latest main
git fetch upstream
git rebase upstream/main
git push origin feature/my-feature --force-with-lease
```

### Common dotnet Commands

```bash
# Restore and build
dotnet restore
dotnet build

# Run tests
dotnet test

# Run specific test
dotnet test --filter "FullyQualifiedName~ErrorTests.WithMetadata"

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Pack for release
dotnet pack -c Release
```

### Code Review Checklist

- [ ] Code is clean and readable
- [ ] Naming follows conventions
- [ ] No magic numbers or strings
- [ ] Error handling is appropriate
- [ ] Performance is acceptable
- [ ] Memory usage is reasonable
- [ ] Thread safety if applicable
- [ ] Tests are comprehensive
- [ ] Documentation is complete
- [ ] No breaking changes (or documented)

---

**Last Updated:** 2025
**Version:** 1.2.3
**Maintainer:** Iede Snoek (Esox Solutions)
