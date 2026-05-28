using Esox.SharpAndRusty.Extensions;
using Esox.SharpAndRusty.Types;

namespace Esox.SharpAndRust.Tests.Extensions;

public class ParseExtensionsTests
{
    [Fact]
    public void TryParse_GenericInt_WhenValid_ReturnsOk()
    {
        Result<int, Error> result = "123".TryParse<int>();

        Assert.True(result.IsSuccess);
        Assert.True(result.TryGetValue(out var value));
        Assert.Equal(123, value);
    }

    [Fact]
    public void TryParse_GenericGuid_WhenValid_ReturnsOk()
    {
        var guid = Guid.NewGuid();

        var result = guid.ToString().TryParse<Guid>();

        Assert.True(result.IsSuccess);
        Assert.True(result.TryGetValue(out var value));
        Assert.Equal(guid, value);
    }

    [Fact]
    public void TryParse_GenericInt_WhenInvalid_ReturnsParseError()
    {
        Result<int, Error> result = "abc".TryParse<int>();

        Assert.True(result.IsFailure);
        Assert.True(result.TryGetError(out var error));
        Assert.Equal(ErrorKind.ParseError, error.Kind);
        Assert.Contains("abc", error.Message);
        Assert.Contains("Int32", error.Message);
    }

    [Fact]
    public void TryParse_GenericInt_WhenNull_ReturnsInvalidInputError()
    {
        string? input = null;

        var result = input.TryParse<int>();

        Assert.True(result.IsFailure);
        Assert.True(result.TryGetError(out var error));
        Assert.Equal(ErrorKind.InvalidInput, error.Kind);
        Assert.Equal("Input cannot be null.", error.Message);
    }

    [Fact]
    public void TryParse_DelegateOverload_WithIntTryParse_ReturnsOk()
    {
        Result<int, Error> result = "42".TryParse<int>(int.TryParse, nameof(int.TryParse));

        Assert.True(result.IsSuccess);
        Assert.True(result.TryGetValue(out var value));
        Assert.Equal(42, value);
    }

    [Fact]
    public void TryParse_DelegateOverload_WhenParserIsNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            "42".TryParse<int>((ParseExtensions.TryParseDelegate<int>)null!));
    }
}

