namespace Esox.SharpAndRusty.Types;

public abstract record ExtendedResult<T, TE>
{
    public sealed record Success(T Value) : ExtendedResult<T, TE>;
    public sealed record Failure(TE Error) : ExtendedResult<T, TE>;
    
    public static ExtendedResult<T, TE> Ok(T value) => new Success(value);
    public static ExtendedResult<T, TE> Err(TE error) => new Failure(error);
    
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
        }
        value = default!;
        return false;
    }
    
    /// <summary>
    /// Matches the result and executes the appropriate function based on whether it's a success or failure.
    /// </summary>
    /// <typeparam name="TR">The return type of the match operation.</typeparam>
    /// <param name="success">Function to execute if the result is successful.</param>
    /// <param name="failure">Function to execute if the result is a failure.</param>
    /// <returns>The result of executing either the success or failure function.</returns>
    /// <exception cref="ArgumentNullException">Thrown when success or failure function is null.</exception>
    public TR Match<TR>(Func<T, TR> success, Func<TE, TR> failure)
    {
        ArgumentNullException.ThrowIfNull(success);
        ArgumentNullException.ThrowIfNull(failure);

        return this switch
        {
            Success s => success(s.Value),
            Failure f => failure(f.Error),
            _ => throw new InvalidOperationException("Unrecognized ExtendedResult type.")
        };
    }
    /// <summary>
    /// Attempts to get the error value from the result.
    /// </summary>
    /// <param name="error">When this method returns, contains the error value if the result is a failure; otherwise, the default value.</param>
    /// <returns>true if the result is a failure; otherwise, false.</returns>
    public bool TryGetError(out TE error)
    {
        switch (this)
        {
            case Success:
                error = default!;
                return false;
            case Failure failure:
                error = failure.Error;
                return true;
        }
        error = default!;
        return false;
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
    public T UnwrapOrElse(Func<TE, T> defaultFactory)
    {
        return this switch 
        {
            Success success => success.Value,
            Failure failure => defaultFactory(failure.Error),
            _ => default!
        };
    }
    
    /// <summary>
    /// Returns this result if it is successful; otherwise, returns the result produced by the alternative function.
    /// </summary>
    /// <param name="alternative">A function that produces an alternative result based on the error.</param>
    /// <returns>This result if successful; otherwise, the alternative result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when alternative is null.</exception>
    public ExtendedResult<T, TE> OrElse(Func<TE, ExtendedResult<T, TE>> alternative)
    {
        if (alternative is null) throw new ArgumentNullException(nameof(alternative));
        return this switch
        {
            Success => this,
            Failure f => alternative(f.Error),
            _ => throw new InvalidOperationException("Unrecognized ExtendedResult type.")
        };
    }
    
    /// <summary>
    /// Executes the specified action with the success value if the result is successful.
    /// Useful for side effects without transforming the result.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <returns>This result unchanged.</returns>
    /// <exception cref="ArgumentNullException">Thrown when action is null.</exception>
    public ExtendedResult<T, TE> Inspect(Action<T> action)
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
    public ExtendedResult<T, TE> InspectErr(Action<TE> action)
    {
        if (action is null) throw new ArgumentNullException(nameof(action));
        if (this is Failure failure)
        {
            action(failure.Error);
        }
        return this;
    }
    
    /// <summary>
    /// Determines whether the specified result is equal to the current result.
    /// </summary>
    public virtual bool Equals(ExtendedResult<T, TE>? other)
    {
        if (ReferenceEquals(this, other)) return true;
        if (other is null) return false;

        return (this, other) switch
        {
            (Success s1, Success s2) => EqualityComparer<T>.Default.Equals(s1.Value, s2.Value),
            (Failure f1, Failure f2) => EqualityComparer<TE>.Default.Equals(f1.Error, f2.Error),
            _ => false
        };
    }

    /// <summary>
    /// Returns the hash code for this result.
    /// </summary>
    public override int GetHashCode()
    {
        return this switch
        {
            Success s => HashCode.Combine(1, s.Value is null ? 0 : EqualityComparer<T>.Default.GetHashCode(s.Value)),
            Failure f => HashCode.Combine(2, f.Error is null ? 0 : EqualityComparer<TE>.Default.GetHashCode(f.Error)),
            _ => 0
        };
    }
    
    /// <summary>
    /// Returns a string representation of this result.
    /// </summary>
    public override string ToString()
    {
        return this switch
        {
            Success s => $"Ok({s.Value})",
            Failure f => $"Err({f.Error})",
            _ => "ExtendedResult(Unknown)"
        };
    }
    
    
    /// <summary>
    /// Executes an asynchronous operation and wraps the result in a Result type.
    /// If the operation throws an exception, it is caught and converted to an error using the error handler.
    /// </summary>
    /// <param name="operation">The asynchronous operation to execute.</param>
    /// <param name="errorHandler">A function that converts an exception to an error value.</param>
    /// <returns>A task representing the asynchronous operation, containing the result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when operation or errorHandler is null.</exception>
    public static async Task<ExtendedResult<T, TE>> TryAsync(Func<Task<T>> operation, Func<Exception, TE> errorHandler)
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
    public static ExtendedResult<T, TE> Try(Func<T> operation, Func<Exception, TE> errorHandler)
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
