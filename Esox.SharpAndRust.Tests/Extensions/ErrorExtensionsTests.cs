using Esox.SharpAndRusty.Extensions;
using Esox.SharpAndRusty.Types;
using Xunit;

namespace Esox.SharpAndRust.Tests.Extensions
{
    public class ErrorExtensionsTests
    {
        [Fact]
        public void Context_OnSuccess_ReturnsUnchanged()
        {
            var result = Result<int, Error>.Ok(42);
            var withContext = result.Context("Failed to process");

            Assert.True(withContext.IsSuccess);
            Assert.True(withContext.TryGetValue(out var value));
            Assert.Equal(42, value);
        }

        [Fact]
        public void Context_OnFailure_AddsContextToError()
        {
            var result = Result<int, Error>.Err(Error.New("Original error"));
            var withContext = result.Context("Failed to process data");

            Assert.True(withContext.IsFailure);
            Assert.True(withContext.TryGetError(out var error));
            Assert.Equal("Failed to process data", error!.Message);
            Assert.NotNull(error.Source);
            Assert.Equal("Original error", error.Source!.Message);
        }

        [Fact]
        public void Context_WithNullMessage_ThrowsArgumentNullException()
        {
            var result = Result<int, Error>.Err(Error.New("Test"));
            Assert.Throws<ArgumentNullException>(() => result.Context(null!));
        }

        [Fact]
        public void Context_CanChainMultipleTimes()
        {
            var result = Result<int, Error>.Err(Error.New("Base error"))
                .Context("Step 1 failed")
                .Context("Step 2 failed")
                .Context("Operation failed");

            Assert.True(result.TryGetError(out var error));
            Assert.Equal("Operation failed", error!.Message);
            Assert.Equal("Step 2 failed", error.Source!.Message);
            Assert.Equal("Step 1 failed", error.Source.Source!.Message);
            Assert.Equal("Base error", error.Source.Source.Source!.Message);
        }

        [Fact]
        public void WithContext_OnSuccess_ReturnsUnchanged()
        {
            var result = Result<int, Error>.Ok(42);
            var withContext = result.WithContext(err => $"Failed: {err.Message}");

            Assert.True(withContext.IsSuccess);
        }

        [Fact]
        public void WithContext_OnFailure_UsesFactoryFunction()
        {
            var result = Result<int, Error>.Err(Error.New("Parse error"));
            var withContext = result.WithContext(err => $"Failed to parse input: {err.Message}");

            Assert.True(withContext.TryGetError(out var error));
            Assert.Equal("Failed to parse input: Parse error", error!.Message);
            Assert.Equal("Parse error", error.Source!.Message);
        }

        [Fact]
        public void WithContext_WithNullFactory_ThrowsArgumentNullException()
        {
            var result = Result<int, Error>.Err(Error.New("Test"));
            Assert.Throws<ArgumentNullException>(() => result.WithContext((Func<Error, string>)null!));
        }

        [Fact]
        public void WithMetadata_OnSuccess_ReturnsUnchanged()
        {
            var result = Result<int, Error>.Ok(42);
            var withMetadata = result.WithMetadata("key", "value");

            Assert.True(withMetadata.IsSuccess);
        }

        [Fact]
        public void WithMetadata_OnFailure_AttachesMetadata()
        {
            var result = Result<int, Error>.Err(Error.New("Error"))
                .WithMetadata("userId", 123)
                .WithMetadata("action", "delete");

            Assert.True(result.TryGetError(out var error));
            Assert.True(error!.TryGetMetadata("userId", out var userId));
            Assert.Equal(123, userId);
            Assert.True(error.TryGetMetadata("action", out var action));
            Assert.Equal("delete", action);
        }

        [Fact]
        public void WithMetadata_TypeSafe_OnSuccess_ReturnsUnchanged()
        {
            var result = Result<int, Error>.Ok(42);
            var withMetadata = result.WithMetadata("key", 123);

            Assert.True(withMetadata.IsSuccess);
            Assert.Equal(42, withMetadata.UnwrapOr(0));
        }

