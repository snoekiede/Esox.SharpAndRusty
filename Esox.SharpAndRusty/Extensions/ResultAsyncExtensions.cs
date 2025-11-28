using Esox.SharpAndRusty.Types;

namespace Esox.SharpAndRusty.Extensions
{
    /// <summary>
    /// Provides async extension methods for the Result type, enabling seamless integration with async/await patterns.
    /// </summary>
    public static class ResultAsyncExtensions
    {
        /// <summary>
        /// Transforms the success value of a Task-wrapped result using the specified mapper function.
        /// This overload awaits the result task before applying the mapper.
        /// </summary>
        /// <typeparam name="T">The type of the original success value.</typeparam>
        /// <typeparam name="E">The type of the error value.</typeparam>
        /// <typeparam name="U">The type of the transformed success value.</typeparam>
        /// <param name="resultTask">The task containing the result to transform.</param>
        /// <param name="mapper">A function to transform the success value.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task containing the transformed result.</returns>
        /// <exception cref="ArgumentNullException">Thrown when resultTask or mapper is null.</exception>
        /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled via the cancellation token.</exception>
        /// <example>
        /// <code>
        /// var result = await GetUserAsync(userId, ct)
        ///     .MapAsync(user => user.Email, ct);
        /// </code>
        /// </example>
        public static async Task<Result<U, E>> MapAsync<T, E, U>(
            this Task<Result<T, E>> resultTask,
            Func<T, U> mapper,
            CancellationToken cancellationToken = default)
        {
            if (resultTask is null) throw new ArgumentNullException(nameof(resultTask));
            if (mapper is null) throw new ArgumentNullException(nameof(mapper));
            
            var result = await resultTask.ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            return result.Map(mapper);
        }

        /// <summary>
        /// Transforms the success value of a result using an async mapper function.
        /// The mapper is only invoked if the result is successful.
        /// </summary>
        /// <typeparam name="T">The type of the original success value.</typeparam>
        /// <typeparam name="E">The type of the error value.</typeparam>
        /// <typeparam name="U">The type of the transformed success value.</typeparam>
        /// <param name="result">The result to transform.</param>
        /// <param name="asyncMapper">An async function to transform the success value.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task containing the transformed result.</returns>
        /// <exception cref="ArgumentNullException">Thrown when asyncMapper is null.</exception>
        /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled via the cancellation token.</exception>
        /// <example>
        /// <code>
        /// var result = await userResult
        ///     .MapAsync(async user => await LoadUserDetailsAsync(user, ct), ct);
        /// </code>
        /// </example>
        public static async Task<Result<U, E>> MapAsync<T, E, U>(
            this Result<T, E> result,
            Func<T, Task<U>> asyncMapper,
            CancellationToken cancellationToken = default)
        {
            if (asyncMapper is null) throw new ArgumentNullException(nameof(asyncMapper));
            
            cancellationToken.ThrowIfCancellationRequested();
            
            if (!result.IsSuccess)
            {
                result.TryGetError(out var error);
                return Result<U, E>.Err(error!);
            }
            
            result.TryGetValue(out var value);
            var mappedValue = await asyncMapper(value!).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            return Result<U, E>.Ok(mappedValue);
        }

        /// <summary>
        /// Chains together a Task-wrapped result with another operation that returns a result.
        /// This overload awaits the result task before applying the binder.
        /// </summary>
        /// <typeparam name="T">The type of the original success value.</typeparam>
        /// <typeparam name="E">The type of the error value.</typeparam>
        /// <typeparam name="U">The type of the success value in the result returned by the binder.</typeparam>
        /// <param name="resultTask">The task containing the result to bind.</param>
        /// <param name="binder">A function that takes the success value and returns a new result.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task containing the result from the binder, or the original error.</returns>
        /// <exception cref="ArgumentNullException">Thrown when resultTask or binder is null.</exception>
        /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled via the cancellation token.</exception>
        /// <example>
        /// <code>
        /// var result = await GetUserAsync(userId, ct)
        ///     .BindAsync(user => ValidateUser(user), ct);
        /// </code>
        /// </example>
        public static async Task<Result<U, E>> BindAsync<T, E, U>(
            this Task<Result<T, E>> resultTask,
            Func<T, Result<U, E>> binder,
            CancellationToken cancellationToken = default)
        {
            if (resultTask is null) throw new ArgumentNullException(nameof(resultTask));
            if (binder is null) throw new ArgumentNullException(nameof(binder));
            
            var result = await resultTask.ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            return result.Bind(binder);
        }

