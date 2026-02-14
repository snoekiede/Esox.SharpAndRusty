using Esox.SharpAndRusty.Types;
using System.Collections.Immutable;

namespace Esox.SharpAndRusty.Extensions;

/// <summary>
/// Provides extension methods for working with <see cref="Validation{T, E}"/> types, including applicative operations.
/// </summary>
public static class ValidationExtensions
{
    /// <summary>
    /// Applies a validation containing a function to a validation containing a value, accumulating errors.
    /// This is the core applicative operation that enables error accumulation.
    /// </summary>
    public static Validation<TResult, E> Apply<T, E, TResult>(
        this Validation<Func<T, TResult>, E> validationFunc,
        Validation<T, E> validation)
    {
        return (validationFunc, validation) switch
        {
            (Validation<Func<T, TResult>, E>.Success successFunc, Validation<T, E>.Success success) =>
                Validation<TResult, E>.Valid(successFunc.Value(success.Value)),

            (Validation<Func<T, TResult>, E>.Failure failureFunc, Validation<T, E>.Failure failure) =>
                Validation<TResult, E>.Invalid(failureFunc.Errors.AddRange(failure.Errors)),

            (Validation<Func<T, TResult>, E>.Failure failureFunc, _) =>
                Validation<TResult, E>.Invalid(failureFunc.Errors),

            (_, Validation<T, E>.Failure failure) =>
                Validation<TResult, E>.Invalid(failure.Errors),

            _ => throw new InvalidOperationException("Validation is in an invalid state.")
        };
    }

    /// <summary>
    /// Combines two validations, accumulating all errors if either fails.
    /// </summary>
    public static Validation<TResult, E> Apply<T1, T2, E, TResult>(
        Validation<T1, E> validation1,
        Validation<T2, E> validation2,
        Func<T1, T2, TResult> combiner)
    {
        if (combiner is null) throw new ArgumentNullException(nameof(combiner));

        return (validation1, validation2) switch
        {
            (Validation<T1, E>.Success s1, Validation<T2, E>.Success s2) =>
                Validation<TResult, E>.Valid(combiner(s1.Value, s2.Value)),

            (Validation<T1, E>.Failure f1, Validation<T2, E>.Failure f2) =>
                Validation<TResult, E>.Invalid(f1.Errors.AddRange(f2.Errors)),

            (Validation<T1, E>.Failure f1, _) =>
                Validation<TResult, E>.Invalid(f1.Errors),

            (_, Validation<T2, E>.Failure f2) =>
                Validation<TResult, E>.Invalid(f2.Errors),

            _ => throw new InvalidOperationException("Validation is in an invalid state.")
        };
    }

    /// <summary>
    /// Combines three validations, accumulating all errors if any fail.
    /// </summary>
    public static Validation<TResult, E> Apply<T1, T2, T3, E, TResult>(
        Validation<T1, E> validation1,
        Validation<T2, E> validation2,
        Validation<T3, E> validation3,
        Func<T1, T2, T3, TResult> combiner)
    {
        if (combiner is null) throw new ArgumentNullException(nameof(combiner));

        var errors = ImmutableList<E>.Empty;
        
        T1? v1 = default;
        var has1 = false;
        if (validation1 is Validation<T1, E>.Success s1)
        {
            v1 = s1.Value;
            has1 = true;
        }
        else if (validation1 is Validation<T1, E>.Failure f1)
        {
            errors = errors.AddRange(f1.Errors);
        }

        T2? v2 = default;
        var has2 = false;
        if (validation2 is Validation<T2, E>.Success s2)
        {
            v2 = s2.Value;
            has2 = true;
        }
        else if (validation2 is Validation<T2, E>.Failure f2)
        {
            errors = errors.AddRange(f2.Errors);
        }

        T3? v3 = default;
        var has3 = false;
        if (validation3 is Validation<T3, E>.Success s3)
        {
            v3 = s3.Value;
            has3 = true;
        }
        else if (validation3 is Validation<T3, E>.Failure f3)
        {
            errors = errors.AddRange(f3.Errors);
        }

        if (has1 && has2 && has3)
        {
            return Validation<TResult, E>.Valid(combiner(v1!, v2!, v3!));
        }

        return Validation<TResult, E>.Invalid(errors);
    }

