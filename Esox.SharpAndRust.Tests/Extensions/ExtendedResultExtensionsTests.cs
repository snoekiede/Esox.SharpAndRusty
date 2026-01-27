using Esox.SharpAndRusty.Extensions;
using Esox.SharpAndRusty.Types;

namespace Esox.SharpAndRust.Tests.Extensions;

public class ExtendedResultExtensionsTests
{
    [Fact]
    public void Map_TransformsOnSuccess_PropagatesError()
    {
        var ok = ExtendedResult<int, string>.Ok(3).Map(x => x * 2);
        Assert.True(ok.TryGetValue(out var v));
        Assert.Equal(6, v);

        var err = ExtendedResult<int, string>.Err("e").Map(x => x * 2);
        Assert.True(err.TryGetError(out var e));
        Assert.Equal("e", e);
    }

    [Fact]
    public void Bind_ChainsAndPropagatesErrors()
    {
        var ok = ExtendedResult<int, string>.Ok(5)
            .Bind(x => ExtendedResult<int, string>.Ok(x + 1))
            .Bind(x => ExtendedResult<string, string>.Ok($"v{x}"));
        Assert.True(ok.TryGetValue(out var v));
        Assert.Equal("v6", v);

        var err1 = ExtendedResult<int, string>.Err("boom")
            .Bind(_ => ExtendedResult<int, string>.Ok(1));
        Assert.True(err1.TryGetError(out var e1));
        Assert.Equal("boom", e1);

        var err2 = ExtendedResult<int, string>.Ok(1)
            .Bind(_ => ExtendedResult<int, string>.Err("x"));
        Assert.True(err2.TryGetError(out var e2));
        Assert.Equal("x", e2);
    }

    [Fact]
    public void Unwrap_And_Expect_Work()
    {
        var ok = ExtendedResult<int, string>.Ok(10);
        Assert.Equal(10, ok.Unwrap());

        var err = ExtendedResult<int, string>.Err("bad");
        var ex = Assert.Throws<InvalidOperationException>(() => err.Unwrap());
        Assert.Contains("Cannot unwrap a failure", ex.Message);

        var ex2 = Assert.Throws<InvalidOperationException>(() => err.Expect("Need value"));
        Assert.Contains("Need value: bad", ex2.Message);
    }

    [Fact]
    public void MapError_TransformsErrorOnly()
    {
        var err = ExtendedResult<int, string>.Err("oops").MapError(e => e.Length);
        Assert.True(err.TryGetError(out var len));
        Assert.Equal(4, len);

        var ok = ExtendedResult<int, string>.Ok(2).MapError(e => e.Length);
        Assert.True(ok.TryGetValue(out var v));
        Assert.Equal(2, v);
    }

    [Fact]
    public void Tap_InvokesBothSides_AndReturnsOriginal()
    {
        var okRan = false;
        var errRan = false;

        var ok = ExtendedResult<int, string>.Ok(3)
            .Tap(_ => okRan = true, _ => errRan = true);

        Assert.True(okRan);
        Assert.False(errRan);
        Assert.True(ok.TryGetValue(out _));

        okRan = false; errRan = false;
        var err = ExtendedResult<int, string>.Err("e")
            .Tap(_ => okRan = true, _ => errRan = true);

        Assert.False(okRan);
        Assert.True(errRan);
        Assert.True(err.TryGetError(out _));
    }

    [Fact]
    public void Select_ProjectsValue()
    {
        var r = ExtendedResult<int, string>.Ok(4).Select(x => x * 3);
        Assert.True(r.TryGetValue(out var v));
        Assert.Equal(12, v);
    }

    [Fact]
    public void SelectMany_Binds()
    {
        var r = ExtendedResult<int, string>.Ok(2)
            .SelectMany(x => ExtendedResult<int, string>.Ok(x + 5));
        Assert.True(r.TryGetValue(out var v));
        Assert.Equal(7, v);
    }

    [Fact]
    public void SelectMany_WithProjector_ComposesValues()
    {
        var r = ExtendedResult<int, string>.Ok(2)
            .SelectMany(x => ExtendedResult<int, string>.Ok(x + 3), (x, y) => x * y);
        Assert.True(r.TryGetValue(out var v));
        Assert.Equal(10, v);
    }

    [Fact]
    public void Combine_AggregatesOrReturnsFirstError()
    {
        var allOk = new[]
        {
            ExtendedResult<int, string>.Ok(1),
            ExtendedResult<int, string>.Ok(2),
            ExtendedResult<int, string>.Ok(3),
        }.Combine();

        Assert.True(allOk.TryGetValue(out var values));
        Assert.Equal(new[] {1,2,3}, values);

        var withErr = new[]
        {
            ExtendedResult<int, string>.Ok(1),
            ExtendedResult<int, string>.Err("bad"),
            ExtendedResult<int, string>.Ok(3),
        }.Combine();

        Assert.True(withErr.TryGetError(out var e));
        Assert.Equal("bad", e);
    }

    [Fact]
    public void Partition_SplitsIntoSuccessesAndFailures()
    {
        var items = new[]
        {
            ExtendedResult<int, string>.Ok(1),
            ExtendedResult<int, string>.Err("a"),
            ExtendedResult<int, string>.Ok(2),
            ExtendedResult<int, string>.Err("b"),
        };

        var (successes, failures) = items.Partition();

        Assert.Equal(new[] {1,2}, successes);
        Assert.Equal(new[] {"a","b"}, failures);
    }

    [Fact]
    public void Linq_Query_Composes()
    {
        var r = from x in ExtendedResult<int, string>.Ok(10)
                from y in ExtendedResult<int, string>.Ok(20)
                select x + y;

        Assert.True(r.TryGetValue(out var v));
        Assert.Equal(30, v);
    }

    // Edge cases with null values/errors in extensions

    [Fact]
    public void Map_WithNullSuccessValue_PropagatesNull()
    {
        var r = ExtendedResult<string?, string>.Ok(null).Map(s => s?.ToUpper());
        Assert.True(r.TryGetValue(out var v));
        Assert.Null(v);
    }

    [Fact]
    public void Bind_WithNullSuccessValue_AllowsTransition()
    {
        var r = ExtendedResult<string?, string>.Ok(null)
            .Bind(s => s is null
                ? ExtendedResult<int, string>.Ok(0)
                : ExtendedResult<int, string>.Ok(s.Length));
        Assert.True(r.TryGetValue(out var v));
        Assert.Equal(0, v);
    }

    [Fact]
    public void MapError_WithNullError_PreservesNull()
    {
        var r = ExtendedResult<int, string?>.Err(null).MapError(e => e == null ? -1 : e.Length);
        Assert.True(r.TryGetError(out var errVal));
        Assert.Equal(-1, errVal);
    }

    [Fact]
    public void Combine_AllowsNullSuccessValues()
    {
        var combined = new[]
        {
            ExtendedResult<string?, string>.Ok(null),
            ExtendedResult<string?, string>.Ok("x"),
        }.Combine();

        Assert.True(combined.TryGetValue(out var values));
        Assert.Equal(new[] { (string?)null, "x" }, values);
    }

    [Fact]
    public void Partition_AllowsNullErrors()
    {
        var items = new[]
        {
            ExtendedResult<string, string?>.Err(null),
            ExtendedResult<string, string?>.Err("a"),
            ExtendedResult<string, string?>.Ok("x"),
        };

        var (successes, failures) = items.Partition();

        Assert.Equal(new[] { "x" }, successes);
        Assert.Equal(new[] { (string?)null, "a" }, failures);
    }
}
