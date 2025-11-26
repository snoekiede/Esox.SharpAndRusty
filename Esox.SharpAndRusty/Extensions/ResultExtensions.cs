using Esox.SharpAndRusty.Types;


namespace Esox.SharpAndRusty.Extensions
{
    public static class ResultExtensions
    {
        /// <param name="result">The result to transform.</param>
        /// <typeparam name="T">The type of the original success value.</typeparam>
        /// <typeparam name="E">The type of the error value.</typeparam>
        extension<T, E>(Result<T, E> result)
        {
            /// <summary>
            /// Transforms the success value of a result using the specified mapper function.
            /// If the result is a failure, the error is propagated unchanged.
            /// </summary>
            /// <typeparam name="U">The type of the transformed success value.</typeparam>
            /// <param name="mapper">A function to transform the success value.</param>
            /// <returns>A new result with the transformed success value, or the original error if the result was a failure.</returns>
            public Result<U, E> Map<U>(Func<T, U> mapper)
            {
                return result.Match(
                    success: value => Result<U, E>.Ok(mapper(value)),
                    failure: Result<U, E>.Err
                );
            }

            /// <summary>
            /// Chains together two operations that return results, allowing the second operation to depend on the success value of the first.
            /// If the result is a failure, the error is propagated unchanged without calling the binder function.
            /// </summary>
            /// <typeparam name="U">The type of the success value in the result returned by the binder.</typeparam>
            /// <param name="binder">A function that takes the success value and returns a new result.</param>
            /// <returns>The result returned by the binder function if successful, or the original error if the result was a failure.</returns>
            public Result<U, E> Bind<U>(Func<T, Result<U, E>> binder)
            {
                return result.Match(
                    success: binder,
                    failure: Result<U, E>.Err
                );
            }

            /// <summary>
            /// Extracts the success value from a result, or throws an exception if the result is a failure. Use with care
            /// </summary>
            /// <returns>The success value contained in the result.</returns>
            /// <exception cref="InvalidOperationException">Thrown when attempting to unwrap a failure result.</exception>
            public T Unwrap()
            {
                return result.Match(
                    success: value => value,
                    failure: error => throw new InvalidOperationException($"Cannot unwrap a failure result: {error}")
                );
            }
        }
    }
}
