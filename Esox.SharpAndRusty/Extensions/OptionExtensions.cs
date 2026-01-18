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
    }
        
}