using Esox.SharpAndRusty.Types;
using Esox.SharpAndRusty.Extensions;

namespace Esox.SharpAndRust.Tests.Extensions;

public class ResultAsyncExtensionsTests
{
    #region MapAsync Tests

    [Fact]
    public async Task MapAsync_TaskResult_TransformsSuccessValue()
    {
        // Arrange
        var resultTask = Task.FromResult(Result<int, string>.Ok(42));

        // Act
        var mapped = await resultTask.MapAsync(x => x * 2);

        // Assert
        Assert.True(mapped.IsSuccess);
        Assert.Equal(84, mapped.UnwrapOr(0));
    }

    [Fact]
    public async Task MapAsync_TaskResult_PropagatesError()
    {
        // Arrange
        var resultTask = Task.FromResult(Result<int, string>.Err("Error"));

        // Act
        var mapped = await resultTask.MapAsync(x => x * 2);

        // Assert
        Assert.True(mapped.IsFailure);
        Assert.True(mapped.TryGetError(out var error));
        Assert.Equal("Error", error);
    }

    [Fact]
    public async Task MapAsync_AsyncMapper_TransformsSuccessValue()
    {
        // Arrange
        var result = Result<int, string>.Ok(42);

        // Act
        var mapped = await result.MapAsync(async x =>
        {
            await Task.Delay(1);
            return x * 2;
        });

        // Assert
        Assert.True(mapped.IsSuccess);
        Assert.Equal(84, mapped.UnwrapOr(0));
    }

    [Fact]
    public async Task MapAsync_AsyncMapper_PropagatesError()
    {
        // Arrange
        var result = Result<int, string>.Err("Error");

        // Act
        var mapped = await result.MapAsync(async x =>
        {
            await Task.Delay(1);
            return x * 2;
        });

        // Assert
        Assert.True(mapped.IsFailure);
    }

    #endregion

    #region BindAsync Tests

    [Fact]
    public async Task BindAsync_TaskResult_ChainsSuccessfully()
    {
        // Arrange
        var resultTask = Task.FromResult(Result<int, string>.Ok(10));

        // Act
        var bound = await resultTask.BindAsync(x =>
            Result<int, string>.Ok(x * 2));

        // Assert
        Assert.True(bound.IsSuccess);
        Assert.Equal(20, bound.UnwrapOr(0));
    }

    [Fact]
    public async Task BindAsync_TaskResult_PropagatesOriginalError()
    {
        // Arrange
        var resultTask = Task.FromResult(Result<int, string>.Err("Original error"));

        // Act
        var bound = await resultTask.BindAsync(x =>
            Result<int, string>.Ok(x * 2));

        // Assert
        Assert.True(bound.IsFailure);
        Assert.True(bound.TryGetError(out var error));
        Assert.Equal("Original error", error);
    }

    [Fact]
    public async Task BindAsync_AsyncBinder_ChainsSuccessfully()
    {
        // Arrange
        var result = Result<int, string>.Ok(10);

        // Act
        var bound = await result.BindAsync(async x =>
        {
            await Task.Delay(1);
            return Result<int, string>.Ok(x * 2);
        });

        // Assert
        Assert.True(bound.IsSuccess);
        Assert.Equal(20, bound.UnwrapOr(0));
    }

    [Fact]
    public async Task BindAsync_AsyncBinder_PropagatesError()
    {
        // Arrange
        var result = Result<int, string>.Ok(10);

        // Act
        var bound = await result.BindAsync(async _ =>
        {
            await Task.Delay(1);
            return Result<int, string>.Err("Binder error");
        });

        // Assert
        Assert.True(bound.IsFailure);
        Assert.True(bound.TryGetError(out var error));
        Assert.Equal("Binder error", error);
    }

    [Fact]
    public async Task BindAsync_TaskResultAndAsyncBinder_WorksTogether()
    {
        // Arrange
        var resultTask = Task.FromResult(Result<int, string>.Ok(10));

        // Act
        var bound = await resultTask.BindAsync(async x =>
        {
            await Task.Delay(1);
            return Result<int, string>.Ok(x * 2);
        });

        // Assert
        Assert.True(bound.IsSuccess);
        Assert.Equal(20, bound.UnwrapOr(0));
    }

    #endregion

    #region MapErrorAsync Tests

    [Fact]
    public async Task MapErrorAsync_TransformsErrorType()
    {
        // Arrange
        var resultTask = Task.FromResult(Result<int, string>.Err("404"));

        // Act
        var mapped = await resultTask.MapErrorAsync(int.Parse);

        // Assert
        Assert.True(mapped.IsFailure);
        Assert.True(mapped.TryGetError(out var error));
        Assert.Equal(404, error);
    }

    [Fact]
    public async Task MapErrorAsync_PreservesSuccessValue()
    {
        // Arrange
        var resultTask = Task.FromResult(Result<int, string>.Ok(42));

        // Act
        var mapped = await resultTask.MapErrorAsync(msg => msg.Length);

        // Assert
        Assert.True(mapped.IsSuccess);
        Assert.Equal(42, mapped.UnwrapOr(0));
    }

    #endregion

    #region TapAsync Tests

