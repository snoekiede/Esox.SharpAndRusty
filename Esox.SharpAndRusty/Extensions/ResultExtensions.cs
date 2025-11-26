using Esox.SharpAndRusty.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Esox.SharpAndRusty.Extensions
{
    public static class ResultExtensions
    {
        /// <summary>
        /// Transforms the success value of a result using the specified mapper function.
        /// If the result is a failure, the error is propagated unchanged.
        /// </summary>
        /// <typeparam name="T">The type of the original success value.</typeparam>
        /// <typeparam name="E">The type of the error value.</typeparam>
        /// <typeparam name="U">The type of the transformed success value.</typeparam>
        /// <param name="result">The result to transform.</param>
        /// <param name="mapper">A function to transform the success value.</param>
        /// <returns>A new result with the transformed success value, or the original error if the result was a failure.</returns>
        public static Result<U, E> Map<T, E, U>(this Result<T, E> result, Func<T, U> mapper)
        {
            return result.Match(
                success: value => Result<U, E>.Ok(mapper(value)),
                failure: error => Result<U, E>.Err(error)
            );
        }
        

        /// <summary>
        /// Chains together two operations that return results, allowing the second operation to depend on the success value of the first.
        /// If the result is a failure, the error is propagated unchanged without calling the binder function.
        /// </summary>
        /// <typeparam name="T">The type of the original success value.</typeparam>
        /// <typeparam name="E">The type of the error value.</typeparam>
        /// <typeparam name="U">The type of the success value in the result returned by the binder.</typeparam>
        /// <param name="result">The result to bind.</param>
        /// <param name="binder">A function that takes the success value and returns a new result.</param>
        /// <returns>The result returned by the binder function if successful, or the original error if the result was a failure.</returns>
        public static Result<U, E> Bind<T, E, U>(this Result<T, E> result, Func<T, Result<U, E>> binder)
        {
            return result.Match(
                success: value => binder(value),
                failure: error => Result<U, E>.Err(error)
            );
        }

        /// <summary>
        /// Extracts the success value from a result, or throws an exception if the result is a failure. Use with care
        /// </summary>
        /// <typeparam name="T">The type of the success value.</typeparam>
        /// <typeparam name="E">The type of the error value.</typeparam>
        /// <param name="result">The result to unwrap.</param>
        /// <returns>The success value contained in the result.</returns>
        /// <exception cref="InvalidOperationException">Thrown when attempting to unwrap a failure result.</exception>
        public static T Unwrap<T, E>(this Result<T, E> result)
        {
            return result.Match(
                success: value => value,
                failure: error => throw new InvalidOperationException($"Cannot unwrap a failure result: {error}")
            );
        }
    }
}
