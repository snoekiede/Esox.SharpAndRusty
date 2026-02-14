using Esox.SharpAndRusty.Types;

namespace Esox.SharpAndRusty.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="Option{T}"/> type.
/// </summary>
public static class OptionExtensions
{
    extension<T>(Option<T> option)
    {
        /// <summary>
        /// Determines whether the option contains a value.
        /// </summary>
        /// <returns><c>true</c> if the option is <c>Some</c>; otherwise, <c>false</c>.</returns>
        public bool IsSome() =>
            option is Option<T>.Some;
        
        /// <summary>
        /// Determines whether the option is empty (contains no value).
        /// </summary>
        /// <returns><c>true</c> if the option is <c>None</c>; otherwise, <c>false</c>.</returns>
        public bool IsNone() => option is Option<T>.None;
        
        /// <summary>
        /// Gets the value contained in the option, or returns a default value if the option is <c>None</c>.
        /// </summary>
        /// <param name="defaultValue">The default value to return if the option is <c>None</c>.</param>
        /// <returns>The value contained in the option if it is <c>Some</c>; otherwise, <paramref name="defaultValue"/>.</returns>
        public T GetValueOrDefault(T defaultValue) =>
            option is Option<T>.Some some ? some.Value : defaultValue;
        
        /// <summary>
        /// Gets the value contained in the option, or invokes a factory function to produce a default value if the option is <c>None</c>.
        /// </summary>
        /// <param name="defaultFactory">A function that produces a default value if the option is <c>None</c>.</param>
        /// <returns>The value contained in the option if it is <c>Some</c>; otherwise, the result of invoking <paramref name="defaultFactory"/>.</returns>
        public T GetValueOrElse(Func<T> defaultFactory) => 
            option is Option<T>.Some some ? some.Value : defaultFactory();
        
        /// <summary>
        /// Transforms the value contained in the option using a mapping function.
        /// </summary>
        /// <typeparam name="TResult">The type of the transformed value.</typeparam>
        /// <param name="mapper">A function that transforms the value from type <typeparamref name="T"/> to type <typeparamref name="TResult"/>.</param>
        /// <returns>A new <see cref="Option{TResult}"/> containing the transformed value if the option is <c>Some</c>; otherwise, <c>None</c>.</returns>
        public Option<TResult> Map<TResult>(Func<T,TResult> mapper) =>
            option is Option<T>.Some some 
                ? new Option<TResult>.Some(mapper(some.Value)) 
                : new Option<TResult>.None();
        
        /// <summary>
        /// Chains together multiple operations that return <see cref="Option{T}"/> values (also known as flatMap or chain).
        /// </summary>
        /// <typeparam name="TResult">The type of the value in the resulting option.</typeparam>
        /// <param name="binder">A function that takes the current value and returns a new <see cref="Option{TResult}"/>.</param>
        /// <returns>The <see cref="Option{TResult}"/> returned by <paramref name="binder"/> if the option is <c>Some</c>; otherwise, <c>None</c>.</returns>
        public Option<TResult> Bind<TResult>(Func<T, Option<TResult>> binder) =>
            option is Option<T>.Some some 
                ? binder(some.Value) 
                : new Option<TResult>.None();

        /// <summary>
        /// Executes one of two actions based on whether the option contains a value.
        /// </summary>
        /// <param name="onSome">The action to execute if the option is <c>Some</c>, receiving the contained value.</param>
        /// <param name="onNone">The action to execute if the option is <c>None</c>.</param>
        public void Match(Action<T> onSome, Action onNone)
        {
            if (option is Option<T>.Some some)
            {
                onSome(some.Value);
            }
            else
            {
                onNone();
            }
        }
        
        /// <summary>
        /// Executes one of two functions based on whether the option contains a value and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="onSome">The function to execute if the option is <c>Some</c>, receiving the contained value.</param>
        /// <param name="onNone">The function to execute if the option is <c>None</c>.</param>
        /// <returns>The result of executing either <paramref name="onSome"/> or <paramref name="onNone"/>.</returns>
        public TResult Match<TResult>(Func<T,TResult> onSome,Func<TResult> onNone) =>
            option is Option<T>.Some some
                ? onSome(some.Value)
                : onNone();

        /// <summary>
        /// Filters the option based on a predicate. Returns <c>Some</c> if the option is <c>Some</c> and the predicate returns <c>true</c>; otherwise, returns <c>None</c>.
        /// </summary>
        /// <param name="predicate">A function to test the contained value.</param>
        /// <returns><c>Some</c> if the option is <c>Some</c> and the predicate returns <c>true</c>; otherwise, <c>None</c>.</returns>
        public Option<T> Filter(Func<T, bool> predicate) =>
            option is Option<T>.Some some && predicate(some.Value)
                ? option
                : new Option<T>.None();

        /// <summary>
        /// Combines two options into a single option containing a tuple of both values.
        /// Returns <c>Some</c> only if both options are <c>Some</c>.
        /// </summary>
        /// <typeparam name="U">The type of the value in the other option.</typeparam>
        /// <param name="other">The option to combine with this option.</param>
        /// <returns>An option containing a tuple of both values if both are <c>Some</c>; otherwise, <c>None</c>.</returns>
        public Option<(T, U)> Zip<U>(Option<U> other) =>
            option is Option<T>.Some some && other is Option<U>.Some otherSome
                ? new Option<(T, U)>.Some((some.Value, otherSome.Value))
                : new Option<(T, U)>.None();