    [Fact]
    public async Task TapAsync_ExecutesSuccessAction()
    {
        // Arrange
        var resultTask = Task.FromResult(Result<int, string>.Ok(42));
        var successCalled = false;
        var failureCalled = false;

        // Act
        var result = await resultTask.TapAsync(
            onSuccess: async _ =>
            {
                await Task.Delay(1);
                successCalled = true;
            },
            onFailure: async _ =>
            {
                await Task.Delay(1);
                failureCalled = true;
            }
        );

        // Assert
        Assert.True(successCalled);
        Assert.False(failureCalled);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task TapAsync_ExecutesFailureAction()
    {
        // Arrange
        var resultTask = Task.FromResult(Result<int, string>.Err("Error"));
        var successCalled = false;
        var failureCalled = false;

        // Act
        var result = await resultTask.TapAsync(
            onSuccess: async _ =>
            {
                await Task.Delay(1);
                successCalled = true;
            },
            onFailure: async _ =>
            {
                await Task.Delay(1);
                failureCalled = true;
            }
        );

        // Assert
        Assert.False(successCalled);
        Assert.True(failureCalled);
        Assert.True(result.IsFailure);
    }

    #endregion

    #region OrElseAsync Tests

    [Fact]
    public async Task OrElseAsync_ReturnsOriginalOnSuccess()
    {
        // Arrange
        var resultTask = Task.FromResult(Result<int, string>.Ok(42));

        // Act
        var result = await resultTask.OrElseAsync(async _ =>
        {
            await Task.Delay(1);
            return Result<int, string>.Ok(0);
        });

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.UnwrapOr(0));
    }

    [Fact]
    public async Task OrElseAsync_ReturnsAlternativeOnFailure()
    {
        // Arrange
        var resultTask = Task.FromResult(Result<int, string>.Err("Error"));

        // Act
        var result = await resultTask.OrElseAsync(async _ =>
        {
            await Task.Delay(1);
            return Result<int, string>.Ok(99);
        });

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(99, result.UnwrapOr(0));
    }

    #endregion

    #region CombineAsync Tests

    [Fact]
    public async Task CombineAsync_CombinesAllSuccesses()
    {
        // Arrange
        var resultTasks = new[]
        {
            Task.FromResult(Result<int, string>.Ok(1)),
            Task.FromResult(Result<int, string>.Ok(2)),
            Task.FromResult(Result<int, string>.Ok(3))
        };

        // Act
        var combined = await resultTasks.CombineAsync();

        // Assert
        Assert.True(combined.IsSuccess);
        combined.TryGetValue(out var values);
        Assert.Equal([1, 2, 3], values);
    }

    [Fact]
    public async Task CombineAsync_ReturnsFirstError()
    {
        // Arrange
        var resultTasks = new[]
        {
            Task.FromResult(Result<int, string>.Ok(1)),
            Task.FromResult(Result<int, string>.Err("Error 1")),
            Task.FromResult(Result<int, string>.Err("Error 2"))
        };

        // Act
        var combined = await resultTasks.CombineAsync();

        // Assert
        Assert.True(combined.IsFailure);
        combined.TryGetError(out var error);
        Assert.Equal("Error 1", error);
    }

    [Fact]
    public async Task CombineAsync_HandlesAsyncOperations()
    {
        // Arrange
        async Task<Result<int, string>> GetValueAsync(int value)
        {
            await Task.Delay(1);
            return Result<int, string>.Ok(value);
        }

        var resultTasks = new[]
        {
            GetValueAsync(1),
            GetValueAsync(2),
            GetValueAsync(3)
        };

        // Act
        var combined = await resultTasks.CombineAsync();

        // Assert
        Assert.True(combined.IsSuccess);
        combined.TryGetValue(out var values);
        Assert.Equal([1, 2, 3], values);
    }

    #endregion

    #region Complex Async Chains

    [Fact]
    public async Task ComplexAsyncChain_WorksCorrectly()
    {
        // Arrange
        async Task<Result<int, string>> GetValueAsync(int value)
        {
            await Task.Delay(1);
            return Result<int, string>.Ok(value);
        }

        async Task<Result<int, string>> DoubleAsync(int value)
        {
            await Task.Delay(1);
            return Result<int, string>.Ok(value * 2);
        }

        // Act
        var result = await GetValueAsync(10)
            .MapAsync(x => x + 5)
            .BindAsync(DoubleAsync)
            .BindAsync(async x =>
            {
                await Task.Delay(1);
                return Result<int, string>.Ok(x + 10);
            });

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(40, result.UnwrapOr(0)); // (10 + 5) * 2 + 10 = 40
    }

    [Fact]
    public async Task AsyncChain_StopsOnFirstError()
    {
        // Arrange
        async Task<Result<int, string>> GetValueAsync(int value)
        {
            await Task.Delay(1);
            return Result<int, string>.Ok(value);
        }

        async Task<Result<int, string>> FailAsync(int value)
        {
            await Task.Delay(1);
            return Result<int, string>.Err("Failed");
        }

        var thirdOperationCalled = false;

        // Act
        var result = await GetValueAsync(10)
            .MapAsync(x => x + 5)
            .BindAsync(FailAsync)
            .BindAsync(async x =>
            {
                thirdOperationCalled = true;
                await Task.Delay(1);
                return Result<int, string>.Ok(x + 10);
            });

        // Assert
        Assert.False(thirdOperationCalled);
        Assert.True(result.IsFailure);
        Assert.True(result.TryGetError(out var error));
        Assert.Equal("Failed", error);
    }

    #endregion

    #region Argument Validation

    [Fact]
    public async Task MapAsync_ThrowsOnNullTask()
    {
        // Arrange
        Task<Result<int, string>>? resultTask = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await resultTask!.MapAsync(x => x * 2));
    }

    [Fact]
    public async Task MapAsync_ThrowsOnNullMapper()
    {
        // Arrange
        var resultTask = Task.FromResult(Result<int, string>.Ok(42));

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await resultTask.MapAsync((Func<int, int>)null!));
    }

    [Fact]
    public async Task BindAsync_ThrowsOnNullTask()
    {
        // Arrange
        Task<Result<int, string>>? resultTask = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await resultTask!.BindAsync(Result<int, string>.Ok));
    }

    #endregion
}