    /// <summary>
    /// Combines four validations, accumulating all errors if any fail.
    /// </summary>
    public static Validation<TResult, E> Apply<T1, T2, T3, T4, E, TResult>(
        Validation<T1, E> validation1,
        Validation<T2, E> validation2,
        Validation<T3, E> validation3,
        Validation<T4, E> validation4,
        Func<T1, T2, T3, T4, TResult> combiner)
    {
        if (combiner is null) throw new ArgumentNullException(nameof(combiner));

        var errors = ImmutableList<E>.Empty;
        var values = new List<object?>();

        foreach (var validation in new object[] { validation1, validation2, validation3, validation4 })
        {
            switch (validation)
            {
                case Validation<T1, E>.Success s1:
                    values.Add(s1.Value);
                    break;
                case Validation<T1, E>.Failure f1:
                    errors = errors.AddRange(f1.Errors);
                    break;
                case Validation<T2, E>.Success s2:
                    values.Add(s2.Value);
                    break;
                case Validation<T2, E>.Failure f2:
                    errors = errors.AddRange(f2.Errors);
                    break;
                case Validation<T3, E>.Success s3:
                    values.Add(s3.Value);
                    break;
                case Validation<T3, E>.Failure f3:
                    errors = errors.AddRange(f3.Errors);
                    break;
                case Validation<T4, E>.Success s4:
                    values.Add(s4.Value);
                    break;
                case Validation<T4, E>.Failure f4:
                    errors = errors.AddRange(f4.Errors);
                    break;
            }
        }

        if (values.Count == 4)
        {
            return Validation<TResult, E>.Valid(
                combiner((T1)values[0]!, (T2)values[1]!, (T3)values[2]!, (T4)values[3]!));
        }

        return Validation<TResult, E>.Invalid(errors);
    }

    /// <summary>
    /// Chains validations sequentially. Unlike Apply, this stops at the first error (like Result.Bind).
    /// </summary>
    public static Validation<TResult, E> Bind<T, E, TResult>(
        this Validation<T, E> validation,
        Func<T, Validation<TResult, E>> binder)
    {
        if (binder is null) throw new ArgumentNullException(nameof(binder));

        return validation switch
        {
            Validation<T, E>.Success success => binder(success.Value),
            Validation<T, E>.Failure failure => new Validation<TResult, E>.Failure(failure.Errors),
            _ => throw new InvalidOperationException("Validation is in an invalid state.")
        };
    }

    /// <summary>
    /// Converts a Result to a Validation.
    /// </summary>
    public static Validation<T, E> ToValidation<T, E>(this Result<T, E> result)
    {
        return result.TryGetValue(out var value)
            ? Validation<T, E>.Valid(value)
            : result.TryGetError(out var error)
                ? Validation<T, E>.Invalid(error)
                : throw new InvalidOperationException("Result is in an invalid state.");
    }

    /// <summary>
    /// Sequences a collection of validations, accumulating all errors.
    /// </summary>
    public static Validation<IEnumerable<T>, E> Sequence<T, E>(
        this IEnumerable<Validation<T, E>> validations)
    {
        var values = new List<T>();
        var errors = ImmutableList<E>.Empty;

        foreach (var validation in validations)
        {
            switch (validation)
            {
                case Validation<T, E>.Success success:
                    values.Add(success.Value);
                    break;
                case Validation<T, E>.Failure failure:
                    errors = errors.AddRange(failure.Errors);
                    break;
            }
        }

        return errors.IsEmpty
            ? Validation<IEnumerable<T>, E>.Valid(values)
            : Validation<IEnumerable<T>, E>.Invalid(errors);
    }

    /// <summary>
    /// Executes an action if the validation succeeded.
    /// </summary>
    public static Validation<T, E> OnSuccess<T, E>(
        this Validation<T, E> validation,
        Action<T> action)
    {
        if (action is null) throw new ArgumentNullException(nameof(action));

        if (validation is Validation<T, E>.Success success)
        {
            action(success.Value);
        }

        return validation;
    }

    /// <summary>
    /// Executes an action if the validation failed, receiving all errors.
    /// </summary>
    public static Validation<T, E> OnFailure<T, E>(
        this Validation<T, E> validation,
        Action<ImmutableList<E>> action)
    {
        if (action is null) throw new ArgumentNullException(nameof(action));

        if (validation is Validation<T, E>.Failure failure)
        {
            action(failure.Errors);
        }

        return validation;
    }
}
