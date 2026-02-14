using Esox.SharpAndRusty.Types;
using Esox.SharpAndRusty.Extensions;

namespace Esox.SharpAndRust.Tests.Extensions;

public class AsyncCollectionExtensionsTests
{
    #region Option SequenceAsync Tests

    [Fact]
    public async Task SequenceAsync_Option_AllSome_ReturnsSomeWithAllValues()
    {
        // Arrange
        var tasks = new[]
        {
            Task.FromResult<Option<int>>(new Option<int>.Some(1)),
            Task.FromResult<Option<int>>(new Option<int>.Some(2)),
            Task.FromResult<Option<int>>(new Option<int>.Some(3))
        };

        // Act
        var result = await tasks.SequenceAsync();

        // Assert
        Assert.True(result.IsSome());
        if (result is Option<IEnumerable<int>>.Some some)
        {
            Assert.Equal(new[] { 1, 2, 3 }, some.Value);
        }
    }

    [Fact]
    public async Task SequenceAsync_Option_WithNone_ReturnsNone()
    {
        // Arrange
        var tasks = new[]
        {
            Task.FromResult<Option<int>>(new Option<int>.Some(1)),
            Task.FromResult<Option<int>>(new Option<int>.None()),
            Task.FromResult<Option<int>>(new Option<int>.Some(3))
        };

        // Act
        var result = await tasks.SequenceAsync();

        // Assert
        Assert.True(result.IsNone());
    }