        /// <summary>
        /// Chains together a result with an async operation that returns a result.
        /// The async binder is only invoked if the result is successful.
        /// </summary>
        /// <typeparam name="T">The type of the original success value.</typeparam>
        /// <typeparam name="E">The type of the error value.</typeparam>
        /// <typeparam name="U">The type of the success value in the result returned by the async binder.</typeparam>
        /// <param name="result">The result to bind.</param>
        /// <param name="asyncBinder">An async function that takes the success value and returns a new result.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task containing the result from the async binder, or the original error.</returns>
        /// <exception cref="ArgumentNullException">Thrown when asyncBinder is null.</exception>
        /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled via the cancellation token.</exception>
        /// <example>
        /// <code>
        /// var result = await userResult
        ///     .BindAsync(async user => await SaveUserAsync(user, ct), ct);
        /// </code>
        /// </example>
        public static async Task<Result<U, E>> BindAsync<T, E, U>(
            this Result<T, E> result,
            Func<T, Task<Result<U, E>>> asyncBinder,
            CancellationToken cancellationToken = default)
        {
            if (asyncBinder is null) throw new ArgumentNullException(nameof(asyncBinder));
            
            cancellationToken.ThrowIfCancellationRequested();
            
            return await result.Match(
                success: async value =>
                {
                    var bindResult = await asyncBinder(value).ConfigureAwait(false);
                    cancellationToken.ThrowIfCancellationRequested();
                    return bindResult;
                },
                failure: error => Task.FromResult(Result<U, E>.Err(error))
            );
        }

        /// <param name="resultTask">The task containing the result to bind.</param>
        /// <typeparam name="T">The type of the original success value.</typeparam>
        /// <typeparam name="E">The type of the error value.</typeparam>
        extension<T, E>(Task<Result<T, E>> resultTask)
        {
            /// <summary>
            /// Chains together two Task-wrapped results.
            /// Both the result task and the binder return tasks that need to be awaited.
            /// </summary>
            /// <typeparam name="U">The type of the success value in the result returned by the async binder.</typeparam>
            /// <param name="asyncBinder">An async function that takes the success value and returns a task containing a new result.</param>
            /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
            /// <returns>A task containing the result from the async binder, or the original error.</returns>
            /// <exception cref="ArgumentNullException">Thrown when resultTask or asyncBinder is null.</exception>
            /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled via the cancellation token.</exception>
            /// <example>
            /// <code>
            /// var result = await GetUserAsync(userId, ct)
            ///     .BindAsync(async user => await ValidateAndSaveUserAsync(user, ct), ct);
            /// </code>
            /// </example>
            public async Task<Result<U, E>> BindAsync<U>(
                Func<T, Task<Result<U, E>>> asyncBinder,
                CancellationToken cancellationToken = default)
            {
                if (resultTask is null) throw new ArgumentNullException(nameof(resultTask));
                if (asyncBinder is null) throw new ArgumentNullException(nameof(asyncBinder));
            
                var result = await resultTask.ConfigureAwait(false);
                return await result.BindAsync(asyncBinder, cancellationToken);
            }

            /// <summary>
            /// Transforms the error value of a Task-wrapped result using the specified error mapper function.
            /// </summary>
            /// <typeparam name="E2">The type of the transformed error value.</typeparam>
            /// <param name="errorMapper">A function to transform the error value.</param>
            /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
            /// <returns>A task containing the result with transformed error type.</returns>
            /// <exception cref="ArgumentNullException">Thrown when resultTask or errorMapper is null.</exception>
            /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled via the cancellation token.</exception>
            /// <example>
            /// <code>
            /// var result = await GetUserAsync(userId, ct)
            ///     .MapErrorAsync(errorMsg => new UserNotFoundException(errorMsg), ct);
            /// </code>
            /// </example>
            public async Task<Result<T, E2>> MapErrorAsync<E2>(
                Func<E, E2> errorMapper,
                CancellationToken cancellationToken = default)
            {
                if (resultTask is null) throw new ArgumentNullException(nameof(resultTask));
                if (errorMapper is null) throw new ArgumentNullException(nameof(errorMapper));
            
                var result = await resultTask.ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
                return result.MapError(errorMapper);
            }

