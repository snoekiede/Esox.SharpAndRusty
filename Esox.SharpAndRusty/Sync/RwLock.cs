using System.Diagnostics.CodeAnalysis;
using Esox.SharpAndRusty.Types;

namespace Esox.SharpAndRusty.Sync;

/// <summary>
///     A reader-writer lock for protecting shared data, inspired by Rust's std::sync::RwLock.
///     This type allows multiple concurrent readers or a single writer, providing interior mutability
///     with shared/exclusive access semantics and integrates with Result/Error types.
/// </summary>
/// <typeparam name="T">The type of the value protected by the RwLock.</typeparam>
/// <remarks>
///     Unlike Rust's RwLock which relies on compile-time borrow checking, this C# implementation uses
///     runtime locks and returns Result types to handle lock acquisition failures gracefully.
///     The RwLock uses ReaderWriterLockSlim internally for efficient reader/writer semantics.
///     Multiple readers can access the data concurrently, but writers have exclusive access.
///
///     <para>
///         <b>Synchronous only.</b> There is no async API. Guards (<see cref="ReadGuard{T}"/> and
///         <see cref="WriteGuard{T}"/>) must never be held across an <c>await</c>. Doing so keeps
///         the underlying OS-level read/write lock held while the thread-pool thread is returned,
///         which blocks any writer (or further readers, once a writer is waiting) and will deadlock
///         once the continuation tries to re-enter the lock on a different thread.
///     </para>
///
///     <para>
///         <b>Known disposal limitation.</b> If a thread is blocked inside
///         <see cref="Read"/> or <see cref="Write"/> (i.e. it is waiting for the lock to become
///         available) at the exact moment <see cref="Dispose"/> is called on another thread,
///         the behaviour is unspecified by <see cref="ReaderWriterLockSlim"/> itself — Microsoft's
///         documentation explicitly states that disposing the lock while other threads are engaged
///         with it is unsupported. In practice this is likely to surface as an
///         <see cref="ObjectDisposedException"/>, which the callers' catch blocks handle, but it is
///         not guaranteed. The common and safe pattern is to ensure all work that may acquire this
///         lock has completed before calling <see cref="Dispose"/> — for example at application
///         shutdown or after draining a work queue. <see cref="Dispose"/> does wait for all
///         <i>live guards</i> (successfully acquired locks that have not yet been released) to drain
///         before tearing down the inner lock, closing the ordinary race of "dispose while a guard
///         is still held"; it is only the narrower "dispose while a thread is blocked trying to
///         acquire" case that cannot be handled safely with this primitive.
///     </para>
/// </remarks>
public sealed class RwLock<T> : IDisposable
{
    private readonly ReaderWriterLockSlim _lock;
    private int _disposed;
    private int _activeGuards; // count of live (successfully acquired, not yet released) guards
    private T _value;

    /// <summary>
    ///     Creates a new RwLock in an unlocked state ready for use.
    /// </summary>
    /// <param name="value">The initial value to protect.</param>
    /// <param name="recursionPolicy">
    ///     The lock recursion policy. Defaults to NoRecursion for better performance and deadlock
    ///     prevention.
    /// </param>
    public RwLock(T value, LockRecursionPolicy recursionPolicy = LockRecursionPolicy.NoRecursion)
    {
        _value = value;
        _lock = new ReaderWriterLockSlim(recursionPolicy);
        _disposed = 0;
    }

    /// <summary>
    ///     Gets whether this RwLock has been disposed.
    /// </summary>
    public bool IsDisposed => Volatile.Read(ref _disposed) == 1;