        [Fact]
        public void WithMetadata_TypeSafe_OnFailure_AttachesTypedMetadata()
        {
            var result = Result<int, Error>.Err(Error.New("Error"))
                .WithMetadata("count", 42)
                .WithMetadata("isRetryable", true)
                .WithMetadata("timestamp", DateTime.UtcNow);

            Assert.True(result.TryGetError(out var error));
            
            Assert.True(error!.TryGetMetadata("count", out int count));
            Assert.Equal(42, count);

            Assert.True(error.TryGetMetadata("isRetryable", out bool isRetryable));
            Assert.True(isRetryable);

            Assert.True(error.TryGetMetadata("timestamp", out DateTime timestamp));
            Assert.NotEqual(default(DateTime), timestamp);
        }

        [Fact]
        public void WithKind_OnSuccess_ReturnsUnchanged()
        {
            var result = Result<int, Error>.Ok(42);
            var withKind = result.WithKind(ErrorKind.NotFound);

            Assert.True(withKind.IsSuccess);
        }

        [Fact]
        public void WithKind_OnFailure_ChangesErrorKind()
        {
            var result = Result<int, Error>.Err(Error.New("Error"))
                .WithKind(ErrorKind.Timeout);

            Assert.True(result.TryGetError(out var error));
            Assert.Equal(ErrorKind.Timeout, error!.Kind);
        }

        [Fact]
        public async Task ContextAsync_OnSuccess_ReturnsUnchanged()
        {
            var result = Task.FromResult(Result<int, Error>.Ok(42));
            var withContext = await result.ContextAsync("Failed");

            Assert.True(withContext.IsSuccess);
        }

        [Fact]
        public async Task ContextAsync_OnFailure_AddsContext()
        {
            var result = Task.FromResult(Result<int, Error>.Err(Error.New("Original error")));
            var withContext = await result.ContextAsync("Operation failed");

            Assert.True(withContext.TryGetError(out var error));
            Assert.Equal("Operation failed", error!.Message);
            Assert.Equal("Original error", error.Source!.Message);
        }

        [Fact]
        public async Task ContextAsync_WithCancellation_ThrowsWhenCancelled()
        {
            using var cts = new CancellationTokenSource();
            var result = Task.FromResult(Result<int, Error>.Err(Error.New("Error")));
            
            cts.Cancel();

            await Assert.ThrowsAsync<OperationCanceledException>(async () =>
                await result.ContextAsync("Failed", cts.Token));
        }

        [Fact]
        public async Task WithContextAsync_OnFailure_UsesFactoryFunction()
        {
            var result = Task.FromResult(Result<int, Error>.Err(Error.New("Parse error")));
            var withContext = await result.WithContextAsync(err => $"Failed: {err.Message}");

            Assert.True(withContext.TryGetError(out var error));
            Assert.Equal("Failed: Parse error", error!.Message);
        }

        [Fact]
        public async Task WithMetadataAsync_OnFailure_AttachesMetadata()
        {
            var result = Task.FromResult(Result<int, Error>.Err(Error.New("Error")));
            var withMetadata = await result.WithMetadataAsync("key", "value");

            Assert.True(withMetadata.TryGetError(out var error));
            Assert.True(error!.TryGetMetadata("key", out var value));
            Assert.Equal("value", value);
        }

        [Fact]
        public void ToResult_ConvertsExceptionToResult()
        {
            var exception = new InvalidOperationException("Operation failed");
            var result = exception.ToResult<int>();

            Assert.True(result.IsFailure);
            Assert.True(result.TryGetError(out var error));
            Assert.Equal("Operation failed", error!.Message);
            Assert.Equal(ErrorKind.InvalidOperation, error.Kind);
        }

        [Fact]
        public void ToResult_WithNullException_ThrowsArgumentNullException()
        {
            Exception exception = null!;
            Assert.Throws<ArgumentNullException>(() => exception.ToResult<int>());
        }

        [Fact]
        public void Try_OnSuccess_ReturnsOk()
        {
            var result = ErrorExtensions.Try(() => int.Parse("42"));

            Assert.True(result.IsSuccess);
            Assert.True(result.TryGetValue(out var value));
            Assert.Equal(42, value);
        }

