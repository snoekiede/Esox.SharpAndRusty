using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;

// ReSharper disable HeapView.ObjectAllocation
// ReSharper disable HeapView.ObjectAllocation.Evident
// ReSharper disable HeapView.BoxingAllocation
namespace Esox.SharpAndRusty.Types;

/// <summary>
///     Represents a rich error type inspired by Rust's error handling patterns.
///     Provides context chaining, stack trace capture, and error categorization.
/// </summary>
public sealed class Error : IEquatable<Error>
{
    private const int MaxErrorChainDepth = 50;

    private readonly ImmutableDictionary<string, object>? _metadata;

    private Error(string message, ErrorKind kind, Error? source, string? stackTrace,
        ImmutableDictionary<string, object>? metadata)
    {
        Message = message ?? throw new ArgumentNullException(nameof(message));
        Kind = kind;
        Source = source;
        StackTrace = stackTrace;
        _metadata = metadata;
    }

    /// <summary>
    ///     Gets the error message.
    /// </summary>
    public string Message { get; }

    /// <summary>
    ///     Gets the error kind/category.
    /// </summary>
    public ErrorKind Kind { get; }

    /// <summary>
    ///     Gets the source error that caused this error, if any.
    /// </summary>
    public Error? Source { get; }

    /// <summary>
    ///     Gets the stack trace where this error was created, if captured.
    /// </summary>
    public string? StackTrace { get; }

    /// <summary>
    ///     Gets whether this error has a source error.
    /// </summary>
    public bool HasSource => Source is not null;

    /// <summary>
    ///     Determines whether the specified error is equal to the current error.
    /// </summary>
    public bool Equals(Error? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return Message == other.Message &&
               Kind == other.Kind &&
               Equals(Source, other.Source);
    }

    /// <summary>
    ///     Creates a new error with the specified message.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <returns>A new <see cref="Error" /> instance.</returns>
    public static Error New(string message) => message is null
        ? throw new ArgumentNullException(nameof(message))
        : new Error(message, ErrorKind.Other, null, null, null);

    /// <summary>
    ///     Creates a new error with the specified message and kind.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="kind">The error kind/category.</param>
    /// <returns>A new <see cref="Error" /> instance.</returns>
    public static Error New(string message, ErrorKind kind) => message is null
        ? throw new ArgumentNullException(nameof(message))
        : new Error(message, kind, null, null, null);

    /// <summary>
    ///     Creates a new error from an exception.
    ///     Captures the exception message, stack trace, and inner exceptions as source errors.
    /// </summary>
    /// <param name="exception">The exception to convert to an error.</param>
    /// <returns>A new <see cref="Error" /> instance.</returns>
    public static Error FromException(Exception exception)
    {
        if (exception is null) throw new ArgumentNullException(nameof(exception));

        ErrorKind kind = exception switch
        {
            ArgumentException or ArgumentNullException or ArgumentOutOfRangeException => ErrorKind.InvalidInput,
            InvalidOperationException => ErrorKind.InvalidOperation,
            NotSupportedException => ErrorKind.NotSupported,
            UnauthorizedAccessException => ErrorKind.PermissionDenied,
            FileNotFoundException or DirectoryNotFoundException => ErrorKind.NotFound,
            TimeoutException => ErrorKind.Timeout,
            OperationCanceledException or TaskCanceledException => ErrorKind.Interrupted,
            IOException => ErrorKind.Io,
            OutOfMemoryException => ErrorKind.ResourceExhausted,
            FormatException => ErrorKind.ParseError,
            _ => ErrorKind.Other
        };

        var source = exception.InnerException is not null
            ? FromException(exception.InnerException)
            : null;

        return new Error(
            exception.Message,
            kind,
            source,
            exception.StackTrace,
            null
        );
    }

    /// <summary>
    ///     Adds context to this error, creating a new error with additional information.
    ///     Similar to Rust's context pattern for error handling.
    /// </summary>
    /// <param name="contextMessage">Additional context message.</param>
    /// <returns>A new <see cref="Error" /> that wraps this error with additional context.</returns>
    public Error WithContext(string contextMessage)
    {
        ArgumentNullException.ThrowIfNull(contextMessage);
        return new Error(contextMessage, Kind, this, null, null);
    }

