using System.Security.Cryptography;
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

            /// <summary>
            /// Projects the success value of the result using a selector function.
            /// This method enables LINQ query comprehension syntax for Result types with select projections.
            /// Equivalent to <see cref="Map{U}(Func{T, U})"/>.
            /// If the result is a failure, the error is propagated unchanged without calling the selector.
            /// </summary>
            /// <typeparam name="U">The type of the projected success value.</typeparam>
            /// <param name="selector">A transform function to apply to the success value.</param>
            /// <returns>A new result with the projected value if successful; otherwise, the original error.</returns>
            /// <example>
            /// <code>
            /// var result = from x in Result&lt;int, string&gt;.Ok(10)
            ///              select x * 2;
            /// </code>
            /// </example>
            public Result<U, E> Select<U>(Func<T, U> selector)
            {
                return result.Map(selector);
            }

            /// <summary>
            /// Projects the success value of the result into a new result using a selector function.
            /// This method enables LINQ query comprehension syntax for Result types.
            /// Equivalent to <see cref="Bind{U}(Func{T, Result{U, E}})"/>.
            /// If the result is a failure, the error is propagated unchanged without calling the selector.
            /// </summary>
            /// <typeparam name="U">The type of the success value in the result returned by the selector.</typeparam>
            /// <param name="selector">A transform function to apply to the success value.</param>
            /// <returns>The result returned by the selector if the original result is successful; otherwise, the original error.</returns>
            /// <example>
            /// <code>
            /// var result = from x in ParseInt("10")
            ///              from y in ParseInt("20")
            ///              select x + y;
            /// </code>
            /// </example>
            public Result<U, E> SelectMany<U>(Func<T, Result<U, E>> selector)
            {
                return result.Bind(selector);
            }

            /// <summary>
            /// Projects the success value of the result into a new result and invokes a result selector function on both values.
            /// This method enables LINQ query comprehension syntax with multiple from clauses and a select projection.
            /// If either the original result or the selector result is a failure, the error is propagated unchanged.
            /// </summary>
            /// <typeparam name="U">The type of the intermediate success value returned by the selector.</typeparam>
            /// <typeparam name="V">The type of the final success value returned by the projector.</typeparam>
            /// <param name="selector">A transform function to apply to the original success value, returning an intermediate result.</param>
            /// <param name="projector">A function to create the final success value from the original and intermediate success values.</param>
            /// <returns>
            /// A result containing the value produced by the projector if both the original and intermediate results are successful;
            /// otherwise, the first error encountered.
            /// </returns>
            /// <example>
            /// <code>
            /// var result = from x in ParseInt("10")
            ///              from y in ParseInt("20")
            ///              select x + y;
            /// 
            /// // Equivalent to:
            /// var result = ParseInt("10").SelectMany(
            ///     x => ParseInt("20"),
            ///     (x, y) => x + y
            /// );
            /// </code>
            /// </example>
            public Result<V, E> SelectMany<U, V>(Func<T, Result<U, E>> selector, Func<T, U, V> projector)
            {
                return result.Bind(t => selector(t).Map(u => projector(t, u)));
            }

            /// <summary>
            /// Filters the success value of the result based on a predicate.
            /// This method enables LINQ query comprehension syntax with where clauses.
            /// If the result is successful and the predicate returns true, returns the original result.
            /// If the result is successful but the predicate returns false, returns a failure with a default error message.
            /// If the result is already a failure, the error is propagated unchanged.
            /// </summary>
            /// <param name="predicate">A function to test the success value.</param>
            /// <returns>The original result if successful and predicate returns true; otherwise, a failure result.</returns>
            /// <remarks>
            /// Note: This method requires a default error value when the predicate fails.
            /// For better control over error messages, consider using <see cref="Bind{U}(Func{T, Result{U, E}})"/> with explicit validation.
            /// </remarks>
            /// <example>
            /// <code>
            /// var result = from x in Result&lt;int, string&gt;.Ok(10)
            ///              where x > 5
            ///              select x * 2;
            /// </code>
            /// </example>
            public Result<T, E> Where(Func<T, bool> predicate)
            {
                return result.Match(
                    success: value => predicate(value) 
                        ? result 
                        : Result<T, E>.Err(default(E)!),
                    failure: _ => result
                );
            }
        }
    }
}
