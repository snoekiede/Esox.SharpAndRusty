using Esox.SharpAndRusty.Types;

namespace Esox.SharpAndRusty.Extensions;

/// <summary>
/// Provides LINQ query syntax support for the <see cref="Option{T}"/> type.
/// Enables using <c>from</c>/<c>select</c> syntax with options.
/// </summary>
public static class OptionLinqExtensions
{
    /// <summary>
    /// Projects the value of an option into a new form. This method enables LINQ query syntax.
    /// </summary>
    /// <typeparam name="T">The type of the value in the option.</typeparam>
    /// <typeparam name="TResult">The type of the value returned by <paramref name="selector"/>.</typeparam>
    /// <param name="option">The option whose value to project.</param>
    /// <param name="selector">A transform function to apply to the contained value.</param>
    /// <returns>
    /// An option whose value is the result of invoking the transform function on the value of the source option,
    /// or <c>None</c> if the source option is <c>None</c>.
    /// </returns>
    /// <remarks>
    /// This method is equivalent to <see cref="OptionExtensions.Map{T, TResult}(Option{T}, Func{T, TResult})"/>.
    /// It exists to enable LINQ query syntax (<c>select</c> keyword).
    /// </remarks>
    /// <example>
    /// <code>
    /// var option = new Option&lt;int&gt;.Some(42);
    /// 
    /// // LINQ query syntax
    /// var result = from x in option
    ///              select x * 2;
    /// // result is Some(84)
    /// 
    /// // Equivalent to:
    /// var result2 = option.Select(x => x * 2);
    /// </code>
    /// </example>
    public static Option<TResult> Select<T, TResult>(
        this Option<T> option,
        Func<T, TResult> selector)
    {
        if (selector is null) throw new ArgumentNullException(nameof(selector));
        
        return option is Option<T>.Some some
            ? new Option<TResult>.Some(selector(some.Value))
            : new Option<TResult>.None();
    }

    /// <summary>
    /// Projects the value of an option to another option and invokes a result selector function on the pair.
    /// This method enables LINQ query syntax with multiple <c>from</c> clauses.
    /// </summary>
    /// <typeparam name="T">The type of the value in the source option.</typeparam>
    /// <typeparam name="TCollection">The type of the intermediate value collected by <paramref name="collectionSelector"/>.</typeparam>
    /// <typeparam name="TResult">The type of the resulting value.</typeparam>
    /// <param name="option">The source option.</param>
    /// <param name="collectionSelector">A transform function to apply to the value of the source option.</param>
    /// <param name="resultSelector">A transform function to apply to the intermediate values.</param>
    /// <returns>
    /// An option whose value is the result of invoking the collection selector and result selector,
    /// or <c>None</c> if either the source option or the collection selector result is <c>None</c>.
    /// </returns>
    /// <remarks>
    /// This method is equivalent to <see cref="OptionExtensions.Bind{T, TResult}(Option{T}, Func{T, Option{TResult}})"/>
    /// followed by a map operation. It exists to enable LINQ query syntax with multiple <c>from</c> clauses.
    /// </remarks>
    /// <example>
    /// <code>
    /// var userOption = new Option&lt;int&gt;.Some(42);
    /// 
    /// // LINQ query syntax
    /// var result = from userId in userOption
    ///              from orders in GetOrders(userId)
    ///              select new { userId, orders };
    /// 
    /// // Equivalent to:
    /// var result2 = userOption.SelectMany(
    ///     userId => GetOrders(userId),
    ///     (userId, orders) => new { userId, orders });
    /// </code>
    /// </example>
    public static Option<TResult> SelectMany<T, TCollection, TResult>(
        this Option<T> option,
        Func<T, Option<TCollection>> collectionSelector,
        Func<T, TCollection, TResult> resultSelector)
    {
        if (collectionSelector is null) throw new ArgumentNullException(nameof(collectionSelector));
        if (resultSelector is null) throw new ArgumentNullException(nameof(resultSelector));
        
        if (option is not Option<T>.Some some)
            return new Option<TResult>.None();
        
        var collectionOption = collectionSelector(some.Value);
        
        return collectionOption is Option<TCollection>.Some collectionSome
            ? new Option<TResult>.Some(resultSelector(some.Value, collectionSome.Value))
            : new Option<TResult>.None();
    }

    /// <summary>
    /// Projects the value of an option to another option. This overload enables simple chaining with LINQ.
    /// </summary>
    /// <typeparam name="T">The type of the value in the source option.</typeparam>
    /// <typeparam name="TResult">The type of the value in the resulting option.</typeparam>
    /// <param name="option">The source option.</param>
    /// <param name="selector">A transform function to apply to the value of the source option.</param>
    /// <returns>
    /// The option returned by <paramref name="selector"/>, or <c>None</c> if the source option is <c>None</c>.
    /// </returns>
    /// <remarks>
    /// This method is equivalent to <see cref="OptionExtensions.Bind{T, TResult}(Option{T}, Func{T, Option{TResult}})"/>.
    /// It enables simpler LINQ expressions when you don't need the two-parameter result selector.
    /// </remarks>
    /// <example>
    /// <code>
    /// var userIdOption = new Option&lt;int&gt;.Some(42);
    /// 
    /// var result = from userId in userIdOption
    ///              from user in GetUser(userId)
    ///              from profile in GetProfile(user)
    ///              select profile.Name;
    /// </code>
    /// </example>
    public static Option<TResult> SelectMany<T, TResult>(
        this Option<T> option,
        Func<T, Option<TResult>> selector)
    {
        if (selector is null) throw new ArgumentNullException(nameof(selector));
        
        return option is Option<T>.Some some
            ? selector(some.Value)
            : new Option<TResult>.None();
    }

    /// <summary>
    /// Filters the option based on a predicate. This method enables LINQ query syntax filtering with the <c>where</c> keyword.
    /// </summary>
    /// <typeparam name="T">The type of the value in the option.</typeparam>
    /// <param name="option">The option to filter.</param>
    /// <param name="predicate">A function to test the contained value.</param>
    /// <returns>
    /// The original option if it is <c>Some</c> and the predicate returns <c>true</c>; otherwise, <c>None</c>.
    /// </returns>
    /// <remarks>
    /// This method is equivalent to <see cref="OptionExtensions.Filter{T}(Option{T}, Func{T, bool})"/>.
    /// It exists to enable LINQ query syntax (<c>where</c> keyword).
    /// </remarks>
    /// <example>
    /// <code>
    /// var option = new Option&lt;int&gt;.Some(42);
    /// 
    /// // LINQ query syntax
    /// var result = from x in option
    ///              where x > 10
    ///              select x * 2;
    /// // result is Some(84)
    /// 
    /// var result2 = from x in option
    ///               where x > 100
    ///               select x * 2;
    /// // result2 is None
    /// </code>
    /// </example>
    public static Option<T> Where<T>(
        this Option<T> option,
        Func<T, bool> predicate)
    {
        if (predicate is null) throw new ArgumentNullException(nameof(predicate));
        
        return option is Option<T>.Some some && predicate(some.Value)
            ? option
            : new Option<T>.None();
    }
}
