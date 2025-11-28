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
    public void Match_ExecutesSuccessFunctionForSuccessfulResult()
    {
        // Arrange
        var result = Result<int, string>.Ok(42);
        var expectedValue = 84;

        // Act
        var actualValue = result.Match(
            success: value => value * 2,
            failure: _ => 0
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
            success: _ => "Success",
            failure: error => $"Error: {error}"
        );

        // Assert
        Assert.Equal(expectedMessage, actualMessage);
    }

    [Fact]
    public void Match_ReturnsCorrectSuccessValue()
    {
        // Arrange
        var result = Result<string, int>.Ok("Hello");

        // Act
        var value = result.Match(
            success: v => v,
            failure: _ => "Error"
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
            success: _ => 0,
            failure: e => e
        );

        // Assert
        Assert.Equal(404, errorCode);
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
            failure: _ => "Unknown"
        );

        var age = result.Match(
            success: p => p.Age.ToString(),
            failure: _ => "Unknown"
        );

        // Assert
        Assert.Equal("John", name);
        Assert.Equal("30", age);
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

    [Fact]
    public void TryGetValue_ReturnsValueForSuccessfulResult()
    {
        // Arrange
        var result = Result<int, string>.Ok(42);

        // Act
        var success = result.TryGetValue(out var value);

        // Assert
        Assert.True(success);
        Assert.Equal(42, value);
    }

    [Fact]
    public void TryGetValue_ReturnsFalseForFailedResult()
    {
        // Arrange
        var result = Result<int, string>.Err("Error");

        // Act
        var success = result.TryGetValue(out var value);

        // Assert
        Assert.False(success);
        Assert.Equal(0, value);
    }

    [Fact]
    public void TryGetError_ReturnsErrorForFailedResult()
    {
        // Arrange
        var result = Result<int, string>.Err("Error occurred");

        // Act
        var failure = result.TryGetError(out var error);

        // Assert
        Assert.True(failure);
        Assert.Equal("Error occurred", error);
    }

    [Fact]
    public void TryGetError_ReturnsFalseForSuccessfulResult()
    {
        // Arrange
        var result = Result<int, string>.Ok(42);

        // Act
        var failure = result.TryGetError(out var error);

        // Assert
        Assert.False(failure);
        Assert.Null(error);
    }

    [Fact]
    public void UnwrapOr_ReturnsValueForSuccessfulResult()
    {
        // Arrange
        var result = Result<int, string>.Ok(42);

        // Act
        var value = result.UnwrapOr(0);

        // Assert
        Assert.Equal(42, value);
    }

    [Fact]
    public void UnwrapOr_ReturnsDefaultForFailedResult()
    {
        // Arrange
        var result = Result<int, string>.Err("Error");

        // Act
        var value = result.UnwrapOr(99);

        // Assert
        Assert.Equal(99, value);
    }

    [Fact]
    public void UnwrapOrElse_ReturnsValueForSuccessfulResult()
    {
        // Arrange
        var result = Result<int, string>.Ok(42);

        // Act
        var value = result.UnwrapOrElse(error => error.Length);

        // Assert
        Assert.Equal(42, value);
    }

    [Fact]
    public void UnwrapOrElse_ComputesDefaultForFailedResult()
    {
        // Arrange
        var result = Result<int, string>.Err("Error");

        // Act
        var value = result.UnwrapOrElse(error => error.Length);

        // Assert
        Assert.Equal(5, value);
    }

    [Fact]
    public void OrElse_ReturnsOriginalForSuccessfulResult()
    {
        // Arrange
        var result = Result<int, string>.Ok(42);

        // Act
        var final = result.OrElse(_ => Result<int, string>.Ok(0));

        // Assert
        Assert.True(final.IsSuccess);
        Assert.Equal(42, final.UnwrapOr(0));
    }

    [Fact]
    public void OrElse_ReturnsAlternativeForFailedResult()
    {
        // Arrange
        var result = Result<int, string>.Err("Error");

        // Act
        var final = result.OrElse(_ => Result<int, string>.Ok(99));

        // Assert
        Assert.True(final.IsSuccess);
        Assert.Equal(99, final.UnwrapOr(0));
    }

    [Fact]
    public void Inspect_ExecutesActionForSuccessfulResult()
    {
        // Arrange
        var result = Result<int, string>.Ok(42);
        var inspected = 0;

        // Act
        var returned = result.Inspect(value => inspected = value);

        // Assert
        Assert.Equal(42, inspected);
        Assert.True(returned.IsSuccess);
    }

    [Fact]
    public void Inspect_DoesNotExecuteActionForFailedResult()
    {
        // Arrange
        var result = Result<int, string>.Err("Error");
        var executed = false;

        // Act
        var returned = result.Inspect(_ => executed = true);

        // Assert
        Assert.False(executed);
        Assert.True(returned.IsFailure);
    }

    [Fact]
    public void InspectErr_ExecutesActionForFailedResult()
    {
        // Arrange
        var result = Result<int, string>.Err("Error");
        var inspectedError = "";

        // Act
        var returned = result.InspectErr(error => inspectedError = error);

        // Assert
        Assert.Equal("Error", inspectedError);
        Assert.True(returned.IsFailure);
    }

    [Fact]
    public void InspectErr_DoesNotExecuteActionForSuccessfulResult()
    {
        // Arrange
        var result = Result<int, string>.Ok(42);
        var executed = false;

        // Act
        var returned = result.InspectErr(_ => executed = true);

        // Assert
        Assert.False(executed);
        Assert.True(returned.IsSuccess);
    }

    [Fact]
    public void Equals_ReturnsTrueForEqualSuccessResults()
    {
        // Arrange
        var result1 = Result<int, string>.Ok(42);
        var result2 = Result<int, string>.Ok(42);

        // Assert
        Assert.Equal(result1, result2);
        Assert.True(result1.Equals(result2));
        Assert.True(result1 == result2);
        Assert.False(result1 != result2);
    }

    [Fact]
    public void Equals_ReturnsFalseForDifferentSuccessResults()
    {
        // Arrange
        var result1 = Result<int, string>.Ok(42);
        var result2 = Result<int, string>.Ok(43);

        // Assert
        Assert.NotEqual(result1, result2);
        Assert.False(result1.Equals(result2));
        Assert.False(result1 == result2);
        Assert.True(result1 != result2);
    }

    [Fact]
    public void Equals_ReturnsTrueForEqualErrorResults()
    {
        // Arrange
        var result1 = Result<int, string>.Err("Error");
        var result2 = Result<int, string>.Err("Error");

        // Assert
        Assert.Equal(result1, result2);
        Assert.True(result1.Equals(result2));
    }

    [Fact]
    public void Equals_ReturnsFalseForSuccessAndError()
    {
        // Arrange
        var result1 = Result<int, string>.Ok(42);
        var result2 = Result<int, string>.Err("Error");

        // Assert
        Assert.NotEqual(result1, result2);
        Assert.False(result1.Equals(result2));
    }

    [Fact]
    public void GetHashCode_IsConsistentForEqualResults()
    {
        // Arrange
        var result1 = Result<int, string>.Ok(42);
        var result2 = Result<int, string>.Ok(42);

        // Assert
        Assert.Equal(result1.GetHashCode(), result2.GetHashCode());
    }

    [Fact]
    public void ToString_ReturnsFormattedStringForSuccess()
    {
        // Arrange
        var result = Result<int, string>.Ok(42);

        // Act
        var str = result.ToString();

        // Assert
        Assert.Equal("Ok(42)", str);
    }

    [Fact]
    public void ToString_ReturnsFormattedStringForError()
    {
        // Arrange
        var result = Result<int, string>.Err("Error");

        // Act
        var str = result.ToString();

        // Assert
        Assert.Equal("Err(Error)", str);
    }

    [Fact]
    public async Task TryAsync_ReturnsSuccessForSuccessfulOperation()
    {
        // Arrange
        async Task<int> Operation()
        {
            await Task.Delay(1);
            return 42;
        }

        // Act
        var result = await Result<int, string>.TryAsync(Operation, ex => ex.Message);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.UnwrapOr(0));
    }

    [Fact]
    public async Task TryAsync_ReturnsErrorForFailedOperation()
    {
        // Arrange
        async Task<int> Operation()
        {
            await Task.Delay(1);
            throw new InvalidOperationException("Test error");
        }

        // Act
        var result = await Result<int, string>.TryAsync(Operation, ex => ex.Message);

        // Assert
        Assert.True(result.IsFailure);
        Assert.True(result.TryGetError(out var error));
        Assert.Contains("Test error", error);
    }

    [Fact]
    public void Try_ReturnsSuccessForSuccessfulOperation()
    {
        // Arrange
        var operation = () => 42;

        // Act
        var result = Result<int, string>.Try(operation, ex => ex.Message);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.UnwrapOr(0));
    }

    [Fact]
    public void Try_ReturnsErrorForFailedOperation()
    {
        // Arrange
        var operation = () => { throw new InvalidOperationException("Test error"); return 42; };

        // Act
        var result = Result<int, string>.Try(operation, ex => ex.Message);

        // Assert
        Assert.True(result.IsFailure);
        Assert.True(result.TryGetError(out var error));
        Assert.Contains("Test error", error);
    }

    private class Person
    {
        public string Name { get; init; } = string.Empty;
        public int Age { get; init; }
    }
}