    /// <summary>
    ///     Adds structured metadata to this error.
    ///     Creates a new error with the metadata attached using efficient immutable collections.
    /// </summary>
    /// <param name="key">The metadata key.</param>
    /// <param name="value">The metadata value. Must be a serializable type (primitives, string, DateTime, Guid, etc.).</param>
    /// <returns>A new <see cref="Error" /> with the metadata attached.</returns>
    /// <exception cref="ArgumentException">Thrown when the value type is not suitable for metadata.</exception>
    public Error WithMetadata(string key, object value)
    {
        if (key is null) throw new ArgumentNullException(nameof(key));
        if (value is null) throw new ArgumentNullException(nameof(value));

        if (!IsMetadataTypeValid(value))
            throw new ArgumentException(
                $"Type {value.GetType().Name} is not suitable for metadata. " +
                $"Use primitive types, string, DateTime, Guid, or other serializable types.",
                nameof(value));

        var newMetadata = (_metadata ?? ImmutableDictionary<string, object>.Empty)
            .SetItem(key, value);

        return new Error(Message, Kind, Source, StackTrace, newMetadata);
    }

    /// <summary>
    ///     Adds structured metadata to this error with compile-time type safety.
    ///     This overload provides better type safety and IntelliSense support for value types.
    /// </summary>
    /// <typeparam name="T">The type of the metadata value. Must be a value type (struct).</typeparam>
    /// <param name="key">The metadata key.</param>
    /// <param name="value">The metadata value.</param>
    /// <returns>A new <see cref="Error" /> with the metadata attached.</returns>
    /// <example>
    ///     <code>
    /// var error = Error.New("Operation failed")
    ///     .WithMetadata("userId", 123)           // int - type-safe
    ///     .WithMetadata("timestamp", DateTime.UtcNow)  // DateTime - type-safe
    ///     .WithMetadata("isRetryable", true);    // bool - type-safe
    /// </code>
    /// </example>
    public Error WithMetadata<T>(string key, T value) where T : struct
    {
        ArgumentNullException.ThrowIfNull(key);
        return WithMetadata(key, (object)value);
    }

    /// <summary>
    ///     Attempts to get metadata by key with type-safe casting.
    /// </summary>
    /// <typeparam name="T">The expected type of the metadata value.</typeparam>
    /// <param name="key">The metadata key.</param>
    /// <param name="value">
    ///     When this method returns, contains the typed metadata value if found and of correct type;
    ///     otherwise, default(T).
    /// </param>
    /// <returns>true if the metadata was found and is of type T; otherwise, false.</returns>
    /// <example>
    ///     <code>
    /// if (error.TryGetMetadata("userId", out int userId))
    /// {
    ///     Console.WriteLine($"User ID: {userId}");
    /// }
    /// </code>
    /// </example>
    public bool TryGetMetadata<T>(string key, out T? value)
    {
        if (key is null) throw new ArgumentNullException(nameof(key));

        if (_metadata is not null && _metadata.TryGetValue(key, out var objValue))
            if (objValue is T typedValue)
            {
                value = typedValue;
                return true;
            }

        value = default;
        return false;
    }

    /// <summary>
    ///     Changes the error kind, creating a new error with the specified kind.
    /// </summary>
    /// <param name="kind">The new error kind.</param>
    /// <returns>A new <see cref="Error" /> with the specified kind.</returns>
    public Error WithKind(ErrorKind kind) => new(Message, kind, Source, StackTrace, _metadata);

    /// <summary>
    ///     Captures the current stack trace and attaches it to this error.
    ///     Note: Stack trace capture has performance implications. Use sparingly in production code.
    /// </summary>
    /// <param name="includeFileInfo">
    ///     Whether to include file name and line number information.
    ///     Setting to true significantly impacts performance but provides more detailed debugging information.
    /// </param>
    /// <returns>A new <see cref="Error" /> with the captured stack trace.</returns>
    public Error CaptureStackTrace(bool includeFileInfo = false)
    {
        var stackTrace = new StackTrace(1, includeFileInfo).ToString();
        return new Error(Message, Kind, Source, stackTrace, _metadata);
    }

    /// <summary>
    ///     Attempts to get metadata by key.
    /// </summary>
    /// <param name="key">The metadata key.</param>
    /// <param name="value">When this method returns, contains the metadata value if found; otherwise, null.</param>
    /// <returns>true if the metadata was found; otherwise, false.</returns>
    public bool TryGetMetadata(string key, out object? value)
    {
        if (key is null) throw new ArgumentNullException(nameof(key));

        if (_metadata is not null && _metadata.TryGetValue(key, out var val))
        {
            value = val;
            return true;
        }

        value = null;
        return false;
    }

    /// <summary>
    ///     Gets the full error chain as a formatted string, including all source errors.
    ///     Error chains deeper than 50 levels will be truncated to prevent stack overflow.
    ///     Circular references are detected and handled gracefully.
    /// </summary>
    /// <returns>A string representation of the complete error chain.</returns>
    public string GetFullMessage()
    {
        var sb = new StringBuilder();
        var visited = new HashSet<Error>();
        AppendErrorChain(sb, this, 0, visited);
        return sb.ToString();
    }

