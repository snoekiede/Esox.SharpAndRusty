using Esox.SharpAndRusty.Types;

namespace Esox.SharpAndRusty.Extensions;

/// <summary>
/// Provides asynchronous collection extension methods for working with sequences of <see cref="Option{T}"/> and <see cref="Result{T, E}"/> types.
/// </summary>
public static class AsyncCollectionExtensions
{
    #region Option Async Collection Extensions

    /// <summary>
    /// Asynchronously transforms a collection of option tasks into an option of a collection.
    /// Returns <c>Some</c> containing all values if all options are <c>Some</c>; otherwise, returns <c>None</c>.
    /// </summary>
    /// <typeparam name="T">The type of values in the options.</typeparam>
    /// <param name="optionTasks">The collection of option tasks to sequence.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for tasks to complete.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains:
    /// <c>Some</c> with all values if all options are <c>Some</c>; otherwise, <c>None</c>.
    /// </returns>
    /// <remarks>
    /// This operation short-circuits on the first <c>None</c> encountered.
    /// Tasks are awaited in order, and the operation stops at the first <c>None</c>.
    /// </remarks>
    /// <example>
    /// <code>
    /// var tasks = userIds.Select(async id => await GetUserAsync(id));
    /// var allUsers = await tasks.SequenceAsync();
    /// // Some([user1, user2, ...]) or None if any lookup fails
    /// </code>
    /// </example>
    public static async Task<Option<IEnumerable<T>>> SequenceAsync<T>(
        this IEnumerable<Task<Option<T>>> optionTasks,
        CancellationToken cancellationToken = default)
    {
        var results = new List<T>();
        
        foreach (var task in optionTasks)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            var option = await task.ConfigureAwait(false);
            if (option is Option<T>.Some some)
            {
                results.Add(some.Value);
            }
            else
            {
                return new Option<IEnumerable<T>>.None();
            }
        }
        
