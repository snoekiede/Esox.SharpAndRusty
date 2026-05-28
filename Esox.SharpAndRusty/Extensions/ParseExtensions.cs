using System.Diagnostics.CodeAnalysis;
using Esox.SharpAndRusty.Types;

namespace Esox.SharpAndRusty.Extensions;

/// <summary>
/// Provides parsing helpers that return <see cref="Result{T, E}"/> instead of using out parameters.
/// </summary>
public static class ParseExtensions
{
    /// <summary>
    /// Represents a TryParse-like method that accepts a string and returns a parsed value via out parameter.
    /// </summary>
    /// <typeparam name="T">The parsed value type.</typeparam>
    /// <param name="input">The input string to parse.</param>
    /// <param name="value">When this method returns, contains the parsed value if successful; otherwise, default.</param>
    /// <returns><c>true</c> when parsing succeeds; otherwise <c>false</c>.</returns>
    public delegate bool TryParseDelegate<T>(string? input, [MaybeNullWhen(false)] out T value);

    extension(string? input)
    {
        /// <summary>
        /// Parses the current string into <typeparamref name="T"/> and returns a <see cref="Result{T, E}"/>.
        /// </summary>
        /// <typeparam name="T">The target type to parse to.</typeparam>
        /// <param name="provider">Optional format provider used by <see cref="IParsable{TSelf}"/> implementations.</param>
        /// <returns>
        /// <see cref="Result{T, E}.Ok(T)"/> when parsing succeeds; otherwise
        /// <see cref="Result{T, E}.Err(E)"/> with a rich <see cref="Error"/>.
        /// </returns>
        public Result<T, Error> TryParse<T>(IFormatProvider? provider = null)
            where T : IParsable<T>
        {
            if (input is null)
            {
                return Result<T, Error>.Err(
                    Error.New("Input cannot be null.", ErrorKind.InvalidInput)
                        .WithMetadata("targetType", typeof(T).Name));
            }

            if (T.TryParse(input, provider, out var value))
            {
                return Result<T, Error>.Ok(value);
            }

            return Result<T, Error>.Err(
                Error.New($"Could not parse '{input}' as {typeof(T).Name}.", ErrorKind.ParseError)
                    .WithMetadata("input", input)
                    .WithMetadata("targetType", typeof(T).Name));
        }

        /// <summary>
        /// Parses the current string using a supplied TryParse-like delegate and returns a <see cref="Result{T, E}"/>.
        /// </summary>
        /// <typeparam name="T">The target type to parse to.</typeparam>
        /// <param name="tryParse">A TryParse-compatible delegate.</param>
        /// <param name="parserName">Optional parser name for diagnostics.</param>
        /// <returns>
        /// <see cref="Result{T, E}.Ok(T)"/> when parsing succeeds; otherwise
        /// <see cref="Result{T, E}.Err(E)"/> with a rich <see cref="Error"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="tryParse"/> is null.</exception>
        public Result<T, Error> TryParse<T>(TryParseDelegate<T> tryParse, string? parserName = null)
        {
            if (tryParse is null) throw new ArgumentNullException(nameof(tryParse));

            if (input is null)
            {
                return Result<T, Error>.Err(
                    Error.New("Input cannot be null.", ErrorKind.InvalidInput)
                        .WithMetadata("parser", parserName ?? "TryParse")
                        .WithMetadata("targetType", typeof(T).Name));
            }

            if (tryParse(input, out var value))
            {
                return Result<T, Error>.Ok(value);
            }

            return Result<T, Error>.Err(
                Error.New($"Could not parse '{input}' as {typeof(T).Name}.", ErrorKind.ParseError)
                    .WithMetadata("input", input)
                    .WithMetadata("parser", parserName ?? "TryParse")
                    .WithMetadata("targetType", typeof(T).Name));
        }
    }
}

