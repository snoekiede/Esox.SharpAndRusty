using Esox.SharpAndRusty.Types;

namespace Esox.SharpAndRusty.Sync
{
    /// <summary>
    /// A reader-writer lock for protecting shared data, inspired by Rust's std::sync::RwLock.
    /// This type allows multiple concurrent readers or a single writer, providing interior mutability
    /// with shared/exclusive access semantics and integrates with Result/Error types.
    /// Works in both synchronous and asynchronous contexts.
    /// </summary>
    /// <typeparam name="T">The type of the value protected by the RwLock.</typeparam>
    /// <remarks>
    /// Unlike Rust's RwLock which relies on compile-time borrow checking, this C# implementation uses
    /// runtime locks and returns Result types to handle lock acquisition failures gracefully.
    /// 
    /// The RwLock uses ReaderWriterLockSlim internally for efficient reader/writer semantics.
    /// Multiple readers can access the data concurrently, but writers have exclusive access.
    /// </remarks>
    public sealed class RwLock<T> : IDisposable
    {
        private readonly ReaderWriterLockSlim _lock;
        private T _value;
        private bool _disposed;

        /// <summary>
        /// Creates a new RwLock in an unlocked state ready for use.
        /// </summary>
        /// <param name="value">The initial value to protect.</param>
        public RwLock(T value)
        {
            _value = value;
            _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
            _disposed = false;
        }

        /// <summary>
        /// Acquires a read lock, blocking the current thread until it is able to do so.
        /// Multiple readers can hold the lock simultaneously.
        /// Returns a ReadGuard that provides read-only access to the protected data.
        /// </summary>
        /// <returns>
        /// A Result containing a ReadGuard on success, or an Error if the lock is disposed.
        /// </returns>
        /// <example>
        /// <code>
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
            if (_disposed)
            {
                return Result<ReadGuard<T>, Error>.Err(
                    Error.New("Cannot read from disposed RwLock", ErrorKind.InvalidOperation)
                );
            }