    [Fact]
    public async Task SequenceAsync_Option_WithCancellation_ThrowsOperationCanceledException()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var tasks = new[]
        {
            Task.FromResult<Option<int>>(new Option<int>.Some(1)),
            Task.FromResult<Option<int>>(new Option<int>.Some(2))
        };

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            async () => await tasks.SequenceAsync(cts.Token));
    }

    #endregion

    #region Option TraverseAsync Tests

    [Fact]
    public async Task TraverseAsync_Option_AllSucceed_ReturnsSomeWithAllValues()
    {
        // Arrange
        var numbers = new[] { 1, 2, 3 };

        // Act
        var result = await numbers.TraverseAsync<int, int>(async n =>
        {
            await Task.Delay(10);
            return new Option<int>.Some(n * 2);
        });

        // Assert
        Assert.True(result.IsSome());
        if (result is Option<IEnumerable<int>>.Some some)
        {
            Assert.Equal(new[] { 2, 4, 6 }, some.Value);
        }
    }

    [Fact]
    public async Task TraverseAsync_Option_WithFailure_ReturnsNone()
    {
        // Arrange
        var numbers = new[] { 1, 2, 3, 4, 5 };

        // Act
        var result = await numbers.TraverseAsync<int, int>(async n =>
        {
            await Task.Delay(10);
            return n == 3
                ? new Option<int>.None()
                : new Option<int>.Some(n);
        });

        // Assert
        Assert.True(result.IsNone());
    }

    [Fact]
    public async Task TraverseAsync_Option_ShortCircuits_OnFirstNone()
    {
        // Arrange
        var numbers = new[] { 1, 2, 3, 4, 5 };
        var callCount = 0;

        // Act
        var result = await numbers.TraverseAsync<int, int>(async n =>
        {
            Interlocked.Increment(ref callCount);
            await Task.Delay(10);
            return n == 3
                ? new Option<int>.None()
                : new Option<int>.Some(n);
        });

        // Assert
        Assert.True(result.IsNone());
        Assert.Equal(3, callCount); // Should stop at 3
    }

    #endregion

    #region Option TraverseParallelAsync Tests

    [Fact]
    public async Task TraverseParallelAsync_Option_AllSucceed_ReturnsSomeWithAllValues()
    {
        // Arrange
        var numbers = Enumerable.Range(1, 10).ToArray();

        // Act
        var result = await numbers.TraverseParallelAsync<int, int>(
            async n =>
            {
                await Task.Delay(10);
                return new Option<int>.Some(n * 2);
            },
            maxDegreeOfParallelism: 5);

        // Assert
        Assert.True(result.IsSome());
        if (result is Option<IEnumerable<int>>.Some some)
        {
            Assert.Equal(numbers.Select(n => n * 2), some.Value);
        }
    }

    [Fact]
    public async Task TraverseParallelAsync_Option_WithFailure_ReturnsNone()
    {
        // Arrange
        var numbers = Enumerable.Range(1, 10).ToArray();

        // Act
        var result = await numbers.TraverseParallelAsync<int, int>(
            async n =>
            {
                await Task.Delay(10);
                return n == 5
                    ? new Option<int>.None()
                    : new Option<int>.Some(n);
            },
            maxDegreeOfParallelism: 5);

        // Assert
        Assert.True(result.IsNone());
    }

    [Fact]
    public async Task TraverseParallelAsync_Option_PreservesOrder()
    {
        // Arrange
        var numbers = Enumerable.Range(1, 20).ToArray();

        // Act
        var result = await numbers.TraverseParallelAsync<int, int>(
            async n =>
            {
                // Add random delay to ensure order is maintained
                await Task.Delay(Random.Shared.Next(1, 20));
                return new Option<int>.Some(n);
            },
            maxDegreeOfParallelism: 10);

        // Assert
        Assert.True(result.IsSome());
        if (result is Option<IEnumerable<int>>.Some some)
        {
            Assert.Equal(numbers, some.Value);
        }
    }

    #endregion

    #region Option CollectSomeAsync Tests

    [Fact]
    public async Task CollectSomeAsync_MixedOptions_ReturnsOnlySomeValues()
    {
        // Arrange
        var tasks = new[]
        {
            Task.FromResult<Option<int>>(new Option<int>.Some(1)),
            Task.FromResult<Option<int>>(new Option<int>.None()),
            Task.FromResult<Option<int>>(new Option<int>.Some(3)),
            Task.FromResult<Option<int>>(new Option<int>.None()),
            Task.FromResult<Option<int>>(new Option<int>.Some(5))
        };

        // Act
        var result = await tasks.CollectSomeAsync();

        // Assert
        Assert.Equal(new[] { 1, 3, 5 }, result);
    }

    [Fact]
    public async Task CollectSomeAsync_AllNone_ReturnsEmptyCollection()
    {
        // Arrange
        var tasks = new[]
        {
            Task.FromResult<Option<int>>(new Option<int>.None()),
            Task.FromResult<Option<int>>(new Option<int>.None())
        };

        // Act
        var result = await tasks.CollectSomeAsync();

        // Assert
        Assert.Empty(result);
    }

    #endregion

    #region Result SequenceAsync Tests

    [Fact]
    public async Task SequenceAsync_Result_AllOk_ReturnsOkWithAllValues()
    {
        // Arrange
        var tasks = new[]
        {
            Task.FromResult(Result<int, string>.Ok(1)),
            Task.FromResult(Result<int, string>.Ok(2)),
            Task.FromResult(Result<int, string>.Ok(3))
        };

        // Act
        var result = await tasks.SequenceAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.TryGetValue(out var values));
        Assert.Equal(new[] { 1, 2, 3 }, values);
    }

    [Fact]
    public async Task SequenceAsync_Result_WithError_ReturnsFirstError()
    {
        // Arrange
        var tasks = new[]
        {
            Task.FromResult(Result<int, string>.Ok(1)),
            Task.FromResult(Result<int, string>.Err("error1")),
            Task.FromResult(Result<int, string>.Err("error2"))
        };

        // Act
        var result = await tasks.SequenceAsync();

        // Assert
        Assert.True(result.IsFailure);
        Assert.True(result.TryGetError(out var error));
        Assert.Equal("error1", error);
    }

    #endregion

    #region Result TraverseAsync Tests

    [Fact]
    public async Task TraverseAsync_Result_AllSucceed_ReturnsOkWithAllValues()
    {
        // Arrange
        var strings = new[] { "1", "2", "3" };

        // Act
        var result = await strings.TraverseAsync<string, int, string>(async s =>
        {
            await Task.Delay(10);
            return int.TryParse(s, out var n)
                ? Result<int, string>.Ok(n)
                : Result<int, string>.Err($"Invalid: {s}");
        });

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.TryGetValue(out var values));
        Assert.Equal(new[] { 1, 2, 3 }, values);
    }

    [Fact]
    public async Task TraverseAsync_Result_WithFailure_ReturnsFirstError()
    {
        // Arrange
        var strings = new[] { "1", "invalid", "3" };

        // Act
        var result = await strings.TraverseAsync<string, int, string>(async s =>
        {
            await Task.Delay(10);
            return int.TryParse(s, out var n)
                ? Result<int, string>.Ok(n)
                : Result<int, string>.Err($"Invalid: {s}");
        });

        // Assert
        Assert.True(result.IsFailure);
        Assert.True(result.TryGetError(out var error));
        Assert.Equal("Invalid: invalid", error);
    }

    [Fact]
    public async Task TraverseAsync_Result_ShortCircuits_OnFirstError()
    {
        // Arrange
        var numbers = new[] { 1, 2, 3, 4, 5 };
        var callCount = 0;

        // Act
        var result = await numbers.TraverseAsync<int, int, string>(async n =>
        {
            Interlocked.Increment(ref callCount);
            await Task.Delay(10);
            return n == 3
                ? Result<int, string>.Err("error at 3")
                : Result<int, string>.Ok(n);
        });

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(3, callCount); // Should stop at 3
    }

    #endregion

    #region Result TraverseParallelAsync Tests

    [Fact]
    public async Task TraverseParallelAsync_Result_AllSucceed_ReturnsOkWithAllValues()
    {
        // Arrange
        var numbers = Enumerable.Range(1, 10).ToArray();

        // Act
        var result = await numbers.TraverseParallelAsync<int, int, string>(
            async n =>
            {
                await Task.Delay(10);
                return Result<int, string>.Ok(n * 2);
            },
            maxDegreeOfParallelism: 5);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.TryGetValue(out var values));
        Assert.Equal(numbers.Select(n => n * 2), values);
    }

    [Fact]
    public async Task TraverseParallelAsync_Result_WithFailure_ReturnsError()
    {
        // Arrange
        var numbers = Enumerable.Range(1, 10).ToArray();

        // Act
        var result = await numbers.TraverseParallelAsync<int, int, string>(
            async n =>
            {
                await Task.Delay(10);
                return n == 5
                    ? Result<int, string>.Err("error at 5")
                    : Result<int, string>.Ok(n);
            },
            maxDegreeOfParallelism: 5);

        // Assert
        Assert.True(result.IsFailure);
    }

    #endregion

    #region Result CollectOkAsync Tests

    [Fact]
    public async Task CollectOkAsync_MixedResults_ReturnsOnlyOkValues()
    {
        // Arrange
        var tasks = new[]
        {
            Task.FromResult(Result<int, string>.Ok(1)),
            Task.FromResult(Result<int, string>.Err("error1")),
            Task.FromResult(Result<int, string>.Ok(3)),
            Task.FromResult(Result<int, string>.Err("error2")),
            Task.FromResult(Result<int, string>.Ok(5))
        };

        // Act
        var result = await tasks.CollectOkAsync();

        // Assert
        Assert.Equal(new[] { 1, 3, 5 }, result);
    }

    [Fact]
    public async Task CollectOkAsync_AllErr_ReturnsEmptyCollection()
    {
        // Arrange
        var tasks = new[]
        {
            Task.FromResult(Result<int, string>.Err("error1")),
            Task.FromResult(Result<int, string>.Err("error2"))
        };

        // Act
        var result = await tasks.CollectOkAsync();

        // Assert
        Assert.Empty(result);
    }

    #endregion

    #region Result CollectErrAsync Tests

    [Fact]
    public async Task CollectErrAsync_MixedResults_ReturnsOnlyErrors()
    {
        // Arrange
        var tasks = new[]
        {
            Task.FromResult(Result<int, string>.Ok(1)),
            Task.FromResult(Result<int, string>.Err("error1")),
            Task.FromResult(Result<int, string>.Ok(3)),
            Task.FromResult(Result<int, string>.Err("error2"))
        };

        // Act
        var result = await tasks.CollectErrAsync();

        // Assert
        Assert.Equal(new[] { "error1", "error2" }, result);
    }

    [Fact]
    public async Task CollectErrAsync_AllOk_ReturnsEmptyCollection()
    {
        // Arrange
        var tasks = new[]
        {
            Task.FromResult(Result<int, string>.Ok(1)),
            Task.FromResult(Result<int, string>.Ok(2))
        };

        // Act
        var result = await tasks.CollectErrAsync();

        // Assert
        Assert.Empty(result);
    }

    #endregion

    #region Result PartitionResultsAsync Tests

    [Fact]
    public async Task PartitionResultsAsync_MixedResults_PartitionsCorrectly()
    {
        // Arrange
        var tasks = new[]
        {
            Task.FromResult(Result<int, string>.Ok(1)),
            Task.FromResult(Result<int, string>.Err("error1")),
            Task.FromResult(Result<int, string>.Ok(3)),
            Task.FromResult(Result<int, string>.Err("error2")),
            Task.FromResult(Result<int, string>.Ok(5))
        };

        // Act
        var (successes, failures) = await tasks.PartitionResultsAsync();

        // Assert
        Assert.Equal(new[] { 1, 3, 5 }, successes);
        Assert.Equal(new[] { "error1", "error2" }, failures);
    }

    [Fact]
    public async Task PartitionResultsAsync_AllOk_ReturnsAllSuccessesNoFailures()
    {
        // Arrange
        var tasks = new[]
        {
            Task.FromResult(Result<int, string>.Ok(1)),
            Task.FromResult(Result<int, string>.Ok(2)),
            Task.FromResult(Result<int, string>.Ok(3))
        };

        // Act
        var (successes, failures) = await tasks.PartitionResultsAsync();

        // Assert
        Assert.Equal(new[] { 1, 2, 3 }, successes);
        Assert.Empty(failures);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task Integration_Option_AsyncUserLookup_WorksCorrectly()
    {
        // Arrange
        var userIds = new[] { 1, 2, 3, 4, 5 };

        async Task<Option<string>> GetUserAsync(int id)
        {
            await Task.Delay(10);
            return id != 3
                ? new Option<string>.Some($"User{id}")
                : new Option<string>.None();
        }

        // Act
        var result = await userIds.TraverseAsync(GetUserAsync);

        // Assert - Should fail because user 3 doesn't exist
        Assert.True(result.IsNone());
    }

    [Fact]
    public async Task Integration_Result_AsyncBatchProcessing_WorksCorrectly()
    {
        // Arrange
        var items = Enumerable.Range(1, 20).ToArray();

        async Task<Result<int, string>> ProcessAsync(int item)
        {
            await Task.Delay(Random.Shared.Next(1, 20));
            return item % 10 == 0
                ? Result<int, string>.Err($"Cannot process {item}")
                : Result<int, string>.Ok(item * 2);
        }

        // Act
        var result = await items.TraverseParallelAsync<int, int, string>(
            ProcessAsync,
            maxDegreeOfParallelism: 5);

        // Assert - Should fail on 10 or 20
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task Integration_Result_BestEffortProcessing_CollectsSuccesses()
    {
        // Arrange
        var items = Enumerable.Range(1, 10).ToArray();

        async Task<Result<int, string>> ProcessAsync(int item)
        {
            await Task.Delay(10);
            return item % 3 == 0
                ? Result<int, string>.Err($"Error at {item}")
                : Result<int, string>.Ok(item * 2);
        }

        // Act
        var tasks = items.Select(ProcessAsync);
        var (successes, failures) = await tasks.PartitionResultsAsync();

        // Assert
        Assert.Equal(7, successes.Count); // 1,2,4,5,7,8,10 succeed
        Assert.Equal(3, failures.Count);  // 3,6,9 fail
    }

    [Fact]
    public async Task Integration_Option_ParallelApiCalls_WorksCorrectly()
    {
        // Arrange
        var urls = Enumerable.Range(1, 50).Select(i => $"https://api.example.com/item/{i}").ToArray();

        async Task<Option<string>> FetchAsync(string url)
        {
            await Task.Delay(Random.Shared.Next(5, 20));
            // Simulate some failures
            return url.Contains("25") || url.Contains("40")
                ? new Option<string>.None()
                : new Option<string>.Some($"Data from {url}");
        }

        // Act
        var result = await urls.TraverseParallelAsync<string, string>(
            FetchAsync,
            maxDegreeOfParallelism: 10);

        // Assert - Should fail because some URLs return None
        Assert.True(result.IsNone());

        // Act - Best effort collection
        var tasks = urls.Select(FetchAsync);
        var available = await tasks.CollectSomeAsync();

        // Assert - Should get partial results
        Assert.Equal(48, available.Count()); // 50 - 2 failures
    }

    #endregion
}
