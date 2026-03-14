using Esox.SharpAndRusty.Extensions;
using Esox.SharpAndRusty.Types;

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
            
            await cts.CancelAsync();

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
            await cts.CancelAsync();

            var result = await ErrorExtensions.TryAsync(async () =>
            {
                await Task.Delay(1000, cts.Token);
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

        #region Async Edge Cases

        [Fact]
        public async Task WithMetadataAsync_TypeSafe_OnSuccess_ReturnsUnchanged()
        {
            // Arrange
            var result = Task.FromResult(Result<int, Error>.Ok(42));

            // Act
            var withMetadata = await result.WithMetadataAsync("key", 123);

            // Assert
            Assert.True(withMetadata.IsSuccess);
            Assert.Equal(42, withMetadata.UnwrapOr(0));
        }

        [Fact]
        public async Task WithMetadataAsync_TypeSafe_OnFailure_AttachesTypedMetadata()
        {
            // Arrange
            var result = Task.FromResult(Result<int, Error>.Err(Error.New("Error")));

            // Act
            var withMetadata = await result
                .WithMetadataAsync("count", 42)
                .WithMetadataAsync("isRetryable", true)
                .WithMetadataAsync("timestamp", DateTime.UtcNow);

            // Assert
            Assert.True(withMetadata.TryGetError(out var error));

            Assert.True(error!.TryGetMetadata("count", out int count));
            Assert.Equal(42, count);

            Assert.True(error.TryGetMetadata("isRetryable", out bool isRetryable));
            Assert.True(isRetryable);

            Assert.True(error.TryGetMetadata("timestamp", out DateTime timestamp));
            Assert.NotEqual(default(DateTime), timestamp);
        }

        [Fact]
        public async Task WithMetadataAsync_TypeSafe_WithComplexType_WorksCorrectly()
        {
            // Arrange
            var result = Task.FromResult(Result<int, Error>.Err(Error.New("Error")));
            var metadata = new Dictionary<string, string> { ["key"] = "value" };

            // Act
            var withMetadata = await result.WithMetadataAsync("dict", metadata);

            // Assert
            Assert.True(withMetadata.TryGetError(out var error));
            Assert.True(error!.TryGetMetadata("dict", out Dictionary<string, string> retrieved));
            Assert.Equal("value", retrieved["key"]);
        }

        [Fact]
        public async Task WithMetadataAsync_WithCancellation_ThrowsWhenCancelled()
        {
            // Arrange
            using var cts = new CancellationTokenSource();
            var result = Task.FromResult(Result<int, Error>.Err(Error.New("Error")));

            await cts.CancelAsync();

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(async () =>
                await result.WithMetadataAsync("key", "value", cts.Token));
        }

        [Fact]
        public async Task WithMetadataAsync_TypeSafe_WithCancellation_ThrowsWhenCancelled()
        {
            // Arrange
            using var cts = new CancellationTokenSource();
            var result = Task.FromResult(Result<int, Error>.Err(Error.New("Error")));

            await cts.CancelAsync();

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(async () =>
                await result.WithMetadataAsync("count", 42, cts.Token));
        }

        [Fact]
        public async Task WithContextAsync_WithCancellation_ThrowsWhenCancelled()
        {
            // Arrange
            using var cts = new CancellationTokenSource();
            var result = Task.FromResult(Result<int, Error>.Err(Error.New("Error")));

            await cts.CancelAsync();

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(async () =>
                await result.WithContextAsync(err => $"Failed: {err.Message}", cts.Token));
        }

        [Fact]
        public async Task ContextAsync_ChainedMultipleTimes_PreservesErrorChain()
        {
            // Arrange
            var result = Task.FromResult(Result<int, Error>.Err(Error.New("Base error")));

            // Act
            var final = await result
                .ContextAsync("Step 1 failed")
                .ContextAsync("Step 2 failed")
                .ContextAsync("Operation failed");

            // Assert
            Assert.True(final.TryGetError(out var error));
            Assert.Equal("Operation failed", error!.Message);
            Assert.Equal("Step 2 failed", error.Source!.Message);
            Assert.Equal("Step 1 failed", error.Source.Source!.Message);
            Assert.Equal("Base error", error.Source.Source.Source!.Message);
        }

        [Fact]
        public async Task WithMetadataAsync_ChainedWithContext_PreservesAllData()
        {
            // Arrange
            var result = Task.FromResult(Result<int, Error>.Err(Error.New("Base error")));

            // Act - Chain metadata and context operations
            // WithMetadataAsync adds metadata to the current error
            // ContextAsync wraps the current error in a new error with a context message
            var final = await result
                .WithMetadataAsync("step", 1)          // Base error gets metadata step=1
                .ContextAsync("Step 1 failed")          // Wraps in new error with this message
                .WithMetadataAsync("step", 2)          // That new error gets metadata step=2
                .ContextAsync("Step 2 failed");        // Wraps again in another new error

            // Assert
            Assert.True(final.TryGetError(out var error));

            // The outermost error is "Step 2 failed" with no metadata
            // (ContextAsync creates a new error without metadata)
            Assert.Equal("Step 2 failed", error!.Message);

            // The metadata is on the source (the error that was wrapped)
            var source1 = error.Source;
            Assert.NotNull(source1);
            Assert.Equal("Step 1 failed", source1!.Message);
            Assert.True(source1.TryGetMetadata("step", out var step2));
            Assert.Equal(2, step2); // The metadata added after first ContextAsync

            // Going deeper in the chain
            var source2 = source1.Source;
            Assert.NotNull(source2);
            // This should be the original "Base error" with step=1 metadata
            Assert.Equal("Base error", source2!.Message);
            Assert.True(source2.TryGetMetadata("step", out var step1));
            Assert.Equal(1, step1); // The metadata added initially
        }

        [Fact]
        public async Task TryAsync_WithLongRunningOperation_CompletesSuccessfully()
        {
            // Arrange & Act
            var result = await ErrorExtensions.TryAsync(async () =>
            {
                await Task.Delay(100);
                var sum = 0;
                for (int i = 0; i < 1000; i++)
                {
                    sum += i;
                }
                return sum;
            });

            // Assert
            Assert.True(result.IsSuccess);
            Assert.True(result.TryGetValue(out var value));
            Assert.Equal(499500, value); // Sum of 0..999
        }

        [Fact]
        public async Task TryAsync_WithExceptionInMiddle_CapturesError()
        {
            // Arrange & Act
            var result = await ErrorExtensions.TryAsync(async () =>
            {
                await Task.Delay(50);
                throw new InvalidOperationException("Failed midway");
                #pragma warning disable CS0162 // Unreachable code detected
                return 42;
                #pragma warning restore CS0162
            });

            // Assert
            Assert.True(result.IsFailure);
            Assert.True(result.TryGetError(out var error));
            Assert.Equal("Failed midway", error!.Message);
            Assert.Equal(ErrorKind.InvalidOperation, error.Kind);
        }

        [Fact]
        public async Task TryAsync_WithCancellationAfterStart_CapturesInterrupted()
        {
            // Arrange
            using var cts = new CancellationTokenSource();

            // Act
            var resultTask = ErrorExtensions.TryAsync(async () =>
            {
                await Task.Delay(50, cts.Token);
                await Task.Delay(1000, cts.Token); // This should be cancelled
                return 42;
            }, cts.Token);

            // Cancel after initial delay
            await Task.Delay(100);
            await cts.CancelAsync();

            var result = await resultTask;

            // Assert
            Assert.True(result.IsFailure);
            Assert.True(result.TryGetError(out var error));
            Assert.Equal(ErrorKind.Interrupted, error!.Kind);
        }

        [Fact]
        public async Task AsyncChain_ComplexScenario_WorksEndToEnd()
        {
            // Arrange
            async Task<Result<int, Error>> ReadAsync(string path)
            {
                await Task.Delay(10);
                return Result<int, Error>.Err(Error.New("File not found", ErrorKind.NotFound));
            }

            // Act
            var result = await ReadAsync("/config.json")
                .ContextAsync("Failed to read configuration")
                .WithMetadataAsync("path", "/config.json")
                .WithMetadataAsync("timestamp", DateTime.UtcNow)
                .ContextAsync("Configuration load failed");

            // Assert
            Assert.True(result.IsFailure);
            Assert.True(result.TryGetError(out var error));
            Assert.Equal("Configuration load failed", error!.Message);
            Assert.Equal("Failed to read configuration", error.Source!.Message);
            Assert.True(error.Source.TryGetMetadata("path", out var path));
            Assert.Equal("/config.json", path);
            Assert.True(error.Source.TryGetMetadata("timestamp", out DateTime timestamp));
            Assert.NotEqual(default(DateTime), timestamp);
        }

        #endregion
    }
}

