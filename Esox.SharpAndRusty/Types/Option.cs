namespace Esox.SharpAndRusty.Types;

public abstract record Option<T>
{
    public sealed record Some(T Value) : Option<T>;
    public sealed record None() : Option<T>;

    /// <summary>
    /// Implicitly converts a value of type <typeparamref name="T"/> to an <see cref="Option{T}.Some"/>.
    /// </summary>
    /// <param name="value">The value to wrap.</param>
    /// <returns>An <see cref="Option{T}"/> containing the value.</returns>
    public static implicit operator Option<T>(T value)
    {
        return new Some(value);
    }
}