using Esox.SharpAndRusty.Types;

namespace Esox.SharpAndRusty.Sync
{
    /// <summary>
    /// A mutual exclusion primitive useful for protecting shared data, inspired by Rust's std::sync::Mutex.
    /// This type provides interior mutability with exclusive access semantics and integrates with Result/Error types.
    /// Works in both synchronous and asynchronous contexts.
    /// </summary>
    /// <typeparam name="T">The type of the value protected by the mutex.</typeparam>
    /// <remarks>
    /// Unlike Rust's Mutex which relies on compile-time borrow checking, this C# implementation uses
    /// runtime locks and returns Result types to handle lock acquisition failures gracefully.
    /// 
    /// The mutex uses SemaphoreSlim internally for async-compatible locking and proper disposal semantics.
    /// Both synchronous (Lock, TryLock) and asynchronous (LockAsync, LockAsyncTimeout) methods are available.
    /// </remarks>
    public sealed class Mutex<T> : IDisposable
    {
        private readonly SemaphoreSlim _semaphore;
        private T _value;
        private bool _disposed;

        /// <summary>
        /// Creates a new mutex in an unlocked state ready for use.
        /// </summary>
        /// <param name="value">The initial value to protect.</param>
        public Mutex(T value)
        {
            _value = value;
            _semaphore = new SemaphoreSlim(1, 1);
            _disposed = false;
        }

        /// <summary>
        /// Acquires the mutex, blocking the current thread until it is able to do so.
        /// Returns a MutexGuard that provides access to the protected data and automatically releases the lock when disposed.
        /// </summary>
        /// <returns>
        /// A Result containing a MutexGuard on success, or an Error if the mutex is disposed or poisoned.
        /// </returns>
        /// <example>
        /// <code>
        /// var mutex = new Mutex&lt;int&gt;(0);
        /// 
        /// var result = mutex.Lock();
        /// if (result.TryGetValue(out var guard))
        /// {
        ///     using (guard)
        ///     {
        ///         guard.Value = 42;
        ///     } // Lock automatically released
        /// }
        /// </code>
        /// </example>
        public Result<MutexGuard<T>, Error> Lock()
        {
            if (_disposed)
            {
                return Result<MutexGuard<T>, Error>.Err(
                    Error.New("Cannot lock disposed mutex", ErrorKind.InvalidOperation)
                );
            }

            try
            {
                _semaphore.Wait();
                return Result<MutexGuard<T>, Error>.Ok(
                    new MutexGuard<T>(this, _semaphore)
                );
            }
            catch (ObjectDisposedException)
            {
                return Result<MutexGuard<T>, Error>.Err(
                    Error.New("Mutex was disposed during lock acquisition", ErrorKind.InvalidOperation)
                );
            }
            catch (Exception ex)
            {
                return Result<MutexGuard<T>, Error>.Err(
                    Error.FromException(ex).WithContext("Failed to acquire mutex lock")
                );
            }
        }

        /// <summary>
        /// Attempts to acquire the mutex without blocking.
        /// If the mutex is currently locked, returns immediately with an error.
        /// </summary>
        /// <returns>
        /// A Result containing a MutexGuard if successful, or an Error if the lock could not be acquired.
        /// </returns>
        /// <example>
        /// <code>
        /// var mutex = new Mutex&lt;int&gt;(0);
        /// 
        /// var result = mutex.TryLock();
        /// result.Match(
        ///     success: guard =>
        ///     {
        ///         using (guard)
        ///         {
        ///             guard.Value++;
        ///         }
        ///         return "Updated";
        ///     },
        ///     failure: error => $"Lock busy: {error.Message}"
        /// );
        /// </code>
        /// </example>
        public Result<MutexGuard<T>, Error> TryLock()
        {
            if (_disposed)
            {
                return Result<MutexGuard<T>, Error>.Err(
                    Error.New("Cannot lock disposed mutex", ErrorKind.InvalidOperation)
                );
            }

            try
            {
                if (_semaphore.Wait(0))
                {
                    return Result<MutexGuard<T>, Error>.Ok(
                        new MutexGuard<T>(this, _semaphore)
                    );
                }
                else
                {
                    return Result<MutexGuard<T>, Error>.Err(
                        Error.New("Mutex is currently locked", ErrorKind.ResourceExhausted)
                            .WithMetadata("lockAttemptTime", DateTime.UtcNow)
                    );
                }
            }
            catch (ObjectDisposedException)
            {
                return Result<MutexGuard<T>, Error>.Err(
                    Error.New("Mutex was disposed during lock acquisition", ErrorKind.InvalidOperation)
                );
            }
            catch (Exception ex)
            {
                return Result<MutexGuard<T>, Error>.Err(
                    Error.FromException(ex).WithContext("Failed to try-lock mutex")
                );
            }
        }

