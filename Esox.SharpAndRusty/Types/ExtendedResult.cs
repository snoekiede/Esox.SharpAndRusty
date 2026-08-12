// ReSharper disable HeapView.ObjectAllocation.Evident
namespace Esox.SharpAndRusty.Types;

public abstract record ExtendedResult<T, TE>
{
    /// <summary>
    ///     Gets a value indicating whether this result represents a successful operation.
    /// </summary>
    public bool IsSuccess => this is Success;

    /// <summary>
    ///     Gets a value indicating whether this result represents a failed operation.
    /// </summary>
    public bool IsFailure => this is Failure;

    /// <summary>
    ///     Determines whether the specified result is equal to the current result.
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

    public static ExtendedResult<T, TE> Ok(T value) => new Success(value);
    public static ExtendedResult<T, TE> Err(TE error) => new Failure(error);

    /// <summary>
    ///     Attempts to get the success value from the result.
    /// </summary>
    /// <param name="value">
    ///     When this method returns, contains the success value if the result is successful; otherwise, the
    ///     default value.
    /// </param>
    /// <returns>true if the result is successful; otherwise, false.</returns>
    public bool TryGetValue(out T value)
    {
        switch (this)
        {
            case Success success:
                value = success.Value;
                return true;
            case Failure:
                break;
        }

        value = default!;
        return false;
    }

    /// <summary>
    ///     Matches the result and executes the appropriate function based on whether it's a success or failure.
    ///     If a null delegate is supplied for the matching branch, returns <c>default</c> for
    ///     <typeparamref name="TR"/> rather than throwing.
    /// </summary>
    /// <typeparam name="TR">The return type of the match operation.</typeparam>
    /// <param name="success">Function to execute if the result is successful.</param>
    /// <param name="failure">Function to execute if the result is a failure.</param>
    /// <returns>The result of executing either the success or failure function, or <c>default</c> if the relevant delegate is null.</returns>
    public TR Match<TR>(Func<T, TR> success, Func<TE, TR> failure)
    {
        return this switch
        {
            Success s => success is not null ? success(s.Value) : default!,
            Failure f => failure is not null ? failure(f.Error) : default!,
            _ => default!
        };
    }

    /// <summary>
    ///     Attempts to get the error value from the result.
    /// </summary>
    /// <param name="error">
    ///     When this method returns, contains the error value if the result is a failure; otherwise, the
    ///     default value.
    /// </param>
    /// <returns>true if the result is a failure; otherwise, false.</returns>
    public bool TryGetError(out TE error)
    {
        switch (this)
        {
            case Success:
                break;
            case Failure failure:
                error = failure.Error;
                return true;
        }

        error = default!;
        return false;
    }

    /// <summary>
    ///     Returns the success value if the result is successful; otherwise, returns the specified default value.
    /// </summary>
    /// <param name="defaultValue">The default value to return if the result is a failure.</param>
    /// <returns>The success value or the default value.</returns>
    public T UnwrapOr(T defaultValue)
    {
        return this switch
        {
            Success success => success.Value,
            _ => defaultValue
        };
    }

    /// <summary>
    ///     Returns the success value if the result is successful; otherwise, computes and returns a default value.
    ///     If <paramref name="defaultFactory"/> is null, returns <c>default</c> for <typeparamref name="T"/>
    ///     rather than throwing.
    /// </summary>
    /// <param name="defaultFactory">A function that produces a default value.</param>
    /// <returns>The success value or the computed default value.</returns>
    public T UnwrapOrElse(Func<TE, T> defaultFactory)
    {
        return this switch
        {
            Success success => success.Value,
            Failure failure => defaultFactory is not null ? defaultFactory(failure.Error) : default!,
            _ => default!
        };
    }

