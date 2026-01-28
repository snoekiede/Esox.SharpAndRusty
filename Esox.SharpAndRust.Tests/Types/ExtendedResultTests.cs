using Esox.SharpAndRusty.Types;
using Esox.SharpAndRusty.Extensions;

namespace Esox.SharpAndRust.Tests.Types;

public class ExtendedResultTests
{
    [Fact]
    public void Ok_TryGetValue_ReturnsTrueAndValue()
    {
        var result = ExtendedResult<int, string>.Ok(42);

        var success = result.TryGetValue(out var value);

        Assert.True(success);
        Assert.Equal(42, value);
        Assert.False(result.TryGetError(out _));
    }

    [Fact]
    public void Err_TryGetError_ReturnsTrueAndError()
    {
        var result = ExtendedResult<int, string>.Err("boom");

        var failure = result.TryGetError(out var error);

        Assert.True(failure);
        Assert.Equal("boom", error);
        Assert.False(result.TryGetValue(out _));
    }

    [Fact]
    public void IsSuccess_ReturnsTrueForOk()
    {
        var result = ExtendedResult<int, string>.Ok(42);

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
    }

    [Fact]
    public void IsSuccess_ReturnsFalseForErr()
    {
        var result = ExtendedResult<int, string>.Err("error");

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void IsFailure_ReturnsTrueForErr()
    {
        var result = ExtendedResult<int, string>.Err("failure");

        Assert.True(result.IsFailure);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void IsFailure_ReturnsFalseForOk()
    {
        var result = ExtendedResult<int, string>.Ok(100);

        Assert.False(result.IsFailure);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void UnwrapOr_ReturnsDefaultOnFailure()
    {
        var result = ExtendedResult<int, string>.Err("e");
        var value = result.UnwrapOr(99);
        Assert.Equal(99, value);
    }

    [Fact]
    public void UnwrapOrElse_ComputesDefaultFromError()
    {
        var result = ExtendedResult<int, string>.Err("error");
        var value = result.UnwrapOrElse(e => e.Length);
        Assert.Equal(5, value);
    }

    [Fact]
    public void Inspect_RunsOnSuccess_AndReturnsSameSemanticInstance()
    {
        var result = ExtendedResult<int, string>.Ok(5);
        var observed = 0;

        var returned = result.Inspect(v => observed = v);

        Assert.Equal(5, observed);
        // Returned result is the same semantic state (still success with same value)
        Assert.True(returned.TryGetValue(out var value));
        Assert.Equal(5, value);
    }

    [Fact]
    public void Inspect_DoesNotRunOnFailure()
    {
        var result = ExtendedResult<int, string>.Err("err");
        var ran = false;

        var returned = result.Inspect(_ => ran = true);

        Assert.False(ran);
        Assert.True(returned.TryGetError(out _));
    }

    [Fact]
    public void InspectErr_RunsOnFailure()
    {
        var result = ExtendedResult<int, string>.Err("oops");
        string? seen = null;

        var returned = result.InspectErr(e => seen = e);

        Assert.Equal("oops", seen);
        Assert.True(returned.TryGetError(out _));
    }

    [Fact]
    public async Task TryAsync_Success()
    {
        async Task<int> Op()
        {
            await Task.Delay(1);
            return 7;
        }

        var result = await ExtendedResult<int, string>.TryAsync(Op, ex => ex.Message);

        Assert.True(result.TryGetValue(out var value));
        Assert.Equal(7, value);
    }

    [Fact]
    public async Task TryAsync_Failure()
    {
        async Task<int> Op()
        {
            await Task.Delay(1);
            throw new InvalidOperationException("nope");
        }

        var result = await ExtendedResult<int, string>.TryAsync(Op, ex => ex.Message);

        Assert.True(result.TryGetError(out var error));
        Assert.Contains("nope", error);
    }

    [Fact]
    public void Try_Success()
    {
        var result = ExtendedResult<int, string>.Try(() => 10, ex => ex.Message);
        Assert.True(result.TryGetValue(out var value));
        Assert.Equal(10, value);
    }

    [Fact]
    public void Try_Failure()
    {
        var result = ExtendedResult<int, string>.Try(() => throw new InvalidOperationException("x"), ex => ex.Message);
        Assert.True(result.TryGetError(out var error));
        Assert.Contains("x", error);
    }

    // Edge cases with null values/errors

    [Fact]
    public void Ok_WithNullValue_Works()
    {
        ExtendedResult<string?, string> result = ExtendedResult<string?, string>.Ok(null);

        Assert.True(result.TryGetValue(out var value));
        Assert.Null(value);

        // Unwrap returns null for success
        var unwrapped = result.Unwrap();
        Assert.Null(unwrapped);

        // Expect returns null on success
        var expected = result.Expect("ignored message");
        Assert.Null(expected);

        // UnwrapOr still returns success value (null), not default
        var or = result.UnwrapOr("fallback");
        Assert.Null(or);

        // Inspect gets called with null
        string? seen = "init";
        var returned = result.Inspect(v => seen = v);
        Assert.Null(seen);
        Assert.True(returned.TryGetValue(out _));
    }

    [Fact]
    public void Err_WithNullError_Works()
    {
        ExtendedResult<int, string?> result = ExtendedResult<int, string?>.Err(null);

        Assert.True(result.TryGetError(out var error));
        Assert.Null(error);
        Assert.False(result.TryGetValue(out _));

        // Unwrap throws
        Assert.Throws<InvalidOperationException>(() => result.Unwrap());

        // Expect throws with message prefix present
        var ex = Assert.Throws<InvalidOperationException>(() => result.Expect("Need value"));
        Assert.StartsWith("Need value:", ex.Message);

        // UnwrapOrElse factory receives null
        var computed = result.UnwrapOrElse(e => e?.Length ?? -1);
        Assert.Equal(-1, computed);

        // InspectErr gets called with null
        string? seen = "init";
        var returned = result.InspectErr(e => seen = e);
        Assert.Null(seen);
        Assert.True(returned.TryGetError(out _));
    }

    [Fact]
    public void Map_TransformsValue_OnSuccess()
    {
        var result = ExtendedResult<int, string>.Ok(5);
        var mapped = result.Map(x => x * 2);
        Assert.True(mapped.TryGetValue(out var value));
        Assert.Equal(10, value);
    }

    [Fact]
    public void Map_PreservesError_OnFailure()
    {
        var result = ExtendedResult<int, string>.Err("err");
        var mapped = result.Map(x => x * 2);
        Assert.True(mapped.TryGetError(out var error));
        Assert.Equal("err", error);
    }

    [Fact]
    public void Bind_Chains_OnSuccess()
    {
        var result = ExtendedResult<int, string>.Ok(3);
        var bound = result.Bind(x => ExtendedResult<int, string>.Ok(x + 4));
        Assert.True(bound.TryGetValue(out var value));
        Assert.Equal(7, value);
    }

    [Fact]
    public void Bind_Skips_OnFailure()
    {
        var result = ExtendedResult<int, string>.Err("boom");
        var bound = result.Bind(x => ExtendedResult<int, string>.Ok(x + 4));
        Assert.True(bound.TryGetError(out var error));
        Assert.Equal("boom", error);
    }

    [Fact]
    public void MapError_TransformsError_OnFailure()
    {
        var result = ExtendedResult<int, string>.Err("oops");
        var mapped = result.MapError(e => e.Length);
        Assert.True(mapped.TryGetError(out var error));
        Assert.Equal(4, error);
    }

    [Fact]
    public void MapError_PreservesValue_OnSuccess()
    {
        var result = ExtendedResult<int, string>.Ok(9);
        var mapped = result.MapError(e => e.Length);
        Assert.True(mapped.TryGetValue(out var value));
        Assert.Equal(9, value);
    }

    [Fact]
    public void Tap_InvokesBothBranches_Correctly()
    {
        // success path
        var s = ExtendedResult<int, string>.Ok(2);
        int seenValue = 0; string? seenError = null;
        var sReturned = s.Tap(val => seenValue = val, err => seenError = err);
        Assert.Equal(2, seenValue);
        Assert.Null(seenError);
        Assert.True(sReturned.TryGetValue(out var v));
        Assert.Equal(2, v);

        // failure path
        var f = ExtendedResult<int, string>.Err("E");
        seenValue = 0; seenError = null;
        var fReturned = f.Tap(val => seenValue = val, err => seenError = err);
        Assert.Equal(0, seenValue);
        Assert.Equal("E", seenError);
        Assert.True(fReturned.TryGetError(out var e2));
        Assert.Equal("E", e2);
    }

    [Fact]
    public void Linq_Select_ProjectsValue()
    {
        var query = from x in ExtendedResult<int, string>.Ok(10)
                    select x * 3;
        Assert.True(query.TryGetValue(out var value));
        Assert.Equal(30, value);
    }

    [Fact]
    public void Linq_SelectMany_ChainsTwo_Ok()
    {
        var query = from x in ExtendedResult<int, string>.Ok(10)
                    from y in ExtendedResult<int, string>.Ok(5)
                    select x + y;
        Assert.True(query.TryGetValue(out var value));
        Assert.Equal(15, value);
    }

    [Fact]
    public void Linq_SelectMany_PropagatesError()
    {
        var query = from x in ExtendedResult<int, string>.Ok(10)
                    from y in ExtendedResult<int, string>.Err("bad")
                    select x + y;
        Assert.True(query.TryGetError(out var error));
        Assert.Equal("bad", error);
    }

    [Fact]
    public void Combine_AllSuccess_ReturnsList()
    {
        var items = new[]
        {
            ExtendedResult<int, string>.Ok(1),
            ExtendedResult<int, string>.Ok(2),
            ExtendedResult<int, string>.Ok(3),
        };
        var combined = items.Combine();
        Assert.True(combined.TryGetValue(out var values));
        Assert.Equal(new[] {1,2,3}, values);
    }

    [Fact]
    public void Combine_FirstError_ReturnsError()
    {
        var items = new[]
        {
            ExtendedResult<int, string>.Ok(1),
            ExtendedResult<int, string>.Err("err2"),
            ExtendedResult<int, string>.Ok(3),
        };
        var combined = items.Combine();
        Assert.True(combined.TryGetError(out var error));
        Assert.Equal("err2", error);
    }

    [Fact]
    public void Partition_SplitsSuccessesAndFailures()
    {
        var items = new[]
        {
            ExtendedResult<int, string>.Ok(1),
            ExtendedResult<int, string>.Err("e1"),
            ExtendedResult<int, string>.Ok(2),
            ExtendedResult<int, string>.Err("e2"),
        };
        var (successes, failures) = items.Partition();
        Assert.Equal(new[] {1,2}, successes);
        Assert.Equal(new[] {"e1","e2"}, failures);
    }
}