        /// <summary>
        /// Attempts to acquire the mutex, blocking until the specified timeout expires.
        /// </summary>
        /// <param name="timeout">The maximum time to wait for the lock.</param>
        /// <returns>
        /// A Result containing a MutexGuard if successful, or an Error if the timeout expired or lock failed.
        /// </returns>
        /// <example>
        /// <code>
        /// var mutex = new Mutex&lt;int&gt;(0);
        /// 
        /// var result = mutex.TryLockTimeout(TimeSpan.FromSeconds(5));
        /// if (result.TryGetValue(out var guard))
        /// {
        ///     using (guard)
        ///     {
        ///         // Have exclusive access for up to 5 seconds
        ///         guard.Value = 42;
        ///     }
        /// }
        /// </code>
        /// </example>
        public Result<MutexGuard<T>, Error> TryLockTimeout(TimeSpan timeout)
        {
            if (_disposed)
            {
                return Result<MutexGuard<T>, Error>.Err(
                    Error.New("Cannot lock disposed mutex", ErrorKind.InvalidOperation)
                );
            }

            try
            {
                if (_semaphore.Wait(timeout))
                {
                    return Result<MutexGuard<T>, Error>.Ok(
                        new MutexGuard<T>(this, _semaphore)
                    );
                }
                else
                {
                    return Result<MutexGuard<T>, Error>.Err(
                        Error.New("Mutex lock timeout expired", ErrorKind.Timeout)
                            .WithMetadata("timeout", timeout)
                            .WithMetadata("attemptTime", DateTime.UtcNow)
                    );
                }
            }
            catch (ObjectDisposedException)
            {
                return Result<MutexGuard<T>, Error>.Err(
                    Error.New("Mutex was disposed during lock acquisition", ErrorKind.InvalidOperation)
                );
            }
            catch (Exception ex)
            {
                return Result<MutexGuard<T>, Error>.Err(
                    Error.FromException(ex).WithContext("Failed to acquire mutex lock with timeout")
                );
            }
        }

        /// <summary>
        /// Asynchronously acquires the mutex, awaiting until it is available.
        /// Returns a MutexGuard that provides access to the protected data and automatically releases the lock when disposed.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to cancel the lock acquisition.</param>
        /// <returns>
        /// A Task containing a Result with a MutexGuard on success, or an Error if the operation was cancelled or failed.
        /// </returns>
        /// <example>
        /// <code>
        /// var mutex = new Mutex&lt;int&gt;(0);
        /// 
        /// var result = await mutex.LockAsync();
        /// if (result.TryGetValue(out var guard))
        /// {
        ///     using (guard)
        ///     {
        ///         await DoWorkAsync(guard.Value);
        ///         guard.Value++;
        ///     }
        /// }
        /// </code>
        /// </example>
        public async Task<Result<MutexGuard<T>, Error>> LockAsync(CancellationToken cancellationToken = default)
        {
            if (_disposed)
            {
                return Result<MutexGuard<T>, Error>.Err(
                    Error.New("Cannot lock disposed mutex", ErrorKind.InvalidOperation)
                );
            }

            try
            {
                await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
                return Result<MutexGuard<T>, Error>.Ok(
                    new MutexGuard<T>(this, _semaphore)
                );
            }
            catch (OperationCanceledException)
            {
                return Result<MutexGuard<T>, Error>.Err(
                    Error.New("Mutex lock was cancelled", ErrorKind.Interrupted)
                );
            }
            catch (ObjectDisposedException)
            {
                return Result<MutexGuard<T>, Error>.Err(
                    Error.New("Mutex was disposed during lock acquisition", ErrorKind.InvalidOperation)
                );
            }
            catch (Exception ex)
            {
                return Result<MutexGuard<T>, Error>.Err(
                    Error.FromException(ex).WithContext("Failed to acquire mutex lock asynchronously")
                );
            }
        }