    private static void AppendErrorChain(StringBuilder sb, Error error, int depth, HashSet<Error> visited)
    {
        while (true)
        {
            if (depth >= MaxErrorChainDepth)
            {
                var indent = new string(' ', depth * 2);
                sb.AppendLine($"{indent}... (error chain truncated at depth {MaxErrorChainDepth})");
                return;
            }

            // Check for circular reference
            if (!visited.Add(error))
            {
                var indent = new string(' ', depth * 2);
                sb.AppendLine($"{indent}... (circular reference detected)");
                return;
            }

            var currentIndent = new string(' ', depth * 2);
            sb.AppendLine($"{currentIndent}{error.Kind}: {error.Message}");

            if (error._metadata is not null && error._metadata.Count > 0)
                foreach (var kvp in error._metadata)
                {
                    sb.AppendLine($"{currentIndent}  [{kvp.Key}={kvp.Value}]");
                }

            if (error.Source is null) return;
            sb.AppendLine($"{currentIndent}Caused by:");
            error = error.Source;
            depth = depth + 1;
        }
    }

    /// <summary>
    ///     Returns a string representation of this error.
    /// </summary>
    public override string ToString() => HasSource ? GetFullMessage() : $"{Kind}: {Message}";

    /// <summary>
    ///     Determines whether the specified object is equal to the current error.
    /// </summary>
    public override bool Equals(object? obj) => obj is Error other && Equals(other);

    /// <summary>
    ///     Returns the hash code for this error.
    /// </summary>
    public override int GetHashCode() => HashCode.Combine(Message, Kind, Source);

    /// <summary>
    ///     Determines whether two errors are equal.
    /// </summary>
    public static bool operator ==(Error? left, Error? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    /// <summary>
    ///     Determines whether two errors are not equal.
    /// </summary>
    public static bool operator !=(Error? left, Error? right) => !(left == right);

    /// <summary>
    ///     Implicitly converts a string to an Error.
    /// </summary>
    public static implicit operator Error(string message) => New(message);

    private static bool IsMetadataTypeValid(object value)
    {
        var type = value.GetType();

        // Allow primitive types
        if (type.IsPrimitive)
            return true;

        // Allow common value types
        if (type == typeof(string) ||
            type == typeof(DateTime) ||
            type == typeof(DateTimeOffset) ||
            type == typeof(TimeSpan) ||
            type == typeof(Guid) ||
            type == typeof(decimal))
            return true;

        // Allow enums
        if (type.IsEnum)
            return true;

        // Check if type is serializable (has SerializableAttribute or is a record/struct)
        return type.IsValueType || type.GetCustomAttributes(typeof(SerializableAttribute), false).Length > 0;
    }
}

/// <summary>
///     Categorizes different kinds of errors, similar to std::io::ErrorKind in Rust.
/// </summary>
public enum ErrorKind
{
    /// <summary>
    ///     An entity was not found.
    /// </summary>
    NotFound,

    /// <summary>
    ///     The operation lacked the necessary privileges to complete.
    /// </summary>
    PermissionDenied,

    /// <summary>
    ///     The connection was refused by the remote server.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global",
        Justification = "Will be used vy clients of this nuget package")]
    ConnectionRefused,

    /// <summary>
    ///     The connection was reset by the remote server.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global",
        Justification = "Will be used vy clients of this nuget package")]
    ConnectionReset,

    /// <summary>
    ///     The operation timed out.
    /// </summary>
    Timeout,

    /// <summary>
    ///     The operation was interrupted.
    /// </summary>
    Interrupted,

    /// <summary>
    ///     Data provided was invalid.
    /// </summary>
    InvalidInput,

    /// <summary>
    ///     The operation is not supported.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global",
        Justification = "Will be used vy clients of this nuget package")]
    NotSupported,

    /// <summary>
    ///     An I/O error occurred.
    /// </summary>
    Io,

    /// <summary>
    ///     An entity already exists.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global",
        Justification = "Will be used vy clients of this nuget package")]
    AlreadyExists,

    /// <summary>
    ///     The operation was invalid for the current state.
    /// </summary>
    InvalidOperation,

    /// <summary>
    ///     A parsing error occurred.
    /// </summary>
    ParseError,

    /// <summary>
    ///     A resource was exhausted (e.g., out of memory, disk full).
    /// </summary>
    ResourceExhausted,

    /// <summary>
    ///     Indicates that an operation failed due to the object being in an invalid state.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global",
        Justification = "Will be used vy clients of this nuget package")]
    InvalidState,

    /// <summary>
    ///     An unclassified error.
    /// </summary>
    Other
}