            try
            {
                _lock.EnterReadLock();
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
        /// Attempts to acquire a read lock without blocking.
        /// If the lock cannot be acquired immediately, returns an error.
        /// </summary>
        /// <returns>
        /// A Result containing a ReadGuard if successful, or an Error if the lock could not be acquired.
        /// </returns>
        /// <example>
        /// <code>
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
            if (_disposed)
            {
                return Result<ReadGuard<T>, Error>.Err(
                    Error.New("Cannot read from disposed RwLock", ErrorKind.InvalidOperation)
                );
            }

            try
            {
                if (_lock.TryEnterReadLock(0))
                {
                    return Result<ReadGuard<T>, Error>.Ok(
                        new ReadGuard<T>(this, _lock)
                    );
                }
                else
                {
                    return Result<ReadGuard<T>, Error>.Err(
                        Error.New("RwLock read lock is currently unavailable", ErrorKind.ResourceExhausted)
                            .WithMetadata("lockAttemptTime", DateTime.UtcNow)
                    );
                }
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
        /// Attempts to acquire a read lock with a timeout.
        /// </summary>
        /// <param name="timeout">The maximum time to wait for the lock.</param>
        /// <returns>
        /// A Result containing a ReadGuard if successful, or an Error if the timeout expired.
        /// </returns>
        /// <example>
        /// <code>
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
            if (_disposed)
            {
                return Result<ReadGuard<T>, Error>.Err(
                    Error.New("Cannot read from disposed RwLock", ErrorKind.InvalidOperation)
                );
            }

            try
            {
                if (_lock.TryEnterReadLock(timeout))
                {
                    return Result<ReadGuard<T>, Error>.Ok(
                        new ReadGuard<T>(this, _lock)
                    );
                }
                else
                {
                    return Result<ReadGuard<T>, Error>.Err(
                        Error.New("RwLock read lock timeout expired", ErrorKind.Timeout)
                            .WithMetadata("timeout", timeout)
                            .WithMetadata("attemptTime", DateTime.UtcNow)
                    );
                }
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
        /// Acquires a write lock, blocking the current thread until it is able to do so.
        /// Only one writer can hold the lock, and no readers can access the data.
        /// Returns a WriteGuard that provides mutable access to the protected data.
        /// </summary>
        /// <returns>
        /// A Result containing a WriteGuard on success, or an Error if the lock is disposed.
        /// </returns>
        /// <example>
        /// <code>
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
            if (_disposed)
            {
                return Result<WriteGuard<T>, Error>.Err(
                    Error.New("Cannot write to disposed RwLock", ErrorKind.InvalidOperation)
                );
            }

            try
            {
                _lock.EnterWriteLock();
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
        /// Attempts to acquire a write lock without blocking.
        /// If the lock cannot be acquired immediately, returns an error.
        /// </summary>
        /// <returns>
        /// A Result containing a WriteGuard if successful, or an Error if the lock could not be acquired.
        /// </returns>
        /// <example>
        /// <code>
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
            if (_disposed)
            {
                return Result<WriteGuard<T>, Error>.Err(
                    Error.New("Cannot write to disposed RwLock", ErrorKind.InvalidOperation)
                );
            }

            try
            {
                if (_lock.TryEnterWriteLock(0))
                {
                    return Result<WriteGuard<T>, Error>.Ok(
                        new WriteGuard<T>(this, _lock)
                    );
                }
                else
                {
                    return Result<WriteGuard<T>, Error>.Err(
                        Error.New("RwLock write lock is currently unavailable", ErrorKind.ResourceExhausted)
                            .WithMetadata("lockAttemptTime", DateTime.UtcNow)
                    );
                }
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
        /// Attempts to acquire a write lock with a timeout.
        /// </summary>
        /// <param name="timeout">The maximum time to wait for the lock.</param>
        /// <returns>
        /// A Result containing a WriteGuard if successful, or an Error if the timeout expired.
        /// </returns>
        /// <example>
        /// <code>
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
            if (_disposed)
            {
                return Result<WriteGuard<T>, Error>.Err(
                    Error.New("Cannot write to disposed RwLock", ErrorKind.InvalidOperation)
                );
            }

            try
            {
                if (_lock.TryEnterWriteLock(timeout))
                {
                    return Result<WriteGuard<T>, Error>.Ok(
                        new WriteGuard<T>(this, _lock)
                    );
                }
                else
                {
                    return Result<WriteGuard<T>, Error>.Err(
                        Error.New("RwLock write lock timeout expired", ErrorKind.Timeout)
                            .WithMetadata("timeout", timeout)
                            .WithMetadata("attemptTime", DateTime.UtcNow)
                    );
                }
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
        /// Consumes the RwLock, returning the underlying data.
        /// This is safe because we take ownership of the lock, ensuring no other references exist.
        /// </summary>
        /// <returns>The underlying value that was protected by the RwLock.</returns>
        /// <remarks>
        /// Similar to Rust's into_inner(), this method takes ownership and returns the inner value.
        /// After calling this method, the RwLock is disposed and cannot be used again.
        /// </remarks>
        /// <example>
        /// <code>
        /// var rwlock = new RwLock&lt;int&gt;(42);
        /// int value = rwlock.IntoInner(); // rwlock is now disposed
        /// </code>
        /// </example>
        public T IntoInner()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(RwLock<>), "Cannot extract value from disposed RwLock");
            }

            var value = _value;
            Dispose();
            return value;
        }

        /// <summary>
        /// Gets whether this RwLock has been disposed.
        /// </summary>
        public bool IsDisposed => _disposed;

        /// <summary>
        /// Releases all resources used by the RwLock.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _lock.Dispose();
                _disposed = true;
            }
        }

        // Internal method for the write guard to update the value
        internal void UpdateValue(T value)
        {
            _value = value;
        }

        // Internal method for guards to get the current value
        internal T GetValue()
        {
            return _value;
        }
    }

