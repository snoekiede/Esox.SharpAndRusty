using System;
using System.Collections.Generic;
using System.Text;

namespace Esox.SharpAndRusty.Types
{
    public readonly struct Result<T, E>
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
        public static Result<T, E> Ok(T value) => new Result<T, E>(value, default(E)!, true);

        /// <summary>
        /// Creates a failed result containing the specified error.
        /// </summary>
        /// <param name="error">The error value.</param>
        /// <returns>A <see cref="Result{T,E}"/> representing failure.</returns>
        public static Result<T, E> Err(E error) => new Result<T, E>(default(T)!, error, false);

        /// <summary>
        /// Implicitly converts a value of type <typeparamref name="T"/> to a successful result.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        public static implicit operator Result<T, E>(T value) => Ok(value);

        /// <summary>
        /// Implicitly converts an error of type <typeparamref name="E"/> to a failed result.
        /// </summary>
        /// <param name="error">The error to convert.</param>
        public static implicit operator Result<T, E>(E error) => Err(error);

        /// <summary>
        /// Matches the result and executes the appropriate function based on whether it's a success or failure.
        /// </summary>
        /// <typeparam name="R">The return type of the match operation.</typeparam>
        /// <param name="success">Function to execute if the result is successful.</param>
        /// <param name="failure">Function to execute if the result is a failure.</param>
        /// <returns>The result of executing either the success or failure function.</returns>
        public R Match<R>(Func<T, R> success, Func<E, R> failure) => IsSuccess ? success(_value) : failure(_error);
    }
}
