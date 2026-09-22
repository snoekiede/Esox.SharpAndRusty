// ReSharper disable HeapView.ObjectAllocation
// ReSharper disable HeapView.ObjectAllocation.Evident
namespace Esox.SharpAndRusty.Types;

/// <summary>
/// Represents either a value of type <typeparamref name="T" /> or no value.
/// </summary>
/// <typeparam name="T">The type of the optional value.</typeparam>
public abstract record Option<T>
{
    /// <summary>
    ///     Implicitly converts a value of type <typeparamref name="T" /> to an <see cref="Option{T}.Some" />.
    ///     For reference types, null values are converted to <see cref="Option{T}.None" />.
    /// </summary>
    /// <param name="value">The value to wrap.</param>
    /// <returns>An <see cref="Option{T}" /> containing the value, or <see cref="Option{T}.None" /> if the value is null.</returns>
    public static implicit operator Option<T>(T value) =>
        // ReSharper disable once HeapView.ObjectAllocation.Evident
        value is null ? new None() : new Some(value);

    /// <summary>
    /// Represents an option containing a value.
    /// </summary>
    /// <param name="Value">The contained value.</param>
    public sealed record Some(T Value) : Option<T>;

    /// <summary>
    /// Represents an option without a value.
    /// </summary>
    public sealed record None : Option<T>;
}