    /// <summary>
    ///     Returns this result if it is successful; otherwise, returns the result produced by the alternative function.
    /// </summary>
    /// <param name="alternative">A function that produces an alternative result based on the error.</param>
    /// <returns>This result if successful; otherwise, the alternative result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when alternative is null.</exception>
    public ExtendedResult<T, TE> OrElse(Func<TE, ExtendedResult<T, TE>> alternative)
    {
        if (alternative is null)
        {
            return ExtendedResult<T, TE>.Err(default!);
        }
        
        return this switch
        {
            Success => this,
            Failure f => alternative(f.Error),
            _ => ExtendedResult<T, TE>.Err(default!)
        };
    }

    /// <summary>
    ///     Executes the specified action with the success value if the result is successful.
    ///     Useful for side effects without transforming the result.
    ///     If <paramref name="action"/> is null the call is a no-op and this result is returned unchanged.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <returns>This result unchanged.</returns>
    public ExtendedResult<T, TE> Inspect(Action<T> action)
    {
        if (action is not null && this is Success success) action(success.Value);
        return this;
    }

    /// <summary>
    ///     Executes the specified action with the error value if the result is a failure.
    ///     Useful for side effects without transforming the result.
    ///     If <paramref name="action"/> is null the call is a no-op and this result is returned unchanged.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <returns>This result unchanged.</returns>
    public ExtendedResult<T, TE> InspectErr(Action<TE> action)
    {
        if (action is not null && this is Failure failure) action(failure.Error);
        return this;
    }

    /// <summary>
    ///     Returns the hash code for this result.
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
    ///     Returns a string representation of this result.
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
    ///     Executes an asynchronous operation and wraps the result in a Result type.
    ///     If the operation throws an exception, it is caught and converted to an error using the error handler.
    ///     A null <paramref name="operation"/> returns <c>Err(default)</c> immediately.
    ///     A null <paramref name="errorHandler"/> causes exceptions to be converted to <c>Err(default)</c>.
    /// </summary>
    /// <param name="operation">The asynchronous operation to execute.</param>
    /// <param name="errorHandler">A function that converts an exception to an error value.</param>
    /// <returns>A task representing the asynchronous operation, containing the result.</returns>
    public static async Task<ExtendedResult<T, TE>> TryAsync(Func<Task<T>> operation, Func<Exception, TE> errorHandler)
    {
        if (operation is null) return Err(default!);

        try
        {
            T value = await operation().ConfigureAwait(false);
            return Ok(value);
        }
        catch (Exception ex)
        {
            return Err(errorHandler is not null ? errorHandler(ex) : default!);
        }
    }

    /// <summary>
    ///     Executes a synchronous operation and wraps the result in a Result type.
    ///     If the operation throws an exception, it is caught and converted to an error using the error handler.
    ///     A null <paramref name="operation"/> returns <c>Err(default)</c> immediately.
    ///     A null <paramref name="errorHandler"/> causes exceptions to be converted to <c>Err(default)</c>.
    /// </summary>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="errorHandler">A function that converts an exception to an error value.</param>
    /// <returns>The result of the operation.</returns>
    public static ExtendedResult<T, TE> Try(Func<T> operation, Func<Exception, TE> errorHandler)
    {
        if (operation is null) return Err(default!);

        try
        {
            T value = operation();
            return Ok(value);
        }
        catch (Exception ex)
        {
            return Err(errorHandler is not null ? errorHandler(ex) : default!);
        }
    }

    /// <summary>
    ///     Implicitly converts a value of type <typeparamref name="T" /> to a successful result.
    /// </summary>
    /// <param name="value">The success value.</param>
    /// <returns>An <see cref="ExtendedResult{T,TE}" /> representing success.</returns>
    public static implicit operator ExtendedResult<T, TE>(T value) => Ok(value);

    /// <summary>
    ///     Implicitly converts a value of type <typeparamref name="TE" /> to a failed result.
    /// </summary>
    /// <param name="error">The error value.</param>
    /// <returns>An <see cref="ExtendedResult{T,TE}" /> representing failure.</returns>
    public static implicit operator ExtendedResult<T, TE>(TE error) => Err(error);

    public sealed record Success(T Value) : ExtendedResult<T, TE>;

    public sealed record Failure(TE Error) : ExtendedResult<T, TE>;
}