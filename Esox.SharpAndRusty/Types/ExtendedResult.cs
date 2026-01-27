namespace Esox.SharpAndRusty.Types;

public abstract record ExtendedResult<T,E>
{
    public sealed record Success(T Value) : ExtendedResult<T,E>;
    public sealed record Failure(E Error) : ExtendedResult<T,E>;
    
    public static ExtendedResult<T,E> Ok(T value) => new Success(value);
    public static ExtendedResult<T,E> Err(E error) => new Failure(error);
    
    /// <summary>
    /// Attempts to get the success value from the result.
    /// </summary>
    /// <param name="value">When this method returns, contains the success value if the result is successful; otherwise, the default value.</param>
    /// <returns>true if the result is successful; otherwise, false.</returns>
    public bool TryGetValue(out T value)
    {
        switch (this)
        {
            case Success success:
                value = success.Value;
                return true;
            case Failure:
                value = default!;
                return false;
            default:
                value = default!;
                return false;
        }
    }
    
    /// <summary>
    /// Attempts to get the error value from the result.
    /// </summary>
    /// <param name="error">When this method returns, contains the error value if the result is a failure; otherwise, the default value.</param>
    /// <returns>true if the result is a failure; otherwise, false.</returns>
    public bool TryGetError(out E error)
    {
        switch (this)
        {
            case Success:
                error = default!;
                return false;
            case Failure failure:
                error = failure.Error;
                return true;
            default:
                error = default!;
                return false;
        }
    }
    
    /// <summary>
    /// Returns the success value if the result is successful; otherwise, returns the specified default value.
    /// </summary>
    /// <param name="defaultValue">The default value to return if the result is a failure.</param>
    /// <returns>The success value or the default value.</returns>
    public T UnwrapOr(T defaultValue)
    {
        return this switch 
        {
            Success success => success.Value,
            Failure => defaultValue,
            _ => defaultValue
        };
    }
    
    /// <summary>
    /// Returns the success value if the result is successful; otherwise, computes and returns a default value.
    /// </summary>
    /// <param name="defaultFactory">A function that produces a default value.</param>
    /// <returns>The success value or the computed default value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when defaultFactory is null.</exception>
    public T UnwrapOrElse(Func<E, T> defaultFactory)
    {
        return this switch 
        {
            Success success => success.Value,
            Failure failure => defaultFactory(failure.Error),
            _ => default!
        };
    }
    
    /// <summary>
    /// Executes the specified action with the success value if the result is successful.
    /// Useful for side effects without transforming the result.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <returns>This result unchanged.</returns>
    /// <exception cref="ArgumentNullException">Thrown when action is null.</exception>
    public ExtendedResult<T, E> Inspect(Action<T> action)
    {
        if (action is null) throw new ArgumentNullException(nameof(action));
        if (this is Success success)
        {
            action(success.Value);
        }
        return this;
    }
    
    /// <summary>
    /// Executes the specified action with the error value if the result is a failure.
    /// Useful for side effects without transforming the result.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <returns>This result unchanged.</returns>
    /// <exception cref="ArgumentNullException">Thrown when action is null.</exception>
    public ExtendedResult<T, E> InspectErr(Action<E> action)
    {
        if (action is null) throw new ArgumentNullException(nameof(action));
        if (this is Failure failure)
        {
            action(failure.Error);
        }
        return this;
    }
    
    /// <summary>
    /// Executes an asynchronous operation and wraps the result in a Result type.
    /// If the operation throws an exception, it is caught and converted to an error using the error handler.
    /// </summary>
    /// <param name="operation">The asynchronous operation to execute.</param>
    /// <param name="errorHandler">A function that converts an exception to an error value.</param>
    /// <returns>A task representing the asynchronous operation, containing the result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when operation or errorHandler is null.</exception>
    public static async Task<ExtendedResult<T, E>> TryAsync(Func<Task<T>> operation, Func<Exception, E> errorHandler)
    {
        if (operation is null) throw new ArgumentNullException(nameof(operation));
        if (errorHandler is null) throw new ArgumentNullException(nameof(errorHandler));

        try
        {
            var value = await operation().ConfigureAwait(false);
            return Ok(value);
        }
        catch (Exception ex)
        {
            return Err(errorHandler(ex));
        }
    }
    
    /// <summary>
    /// Executes a synchronous operation and wraps the result in a Result type.
    /// If the operation throws an exception, it is caught and converted to an error using the error handler.
    /// </summary>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="errorHandler">A function that converts an exception to an error value.</param>
    /// <returns>The result of the operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when operation or errorHandler is null.</exception>
    public static ExtendedResult<T, E> Try(Func<T> operation, Func<Exception, E> errorHandler)
    {
        if (operation is null) throw new ArgumentNullException(nameof(operation));
        if (errorHandler is null) throw new ArgumentNullException(nameof(errorHandler));

        try
        {
            var value = operation();
            return Ok(value);
        }
        catch (Exception ex)
        {
            return Err(errorHandler(ex));
        }
    }
    
}