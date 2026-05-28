using Esox.SharpAndRusty.Extensions;
using Esox.SharpAndRusty.Types;

namespace Esox.SharpAndRust.Tests.Types;

public class ResultOptionAccessTests
{
    [Fact]
    public void ValueOption_OnSuccess_ReturnsSome()
    {
        var result = Result<int, string>.Ok(123);

        var valueOption = result.ValueOption();

        Assert.True(valueOption.IsSome());
        Assert.Equal(123, valueOption.GetValueOrDefault(0));
    }

    [Fact]
    public void ValueOption_OnFailure_ReturnsNone()
    {
        var result = Result<int, string>.Err("boom");

        var valueOption = result.ValueOption();

        Assert.True(valueOption.IsNone());
    }

    [Fact]
    public void ErrorOption_OnFailure_ReturnsSome()
    {
        var result = Result<int, string>.Err("boom");

        var errorOption = result.ErrorOption();

        Assert.True(errorOption.IsSome());
        Assert.Equal("boom", errorOption.GetValueOrDefault(""));
    }

    [Fact]
    public void ErrorOption_OnSuccess_ReturnsNone()
    {
        var result = Result<int, string>.Ok(123);

        var errorOption = result.ErrorOption();

        Assert.True(errorOption.IsNone());
    }
}

