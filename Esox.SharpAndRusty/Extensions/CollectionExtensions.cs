using Esox.SharpAndRusty.Types;

namespace Esox.SharpAndRusty.Extensions;

/// <summary>
/// Provides collection extension methods for working with sequences of <see cref="Option{T}"/> and <see cref="Result{T, E}"/> types.
/// </summary>
public static class CollectionExtensions
{
    #region Option Collection Extensions

    /// <summary>
    /// Transforms a collection of options into an option of a collection.
    /// Returns <c>Some</c> containing all values if all options are <c>Some</c>; otherwise, returns <c>None</c>.
    /// </summary>
    /// <typeparam name="T">The type of values in the options.</typeparam>
    /// <param name="options">The collection of options to sequence.</param>
    /// <returns>
    /// <c>Some</c> containing a list of all values if all options are <c>Some</c>; 
    /// otherwise, <c>None</c> if any option is <c>None</c>.
    /// </returns>
    /// <remarks>
    /// This operation short-circuits on the first <c>None</c> encountered.
    /// The returned collection preserves the order of the input collection.
    /// </remarks>
    /// <example>
    /// <code>
    /// var options = new[]
    /// {
    ///     new Option&lt;int&gt;.Some(1),
    ///     new Option&lt;int&gt;.Some(2),
    ///     new Option&lt;int&gt;.Some(3)
    /// };
    /// var result = options.Sequence(); // Some([1, 2, 3])
    /// 
    /// var withNone = new[]
    /// {
    ///     new Option&lt;int&gt;.Some(1),
    ///     new Option&lt;int&gt;.None(),
    ///     new Option&lt;int&gt;.Some(3)
    /// };
    /// var result2 = withNone.Sequence(); // None
    /// </code>
    /// </example>
    public static Option<IEnumerable<T>> Sequence<T>(this IEnumerable<Option<T>> options)
    {
        var results = new List<T>();
        
        foreach (var option in options)
        {
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
    /// Maps each element of a collection through a function that returns an option, 
    /// then sequences the results.
    /// Returns <c>Some</c> containing all mapped values if all operations succeed; 
    /// otherwise, returns <c>None</c>.
    /// </summary>
    /// <typeparam name="T">The type of elements in the source collection.</typeparam>
    /// <typeparam name="U">The type of values in the resulting options.</typeparam>
    /// <param name="source">The source collection to traverse.</param>
    /// <param name="selector">A function that transforms each element into an option.</param>
    /// <returns>
    /// <c>Some</c> containing a list of all transformed values if all operations succeed;
    /// otherwise, <c>None</c> if any operation fails.
    /// </returns>
    /// <remarks>
    /// This is a combination of <see cref="Enumerable.Select{TSource, TResult}(IEnumerable{TSource}, Func{TSource, TResult})"/>
    /// and <see cref="Sequence{T}(IEnumerable{Option{T}})"/>.
    /// The operation short-circuits on the first <c>None</c> returned by the selector.
    /// </remarks>
    /// <example>
    /// <code>
    /// var numbers = new[] { "1", "2", "3" };
    /// var result = numbers.Traverse(s => 
    ///     int.TryParse(s, out var n) 
    ///         ? new Option&lt;int&gt;.Some(n) 
    ///         : new Option&lt;int&gt;.None()
    /// ); // Some([1, 2, 3])
    /// 
    /// var withInvalid = new[] { "1", "invalid", "3" };
    /// var result2 = withInvalid.Traverse(s => 
    ///     int.TryParse(s, out var n) 
    ///         ? new Option&lt;int&gt;.Some(n) 
    ///         : new Option&lt;int&gt;.None()
    /// ); // None
    /// </code>
    /// </example>
    public static Option<IEnumerable<U>> Traverse<T, U>(
        this IEnumerable<T> source,
        Func<T, Option<U>> selector)
    {
        var results = new List<U>();
        
        foreach (var item in source)
        {
            var option = selector(item);
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
    /// Collects all <c>Some</c> values from a collection of options, discarding any <c>None</c> values.
    /// </summary>
    /// <typeparam name="T">The type of values in the options.</typeparam>
    /// <param name="options">The collection of options to collect from.</param>
    /// <returns>A collection containing only the values from <c>Some</c> options.</returns>
    /// <remarks>
    /// This method never fails - it always returns a collection (which may be empty if all options are <c>None</c>).
    /// The returned collection preserves the order of <c>Some</c> values from the input.
    /// </remarks>
    /// <example>
    /// <code>
    /// var options = new[]
    /// {
    ///     new Option&lt;int&gt;.Some(1),
    ///     new Option&lt;int&gt;.None(),
    ///     new Option&lt;int&gt;.Some(3)
    /// };
    /// var values = options.CollectSome(); // [1, 3]
    /// </code>
    /// </example>
    public static IEnumerable<T> CollectSome<T>(this IEnumerable<Option<T>> options)
    {
        foreach (var option in options)
        {
            if (option is Option<T>.Some some)
            {
                yield return some.Value;
            }
        }
    }

    /// <summary>
    /// Partitions a collection of options into two collections: one containing all <c>Some</c> values,
    /// and one containing the count of <c>None</c> values.
    /// </summary>
    /// <typeparam name="T">The type of values in the options.</typeparam>
    /// <param name="options">The collection of options to partition.</param>
    /// <returns>
    /// A tuple containing:
    /// - A list of all values from <c>Some</c> options
    /// - The count of <c>None</c> options
    /// </returns>
    /// <example>
    /// <code>
    /// var options = new[]
    /// {
    ///     new Option&lt;int&gt;.Some(1),
    ///     new Option&lt;int&gt;.None(),
    ///     new Option&lt;int&gt;.Some(3),
    ///     new Option&lt;int&gt;.None()
    /// };
    /// var (values, noneCount) = options.PartitionOptions(); 
    /// // values = [1, 3], noneCount = 2
    /// </code>
    /// </example>
    public static (List<T> values, int noneCount) PartitionOptions<T>(this IEnumerable<Option<T>> options)
    {
        var values = new List<T>();
        var noneCount = 0;
        
        foreach (var option in options)
        {
            if (option is Option<T>.Some some)
            {
                values.Add(some.Value);
            }
            else
            {
                noneCount++;
            }
        }
        
        return (values, noneCount);
    }

    #endregion

    #region Result Collection Extensions

    /// <summary>
    /// Transforms a collection of results into a result of a collection.
    /// Returns <c>Ok</c> containing all values if all results are <c>Ok</c>; 
    /// otherwise, returns <c>Err</c> with the first error encountered.
    /// </summary>
    /// <typeparam name="T">The type of success values in the results.</typeparam>
    /// <typeparam name="E">The type of error values in the results.</typeparam>
    /// <param name="results">The collection of results to sequence.</param>
    /// <returns>
    /// <c>Ok</c> containing a list of all success values if all results are <c>Ok</c>;
    /// otherwise, <c>Err</c> containing the first error encountered.
    /// </returns>
    /// <remarks>
    /// This operation short-circuits on the first <c>Err</c> encountered.
    /// The returned collection preserves the order of the input collection.
    /// </remarks>
    /// <example>
    /// <code>
    /// var results = new[]
    /// {
    ///     Result&lt;int, string&gt;.Ok(1),
    ///     Result&lt;int, string&gt;.Ok(2),
    ///     Result&lt;int, string&gt;.Ok(3)
    /// };
    /// var combined = results.Sequence(); // Ok([1, 2, 3])
    /// 
    /// var withError = new[]
    /// {
    ///     Result&lt;int, string&gt;.Ok(1),
    ///     Result&lt;int, string&gt;.Err("error"),
    ///     Result&lt;int, string&gt;.Ok(3)
    /// };
    /// var combined2 = withError.Sequence(); // Err("error")
    /// </code>
    /// </example>
    public static Result<IEnumerable<T>, E> Sequence<T, E>(this IEnumerable<Result<T, E>> results)
    {
        var values = new List<T>();
        
        foreach (var result in results)
        {
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
    /// Maps each element of a collection through a function that returns a result,
    /// then sequences the results.
    /// Returns <c>Ok</c> containing all mapped values if all operations succeed;
    /// otherwise, returns <c>Err</c> with the first error encountered.
    /// </summary>
    /// <typeparam name="T">The type of elements in the source collection.</typeparam>
    /// <typeparam name="U">The type of success values in the resulting results.</typeparam>
    /// <typeparam name="E">The type of error values in the resulting results.</typeparam>
    /// <param name="source">The source collection to traverse.</param>
    /// <param name="selector">A function that transforms each element into a result.</param>
    /// <returns>
    /// <c>Ok</c> containing a list of all transformed values if all operations succeed;
    /// otherwise, <c>Err</c> containing the first error encountered.
    /// </returns>
    /// <remarks>
    /// This is a combination of <see cref="Enumerable.Select{TSource, TResult}(IEnumerable{TSource}, Func{TSource, TResult})"/>
    /// and <see cref="Sequence{T, E}(IEnumerable{Result{T, E}})"/>.
    /// The operation short-circuits on the first <c>Err</c> returned by the selector.
    /// </remarks>
    /// <example>
    /// <code>
    /// var numbers = new[] { "1", "2", "3" };
    /// var result = numbers.Traverse&lt;string, int, string&gt;(s => 
    ///     int.TryParse(s, out var n)
    ///         ? Result&lt;int, string&gt;.Ok(n)
    ///         : Result&lt;int, string&gt;.Err($"Invalid: {s}")
    /// ); // Ok([1, 2, 3])
    /// 
    /// var withInvalid = new[] { "1", "invalid", "3" };
    /// var result2 = withInvalid.Traverse&lt;string, int, string&gt;(s => 
    ///     int.TryParse(s, out var n)
    ///         ? Result&lt;int, string&gt;.Ok(n)
    ///         : Result&lt;int, string&gt;.Err($"Invalid: {s}")
    /// ); // Err("Invalid: invalid")
    /// </code>
    /// </example>
    public static Result<IEnumerable<U>, E> Traverse<T, U, E>(
        this IEnumerable<T> source,
        Func<T, Result<U, E>> selector)
    {
        var values = new List<U>();
        
        foreach (var item in source)
        {
            var result = selector(item);
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
    /// Collects all <c>Ok</c> values from a collection of results, discarding any <c>Err</c> values.
    /// </summary>
    /// <typeparam name="T">The type of success values in the results.</typeparam>
    /// <typeparam name="E">The type of error values in the results.</typeparam>
    /// <param name="results">The collection of results to collect from.</param>
    /// <returns>A collection containing only the values from <c>Ok</c> results.</returns>
    /// <remarks>
    /// This method never fails - it always returns a collection (which may be empty if all results are <c>Err</c>).
    /// The returned collection preserves the order of <c>Ok</c> values from the input.
    /// </remarks>
    /// <example>
    /// <code>
    /// var results = new[]
    /// {
    ///     Result&lt;int, string&gt;.Ok(1),
    ///     Result&lt;int, string&gt;.Err("error"),
    ///     Result&lt;int, string&gt;.Ok(3)
    /// };
    /// var values = results.CollectOk(); // [1, 3]
    /// </code>
    /// </example>
    public static IEnumerable<T> CollectOk<T, E>(this IEnumerable<Result<T, E>> results)
    {
        foreach (var result in results)
        {
            if (result.TryGetValue(out var value))
            {
                yield return value;
            }
        }
    }

    /// <summary>
    /// Collects all <c>Err</c> values from a collection of results, discarding any <c>Ok</c> values.
    /// </summary>
    /// <typeparam name="T">The type of success values in the results.</typeparam>
    /// <typeparam name="E">The type of error values in the results.</typeparam>
    /// <param name="results">The collection of results to collect from.</param>
    /// <returns>A collection containing only the errors from <c>Err</c> results.</returns>
    /// <remarks>
    /// This method never fails - it always returns a collection (which may be empty if all results are <c>Ok</c>).
    /// The returned collection preserves the order of <c>Err</c> values from the input.
    /// </remarks>
    /// <example>
    /// <code>
    /// var results = new[]
    /// {
    ///     Result&lt;int, string&gt;.Ok(1),
    ///     Result&lt;int, string&gt;.Err("error1"),
    ///     Result&lt;int, string&gt;.Err("error2")
    /// };
    /// var errors = results.CollectErr(); // ["error1", "error2"]
    /// </code>
    /// </example>
    public static IEnumerable<E> CollectErr<T, E>(this IEnumerable<Result<T, E>> results)
    {
        foreach (var result in results)
        {
            if (result.TryGetError(out var error))
            {
                yield return error;
            }
        }
    }

    /// <summary>
    /// Partitions a collection of results into two collections: one containing all <c>Ok</c> values,
    /// and one containing all <c>Err</c> values.
    /// </summary>
    /// <typeparam name="T">The type of success values in the results.</typeparam>
    /// <typeparam name="E">The type of error values in the results.</typeparam>
    /// <param name="results">The collection of results to partition.</param>
    /// <returns>
    /// A tuple containing:
    /// - A list of all values from <c>Ok</c> results
    /// - A list of all errors from <c>Err</c> results
    /// </returns>
    /// <example>
    /// <code>
    /// var results = new[]
    /// {
    ///     Result&lt;int, string&gt;.Ok(1),
    ///     Result&lt;int, string&gt;.Err("error1"),
    ///     Result&lt;int, string&gt;.Ok(3),
    ///     Result&lt;int, string&gt;.Err("error2")
    /// };
    /// var (successes, failures) = results.PartitionResults();
    /// // successes = [1, 3], failures = ["error1", "error2"]
    /// </code>
    /// </example>
    public static (List<T> successes, List<E> failures) PartitionResults<T, E>(
        this IEnumerable<Result<T, E>> results)
    {
        var successes = new List<T>();
        var failures = new List<E>();
        
        foreach (var result in results)
        {
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

    #region Either Collection Extensions

    /// <summary>
    /// Transforms a collection of eithers into an either of a collection of lefts.
    /// Returns <c>Left</c> containing all left values if all eithers are <c>Left</c>;
    /// otherwise, returns the first <c>Right</c> encountered.
    /// </summary>
    /// <typeparam name="L">The type of left values in the eithers.</typeparam>
    /// <typeparam name="R">The type of right values in the eithers.</typeparam>
    /// <param name="eithers">The collection of eithers to sequence.</param>
    /// <returns>
    /// <c>Left</c> containing a list of all left values if all eithers are <c>Left</c>;
    /// otherwise, <c>Right</c> containing the first right value encountered.
    /// </returns>
    /// <remarks>
    /// This operation short-circuits on the first <c>Right</c> encountered.
    /// The returned collection preserves the order of the input collection.
    /// </remarks>
    /// <example>
    /// <code>
    /// var allLefts = new[]
    /// {
    ///     new Either&lt;int, string&gt;.Left(1),
    ///     new Either&lt;int, string&gt;.Left(2),
    ///     new Either&lt;int, string&gt;.Left(3)
    /// };
    /// var result = allLefts.SequenceLeft(); // Left([1, 2, 3])
    /// 
    /// var withRight = new[]
    /// {
    ///     new Either&lt;int, string&gt;.Left(1),
    ///     new Either&lt;int, string&gt;.Right("error"),
    ///     new Either&lt;int, string&gt;.Left(3)
    /// };
    /// var result2 = withRight.SequenceLeft(); // Right("error")
    /// </code>
    /// </example>
    public static Either<IEnumerable<L>, R> SequenceLeft<L, R>(this IEnumerable<Either<L, R>> eithers)
    {
        var values = new List<L>();

        foreach (var either in eithers)
        {
            if (either is Either<L, R>.Left left)
            {
                values.Add(left.Value);
            }
            else if (either is Either<L, R>.Right right)
            {
                return new Either<IEnumerable<L>, R>.Right(right.Value);
            }
        }

        return new Either<IEnumerable<L>, R>.Left(values);
    }

    /// <summary>
    /// Transforms a collection of eithers into an either of a collection of rights.
    /// Returns <c>Right</c> containing all right values if all eithers are <c>Right</c>;
    /// otherwise, returns the first <c>Left</c> encountered.
    /// </summary>
    /// <typeparam name="L">The type of left values in the eithers.</typeparam>
    /// <typeparam name="R">The type of right values in the eithers.</typeparam>
    /// <param name="eithers">The collection of eithers to sequence.</param>
    /// <returns>
    /// <c>Right</c> containing a list of all right values if all eithers are <c>Right</c>;
    /// otherwise, <c>Left</c> containing the first left value encountered.
    /// </returns>
    /// <remarks>
    /// This operation short-circuits on the first <c>Left</c> encountered.
    /// The returned collection preserves the order of the input collection.
    /// </remarks>
    /// <example>
    /// <code>
    /// var allRights = new[]
    /// {
    ///     new Either&lt;string, int&gt;.Right(1),
    ///     new Either&lt;string, int&gt;.Right(2),
    ///     new Either&lt;string, int&gt;.Right(3)
    /// };
    /// var result = allRights.SequenceRight(); // Right([1, 2, 3])
    /// 
    /// var withLeft = new[]
    /// {
    ///     new Either&lt;string, int&gt;.Right(1),
    ///     new Either&lt;string, int&gt;.Left("error"),
    ///     new Either&lt;string, int&gt;.Right(3)
    /// };
    /// var result2 = withLeft.SequenceRight(); // Left("error")
    /// </code>
    /// </example>
    public static Either<L, IEnumerable<R>> SequenceRight<L, R>(this IEnumerable<Either<L, R>> eithers)
    {
        var values = new List<R>();

        foreach (var either in eithers)
        {
            if (either is Either<L, R>.Right right)
            {
                values.Add(right.Value);
            }
            else if (either is Either<L, R>.Left left)
            {
                return new Either<L, IEnumerable<R>>.Left(left.Value);
            }
        }

        return new Either<L, IEnumerable<R>>.Right(values);
    }

    /// <summary>
    /// Maps each element of a collection through a function that returns an either,
    /// then sequences the lefts.
    /// Returns <c>Left</c> containing all mapped left values if all operations produce <c>Left</c>;
    /// otherwise, returns the first <c>Right</c> encountered.
    /// </summary>
    /// <typeparam name="T">The type of elements in the source collection.</typeparam>
    /// <typeparam name="L">The type of left values in the resulting eithers.</typeparam>
    /// <typeparam name="R">The type of right values in the resulting eithers.</typeparam>
    /// <param name="source">The source collection to traverse.</param>
    /// <param name="selector">A function that transforms each element into an either.</param>
    /// <returns>
    /// <c>Left</c> containing a list of all transformed left values if all operations produce <c>Left</c>;
    /// otherwise, <c>Right</c> containing the first right value encountered.
    /// </returns>
    /// <remarks>
    /// This is a combination of <see cref="Enumerable.Select{TSource, TResult}(IEnumerable{TSource}, Func{TSource, TResult})"/>
    /// and <see cref="SequenceLeft{L, R}(IEnumerable{Either{L, R}})"/>.
    /// The operation short-circuits on the first <c>Right</c> returned by the selector.
    /// </remarks>
    /// <example>
    /// <code>
    /// var numbers = new[] { "1", "2", "3" };
    /// var result = numbers.TraverseLeft&lt;string, int, string&gt;(s =>
    ///     int.TryParse(s, out var n)
    ///         ? new Either&lt;int, string&gt;.Left(n)
    ///         : new Either&lt;int, string&gt;.Right($"Invalid: {s}")
    /// ); // Left([1, 2, 3])
    /// </code>
    /// </example>
    public static Either<IEnumerable<L>, R> TraverseLeft<T, L, R>(
        this IEnumerable<T> source,
        Func<T, Either<L, R>> selector)
    {
        var values = new List<L>();

        foreach (var item in source)
        {
            var either = selector(item);
            if (either is Either<L, R>.Left left)
            {
                values.Add(left.Value);
            }
            else if (either is Either<L, R>.Right right)
            {
                return new Either<IEnumerable<L>, R>.Right(right.Value);
            }
        }

        return new Either<IEnumerable<L>, R>.Left(values);
    }

    /// <summary>
    /// Maps each element of a collection through a function that returns an either,
    /// then sequences the rights.
    /// Returns <c>Right</c> containing all mapped right values if all operations produce <c>Right</c>;
    /// otherwise, returns the first <c>Left</c> encountered.
    /// </summary>
    /// <typeparam name="T">The type of elements in the source collection.</typeparam>
    /// <typeparam name="L">The type of left values in the resulting eithers.</typeparam>
    /// <typeparam name="R">The type of right values in the resulting eithers.</typeparam>
    /// <param name="source">The source collection to traverse.</param>
    /// <param name="selector">A function that transforms each element into an either.</param>
    /// <returns>
    /// <c>Right</c> containing a list of all transformed right values if all operations produce <c>Right</c>;
    /// otherwise, <c>Left</c> containing the first left value encountered.
    /// </returns>
    /// <remarks>
    /// This is a combination of <see cref="Enumerable.Select{TSource, TResult}(IEnumerable{TSource}, Func{TSource, TResult})"/>
    /// and <see cref="SequenceRight{L, R}(IEnumerable{Either{L, R}})"/>.
    /// The operation short-circuits on the first <c>Left</c> returned by the selector.
    /// </remarks>
    /// <example>
    /// <code>
    /// var numbers = new[] { "1", "2", "3" };
    /// var result = numbers.TraverseRight&lt;string, string, int&gt;(s =>
    ///     int.TryParse(s, out var n)
    ///         ? new Either&lt;string, int&gt;.Right(n)
    ///         : new Either&lt;string, int&gt;.Left($"Invalid: {s}")
    /// ); // Right([1, 2, 3])
    /// </code>
    /// </example>
    public static Either<L, IEnumerable<R>> TraverseRight<T, L, R>(
        this IEnumerable<T> source,
        Func<T, Either<L, R>> selector)
    {
        var values = new List<R>();

        foreach (var item in source)
        {
            var either = selector(item);
            if (either is Either<L, R>.Right right)
            {
                values.Add(right.Value);
            }
            else if (either is Either<L, R>.Left left)
            {
                return new Either<L, IEnumerable<R>>.Left(left.Value);
            }
        }

        return new Either<L, IEnumerable<R>>.Right(values);
    }

    #endregion

    #region Validation Collection Extensions

    /// <summary>
    /// Maps each element of a collection through a function that returns a validation,
    /// then sequences the results, accumulating ALL errors.
    /// Returns <c>Valid</c> containing all mapped values if all operations succeed;
    /// otherwise, returns <c>Invalid</c> containing all accumulated errors.
    /// </summary>
    /// <typeparam name="T">The type of elements in the source collection.</typeparam>
    /// <typeparam name="U">The type of success values in the resulting validations.</typeparam>
    /// <typeparam name="E">The type of error values in the resulting validations.</typeparam>
    /// <param name="source">The source collection to traverse.</param>
    /// <param name="validator">A function that validates each element.</param>
    /// <returns>
    /// <c>Valid</c> containing a list of all transformed values if all operations succeed;
    /// otherwise, <c>Invalid</c> containing all accumulated errors.
    /// </returns>
    /// <remarks>
    /// This is the validation equivalent of <see cref="Traverse{T, U, E}(IEnumerable{T}, Func{T, Result{U, E}})"/>,
    /// but unlike Result's Traverse, this collects ALL errors instead of short-circuiting.
    /// Perfect for validating all fields in a form and showing all errors at once.
    /// Note: There is also a <see cref="ValidationExtensions.Sequence{T, E}(IEnumerable{Validation{T, E}})"/> method
    /// for when you already have a collection of validations.
    /// </remarks>
    /// <example>
    /// <code>
    /// var inputs = new[] { "1", "invalid", "3", "bad" };
    /// var result = inputs.TraverseValidation&lt;string, int, string&gt;(s =>
    ///     int.TryParse(s, out var n)
    ///         ? Validation&lt;int, string&gt;.Valid(n)
    ///         : Validation&lt;int, string&gt;.Invalid($"Invalid: {s}")
    /// );
    /// // Invalid(["Invalid: invalid", "Invalid: bad"]) - both errors collected!
    /// </code>
    /// </example>
    public static Validation<IEnumerable<U>, E> TraverseValidation<T, U, E>(
        this IEnumerable<T> source,
        Func<T, Validation<U, E>> validator)
    {
        var values = new List<U>();
        var errors = new List<E>();

        foreach (var item in source)
        {
            var validation = validator(item);
            if (validation is Validation<U, E>.Success success)
            {
                values.Add(success.Value);
            }
            else if (validation is Validation<U, E>.Failure failure)
            {
                errors.AddRange(failure.Errors);
            }
        }

        if (errors.Count > 0)
        {
            return Validation<IEnumerable<U>, E>.Invalid(errors);
        }

        return Validation<IEnumerable<U>, E>.Valid(values);
    }

    /// <summary>
    /// Partitions a collection of validations into two collections: one containing all <c>Valid</c> values,
    /// and one containing all error collections from <c>Invalid</c> validations.
    /// </summary>
    /// <typeparam name="T">The type of success values in the validations.</typeparam>
    /// <typeparam name="E">The type of error values in the validations.</typeparam>
    /// <param name="validations">The collection of validations to partition.</param>
    /// <returns>
    /// A tuple containing:
    /// - A list of all values from <c>Valid</c> validations
    /// - A list of error collections from <c>Invalid</c> validations (each invalid validation's errors as a separate collection)
    /// </returns>
    /// <example>
    /// <code>
    /// var validations = new[]
    /// {
    ///     Validation&lt;int, string&gt;.Valid(1),
    ///     Validation&lt;int, string&gt;.Invalid("error1", "error2"),
    ///     Validation&lt;int, string&gt;.Valid(3),
    ///     Validation&lt;int, string&gt;.Invalid("error3")
    /// };
    /// var (valid, invalid) = validations.PartitionValidations();
    /// // valid = [1, 3]
    /// // invalid = [["error1", "error2"], ["error3"]]
    /// </code>
    /// </example>
    public static (List<T> valid, List<IReadOnlyList<E>> invalid) PartitionValidations<T, E>(
        this IEnumerable<Validation<T, E>> validations)
    {
        var valid = new List<T>();
        var invalid = new List<IReadOnlyList<E>>();

        foreach (var validation in validations)
        {
            if (validation is Validation<T, E>.Success success)
            {
                valid.Add(success.Value);
            }
            else if (validation is Validation<T, E>.Failure failure)
            {
                invalid.Add(failure.Errors);
            }
        }

        return (valid, invalid);
    }

    #endregion

    #region Utility Extensions

    /// <summary>
    /// Returns the first <c>Ok</c> result from a collection, or <c>Err</c> containing all accumulated errors
    /// if no <c>Ok</c> result is found.
    /// </summary>
    /// <typeparam name="T">The type of success values in the results.</typeparam>
    /// <typeparam name="E">The type of error values in the results.</typeparam>
    /// <param name="results">The collection of results to search.</param>
    /// <returns>
    /// The first <c>Ok</c> result if found; otherwise, <c>Err</c> containing all accumulated errors.
    /// </returns>
    /// <remarks>
    /// This method short-circuits on the first <c>Ok</c> found, but if none are found,
    /// it returns all errors encountered.
    /// </remarks>
    /// <example>
    /// <code>
    /// var results = new[]
    /// {
    ///     Result&lt;int, string&gt;.Err("error1"),
    ///     Result&lt;int, string&gt;.Ok(42),
    ///     Result&lt;int, string&gt;.Err("error2")
    /// };
    /// var first = results.FirstOk(); // Ok(42)
    /// 
    /// var allErrors = new[]
    /// {
    ///     Result&lt;int, string&gt;.Err("error1"),
    ///     Result&lt;int, string&gt;.Err("error2")
    /// };
    /// var firstFail = allErrors.FirstOk(); // Err(["error1", "error2"])
    /// </code>
    /// </example>
    public static Result<T, IEnumerable<E>> FirstOk<T, E>(this IEnumerable<Result<T, E>> results)
    {
        var errors = new List<E>();

        foreach (var result in results)
        {
            if (result.TryGetValue(out var value))
            {
                return Result<T, IEnumerable<E>>.Ok(value);
            }
            else if (result.TryGetError(out var error))
            {
                errors.Add(error);
            }
        }

        return Result<T, IEnumerable<E>>.Err(errors);
    }

    /// <summary>
    /// Returns the first <c>Some</c> option from a collection, or <c>None</c> if all options are <c>None</c>.
    /// </summary>
    /// <typeparam name="T">The type of values in the options.</typeparam>
    /// <param name="options">The collection of options to search.</param>
    /// <returns>The first <c>Some</c> option if found; otherwise, <c>None</c>.</returns>
    /// <remarks>
    /// This method short-circuits on the first <c>Some</c> found.
    /// </remarks>
    /// <example>
    /// <code>
    /// var options = new[]
    /// {
    ///     new Option&lt;int&gt;.None(),
    ///     new Option&lt;int&gt;.Some(42),
    ///     new Option&lt;int&gt;.Some(99)
    /// };
    /// var first = options.FirstSome(); // Some(42)
    /// </code>
    /// </example>
    public static Option<T> FirstSome<T>(this IEnumerable<Option<T>> options)
    {
        foreach (var option in options)
        {
            if (option is Option<T>.Some)
            {
                return option;
            }
        }

        return new Option<T>.None();
    }

    /// <summary>
    /// Applies a selector function to each element in a collection and returns the first <c>Some</c> result,
    /// or <c>None</c> if all results are <c>None</c>.
    /// </summary>
    /// <typeparam name="T">The type of elements in the source collection.</typeparam>
    /// <typeparam name="U">The type of values in the resulting options.</typeparam>
    /// <param name="source">The source collection to search.</param>
    /// <param name="selector">A function that transforms each element into an option.</param>
    /// <returns>The first <c>Some</c> result from the selector; otherwise, <c>None</c>.</returns>
    /// <remarks>
    /// This method is similar to LINQ's FirstOrDefault, but returns an Option instead of null.
    /// It short-circuits on the first <c>Some</c> result from the selector.
    /// This is particularly useful for finding the first valid transformation of a collection element.
    /// </remarks>
    /// <example>
    /// <code>
    /// var strings = new[] { "invalid", "42", "99" };
    /// var first = strings.Choose(s =>
    ///     int.TryParse(s, out var n)
    ///         ? new Option&lt;int&gt;.Some(n)
    ///         : new Option&lt;int&gt;.None()
    /// ); // Some(42)
    /// </code>
    /// </example>
    public static Option<U> Choose<T, U>(this IEnumerable<T> source, Func<T, Option<U>> selector)
    {
        foreach (var item in source)
        {
            var option = selector(item);
            if (option is Option<U>.Some)
            {
                return option;
            }
        }

        return new Option<U>.None();
    }

    /// <summary>
    /// Transforms a collection of results into a result of a collection,
    /// accumulating ALL errors instead of short-circuiting on the first error.
    /// Returns <c>Ok</c> containing all values if all results are <c>Ok</c>;
    /// otherwise, returns <c>Err</c> containing all accumulated errors.
    /// </summary>
    /// <typeparam name="T">The type of success values in the results.</typeparam>
    /// <typeparam name="E">The type of error values in the results.</typeparam>
    /// <param name="results">The collection of results to sequence.</param>
    /// <returns>
    /// <c>Ok</c> containing a list of all success values if all results are <c>Ok</c>;
    /// otherwise, <c>Err</c> containing all accumulated errors.
    /// </returns>
    /// <remarks>
    /// Unlike <see cref="Sequence{T, E}(IEnumerable{Result{T, E}})"/> which short-circuits on the first error,
    /// this method continues processing all results and accumulates all errors.
    /// This is useful when you want to see all failures at once, similar to how Validation works.
    /// </remarks>
    /// <example>
    /// <code>
    /// var results = new[]
    /// {
    ///     Result&lt;int, string&gt;.Ok(1),
    ///     Result&lt;int, string&gt;.Err("error1"),
    ///     Result&lt;int, string&gt;.Ok(3),
    ///     Result&lt;int, string&gt;.Err("error2")
    /// };
    /// var combined = results.SequenceAll(); 
    /// // Err(["error1", "error2"]) - both errors collected!
    /// 
    /// // Compare with regular Sequence:
    /// var shortCircuit = results.Sequence(); 
    /// // Err("error1") - stops at first error
    /// </code>
    /// </example>
    public static Result<IEnumerable<T>, IEnumerable<E>> SequenceAll<T, E>(
        this IEnumerable<Result<T, E>> results)
    {
        var values = new List<T>();
        var errors = new List<E>();

        foreach (var result in results)
        {
            if (result.TryGetValue(out var value))
            {
                values.Add(value);
            }
            else if (result.TryGetError(out var error))
            {
                errors.Add(error);
            }
        }

        if (errors.Count > 0)
        {
            return Result<IEnumerable<T>, IEnumerable<E>>.Err(errors);
        }

        return Result<IEnumerable<T>, IEnumerable<E>>.Ok(values);
    }

    /// <summary>
    /// Returns true if any result in the collection is <c>Ok</c>.
    /// </summary>
    /// <typeparam name="T">The type of success values in the results.</typeparam>
    /// <typeparam name="E">The type of error values in the results.</typeparam>
    /// <param name="results">The collection of results to check.</param>
    /// <returns>true if at least one result is <c>Ok</c>; otherwise, false.</returns>
    /// <remarks>This method short-circuits on the first <c>Ok</c> found.</remarks>
    /// <example>
    /// <code>
    /// var results = new[]
    /// {
    ///     Result&lt;int, string&gt;.Err("error1"),
    ///     Result&lt;int, string&gt;.Ok(42),
    ///     Result&lt;int, string&gt;.Err("error2")
    /// };
    /// bool hasSuccess = results.AnyOk(); // true
    /// </code>
    /// </example>
    public static bool AnyOk<T, E>(this IEnumerable<Result<T, E>> results)
    {
        foreach (var result in results)
        {
            if (result.IsSuccess)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Returns true if all results in the collection are <c>Ok</c>.
    /// </summary>
    /// <typeparam name="T">The type of success values in the results.</typeparam>
    /// <typeparam name="E">The type of error values in the results.</typeparam>
    /// <param name="results">The collection of results to check.</param>
    /// <returns>true if all results are <c>Ok</c>; otherwise, false.</returns>
    /// <remarks>
    /// This method short-circuits on the first <c>Err</c> found.
    /// Returns true for an empty collection.
    /// </remarks>
    /// <example>
    /// <code>
    /// var results = new[]
    /// {
    ///     Result&lt;int, string&gt;.Ok(1),
    ///     Result&lt;int, string&gt;.Ok(2),
    ///     Result&lt;int, string&gt;.Ok(3)
    /// };
    /// bool allSuccess = results.AllOk(); // true
    /// </code>
    /// </example>
    public static bool AllOk<T, E>(this IEnumerable<Result<T, E>> results)
    {
        foreach (var result in results)
        {
            if (result.IsFailure)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Returns true if any option in the collection is <c>Some</c>.
    /// </summary>
    /// <typeparam name="T">The type of values in the options.</typeparam>
    /// <param name="options">The collection of options to check.</param>
    /// <returns>true if at least one option is <c>Some</c>; otherwise, false.</returns>
    /// <remarks>This method short-circuits on the first <c>Some</c> found.</remarks>
    /// <example>
    /// <code>
    /// var options = new[]
    /// {
    ///     new Option&lt;int&gt;.None(),
    ///     new Option&lt;int&gt;.Some(42),
    ///     new Option&lt;int&gt;.None()
    /// };
    /// bool hasValue = options.AnySome(); // true
    /// </code>
    /// </example>
    public static bool AnySome<T>(this IEnumerable<Option<T>> options)
    {
        foreach (var option in options)
        {
            if (option is Option<T>.Some)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Returns true if all options in the collection are <c>Some</c>.
    /// </summary>
    /// <typeparam name="T">The type of values in the options.</typeparam>
    /// <param name="options">The collection of options to check.</param>
    /// <returns>true if all options are <c>Some</c>; otherwise, false.</returns>
    /// <remarks>
    /// This method short-circuits on the first <c>None</c> found.
    /// Returns true for an empty collection.
    /// </remarks>
    /// <example>
    /// <code>
    /// var options = new[]
    /// {
    ///     new Option&lt;int&gt;.Some(1),
    ///     new Option&lt;int&gt;.Some(2),
    ///     new Option&lt;int&gt;.Some(3)
    /// };
    /// bool allPresent = options.AllSome(); // true
    /// </code>
    /// </example>
    public static bool AllSome<T>(this IEnumerable<Option<T>> options)
    {
        foreach (var option in options)
        {
            if (option is Option<T>.None)
            {
                return false;
            }
        }
        return true;
    }

    #endregion

    #region Dictionary Extensions

    /// <summary>
    /// Converts a collection of results containing key-value pairs to a dictionary,
    /// discarding any <c>Err</c> results.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    /// <typeparam name="E">The type of error values in the results.</typeparam>
    /// <param name="results">The collection of results to convert.</param>
    /// <returns>A dictionary containing only the key-value pairs from <c>Ok</c> results.</returns>
    /// <remarks>
    /// If there are duplicate keys in the successful results, only the first occurrence is kept.
    /// Failed results are silently discarded.
    /// </remarks>
    /// <example>
    /// <code>
    /// var results = new[]
    /// {
    ///     Result&lt;KeyValuePair&lt;string, int&gt;, string&gt;.Ok(new("a", 1)),
    ///     Result&lt;KeyValuePair&lt;string, int&gt;, string&gt;.Err("error"),
    ///     Result&lt;KeyValuePair&lt;string, int&gt;, string&gt;.Ok(new("b", 2))
    /// };
    /// var dict = results.ToOkDictionary(); // {"a": 1, "b": 2}
    /// </code>
    /// </example>
    public static Dictionary<TKey, TValue> ToOkDictionary<TKey, TValue, E>(
        this IEnumerable<Result<KeyValuePair<TKey, TValue>, E>> results)
        where TKey : notnull
    {
        var dictionary = new Dictionary<TKey, TValue>();

        foreach (var result in results)
        {
            if (result.TryGetValue(out var kvp))
            {
                dictionary.TryAdd(kvp.Key, kvp.Value);
            }
        }

        return dictionary;
    }

    /// <summary>
    /// Converts a collection of options containing key-value pairs to a dictionary,
    /// discarding any <c>None</c> options.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    /// <param name="options">The collection of options to convert.</param>
    /// <returns>A dictionary containing only the key-value pairs from <c>Some</c> options.</returns>
    /// <remarks>
    /// If there are duplicate keys in the Some options, only the first occurrence is kept.
    /// None options are silently discarded.
    /// </remarks>
    /// <example>
    /// <code>
    /// var options = new[]
    /// {
    ///     new Option&lt;KeyValuePair&lt;string, int&gt;&gt;.Some(new("a", 1)),
    ///     new Option&lt;KeyValuePair&lt;string, int&gt;&gt;.None(),
    ///     new Option&lt;KeyValuePair&lt;string, int&gt;&gt;.Some(new("b", 2))
    /// };
    /// var dict = options.ToSomeDictionary(); // {"a": 1, "b": 2}
    /// </code>
    /// </example>
    public static Dictionary<TKey, TValue> ToSomeDictionary<TKey, TValue>(
        this IEnumerable<Option<KeyValuePair<TKey, TValue>>> options)
        where TKey : notnull
    {
        var dictionary = new Dictionary<TKey, TValue>();

        foreach (var option in options)
        {
            if (option is Option<KeyValuePair<TKey, TValue>>.Some some)
            {
                dictionary.TryAdd(some.Value.Key, some.Value.Value);
            }
        }

        return dictionary;
    }

    #endregion
}
