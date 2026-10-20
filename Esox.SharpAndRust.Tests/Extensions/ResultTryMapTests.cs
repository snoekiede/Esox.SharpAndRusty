using Esox.SharpAndRusty.Extensions;
using Esox.SharpAndRusty.Types;

namespace Esox.SharpAndRusty.Tests.Extensions;

public class ResultTryMapTests
{
    [Fact]
    public void TryMap_OnSuccess_MapsValueAndDoesNotConvertException()
    {
        var mapperCalls = 0;
        var exceptionMapperCalls = 0;

        var result = TryMap(Result<int, string>.Ok(21),
            value =>
            {
                mapperCalls++;
                return value * 2;
            },
            _ =>
            {
                exceptionMapperCalls++;
                return "unexpected";
            });

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.True(result.TryGetValue(out var value));
        Assert.Equal(42, value);
        Assert.False(result.TryGetError(out _));
        Assert.Equal(1, mapperCalls);
        Assert.Equal(0, exceptionMapperCalls);
    }

    [Fact]
    public void TryMap_OnFailure_PropagatesErrorWithoutInvokingMappers()
    {
        var mapperCalls = 0;
        var exceptionMapperCalls = 0;

        var result = TryMap(Result<int, string>.Err("original error"),
            value =>
            {
                mapperCalls++;
                return value * 2;
            },
            _ =>
            {
                exceptionMapperCalls++;
                return "mapped exception";
            });

        Assert.True(result.IsFailure);
        Assert.False(result.IsSuccess);
        Assert.True(result.TryGetError(out var error));
        Assert.Equal("original error", error);
        Assert.False(result.TryGetValue(out _));
        Assert.Equal(0, mapperCalls);
        Assert.Equal(0, exceptionMapperCalls);
    }

    [Fact]
    public void TryMap_WhenMapperThrows_ConvertsExceptionToError()
    {
        var exception = new InvalidOperationException("mapper failed");
        Exception? convertedException = null;

        var result = TryMap<int, string, int>(Result<int, string>.Ok(1),
            _ => throw exception,
            ex =>
            {
                convertedException = ex;
                return ex.Message;
            });

        Assert.True(result.IsFailure);
        Assert.True(result.TryGetError(out var error));
        Assert.Equal("mapper failed", error);
        Assert.Same(exception, convertedException);
    }

    [Fact]
    public void TryMap_WhenMapperReturnsNull_PreservesNullSuccessValue()
    {
        var result = TryMap<int, string, string?>(Result<int, string>.Ok(1), _ => null, _ => "unexpected");

        Assert.True(result.IsSuccess);
        Assert.True(result.TryGetValue(out var value));
        Assert.Null(value);
        Assert.False(result.TryGetError(out _));
    }

    [Fact]
    public void TryMap_WithNullMapper_ThrowsArgumentNullException()
    {
        var result = Result<int, string>.Ok(1);

        var exception = Assert.Throws<ArgumentNullException>(() =>
            TryMap<int, string, int>(result, null!, _ => "mapped"));

        Assert.Equal("mapper", exception.ParamName);
    }

    [Fact]
    public void TryMap_WithNullExceptionMapper_ThrowsArgumentNullException()
    {
        var result = Result<int, string>.Ok(1);

        var exception = Assert.Throws<ArgumentNullException>(() =>
            TryMap<int, string, int>(result, _ => 2, null!));

        Assert.Equal("exceptionToErrorMapper", exception.ParamName);
    }

    [Fact]
    public void TryMap_WhenExceptionMapperThrows_PropagatesException()
    {
        var exception = new InvalidOperationException("mapper failed");

        var thrown = Assert.Throws<FormatException>(() =>
            TryMap<int, string, int>(Result<int, string>.Ok(1),
                _ => throw exception,
                _ => throw new FormatException("exception conversion failed")));

        Assert.Equal("exception conversion failed", thrown.Message);
    }

    private static Result<U, E> TryMap<T, E, U>(
        Result<T, E> result,
        Func<T, U> mapper,
        Func<Exception, E> exceptionToErrorMapper) =>
        ResultExtensions.TryMap<T, E, U>(result, mapper, exceptionToErrorMapper);
}
