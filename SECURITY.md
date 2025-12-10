# Security Policy

## Supported Versions

We take security seriously and provide security updates for the following versions of Esox.SharpAndRusty:

| Version | Supported          | Status |
| ------- | ------------------ | ------ |
| 1.2.x   | :white_check_mark: | Current stable release |
| 1.1.x   | :white_check_mark: | Maintenance support |
| 1.0.x   | :warning:          | Critical fixes only |
| < 1.0   | :x:                | No longer supported |

### Support Timeline

- **Current Release (1.2.x)**: Full security support with immediate patches
  - Latest: **1.2.2** (includes ?? experimental Mutex<T> and RwLock<T> features)
  - Core Result/Error functionality: Production-ready (9.5/10)
  - **?? Experimental**: Mutex<T> and RwLock<T> (use with caution in production)
- **Previous Release (1.1.x)**: Security updates for critical vulnerabilities (6 months after 1.2.0 release)
- **Older Releases (1.0.x)**: Critical security fixes only (3 months after 1.2.0 release)
- **Legacy Versions (< 1.0)**: Not supported - please upgrade

---

## ?? Experimental Features Security Notice

### Mutex<T> and RwLock<T> - Experimental Status

> **?? IMPORTANT**: The `Mutex<T>` and `RwLock<T>` types are **experimental features**. While they are thoroughly tested and follow security best practices, their APIs may change in future versions.

**Security Considerations for Experimental Features:**

1. **API Stability**: Experimental features may have breaking changes in minor versions
2. **Production Use**: Recommended for non-critical paths only until API stabilizes
3. **Thorough Testing**: Test extensively in your specific scenarios before deployment
4. **Monitoring**: Implement comprehensive monitoring for deadlocks and race conditions
5. **Rollback Plans**: Have contingency plans if issues arise
6. **Community Feedback**: Report any security concerns immediately

**What "Experimental" Means for Security:**
- Thoroughly tested (36+ tests for Mutex<T>)
- Follows security best practices
- Built on well-tested .NET primitives (SemaphoreSlim, ReaderWriterLockSlim)
- Result-based API for explicit error handling
- API may change based on feedback
- Use caution in production-critical systems
- Extensive real-world testing recommended

---

## Reporting a Vulnerability

We take all security vulnerabilities seriously. If you discover a security issue, please report it responsibly.

### How to Report

**Please do NOT report security vulnerabilities through public GitHub issues.**

Instead, please report security vulnerabilities by:

1. **Email** (Preferred): Send details to **info@codenomad.nl**


### What to Include

Please include the following information in your report:

- **Description**: Clear description of the vulnerability
- **Impact**: What an attacker could achieve
- **Reproduction Steps**: Detailed steps to reproduce the issue
- **Affected Versions**: Which versions are affected
- **Proof of Concept**: Code or screenshots (if applicable)
- **Suggested Fix**: Your proposed solution (if you have one)
- **Contact Information**: How we can reach you for follow-up

**Example Template:**

```markdown
## Vulnerability Report

### Summary
Brief description of the vulnerability.

### Affected Component
- File: ErrorExtensions.cs
- Method: TryAsync
- Versions: 1.2.0, 1.1.0

### Vulnerability Type
- [ ] Code Injection
- [ ] Information Disclosure
- [ ] Denial of Service
- [ ] Authentication Bypass
- [ ] Other: _______________

### Impact
What can an attacker do with this vulnerability?

### Reproduction Steps
1. Create a Result<T, Error> with...
2. Call method X with parameters...
3. Observe behavior...

### Proof of Concept
```csharp
// Minimal code to reproduce
var result = Result<int, Error>.Ok(42);
// ...
```

### Environment
- .NET Version: 10.0
- OS: Windows 11 / macOS / Linux
- Library Version: 1.2.0

### Suggested Fix
Optional: Your proposed solution.
```
```

### Response Timeline

We aim to respond to security reports according to the following timeline:

- **Initial Response**: Within 48 hours
- **Confirmation**: Within 5 business days
- **Status Updates**: Every 7 days until resolution
- **Patch Release**: Based on severity (see below)



---

## Security Best Practices

### For Library Users

#### 1. Keep Dependencies Updated

Always use the latest stable version:

```bash
# Check for updates
dotnet list package --outdated

