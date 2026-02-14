using Esox.SharpAndRusty.Types;

namespace Esox.SharpAndRusty.Extensions;

/// <summary>
/// Provides extension methods for safely wrapping exception-throwing code into <see cref="Result{T, E}"/> and <see cref="Option{T}"/> types.
/// </summary>
public static class TryExtensions
{
    /// <summary>
    /// Executes a function and wraps any exceptions into a <see cref="Result{T, Exception}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <param name="func">The function to execute.</param>
    /// <returns>
    /// A result containing the return value of <paramref name="func"/> if successful,
    /// or an error containing the exception if one was thrown.
    /// </returns>
    /// <example>
    /// <code>
    /// var result = ResultExtensions.Try(() => 
    ///     JsonSerializer.Deserialize&lt;User&gt;(json));
    /// // Returns Result&lt;User, Exception&gt;
    /// </code>
    /// </example>
    public static Result<T, Exception> Try<T>(Func<T> func)
    {
        if (func is null) throw new ArgumentNullException(nameof(func));
        
        try
        {
            return Result<T, Exception>.Ok(func());
        }
        catch (Exception ex)
        {
            return Result<T, Exception>.Err(ex);
        }
    }

    /// <summary>
    /// Executes a function and wraps any exceptions into a result with a custom error type.
    /// </summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <typeparam name="E">The type of the error value.</typeparam>
    /// <param name="func">The function to execute.</param>
    /// <param name="errorMapper">A function that maps exceptions to the error type.</param>
    /// <returns>
    /// A result containing the return value of <paramref name="func"/> if successful,
    /// or an error produced by <paramref name="errorMapper"/> if an exception was thrown.
    /// </returns>
    /// <example>
    /// <code>
    /// var result = ResultExtensions.Try(
    ///     () => int.Parse(input),
    ///     ex => $"Parse failed: {ex.Message}");
    /// // Returns Result&lt;int, string&gt;
    /// </code>
    /// </example>
    public static Result<T, E> Try<T, E>(Func<T> func, Func<Exception, E> errorMapper)
    {
        if (func is null) throw new ArgumentNullException(nameof(func));
        if (errorMapper is null) throw new ArgumentNullException(nameof(errorMapper));
        
        try
        {
            return Result<T, E>.Ok(func());
        }
        catch (Exception ex)
        {
            return Result<T, E>.Err(errorMapper(ex));
        }
    }

    /// <summary>
    /// Asynchronously executes a function and wraps any exceptions into a <see cref="Result{T, Exception}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <param name="asyncFunc">The async function to execute.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains
    /// a result with the return value of <paramref name="asyncFunc"/> if successful,
    /// or an error containing the exception if one was thrown.
    /// </returns>
    /// <example>
    /// <code>
    /// var result = await ResultExtensions.TryAsync(async () =>
    ///     await File.ReadAllTextAsync(path));
    /// // Returns Result&lt;string, Exception&gt;
    /// </code>
    /// </example>
    public static async Task<Result<T, Exception>> TryAsync<T>(
        Func<Task<T>> asyncFunc,
        CancellationToken cancellationToken = default)
    {
        if (asyncFunc is null) throw new ArgumentNullException(nameof(asyncFunc));
        
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await asyncFunc().ConfigureAwait(false);
            return Result<T, Exception>.Ok(result);
        }
        catch (Exception ex)
        {
            return Result<T, Exception>.Err(ex);
        }
    }

    /// <summary>
    /// Asynchronously executes a function and wraps any exceptions into a result with a custom error type.
    /// </summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <typeparam name="E">The type of the error value.</typeparam>
    /// <param name="asyncFunc">The async function to execute.</param>
    /// <param name="errorMapper">A function that maps exceptions to the error type.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains
    /// a result with the return value of <paramref name="asyncFunc"/> if successful,
    /// or an error produced by <paramref name="errorMapper"/> if an exception was thrown.
    /// </returns>
    /// <example>
    /// <code>
    /// var result = await ResultExtensions.TryAsync(
    ///     async () => await FetchDataAsync(url),
    ///     ex => Error.New($"Fetch failed: {ex.Message}"));
    /// </code>
    /// </example>
    public static async Task<Result<T, E>> TryAsync<T, E>(
        Func<Task<T>> asyncFunc,
        Func<Exception, E> errorMapper,
        CancellationToken cancellationToken = default)
    {
        if (asyncFunc is null) throw new ArgumentNullException(nameof(asyncFunc));
        if (errorMapper is null) throw new ArgumentNullException(nameof(errorMapper));
        
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await asyncFunc().ConfigureAwait(false);
            return Result<T, E>.Ok(result);
        }
        catch (Exception ex)
        {
            return Result<T, E>.Err(errorMapper(ex));
        }
    }

    /// <summary>
    /// Executes a function and wraps any exceptions or null results into an <see cref="Option{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="func">The function to execute.</param>
    /// <returns>
    /// An option containing the return value of <paramref name="func"/> if successful and non-null,
    /// or <c>None</c> if an exception was thrown or the result was null.
    /// </returns>
    /// <example>
    /// <code>
    /// var option = OptionExtensions.Try(() => 
    ///     users.FirstOrDefault(u => u.Id == id));
    /// // Returns Option&lt;User&gt;
    /// </code>
    /// </example>
    public static Option<T> TryOption<T>(Func<T?> func) where T : class
    {
        if (func is null) throw new ArgumentNullException(nameof(func));
        
        try
        {
            var result = func();
            return result is not null
                ? new Option<T>.Some(result)
                : new Option<T>.None();
        }
        catch
        {
            return new Option<T>.None();
        }
    }

    /// <summary>
    /// Asynchronously executes a function and wraps any exceptions or null results into an <see cref="Option{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="asyncFunc">The async function to execute.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains
    /// an option with the return value of <paramref name="asyncFunc"/> if successful and non-null,
    /// or <c>None</c> if an exception was thrown or the result was null.
    /// </returns>
    /// <example>
    /// <code>
    /// var option = await OptionExtensions.TryAsync(async () =>
    ///     await _db.Users.FirstOrDefaultAsync(u => u.Id == id));
    /// // Returns Option&lt;User&gt;
    /// </code>
    /// </example>
    public static async Task<Option<T>> TryOptionAsync<T>(
        Func<Task<T?>> asyncFunc,
        CancellationToken cancellationToken = default) where T : class
    {
        if (asyncFunc is null) throw new ArgumentNullException(nameof(asyncFunc));
        
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await asyncFunc().ConfigureAwait(false);
            return result is not null
                ? new Option<T>.Some(result)
                : new Option<T>.None();
        }
        catch
        {
            return new Option<T>.None();
        }
    }
}