        /// <summary>
        /// Asynchronously attempts to acquire the mutex with a specified timeout.
        /// </summary>
        /// <param name="timeout">The maximum time to wait for the lock.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the lock acquisition.</param>
        /// <returns>
        /// A Task containing a Result with a MutexGuard if successful, or an Error if the timeout expired or operation was cancelled.
        /// </returns>
        /// <example>
        /// <code>
        /// var mutex = new Mutex&lt;int&gt;(0);
        /// var cts = new CancellationTokenSource();
        /// 
        /// var result = await mutex.LockAsyncTimeout(TimeSpan.FromSeconds(5), cts.Token);
        /// result.Match(
        ///     success: guard =>
        ///     {
        ///         using (guard)
        ///         {
        ///             guard.Value = 42;
        ///         }
        ///         return "Success";
        ///     },
        ///     failure: error => $"Failed: {error.Message}"
        /// );
        /// </code>
        /// </example>
        public async Task<Result<MutexGuard<T>, Error>> LockAsyncTimeout(
            TimeSpan timeout,
            CancellationToken cancellationToken = default)
        {
            if (_disposed)
            {
                return Result<MutexGuard<T>, Error>.Err(
                    Error.New("Cannot lock disposed mutex", ErrorKind.InvalidOperation)
                );
            }

            try
            {
                if (await _semaphore.WaitAsync(timeout, cancellationToken).ConfigureAwait(false))
                {
                    return Result<MutexGuard<T>, Error>.Ok(
                        new MutexGuard<T>(this, _semaphore)
                    );
                }
                else
                {
                    return Result<MutexGuard<T>, Error>.Err(
                        Error.New("Mutex lock timeout expired", ErrorKind.Timeout)
                            .WithMetadata("timeout", timeout)
                            .WithMetadata("attemptTime", DateTime.UtcNow)
                    );
                }
            }
            catch (OperationCanceledException)
            {
                return Result<MutexGuard<T>, Error>.Err(
                    Error.New("Mutex lock was cancelled", ErrorKind.Interrupted)
                );
            }
            catch (ObjectDisposedException)
            {
                return Result<MutexGuard<T>, Error>.Err(
                    Error.New("Mutex was disposed during lock acquisition", ErrorKind.InvalidOperation)
                );
            }
            catch (Exception ex)
            {
                return Result<MutexGuard<T>, Error>.Err(
                    Error.FromException(ex).WithContext("Failed to acquire mutex lock asynchronously with timeout")
                );
            }
        }

