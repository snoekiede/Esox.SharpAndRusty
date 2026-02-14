using Esox.SharpAndRusty.Types;

namespace Esox.SharpAndRusty.Extensions;

/// <summary>
/// Provides extension methods for working with <see cref="Either{L, R}"/> types.
/// </summary>
public static class EitherExtensions
{
    /// <summary>
    /// Chains together operations that return Either values, operating on the Right value.
    /// </summary>
    /// <typeparam name="L">The type of the Left value.</typeparam>
    /// <typeparam name="R">The type of the current Right value.</typeparam>
    /// <typeparam name="R2">The type of the new Right value.</typeparam>
    /// <param name="either">The either to bind.</param>
    /// <param name="binder">A function that transforms the Right value into a new Either.</param>
    /// <returns>The result of the binder if Right, otherwise the original Left value.</returns>
    public static Either<L, R2> BindRight<L, R, R2>(
        this Either<L, R> either,
        Func<R, Either<L, R2>> binder)
    {
        if (binder is null) throw new ArgumentNullException(nameof(binder));

        return either switch
        {
            Either<L, R>.Left left => new Either<L, R2>.Left(left.Value),
            Either<L, R>.Right right => binder(right.Value),
            _ => throw new InvalidOperationException("Either is in an invalid state.")
        };
    }

    /// <summary>
    /// Chains together operations that return Either values, operating on the Left value.
    /// </summary>
    /// <typeparam name="L">The type of the current Left value.</typeparam>
    /// <typeparam name="R">The type of the Right value.</typeparam>
    /// <typeparam name="L2">The type of the new Left value.</typeparam>
    /// <param name="either">The either to bind.</param>
    /// <param name="binder">A function that transforms the Left value into a new Either.</param>
    /// <returns>The result of the binder if Left, otherwise the original Right value.</returns>
    public static Either<L2, R> BindLeft<L, R, L2>(
        this Either<L, R> either,
        Func<L, Either<L2, R>> binder)
    {
        if (binder is null) throw new ArgumentNullException(nameof(binder));

        return either switch
        {
            Either<L, R>.Left left => binder(left.Value),
            Either<L, R>.Right right => new Either<L2, R>.Right(right.Value),
            _ => throw new InvalidOperationException("Either is in an invalid state.")
        };
    }

    /// <summary>
    /// Executes an action if the Either is Left.
    /// </summary>
    /// <typeparam name="L">The type of the Left value.</typeparam>
    /// <typeparam name="R">The type of the Right value.</typeparam>
    /// <param name="either">The either to inspect.</param>
    /// <param name="action">The action to execute on the Left value.</param>
    /// <returns>The original either for method chaining.</returns>
    public static Either<L, R> IfLeft<L, R>(this Either<L, R> either, Action<L> action)
    {
        if (action is null) throw new ArgumentNullException(nameof(action));

        if (either is Either<L, R>.Left left)
        {
            action(left.Value);
        }

        return either;
    }

    /// <summary>
    /// Executes an action if the Either is Right.
    /// </summary>
    /// <typeparam name="L">The type of the Left value.</typeparam>
    /// <typeparam name="R">The type of the Right value.</typeparam>
    /// <param name="either">The either to inspect.</param>
    /// <param name="action">The action to execute on the Right value.</param>
    /// <returns>The original either for method chaining.</returns>
    public static Either<L, R> IfRight<L, R>(this Either<L, R> either, Action<R> action)
    {
        if (action is null) throw new ArgumentNullException(nameof(action));

        if (either is Either<L, R>.Right right)
        {
            action(right.Value);
        }

        return either;
    }

    /// <summary>
    /// Gets the Left value or returns a default value.
    /// </summary>
    /// <typeparam name="L">The type of the Left value.</typeparam>
    /// <typeparam name="R">The type of the Right value.</typeparam>
    /// <param name="either">The either to extract the value from.</param>
    /// <param name="defaultValue">The value to return if the either is Right.</param>
    /// <returns>The Left value if present, otherwise <paramref name="defaultValue"/>.</returns>
    public static L GetLeftOrDefault<L, R>(this Either<L, R> either, L defaultValue)
    {
        return either is Either<L, R>.Left left ? left.Value : defaultValue;
    }

    /// <summary>
    /// Gets the Right value or returns a default value.
    /// </summary>
    /// <typeparam name="L">The type of the Left value.</typeparam>
    /// <typeparam name="R">The type of the Right value.</typeparam>
    /// <param name="either">The either to extract the value from.</param>
    /// <param name="defaultValue">The value to return if the either is Left.</param>
    /// <returns>The Right value if present, otherwise <paramref name="defaultValue"/>.</returns>
    public static R GetRightOrDefault<L, R>(this Either<L, R> either, R defaultValue)
    {
        return either is Either<L, R>.Right right ? right.Value : defaultValue;
    }

    /// <summary>
    /// Converts a collection of Eithers into an Either of two collections.
    /// </summary>
    /// <typeparam name="L">The type of the Left values.</typeparam>
    /// <typeparam name="R">The type of the Right values.</typeparam>
    /// <param name="eithers">The collection of eithers to partition.</param>
    /// <returns>
    /// A tuple containing:
    /// - A list of all Left values
    /// - A list of all Right values
    /// </returns>
    public static (List<L> lefts, List<R> rights) Partition<L, R>(
        this IEnumerable<Either<L, R>> eithers)
    {
        var lefts = new List<L>();
        var rights = new List<R>();

        foreach (var either in eithers)
        {
            switch (either)
            {
                case Either<L, R>.Left left:
                    lefts.Add(left.Value);
                    break;
                case Either<L, R>.Right right:
                    rights.Add(right.Value);
                    break;
            }
        }

        return (lefts, rights);
    }

    /// <summary>
    /// Collects all Left values from a collection of Eithers.
    /// </summary>
    /// <typeparam name="L">The type of the Left values.</typeparam>
    /// <typeparam name="R">The type of the Right values.</typeparam>
    /// <param name="eithers">The collection of eithers to collect from.</param>
    /// <returns>A collection containing only the Left values.</returns>
    public static IEnumerable<L> Lefts<L, R>(this IEnumerable<Either<L, R>> eithers)
    {
        foreach (var either in eithers)
        {
            if (either is Either<L, R>.Left left)
            {
                yield return left.Value;
            }
        }
    }

    /// <summary>
    /// Collects all Right values from a collection of Eithers.
    /// </summary>
    /// <typeparam name="L">The type of the Left values.</typeparam>
    /// <typeparam name="R">The type of the Right values.</typeparam>
    /// <param name="eithers">The collection of eithers to collect from.</param>
    /// <returns>A collection containing only the Right values.</returns>
    public static IEnumerable<R> Rights<L, R>(this IEnumerable<Either<L, R>> eithers)
    {
        foreach (var either in eithers)
        {
            if (either is Either<L, R>.Right right)
            {
                yield return right.Value;
            }
        }
    }
}
