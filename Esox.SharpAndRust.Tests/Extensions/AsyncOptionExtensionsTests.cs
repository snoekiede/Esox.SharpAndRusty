using Esox.SharpAndRusty.Types;
using Esox.SharpAndRusty.Extensions;

namespace Esox.SharpAndRust.Tests.Extensions;

public class AsyncOptionExtensionsTests
{
    #region MapAsync Tests

    [Fact]
    public async Task MapAsync_WithSome_TransformsValue()
    {
        // Arrange
        var option = new Option<int>.Some(42);

        // Act
        var result = await option.MapAsync(async x =>
        {
            await Task.Delay(10);
            return x.ToString();
        });

        // Assert
        Assert.True(result.IsSome());
        if (result is Option<string>.Some some)
        {
            Assert.Equal("42", some.Value);
        }
    }

    [Fact]
    public async Task MapAsync_WithNone_ReturnsNone()
    {
        // Arrange
        var option = new Option<int>.None();
        var mapperCalled = false;

        // Act
        var result = await option.MapAsync(async x =>
        {
            mapperCalled = true;
            await Task.Delay(10);
            return x.ToString();
        });

        // Assert
        Assert.True(result.IsNone());
        Assert.False(mapperCalled);
    }

    [Fact]
    public async Task MapAsync_WithCancellation_ThrowsOperationCanceledException()
    {
        // Arrange
        var option = new Option<int>.Some(42);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await option.MapAsync(async x =>
            {
                await Task.Delay(10);
                return x.ToString();
            }, cts.Token));
    }

    #endregion

    #region BindAsync Tests

    [Fact]
    public async Task BindAsync_WithSome_ReturnsBoundResult()
    {
        // Arrange
        var option = new Option<int>.Some(42);

        // Act
        var result = await option.BindAsync<int, string>(async x =>
        {
            await Task.Delay(10);
            return x > 10
                ? new Option<string>.Some($"Large: {x}")
                : new Option<string>.None();
        });

        // Assert
        Assert.True(result.IsSome());
        if (result is Option<string>.Some some)
        {
            Assert.Equal("Large: 42", some.Value);
        }
    }

    [Fact]
    public async Task BindAsync_WithSomeButBinderReturnsNone_ReturnsNone()
    {
        // Arrange
        var option = new Option<int>.Some(5);

        // Act
        var result = await option.BindAsync<int, string>(async x =>
        {
            await Task.Delay(10);
            return x > 10
                ? new Option<string>.Some($"Large: {x}")
                : new Option<string>.None();
        });

        // Assert
        Assert.True(result.IsNone());
    }

    [Fact]
    public async Task BindAsync_WithNone_ReturnsNone()
    {
        // Arrange
        var option = new Option<int>.None();
        var binderCalled = false;

        // Act
        var result = await option.BindAsync<int, string>(async x =>
        {
            binderCalled = true;
            await Task.Delay(10);
            return new Option<string>.Some(x.ToString());
        });

        // Assert
        Assert.True(result.IsNone());
        Assert.False(binderCalled);
    }

    #endregion

    #region FilterAsync Tests

    [Fact]
    public async Task FilterAsync_WithSomeAndPredicateTrue_ReturnsSome()
    {
        // Arrange
        var option = new Option<int>.Some(42);

        // Act
        var result = await option.FilterAsync(async x =>
        {
            await Task.Delay(10);
            return x > 10;
        });

        // Assert
        Assert.True(result.IsSome());
        if (result is Option<int>.Some some)
        {
            Assert.Equal(42, some.Value);
        }
    }

    [Fact]
    public async Task FilterAsync_WithSomeAndPredicateFalse_ReturnsNone()
    {
        // Arrange
        var option = new Option<int>.Some(5);

        // Act
        var result = await option.FilterAsync(async x =>
        {
            await Task.Delay(10);
            return x > 10;
        });

        // Assert
        Assert.True(result.IsNone());
    }

    [Fact]
    public async Task FilterAsync_WithNone_ReturnsNone()
    {
        // Arrange
        var option = new Option<int>.None();
        var predicateCalled = false;

        // Act
        var result = await option.FilterAsync(async x =>
        {
            predicateCalled = true;
            await Task.Delay(10);
            return true;
        });

        // Assert
        Assert.True(result.IsNone());
        Assert.False(predicateCalled);
    }

    #endregion

    #region InspectAsync Tests

    [Fact]
    public async Task InspectAsync_WithSome_CallsInspector()
    {
        // Arrange
        var option = new Option<int>.Some(42);
        var inspectedValue = 0;

        // Act
        var result = await option.InspectAsync(async x =>
        {
            await Task.Delay(10);
            inspectedValue = x;
        });

        // Assert
        Assert.Equal(42, inspectedValue);
        Assert.True(result.IsSome());
        if (result is Option<int>.Some some)
        {
            Assert.Equal(42, some.Value);
        }
    }

    [Fact]
    public async Task InspectAsync_WithNone_DoesNotCallInspector()
    {
        // Arrange
        var option = new Option<int>.None();
        var inspectorCalled = false;

        // Act
        var result = await option.InspectAsync(async x =>
        {
            inspectorCalled = true;
            await Task.Delay(10);
        });

        // Assert
        Assert.False(inspectorCalled);
        Assert.True(result.IsNone());
    }

    #endregion

    #region InspectNoneAsync Tests

    [Fact]
    public async Task InspectNoneAsync_WithNone_CallsInspector()
    {
        // Arrange
        var option = new Option<int>.None();
        var inspectorCalled = false;

        // Act
        var result = await option.InspectNoneAsync(async () =>
        {
            inspectorCalled = true;
            await Task.Delay(10);
        });

        // Assert
        Assert.True(inspectorCalled);
        Assert.True(result.IsNone());
    }

    [Fact]
    public async Task InspectNoneAsync_WithSome_DoesNotCallInspector()
    {
        // Arrange
        var option = new Option<int>.Some(42);
        var inspectorCalled = false;

        // Act
        var result = await option.InspectNoneAsync(async () =>
        {
            inspectorCalled = true;
            await Task.Delay(10);
        });

        // Assert
        Assert.False(inspectorCalled);
        Assert.True(result.IsSome());
    }

    #endregion

    #region MatchAsync (Action) Tests

    [Fact]
    public async Task MatchAsync_Action_WithSome_CallsOnSome()
    {
        // Arrange
        var option = new Option<int>.Some(42);
        var someValue = 0;
        var noneCalled = false;

        // Act
        await option.MatchAsync(
            async x =>
            {
                await Task.Delay(10);
                someValue = x;
            },
            async () =>
            {
                noneCalled = true;
                await Task.Delay(10);
            });

        // Assert
        Assert.Equal(42, someValue);
        Assert.False(noneCalled);
    }

    [Fact]
    public async Task MatchAsync_Action_WithNone_CallsOnNone()
    {
        // Arrange
        var option = new Option<int>.None();
        var someCalled = false;
        var noneCalled = false;

        // Act
        await option.MatchAsync(
            async x =>
            {
                someCalled = true;
                await Task.Delay(10);
            },
            async () =>
            {
                noneCalled = true;
                await Task.Delay(10);
            });

        // Assert
        Assert.False(someCalled);
        Assert.True(noneCalled);
    }

    #endregion

    #region MatchAsync (Func) Tests

    [Fact]
    public async Task MatchAsync_Func_WithSome_ReturnsOnSomeResult()
    {
        // Arrange
        var option = new Option<int>.Some(42);

        // Act
        var result = await option.MatchAsync(
            async x =>
            {
                await Task.Delay(10);
                return $"Value: {x}";
            },
            async () =>
            {
                await Task.Delay(10);
                return "No value";
            });

        // Assert
        Assert.Equal("Value: 42", result);
    }

    [Fact]
    public async Task MatchAsync_Func_WithNone_ReturnsOnNoneResult()
    {
        // Arrange
        var option = new Option<int>.None();

        // Act
        var result = await option.MatchAsync(
            async x =>
            {
                await Task.Delay(10);
                return $"Value: {x}";
            },
            async () =>
            {
                await Task.Delay(10);
                return "No value";
            });

        // Assert
        Assert.Equal("No value", result);
    }

    #endregion

    #region OkOrElseAsync Tests

    [Fact]
    public async Task OkOrElseAsync_WithSome_ReturnsOk()
    {
        // Arrange
        var option = new Option<int>.Some(42);
        var factoryCalled = false;

        // Act
        var result = await option.OkOrElseAsync(async () =>
        {
            factoryCalled = true;
            await Task.Delay(10);
            return "Error";
        });

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.TryGetValue(out var value));
        Assert.Equal(42, value);
        Assert.False(factoryCalled);
    }

    [Fact]
    public async Task OkOrElseAsync_WithNone_ReturnsErr()
    {
        // Arrange
        var option = new Option<int>.None();

        // Act
        var result = await option.OkOrElseAsync(async () =>
        {
            await Task.Delay(10);
            return "Error message";
        });

        // Assert
        Assert.True(result.IsFailure);
        Assert.True(result.TryGetError(out var error));
        Assert.Equal("Error message", error);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task Integration_AsyncChaining_WorksCorrectly()
    {
        // Arrange
        var option = new Option<int>.Some(10);
        var log = new List<string>();

        // Act
        var result = await option
            .InspectAsync(async x =>
            {
                await Task.Delay(10);
                log.Add($"Start: {x}");
            })
            .ContinueWith(t => t.Result.FilterAsync(async x =>
            {
                await Task.Delay(10);
                return x > 5;
            }))
            .Unwrap()
            .ContinueWith(t => t.Result.MapAsync(async x =>
            {
                await Task.Delay(10);
                return x * 2;
            }))
            .Unwrap()
            .ContinueWith(t => t.Result.InspectAsync(async x =>
            {
                await Task.Delay(10);
                log.Add($"Final: {x}");
            }))
            .Unwrap();

        // Assert
        Assert.True(result.IsSome());
        Assert.Equal(2, log.Count);
        Assert.Contains("Start: 10", log);
        Assert.Contains("Final: 20", log);
    }

    [Fact]
    public async Task Integration_AsyncUserLookupAndTransform_WorksCorrectly()
    {
        // Arrange
        async Task<Option<string>> GetUserNameAsync(int id)
        {
            await Task.Delay(10);
            return id > 0
                ? new Option<string>.Some($"User{id}")
                : new Option<string>.None();
        }

        async Task<Option<string>> ValidateUserAsync(string name)
        {
            await Task.Delay(10);
            return name.Length > 3
                ? new Option<string>.Some(name)
                : new Option<string>.None();
        }

        var userIdOption = new Option<int>.Some(42);

        // Act
        var result = await userIdOption
            .BindAsync(GetUserNameAsync)
            .ContinueWith(t => t.Result.BindAsync(ValidateUserAsync))
            .Unwrap();

        // Assert
        Assert.True(result.IsSome());
        if (result is Option<string>.Some some)
        {
            Assert.Equal("User42", some.Value);
        }
    }

    [Fact]
    public async Task Integration_AsyncPipeline_WithNone_ShortCircuits()
    {
        // Arrange
        var option = new Option<int>.None();
        var operations = 0;

        // Act
        var result = await option
            .MapAsync(async x =>
            {
                Interlocked.Increment(ref operations);
                await Task.Delay(10);
                return x * 2;
            })
            .ContinueWith(t => t.Result.FilterAsync(async x =>
            {
                Interlocked.Increment(ref operations);
                await Task.Delay(10);
                return true;
            }))
            .Unwrap()
            .ContinueWith(t => t.Result.MapAsync(async x =>
            {
                Interlocked.Increment(ref operations);
                await Task.Delay(10);
                return x.ToString();
            }))
            .Unwrap();

        // Assert
        Assert.True(result.IsNone());
        Assert.Equal(0, operations); // No operations should execute
    }

    [Fact]
    public async Task Integration_ConvertToResultWithAsyncError_WorksCorrectly()
    {
        // Arrange
        var option = new Option<int>.None();

        async Task<string> CreateErrorAsync()
        {
            await Task.Delay(10);
            return "Value not found during async lookup";
        }

        // Act
        var result = await option.OkOrElseAsync(CreateErrorAsync);

        // Assert
        Assert.True(result.IsFailure);
        Assert.True(result.TryGetError(out var error));
        Assert.Equal("Value not found during async lookup", error);
    }

    #endregion
}
