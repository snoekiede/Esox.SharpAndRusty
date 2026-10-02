using Esox.SharpAndRusty.Extensions;
using Esox.SharpAndRusty.Types;

namespace Esox.SharpAndRusty.Tests.Extensions;

public class EnsureExtensionsTests
{
    private static Result<int, string> Ok(int value) => Result<int, string>.Ok(value);
    private static Result<int, string> Err(string error) => Result<int, string>.Err(error);

    #region Result<T, E>.Ensure

    [Fact]
    public void Ensure_PredicateHolds_ReturnsOriginalResult()
    {
        var result = Ok(5).Ensure(x => x > 0, x => $"{x} must be positive");

        Assert.Equal(Ok(5), result);
    }

    [Fact]
    public void Ensure_PredicateFails_ReturnsErrorFromFactory()
    {
        var result = Ok(-1).Ensure(x => x > 0, x => $"{x} must be positive");

        Assert.Equal(Err("-1 must be positive"), result);
    }

    [Fact]
    public void Ensure_FixedError_PredicateFails_ReturnsThatError()
    {
        var result = Ok(-1).Ensure(x => x > 0, "must be positive");

        Assert.Equal(Err("must be positive"), result);
    }

    [Fact]
    public void Ensure_FixedError_PredicateHolds_ReturnsOriginalResult()
    {
        var result = Ok(1).Ensure(x => x > 0, "must be positive");

        Assert.Equal(Ok(1), result);
    }

    [Fact]
    public void Ensure_OnFailure_ReturnsOriginalErrorWithoutCallingPredicate()
    {
        var predicateCalls = 0;
        var factoryCalls = 0;

        var result = Err("original").Ensure(
            x =>
            {
                predicateCalls++;
                return true;
            },
            x =>
            {
                factoryCalls++;
                return "unused";
            });

        Assert.Equal(Err("original"), result);
        Assert.Equal(0, predicateCalls);
        Assert.Equal(0, factoryCalls);
    }

    [Fact]
    public void Ensure_PredicateHolds_DoesNotCallErrorFactory()
    {
        var factoryCalls = 0;

        Ok(5).Ensure(x => x > 0, x =>
        {
            factoryCalls++;
            return "unused";
        });

        Assert.Equal(0, factoryCalls);
    }

    [Fact]
    public void Ensure_Chained_StopsAtFirstFailingCheck()
    {
        var thirdCheckCalls = 0;

        var result = Ok(150)
            .Ensure(x => x > 0, "must be positive")
            .Ensure(x => x < 100, "must be below 100")
            .Ensure(x =>
            {
                thirdCheckCalls++;
                return x != 150;
            }, "must not be 150");

        Assert.Equal(Err("must be below 100"), result);
        Assert.Equal(0, thirdCheckCalls);
    }

    [Fact]
    public void Ensure_ComposesWithBindMapAndLinq()
    {
        var result = from a in Ok(10)
                     from b in Ok(20).Ensure(x => x > 5, "too small")
                     select a + b;

        Assert.Equal(Ok(30), result);
    }

    [Fact]
    public void Ensure_NullPredicate_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => Ok(1).Ensure(null!, x => "error"));
        Assert.Throws<ArgumentNullException>(() => Ok(1).Ensure(null!, "error"));
    }

    [Fact]
    public void Ensure_NullErrorFactory_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => Ok(1).Ensure(x => true, (Func<int, string>)null!));
    }

    [Fact]
    public void Ensure_PredicateThrows_ExceptionPropagates()
    {
        Assert.Throws<InvalidOperationException>(() =>
            Ok(1).Ensure(x => throw new InvalidOperationException("boom"), x => "error"));
    }

    #endregion

    #region ExtendedResult<T, E>.Ensure

    [Fact]
    public void ExtendedResult_Ensure_PredicateHolds_ReturnsOriginalResult()
    {
        var original = ExtendedResult<int, string>.Ok(5);

        var result = original.Ensure(x => x > 0, "must be positive");

        Assert.Equal(original, result);
    }

    [Fact]
    public void ExtendedResult_Ensure_PredicateFails_ReturnsErrorFromFactory()
    {
        var result = ExtendedResult<int, string>.Ok(-1).Ensure(x => x > 0, x => $"{x} must be positive");

        Assert.Equal(ExtendedResult<int, string>.Err("-1 must be positive"), result);
    }

    [Fact]
    public void ExtendedResult_Ensure_OnFailure_ReturnsOriginalWithoutCallingPredicate()
    {
        var predicateCalls = 0;
        var original = ExtendedResult<int, string>.Err("original");

        var result = original.Ensure(x =>
        {
            predicateCalls++;
            return true;
        }, "unused");

        Assert.Equal(original, result);
        Assert.Equal(0, predicateCalls);
    }

    [Fact]
    public void ExtendedResult_Ensure_NullArguments_Throw()
    {
        var result = ExtendedResult<int, string>.Ok(1);

        Assert.Throws<ArgumentNullException>(() => result.Ensure(null!, x => "error"));
        Assert.Throws<ArgumentNullException>(() => result.Ensure(x => true, (Func<int, string>)null!));
    }

    #endregion

    #region Task<Result<T, E>>.EnsureAsync

    [Fact]
    public async Task EnsureAsync_PredicateHolds_ReturnsOriginalResult()
    {
        var result = await Task.FromResult(Ok(5)).EnsureAsync(x => x > 0, x => $"{x} must be positive");

        Assert.Equal(Ok(5), result);
    }

    [Fact]
    public async Task EnsureAsync_PredicateFails_ReturnsErrorFromFactory()
    {
        var result = await Task.FromResult(Ok(-1)).EnsureAsync(x => x > 0, x => $"{x} must be positive");

        Assert.Equal(Err("-1 must be positive"), result);
    }

    [Fact]
    public async Task EnsureAsync_AsyncPredicateHolds_ReturnsOriginalResult()
    {
        var result = await Task.FromResult(Ok(5)).EnsureAsync(async x =>
        {
            await Task.Yield();
            return x > 0;
        }, x => $"{x} must be positive");

        Assert.Equal(Ok(5), result);
    }

    [Fact]
    public async Task EnsureAsync_AsyncPredicateFails_ReturnsErrorFromFactory()
    {
        var result = await Task.FromResult(Ok(-1)).EnsureAsync(async x =>
        {
            await Task.Yield();
            return x > 0;
        }, x => $"{x} must be positive");

        Assert.Equal(Err("-1 must be positive"), result);
    }

    [Fact]
    public async Task EnsureAsync_OnFailure_DoesNotCallPredicate()
    {
        var predicateCalls = 0;

        var result = await Task.FromResult(Err("original")).EnsureAsync(async x =>
        {
            predicateCalls++;
            await Task.Yield();
            return true;
        }, x => "unused");

        Assert.Equal(Err("original"), result);
        Assert.Equal(0, predicateCalls);
    }

    [Fact]
    public async Task EnsureAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            Task.FromResult(Ok(1)).EnsureAsync(x => true, x => "error", cts.Token));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            Task.FromResult(Ok(1)).EnsureAsync(async x =>
            {
                await Task.Yield();
                return true;
            }, x => "error", cts.Token));
    }

    [Fact]
    public async Task EnsureAsync_NullArguments_Throw()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            ((Task<Result<int, string>>)null!).EnsureAsync(x => true, x => "error"));

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            Task.FromResult(Ok(1)).EnsureAsync((Func<int, bool>)null!, x => "error"));

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            Task.FromResult(Ok(1)).EnsureAsync(x => true, (Func<int, string>)null!));
    }

    #endregion
}
