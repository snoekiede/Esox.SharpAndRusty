using Esox.SharpAndRusty.Types;

namespace Esox.SharpAndRusty.Extensions;

/// <summary>
/// Provides convenient pattern matching helper methods for <see cref="Option{T}"/> and <see cref="Result{T, E}"/> types.
/// </summary>
public static class PatternMatchingHelpers
{
    #region Option Helpers

    /// <summary>
    /// Executes an action if the option contains a value.
    /// </summary>
    /// <typeparam name="T">The type of the value in the option.</typeparam>
    /// <param name="option">The option to inspect.</param>
    /// <param name="action">The action to execute if the option is <c>Some</c>.</param>
    /// <returns>The original option for method chaining.</returns>
    public static Option<T> IfSome<T>(this Option<T> option, Action<T> action)
    {
        if (action is null) throw new ArgumentNullException(nameof(action));
        
        if (option is Option<T>.Some some)
        {
            action(some.Value);
        }
        
        return option;
    }

    /// <summary>
    /// Executes an action if the option is empty.
    /// </summary>
    /// <typeparam name="T">The type of the value in the option.</typeparam>
    /// <param name="option">The option to inspect.</param>
    /// <param name="action">The action to execute if the option is <c>None</c>.</param>
    /// <returns>The original option for method chaining.</returns>
    public static Option<T> IfNone<T>(this Option<T> option, Action action)
    {
        if (action is null) throw new ArgumentNullException(nameof(action));
        
        if (option.IsNone())
        {
            action();
        }
        
        return option;
    }

    /// <summary>
    /// Gets the value from the option or returns a default value.
    /// </summary>
    /// <typeparam name="T">The type of the value in the option.</typeparam>
    /// <param name="option">The option to extract the value from.</param>
    /// <param name="defaultValue">The value to return if the option is <c>None</c>.</param>
    /// <returns>The contained value if <c>Some</c>, otherwise <paramref name="defaultValue"/>.</returns>
    public static T GetOrDefault<T>(this Option<T> option, T defaultValue)
    {
        return option is Option<T>.Some some ? some.Value : defaultValue;
    }

    /// <summary>
    /// Gets the value from the option or computes a default value.
    /// </summary>
    /// <typeparam name="T">The type of the value in the option.</typeparam>
    /// <param name="option">The option to extract the value from.</param>
    /// <param name="defaultFactory">A function that produces a default value.</param>
    /// <returns>The contained value if <c>Some</c>, otherwise the value produced by <paramref name="defaultFactory"/>.</returns>
    public static T GetOrElse<T>(this Option<T> option, Func<T> defaultFactory)
    {
        if (defaultFactory is null) throw new ArgumentNullException(nameof(defaultFactory));
        
        return option is Option<T>.Some some ? some.Value : defaultFactory();
    }

    /// <summary>
    /// Throws an exception if the option is <c>None</c>, otherwise returns the contained value.
    /// </summary>
    /// <typeparam name="T">The type of the value in the option.</typeparam>
    /// <param name="option">The option to extract the value from.</param>
    /// <param name="exceptionFactory">A function that produces an exception to throw if the option is <c>None</c>.</param>
    /// <returns>The contained value if <c>Some</c>.</returns>
    /// <exception cref="Exception">Thrown when the option is <c>None</c>.</exception>
    public static T GetOrThrow<T>(this Option<T> option, Func<Exception> exceptionFactory)
    {
        if (exceptionFactory is null) throw new ArgumentNullException(nameof(exceptionFactory));
        
        return option is Option<T>.Some some ? some.Value : throw exceptionFactory();
    }

    #endregion

    #region Result Helpers

    /// <summary>
    /// Executes an action if the result is successful.
    /// </summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <typeparam name="E">The type of the error value.</typeparam>
    /// <param name="result">The result to inspect.</param>
    /// <param name="action">The action to execute on the success value.</param>
    /// <returns>The original result for method chaining.</returns>
    public static Result<T, E> OnSuccess<T, E>(this Result<T, E> result, Action<T> action)
    {
        if (action is null) throw new ArgumentNullException(nameof(action));
        
        if (result.TryGetValue(out var value))
        {
            action(value);
        }
        
        return result;
    }