    /// <summary>
    ///     Gets whether the read lock is held by the current thread.
    ///     Useful for diagnostics and debugging deadlock situations.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global",
        Justification = "Will be used vy clients of this nuget package")]
    public bool IsReadLockHeld => Volatile.Read(ref _disposed) == 0 && _lock.IsReadLockHeld;

    /// <summary>
    ///     Gets whether the write lock is held by the current thread.
    ///     Useful for diagnostics and debugging deadlock situations.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global",
        Justification = "Will be used vy clients of this nuget package")]
    public bool IsWriteLockHeld => Volatile.Read(ref _disposed) == 0 && _lock.IsWriteLockHeld;

    /// <summary>
    ///     Gets the total number of unique threads that have entered read mode.
    ///     Useful for monitoring concurrent access patterns.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global",
        Justification = "Will be used vy clients of this nuget package")]
    public int CurrentReadCount => Volatile.Read(ref _disposed) == 0 ? _lock.CurrentReadCount : 0;

    /// <summary>
    ///     Releases all resources used by the RwLock.
    /// </summary>
    /// <remarks>
    ///     Waits up to 5 seconds for all currently-held guards to be released before disposing
    ///     the inner <see cref="ReaderWriterLockSlim"/>. This closes the common race of
    ///     "dispose while another thread still holds a live guard". It does <i>not</i> handle
    ///     the narrower case where a thread is blocked <i>trying to acquire</i> the lock at the
    ///     moment of disposal — see the class-level remarks for the full explanation.
    /// </remarks>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 1)
        {
            return;
        }

        // Drain live guards before tearing down the inner lock. SpinWait escalates from pure
        // CPU spinning to Thread.Yield() / Thread.Sleep() automatically, ensuring the holder
        // thread gets CPU time to call guard.Dispose() even on a loaded or single-core machine.
        var deadline = Environment.TickCount64 + 5_000;
        var spinner = new SpinWait();
        while (Volatile.Read(ref _activeGuards) > 0 && Environment.TickCount64 < deadline)
        {
            spinner.SpinOnce();
        }

        try
        {
            _lock.Dispose();
        }
        catch (SynchronizationLockException)
        {
            // ReaderWriterLockSlim.Dispose() throws SynchronizationLockException if a thread is
            // inside EnterReadLock/EnterWriteLock/TryEnterReadLock/TryEnterWriteLock at the exact
            // moment we dispose. This is the documented "blocked-on-entry during Dispose" limitation
            // described in the class remarks. The drain-wait above covers live guards; this catch
            // covers the narrower race that we cannot eliminate without a cancellable wait primitive.
            // Those threads will receive an ObjectDisposedException from the inner lock, which all
            // public lock methods already handle via their catch blocks.
        }
    }

    /// <summary>
    ///     Acquires a read lock, blocking the current thread until it is able to do so.
    ///     Multiple readers can hold the lock simultaneously.
    ///     Returns a ReadGuard that provides read-only access to the protected data.
    /// </summary>
    /// <returns>
    ///     A Result containing a ReadGuard on success, or an Error if the lock is disposed.
    /// </returns>
    /// <example>
    ///     <code>
    /// var rwlock = new RwLock&lt;int&gt;(42);
    /// 
    /// var result = rwlock.Read();
    /// if (result.TryGetValue(out var guard))
    /// {
    ///     using (guard)
    ///     {
    ///         Console.WriteLine(guard.Value); // Read-only access
    ///     }
    /// }
    /// </code>
    /// </example>
    public Result<ReadGuard<T>, Error> Read()
    {
        try
        {
            _lock.EnterReadLock();
            Interlocked.Increment(ref _activeGuards);

            if (Volatile.Read(ref _disposed) == 1)
            {
                Interlocked.Decrement(ref _activeGuards);
                _lock.ExitReadLock();
                return Result<ReadGuard<T>, Error>.Err(
                    Error.New("Cannot read from disposed RwLock", ErrorKind.InvalidOperation)
                );
            }

            return Result<ReadGuard<T>, Error>.Ok(
                new ReadGuard<T>(this, _lock)
            );
        }
        catch (ObjectDisposedException)
        {
            return Result<ReadGuard<T>, Error>.Err(
                Error.New("RwLock was disposed during read lock acquisition", ErrorKind.InvalidOperation)
            );
        }
        catch (Exception ex)
        {
            return Result<ReadGuard<T>, Error>.Err(
                Error.FromException(ex).WithContext("Failed to acquire read lock")
            );
        }
    }

    /// <summary>
    ///     Attempts to acquire a read lock without blocking.
    ///     If the lock cannot be acquired immediately, returns an error.
    /// </summary>
    /// <returns>
    ///     A Result containing a ReadGuard if successful, or an Error if the lock could not be acquired.
    /// </returns>
    /// <example>
    ///     <code>
    /// var rwlock = new RwLock&lt;int&gt;(42);
    /// 
    /// var result = rwlock.TryRead();
    /// result.Match(
    ///     success: guard =>
    ///     {
    ///         using (guard)
    ///         {
    ///             Console.WriteLine(guard.Value);
    ///         }
    ///         return "Read";
    ///     },
    ///     failure: error => "Lock busy"
    /// );
    /// </code>
    /// </example>
    public Result<ReadGuard<T>, Error> TryRead()
    {
        try
        {
            if (_lock.TryEnterReadLock(0))
            {
                Interlocked.Increment(ref _activeGuards);

                if (Volatile.Read(ref _disposed) == 1)
                {
                    Interlocked.Decrement(ref _activeGuards);
                    _lock.ExitReadLock();
                    return Result<ReadGuard<T>, Error>.Err(
                        Error.New("Cannot read from disposed RwLock", ErrorKind.InvalidOperation)
                    );
                }

                return Result<ReadGuard<T>, Error>.Ok(
                    new ReadGuard<T>(this, _lock)
                );
            }

            return Result<ReadGuard<T>, Error>.Err(
                Error.New("RwLock read lock is currently unavailable", ErrorKind.ResourceExhausted)
                    .WithMetadata("lockAttemptTime", DateTime.UtcNow)
            );
        }
        catch (LockRecursionException ex)
        {
            return Result<ReadGuard<T>, Error>.Err(
                Error.FromException(ex)
                    .WithContext("Lock recursion not allowed")
                    .WithKind(ErrorKind.InvalidOperation)
            );
        }
        catch (ObjectDisposedException)
        {
            return Result<ReadGuard<T>, Error>.Err(
                Error.New("RwLock was disposed during read lock acquisition", ErrorKind.InvalidOperation)
            );
        }
        catch (Exception ex)
        {
            return Result<ReadGuard<T>, Error>.Err(
                Error.FromException(ex).WithContext("Failed to try-read from RwLock")
            );
        }
    }

    /// <summary>
    ///     Attempts to acquire a read lock with a timeout.
    /// </summary>
    /// <param name="timeout">The maximum time to wait for the lock.</param>
    /// <returns>
    ///     A Result containing a ReadGuard if successful, or an Error if the timeout expired.
    /// </returns>
    /// <example>
    ///     <code>
    /// var rwlock = new RwLock&lt;int&gt;(42);
    /// 
    /// var result = rwlock.TryReadTimeout(TimeSpan.FromSeconds(5));
    /// if (result.TryGetValue(out var guard))
    /// {
    ///     using (guard)
    ///     {
    ///         Console.WriteLine(guard.Value);
    ///     }
    /// }
    /// </code>
    /// </example>
    public Result<ReadGuard<T>, Error> TryReadTimeout(TimeSpan timeout)
    {
        try
        {
            if (_lock.TryEnterReadLock(timeout))
            {
                Interlocked.Increment(ref _activeGuards);

                if (Volatile.Read(ref _disposed) == 1)
                {
                    Interlocked.Decrement(ref _activeGuards);
                    _lock.ExitReadLock();
                    return Result<ReadGuard<T>, Error>.Err(
                        Error.New("Cannot read from disposed RwLock", ErrorKind.InvalidOperation)
                    );
                }

                return Result<ReadGuard<T>, Error>.Ok(
                    new ReadGuard<T>(this, _lock)
                );
            }

            return Result<ReadGuard<T>, Error>.Err(
                Error.New("RwLock read lock timeout expired", ErrorKind.Timeout)
                    .WithMetadata("timeout", timeout)
                    .WithMetadata("attemptTime", DateTime.UtcNow)
            );
        }
        catch (LockRecursionException ex)
        {
            return Result<ReadGuard<T>, Error>.Err(
                Error.FromException(ex)
                    .WithContext("Lock recursion not allowed")
                    .WithKind(ErrorKind.InvalidOperation)
            );
        }
        catch (ObjectDisposedException)
        {
            return Result<ReadGuard<T>, Error>.Err(
                Error.New("RwLock was disposed during read lock acquisition", ErrorKind.InvalidOperation)
            );
        }
        catch (Exception ex)
        {
            return Result<ReadGuard<T>, Error>.Err(
                Error.FromException(ex).WithContext("Failed to acquire read lock with timeout")
            );
        }
    }

    /// <summary>
    ///     Acquires a write lock, blocking the current thread until it is able to do so.
    ///     Only one writer can hold the lock, and no readers can access the data.
    ///     Returns a WriteGuard that provides mutable access to the protected data.
    /// </summary>
    /// <returns>
    ///     A Result containing a WriteGuard on success, or an Error if the lock is disposed.
    /// </returns>
    /// <example>
    ///     <code>
    /// var rwlock = new RwLock&lt;int&gt;(0);
    /// 
    /// var result = rwlock.Write();
    /// if (result.TryGetValue(out var guard))
    /// {
    ///     using (guard)
    ///     {
    ///         guard.Value = 42; // Exclusive write access
    ///     }
    /// }
    /// </code>
    /// </example>
    public Result<WriteGuard<T>, Error> Write()
    {
        try
        {
            _lock.EnterWriteLock();
            Interlocked.Increment(ref _activeGuards);

            if (Volatile.Read(ref _disposed) == 1)
            {
                Interlocked.Decrement(ref _activeGuards);
                _lock.ExitWriteLock();
                return Result<WriteGuard<T>, Error>.Err(
                    Error.New("Cannot write to disposed RwLock", ErrorKind.InvalidOperation)
                );
            }

            return Result<WriteGuard<T>, Error>.Ok(
                new WriteGuard<T>(this, _lock)
            );
        }
        catch (ObjectDisposedException)
        {
            return Result<WriteGuard<T>, Error>.Err(
                Error.New("RwLock was disposed during write lock acquisition", ErrorKind.InvalidOperation)
            );
        }
        catch (Exception ex)
        {
            return Result<WriteGuard<T>, Error>.Err(
                Error.FromException(ex).WithContext("Failed to acquire write lock")
            );
        }
    }

    /// <summary>
    ///     Attempts to acquire a write lock without blocking.
    ///     If the lock cannot be acquired immediately, returns an error.
    /// </summary>
    /// <returns>
    ///     A Result containing a WriteGuard if successful, or an Error if the lock could not be acquired.
    /// </returns>
    /// <example>
    ///     <code>
    /// var rwlock = new RwLock&lt;int&gt;(0);
    /// 
    /// var result = rwlock.TryWrite();
    /// result.Match(
    ///     success: guard =>
    ///     {
    ///         using (guard)
    ///         {
    ///             guard.Value++;
    ///         }
    ///         return "Updated";
    ///     },
    ///     failure: error => "Lock busy"
    /// );
    /// </code>
    /// </example>
    public Result<WriteGuard<T>, Error> TryWrite()
    {
        try
        {
            if (_lock.TryEnterWriteLock(0))
            {
                Interlocked.Increment(ref _activeGuards);

                if (Volatile.Read(ref _disposed) == 1)
                {
                    Interlocked.Decrement(ref _activeGuards);
                    _lock.ExitWriteLock();
                    return Result<WriteGuard<T>, Error>.Err(
                        Error.New("Cannot write to disposed RwLock", ErrorKind.InvalidOperation)
                    );
                }

                return Result<WriteGuard<T>, Error>.Ok(
                    new WriteGuard<T>(this, _lock)
                );
            }

            return Result<WriteGuard<T>, Error>.Err(
                Error.New("RwLock write lock is currently unavailable", ErrorKind.ResourceExhausted)
                    .WithMetadata("lockAttemptTime", DateTime.UtcNow)
            );
        }
        catch (LockRecursionException ex)
        {
            return Result<WriteGuard<T>, Error>.Err(
                Error.FromException(ex)
                    .WithContext("Lock recursion not allowed")
                    .WithKind(ErrorKind.InvalidOperation)
            );
        }
        catch (ObjectDisposedException)
        {
            return Result<WriteGuard<T>, Error>.Err(
                Error.New("RwLock was disposed during write lock acquisition", ErrorKind.InvalidOperation)
            );
        }
        catch (Exception ex)
        {
            return Result<WriteGuard<T>, Error>.Err(
                Error.FromException(ex).WithContext("Failed to try-write to RwLock")
            );
        }
    }

    /// <summary>
    ///     Attempts to acquire a write lock with a timeout.
    /// </summary>
    /// <param name="timeout">The maximum time to wait for the lock.</param>
    /// <returns>
    ///     A Result containing a WriteGuard if successful, or an Error if the timeout expired.
    /// </returns>
    /// <example>
    ///     <code>
    /// var rwlock = new RwLock&lt;int&gt;(0);
    /// 
    /// var result = rwlock.TryWriteTimeout(TimeSpan.FromSeconds(5));
    /// if (result.TryGetValue(out var guard))
    /// {
    ///     using (guard)
    ///     {
    ///         guard.Value = 42;
    ///     }
    /// }
    /// </code>
    /// </example>
    public Result<WriteGuard<T>, Error> TryWriteTimeout(TimeSpan timeout)
    {
        try
        {
            if (_lock.TryEnterWriteLock(timeout))
            {
                Interlocked.Increment(ref _activeGuards);

                if (Volatile.Read(ref _disposed) == 1)
                {
                    Interlocked.Decrement(ref _activeGuards);
                    _lock.ExitWriteLock();
                    return Result<WriteGuard<T>, Error>.Err(
                        Error.New("Cannot write to disposed RwLock", ErrorKind.InvalidOperation)
                    );
                }

                return Result<WriteGuard<T>, Error>.Ok(
                    new WriteGuard<T>(this, _lock)
                );
            }

            return Result<WriteGuard<T>, Error>.Err(
                Error.New("RwLock write lock timeout expired", ErrorKind.Timeout)
                    .WithMetadata("timeout", timeout)
                    .WithMetadata("attemptTime", DateTime.UtcNow)
            );
        }
        catch (LockRecursionException ex)
        {
            return Result<WriteGuard<T>, Error>.Err(
                Error.FromException(ex)
                    .WithContext("Lock recursion not allowed")
                    .WithKind(ErrorKind.InvalidOperation)
            );
        }
        catch (ObjectDisposedException)
        {
            return Result<WriteGuard<T>, Error>.Err(
                Error.New("RwLock was disposed during write lock acquisition", ErrorKind.InvalidOperation)
            );
        }
        catch (Exception ex)
        {
            return Result<WriteGuard<T>, Error>.Err(
                Error.FromException(ex).WithContext("Failed to acquire write lock with timeout")
            );
        }
    }

    /// <summary>
    ///     Consumes the RwLock, returning the underlying data.
    /// </summary>
    /// <returns>
    ///     A Result containing the underlying value on success, or an Error if the lock is already
    ///     disposed or if exclusive access cannot be obtained within 5 seconds.
    /// </returns>
    /// <remarks>
    ///     Inspired by Rust's <c>into_inner()</c>. Unlike Rust, C# has no ownership model, so this
    ///     method cannot statically guarantee that the caller is the sole owner. Instead it attempts
    ///     to acquire an exclusive write lock (waiting up to 5 seconds) to ensure no reader or
    ///     writer guard is currently active on any thread. If the write lock is granted the value is
    ///     extracted, the lock is released, and the RwLock is disposed.
    ///
    ///     <para>
    ///         <b>Cross-thread safety:</b> it is correct to call this method from a different thread
    ///         to the one that originally created the RwLock, provided all live guards have been
    ///         disposed first — the write-lock attempt will succeed once the last guard is released.
    ///         If a guard is held indefinitely on another thread the 5-second timeout will expire
    ///         and an error is returned; the RwLock is <i>not</i> disposed in that case.
    ///     </para>
    /// </remarks>
    /// <example>
    ///     <code>
    /// var rwlock = new RwLock&lt;int&gt;(42);
    /// var result = rwlock.IntoInner();
    /// if (result.TryGetValue(out var value))
    /// {
    ///     Console.WriteLine(value); // rwlock is now disposed
    /// }
    /// </code>
    /// </example>
    public Result<T, Error> IntoInner()
    {
        if (IsDisposed)
            return Result<T, Error>.Err(
                Error.New("Cannot extract value from disposed RwLock", ErrorKind.InvalidOperation));

        try
        {
            if (!_lock.TryEnterWriteLock(TimeSpan.FromSeconds(5)))
                return Result<T, Error>.Err(
                    Error.New("Cannot extract value while locks are held elsewhere", ErrorKind.InvalidOperation));
        }
        catch (LockRecursionException ex)
        {
            return Result<T, Error>.Err(
                Error.FromException(ex)
                    .WithContext("Cannot call IntoInner while a read lock is held on the same thread")
                    .WithKind(ErrorKind.InvalidOperation));
        }
        catch (ObjectDisposedException)
        {
            return Result<T, Error>.Err(
                Error.New("RwLock was disposed during IntoInner write lock acquisition", ErrorKind.InvalidOperation));
        }

        try
        {
            var value = _value;
            return Result<T, Error>.Ok(value);
        }
        finally
        {
            _lock.ExitWriteLock();
            Dispose();
        }
    }

    // Internal method for the write guard to update the value
    internal void UpdateValue(T value)
    {
        _value = value;
    }

    // Internal method for guards to get the current value
    internal T GetValue() => _value;

    // Called by ReadGuard and WriteGuard when they are disposed to signal that a live guard has
    // been released. Dispose() waits for this count to reach zero before tearing down the inner lock.
    internal void DecrementActiveGuards() => Interlocked.Decrement(ref _activeGuards);
}