        [Fact]
        public void Try_OnException_ReturnsErr()
        {
            var result = ErrorExtensions.Try(() => int.Parse("not a number"));

            Assert.True(result.IsFailure);
            Assert.True(result.TryGetError(out var error));
            Assert.Contains("correct format", error!.Message);
            // FormatException is now mapped to ParseError instead of Other
            Assert.Equal(ErrorKind.ParseError, error.Kind);
        }

        [Fact]
        public void Try_WithNullOperation_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => ErrorExtensions.Try<int>(null!));
        }

        [Fact]
        public async Task TryAsync_OnSuccess_ReturnsOk()
        {
            var result = await ErrorExtensions.TryAsync(async () =>
            {
                await Task.Delay(1);
                return 42;
            });

            Assert.True(result.IsSuccess);
            Assert.True(result.TryGetValue(out var value));
            Assert.Equal(42, value);
        }

        [Fact]
        public async Task TryAsync_OnException_ReturnsErr()
        {
            var result = await ErrorExtensions.TryAsync<int>(async () =>
            {
                await Task.Delay(1);
                throw new InvalidOperationException("Async operation failed");
            });

            Assert.True(result.IsFailure);
            Assert.True(result.TryGetError(out var error));
            Assert.Equal("Async operation failed", error!.Message);
            Assert.Equal(ErrorKind.InvalidOperation, error.Kind);
        }

        [Fact]
        public async Task TryAsync_WithCancellation_ThrowsWhenCancelled()
        {
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            var result = await ErrorExtensions.TryAsync(async () =>
            {
                await Task.Delay(1000);
                return 42;
            }, cts.Token);

            // When operation is cancelled before it starts, TryAsync catches the exception
            // and returns an Err result with the cancellation error
            Assert.True(result.IsFailure);
            Assert.True(result.TryGetError(out var error));
            Assert.Equal(ErrorKind.Interrupted, error!.Kind);
        }

        [Fact]
        public async Task TryAsync_WithNullOperation_ThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await ErrorExtensions.TryAsync<int>(null!));
        }

        [Fact]
        public void ComplexScenario_ChainContextAndMetadata()
        {
            var result = ErrorExtensions.Try(() => int.Parse("not a number"))
                .Context("Failed to parse user input")
                .WithMetadata("input", "not a number")
                .WithMetadata("field", "age")
                .WithKind(ErrorKind.ParseError)
                .Context("User validation failed");

            Assert.True(result.IsFailure);
            Assert.True(result.TryGetError(out var error));
            Assert.Equal("User validation failed", error!.Message);
            Assert.Equal(ErrorKind.ParseError, error.Kind);

            var source = error.Source;
            Assert.NotNull(source);
            Assert.Equal("Failed to parse user input", source!.Message);
            Assert.True(source.TryGetMetadata("input", out var input));
            Assert.Equal("not a number", input);
            Assert.True(source.TryGetMetadata("field", out var field));
            Assert.Equal("age", field);
        }

        [Fact]
        public void RealWorldScenario_FileProcessing()
        {
            // Simulate a file processing operation that fails
            Result<string, Error> ReadFile(string path)
            {
                return Result<string, Error>.Err(Error.New("File not found", ErrorKind.NotFound));
            }

            Result<int, Error> ParseContent(string content)
            {
                return ErrorExtensions.Try(() => int.Parse(content));
            }

            var result = ReadFile("/etc/config.json")
                .Context("Failed to read configuration file")
                .WithMetadata("path", "/etc/config.json")
                .Bind(content => ParseContent(content)
                    .Context("Failed to parse configuration")
                    .WithKind(ErrorKind.ParseError));

            Assert.True(result.IsFailure);
            Assert.True(result.TryGetError(out var error));
            
            var fullMessage = error!.GetFullMessage();
            Assert.Contains("Failed to read configuration file", fullMessage);
            Assert.Contains("path=/etc/config.json", fullMessage);
            Assert.Contains("File not found", fullMessage);
        }
    }
}
