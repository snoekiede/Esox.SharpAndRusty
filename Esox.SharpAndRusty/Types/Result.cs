namespace Esox.SharpAndRusty.Types
{
    /// <summary>
    /// Represents the result of an operation that can either succeed with a value of type <typeparamref name="T"/>
    /// or fail with an error of type <typeparamref name="E"/>.
    /// </summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <typeparam name="E">The type of the error value.</typeparam>
    public readonly struct Result<T, E> : IEquatable<Result<T, E>>
    {
        private readonly T _value;
        private readonly E _error;

        /// <summary>
        /// Gets a value indicating whether this result represents a successful operation.
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// Gets a value indicating whether this result represents a failed operation.
        /// </summary>
        public bool IsFailure => !IsSuccess;

        private Result(T value, E error, bool isSuccess)
        {
            _value = value;
            _error = error;
            IsSuccess = isSuccess;
        }

        /// <summary>
        /// Creates a successful result containing the specified value.
        /// </summary>
        /// <param name="value">The success value.</param>
        /// <returns>A <see cref="Result{T,E}"/> representing success.</returns>
        public static Result<T, E> Ok(T value)
        {
            return new Result<T, E>(value, default(E)!, true);
        }

        /// <summary>
        /// Creates a failed result containing the specified error.
        /// </summary>
        /// <param name="error">The error value.</param>
        /// <returns>A <see cref="Result{T,E}"/> representing failure.</returns>
        public static Result<T, E> Err(E error)
        {
            return new Result<T, E>(default(T)!, error, false);
        }

        /// <summary>
        /// Matches the result and executes the appropriate function based on whether it's a success or failure.
        /// </summary>
        /// <typeparam name="R">The return type of the match operation.</typeparam>
        /// <param name="success">Function to execute if the result is successful.</param>
        /// <param name="failure">Function to execute if the result is a failure.</param>
        /// <returns>The result of executing either the success or failure function.</returns>
        /// <exception cref="ArgumentNullException">Thrown when success or failure function is null.</exception>
        public R Match<R>(Func<T, R> success, Func<E, R> failure)
        {
            if (success is null) throw new ArgumentNullException(nameof(success));
            if (failure is null) throw new ArgumentNullException(nameof(failure));
            
            return IsSuccess ? success(_value) : failure(_error);
        }

        /// <summary>
        /// Attempts to get the success value from the result.
        /// </summary>
        /// <param name="value">When this method returns, contains the success value if the result is successful; otherwise, the default value.</param>
        /// <returns>true if the result is successful; otherwise, false.</returns>
        public bool TryGetValue(out T value)
        {
            if (IsSuccess)
            {
                value = _value;
                return true;
            }
            value = default!;
            return false;
        }

        /// <summary>
        /// Attempts to get the error value from the result.
        /// </summary>
        /// <param name="error">When this method returns, contains the error value if the result is a failure; otherwise, the default value.</param>
        /// <returns>true if the result is a failure; otherwise, false.</returns>
        public bool TryGetError(out E error)
        {
            if (IsFailure)
            {
                error = _error;
                return true;
            }
            error = default!;
            return false;
        }

        /// <summary>
        /// Returns the success value if the result is successful; otherwise, returns the specified default value.
        /// </summary>
        /// <param name="defaultValue">The default value to return if the result is a failure.</param>
        /// <returns>The success value or the default value.</returns>
        public T UnwrapOr(T defaultValue)
        {
            return IsSuccess ? _value : defaultValue;
        }

        /// <summary>
        /// Returns the success value if the result is successful; otherwise, computes and returns a default value.
        /// </summary>
        /// <param name="defaultFactory">A function that produces a default value.</param>
        /// <returns>The success value or the computed default value.</returns>
        /// <exception cref="ArgumentNullException">Thrown when defaultFactory is null.</exception>
        public T UnwrapOrElse(Func<E, T> defaultFactory)
        {
            if (defaultFactory is null) throw new ArgumentNullException(nameof(defaultFactory));
            return IsSuccess ? _value : defaultFactory(_error);
        }

        /// <summary>
        /// Returns this result if it is successful; otherwise, returns the result produced by the alternative function.
        /// </summary>
        /// <param name="alternative">A function that produces an alternative result based on the error.</param>
        /// <returns>This result if successful; otherwise, the alternative result.</returns>
        /// <exception cref="ArgumentNullException">Thrown when alternative is null.</exception>
        public Result<T, E> OrElse(Func<E, Result<T, E>> alternative)
        {
            if (alternative is null) throw new ArgumentNullException(nameof(alternative));
            return IsSuccess ? this : alternative(_error);
        }

        /// <summary>
        /// Executes the specified action with the success value if the result is successful.
        /// Useful for side effects without transforming the result.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <returns>This result unchanged.</returns>
        /// <exception cref="ArgumentNullException">Thrown when action is null.</exception>
        public Result<T, E> Inspect(Action<T> action)
        {
            if (action is null) throw new ArgumentNullException(nameof(action));
            if (IsSuccess) action(_value);
            return this;
        }

        /// <summary>
        /// Executes the specified action with the error value if the result is a failure.
        /// Useful for side effects without transforming the result.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <returns>This result unchanged.</returns>
        /// <exception cref="ArgumentNullException">Thrown when action is null.</exception>
        public Result<T, E> InspectErr(Action<E> action)
        {
            if (action is null) throw new ArgumentNullException(nameof(action));
            if (IsFailure) action(_error);
            return this;
        }

        /// <summary>
        /// Determines whether the specified result is equal to the current result.
        /// </summary>
        public bool Equals(Result<T, E> other)
        {
            if (IsSuccess != other.IsSuccess)
                return false;

            if (IsSuccess)
                return EqualityComparer<T>.Default.Equals(_value, other._value);
            else
                return EqualityComparer<E>.Default.Equals(_error, other._error);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current result.
        /// </summary>
        public override bool Equals(object? obj)
        {
            return obj is Result<T, E> other && Equals(other);
        }

        /// <summary>
        /// Returns the hash code for this result.
        /// </summary>
        public override int GetHashCode()
        {
            if (IsSuccess)
                return HashCode.Combine(IsSuccess, _value);
            else
                return HashCode.Combine(IsSuccess, _error);
        }

        /// <summary>
        /// Returns a string representation of this result.
        /// </summary>
        public override string ToString()
        {
            return IsSuccess ? $"Ok({_value})" : $"Err({_error})";
        }

        /// <summary>
        /// Determines whether two results are equal.
        /// </summary>
        public static bool operator ==(Result<T, E> left, Result<T, E> right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two results are not equal.
        /// </summary>
        public static bool operator !=(Result<T, E> left, Result<T, E> right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Executes an asynchronous operation and wraps the result in a Result type.
        /// If the operation throws an exception, it is caught and converted to an error using the error handler.
        /// </summary>
        /// <param name="operation">The asynchronous operation to execute.</param>
        /// <param name="errorHandler">A function that converts an exception to an error value.</param>
        /// <returns>A task representing the asynchronous operation, containing the result.</returns>
        /// <exception cref="ArgumentNullException">Thrown when operation or errorHandler is null.</exception>
        public static async Task<Result<T, E>> TryAsync(Func<Task<T>> operation, Func<Exception, E> errorHandler)
        {
            if (operation is null) throw new ArgumentNullException(nameof(operation));
            if (errorHandler is null) throw new ArgumentNullException(nameof(errorHandler));

            try
            {
                var value = await operation().ConfigureAwait(false);
                return Ok(value);
            }
            catch (Exception ex)
            {
                return Err(errorHandler(ex));
            }
        }

        /// <summary>
        /// Executes a synchronous operation and wraps the result in a Result type.
        /// If the operation throws an exception, it is caught and converted to an error using the error handler.
        /// </summary>
        /// <param name="operation">The operation to execute.</param>
        /// <param name="errorHandler">A function that converts an exception to an error value.</param>
        /// <returns>The result of the operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when operation or errorHandler is null.</exception>
        public static Result<T, E> Try(Func<T> operation, Func<Exception, E> errorHandler)
        {
            if (operation is null) throw new ArgumentNullException(nameof(operation));
            if (errorHandler is null) throw new ArgumentNullException(nameof(errorHandler));

            try
            {
                var value = operation();
                return Ok(value);
            }
            catch (Exception ex)
            {
                return Err(errorHandler(ex));
            }
        }

    }
}