/// <summary>
///     An RAII guard that provides read-only access to the data protected by an RwLock.
///     The read lock is automatically released when the guard is disposed.
///     Multiple ReadGuards can exist simultaneously.
/// </summary>
/// <typeparam name="T">The type of the value protected by the RwLock.</typeparam>
/// <remarks>
///     This type is inspired by Rust's RwLockReadGuard and provides automatic lock release through the IDisposable
///     pattern.
///     Always use within a using statement or dispose explicitly to ensure the lock is released.
/// </remarks>
public sealed class ReadGuard<T> : IDisposable
{
    private readonly ReaderWriterLockSlim _lock;
    private readonly RwLock<T> _rwlock;
    private readonly T _value;
    private bool _disposed;

    internal ReadGuard(RwLock<T> rwlock, ReaderWriterLockSlim lockSlim)
    {
        _rwlock = rwlock;
        _value = rwlock.GetValue();
        _lock = lockSlim;
        _disposed = false;
    }

    /// <summary>
    ///     Gets the value protected by the RwLock (read-only access).
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown if the guard has been disposed.</exception>
    /// <example>
    ///     <code>
    /// var rwlock = new RwLock&lt;int&gt;(42);
    /// using var guard = rwlock.Read().Unwrap();
    /// 
    /// int value = guard.Value; // Read-only access
    /// </code>
    /// </example>
    public T Value
    {
        get
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ReadGuard<>), "Cannot access value from disposed guard");
            return _value;
        }
    }

    /// <summary>
    ///     Releases the read lock.
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;

            try
            {
                _lock.ExitReadLock();
            }
            catch (SynchronizationLockException)
            {
                // Lock was already released or not held
                // This is acceptable in cleanup scenarios
            }
            catch (ObjectDisposedException)
            {
                // RwLock was disposed while we held the guard
                // This is acceptable - the lock cleanup will handle it
            }

            _rwlock.DecrementActiveGuards();
        }
    }

    /// <summary>
    ///     Maps a function over the guarded value, returning the result of the function.
    ///     The guard remains locked during the function execution.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="mapper">The function to apply to the guarded value.</param>
    /// <returns>The result of applying the function.</returns>
    /// <exception cref="ArgumentNullException">Thrown if mapper is null.</exception>
    /// <exception cref="ObjectDisposedException">Thrown if the guard has been disposed.</exception>
    /// <example>
    ///     <code>
    /// var rwlock = new RwLock&lt;int&gt;(42);
    /// using var guard = rwlock.Read().Unwrap();
    /// 
    /// string result = guard.Map(x => $"Value is {x}");
    /// </code>
    /// </example>
    public TResult Map<TResult>(Func<T, TResult> mapper)
    {
        if (mapper is null) throw new ArgumentNullException(nameof(mapper));
        if (_disposed) throw new ObjectDisposedException(nameof(ReadGuard<>), "Cannot map over disposed guard");

        return mapper(_value);
    }
}