        /// <summary>
        /// Combines two options using a zipper function. Returns <c>Some</c> only if both options are <c>Some</c>.
        /// </summary>
        /// <typeparam name="U">The type of the value in the other option.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="other">The option to combine with this option.</param>
        /// <param name="zipper">A function that combines the two values.</param>
        /// <returns>An option containing the result of applying the zipper function if both options are <c>Some</c>; otherwise, <c>None</c>.</returns>
        public Option<TResult> ZipWith<U, TResult>(Option<U> other, Func<T, U, TResult> zipper) =>
            option is Option<T>.Some some && other is Option<U>.Some otherSome
                ? new Option<TResult>.Some(zipper(some.Value, otherSome.Value))
                : new Option<TResult>.None();

        /// <summary>
        /// Returns the second option if this option is <c>Some</c>; otherwise, returns <c>None</c>.
        /// This is useful for chaining operations where both must succeed.
        /// </summary>
        /// <typeparam name="U">The type of the value in the other option.</typeparam>
        /// <param name="other">The option to return if this option is <c>Some</c>.</param>
        /// <returns>The <paramref name="other"/> option if this option is <c>Some</c>; otherwise, <c>None</c>.</returns>
        public Option<U> And<U>(Option<U> other) =>
            option.IsSome() ? other : new Option<U>.None();

        /// <summary>
        /// Returns this option if it is <c>Some</c>; otherwise, returns the alternative option.
        /// This provides a fallback mechanism.
        /// </summary>
        /// <param name="alternative">The option to return if this option is <c>None</c>.</param>
        /// <returns>This option if it is <c>Some</c>; otherwise, <paramref name="alternative"/>.</returns>
        public Option<T> Or(Option<T> alternative) =>
            option.IsSome() ? option : alternative;

        /// <summary>
        /// Returns <c>Some</c> if exactly one of the two options is <c>Some</c>; otherwise, returns <c>None</c>.
        /// This implements exclusive OR logic.
        /// </summary>
        /// <param name="other">The option to compare with this option.</param>
        /// <returns><c>Some</c> if exactly one option is <c>Some</c>; otherwise, <c>None</c>.</returns>
        public Option<T> Xor(Option<T> other) =>
            (option.IsSome(), other.IsSome()) switch
            {
                (true, false) => option,
                (false, true) => other,
                _ => new Option<T>.None()
            };

        /// <summary>
        /// Executes a side effect action on the contained value if the option is <c>Some</c>, then returns the option unchanged.
        /// Useful for debugging or logging.
        /// </summary>
        /// <param name="inspector">An action to execute on the contained value.</param>
        /// <returns>The original option, unchanged.</returns>
        public Option<T> Inspect(Action<T> inspector)
        {
            if (option is Option<T>.Some some)
            {
                inspector(some.Value);
            }
            return option;
        }

        /// <summary>
        /// Executes a side effect action if the option is <c>None</c>, then returns the option unchanged.
        /// Useful for debugging or logging when a value is absent.
        /// </summary>
        /// <param name="inspector">An action to execute if the option is <c>None</c>.</param>
        /// <returns>The original option, unchanged.</returns>
        public Option<T> InspectNone(Action inspector)
        {
            if (option.IsNone())
            {
                inspector();
            }
            return option;
        }

        /// <summary>
        /// Converts the option to a <see cref="Result{T, E}"/>.
        /// Returns <c>Ok</c> if the option is <c>Some</c>; otherwise, returns <c>Err</c> with the provided error.
        /// </summary>
        /// <typeparam name="E">The type of the error.</typeparam>
        /// <param name="error">The error to use if the option is <c>None</c>.</param>
        /// <returns>A result containing the value if <c>Some</c>; otherwise, a result containing the error.</returns>
        public Result<T, E> OkOr<E>(E error) =>
            option is Option<T>.Some some
                ? Result<T, E>.Ok(some.Value)
                : Result<T, E>.Err(error);

        /// <summary>
        /// Converts the option to a <see cref="Result{T, E}"/>.
        /// Returns <c>Ok</c> if the option is <c>Some</c>; otherwise, returns <c>Err</c> with an error produced by the factory.
        /// </summary>
        /// <typeparam name="E">The type of the error.</typeparam>
        /// <param name="errorFactory">A function that produces an error if the option is <c>None</c>.</param>
        /// <returns>A result containing the value if <c>Some</c>; otherwise, a result containing the produced error.</returns>
        public Result<T, E> OkOrElse<E>(Func<E> errorFactory) =>
            option is Option<T>.Some some
                ? Result<T, E>.Ok(some.Value)
                : Result<T, E>.Err(errorFactory());
    }

    /// <summary>
    /// Converts an option of a value type to a nullable value type.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="option">The option to convert.</param>
    /// <returns>The contained value if <c>Some</c>; otherwise, <c>null</c>.</returns>
    public static T? ToNullable<T>(this Option<T> option) where T : struct =>
        option is Option<T>.Some some ? some.Value : null;

    /// <summary>
    /// Flattens a nested option. If the outer option is <c>Some</c> and contains a <c>Some</c>, returns the inner <c>Some</c>.
    /// Otherwise, returns <c>None</c>.
    /// </summary>
    /// <typeparam name="T">The type of the innermost value.</typeparam>
    /// <param name="nested">The nested option to flatten.</param>
    /// <returns>The inner option if the outer is <c>Some</c>; otherwise, <c>None</c>.</returns>
    public static Option<T> Flatten<T>(this Option<Option<T>> nested) =>
        nested is Option<Option<T>>.Some some
            ? some.Value
            : new Option<T>.None();
}