using Esox.SharpAndRusty.Types;

namespace Esox.SharpAndRusty.Extensions;

/// <summary>
/// Provides asynchronous extension methods for the <see cref="Option{T}"/> type.
/// </summary>
public static class AsyncOptionExtensions
{
    /// <summary>
    /// Asynchronously transforms the value contained in the option using an async mapping function.
    /// </summary>
    /// <typeparam name="T">The type of the value in the option.</typeparam>
    /// <typeparam name="TResult">The type of the transformed value.</typeparam>
    /// <param name="option">The option to transform.</param>
    /// <param name="asyncMapper">An async function that transforms the value from type <typeparamref name="T"/> to type <typeparamref name="TResult"/>.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains:
    /// A new <see cref="Option{TResult}"/> containing the transformed value if the option is <c>Some</c>; otherwise, <c>None</c>.
    /// </returns>
    /// <remarks>
    /// The async mapper is only invoked if the option is <c>Some</c>.
    /// If the option is <c>None</c>, the operation completes immediately without calling the mapper.
    /// </remarks>
    /// <example>
    /// <code>
    /// var userOption = new Option&lt;int&gt;.Some(42);
    /// var result = await userOption.MapAsync(async id =&gt;
    /// {
    ///     await Task.Delay(100);
    ///     return await GetUserNameAsync(id);
    /// });
    /// // result is Some(userName) if userOption was Some
    /// </code>
    /// </example>
    public static async Task<Option<TResult>> MapAsync<T, TResult>(
        this Option<T> option,
        Func<T, Task<TResult>> asyncMapper,
        CancellationToken cancellationToken = default)
    {
        if (option is Option<T>.Some some)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await asyncMapper(some.Value).ConfigureAwait(false);
            return new Option<TResult>.Some(result);
        }
        