# Update to latest version
dotnet add package Esox.SharpAndRusty --version 1.2.0
```

#### 2. Validate Input Data

Always validate data before processing, especially in error messages:

```csharp
// Don't expose sensitive data in error messages
var error = Error.New($"Failed to process credit card: {creditCardNumber}");

// Use sanitized messages
var error = Error.New("Failed to process payment")
    .WithMetadata("transactionId", transactionId)  // Safe to log
    .WithMetadata("timestamp", DateTime.UtcNow);
```

#### 3. Handle Sensitive Metadata Carefully

Be cautious with metadata containing sensitive information:

```csharp
// Don't store sensitive data in metadata
error.WithMetadata("password", userPassword);
error.WithMetadata("apiKey", secretKey);

// Use metadata for debugging, not secrets
error.WithMetadata("userId", userId);
error.WithMetadata("operationType", "login");
error.WithMetadata("attemptCount", 3);
```

#### 4. Secure Exception Handling

Use `Try` and `TryAsync` to prevent information leakage:

```csharp
// Don't expose full exception details to users
try
{
    // Operation
}
catch (Exception ex)
{
    return Error.FromException(ex); // May contain stack traces
}

// Sanitize error messages for users
var result = ErrorExtensions.Try(() => SensitiveOperation())
    .MapError(error => Error.New("Operation failed")
        .WithKind(error.Kind)
        // Don't include stack trace or detailed error in production
    );
```

#### 5. Limit Error Chain Depth

The library automatically limits error chains to 50 levels, but be mindful:

```csharp
// The library protects against this automatically
// But avoid creating unnecessarily deep chains
var error = baseError;
for (int i = 0; i < 1000; i++)  // Protected by depth limit
{
    error = error.WithContext($"Level {i}");
}
```

#### 6. Secure Async Operations

Always use cancellation tokens to prevent resource exhaustion:

```csharp
// Use cancellation tokens
var result = await ErrorExtensions.TryAsync(
    async () => await LongRunningOperation(),
    cancellationToken: cts.Token
);

// Set timeouts for operations
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
var result = await ProcessAsync(cts.Token);
```

### Mutex<T> and RwLock<T> Security (Experimental)

**Issue**: Improper use of concurrency primitives can lead to deadlocks, race conditions, or resource leaks.

**Mitigation**:
- **Result-based API** - All lock operations return explicit success/failure
- **RAII lock management** - Automatic lock release via `IDisposable`
- **Well-tested primitives** - Built on `SemaphoreSlim` (Mutex) and `ReaderWriterLockSlim` (RwLock)
- **Timeout support** - Prevents indefinite waiting
- **Cancellation support** - Allows graceful shutdown
- **No lock recursion** - Prevents accidental deadlocks from recursive locking

**Mutex<T> Best Practices:**
```csharp
// Always use 'using' for guards
var result = mutex.Lock();
if (result.TryGetValue(out var guard))
{
    using (guard)  // Lock automatically released
    {
        guard.Value++;
    }
}

// Use timeouts to prevent deadlocks
var result = mutex.TryLockTimeout(TimeSpan.FromSeconds(5));

// ? Use cancellation tokens for async operations
var result = await mutex.LockAsync(cancellationToken);

// Don't forget to dispose guards
var result = mutex.Lock();
if (result.TryGetValue(out var guard))
{
    guard.Value++;  // Never released - RESOURCE LEAK!
}
```

**RwLock<T> Best Practices:**
```csharp
// Multiple readers can access concurrently
var readResult = rwLock.Read();
if (readResult.TryGetValue(out var readGuard))
{
    using (readGuard)
    {
        // Read-only access - safe with multiple readers
        var value = readGuard.Value;
    }
}

// ? Exclusive writer access
var writeResult = rwLock.Write();
if (writeResult.TryGetValue(out var writeGuard))
{
    using (writeGuard)
    {
        // Exclusive write access
        writeGuard.Value = newValue;
    }
}

// ? Use try-variants to avoid blocking
var tryResult = rwLock.TryWrite();
if (!tryResult.IsSuccess)
{
    // Handle lock unavailable gracefully
}
```

**Deadlock Prevention:**
```csharp
// Bad: Can deadlock if threads acquire in different order
mutex1.Lock();  // Thread 1 holds mutex1
mutex2.Lock();  // Thread 2 holds mutex2
// Thread 1 waits for mutex2, Thread 2 waits for mutex1 = DEADLOCK