    /// <summary>
    /// An RAII guard that provides read-only access to the data protected by an RwLock.
    /// The read lock is automatically released when the guard is disposed.
    /// Multiple ReadGuards can exist simultaneously.
    /// </summary>
    /// <typeparam name="T">The type of the value protected by the RwLock.</typeparam>
    /// <remarks>
    /// This type is inspired by Rust's RwLockReadGuard and provides automatic lock release through the IDisposable pattern.
    /// Always use within a using statement or dispose explicitly to ensure the lock is released.
    /// </remarks>
    public sealed class ReadGuard<T> : IDisposable
    {
        private readonly RwLock<T> _rwlock;
        private readonly ReaderWriterLockSlim _lock;
        private bool _disposed;
        private readonly T _value;

        internal ReadGuard(RwLock<T> rwlock, ReaderWriterLockSlim lockSlim)
        {
            _rwlock = rwlock;
            _value = rwlock.GetValue();
            _lock = lockSlim;
            _disposed = false;
        }

        /// <summary>
        /// Gets the value protected by the RwLock (read-only access).
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown if the guard has been disposed.</exception>
        /// <example>
        /// <code>
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
                {
                    throw new ObjectDisposedException(nameof(ReadGuard<>), "Cannot access value from disposed guard");
                }
                return _value;
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
        /// var rwlock = new RwLock&lt;int&gt;(42);
        /// using var guard = rwlock.Read().Unwrap();
        /// 
        /// string result = guard.Map(x => $"Value is {x}");
        /// </code>
        /// </example>
        public TResult Map<TResult>(Func<T, TResult> mapper)
        {
            if (mapper is null) throw new ArgumentNullException(nameof(mapper));
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ReadGuard<>), "Cannot map over disposed guard");
            }

            return mapper(_value);
        }

        /// <summary>
        /// Releases the read lock.
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
            }
        }
    }

    /// <summary>
    /// An RAII guard that provides exclusive write access to the data protected by an RwLock.
    /// The write lock is automatically released when the guard is disposed.
    /// Only one WriteGuard can exist at a time, and no ReadGuards can be active.
    /// </summary>
    /// <typeparam name="T">The type of the value protected by the RwLock.</typeparam>
    /// <remarks>
    /// This type is inspired by Rust's RwLockWriteGuard and provides automatic lock release through the IDisposable pattern.
    /// Always use within a using statement or dispose explicitly to ensure the lock is released.
    /// </remarks>
    public sealed class WriteGuard<T> : IDisposable
    {
        private readonly RwLock<T> _rwlock;
        private readonly ReaderWriterLockSlim _lock;
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
        /// Gets or sets the value protected by the RwLock.
        /// This property provides exclusive mutable access to the protected data while the guard is held.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown if the guard has been disposed.</exception>
        /// <example>
        /// <code>
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
                {
                    throw new ObjectDisposedException(nameof(WriteGuard<>), "Cannot access value from disposed guard");
                }
                return _value;
            }
            set
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException(nameof(WriteGuard<>), "Cannot modify value from disposed guard");
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
        /// var rwlock = new RwLock&lt;int&gt;(42);
        /// using var guard = rwlock.Write().Unwrap();
        /// 
        /// string result = guard.Map(x => $"Value is {x}");
        /// </code>
        /// </example>
        public TResult Map<TResult>(Func<T, TResult> mapper)
        {
            if (mapper is null) throw new ArgumentNullException(nameof(mapper));
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(WriteGuard<>), "Cannot map over disposed guard");
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
        /// var rwlock = new RwLock&lt;int&gt;(42);
        /// using var guard = rwlock.Write().Unwrap();
        /// 
        /// guard.Update(x => x * 2); // Value is now 84
        /// </code>
        /// </example>
        public void Update(Func<T, T> updater)
        {
            if (updater is null) throw new ArgumentNullException(nameof(updater));
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(WriteGuard<>), "Cannot update disposed guard");
            }

            _value = updater(_value);
        }

        /// <summary>
        /// Releases the write lock by writing back the modified value and releasing the lock.
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
            }
        }
    }
}
