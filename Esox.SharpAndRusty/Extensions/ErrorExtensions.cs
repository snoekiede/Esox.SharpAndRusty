using Esox.SharpAndRusty.Types;

namespace Esox.SharpAndRusty.Extensions
{
    /// <summary>
    /// Extension methods for working with <see cref="Error"/> type.
    /// </summary>
    public static class ErrorExtensions
    {
        /// <param name="result">The result to add context to.</param>
        /// <typeparam name="T">The type of the success value.</typeparam>
        extension<T>(Result<T, Error> result)
        {
            /// <summary>
            /// Adds context to the error in a Result, wrapping the error with additional information.
            /// If the result is successful, it is returned unchanged.
            /// Similar to Rust's `context()` pattern for error handling.
            /// </summary>
            /// <param name="contextMessage">The context message to add.</param>
            /// <returns>The original result if successful; otherwise, a result with the contextualized error.</returns>
            /// <exception cref="ArgumentNullException">Thrown when contextMessage is null.</exception>
            /// <example>
            /// <code>
            /// var result = ReadFile("config.json")
            ///     .Context("Failed to load configuration");
            /// // Error: "Failed to load configuration"
            /// //   Caused by: "File not found: config.json"
            /// </code>
            /// </example>
            public Result<T, Error> Context(string contextMessage)
            {
                if (contextMessage is null) throw new ArgumentNullException(nameof(contextMessage));
                
                return result.MapError(error => error.WithContext(contextMessage));
            }

            /// <summary>
            /// Adds context to the error using a function that can access the error.
            /// Useful when you need to include error details in the context message.
            /// </summary>
            /// <param name="contextFactory">A function that takes the error and produces a context message.</param>
            /// <returns>The original result if successful; otherwise, a result with the contextualized error.</returns>
            /// <exception cref="ArgumentNullException">Thrown when contextFactory is null.</exception>
            /// <example>
            /// <code>
            /// var result = ParseInt(input)
            ///     .WithContext(err => $"Failed to parse user age: {err.Message}");
            /// </code>
            /// </example>
            public Result<T, Error> WithContext(Func<Error, string> contextFactory)
            {
                if (contextFactory is null) throw new ArgumentNullException(nameof(contextFactory));
                
                return result.MapError(error => error.WithContext(contextFactory(error)));
            }

            /// <summary>
            /// Attaches metadata to the error in a Result.
            /// If the result is successful, it is returned unchanged.
            /// </summary>
            /// <param name="key">The metadata key.</param>
            /// <param name="value">The metadata value.</param>
            /// <returns>The original result if successful; otherwise, a result with the error containing the metadata.</returns>
            /// <exception cref="ArgumentNullException">Thrown when key or value is null.</exception>
            /// <example>
            /// <code>
            /// var result = GetUser(userId)
            ///     .WithMetadata("userId", userId)
            ///     .WithMetadata("timestamp", DateTime.UtcNow);
            /// </code>
            /// </example>
            public Result<T, Error> WithMetadata(string key, object value)
            {
                if (key is null) throw new ArgumentNullException(nameof(key));
                if (value is null) throw new ArgumentNullException(nameof(value));
                
                return result.MapError(error => error.WithMetadata(key, value));
            }

            /// <summary>
            /// Attaches type-safe metadata to the error in a Result.
            /// If the result is successful, it is returned unchanged.
            /// </summary>
            /// <typeparam name="TValue">The type of the metadata value. Must be a value type (struct).</typeparam>
            /// <param name="key">The metadata key.</param>
            /// <param name="value">The metadata value.</param>
            /// <returns>The original result if successful; otherwise, a result with the error containing the metadata.</returns>
            /// <exception cref="ArgumentNullException">Thrown when key is null.</exception>
            /// <example>
            /// <code>
            /// var result = GetUser(userId)
            ///     .WithMetadata("userId", 123)           // Type-safe: int
            ///     .WithMetadata("timestamp", DateTime.UtcNow);  // Type-safe: DateTime
            /// </code>
            /// </example>
            public Result<T, Error> WithMetadata<TValue>(string key, TValue value) where TValue : struct
            {
                if (key is null) throw new ArgumentNullException(nameof(key));
                
                return result.MapError(error => error.WithMetadata(key, value));
            }

            /// <summary>
            /// Changes the error kind in a Result.
            /// If the result is successful, it is returned unchanged.
            /// </summary>
            /// <param name="kind">The new error kind.</param>
            /// <returns>The original result if successful; otherwise, a result with the error having the specified kind.</returns>
            /// <example>
            /// <code>
            /// var result = ValidateUser(user)
            ///     .WithKind(ErrorKind.InvalidInput);
            /// </code>
            /// </example>
            public Result<T, Error> WithKind(ErrorKind kind)
            {
                return result.MapError(error => error.WithKind(kind));
            }
        }

        /// <param name="result">The result task to add context to.</param>
        /// <typeparam name="T">The type of the success value.</typeparam>
        extension<T>(Task<Result<T, Error>> result)
        {
            /// <summary>
            /// Adds context to the error in an async Result.
            /// If the result is successful, it is returned unchanged.
            /// </summary>
            /// <param name="contextMessage">The context message to add.</param>
            /// <param name="cancellationToken">Optional cancellation token.</param>
            /// <returns>The original result if successful; otherwise, a result with the contextualized error.</returns>
            /// <exception cref="ArgumentNullException">Thrown when contextMessage is null.</exception>
            /// <example>
            /// <code>
            /// var result = await LoadUserAsync(userId)
            ///     .ContextAsync("Failed to load user from database");
            /// </code>
            /// </example>
            public async Task<Result<T, Error>> ContextAsync(string contextMessage, CancellationToken cancellationToken = default)
            {
                if (contextMessage is null) throw new ArgumentNullException(nameof(contextMessage));
                
                var res = await result.ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
                
                return res.MapError(error => error.WithContext(contextMessage));
            }

