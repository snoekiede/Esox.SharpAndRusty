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
            /// <exception cref="ArgumentNullException">Thrown when mapper is null.</exception>
            public Result<U, E> Map<U>(Func<T, U> mapper)
            {
                if (mapper is null) throw new ArgumentNullException(nameof(mapper));
                
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
            /// <exception cref="ArgumentNullException">Thrown when binder is null.</exception>
            public Result<U, E> Bind<U>(Func<T, Result<U, E>> binder)
            {
                if (binder is null) throw new ArgumentNullException(nameof(binder));
                
                return result.Match(
                    success: binder,
                    failure: Result<U, E>.Err
                );
            }

            /// <summary>
            /// Extracts the success value from a result, or throws an exception if the result is a failure. Use with care.
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
            /// Extracts the success value from a result with a custom error message, or throws an exception if the result is a failure.
            /// Provides better context than <see cref="Unwrap"/> for debugging.
            /// </summary>
            /// <param name="message">Custom error message to include in the exception.</param>
            /// <returns>The success value contained in the result.</returns>
            /// <exception cref="InvalidOperationException">Thrown with custom message when attempting to unwrap a failure result.</exception>
            /// <exception cref="ArgumentNullException">Thrown when message is null.</exception>
            /// <example>
            /// <code>
            /// var userId = userResult.Expect("User ID is required for this operation");
            /// </code>
            /// </example>
            public T Expect(string message)
            {
                if (message is null) throw new ArgumentNullException(nameof(message));
                
                return result.Match(
                    success: value => value,
                    failure: error => throw new InvalidOperationException($"{message}: {error}")
                );
            }

            /// <summary>
            /// Transforms the error value while preserving the success value.
            /// Useful for converting between different error types in a pipeline.
            /// </summary>
            /// <typeparam name="E2">The new error type.</typeparam>
            /// <param name="errorMapper">A function to transform the error value.</param>
            /// <returns>A new result with the same success value but transformed error type.</returns>
            /// <exception cref="ArgumentNullException">Thrown when errorMapper is null.</exception>
            /// <example>
            /// <code>
            /// Result&lt;User, string&gt; result = GetUser(id);
            /// Result&lt;User, ErrorCode&gt; mapped = result.MapError(msg => ErrorCode.NotFound);
            /// </code>
            /// </example>
            public Result<T, E2> MapError<E2>(Func<E, E2> errorMapper)
            {
                if (errorMapper is null) throw new ArgumentNullException(nameof(errorMapper));
                
                return result.Match(
                    success: value => Result<T, E2>.Ok(value),
                    failure: error => Result<T, E2>.Err(errorMapper(error))
                );
            }

            /// <summary>
            /// Executes actions on both success and failure without transforming the result.
            /// Combines <see cref="Result{T,E}.Inspect"/> and <see cref="Result{T,E}.InspectErr"/> into a single call.
            /// </summary>
            /// <param name="onSuccess">Action to execute if the result is successful.</param>
            /// <param name="onFailure">Action to execute if the result is a failure.</param>
            /// <returns>The original result unchanged.</returns>
            /// <exception cref="ArgumentNullException">Thrown when onSuccess or onFailure is null.</exception>
            /// <example>
            /// <code>
            /// result.Tap(
            ///     onSuccess: user => Logger.Info($"Found user: {user.Name}"),
            ///     onFailure: error => Logger.Error($"Error: {error}")
            /// );
            /// </code>
            /// </example>
            public Result<T, E> Tap(Action<T> onSuccess, Action<E> onFailure)
            {
                if (onSuccess is null) throw new ArgumentNullException(nameof(onSuccess));
                if (onFailure is null) throw new ArgumentNullException(nameof(onFailure));
                
                return result
                    .Inspect(onSuccess)
                    .InspectErr(onFailure);
            }

            /// <summary>
            /// Checks if the result contains a specific success value.
            /// </summary>
            /// <param name="value">The value to check for.</param>
            /// <returns>True if the result is successful and contains the specified value; otherwise, false.</returns>
            /// <example>
            /// <code>
            /// if (result.Contains(42))
            /// {
            ///     Console.WriteLine("Result contains 42");
            /// }
            /// </code>
            /// </example>
            public bool Contains(T value)
            {
                return result.Match(
                    success: v => EqualityComparer<T>.Default.Equals(v, value),
                    failure: _ => false
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
            /// <exception cref="ArgumentNullException">Thrown when selector is null.</exception>
            /// <example>
            /// <code>
            /// var result = from x in Result&lt;int, string&gt;.Ok(10)
            ///              select x * 2;
            /// </code>
            /// </example>
            public Result<U, E> Select<U>(Func<T, U> selector)
            {
                if (selector is null) throw new ArgumentNullException(nameof(selector));
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
            /// <exception cref="ArgumentNullException">Thrown when selector is null.</exception>
            /// <example>
            /// <code>
            /// var result = from x in ParseInt("10")
            ///              from y in ParseInt("20")
            ///              select x + y;
            /// </code>
            /// </example>
            public Result<U, E> SelectMany<U>(Func<T, Result<U, E>> selector)
            {
                if (selector is null) throw new ArgumentNullException(nameof(selector));
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
            /// <exception cref="ArgumentNullException">Thrown when selector or projector is null.</exception>
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
                if (selector is null) throw new ArgumentNullException(nameof(selector));
                if (projector is null) throw new ArgumentNullException(nameof(projector));
                
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
            /// <exception cref="ArgumentNullException">Thrown when predicate is null.</exception>
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
                if (predicate is null) throw new ArgumentNullException(nameof(predicate));
                
                return result.Match(
                    success: value => predicate(value) 
                        ? result 
                        : Result<T, E>.Err(default(E)!),
                    failure: _ => result
                );
            }
        }

        /// <summary>
        /// Combines multiple results into a single result containing a collection of values.
        /// If any result is an error, returns the first error encountered.
        /// </summary>
        /// <typeparam name="T">The type of the success values.</typeparam>
        /// <typeparam name="E">The type of the error value.</typeparam>
        /// <param name="results">The collection of results to combine.</param>
        /// <returns>
        /// A result containing all success values if all results are successful;
        /// otherwise, the first error encountered.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when results is null.</exception>
        /// <example>
        /// <code>
        /// var userIds = new[] { 1, 2, 3 };
        /// var results = userIds.Select(id => GetUser(id));
        /// Result&lt;IEnumerable&lt;User&gt;, string&gt; combined = results.Combine();
        /// // All succeed: Ok([User1, User2, User3])
        /// // Any fail: Err("User 2 not found")
        /// </code>
        /// </example>
        public static Result<IEnumerable<T>, E> Combine<T, E>(this IEnumerable<Result<T, E>> results)
        {
            if (results is null) throw new ArgumentNullException(nameof(results));
            
            var values = new List<T>();
            foreach (var result in results)
            {
                if (result.TryGetValue(out var value))
                    values.Add(value);
                else if (result.TryGetError(out var error))
                    return Result<IEnumerable<T>, E>.Err(error);
            }
            return Result<IEnumerable<T>, E>.Ok(values);
        }

        /// <summary>
        /// Separates a collection of results into successes and failures.
        /// Useful for batch operations where you want to process both successful and failed results.
        /// </summary>
        /// <typeparam name="T">The type of the success values.</typeparam>
        /// <typeparam name="E">The type of the error values.</typeparam>
        /// <param name="results">The collection of results to partition.</param>
        /// <returns>A tuple containing lists of successes and failures.</returns>
        /// <exception cref="ArgumentNullException">Thrown when results is null.</exception>
        /// <example>
        /// <code>
        /// var results = userIds.Select(id => GetUser(id));
        /// var (successes, failures) = results.Partition();
        /// Console.WriteLine($"Found {successes.Count} users, {failures.Count} errors");
        /// </code>
        /// </example>
        public static (List<T> successes, List<E> failures) Partition<T, E>(this IEnumerable<Result<T, E>> results)
        {
            if (results is null) throw new ArgumentNullException(nameof(results));
            
            var successes = new List<T>();
            var failures = new List<E>();
            
            foreach (var result in results)
            {
                if (result.TryGetValue(out var value))
                    successes.Add(value);
                else if (result.TryGetError(out var error))
                    failures.Add(error);
            }
            
            return (successes, failures);
        }
    }
}