// Good: Use timeouts
var result1 = mutex1.TryLockTimeout(TimeSpan.FromSeconds(5));
var result2 = mutex2.TryLockTimeout(TimeSpan.FromSeconds(5));

// Better: Always acquire locks in the same order
// All threads must lock mutex1 before mutex2
```

**Experimental Status Note**:
- Mutex<T> and RwLock<T> are currently experimental
- API may change in future versions
- Thorough testing recommended before production use
- Report any deadlocks, race conditions, or unexpected behavior immediately
- Security patches will be provided promptly for reported issues

### For Contributors

#### 1. Code Review Checklist

All code must pass security review before merging:

- [ ] No hardcoded secrets or credentials
- [ ] Input validation for all public APIs
- [ ] No sensitive data in logs or error messages
- [ ] Proper exception handling
- [ ] No SQL injection vectors (if database access added)
- [ ] No code injection vulnerabilities
- [ ] Thread-safe operations
- [ ] Proper async/await patterns with cancellation
- [ ] No denial of service vulnerabilities

#### 2. Dependency Security

- Use only trusted NuGet packages
- Keep dependencies updated
- Review dependency security advisories
- Minimize dependency count

#### 3. Secure Coding Guidelines

**Argument Validation:**
```csharp
public Error WithMetadata(string key, object value)
{
    // Always validate arguments
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
}
```

**Immutability:**
```csharp
// Use readonly fields
private readonly string _message;
private readonly ImmutableDictionary<string, object>? _metadata;

// Return new instances, don't modify existing
public Error WithContext(string contextMessage)
{
    return new Error(contextMessage, _kind, this, null, null);
}
```

**Resource Cleanup:**
```csharp
// Proper async disposal
public async Task<Result<T, Error>> ProcessAsync(CancellationToken cancellationToken)
{
    await using var resource = await AcquireResourceAsync();
    // Use resource
}
```

---

## Known Security Considerations

### 1. Stack Trace Exposure

**Issue**: Stack traces may contain sensitive file paths or internal implementation details.

**Mitigation**:
- Use `CaptureStackTrace(includeFileInfo: false)` in production
- Sanitize error messages before sending to clients
- Use metadata for structured logging instead of stack traces

**Example**:
```csharp
#if DEBUG
var error = Error.New("Operation failed")
    .CaptureStackTrace(includeFileInfo: true);  // Detailed in dev
#else
var error = Error.New("Operation failed")
    .CaptureStackTrace(includeFileInfo: false);  // Safe in production
#endif
```

### 2. Error Message Information Disclosure

**Issue**: Detailed error messages might reveal internal system structure.

**Mitigation**:
- Use generic error messages for external users
- Store detailed information in metadata (logged internally)
- Filter error messages based on environment

**Example**:
```csharp
public Error CreateUserFacingError(Error internalError)
{
    // External: Generic message
    var publicError = Error.New("An error occurred")
        .WithKind(internalError.Kind);
    
    // Internal: Full context (logged, not sent to client)
    Logger.LogError(internalError.GetFullMessage());
    
    return publicError;
}
```

### 3. Metadata Storage

**Issue**: Metadata is stored as objects and could contain references to sensitive data.

**Mitigation**:
- Type validation prevents storing complex objects
- Only primitives, strings, and value types allowed
- Immutable collections prevent modification

**Protected by Design**:
```csharp
// Type validation prevents this
error.WithMetadata("connection", databaseConnection);  // Throws ArgumentException

