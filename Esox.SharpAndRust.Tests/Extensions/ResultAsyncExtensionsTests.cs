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

    #region CancellationToken Tests

    [Fact]
    public async Task MapAsync_TaskResult_RespectsCancellation()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var resultTask = Task.Run(async () =>
        {
            await Task.Delay(100, cts.Token);
            return Result<int, string>.Ok(42);
        }, cts.Token);
        await cts.CancelAsync();

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(async () =>
            await resultTask.MapAsync(x => x * 2, cts.Token));
    }

    [Fact]
    public async Task MapAsync_AsyncMapper_RespectsCancellation()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var result = Result<int, string>.Ok(42);
        
        // Act & Assert
        await cts.CancelAsync();
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await result.MapAsync(async x =>
            {
                await Task.Delay(100, cts.Token);
                return x * 2;
            }, cts.Token));
    }

    [Fact]
    public async Task MapAsync_AsyncMapper_RespectsCancellationAfterMapper()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var result = Result<int, string>.Ok(42);
        
        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await result.MapAsync(async x =>
            {
                await Task.Delay(10, cts.Token);
                await cts.CancelAsync();
                return x * 2;
            }, cts.Token));
    }

    [Fact]
    public async Task BindAsync_TaskResult_RespectsCancellation()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var resultTask = Task.Run(async () =>
        {
            await Task.Delay(100, cts.Token);
            return Result<int, string>.Ok(42);
        }, cts.Token);
        await cts.CancelAsync();

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(async () =>
            await resultTask.BindAsync(x => Result<int, string>.Ok(x * 2), cts.Token));
    }

    [Fact]
    public async Task BindAsync_AsyncBinder_RespectsCancellation()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var result = Result<int, string>.Ok(42);
        
        // Act & Assert
        await cts.CancelAsync();
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await result.BindAsync(async x =>
            {
                await Task.Delay(100, cts.Token);
                return Result<int, string>.Ok(x * 2);
            }, cts.Token));
    }

    [Fact]
    public async Task BindAsync_AsyncBinder_RespectsCancellationAfterBinder()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var result = Result<int, string>.Ok(42);
        
        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await result.BindAsync(async x =>
            {
                await Task.Delay(10, cts.Token);
                await cts.CancelAsync();
                return Result<int, string>.Ok(x * 2);
            }, cts.Token));
    }

    [Fact]
    public async Task MapErrorAsync_RespectsCancellation()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var resultTask = Task.Run(async () =>
        {
            await Task.Delay(100, cts.Token);
            return Result<int, string>.Err("error");
        }, cts.Token);
        await cts.CancelAsync();

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(async () =>
            await resultTask.MapErrorAsync(msg => msg.Length, cts.Token));
    }

    [Fact]
    public async Task TapAsync_RespectsCancellation()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var resultTask = Task.Run(async () =>
        {
            await Task.Delay(100, cts.Token);
            return Result<int, string>.Ok(42);
        }, cts.Token);
        await cts.CancelAsync();

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(async () =>
            await resultTask.TapAsync(
                onSuccess: async _ => await Task.Delay(10, cts.Token),
                onFailure: async _ => await Task.Delay(10, cts.Token),
                cts.Token));
    }

    [Fact]
    public async Task TapAsync_RespectsCancellationAfterSuccessAction()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var resultTask = Task.FromResult(Result<int, string>.Ok(42));

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await resultTask.TapAsync(
                onSuccess: async _ =>
                {
                    await Task.Delay(10, cts.Token);
                    await cts.CancelAsync();
                },
                onFailure: async _ => await Task.Delay(10, cts.Token),
                cts.Token));
    }

    [Fact]
    public async Task TapAsync_RespectsCancellationAfterFailureAction()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var resultTask = Task.FromResult(Result<int, string>.Err("error"));

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await resultTask.TapAsync(
                onSuccess: async _ => await Task.Delay(10, cts.Token),
                onFailure: async _ =>
                {
                    await Task.Delay(10, cts.Token);
                    await cts.CancelAsync();
                },
                cts.Token));
    }

    [Fact]
    public async Task OrElseAsync_RespectsCancellation()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var resultTask = Task.Run(async () =>
        {
            await Task.Delay(100, cts.Token);
            return Result<int, string>.Err("error");
        }, cts.Token);
        await cts.CancelAsync();

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(async () =>
            await resultTask.OrElseAsync(
                async _ => await Task.FromResult(Result<int, string>.Ok(99)),
                cts.Token));
    }

    [Fact]
    public async Task OrElseAsync_RespectsCancellationAfterAlternative()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var resultTask = Task.FromResult(Result<int, string>.Err("error"));

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await resultTask.OrElseAsync(
                async _ =>
                {
                    await Task.Delay(10, cts.Token);
                    await cts.CancelAsync();
                    return Result<int, string>.Ok(99);
                },
                cts.Token));
    }

    [Fact]
    public async Task CombineAsync_RespectsCancellation()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var resultTasks = new []
        {
            Task.Run(async () =>
            {
                await Task.Delay(100, cts.Token);
                return Result<int, string>.Ok(1);
            }, cts.Token),
            Task.Run(async () =>
            {
                await Task.Delay(100, cts.Token);
                return Result<int, string>.Ok(2);
            }, cts.Token)
        };
        await cts.CancelAsync();

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(async () =>
            await resultTasks.CombineAsync(cts.Token));
    }

    [Fact]
    public async Task CancellationToken_DoesNotAffectNormalOperation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var result = Result<int, string>.Ok(42);

        // Act
        var mapped = await result.MapAsync(async x =>
        {
            await Task.Delay(10, cts.Token);
            return x * 2;
        }, cts.Token);

        // Assert
        Assert.True(mapped.IsSuccess);
        Assert.Equal(84, mapped.UnwrapOr(0));
    }

    #endregion

    #region Async Edge Cases

    [Fact]
    public async Task CombineAsync_WithEmptyCollection_ReturnsEmptySuccess()
    {
        // Arrange
        var resultTasks = Array.Empty<Task<Result<int, string>>>();

        // Act
        var combined = await resultTasks.CombineAsync();

        // Assert
        Assert.True(combined.IsSuccess);
        combined.TryGetValue(out var values);
        Assert.Empty(values!);
    }

    [Fact]
    public async Task CombineAsync_WithNullCollection_ThrowsArgumentNullException()
    {
        // Arrange
        IEnumerable<Task<Result<int, string>>>? resultTasks = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await resultTasks!.CombineAsync());
    }

    [Fact]
    public async Task CombineAsync_WithMixedDelays_PreservesOrder()
    {
        // Arrange
        async Task<Result<int, string>> GetValueAsync(int value, int delayMs)
        {
            await Task.Delay(delayMs);
            return Result<int, string>.Ok(value);
        }

        var resultTasks = new[]
        {
            GetValueAsync(1, 100), // Slowest
            GetValueAsync(2, 10),  // Fastest
            GetValueAsync(3, 50)   // Middle
        };

        // Act
        var combined = await resultTasks.CombineAsync();

        // Assert
        Assert.True(combined.IsSuccess);
        combined.TryGetValue(out var values);
        Assert.Equal([1, 2, 3], values); // Order preserved despite different completion times
    }

    [Fact]
    public async Task CombineAsync_WithErrorInMiddle_ReturnsFirstError()
    {
        // Arrange
        async Task<Result<int, string>> GetValueAsync(int value, bool fail = false)
        {
            await Task.Delay(10);
            return fail ? Result<int, string>.Err($"Error {value}") : Result<int, string>.Ok(value);
        }

        var resultTasks = new[]
        {
            GetValueAsync(1, false),
            GetValueAsync(2, true),  // First error
            GetValueAsync(3, true),  // Second error
            GetValueAsync(4, false)
        };

        // Act
        var combined = await resultTasks.CombineAsync();

        // Assert
        Assert.True(combined.IsFailure);
        combined.TryGetError(out var error);
        Assert.Equal("Error 2", error); // First error wins
    }

    [Fact]
    public async Task MapAsync_WithExceptionInMapper_PropagatesException()
    {
        // Arrange
        var resultTask = Task.FromResult(Result<int, string>.Ok(42));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await resultTask.MapAsync<int, string, int>(_ =>
            {
                throw new InvalidOperationException("Mapper failed");
            }));
    }

    [Fact]
    public async Task MapAsync_AsyncMapper_WithExceptionInMapper_PropagatesException()
    {
        // Arrange
        var result = Result<int, string>.Ok(42);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await result.MapAsync<int, string, int>(async _ =>
            {
                await Task.Delay(10);
                throw new InvalidOperationException("Async mapper failed");
                #pragma warning disable CS0162 // Unreachable code detected
                return 0;
                #pragma warning restore CS0162
            }));
    }

    [Fact]
    public async Task BindAsync_WithExceptionInBinder_PropagatesException()
    {
        // Arrange
        var resultTask = Task.FromResult(Result<int, string>.Ok(42));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await resultTask.BindAsync(async _ =>
            {
                await Task.CompletedTask;
                throw new InvalidOperationException("Binder failed");
                #pragma warning disable CS0162 // Unreachable code detected
                return Result<int, string>.Ok(0);
                #pragma warning restore CS0162
            }));
    }

    [Fact]
    public async Task BindAsync_AsyncBinder_WithExceptionInBinder_PropagatesException()
    {
        // Arrange
        var result = Result<int, string>.Ok(42);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await result.BindAsync<int, string, int>(async _ =>
            {
                await Task.Delay(10);
                throw new InvalidOperationException("Async binder failed");
                #pragma warning disable CS0162 // Unreachable code detected
                return Result<int, string>.Ok(0);
                #pragma warning restore CS0162
            }));
    }

    [Fact]
    public async Task TapAsync_WithExceptionInSuccessAction_PropagatesException()
    {
        // Arrange
        var resultTask = Task.FromResult(Result<int, string>.Ok(42));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await resultTask.TapAsync(
                onSuccess: async _ =>
                {
                    await Task.Delay(10);
                    throw new InvalidOperationException("Success action failed");
                },
                onFailure: async _ => await Task.Delay(10)));
    }

    [Fact]
    public async Task TapAsync_WithExceptionInFailureAction_PropagatesException()
    {
        // Arrange
        var resultTask = Task.FromResult(Result<int, string>.Err("error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await resultTask.TapAsync(
                onSuccess: async _ => await Task.Delay(10),
                onFailure: async _ =>
                {
                    await Task.Delay(10);
                    throw new InvalidOperationException("Failure action failed");
                }));
    }

    [Fact]
    public async Task OrElseAsync_WithExceptionInAlternative_PropagatesException()
    {
        // Arrange
        var resultTask = Task.FromResult(Result<int, string>.Err("error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await resultTask.OrElseAsync(async _ =>
            {
                await Task.Delay(10);
                throw new InvalidOperationException("Alternative failed");
            }));
    }

    [Fact]
    public async Task MapErrorAsync_WithExceptionInMapper_PropagatesException()
    {
        // Arrange
        var resultTask = Task.FromResult(Result<int, string>.Err("error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await resultTask.MapErrorAsync<int, string, int>(_ =>
            {
                throw new InvalidOperationException("Error mapper failed");
            }));
    }

    [Fact]
    public async Task ComplexAsyncChain_WithMultipleTransformations_MaintainsCorrectState()
    {
        // Arrange
        async Task<Result<int, string>> GetValueAsync(int value)
        {
            await Task.Delay(10);
            return Result<int, string>.Ok(value);
        }

        var state = new List<string>();

        // Act - chain operations step by step
        var result1 = await GetValueAsync(10);
        state.Add($"Initial: {result1.UnwrapOr(0)}");

        var result2 = await result1.MapAsync(async x =>
        {
            await Task.Delay(5);
            state.Add($"Mapped: {x}");
            return x * 2;
        });

        state.Add($"After map: {result2.UnwrapOr(0)}");

        var result3 = await result2.BindAsync(async x =>
        {
            await Task.Delay(5);
            state.Add($"Bound: {x}");
            return Result<int, string>.Ok(x + 10);
        });

        // Assert
        Assert.True(result3.IsSuccess);
        Assert.Equal(30, result3.UnwrapOr(0));
        Assert.Equal(4, state.Count);
        Assert.Equal("Initial: 10", state[0]);
        Assert.Equal("Mapped: 10", state[1]);
        Assert.Equal("After map: 20", state[2]);
        Assert.Equal("Bound: 20", state[3]);
    }

    [Fact]
    public async Task CombineAsync_WithLargeCollection_CompletesSuccessfully()
    {
        // Arrange
        async Task<Result<int, string>> GetValueAsync(int value)
        {
            await Task.Delay(1);
            return Result<int, string>.Ok(value);
        }

        var resultTasks = Enumerable.Range(0, 100).Select(GetValueAsync);

        // Act
        var combined = await resultTasks.CombineAsync();

        // Assert
        Assert.True(combined.IsSuccess);
        combined.TryGetValue(out var values);
        Assert.Equal(100, values!.Count());
        Assert.Equal(Enumerable.Range(0, 100), values);
    }

    [Fact]
    public async Task AsyncChain_WithIntermittentDelays_CompletesCorrectly()
    {
        // Arrange
        async Task<Result<int, string>> SlowOperationAsync(int value)
        {
            await Task.Delay(50);
            return Result<int, string>.Ok(value);
        }

        async Task<Result<int, string>> FastOperationAsync(int value)
        {
            await Task.Delay(1);
            return Result<int, string>.Ok(value);
        }

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var result = await SlowOperationAsync(10)
            .BindAsync(FastOperationAsync)
            .BindAsync(SlowOperationAsync)
            .BindAsync(FastOperationAsync);

        stopwatch.Stop();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(10, result.UnwrapOr(0));
        Assert.True(stopwatch.ElapsedMilliseconds >= 100, "Should take at least 100ms (2 slow operations)");
    }

    [Fact]
    public async Task OrElseAsync_ChainedMultipleTimes_ReturnsFirstSuccess()
    {
        // Arrange
        async Task<Result<int, string>> FailAsync(string error)
        {
            await Task.Delay(10);
            return Result<int, string>.Err(error);
        }

        async Task<Result<int, string>> SucceedAsync(int value)
        {
            await Task.Delay(10);
            return Result<int, string>.Ok(value);
        }

        // Act
        var result = await FailAsync("Error 1")
            .OrElseAsync(async _ => await FailAsync("Error 2"))
            .OrElseAsync(async _ => await SucceedAsync(42))
            .OrElseAsync(async _ => await SucceedAsync(99)); // Should not be called

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.UnwrapOr(0));
    }

    [Fact]
    public async Task MapAsync_WithLongRunningMapper_CompletesSuccessfully()
    {
        // Arrange
        var result = Result<int, string>.Ok(42);

        // Act
        var mapped = await result.MapAsync(async x =>
        {
            await Task.Delay(100);
            var sum = 0;
            for (int i = 0; i < 1000; i++)
            {
                sum += x;
            }
            return sum;
        });

        // Assert
        Assert.True(mapped.IsSuccess);
        Assert.Equal(42000, mapped.UnwrapOr(0));
    }

    #endregion
}