        return new Option<TResult>.None();
    }

    /// <summary>
    /// Asynchronously chains together multiple operations that return <see cref="Option{T}"/> values (also known as flatMap or chain).
    /// </summary>
    /// <typeparam name="T">The type of the value in the option.</typeparam>
    /// <typeparam name="TResult">The type of the value in the resulting option.</typeparam>
    /// <param name="option">The option to bind.</param>
    /// <param name="asyncBinder">An async function that takes the current value and returns a new <see cref="Option{TResult}"/>.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains:
    /// The <see cref="Option{TResult}"/> returned by <paramref name="asyncBinder"/> if the option is <c>Some</c>; otherwise, <c>None</c>.
    /// </returns>
    /// <remarks>
    /// The async binder is only invoked if the option is <c>Some</c>.
    /// This method allows chaining async operations that may themselves fail (return None).
    /// </remarks>
    /// <example>
    /// <code>
    /// var userIdOption = new Option&lt;int&gt;.Some(42);
    /// var result = await userIdOption.BindAsync(async id =&gt;
    ///     await FindUserInDatabaseAsync(id)); // Returns Option&lt;User&gt;
    /// // result is Some(user) if both operations succeeded, None otherwise
    /// </code>
    /// </example>
    public static async Task<Option<TResult>> BindAsync<T, TResult>(
        this Option<T> option,
        Func<T, Task<Option<TResult>>> asyncBinder,
        CancellationToken cancellationToken = default)
    {
        if (option is Option<T>.Some some)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await asyncBinder(some.Value).ConfigureAwait(false);
        }
        
        return new Option<TResult>.None();
    }

    /// <summary>
    /// Asynchronously filters the option based on an async predicate.
    /// Returns <c>Some</c> if the option is <c>Some</c> and the async predicate returns <c>true</c>; otherwise, returns <c>None</c>.
    /// </summary>
    /// <typeparam name="T">The type of the value in the option.</typeparam>
    /// <param name="option">The option to filter.</param>
    /// <param name="asyncPredicate">An async function to test the contained value.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains:
    /// <c>Some</c> if the option is <c>Some</c> and the async predicate returns <c>true</c>; otherwise, <c>None</c>.
    /// </returns>
    /// <remarks>
    /// The async predicate is only invoked if the option is <c>Some</c>.
    /// </remarks>
    /// <example>
    /// <code>
    /// var userOption = new Option&lt;User&gt;.Some(user);
    /// var result = await userOption.FilterAsync(async u =&gt;
    ///     await IsUserActiveAsync(u));
    /// // result is Some(user) if user is active, None otherwise
    /// </code>
    /// </example>
    public static async Task<Option<T>> FilterAsync<T>(
        this Option<T> option,
        Func<T, Task<bool>> asyncPredicate,
        CancellationToken cancellationToken = default)
    {
        if (option is Option<T>.Some some)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await asyncPredicate(some.Value).ConfigureAwait(false);
            return result ? option : new Option<T>.None();
        }
        
        return new Option<T>.None();
    }

    /// <summary>
    /// Asynchronously executes a side effect action on the contained value if the option is <c>Some</c>, then returns the option unchanged.
    /// Useful for debugging, logging, or other side effects in async contexts.
    /// </summary>
    /// <typeparam name="T">The type of the value in the option.</typeparam>
    /// <param name="option">The option to inspect.</param>
    /// <param name="asyncInspector">An async action to execute on the contained value.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the original option, unchanged.
    /// </returns>
    /// <remarks>
    /// The async inspector is only invoked if the option is <c>Some</c>.
    /// The option is returned unchanged, making this method suitable for insertion into a chain of operations.
    /// </remarks>
    /// <example>
    /// <code>
    /// var result = await userOption
    ///     .InspectAsync(async user =&gt; 
    ///         await LogUserAccessAsync(user))
    ///     .MapAsync(async user =&gt; 
    ///         await TransformUserAsync(user));
    /// </code>
    /// </example>
    public static async Task<Option<T>> InspectAsync<T>(
        this Option<T> option,
        Func<T, Task> asyncInspector,
        CancellationToken cancellationToken = default)
    {
        if (option is Option<T>.Some some)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await asyncInspector(some.Value).ConfigureAwait(false);
        }
        
        return option;
    }

    /// <summary>
    /// Asynchronously executes a side effect action if the option is <c>None</c>, then returns the option unchanged.
    /// Useful for debugging, logging, or other side effects when a value is absent in async contexts.
    /// </summary>
    /// <typeparam name="T">The type of the value in the option.</typeparam>
    /// <param name="option">The option to inspect.</param>
    /// <param name="asyncInspector">An async action to execute if the option is <c>None</c>.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the original option, unchanged.
    /// </returns>
    /// <remarks>
    /// The async inspector is only invoked if the option is <c>None</c>.
    /// </remarks>
    /// <example>
    /// <code>
    /// var result = await userOption
    ///     .InspectNoneAsync(async () =&gt; 
    ///         await LogUserNotFoundAsync());
    /// </code>
    /// </example>
    public static async Task<Option<T>> InspectNoneAsync<T>(
        this Option<T> option,
        Func<Task> asyncInspector,
        CancellationToken cancellationToken = default)
    {
        if (option.IsNone())
        {
            cancellationToken.ThrowIfCancellationRequested();
            await asyncInspector().ConfigureAwait(false);
        }
        
        return option;
    }

    /// <summary>
    /// Asynchronously executes one of two async actions based on whether the option contains a value.
    /// </summary>
    /// <typeparam name="T">The type of the value in the option.</typeparam>
    /// <param name="option">The option to match.</param>
    /// <param name="onSomeAsync">The async action to execute if the option is <c>Some</c>, receiving the contained value.</param>
    /// <param name="onNoneAsync">The async action to execute if the option is <c>None</c>.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <example>
    /// <code>
    /// await userOption.MatchAsync(
    ///     async user =&gt; await ProcessUserAsync(user),
    ///     async () =&gt; await HandleMissingUserAsync());
    /// </code>
    /// </example>
    public static async Task MatchAsync<T>(
        this Option<T> option,
        Func<T, Task> onSomeAsync,
        Func<Task> onNoneAsync,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        if (option is Option<T>.Some some)
        {
            await onSomeAsync(some.Value).ConfigureAwait(false);
        }
        else
        {
            await onNoneAsync().ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Asynchronously executes one of two async functions based on whether the option contains a value and returns the result.
    /// </summary>
    /// <typeparam name="T">The type of the value in the option.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="option">The option to match.</param>
    /// <param name="onSomeAsync">The async function to execute if the option is <c>Some</c>, receiving the contained value.</param>
    /// <param name="onNoneAsync">The async function to execute if the option is <c>None</c>.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains
    /// the result of executing either <paramref name="onSomeAsync"/> or <paramref name="onNoneAsync"/>.
    /// </returns>
    /// <example>
    /// <code>
    /// var message = await userOption.MatchAsync(
    ///     async user =&gt; await FormatUserMessageAsync(user),
    ///     async () =&gt; await GetDefaultMessageAsync());
    /// </code>
    /// </example>
    public static async Task<TResult> MatchAsync<T, TResult>(
        this Option<T> option,
        Func<T, Task<TResult>> onSomeAsync,
        Func<Task<TResult>> onNoneAsync,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        if (option is Option<T>.Some some)
        {
            return await onSomeAsync(some.Value).ConfigureAwait(false);
        }
        
        return await onNoneAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously converts the option to a <see cref="Result{T, E}"/> using an async error factory.
    /// Returns <c>Ok</c> if the option is <c>Some</c>; otherwise, returns <c>Err</c> with an error produced by the async factory.
    /// </summary>
    /// <typeparam name="T">The type of the value in the option.</typeparam>
    /// <typeparam name="E">The type of the error.</typeparam>
    /// <param name="option">The option to convert.</param>
    /// <param name="asyncErrorFactory">An async function that produces an error if the option is <c>None</c>.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains:
    /// A result containing the value if <c>Some</c>; otherwise, a result containing the produced error.
    /// </returns>
    /// <example>
    /// <code>
    /// var result = await userOption.OkOrElseAsync(async () =&gt;
    ///     await CreateNotFoundErrorAsync("User not found"));
    /// </code>
    /// </example>
    public static async Task<Result<T, E>> OkOrElseAsync<T, E>(
        this Option<T> option,
        Func<Task<E>> asyncErrorFactory,
        CancellationToken cancellationToken = default)
    {
        if (option is Option<T>.Some some)
        {
            return Result<T, E>.Ok(some.Value);
        }
        
        cancellationToken.ThrowIfCancellationRequested();
        var error = await asyncErrorFactory().ConfigureAwait(false);
        return Result<T, E>.Err(error);
    }
}
