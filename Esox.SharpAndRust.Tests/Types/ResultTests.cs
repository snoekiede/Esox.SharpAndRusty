using Esox.SharpAndRusty.Types;

namespace Esox.SharpAndRust.Tests.Types;

public class ResultTests
{
    [Fact]
    public void Ok_CreatesSuccessfulResult()
    {
        // Arrange & Act
        var result = Result<int, string>.Ok(42);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
    }

    [Fact]
    public void Err_CreatesFailedResult()
    {
        // Arrange & Act
        var result = Result<int, string>.Err("Error occurred");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Match_ExecutesSuccessFunctionForSuccessfulResult()
    {
        // Arrange
        var result = Result<int, string>.Ok(42);
        var expectedValue = 84;

        // Act
        var actualValue = result.Match(
            success: value => value * 2,
            failure: error => 0
        );

        // Assert
        Assert.Equal(expectedValue, actualValue);
    }

    [Fact]
    public void Match_ExecutesFailureFunctionForFailedResult()
    {
        // Arrange
        var result = Result<int, string>.Err("Error occurred");
        var expectedMessage = "Error: Error occurred";

        // Act
        var actualMessage = result.Match(
            success: value => "Success",
            failure: error => $"Error: {error}"
        );

        // Assert
        Assert.Equal(expectedMessage, actualMessage);
    }

    [Fact]
    public void ImplicitConversion_FromValue_CreatesSuccessfulResult()
    {
        // Arrange & Act
        Result<int, string> result = 42;

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
    }

    [Fact]
    public void ImplicitConversion_FromError_CreatesFailedResult()
    {
        // Arrange & Act
        Result<int, string> result = "Error occurred";

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Match_ReturnsCorrectSuccessValue()
    {
        // Arrange
        var result = Result<string, int>.Ok("Hello");

        // Act
        var value = result.Match(
            success: v => v,
            failure: e => "Error"
        );

        // Assert
        Assert.Equal("Hello", value);
    }

    [Fact]
    public void Match_ReturnsCorrectErrorValue()
    {
        // Arrange
        var result = Result<string, int>.Err(404);

        // Act
        var errorCode = result.Match(
            success: v => 0,
            failure: e => e
        );

        // Assert
        Assert.Equal(404, errorCode);
    }

    [Fact]
    public void Ok_WithNullValue_CreatesSuccessfulResult()
    {
        // Arrange & Act
        var result = Result<string?, int>.Ok(null);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
    }

    [Fact]
    public void Err_WithNullError_CreatesFailedResult()
    {
        // Arrange & Act
        var result = Result<int, string?>.Err(null);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Match_WithComplexTypes_WorksCorrectly()
    {
        // Arrange
        var person = new Person { Name = "John", Age = 30 };
        var result = Result<Person, string>.Ok(person);

        // Act
        var name = result.Match(
            success: p => p.Name,
            failure: error => "Unknown"
        );

        // Assert
        Assert.Equal("John", name);
    }

    [Fact]
    public void IsFailure_IsOppositeOfIsSuccess_ForSuccessfulResult()
    {
        // Arrange
        var result = Result<int, string>.Ok(42);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(!result.IsSuccess, result.IsFailure);
    }

    [Fact]
    public void IsFailure_IsOppositeOfIsSuccess_ForFailedResult()
    {
        // Arrange
        var result = Result<int, string>.Err("Error");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(!result.IsSuccess, result.IsFailure);
    }

    [Fact]
    public void Match_CanReturnDifferentType_FromSuccessAndFailure()
    {
        // Arrange
        var successResult = Result<int, string>.Ok(42);
        var failureResult = Result<int, string>.Err("Error");

        // Act
        var successOutput = successResult.Match(
            success: value => $"Value: {value}",
            failure: error => $"Error: {error}"
        );

        var failureOutput = failureResult.Match(
            success: value => $"Value: {value}",
            failure: error => $"Error: {error}"
        );

        // Assert
        Assert.Equal("Value: 42", successOutput);
        Assert.Equal("Error: Error", failureOutput);
    }

    private class Person
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }
}