            /// <summary>
            /// Executes async actions on both success and failure of a Task-wrapped result without transforming it.
            /// </summary>
            /// <param name="onSuccess">Async action to execute if the result is successful.</param>
            /// <param name="onFailure">Async action to execute if the result is a failure.</param>
            /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
            /// <returns>A task containing the original result unchanged.</returns>
            /// <exception cref="ArgumentNullException">Thrown when resultTask, onSuccess, or onFailure is null.</exception>
            /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled via the cancellation token.</exception>
            /// <example>
            /// <code>
            /// var result = await GetUserAsync(userId, ct)
            ///     .TapAsync(
            ///         onSuccess: async user => await LogSuccessAsync(user, ct),
            ///         onFailure: async error => await LogErrorAsync(error, ct),
            ///         ct
            ///     );
            /// </code>
            /// </example>
            public async Task<Result<T, E>> TapAsync(
                Func<T, Task> onSuccess,
                Func<E, Task> onFailure,
                CancellationToken cancellationToken = default)
            {
                if (resultTask is null) throw new ArgumentNullException(nameof(resultTask));
                if (onSuccess is null) throw new ArgumentNullException(nameof(onSuccess));
                if (onFailure is null) throw new ArgumentNullException(nameof(onFailure));
            
                var result = await resultTask.ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
            
                if (result.IsSuccess && result.TryGetValue(out var value))
                {
                    await onSuccess(value).ConfigureAwait(false);
                    cancellationToken.ThrowIfCancellationRequested();
                }
                else if (result.TryGetError(out var error))
                {
                    await onFailure(error).ConfigureAwait(false);
                    cancellationToken.ThrowIfCancellationRequested();
                }
            
                return result;
            }

            /// <summary>
            /// Provides an alternative result from an async function if the Task-wrapped result is a failure.
            /// </summary>
            /// <param name="asyncAlternative">An async function that produces an alternative result based on the error.</param>
            /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
            /// <returns>A task containing the original result if successful; otherwise, the alternative result.</returns>
            /// <exception cref="ArgumentNullException">Thrown when resultTask or asyncAlternative is null.</exception>
            /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled via the cancellation token.</exception>
            /// <example>
            /// <code>
            /// var result = await GetUserFromCacheAsync(userId, ct)
            ///     .OrElseAsync(async error => await GetUserFromDatabaseAsync(userId, ct), ct);
            /// </code>
            /// </example>
            public async Task<Result<T, E>> OrElseAsync(
                Func<E, Task<Result<T, E>>> asyncAlternative,
                CancellationToken cancellationToken = default)
            {
                if (resultTask is null) throw new ArgumentNullException(nameof(resultTask));
                if (asyncAlternative is null) throw new ArgumentNullException(nameof(asyncAlternative));
            
                var result = await resultTask.ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
            
                if (result.IsSuccess)
                    return result;
            
                result.TryGetError(out var error);
                var alternativeResult = await asyncAlternative(error!).ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
                return alternativeResult;
            }
        }

        /// <summary>
        /// Combines multiple Task-wrapped results into a single result containing a collection of values.
        /// All tasks are awaited concurrently. If any result is an error, returns the first error encountered.
        /// </summary>
        /// <typeparam name="T">The type of the success values.</typeparam>
        /// <typeparam name="E">The type of the error value.</typeparam>
        /// <param name="resultTasks">The collection of tasks containing results to combine.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>
        /// A task containing a result with all success values if all results are successful;
        /// otherwise, the first error encountered.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when resultTasks is null.</exception>
        /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled via the cancellation token.</exception>
        /// <example>
        /// <code>
        /// var userTasks = userIds.Select(id => GetUserAsync(id, ct));
        /// var combined = await userTasks.CombineAsync(ct);
        /// // All succeed: Ok([User1, User2, User3])
        /// // Any fail: Err("User 2 not found")
        /// </code>
        /// </example>
        public static async Task<Result<IEnumerable<T>, E>> CombineAsync<T, E>(
            this IEnumerable<Task<Result<T, E>>> resultTasks,
            CancellationToken cancellationToken = default)
        {
            if (resultTasks is null) throw new ArgumentNullException(nameof(resultTasks));
            
            var results = await Task.WhenAll(resultTasks).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            return results.Combine();
        }
    }
}
