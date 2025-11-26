using Esox.SharpAndRusty.Types;
using Esox.SharpAndRusty.Extensions;

namespace Esox.SharpAndRust.Tests.Extensions;

public class ResultExtensionsTests
{
    [Fact]
    public void Map_TransformsSuccessValue()
    {
        // Arrange
        var result = Result<int, string>.Ok(5);

        // Act
        var mapped = result.Map<int, string, string>(x => $"Value: {x}");

        // Assert
        var output = mapped.Match(
            success: v => v,
            failure: e => "Error"
        );
        Assert.Equal("Value: 5", output);
        Assert.True(mapped.IsSuccess);
    }

    [Fact]
    public void Map_PropagatesError()
    {
        // Arrange
        var result = Result<int, string>.Err("Original error");

        // Act
        var mapped = result.Map<int, string, string>(x => $"Value: {x}");

        // Assert
        var error = mapped.Match(
            success: v => "Success",
            failure: e => e
        );
        Assert.Equal("Original error", error);
        Assert.True(mapped.IsFailure);
    }

    [Fact]
    public void Map_CanChangeSuccessType()
    {
        // Arrange
        var result = Result<string, int>.Ok("42");

        // Act
        var mapped = result.Map<string, int, int>(x => int.Parse(x));

        // Assert
        var value = mapped.Match(
            success: v => v,
            failure: e => 0
        );
        Assert.Equal(42, value);
        Assert.True(mapped.IsSuccess);
    }

    [Fact]
    public void Map_DoesNotInvokeMapperOnError()
    {
        // Arrange
        var result = Result<int, string>.Err("Error");
        var mapperInvoked = false;

        // Act
        var mapped = result.Map<int, string, int>(x =>
        {
            mapperInvoked = true;
            return x * 2;
        });

        // Assert
        Assert.False(mapperInvoked);
        Assert.True(mapped.IsFailure);
    }

    [Fact]
    public void Bind_ChainsSuccessfulOperations()
    {
        // Arrange
        var result = Result<int, string>.Ok(5);

        // Act
        var bound = result.Bind(x => Result<string, string>.Ok($"Number: {x}"));

        // Assert
        var output = bound.Match(
            success: v => v,
            failure: e => "Error"
        );
        Assert.Equal("Number: 5", output);
        Assert.True(bound.IsSuccess);
    }

    [Fact]
    public void Bind_PropagatesOriginalError()
    {
        // Arrange
        var result = Result<int, string>.Err("Original error");

        // Act
        var bound = result.Bind(x => Result<string, string>.Ok($"Number: {x}"));

        // Assert
        var error = bound.Match(
            success: v => "Success",
            failure: e => e
        );
        Assert.Equal("Original error", error);
        Assert.True(bound.IsFailure);
    }

    [Fact]
    public void Bind_PropagatesBinderError()
    {
        // Arrange
        var result = Result<int, string>.Ok(5);

        // Act
        var bound = result.Bind(x => Result<string, string>.Err("Binder error"));

        // Assert
        var error = bound.Match(
            success: v => "Success",
            failure: e => e
        );
        Assert.Equal("Binder error", error);
        Assert.True(bound.IsFailure);
    }

    [Fact]
    public void Bind_DoesNotInvokeBinderOnError()
    {
        // Arrange
        var result = Result<int, string>.Err("Error");
        var binderInvoked = false;

        // Act
        var bound = result.Bind(x =>
        {
            binderInvoked = true;
            return Result<string, string>.Ok($"Value: {x}");
        });

        // Assert
        Assert.False(binderInvoked);
        Assert.True(bound.IsFailure);
    }

    [Fact]
    public void Bind_AllowsChaining()
    {
        // Arrange
        var result = Result<int, string>.Ok(5);

        // Act
        var final = result
            .Bind(x => Result<int, string>.Ok(x * 2))
            .Bind(x => Result<int, string>.Ok(x + 10))
            .Bind(x => Result<string, string>.Ok($"Final: {x}"));

        // Assert
        var output = final.Match(
            success: v => v,
            failure: e => "Error"
        );
        Assert.Equal("Final: 20", output);
        Assert.True(final.IsSuccess);
    }

    [Fact]
    public void Bind_StopsOnFirstError()
    {
        // Arrange
        var result = Result<int, string>.Ok(5);
        var thirdBinderInvoked = false;

        // Act
        var final = result
            .Bind(x => Result<int, string>.Ok(x * 2))
            .Bind(x => Result<int, string>.Err("Second binder failed"))
            .Bind(x =>
            {
                thirdBinderInvoked = true;
                return Result<string, string>.Ok($"Final: {x}");
            });

        // Assert
        Assert.False(thirdBinderInvoked);
        Assert.True(final.IsFailure);
    }

    [Fact]
    public void Unwrap_ReturnsValueFromSuccessfulResult()
    {
        // Arrange
        var result = Result<int, string>.Ok(42);

        // Act
        var value = result.Unwrap();

        // Assert
        Assert.Equal(42, value);
    }

    [Fact]
    public void Unwrap_ThrowsExceptionForFailedResult()
    {
        // Arrange
        var result = Result<int, string>.Err("Error occurred");

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => result.Unwrap());
        Assert.Contains("Cannot unwrap a failure result", exception.Message);
        Assert.Contains("Error occurred", exception.Message);
    }

    [Fact]
    public void Unwrap_ReturnsNullValueFromSuccessfulResult()
    {
        // Arrange
        var result = Result<string?, int>.Ok(null);

        // Act
        var value = result.Unwrap();

        // Assert
        Assert.Null(value);
    }

    [Fact]
    public void Map_CanBeChainedWithBind()
    {
        // Arrange
        var result = Result<int, string>.Ok(5);

        // Act
        var final = result
            .Map<int, string, int>(x => x * 2)
            .Bind(x => Result<string, string>.Ok($"Value: {x}"));

        // Assert
        var output = final.Match(
            success: v => v,
            failure: e => "Error"
        );
        Assert.Equal("Value: 10", output);
        Assert.True(final.IsSuccess);
    }

    [Fact]
    public void ComplexChaining_WorksCorrectly()
    {
        // Arrange
        var result = Result<string, string>.Ok("10");

        // Act
        var final = result
            .Map<string, string, int>(x => int.Parse(x))
            .Bind(x => x > 5 ? Result<int, string>.Ok(x) : Result<int, string>.Err("Too small"))
            .Map<int, string, string>(x => $"Valid number: {x}")
            .Bind(x => Result<string, string>.Ok(x.ToUpper()));

        // Assert
        var output = final.Match(
            success: v => v,
            failure: e => "Error"
        );
        Assert.Equal("VALID NUMBER: 10", output);
        Assert.True(final.IsSuccess);
    }
}