    /// <summary>
    /// Executes an action if the result is an error.
    /// </summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <typeparam name="E">The type of the error value.</typeparam>
    /// <param name="result">The result to inspect.</param>
    /// <param name="action">The action to execute on the error value.</param>
    /// <returns>The original result for method chaining.</returns>
    public static Result<T, E> OnFailure<T, E>(this Result<T, E> result, Action<E> action)
    {
        if (action is null) throw new ArgumentNullException(nameof(action));
        
        if (result.TryGetError(out var error))
        {
            action(error);
        }
        
        return result;
    }

    /// <summary>
    /// Executes actions for both success and failure cases.
    /// </summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <typeparam name="E">The type of the error value.</typeparam>
    /// <param name="result">The result to inspect.</param>
    /// <param name="onSuccess">The action to execute on success.</param>
    /// <param name="onFailure">The action to execute on failure.</param>
    /// <returns>The original result for method chaining.</returns>
    public static Result<T, E> Do<T, E>(
        this Result<T, E> result,
        Action<T> onSuccess,
        Action<E> onFailure)
    {
        if (onSuccess is null) throw new ArgumentNullException(nameof(onSuccess));
        if (onFailure is null) throw new ArgumentNullException(nameof(onFailure));
        
        if (result.TryGetValue(out var value))
        {
            onSuccess(value);
        }
        else if (result.TryGetError(out var error))
        {
            onFailure(error);
        }
        
        return result;
    }

    /// <summary>
    /// Gets the success value from the result or returns a default value.
    /// </summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <typeparam name="E">The type of the error value.</typeparam>
    /// <param name="result">The result to extract the value from.</param>
    /// <param name="defaultValue">The value to return if the result is an error.</param>
    /// <returns>The success value if successful, otherwise <paramref name="defaultValue"/>.</returns>
    public static T GetValueOrDefault<T, E>(this Result<T, E> result, T defaultValue)
    {
        return result.TryGetValue(out var value) ? value : defaultValue;
    }

    /// <summary>
    /// Gets the success value from the result or computes a default value from the error.
    /// </summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <typeparam name="E">The type of the error value.</typeparam>
    /// <param name="result">The result to extract the value from.</param>
    /// <param name="defaultFactory">A function that produces a default value from the error.</param>
    /// <returns>The success value if successful, otherwise the value produced by <paramref name="defaultFactory"/>.</returns>
    public static T GetValueOrElse<T, E>(this Result<T, E> result, Func<E, T> defaultFactory)
    {
        if (defaultFactory is null) throw new ArgumentNullException(nameof(defaultFactory));
        
        return result.TryGetValue(out var value)
            ? value
            : result.TryGetError(out var error)
                ? defaultFactory(error)
                : throw new InvalidOperationException("Result is in an invalid state.");
    }

    /// <summary>
    /// Throws an exception if the result is an error, otherwise returns the success value.
    /// </summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <typeparam name="E">The type of the error value.</typeparam>
    /// <param name="result">The result to extract the value from.</param>
    /// <param name="exceptionFactory">A function that produces an exception from the error.</param>
    /// <returns>The success value if the result is successful.</returns>
    /// <exception cref="Exception">Thrown when the result is an error.</exception>
    public static T GetValueOrThrow<T, E>(this Result<T, E> result, Func<E, Exception> exceptionFactory)
    {
        if (exceptionFactory is null) throw new ArgumentNullException(nameof(exceptionFactory));
        
        if (result.TryGetValue(out var value))
        {
            return value;
        }
        
        if (result.TryGetError(out var error))
        {
            throw exceptionFactory(error);
        }
        
        throw new InvalidOperationException("Result is in an invalid state.");
    }

    /// <summary>
    /// Converts a result to an option, discarding any error information.
    /// </summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <typeparam name="E">The type of the error value.</typeparam>
    /// <param name="result">The result to convert.</param>
    /// <returns>
    /// <c>Some</c> containing the success value if the result is successful; otherwise, <c>None</c>.
    /// </returns>
    public static Option<T> ToOption<T, E>(this Result<T, E> result)
    {
        return result.TryGetValue(out var value)
            ? new Option<T>.Some(value)
            : new Option<T>.None();
    }

    #endregion
}