            /// <summary>
            /// Adds context to the error in an async Result using a factory function.
            /// </summary>
            /// <param name="contextFactory">A function that takes the error and produces a context message.</param>
            /// <param name="cancellationToken">Optional cancellation token.</param>
            /// <returns>The original result if successful; otherwise, a result with the contextualized error.</returns>
            /// <exception cref="ArgumentNullException">Thrown when contextFactory is null.</exception>
            public async Task<Result<T, Error>> WithContextAsync(Func<Error, string> contextFactory, CancellationToken cancellationToken = default)
            {
                if (contextFactory is null) throw new ArgumentNullException(nameof(contextFactory));
                
                var res = await result.ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
                
                return res.MapError(error => error.WithContext(contextFactory(error)));
            }

            /// <summary>
            /// Attaches metadata to the error in an async Result.
            /// </summary>
            /// <param name="key">The metadata key.</param>
            /// <param name="value">The metadata value.</param>
            /// <param name="cancellationToken">Optional cancellation token.</param>
            /// <returns>The original result if successful; otherwise, a result with the error containing the metadata.</returns>
            /// <exception cref="ArgumentNullException">Thrown when key or value is null.</exception>
            public async Task<Result<T, Error>> WithMetadataAsync(string key, object value, CancellationToken cancellationToken = default)
            {
                if (key is null) throw new ArgumentNullException(nameof(key));
                if (value is null) throw new ArgumentNullException(nameof(value));
                
                var res = await result.ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
                
                return res.MapError(error => error.WithMetadata(key, value));
            }

            /// <summary>
            /// Attaches type-safe metadata to the error in an async Result.
            /// </summary>
            /// <typeparam name="TValue">The type of the metadata value. Must be a value type (struct).</typeparam>
            /// <param name="key">The metadata key.</param>
            /// <param name="value">The metadata value.</param>
            /// <param name="cancellationToken">Optional cancellation token.</param>
            /// <returns>The original result if successful; otherwise, a result with the error containing the metadata.</returns>
            /// <exception cref="ArgumentNullException">Thrown when key is null.</exception>
            public async Task<Result<T, Error>> WithMetadataAsync<TValue>(string key, TValue value, CancellationToken cancellationToken = default) where TValue : struct
            {
                if (key is null) throw new ArgumentNullException(nameof(key));
                
                var res = await result.ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
                
                return res.MapError(error => error.WithMetadata(key, value));
            }
        }

        /// <summary>
        /// Converts an exception to an Error and wraps it in a failed Result.
        /// </summary>
        /// <typeparam name="T">The type of the success value.</typeparam>
        /// <param name="exception">The exception to convert.</param>
        /// <returns>A failed Result containing the error.</returns>
        /// <exception cref="ArgumentNullException">Thrown when exception is null.</exception>
        public static Result<T, Error> ToResult<T>(this Exception exception)
        {
            if (exception is null) throw new ArgumentNullException(nameof(exception));
            return Result<T, Error>.Err(Error.FromException(exception));
        }

        /// <summary>
        /// Executes an operation and converts any exception to an Error.
        /// Similar to Result.Try but specifically for Error type.
        /// </summary>
        /// <typeparam name="T">The type of the success value.</typeparam>
        /// <param name="operation">The operation to execute.</param>
        /// <returns>A Result containing either the operation result or an Error.</returns>
        /// <exception cref="ArgumentNullException">Thrown when operation is null.</exception>
        /// <example>
        /// <code>
        /// var result = ErrorExtensions.Try(() => int.Parse("42"));
        /// // Result: Ok(42)
        /// 
        /// var failed = ErrorExtensions.Try(() => int.Parse("abc"));
        /// // Result: Err(Error with message "Input string was not in a correct format")
        /// </code>
        /// </example>
        public static Result<T, Error> Try<T>(Func<T> operation)
        {
            if (operation is null) throw new ArgumentNullException(nameof(operation));
            
            try
            {
                return Result<T, Error>.Ok(operation());
            }
            catch (Exception ex)
            {
                return Result<T, Error>.Err(Error.FromException(ex));
            }
        }

        /// <summary>
        /// Executes an async operation and converts any exception to an Error.
        /// </summary>
        /// <typeparam name="T">The type of the success value.</typeparam>
        /// <param name="operation">The async operation to execute.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>A Result containing either the operation result or an Error.</returns>
        /// <exception cref="ArgumentNullException">Thrown when operation is null.</exception>
        /// <example>
        /// <code>
        /// var result = await ErrorExtensions.TryAsync(async () => await httpClient.GetStringAsync(url));
        /// </code>
        /// </example>
        public static async Task<Result<T, Error>> TryAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken = default)
        {
            if (operation is null) throw new ArgumentNullException(nameof(operation));
            
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                var value = await operation().ConfigureAwait(false);
                return Result<T, Error>.Ok(value);
            }
            catch (Exception ex)
            {
                return Result<T, Error>.Err(Error.FromException(ex));
            }
        }
    }
}