/// <summary>
///     An RAII guard that provides exclusive write access to the data protected by an RwLock.
///     The write lock is automatically released when the guard is disposed.
///     Only one WriteGuard can exist at a time, and no ReadGuards can be active.
/// </summary>
/// <typeparam name="T">The type of the value protected by the RwLock.</typeparam>
/// <remarks>
///     This type is inspired by Rust's RwLockWriteGuard and provides automatic lock release through the IDisposable
///     pattern.
///     Always use within a using statement or dispose explicitly to ensure the lock is released.
/// </remarks>
public sealed class WriteGuard<T> : IDisposable
{
    private readonly ReaderWriterLockSlim _lock;
    private readonly RwLock<T> _rwlock;
    private bool _disposed;
    private T _value;

    internal WriteGuard(RwLock<T> rwlock, ReaderWriterLockSlim lockSlim)
    {
        _rwlock = rwlock;
        _value = rwlock.GetValue();
        _lock = lockSlim;
        _disposed = false;
    }

    /// <summary>
    ///     Gets or sets the value protected by the RwLock.
    ///     This property provides exclusive mutable access to the protected data while the guard is held.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown if the guard has been disposed.</exception>
    /// <example>
    ///     <code>
    /// var rwlock = new RwLock&lt;int&gt;(0);
    /// using var guard = rwlock.Write().Unwrap();
    /// 
    /// // Read the value
    /// int current = guard.Value;
    /// 
    /// // Modify the value
    /// guard.Value = current + 1;
    /// </code>
    /// </example>
    public T Value
    {
        get
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(WriteGuard<>), "Cannot access value from disposed guard");
            return _value;
        }
        set
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(WriteGuard<>), "Cannot modify value from disposed guard");
            _value = value;
        }
    }

    /// <summary>
    ///     Releases the write lock by writing back the modified value and releasing the lock.
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            // Write the value back to the RwLock before releasing the lock
            // This ensures any modifications are persisted
            try
            {
                _rwlock.UpdateValue(_value);
            }
            catch (ObjectDisposedException)
            {
                // RwLock was disposed while we held the guard
                // Value updates are lost but that's acceptable
            }

            _disposed = true;

            try
            {
                _lock.ExitWriteLock();
            }
            catch (SynchronizationLockException)
            {
                // Lock was already released or not held
                // This is acceptable in cleanup scenarios
            }
            catch (ObjectDisposedException)
            {
                // RwLock was disposed while we held the guard
                // This is acceptable - the lock cleanup will handle it
            }

            _rwlock.DecrementActiveGuards();
        }
    }

    /// <summary>
    ///     Maps a function over the guarded value, returning the result of the function.
    ///     The guard remains locked during the function execution.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="mapper">The function to apply to the guarded value.</param>
    /// <returns>The result of applying the function.</returns>
    /// <exception cref="ArgumentNullException">Thrown if mapper is null.</exception>
    /// <exception cref="ObjectDisposedException">Thrown if the guard has been disposed.</exception>
    /// <example>
    ///     <code>
    /// var rwlock = new RwLock&lt;int&gt;(42);
    /// using var guard = rwlock.Write().Unwrap();
    /// 
    /// string result = guard.Map(x => $"Value is {x}");
    /// </code>
    /// </example>
    public TResult Map<TResult>(Func<T, TResult> mapper)
    {
        if (mapper is null) throw new ArgumentNullException(nameof(mapper));
        if (_disposed) throw new ObjectDisposedException(nameof(WriteGuard<>), "Cannot map over disposed guard");

        return mapper(_value);
    }

    /// <summary>
    ///     Applies a function to the guarded value and updates it with the result.
    ///     The guard remains locked during the function execution.
    /// </summary>
    /// <param name="updater">The function to transform the guarded value.</param>
    /// <exception cref="ArgumentNullException">Thrown if updater is null.</exception>
    /// <exception cref="ObjectDisposedException">Thrown if the guard has been disposed.</exception>
    /// <example>
    ///     <code>
    /// var rwlock = new RwLock&lt;int&gt;(42);
    /// using var guard = rwlock.Write().Unwrap();
    /// 
    /// guard.Update(x => x * 2); // Value is now 84
    /// </code>
    /// </example>
    public void Update(Func<T, T> updater)
    {
        if (updater is null) throw new ArgumentNullException(nameof(updater));
        if (_disposed) throw new ObjectDisposedException(nameof(WriteGuard<>), "Cannot update disposed guard");

        _value = updater(_value);
    }
}