// ? Only safe types allowed
error.WithMetadata("userId", 123);           // OK: int
error.WithMetadata("timestamp", DateTime.UtcNow);  // OK: DateTime
```

### 4. Async Exception Handling

**Issue**: Unhandled exceptions in async operations could leak information.

**Mitigation**:
- All async methods have proper exception handling
- Cancellation tokens supported throughout
- Proper async/await patterns used

**Built-in Protection**:
```csharp
public static async Task<Result<T, Error>> TryAsync<T>(
    Func<Task<T>> operation,
    CancellationToken cancellationToken = default)
{
    try
    {
        cancellationToken.ThrowIfCancellationRequested();
        var value = await operation().ConfigureAwait(false);
        return Ok(value);
    }
    catch (Exception ex)  // All exceptions caught
    {
        return Err(Error.FromException(ex));
    }
}
```

### 5. Circular Reference Protection

**Issue**: Circular references in error chains could cause stack overflow or infinite loops.

**Mitigation**:
- HashSet-based cycle detection in `GetFullMessage()`
- Depth limiting (max 50 levels)
- Graceful degradation with informative messages

**Automatic Protection**:
```csharp
// Protected automatically - no user action needed
var message = error.GetFullMessage();  // Safe, even with cycles
```

### 6. Mutex<T> and RwLock<T> Concurrency Safety (Experimental)

**Issue**: Improper use of concurrency primitives can lead to deadlocks, race conditions, or resource leaks.

**Mitigation**:
- **Result-based API** - All lock operations return `Result<Guard, Error>` for explicit handling
- **RAII pattern** - Guards automatically release locks on disposal
- **Timeout support** - All blocking operations have timeout variants
- **Cancellation support** - Async operations support `CancellationToken`
- **No lock recursion** - Configured to prevent recursive locking
- **Tested primitives** - Built on `SemaphoreSlim` and `ReaderWriterLockSlim`

**Automatic Protection**:
```csharp
// Protected by RAII - lock automatically released
using var guard = mutex.Lock().Expect("Failed to acquire lock");
guard.Value++;
// Lock released here automatically

// Timeout prevents indefinite waiting
var result = mutex.TryLockTimeout(TimeSpan.FromSeconds(5));
result.Match(
    success: guard => { /* process */ },
    failure: error => { /* handle timeout */ }
);

// Cancellation allows graceful shutdown
var result = await mutex.LockAsync(cancellationToken);
```

**Common Pitfalls to Avoid**:
```csharp
// Forgetting to dispose guard
var result = mutex.Lock();
if (result.TryGetValue(out var guard))
{
    guard.Value++;
    // Guard not disposed - lock never released!
}

// Inconsistent lock ordering
// Thread 1: lock(A), lock(B)
// Thread 2: lock(B), lock(A)
// Result: Potential deadlock

// Holding lock while doing expensive I/O
using var guard = mutex.Lock().Unwrap();
await ExpensiveNetworkCall();  // Other threads blocked!
guard.Value = result;