        /// <summary>
        /// Consumes the mutex, returning the underlying data.
        /// This is safe because we take ownership of the mutex, ensuring no other references exist.
        /// </summary>
        /// <returns>The underlying value that was protected by the mutex.</returns>
        /// <remarks>
        /// Similar to Rust's into_inner(), this method takes ownership and returns the inner value.
        /// After calling this method, the mutex is disposed and cannot be used again.
        /// </remarks>
        /// <example>
        /// <code>
        /// var mutex = new Mutex&lt;int&gt;(42);
        /// int value = mutex.IntoInner(); // mutex is now disposed
        /// </code>
        /// </example>
        public T IntoInner()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(Mutex<>), "Cannot extract value from disposed mutex");
            }

            var value = _value;
            Dispose();
            return value;
        }

        /// <summary>
        /// Gets whether this mutex has been disposed.
        /// </summary>
        public bool IsDisposed => _disposed;

        /// <summary>
        /// Releases all resources used by the mutex.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _semaphore.Dispose();
                _disposed = true;
            }
        }

        // Internal method for the guard to update the value
        internal void UpdateValue(T value)
        {
            _value = value;
        }

        // Internal method for the guard to get the current value
        internal T GetValue()
        {
            return _value;
        }
    }

    /// <summary>
    /// An RAII (Resource Acquisition Is Initialization) guard that provides exclusive access to the data protected by a Mutex.
    /// The lock is automatically released when the guard is disposed.
    /// </summary>
    /// <typeparam name="T">The type of the value protected by the mutex.</typeparam>
    /// <remarks>
    /// This type is inspired by Rust's MutexGuard and provides automatic lock release through the IDisposable pattern.
    /// Always use within a using statement or dispose explicitly to ensure the lock is released.
    /// </remarks>
    public sealed class MutexGuard<T> : IDisposable
    {
        private readonly Mutex<T> _mutex;
        private readonly SemaphoreSlim _semaphore;
        private bool _disposed;
        private T _value;

        internal MutexGuard(Mutex<T> mutex, SemaphoreSlim semaphore)
        {
            _mutex = mutex;
            _value = mutex.GetValue();
            _semaphore = semaphore;
            _disposed = false;
        }

        /// <summary>
        /// Gets or sets the value protected by the mutex.
        /// This property provides mutable access to the protected data while the guard is held.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown if the guard has been disposed.</exception>
        /// <example>
        /// <code>
        /// var mutex = new Mutex&lt;int&gt;(0);
        /// using var guard = mutex.Lock().Unwrap();
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
                {
                    throw new ObjectDisposedException(nameof(MutexGuard<>), "Cannot access value from disposed guard");
                }
                return _value;
            }
            set
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException(nameof(MutexGuard<>), "Cannot modify value from disposed guard");
                }
                _value = value;
            }
        }

        /// <summary>
        /// Maps a function over the guarded value, returning the result of the function.
        /// The guard remains locked during the function execution.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="mapper">The function to apply to the guarded value.</param>
        /// <returns>The result of applying the function.</returns>
        /// <exception cref="ArgumentNullException">Thrown if mapper is null.</exception>
        /// <exception cref="ObjectDisposedException">Thrown if the guard has been disposed.</exception>
        /// <example>
        /// <code>
        /// var mutex = new Mutex&lt;int&gt;(42);
        /// using var guard = mutex.Lock().Unwrap();
        /// 
        /// string result = guard.Map(x => $"Value is {x}");
        /// </code>
        /// </example>
        public TResult Map<TResult>(Func<T, TResult> mapper)
        {
            if (mapper is null) throw new ArgumentNullException(nameof(mapper));
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(MutexGuard<>), "Cannot map over disposed guard");
            }

            return mapper(_value);
        }

        /// <summary>
        /// Applies a function to the guarded value and updates it with the result.
        /// The guard remains locked during the function execution.
        /// </summary>
        /// <param name="updater">The function to transform the guarded value.</param>
        /// <exception cref="ArgumentNullException">Thrown if updater is null.</exception>
        /// <exception cref="ObjectDisposedException">Thrown if the guard has been disposed.</exception>
        /// <example>
        /// <code>
        /// var mutex = new Mutex&lt;int&gt;(42);
        /// using var guard = mutex.Lock().Unwrap();
        /// 
        /// guard.Update(x => x * 2); // Value is now 84
        /// </code>
        /// </example>
        public void Update(Func<T, T> updater)
        {
            if (updater is null) throw new ArgumentNullException(nameof(updater));
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(MutexGuard<>), "Cannot update disposed guard");
            }

            _value = updater(_value);
        }

        /// <summary>
        /// Releases the mutex lock by writing back the modified value and releasing the semaphore.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                // Write the value back to the mutex before releasing the lock
                // This ensures any modifications are persisted
                try
                {
                    _mutex.UpdateValue(_value);
                }
                catch (ObjectDisposedException)
                {
                    // Mutex was disposed while we held the guard
                    // Value updates are lost but that's acceptable
                }

                _disposed = true;
                
                try
                {
                    _semaphore.Release();
                }
                catch (ObjectDisposedException)
                {
                    // Mutex was disposed while we held the guard
                    // This is acceptable - the mutex cleanup will handle it
                }
            }
        }
    }
}
