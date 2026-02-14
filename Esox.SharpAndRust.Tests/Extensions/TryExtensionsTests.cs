using Esox.SharpAndRusty.Types;
using Esox.SharpAndRusty.Extensions;
using System.Text.Json;

namespace Esox.SharpAndRust.Tests.Extensions;

public class TryExtensionsTests
{
    #region Try<T> Tests

    [Fact]
    public void Try_WithSuccessfulFunction_ReturnsOk()
    {
        // Arrange & Act
        var result = TryExtensions.Try(() => 42);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.TryGetValue(out var value));
        Assert.Equal(42, value);
    }

    [Fact]
    public void Try_WithThrowingFunction_ReturnsErr()
    {
        // Arrange & Act
        var result = TryExtensions.Try<int>(() => throw new InvalidOperationException("Test error"));

        // Assert
        Assert.True(result.IsFailure);
        Assert.True(result.TryGetError(out var error));
        Assert.IsType<InvalidOperationException>(error);
        Assert.Equal("Test error", error.Message);
    }

    [Fact]
    public void Try_WithNullFunction_ThrowsArgumentNullException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            TryExtensions.Try<int>(null!));
    }

    [Fact]
    public void Try_WithJsonDeserialization_Success()
    {
        // Arrange
        var json = """{"Name":"Alice","Age":30}""";

        // Act
        var result = TryExtensions.Try(() =>
            JsonSerializer.Deserialize<TestUser>(json));

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.TryGetValue(out var user));
        Assert.NotNull(user);
        Assert.Equal("Alice", user.Name);
        Assert.Equal(30, user.Age);
    }

    [Fact]
    public void Try_WithJsonDeserialization_Failure()
    {
        // Arrange
        var json = "invalid json";

        // Act
        var result = TryExtensions.Try(() =>
            JsonSerializer.Deserialize<TestUser>(json));

        // Assert
        Assert.True(result.IsFailure);
        Assert.True(result.TryGetError(out var error));
        Assert.IsType<JsonException>(error);
    }

    #endregion

    #region Try<T, E> with Error Mapper Tests

    [Fact]
    public void Try_WithErrorMapper_Success_ReturnsOk()
    {
        // Arrange & Act
        var result = TryExtensions.Try(
            () => int.Parse("42"),
            ex => $"Parse failed: {ex.Message}");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.TryGetValue(out var value));
        Assert.Equal(42, value);
    }

    [Fact]
    public void Try_WithErrorMapper_Failure_ReturnsMappedError()
    {
        // Arrange & Act
        var result = TryExtensions.Try(
            () => int.Parse("invalid"),
            ex => $"Parse failed: {ex.Message}");

        // Assert
        Assert.True(result.IsFailure);
        Assert.True(result.TryGetError(out var error));
        Assert.Contains("Parse failed:", error);
    }

    [Fact]
    public void Try_WithErrorMapper_NullFunction_ThrowsArgumentNullException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            TryExtensions.Try<int, string>(null!, ex => ex.Message));
    }

    [Fact]
    public void Try_WithErrorMapper_NullMapper_ThrowsArgumentNullException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            TryExtensions.Try(() => 42, (Func<Exception, string>)null!));
    }

    [Fact]
    public void Try_WithErrorMapper_CustomErrorType()
    {
        // Arrange
        var errorMapper = (Exception ex) => Error.New(ex.Message)
            .WithKind(ErrorKind.InvalidInput);

        // Act
        var result = TryExtensions.Try(
            () => int.Parse("invalid"),
            errorMapper);

        // Assert
        Assert.True(result.IsFailure);
        Assert.True(result.TryGetError(out var error));
        Assert.Equal(ErrorKind.InvalidInput, error.Kind);
    }

    #endregion

    #region TryAsync<T> Tests

    [Fact]
    public async Task TryAsync_WithSuccessfulFunction_ReturnsOk()
    {
        // Arrange & Act
        var result = await TryExtensions.TryAsync(async () =>
        {
            await Task.Delay(10);
            return 42;
        });

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.TryGetValue(out var value));
        Assert.Equal(42, value);
    }

    [Fact]
    public async Task TryAsync_WithThrowingFunction_ReturnsErr()
    {
        // Arrange & Act
        var result = await TryExtensions.TryAsync<int>(async () =>
        {
            await Task.Delay(10);
            throw new InvalidOperationException("Async error");
        });

        // Assert
        Assert.True(result.IsFailure);
        Assert.True(result.TryGetError(out var error));
        Assert.IsType<InvalidOperationException>(error);
        Assert.Equal("Async error", error.Message);
    }

    [Fact]
    public async Task TryAsync_WithNullFunction_ThrowsArgumentNullException()
    {
        // Arrange, Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await TryExtensions.TryAsync<int>(null!));
    }

    [Fact]
    public async Task TryAsync_WithCancellation_ThrowsOperationCanceledException()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var result = await TryExtensions.TryAsync(async () =>
        {
            await Task.Delay(1000);
            return 42;
        }, cts.Token);

        // Assert
        Assert.True(result.IsFailure);
        Assert.True(result.TryGetError(out var error));
        Assert.IsType<OperationCanceledException>(error);
    }

    [Fact]
    public async Task TryAsync_WithFileRead_Success()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        await File.WriteAllTextAsync(tempFile, "Hello World");

        try
        {
            // Act
            var result = await TryExtensions.TryAsync(async () =>
                await File.ReadAllTextAsync(tempFile));

            // Assert
            Assert.True(result.IsSuccess);
            Assert.True(result.TryGetValue(out var content));
            Assert.Equal("Hello World", content);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    #endregion

    #region TryAsync<T, E> with Error Mapper Tests

    [Fact]
    public async Task TryAsync_WithErrorMapper_Success_ReturnsOk()
    {
        // Arrange & Act
        var result = await TryExtensions.TryAsync(
            async () =>
            {
                await Task.Delay(10);
                return 42;
            },
            ex => $"Async error: {ex.Message}");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.TryGetValue(out var value));
        Assert.Equal(42, value);
    }

    [Fact]
    public async Task TryAsync_WithErrorMapper_Failure_ReturnsMappedError()
    {
        // Arrange & Act
        var result = await TryExtensions.TryAsync(
            async Task<int> () =>
            {
                await Task.Delay(10);
                throw new InvalidOperationException("Async failure");
            },
            ex => $"Error: {ex.Message}");

        // Assert
        Assert.True(result.IsFailure);
        Assert.True(result.TryGetError(out var error));
        Assert.Contains("Error: Async failure", error);
    }

    [Fact]
    public async Task TryAsync_WithErrorMapper_NullFunction_ThrowsArgumentNullException()
    {
        // Arrange, Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await TryExtensions.TryAsync<int, string>(null!, ex => ex.Message));
    }

    [Fact]
    public async Task TryAsync_WithErrorMapper_NullMapper_ThrowsArgumentNullException()
    {
        // Arrange, Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await TryExtensions.TryAsync(
                async () => { await Task.Delay(10); return 42; },
                (Func<Exception, string>)null!));
    }

    #endregion

    #region TryOption<T> Tests

    [Fact]
    public void TryOption_WithSuccessfulFunction_ReturnsSome()
    {
        // Arrange & Act
        var result = TryExtensions.TryOption(() => "Hello");

        // Assert
        Assert.True(result.IsSome());
        if (result is Option<string>.Some some)
        {
            Assert.Equal("Hello", some.Value);
        }
    }

    [Fact]
    public void TryOption_WithNullResult_ReturnsNone()
    {
        // Arrange & Act
        var result = TryExtensions.TryOption<string>(() => null);

        // Assert
        Assert.True(result.IsNone());
    }

    [Fact]
    public void TryOption_WithThrowingFunction_ReturnsNone()
    {
        // Arrange & Act
        var result = TryExtensions.TryOption<string>(() =>
            throw new InvalidOperationException("Error"));

        // Assert
        Assert.True(result.IsNone());
    }

    [Fact]
    public void TryOption_WithNullFunction_ThrowsArgumentNullException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            TryExtensions.TryOption<string>(null!));
    }

    [Fact]
    public void TryOption_WithFirstOrDefault_Found()
    {
        // Arrange
        var users = new[] { new TestUser("Alice", 30), new TestUser("Bob", 25) };

        // Act
        var result = TryExtensions.TryOption(() =>
            users.FirstOrDefault(u => u.Name == "Alice"));

        // Assert
        Assert.True(result.IsSome());
        if (result is Option<TestUser>.Some some)
        {
            Assert.Equal("Alice", some.Value.Name);
        }
    }

    [Fact]
    public void TryOption_WithFirstOrDefault_NotFound()
    {
        // Arrange
        var users = new[] { new TestUser("Alice", 30), new TestUser("Bob", 25) };

        // Act
        var result = TryExtensions.TryOption(() =>
            users.FirstOrDefault(u => u.Name == "Charlie"));

        // Assert
        Assert.True(result.IsNone());
    }

    #endregion

    #region TryOptionAsync<T> Tests

    [Fact]
    public async Task TryOptionAsync_WithSuccessfulFunction_ReturnsSome()
    {
        // Arrange & Act
        var result = await TryExtensions.TryOptionAsync(async () =>
        {
            await Task.Delay(10);
            return "Hello";
        });

        // Assert
        Assert.True(result.IsSome());
        if (result is Option<string>.Some some)
        {
            Assert.Equal("Hello", some.Value);
        }
    }

    [Fact]
    public async Task TryOptionAsync_WithNullResult_ReturnsNone()
    {
        // Arrange & Act
        var result = await TryExtensions.TryOptionAsync<string>(async () =>
        {
            await Task.Delay(10);
            return null;
        });

        // Assert
        Assert.True(result.IsNone());
    }

    [Fact]
    public async Task TryOptionAsync_WithThrowingFunction_ReturnsNone()
    {
        // Arrange & Act
        var result = await TryExtensions.TryOptionAsync<string>(async () =>
        {
            await Task.Delay(10);
            throw new InvalidOperationException("Error");
        });

        // Assert
        Assert.True(result.IsNone());
    }

    [Fact]
    public async Task TryOptionAsync_WithNullFunction_ThrowsArgumentNullException()
    {
        // Arrange, Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await TryExtensions.TryOptionAsync<string>(null!));
    }

    [Fact]
    public async Task TryOptionAsync_WithCancellation_ReturnsNone()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var result = await TryExtensions.TryOptionAsync(async () =>
        {
            await Task.Delay(1000);
            return "Hello";
        }, cts.Token);

        // Assert
        Assert.True(result.IsNone());
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void Integration_JsonParsingPipeline()
    {
        // Arrange
        var json = """{"Name":"Alice","Age":30}""";

        // Act
        var result = TryExtensions.Try(
                () => JsonSerializer.Deserialize<TestUser>(json),
                ex => $"Deserialization failed: {ex.Message}")
            .Bind(user => user != null
                ? Result<TestUser, string>.Ok(user)
                : Result<TestUser, string>.Err("User was null"))
            .Map(user => user.Name);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.TryGetValue(out var name));
        Assert.Equal("Alice", name);
    }

    [Fact]
    public async Task Integration_AsyncDataProcessing()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        await File.WriteAllTextAsync(tempFile, "42");

        try
        {
            // Act
            var result = await TryExtensions.TryAsync(
                async () => await File.ReadAllTextAsync(tempFile),
                ex => $"Read failed: {ex.Message}");

            var parsed = result.Bind(content =>
                TryExtensions.Try(
                    () => int.Parse(content),
                    ex => $"Parse failed: {ex.Message}"));

            // Assert
            Assert.True(parsed.IsSuccess);
            Assert.True(parsed.TryGetValue(out var value));
            Assert.Equal(42, value);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void Integration_OptionChaining()
    {
        // Arrange
        var users = new[]
        {
            new TestUser("Alice", 30),
            new TestUser("Bob", 25),
            new TestUser("Charlie", 35)
        };

        // Act
        var result = TryExtensions.TryOption(() =>
                users.FirstOrDefault(u => u.Name == "Bob"))
            .Filter(u => u.Age >= 18)
            .Map(u => $"{u.Name} is {u.Age} years old");

        // Assert
        Assert.True(result.IsSome());
        if (result is Option<string>.Some some)
        {
            Assert.Equal("Bob is 25 years old", some.Value);
        }
    }

    #endregion

    #region Helper Types

    private record TestUser(string Name, int Age);

    #endregion
}
