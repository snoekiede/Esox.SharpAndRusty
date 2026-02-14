namespace Esox.SharpAndRusty.Types;

/// <summary>
/// Represents a value that can be one of two possible types (Left or Right).
/// Unlike <see cref="Result{T, E}"/>, Either doesn't imply success or failure - both sides are equally valid.
/// </summary>
/// <typeparam name="L">The type of the Left value.</typeparam>
/// <typeparam name="R">The type of the Right value.</typeparam>
/// <remarks>
/// Either is useful when you have two equally valid alternatives that aren't success/failure:
/// - Configuration from file OR environment variables
/// - Cached data OR fresh data from database  
/// - Local file OR remote URL
/// </remarks>
public abstract record Either<L, R>
{
    /// <summary>
    /// Represents the Left case of the Either.
    /// </summary>
    public sealed record Left(L Value) : Either<L, R>
    {
        /// <summary>
        /// Returns a string representation of the Left value.
        /// </summary>
        public override string ToString() => $"Left({Value})";
    }

    /// <summary>
    /// Represents the Right case of the Either.
    /// </summary>
    public sealed record Right(R Value) : Either<L, R>
    {
        /// <summary>
        /// Returns a string representation of the Right value.
        /// </summary>
        public override string ToString() => $"Right({Value})";
    }

    /// <summary>
    /// Gets a value indicating whether this Either contains a Left value.
    /// </summary>
    public bool IsLeft => this is Left;

    /// <summary>
    /// Gets a value indicating whether this Either contains a Right value.
    /// </summary>
    public bool IsRight => this is Right;

    /// <summary>
    /// Attempts to get the Left value.
    /// </summary>
    /// <param name="value">When this method returns, contains the Left value if present; otherwise, the default value.</param>
    /// <returns><c>true</c> if the Either contains a Left value; otherwise, <c>false</c>.</returns>
    public bool TryGetLeft(out L? value)
    {
        if (this is Left left)
        {
            value = left.Value;
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Attempts to get the Right value.
    /// </summary>
    /// <param name="value">When this method returns, contains the Right value if present; otherwise, the default value.</param>
    /// <returns><c>true</c> if the Either contains a Right value; otherwise, <c>false</c>.</returns>
    public bool TryGetRight(out R? value)
    {
        if (this is Right right)
        {
            value = right.Value;
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Executes one of two functions depending on whether the Either is Left or Right.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="onLeft">The function to execute if the Either is Left.</param>
    /// <param name="onRight">The function to execute if the Either is Right.</param>
    /// <returns>The result of the executed function.</returns>
    public TResult Match<TResult>(Func<L, TResult> onLeft, Func<R, TResult> onRight)
    {
        if (onLeft is null) throw new ArgumentNullException(nameof(onLeft));
        if (onRight is null) throw new ArgumentNullException(nameof(onRight));

        return this switch
        {
            Left left => onLeft(left.Value),
            Right right => onRight(right.Value),
            _ => throw new InvalidOperationException("Either is in an invalid state.")
        };
    }

    /// <summary>
    /// Executes one of two actions depending on whether the Either is Left or Right.
    /// </summary>
    /// <param name="onLeft">The action to execute if the Either is Left.</param>
    /// <param name="onRight">The action to execute if the Either is Right.</param>
    public void Match(Action<L> onLeft, Action<R> onRight)
    {
        if (onLeft is null) throw new ArgumentNullException(nameof(onLeft));
        if (onRight is null) throw new ArgumentNullException(nameof(onRight));

        switch (this)
        {
            case Left left:
                onLeft(left.Value);
                break;
            case Right right:
                onRight(right.Value);
                break;
            default:
                throw new InvalidOperationException("Either is in an invalid state.");
        }
    }

    /// <summary>
    /// Transforms the Left value if present, otherwise returns the Right value unchanged.
    /// </summary>
    /// <typeparam name="L2">The type of the transformed Left value.</typeparam>
    /// <param name="mapper">A function that transforms the Left value.</param>
    /// <returns>A new Either with the transformed Left value, or the original Right value.</returns>
    public Either<L2, R> MapLeft<L2>(Func<L, L2> mapper)
    {
        if (mapper is null) throw new ArgumentNullException(nameof(mapper));

        return this switch
        {
            Left left => new Either<L2, R>.Left(mapper(left.Value)),
            Right right => new Either<L2, R>.Right(right.Value),
            _ => throw new InvalidOperationException("Either is in an invalid state.")
        };
    }

    /// <summary>
    /// Transforms the Right value if present, otherwise returns the Left value unchanged.
    /// </summary>
    /// <typeparam name="R2">The type of the transformed Right value.</typeparam>
    /// <param name="mapper">A function that transforms the Right value.</param>
    /// <returns>A new Either with the transformed Right value, or the original Left value.</returns>
    public Either<L, R2> MapRight<R2>(Func<R, R2> mapper)
    {
        if (mapper is null) throw new ArgumentNullException(nameof(mapper));

        return this switch
        {
            Left left => new Either<L, R2>.Left(left.Value),
            Right right => new Either<L, R2>.Right(mapper(right.Value)),
            _ => throw new InvalidOperationException("Either is in an invalid state.")
        };
    }

    /// <summary>
    /// Transforms both the Left and Right values.
    /// </summary>
    /// <typeparam name="L2">The type of the transformed Left value.</typeparam>
    /// <typeparam name="R2">The type of the transformed Right value.</typeparam>
    /// <param name="leftMapper">A function that transforms the Left value.</param>
    /// <param name="rightMapper">A function that transforms the Right value.</param>
    /// <returns>A new Either with both values potentially transformed.</returns>
    public Either<L2, R2> Map<L2, R2>(Func<L, L2> leftMapper, Func<R, R2> rightMapper)
    {
        if (leftMapper is null) throw new ArgumentNullException(nameof(leftMapper));
        if (rightMapper is null) throw new ArgumentNullException(nameof(rightMapper));

        return this switch
        {
            Left left => new Either<L2, R2>.Left(leftMapper(left.Value)),
            Right right => new Either<L2, R2>.Right(rightMapper(right.Value)),
            _ => throw new InvalidOperationException("Either is in an invalid state.")
        };
    }

    /// <summary>
    /// Swaps Left and Right values.
    /// </summary>
    /// <returns>An Either with Left and Right swapped.</returns>
    public Either<R, L> Swap()
    {
        return this switch
        {
            Left left => new Either<R, L>.Right(left.Value),
            Right right => new Either<R, L>.Left(right.Value),
            _ => throw new InvalidOperationException("Either is in an invalid state.")
        };
    }

    /// <summary>
    /// Converts the Either to an Option of the Left value.
    /// </summary>
    /// <returns>Some if Left, None if Right.</returns>
    public Option<L> LeftOption()
    {
        return this is Left left
            ? new Option<L>.Some(left.Value)
            : new Option<L>.None();
    }

    /// <summary>
    /// Converts the Either to an Option of the Right value.
    /// </summary>
    /// <returns>Some if Right, None if Left.</returns>
    public Option<R> RightOption()
    {
        return this is Right right
            ? new Option<R>.Some(right.Value)
            : new Option<R>.None();
    }

    public override string ToString()
    {
        return this switch
        {
            Left left => $"Left({left.Value})",
            Right right => $"Right({right.Value})",
            _ => "Either(invalid)"
        };
    }
}