        return new Option<IEnumerable<T>>.Some(results);
    }

    /// <summary>
    /// Asynchronously maps each element of a collection through an async function that returns an option,
    /// then sequences the results.
    /// Returns <c>Some</c> containing all mapped values if all operations succeed; otherwise, returns <c>None</c>.
    /// </summary>
    /// <typeparam name="T">The type of elements in the source collection.</typeparam>
    /// <typeparam name="U">The type of values in the resulting options.</typeparam>
    /// <param name="source">The source collection to traverse.</param>
    /// <param name="asyncSelector">An async function that transforms each element into an option.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for tasks to complete.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains:
    /// <c>Some</c> with all transformed values if all operations succeed; otherwise, <c>None</c>.
    /// </returns>
    /// <remarks>
    /// This operation short-circuits on the first <c>None</c> returned by the selector.
    /// Elements are processed sequentially to maintain order and enable short-circuiting.
    /// </remarks>
    /// <example>
    /// <code>
    /// var userIds = new[] { 1, 2, 3 };
    /// var result = await userIds.TraverseAsync(async id => 
    ///     await GetUserFromDatabaseAsync(id));
    /// // Some([user1, user2, user3]) or None if any lookup fails
    /// </code>
    /// </example>
    public static async Task<Option<IEnumerable<U>>> TraverseAsync<T, U>(
        this IEnumerable<T> source,
        Func<T, Task<Option<U>>> asyncSelector,
        CancellationToken cancellationToken = default)
    {
        var results = new List<U>();
        
        foreach (var item in source)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            var option = await asyncSelector(item).ConfigureAwait(false);
            if (option is Option<U>.Some some)
            {
                results.Add(some.Value);
            }
            else
            {
                return new Option<IEnumerable<U>>.None();
            }
        }
        
        return new Option<IEnumerable<U>>.Some(results);
    }

    /// <summary>
    /// Asynchronously maps each element of a collection through an async function that returns an option,
    /// then sequences the results with parallel execution.
    /// Returns <c>Some</c> containing all mapped values if all operations succeed; otherwise, returns <c>None</c>.
    /// </summary>
    /// <typeparam name="T">The type of elements in the source collection.</typeparam>
    /// <typeparam name="U">The type of values in the resulting options.</typeparam>
    /// <param name="source">The source collection to traverse.</param>
    /// <param name="asyncSelector">An async function that transforms each element into an option.</param>
    /// <param name="maxDegreeOfParallelism">The maximum number of concurrent operations. Use -1 for unlimited.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for tasks to complete.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains:
    /// <c>Some</c> with all transformed values if all operations succeed; otherwise, <c>None</c>.
    /// </returns>
    /// <remarks>
    /// This operation processes elements in parallel for better performance with I/O-bound operations.
    /// The order of results matches the order of the input collection.
    /// Unlike the sequential version, this cannot short-circuit early, so all tasks will be started.
    /// </remarks>
    /// <example>
    /// <code>
    /// var userIds = Enumerable.Range(1, 100);
    /// var result = await userIds.TraverseParallelAsync(
    ///     async id => await GetUserFromApiAsync(id),
    ///     maxDegreeOfParallelism: 10);
    /// </code>
    /// </example>
    public static async Task<Option<IEnumerable<U>>> TraverseParallelAsync<T, U>(
        this IEnumerable<T> source,
        Func<T, Task<Option<U>>> asyncSelector,
        int maxDegreeOfParallelism = -1,
        CancellationToken cancellationToken = default)
    {
        var sourceList = source as IList<T> ?? source.ToList();
        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = maxDegreeOfParallelism,
            CancellationToken = cancellationToken
        };

        var results = new Option<U>[sourceList.Count];
        
        await Parallel.ForEachAsync(
            sourceList.Select((item, index) => (item, index)),
            options,
            async (tuple, ct) =>
            {
                var (item, index) = tuple;
                results[index] = await asyncSelector(item).ConfigureAwait(false);
            }).ConfigureAwait(false);

        var values = new List<U>(sourceList.Count);
        foreach (var option in results)
        {
            if (option is Option<U>.Some some)
            {
                values.Add(some.Value);
            }
            else
            {
                return new Option<IEnumerable<U>>.None();
            }
        }

        return new Option<IEnumerable<U>>.Some(values);
    }

    /// <summary>
    /// Asynchronously collects all <c>Some</c> values from a collection of option tasks,
    /// discarding any <c>None</c> values.
    /// </summary>
    /// <typeparam name="T">The type of values in the options.</typeparam>
    /// <param name="optionTasks">The collection of option tasks to collect from.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for tasks to complete.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains
    /// a collection with only the values from <c>Some</c> options.
    /// </returns>
    /// <remarks>
    /// This method never fails - it always returns a collection (which may be empty).
    /// All tasks are awaited, and the operation continues even after encountering <c>None</c> values.
    /// </remarks>
    /// <example>
    /// <code>
    /// var tasks = userIds.Select(async id => await TryGetUserAsync(id));
    /// var availableUsers = await tasks.CollectSomeAsync();
    /// // Returns all users that were found
    /// </code>
    /// </example>
    public static async Task<IEnumerable<T>> CollectSomeAsync<T>(
        this IEnumerable<Task<Option<T>>> optionTasks,
        CancellationToken cancellationToken = default)
    {
        var results = new List<T>();
        
        foreach (var task in optionTasks)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            var option = await task.ConfigureAwait(false);
            if (option is Option<T>.Some some)
            {
                results.Add(some.Value);
            }
        }
        
        return results;
    }

    #endregion

    #region Result Async Collection Extensions

    /// <summary>
    /// Asynchronously transforms a collection of result tasks into a result of a collection.
    /// Returns <c>Ok</c> containing all values if all results are <c>Ok</c>;
    /// otherwise, returns <c>Err</c> with the first error encountered.
    /// </summary>
    /// <typeparam name="T">The type of success values in the results.</typeparam>
    /// <typeparam name="E">The type of error values in the results.</typeparam>
    /// <param name="resultTasks">The collection of result tasks to sequence.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for tasks to complete.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains:
    /// <c>Ok</c> with all success values if all results are <c>Ok</c>;
    /// otherwise, <c>Err</c> with the first error encountered.
    /// </returns>
    /// <remarks>
    /// This operation short-circuits on the first <c>Err</c> encountered.
    /// Tasks are awaited in order, and the operation stops at the first <c>Err</c>.
    /// </remarks>
    /// <example>
    /// <code>
    /// var tasks = ids.Select(async id => await ProcessAsync(id));
    /// var combined = await tasks.SequenceAsync();
    /// // Ok([result1, result2, ...]) or Err(firstError)
    /// </code>
    /// </example>
    public static async Task<Result<IEnumerable<T>, E>> SequenceAsync<T, E>(
        this IEnumerable<Task<Result<T, E>>> resultTasks,
        CancellationToken cancellationToken = default)
    {
        var values = new List<T>();
        
        foreach (var task in resultTasks)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            var result = await task.ConfigureAwait(false);
            if (result.TryGetValue(out var value))
            {
                values.Add(value);
            }
            else if (result.TryGetError(out var error))
            {
                return Result<IEnumerable<T>, E>.Err(error);
            }
        }
        
        return Result<IEnumerable<T>, E>.Ok(values);
    }

    /// <summary>
    /// Asynchronously maps each element of a collection through an async function that returns a result,
    /// then sequences the results.
    /// Returns <c>Ok</c> containing all mapped values if all operations succeed;
    /// otherwise, returns <c>Err</c> with the first error encountered.
    /// </summary>
    /// <typeparam name="T">The type of elements in the source collection.</typeparam>
    /// <typeparam name="U">The type of success values in the resulting results.</typeparam>
    /// <typeparam name="E">The type of error values in the resulting results.</typeparam>
    /// <param name="source">The source collection to traverse.</param>
    /// <param name="asyncSelector">An async function that transforms each element into a result.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for tasks to complete.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains:
    /// <c>Ok</c> with all transformed values if all operations succeed;
    /// otherwise, <c>Err</c> with the first error encountered.
    /// </returns>
    /// <remarks>
    /// This operation short-circuits on the first <c>Err</c> returned by the selector.
    /// Elements are processed sequentially to maintain order and enable short-circuiting.
    /// </remarks>
    /// <example>
    /// <code>
    /// var strings = new[] { "1", "2", "3" };
    /// var result = await strings.TraverseAsync&lt;string, int, string&gt;(
    ///     async s => await ParseIntAsync(s));
    /// // Ok([1, 2, 3]) or Err("Parse error: ...")
    /// </code>
    /// </example>
    public static async Task<Result<IEnumerable<U>, E>> TraverseAsync<T, U, E>(
        this IEnumerable<T> source,
        Func<T, Task<Result<U, E>>> asyncSelector,
        CancellationToken cancellationToken = default)
    {
        var values = new List<U>();
        
        foreach (var item in source)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            var result = await asyncSelector(item).ConfigureAwait(false);
            if (result.TryGetValue(out var value))
            {
                values.Add(value);
            }
            else if (result.TryGetError(out var error))
            {
                return Result<IEnumerable<U>, E>.Err(error);
            }
        }
        
        return Result<IEnumerable<U>, E>.Ok(values);
    }

    /// <summary>
    /// Asynchronously maps each element of a collection through an async function that returns a result,
    /// then sequences the results with parallel execution.
    /// Returns <c>Ok</c> containing all mapped values if all operations succeed;
    /// otherwise, returns <c>Err</c> with the first error encountered.
    /// </summary>
    /// <typeparam name="T">The type of elements in the source collection.</typeparam>
    /// <typeparam name="U">The type of success values in the resulting results.</typeparam>
    /// <typeparam name="E">The type of error values in the resulting results.</typeparam>
    /// <param name="source">The source collection to traverse.</param>
    /// <param name="asyncSelector">An async function that transforms each element into a result.</param>
    /// <param name="maxDegreeOfParallelism">The maximum number of concurrent operations. Use -1 for unlimited.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for tasks to complete.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains:
    /// <c>Ok</c> with all transformed values if all operations succeed;
    /// otherwise, <c>Err</c> with the first error encountered.
    /// </returns>
    /// <remarks>
    /// This operation processes elements in parallel for better performance with I/O-bound operations.
    /// The order of results matches the order of the input collection.
    /// Unlike the sequential version, this cannot short-circuit early, so all tasks will be started.
    /// </remarks>
    /// <example>
    /// <code>
    /// var urls = GetUrls();
    /// var result = await urls.TraverseParallelAsync(
    ///     async url => await FetchDataAsync(url),
    ///     maxDegreeOfParallelism: 10);
    /// </code>
    /// </example>
    public static async Task<Result<IEnumerable<U>, E>> TraverseParallelAsync<T, U, E>(
        this IEnumerable<T> source,
        Func<T, Task<Result<U, E>>> asyncSelector,
        int maxDegreeOfParallelism = -1,
        CancellationToken cancellationToken = default)
    {
        var sourceList = source as IList<T> ?? source.ToList();
        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = maxDegreeOfParallelism,
            CancellationToken = cancellationToken
        };

        var results = new Result<U, E>[sourceList.Count];
        
        await Parallel.ForEachAsync(
            sourceList.Select((item, index) => (item, index)),
            options,
            async (tuple, ct) =>
            {
                var (item, index) = tuple;
                results[index] = await asyncSelector(item).ConfigureAwait(false);
            }).ConfigureAwait(false);

        var values = new List<U>(sourceList.Count);
        foreach (var result in results)
        {
            if (result.TryGetValue(out var value))
            {
                values.Add(value);
            }
            else if (result.TryGetError(out var error))
            {
                return Result<IEnumerable<U>, E>.Err(error);
            }
        }

        return Result<IEnumerable<U>, E>.Ok(values);
    }

    /// <summary>
    /// Asynchronously collects all <c>Ok</c> values from a collection of result tasks,
    /// discarding any <c>Err</c> values.
    /// </summary>
    /// <typeparam name="T">The type of success values in the results.</typeparam>
    /// <typeparam name="E">The type of error values in the results.</typeparam>
    /// <param name="resultTasks">The collection of result tasks to collect from.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for tasks to complete.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains
    /// a collection with only the values from <c>Ok</c> results.
    /// </returns>
    /// <remarks>
    /// This method never fails - it always returns a collection (which may be empty).
    /// All tasks are awaited, and the operation continues even after encountering <c>Err</c> values.
    /// </remarks>
    /// <example>
    /// <code>
    /// var tasks = urls.Select(async url => await TryFetchAsync(url));
    /// var successfulData = await tasks.CollectOkAsync();
    /// // Returns all successfully fetched data
    /// </code>
    /// </example>
    public static async Task<IEnumerable<T>> CollectOkAsync<T, E>(
        this IEnumerable<Task<Result<T, E>>> resultTasks,
        CancellationToken cancellationToken = default)
    {
        var values = new List<T>();
        
        foreach (var task in resultTasks)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            var result = await task.ConfigureAwait(false);
            if (result.TryGetValue(out var value))
            {
                values.Add(value);
            }
        }
        
        return values;
    }

    /// <summary>
    /// Asynchronously collects all <c>Err</c> values from a collection of result tasks,
    /// discarding any <c>Ok</c> values.
    /// </summary>
    /// <typeparam name="T">The type of success values in the results.</typeparam>
    /// <typeparam name="E">The type of error values in the results.</typeparam>
    /// <param name="resultTasks">The collection of result tasks to collect from.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for tasks to complete.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains
    /// a collection with only the errors from <c>Err</c> results.
    /// </returns>
    /// <remarks>
    /// This method never fails - it always returns a collection (which may be empty).
    /// All tasks are awaited, and the operation continues even after encountering <c>Ok</c> values.
    /// </remarks>
    /// <example>
    /// <code>
    /// var tasks = operations.Select(async op => await ExecuteAsync(op));
    /// var allErrors = await tasks.CollectErrAsync();
    /// LogErrors(allErrors);
    /// </code>
    /// </example>
    public static async Task<IEnumerable<E>> CollectErrAsync<T, E>(
        this IEnumerable<Task<Result<T, E>>> resultTasks,
        CancellationToken cancellationToken = default)
    {
        var errors = new List<E>();
        
        foreach (var task in resultTasks)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            var result = await task.ConfigureAwait(false);
            if (result.TryGetError(out var error))
            {
                errors.Add(error);
            }
        }
        
        return errors;
    }

    /// <summary>
    /// Asynchronously partitions a collection of result tasks into two collections:
    /// one containing all <c>Ok</c> values, and one containing all <c>Err</c> values.
    /// </summary>
    /// <typeparam name="T">The type of success values in the results.</typeparam>
    /// <typeparam name="E">The type of error values in the results.</typeparam>
    /// <param name="resultTasks">The collection of result tasks to partition.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for tasks to complete.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains
    /// a tuple with a list of all values from <c>Ok</c> results and a list of all errors from <c>Err</c> results.
    /// </returns>
    /// <remarks>
    /// All tasks are awaited, and both successes and failures are collected.
    /// </remarks>
    /// <example>
    /// <code>
    /// var tasks = operations.Select(async op => await ExecuteAsync(op));
    /// var (successes, failures) = await tasks.PartitionResultsAsync();
    /// 
    /// Console.WriteLine($"Succeeded: {successes.Count}");
    /// Console.WriteLine($"Failed: {failures.Count}");
    /// LogErrors(failures);
    /// ProcessResults(successes);
    /// </code>
    /// </example>
    public static async Task<(List<T> successes, List<E> failures)> PartitionResultsAsync<T, E>(
        this IEnumerable<Task<Result<T, E>>> resultTasks,
        CancellationToken cancellationToken = default)
    {
        var successes = new List<T>();
        var failures = new List<E>();
        
        foreach (var task in resultTasks)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            var result = await task.ConfigureAwait(false);
            if (result.TryGetValue(out var value))
            {
                successes.Add(value);
            }
            else if (result.TryGetError(out var error))
            {
                failures.Add(error);
            }
        }
        
        return (successes, failures);
    }

    #endregion
}
