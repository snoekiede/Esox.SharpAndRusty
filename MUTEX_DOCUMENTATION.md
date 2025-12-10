# Rust-like Mutex in Esox.SharpAndRusty

## Overview

The `Mutex<T>` type provides Rust-inspired mutual exclusion for protecting shared data in concurrent scenarios. Unlike traditional C# locking mechanisms, this implementation integrates seamlessly with the `Result<T, E>` and `Error` types, providing type-safe, explicit error handling for lock operations.

---

## Table of Contents

- [What is Mutex?](#what-is-mutex)
- [Key Features](#key-features)
- [Basic Usage](#basic-usage)
- [API Reference](#api-reference)
- [Advanced Scenarios](#advanced-scenarios)
- [Comparison with Rust](#comparison-with-rust)
- [Best Practices](#best-practices)
- [Performance Considerations](#performance-considerations)

---

## What is Mutex?

A **Mutex** (mutual exclusion) is a synchronization primitive that protects shared data by allowing only one thread to access it at a time. This implementation is inspired by Rust's `std::sync::Mutex` and provides:

- **Interior Mutability**: Modify shared data safely across threads
- **RAII Locking**: Automatic lock release through `IDisposable`
- **Result-based API**: Explicit error handling for lock operations
- **Async Support**: Full `async`/`await` integration with cancellation

### Comparison with Traditional C# Locking

**Traditional C# `lock`:**
```csharp
private readonly object _lock = new();
private int _value = 0;

public void Increment()
{
    lock (_lock)
    {
        _value++;
    }
}
```

**Esox.SharpAndRusty `Mutex<T>`:**
```csharp
private readonly Mutex<int> _mutex = new Mutex<int>(0);

public Result<Unit, Error> Increment()
{
    var result = _mutex.Lock();
    if (result.TryGetValue(out var guard))
    {
        using (guard)
        {
            guard.Value++;
        }
        return Result<Unit, Error>.Ok(default);
    }
    return Result<Unit, Error>.Err(result.TryGetError(out var err) ? err : Error.New("Unknown error"));
}
```

**Benefits:**
- Explicit success/failure handling
- Type-safe access to protected data
- Better error reporting
- Integrates with existing Result-based code

---

## Key Features

### 1. **Type-Safe Data Protection**

The mutex wraps your data and provides controlled access:

```csharp
var mutex = new Mutex<int>(42);

// Access through guard
var result = mutex.Lock();
if (result.TryGetValue(out var guard))
{
    using (guard)
    {
        int value = guard.Value;  // Read
        guard.Value = 100;         // Write
    } // Lock automatically released
}
```

### 2. **Result-Based Error Handling**

All lock operations return `Result<MutexGuard<T>, Error>`:

```csharp
var mutex = new Mutex<int>(0);

var result = mutex.Lock();
result.Match(
    success: guard =>
    {
        using (guard) { /* work with data */ }
        return "Success";
    },
    failure: error => $"Failed to acquire lock: {error.Message}"
);
```

### 3. **Multiple Lock Strategies**

```csharp
// Blocking lock
var result1 = mutex.Lock();

// Non-blocking try
var result2 = mutex.TryLock();

// Timeout-based
var result3 = mutex.TryLockTimeout(TimeSpan.FromSeconds(5));

// Async
var result4 = await mutex.LockAsync();

// Async with timeout
var result5 = await mutex.LockAsyncTimeout(TimeSpan.FromSeconds(5), cancellationToken);
```

### 4. **RAII Lock Management**

Locks are automatically released when guards are disposed:

```csharp
var mutex = new Mutex<int>(0);

// Lock is held only within the using block
using (var guard = mutex.Lock().Unwrap())
{
    guard.Value = 42;
} // Lock released here

// Can immediately acquire again
using (var guard = mutex.Lock().Unwrap())
{
    Console.WriteLine(guard.Value); // 42
}
```

### 5. **Functional Operations**

Guards support functional operations:

```csharp
var mutex = new Mutex<int>(42);
using var guard = mutex.Lock().Unwrap();

// Map - transform value
string result = guard.Map(x => $"Value is {x}");

// Update - modify in place
guard.Update(x => x * 2);
```

---

## Basic Usage

### Creating a Mutex

```csharp
using Esox.SharpAndRusty.Async;
using Esox.SharpAndRusty.Types;

// Create with initial value
var mutex = new Mutex<int>(0);

// Works with any type
var stringMutex = new Mutex<string>("Hello");
var listMutex = new Mutex<List<int>>(new List<int>());
```

### Locking and Accessing Data

```csharp
var mutex = new Mutex<int>(0);

// Lock and access
var result = mutex.Lock();
if (result.TryGetValue(out var guard))
{
    using (guard)
    {
        // Read value
        int current = guard.Value;
        
        // Modify value
        guard.Value = current + 1;
    }
}
```

### Error Handling

```csharp
var mutex = new Mutex<int>(42);

var result = mutex.Lock();
if (result.IsFailure && result.TryGetError(out var error))
{
    Console.WriteLine($"Lock failed: {error.Message}");
    Console.WriteLine($"Error kind: {error.Kind}");
    
    if (error.TryGetMetadata("attemptTime", out DateTime time))
    {
        Console.WriteLine($"Attempted at: {time}");
    }
}
```

### Try Lock (Non-Blocking)

```csharp
var mutex = new Mutex<int>(42);

var result = mutex.TryLock();
result.Match(
    success: guard =>
    {
        using (guard)
        {
            Console.WriteLine($"Acquired lock: {guard.Value}");
        }
        return "Done";
    },
    failure: error =>
    {
        Console.WriteLine($"Lock busy: {error.Message}");
        return "Skipped";
    }
);
```

### Async Locking

```csharp
var mutex = new Mutex<int>(0);

async Task IncrementAsync()
{
    var result = await mutex.LockAsync();
    if (result.TryGetValue(out var guard))
    {
        using (guard)
        {
            await Task.Delay(10); // Simulate async work
            guard.Value++;
        }
    }
}

await Task.WhenAll(
    IncrementAsync(),
    IncrementAsync(),
    IncrementAsync()
);
```

---

## API Reference

### `Mutex<T>` Class

#### Constructor

```csharp
public Mutex(T value)
```

Creates a new mutex with the specified initial value.

#### Methods

##### `Lock()`

```csharp
public Result<MutexGuard<T>, Error> Lock()
```

Acquires the mutex, blocking until available.

**Returns:** `Result<MutexGuard<T>, Error>`
- `Ok(guard)` - Lock acquired successfully
- `Err(error)` - Lock acquisition failed

**Errors:**
- `InvalidOperation` - Mutex is disposed

**Example:**
```csharp
var mutex = new Mutex<int>(42);
var result = mutex.Lock();
```

##### `TryLock()`

```csharp
public Result<MutexGuard<T>, Error> TryLock()
```

Attempts to acquire the mutex without blocking.

**Returns:** `Result<MutexGuard<T>, Error>`
- `Ok(guard)` - Lock acquired
- `Err(error)` - Lock currently held or mutex disposed

**Errors:**
- `ResourceExhausted` - Lock is currently held
- `InvalidOperation` - Mutex is disposed

**Example:**
```csharp
var result = mutex.TryLock();
if (result.IsFailure)
{
    Console.WriteLine("Lock busy, try again later");
}
```

##### `TryLockTimeout(TimeSpan)`

```csharp
public Result<MutexGuard<T>, Error> TryLockTimeout(TimeSpan timeout)
```

Attempts to acquire the mutex, waiting up to the specified timeout.

**Parameters:**
- `timeout` - Maximum time to wait

**Returns:** `Result<MutexGuard<T>, Error>`
- `Ok(guard)` - Lock acquired within timeout
- `Err(error)` - Timeout expired or mutex disposed

**Errors:**
- `Timeout` - Timeout expired (includes timeout metadata)
- `InvalidOperation` - Mutex is disposed

**Example:**
```csharp
var result = mutex.TryLockTimeout(TimeSpan.FromSeconds(5));
```

##### `LockAsync(CancellationToken)`

```csharp
public async Task<Result<MutexGuard<T>, Error>> LockAsync(
    CancellationToken cancellationToken = default)
```

Asynchronously acquires the mutex.

**Parameters:**
- `cancellationToken` - Optional cancellation token

**Returns:** `Task<Result<MutexGuard<T>, Error>>`
- `Ok(guard)` - Lock acquired
- `Err(error)` - Operation cancelled or mutex disposed

**Errors:**
- `Interrupted` - Operation was cancelled
- `InvalidOperation` - Mutex is disposed

**Example:**
```csharp
var cts = new CancellationTokenSource();
var result = await mutex.LockAsync(cts.Token);
```

##### `LockAsyncTimeout(TimeSpan, CancellationToken)`

```csharp
public async Task<Result<MutexGuard<T>, Error>> LockAsyncTimeout(
    TimeSpan timeout,
    CancellationToken cancellationToken = default)
```

Asynchronously attempts to acquire the mutex with timeout and cancellation.

**Parameters:**
- `timeout` - Maximum time to wait
- `cancellationToken` - Optional cancellation token

**Returns:** `Task<Result<MutexGuard<T>, Error>>`

**Errors:**
- `Timeout` - Timeout expired
- `Interrupted` - Operation was cancelled
- `InvalidOperation` - Mutex is disposed

**Example:**
```csharp
var result = await mutex.LockAsyncTimeout(
    TimeSpan.FromSeconds(10),
    cancellationToken
);
```

##### `IntoInner()`

```csharp
public T IntoInner()
```

Consumes the mutex and returns the inner value. The mutex is disposed after this call.

**Returns:** The protected value

**Throws:** `ObjectDisposedException` if mutex is already disposed

**Example:**
```csharp
var mutex = new Mutex<int>(42);
int value = mutex.IntoInner(); // mutex is now disposed
```

##### `Dispose()`

```csharp
public void Dispose()
```

Releases all resources used by the mutex.

#### Properties

```csharp
public bool IsDisposed { get; }
```

Gets whether the mutex has been disposed.

---

### `MutexGuard<T>` Class

The guard provides exclusive access to the protected data.

#### Properties

```csharp
public T Value { get; set; }
```

Gets or sets the protected value.

**Throws:** `ObjectDisposedException` if guard is disposed

#### Methods

##### `Map<TResult>(Func<T, TResult>)`

```csharp
public TResult Map<TResult>(Func<T, TResult> mapper)
```

Transforms the guarded value.

**Example:**
```csharp
using var guard = mutex.Lock().Unwrap();
string result = guard.Map(x => $"Value: {x}");
```

##### `Update(Func<T, T>)`

```csharp
public void Update(Func<T, T> updater)
```

Updates the guarded value in place.

**Example:**
```csharp
using var guard = mutex.Lock().Unwrap();
guard.Update(x => x * 2);
```

##### `Dispose()`

```csharp
public void Dispose()
```

Releases the lock and writes back any modifications.

---

## Advanced Scenarios

### Shared Counter

```csharp
public class SharedCounter
{
    private readonly Mutex<int> _mutex = new Mutex<int>(0);

    public Result<int, Error> Increment()
    {
        return _mutex.Lock()
            .Map(guard =>
            {
                using (guard)
                {
                    guard.Update(x => x + 1);
                    return guard.Value;
                }
            });
    }

    public Result<int, Error> GetValue()
    {
        return _mutex.Lock()
            .Map(guard =>
            {
                using (guard)
                {
                    return guard.Value;
                }
            });
    }
}
```

### Thread-Safe Cache

```csharp
public class ThreadSafeCache<TKey, TValue> where TKey : notnull
{
    private readonly Mutex<Dictionary<TKey, TValue>> _mutex;

    public ThreadSafeCache()
    {
        _mutex = new Mutex<Dictionary<TKey, TValue>>(
            new Dictionary<TKey, TValue>()
        );
    }

    public Result<TValue?, Error> Get(TKey key)
    {
        return _mutex.Lock()
            .Map(guard =>
            {
                using (guard)
                {
                    guard.Value.TryGetValue(key, out var value);
                    return value;
                }
            });
    }

    public Result<Unit, Error> Set(TKey key, TValue value)
    {
        return _mutex.Lock()
            .Map(guard =>
            {
                using (guard)
                {
                    guard.Value[key] = value;
                    return default(Unit);
                }
            });
    }
}
```

### Async Work Queue

```csharp
public class AsyncWorkQueue<T>
{
    private readonly Mutex<Queue<T>> _mutex;

    public AsyncWorkQueue()
    {
        _mutex = new Mutex<Queue<T>>(new Queue<T>());
    }

    public async Task<Result<Unit, Error>> EnqueueAsync(
        T item,
        CancellationToken cancellationToken = default)
    {
        var result = await _mutex.LockAsync(cancellationToken);
        return result.Map(guard =>
        {
            using (guard)
            {
                guard.Value.Enqueue(item);
                return default(Unit);
            }
        });
    }

    public async Task<Result<T?, Error>> DequeueAsync(
        CancellationToken cancellationToken = default)
    {
        var result = await _mutex.LockAsync(cancellationToken);
        return result.Map(guard =>
        {
            using (guard)
            {
                if (guard.Value.Count > 0)
                {
                    return guard.Value.Dequeue();
                }
                return default(T);
            }
        });
    }
}
```

### Timeout-Based Resource Access

```csharp
public class ResourcePool<T>
{
    private readonly Mutex<Stack<T>> _mutex;
    private readonly TimeSpan _timeout;

    public ResourcePool(IEnumerable<T> resources, TimeSpan timeout)
    {
        _mutex = new Mutex<Stack<T>>(new Stack<T>(resources));
        _timeout = timeout;
    }

    public Result<PooledResource<T>, Error> Acquire()
    {
        return _mutex.TryLockTimeout(_timeout)
            .Bind(guard =>
            {
                if (guard.Value.Count == 0)
                {
                    guard.Dispose();
                    return Result<PooledResource<T>, Error>.Err(
                        Error.New("No resources available", ErrorKind.ResourceExhausted)
                    );
                }

                var resource = guard.Value.Pop();
                guard.Dispose();
                
                return Result<PooledResource<T>, Error>.Ok(
                    new PooledResource<T>(resource, this)
                );
            });
    }

    internal void Return(T resource)
    {
        var result = _mutex.TryLockTimeout(_timeout);
        if (result.TryGetValue(out var guard))
        {
            using (guard)
            {
                guard.Value.Push(resource);
            }
        }
    }
}

public class PooledResource<T> : IDisposable
{
    private readonly T _resource;
    private readonly ResourcePool<T> _pool;
    private bool _disposed;

    internal PooledResource(T resource, ResourcePool<T> pool)
    {
        _resource = resource;
        _pool = pool;
    }

    public T Value => _resource;

    public void Dispose()
    {
        if (!_disposed)
        {
            _pool.Return(_resource);
            _disposed = true;
        }
    }
}
```

---

## Comparison with Rust

### Similarities

**RAII Lock Management**
- Both use RAII for automatic lock release
- Rust: `drop(guard)`, C#: `guard.Dispose()`

**Interior Mutability**
- Both allow mutation of shared data
- Rust: `*guard = value`, C#: `guard.Value = value`

**Type Safety**
- Both provide compile-time type safety
- Protected data type is part of the type signature

**Explicit Lock Acquisition**
- Both make locking explicit
- Rust: `mutex.lock()`, C#: `mutex.Lock()`

### Differences

#### 1. **Error Handling**

**Rust:**
```rust
let mutex = Mutex::new(0);
let mut guard = mutex.lock().unwrap(); // Panics on poisoned mutex
*guard += 1;
```

**C#:**
```csharp
var mutex = new Mutex<int>(0);
var result = mutex.Lock();
if (result.TryGetValue(out var guard))
{
    using (guard)
    {
        guard.Value++;
    }
}
```

Rust panics on poisoned mutexes, C# returns `Result<T, Error>`.

#### 2. **Poisoning**

**Rust:** Mutexes are poisoned if a thread panics while holding the lock.

**C#:** No poisoning concept - if an exception occurs, the lock is released normally via `finally` blocks.

#### 3. **Borrowing vs Disposal**

**Rust:** Compile-time borrow checker ensures guard is valid.

**C#:** Runtime disposal pattern with `IDisposable`.

#### 4. **Try Lock Semantics**

**Rust:**
```rust
match mutex.try_lock() {
    Ok(guard) => { /* use guard */ }
    Err(TryLockError::WouldBlock) => { /* handle busy */ }
    Err(TryLockError::Poisoned(_)) => { /* handle poison */ }
}
```

**C#:**
```csharp
var result = mutex.TryLock();
result.Match(
    success: guard => { /* use guard */ },
    failure: error => { /* handle error */ }
);
```

---

## Best Practices

### 1. **Always Use `using` Statements**

```csharp
// ? Good - automatic disposal
var result = mutex.Lock();
if (result.TryGetValue(out var guard))
{
    using (guard)
    {
        guard.Value = 42;
    }
}

// Bad - might forget to dispose
var result = mutex.Lock();
if (result.TryGetValue(out var guard))
{
    guard.Value = 42;
    guard.Dispose(); // Easy to forget
}
```

### 2. **Keep Critical Sections Short**

```csharp
// Good - minimal work while locked
var data = await LoadDataAsync();
var result = mutex.Lock();
if (result.TryGetValue(out var guard))
{
    using (guard)
    {
        guard.Value = data; // Quick assignment
    }
}

// Bad - long operation while locked
var result = mutex.Lock();
if (result.TryGetValue(out var guard))
{
    using (guard)
    {
        var data = await LoadDataAsync(); // Blocking others
        guard.Value = data;
    }
}
```

### 3. **Use Appropriate Lock Methods**

```csharp
// For critical operations - use blocking lock
var result = mutex.Lock();

// For optional operations - use try lock
var result = mutex.TryLock();
if (result.IsFailure)
{
    return; // Skip if busy
}

// For time-sensitive operations - use timeout
var result = mutex.TryLockTimeout(TimeSpan.FromSeconds(5));

// For async contexts - use async methods
var result = await mutex.LockAsync(cancellationToken);
```

### 4. **Handle Errors Appropriately**

```csharp
var result = mutex.Lock();
result.Match(
    success: guard =>
    {
        using (guard)
        {
            // Success path
        }
        return "Done";
    },
    failure: error =>
    {
        // Log error with context
        logger.Error($"Lock failed: {error.GetFullMessage()}");
        
        // Take appropriate action based on error kind
        return error.Kind switch
        {
            ErrorKind.Timeout => "Retry later",
            ErrorKind.Interrupted => "Cancelled",
            _ => "Failed"
        };
    }
);
```

### 5. **Prefer Functional Operations**

```csharp
// Good - functional style
var result = mutex.Lock()
    .Map(guard =>
    {
        using (guard)
        {
            guard.Update(x => x * 2);
            return guard.Value;
        }
    });

// Acceptable but less functional
var result = mutex.Lock();
if (result.TryGetValue(out var guard))
{
    using (guard)
    {
        guard.Value = guard.Value * 2;
        return guard.Value;
    }
}
```

### 6. **Avoid Deadlocks**

```csharp
// Bad - potential deadlock
var result1 = mutex1.Lock();
var result2 = mutex2.Lock(); // Might deadlock if another thread locks in opposite order

// Better - use timeout
var result1 = mutex1.TryLockTimeout(TimeSpan.FromSeconds(5));
var result2 = mutex2.TryLockTimeout(TimeSpan.FromSeconds(5));

// Best - consistent lock ordering
// Always acquire mutexes in the same order across all threads
```

---

## Performance Considerations

### Memory Overhead

- **Mutex<T>**: ~40-48 bytes + size of T
  - `SemaphoreSlim`: ~32 bytes
  - Value: sizeof(T)
  - Flags and references: ~16 bytes

- **MutexGuard<T>**: ~32-40 bytes
  - References: 16 bytes
  - Value copy: sizeof(T)
  - Flags: 8 bytes

### Operation Costs

| Operation | Cost | Notes |
|-----------|------|-------|
| `Lock()` | O(1) + blocking | Blocks if lock held |
| `TryLock()` | O(1) | Non-blocking, immediate return |
| `TryLockTimeout()` | O(1) + blocking up to timeout | Returns on timeout |
| `LockAsync()` | O(1) + async wait | Async-friendly, no thread blocking |
| `LockAsyncTimeout()` | O(1) + async wait up to timeout | Combines async and timeout |
| Guard disposal | O(1) | Releases semaphore |

### Comparison with Alternatives

| Approach | Pros | Cons |
|----------|------|------|
| `Mutex<T>` | Type-safe, explicit errors, RAII | Slight overhead from Result |
| `lock` statement | Simple, well-known | Not async-friendly, no timeouts |
| `SemaphoreSlim` | Low-level control | Manual management, error-prone |
| `Monitor` | Very fast | No async support, manual management |

### Performance Tips

1. **Prefer async methods in async contexts**:
   ```csharp
   // Good - uses async properly
   var result = await mutex.LockAsync();
   
   // Bad - blocks thread in async method
   var result = mutex.Lock();
   ```

2. **Use TryLock for optional operations**:
   ```csharp
   // Skip if busy - better throughput
   var result = mutex.TryLock();
   if (result.IsSuccess) { /* do work */ }
   ```

3. **Batch operations when possible**:
   ```csharp
   // Good - one lock for multiple operations
   using var guard = mutex.Lock().Unwrap();
   guard.Value.Add(item1);
   guard.Value.Add(item2);
   guard.Value.Add(item3);
   
   // Bad - multiple locks
   mutex.Lock().Unwrap().Value.Add(item1);
   mutex.Lock().Unwrap().Value.Add(item2);
   mutex.Lock().Unwrap().Value.Add(item3);
   ```

---

## Summary

The `Mutex<T>` type provides:

- **Type-Safe Concurrency** - Compiler-enforced exclusive access
- **Explicit Error Handling** - Result-based API for failures
- **RAII Lock Management** - Automatic release via disposal
- **Async Support** - Full async/await integration
- **Flexible Locking** - Blocking, try, timeout, and async variants
- **Functional Operations** - Map, Update, and more
- **Rich Error Context** - Detailed error information with metadata

Use `Mutex<T>` when you need:
- Thread-safe access to shared mutable data
- Explicit error handling for lock operations
- Async-compatible locking
- Integration with Result-based code

---

**See Also:**
- [Result Type Documentation](../ERROR_TYPE.md)
- [Error Type Documentation](../ERROR_TYPE.md)
- [Async Extensions](ASYNC_EXTENSIONS.md)

---

**Version:** 1.2.1  
**Status:** Production Ready  
**Test Coverage:** 36 tests, 100% passing