// Better: Do work outside the lock
var result = await ExpensiveNetworkCall();
using var guard = mutex.Lock().Unwrap();
guard.Value = result;  // Lock held minimally
```

**Experimental Feature Risks**:
- API changes in future versions may require code updates
- Edge cases may exist in specific concurrency scenarios
- Stress testing recommended for high-contention use cases
- Community feedback will improve API safety and usability

---

## Security Features

### Built-in Security Mechanisms

1. **Immutability**
   - All types are immutable by design
   - Prevents accidental or malicious modification
   - Thread-safe by default

2. **Type Safety**
   - Strong typing prevents type confusion attacks
   - Generic constraints ensure type correctness
   - Nullable reference types prevent null reference errors

3. **Resource Management**
   - Proper async/await patterns
   - Cancellation token support
   - No resource leaks
   - RAII pattern for automatic cleanup (Mutex guards)

4. **Depth Limiting**
   - Error chains limited to 50 levels
   - Prevents stack overflow attacks
   - Graceful degradation

5. **Cycle Detection**
   - HashSet-based circular reference detection
   - Prevents infinite loops
   - O(1) detection per node

6. **Argument Validation**
   - All public APIs validate arguments
   - Clear exception messages
   - No undefined behavior

7. **Concurrency Safety (Mutex<T> and RwLock<T> - ?? Experimental)**
   - Explicit lock acquisition via Result types
   - Automatic lock release via IDisposable (RAII pattern)
   - Timeout support to prevent indefinite waiting
   - Cancellation token support for async operations
   - No lock recursion to prevent accidental deadlocks
   - Built on well-tested .NET concurrency primitives
   - Result-based API forces explicit error handling

---

## Compliance and Standards

### Security Standards

This library follows these security principles:

- **OWASP Secure Coding Practices**: Input validation, error handling, data protection
- **Principle of Least Privilege**: Minimal API surface, restricted access
- **Defense in Depth**: Multiple layers of protection
- **Fail Securely**: Secure defaults, graceful degradation
- **Secure by Design**: Immutability, type safety, validation

### Privacy Considerations

- **No Telemetry**: Library does not collect or send any data
- **No External Dependencies**: No third-party data transmission
- **Local Processing Only**: All operations are local
- **User Control**: You control what data goes into errors/metadata

---

## Security Updates

### Notification Channels

Stay informed about security updates:

1. **GitHub Security Advisories**: [Subscribe to security advisories](https://github.com/snoekiede/Esox.SharpAndRusty/security/advisories)
2. **GitHub Releases**: [Watch releases](https://github.com/snoekiede/Esox.SharpAndRusty/releases)
3. **NuGet**: Check for updates regularly

### Update Process

When a security update is released:

1. **Announcement**: Published on GitHub and security advisory
2. **Patch Release**: New version released with fix
3. **CHANGELOG**: Updated with security fix details
4. **Documentation**: Updated if necessary

---

## Responsible Disclosure

### Our Commitment

We are committed to:

- **Acknowledgment**: We will acknowledge receipt of your report
- **Communication**: We will keep you informed of progress
- **Credit**: We will credit you in release notes (if desired)
- **Responsible Disclosure**: We will coordinate disclosure timing with you



## Questions and Contact

### Security Questions

For security-related questions:

- **Email**: security@esoxsolutions.com
- **GitHub**: [Create a private security advisory](https://github.com/snoekiede/Esox.SharpAndRusty/security/advisories/new)

### General Questions

For non-security questions:

- **GitHub Issues**: [Create an issue](https://github.com/snoekiede/Esox.SharpAndRusty/issues)
- **GitHub Discussions**: [Start a discussion](https://github.com/snoekiede/Esox.SharpAndRusty/discussions)

---

## Additional Resources

- [OWASP Secure Coding Practices](https://owasp.org/www-project-secure-coding-practices-quick-reference-guide/)
- [Microsoft Security Development Lifecycle](https://www.microsoft.com/en-us/securityengineering/sdl)
- [.NET Security Guidelines](https://docs.microsoft.com/en-us/dotnet/standard/security/)
- [NuGet Package Security Best Practices](https://docs.microsoft.com/en-us/nuget/concepts/security-best-practices)
- [.NET Concurrency Best Practices](https://docs.microsoft.com/en-us/dotnet/standard/threading/managed-threading-best-practices)
- [Deadlock Prevention Patterns](https://docs.microsoft.com/en-us/dotnet/standard/threading/managed-threading-best-practices#deadlocks)

---

**Last Updated**: 2025  
**Policy Version**: 1.1  
**Contact**: security@esoxsolutions.com

---

## Appendix: Security Checklist for Users

### Before Using in Production

- [ ] Review this security policy
- [ ] Update to latest stable version (1.2.2)
- [ ] Validate no sensitive data in error messages
- [ ] Sanitize error messages before sending to clients
- [ ] Use `includeFileInfo: false` for stack traces
- [ ] Implement proper logging (separate from user-facing errors)
- [ ] Use cancellation tokens for async operations
- [ ] Set appropriate timeouts
- [ ] Test error handling paths
- [ ] Review metadata contents

**If using Mutex<T> or RwLock<T> (?? Experimental):**
- [ ] ?? Understand experimental status and potential API changes
- [ ] ?? Use in non-critical paths initially
- [ ] ? Always use `using` statements with lock guards
- [ ] ? Set appropriate timeouts to prevent deadlocks
- [ ] ? Use consistent lock ordering if acquiring multiple locks
- [ ] ? Minimize time holding locks (no expensive I/O under lock)
- [ ] ? Test concurrency scenarios thoroughly (stress tests)
- [ ] ? Monitor for deadlocks in production
- [ ] ? Have rollback plan if issues arise
- [ ] ? Report any issues or concerns immediately
- [ ] ?? Be prepared for API changes in future versions
- [ ] ?? Subscribe to release notes for breaking changes

### Ongoing Security Maintenance

- [ ] Monitor for security advisories
- [ ] Update dependencies regularly
- [ ] Review security logs
- [ ] Test security scenarios
- [ ] Train team on secure usage
- [ ] Conduct security reviews
- [ ] Keep documentation updated
- [ ] Review concurrency patterns (if using experimental features)
- [ ] Monitor for deadlocks and race conditions
- [ ] Stay informed about experimental feature stabilization

---

Thank you for helping keep Esox.SharpAndRusty and its users secure! ??
