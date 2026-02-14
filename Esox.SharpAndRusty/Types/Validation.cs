using System.Collections.Immutable;

namespace Esox.SharpAndRusty.Types;

/// <summary>
/// Represents a validation result that can either succeed with a value or fail with a collection of errors.
/// Unlike <see cref="Result{T, E}"/>, Validation accumulates ALL errors instead of stopping at the first one.
/// </summary>
/// <typeparam name="T">The type of the success value.</typeparam>
/// <typeparam name="E">The type of the error values.</typeparam>
/// <remarks>
/// Validation is perfect for scenarios where you want to collect all validation errors at once:
/// - Form validation (show all field errors)
/// - Configuration validation (report all missing/invalid settings)
/// - Multi-field business rule validation
/// </remarks>
public abstract record Validation<T, E>
{
    /// <summary>
    /// Represents a successful validation with a value.
    /// </summary>
    public sealed record Success(T Value) : Validation<T, E>;

    /// <summary>
    /// Represents a failed validation with one or more errors.
    /// </summary>
    public sealed record Failure(ImmutableList<E> Errors) : Validation<T, E>
    {
        /// <summary>
        /// Creates a Failure with a single error.
        /// </summary>
        public Failure(E error) : this(ImmutableList.Create(error)) { }

        /// <summary>
        /// Creates a Failure with multiple errors.
        /// </summary>
        public Failure(IEnumerable<E> errors) : this(errors.ToImmutableList()) { }
    }

    /// <summary>
    /// Gets a value indicating whether the validation succeeded.
    /// </summary>
    public bool IsSuccess => this is Success;

    /// <summary>
    /// Gets a value indicating whether the validation failed.
    /// </summary>
    public bool IsFailure => this is Failure;

    /// <summary>
    /// Attempts to get the success value.
    /// </summary>
    /// <param name="value">When this method returns, contains the success value if present; otherwise, the default value.</param>
    /// <returns><c>true</c> if the validation succeeded; otherwise, <c>false</c>.</returns>
    public bool TryGetValue(out T? value)
    {
        if (this is Success success)
        {
            value = success.Value;
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Attempts to get the error collection.
    /// </summary>
    /// <param name="errors">When this method returns, contains the errors if present; otherwise, an empty list.</param>
    /// <returns><c>true</c> if the validation failed; otherwise, <c>false</c>.</returns>
    public bool TryGetErrors(out ImmutableList<E> errors)
    {
        if (this is Failure failure)
        {
            errors = failure.Errors;
            return true;
        }

        errors = ImmutableList<E>.Empty;
        return false;
    }

    /// <summary>
    /// Executes one of two functions depending on whether the validation succeeded or failed.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="onSuccess">The function to execute if the validation succeeded.</param>
    /// <param name="onFailure">The function to execute if the validation failed, receiving all errors.</param>
    /// <returns>The result of the executed function.</returns>
    public TResult Match<TResult>(
        Func<T, TResult> onSuccess,
        Func<ImmutableList<E>, TResult> onFailure)
    {
        if (onSuccess is null) throw new ArgumentNullException(nameof(onSuccess));
        if (onFailure is null) throw new ArgumentNullException(nameof(onFailure));

        return this switch
        {
            Success success => onSuccess(success.Value),
            Failure failure => onFailure(failure.Errors),
            _ => throw new InvalidOperationException("Validation is in an invalid state.")
        };
    }

    /// <summary>
    /// Transforms the success value if present, otherwise returns the errors unchanged.
    /// </summary>
    /// <typeparam name="TResult">The type of the transformed value.</typeparam>
    /// <param name="mapper">A function that transforms the success value.</param>
    /// <returns>A new validation with the transformed value or the original errors.</returns>
    public Validation<TResult, E> Map<TResult>(Func<T, TResult> mapper)
    {
        if (mapper is null) throw new ArgumentNullException(nameof(mapper));

        return this switch
        {
            Success success => new Validation<TResult, E>.Success(mapper(success.Value)),
            Failure failure => new Validation<TResult, E>.Failure(failure.Errors),
            _ => throw new InvalidOperationException("Validation is in an invalid state.")
        };
    }

    /// <summary>
    /// Transforms the error collection if present, otherwise returns the success value unchanged.
    /// </summary>
    /// <typeparam name="E2">The type of the transformed errors.</typeparam>
    /// <param name="errorMapper">A function that transforms each error.</param>
    /// <returns>A new validation with the transformed errors or the original success value.</returns>
    public Validation<T, E2> MapErrors<E2>(Func<E, E2> errorMapper)
    {
        if (errorMapper is null) throw new ArgumentNullException(nameof(errorMapper));

        return this switch
        {
            Success success => new Validation<T, E2>.Success(success.Value),
            Failure failure => new Validation<T, E2>.Failure(
                failure.Errors.Select(errorMapper).ToImmutableList()),
            _ => throw new InvalidOperationException("Validation is in an invalid state.")
        };
    }

    /// <summary>
    /// Converts the validation to a Result, combining all errors if present.
    /// </summary>
    /// <param name="errorCombiner">A function that combines multiple errors into a single error.</param>
    /// <returns>A Result containing the value or the combined error.</returns>
    public Result<T, E> ToResult(Func<ImmutableList<E>, E> errorCombiner)
    {
        if (errorCombiner is null) throw new ArgumentNullException(nameof(errorCombiner));

        return this switch
        {
            Success success => Result<T, E>.Ok(success.Value),
            Failure failure => Result<T, E>.Err(errorCombiner(failure.Errors)),
            _ => throw new InvalidOperationException("Validation is in an invalid state.")
        };
    }

    /// <summary>
    /// Converts the validation to a Result using the first error.
    /// </summary>
    /// <returns>A Result containing the value or the first error.</returns>
    public Result<T, E> ToResultFirstError()
    {
        return this switch
        {
            Success success => Result<T, E>.Ok(success.Value),
            Failure failure => Result<T, E>.Err(failure.Errors.First()),
            _ => throw new InvalidOperationException("Validation is in an invalid state.")
        };
    }

    /// <summary>
    /// Creates a successful validation.
    /// </summary>
    public static Validation<T, E> Valid(T value) => new Success(value);

    /// <summary>
    /// Creates a failed validation with a single error.
    /// </summary>
    public static Validation<T, E> Invalid(E error) => new Failure(error);

    /// <summary>
    /// Creates a failed validation with multiple errors.
    /// </summary>
    public static Validation<T, E> Invalid(IEnumerable<E> errors) => new Failure(errors);

    public override string ToString()
    {
        return this switch
        {
            Success success => $"Valid({success.Value})",
            Failure failure => $"Invalid([{string.Join(", ", failure.Errors)}])",
            _ => "Validation(invalid)"
        };
    }
}
