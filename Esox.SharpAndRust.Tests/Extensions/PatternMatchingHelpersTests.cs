using Esox.SharpAndRusty.Types;
using Esox.SharpAndRusty.Extensions;

namespace Esox.SharpAndRust.Tests.Extensions;

public class PatternMatchingHelpersTests
{
    #region Option.IfSome Tests

    [Fact]
    public void IfSome_WithSome_ExecutesAction()
    {
        // Arrange
        var option = new Option<int>.Some(42);
        var executed = false;
        var capturedValue = 0;

        // Act
        var result = option.IfSome(value =>
        {
            executed = true;
            capturedValue = value;
        });

        // Assert
        Assert.True(executed);
        Assert.Equal(42, capturedValue);
        Assert.True(result.IsSome());
    }

    [Fact]
    public void IfSome_WithNone_DoesNotExecuteAction()
    {
        // Arrange
        var option = new Option<int>.None();
        var executed = false;

        // Act
        var result = option.IfSome(_ => executed = true);

        // Assert
        Assert.False(executed);
        Assert.True(result.IsNone());
    }

    [Fact]
    public void IfSome_WithNullAction_ThrowsArgumentNullException()
    {
        // Arrange
        var option = new Option<int>.Some(42);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            option.IfSome(null!));
    }

    #endregion

    #region Option.IfNone Tests

    [Fact]
    public void IfNone_WithNone_ExecutesAction()
    {
        // Arrange
        var option = new Option<int>.None();
        var executed = false;

        // Act
        var result = option.IfNone(() => executed = true);

        // Assert
        Assert.True(executed);
        Assert.True(result.IsNone());
    }

    [Fact]
    public void IfNone_WithSome_DoesNotExecuteAction()
    {
        // Arrange
        var option = new Option<int>.Some(42);
        var executed = false;

        // Act
        var result = option.IfNone(() => executed = true);

        // Assert
        Assert.False(executed);
        Assert.True(result.IsSome());
    }

    [Fact]
    public void IfNone_WithNullAction_ThrowsArgumentNullException()
    {
        // Arrange
        var option = new Option<int>.None();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            option.IfNone(null!));
    }

    #endregion

    #region Option.GetOrDefault Tests

    [Fact]
    public void GetOrDefault_WithSome_ReturnsValue()
    {
        // Arrange
        var option = new Option<int>.Some(42);

        // Act
        var result = option.GetOrDefault(0);

        // Assert
        Assert.Equal(42, result);
    }

    [Fact]
    public void GetOrDefault_WithNone_ReturnsDefault()
    {
        // Arrange
        var option = new Option<int>.None();

        // Act
        var result = option.GetOrDefault(99);

        // Assert
        Assert.Equal(99, result);
    }

    [Fact]
    public void GetOrDefault_WithReferenceType_WorksCorrectly()
    {
        // Arrange
        var someOption = new Option<string>.Some("Hello");
        var noneOption = new Option<string>.None();

        // Act
        var someResult = someOption.GetOrDefault("Default");
        var noneResult = noneOption.GetOrDefault("Default");

        // Assert
        Assert.Equal("Hello", someResult);
        Assert.Equal("Default", noneResult);
    }

    #endregion

    #region Option.GetOrElse Tests

    [Fact]
    public void GetOrElse_WithSome_ReturnsValue()
    {
        // Arrange
        var option = new Option<int>.Some(42);
        var factoryCalled = false;

        // Act
        var result = option.GetOrElse(() =>
        {
            factoryCalled = true;
            return 99;
        });

        // Assert
        Assert.Equal(42, result);
        Assert.False(factoryCalled);
    }

    [Fact]
    public void GetOrElse_WithNone_CallsFactory()
    {
        // Arrange
        var option = new Option<int>.None();
        var factoryCalled = false;

        // Act
        var result = option.GetOrElse(() =>
        {
            factoryCalled = true;
            return 99;
        });

        // Assert
        Assert.Equal(99, result);
        Assert.True(factoryCalled);
    }

    [Fact]
    public void GetOrElse_WithNullFactory_ThrowsArgumentNullException()
    {
        // Arrange
        var option = new Option<int>.None();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            option.GetOrElse(null!));
    }

    #endregion

    #region Option.GetOrThrow Tests

    [Fact]
    public void GetOrThrow_WithSome_ReturnsValue()
    {
        // Arrange
        var option = new Option<int>.Some(42);

        // Act
        var result = option.GetOrThrow(() => new InvalidOperationException());

        // Assert
        Assert.Equal(42, result);
    }

    [Fact]
    public void GetOrThrow_WithNone_ThrowsException()
    {
        // Arrange
        var option = new Option<int>.None();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            option.GetOrThrow(() => new InvalidOperationException("No value")));

        Assert.Equal("No value", exception.Message);
    }

    [Fact]
    public void GetOrThrow_WithNullFactory_ThrowsArgumentNullException()
    {
        // Arrange
        var option = new Option<int>.None();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            option.GetOrThrow(null!));
    }

    #endregion

    #region Result.OnSuccess Tests

    [Fact]
    public void OnSuccess_WithSuccess_ExecutesAction()
    {
        // Arrange
        var result = Result<int, string>.Ok(42);
        var executed = false;
        var capturedValue = 0;

        // Act
        var returnedResult = result.OnSuccess(value =>
        {
            executed = true;
            capturedValue = value;
        });

        // Assert
        Assert.True(executed);
        Assert.Equal(42, capturedValue);
        Assert.True(returnedResult.IsSuccess);
    }

    [Fact]
    public void OnSuccess_WithFailure_DoesNotExecuteAction()
    {
        // Arrange
        var result = Result<int, string>.Err("error");
        var executed = false;

        // Act
        var returnedResult = result.OnSuccess(_ => executed = true);

        // Assert
        Assert.False(executed);
        Assert.True(returnedResult.IsFailure);
    }

    [Fact]
    public void OnSuccess_WithNullAction_ThrowsArgumentNullException()
    {
        // Arrange
        var result = Result<int, string>.Ok(42);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            result.OnSuccess(null!));
    }

    #endregion

    #region Result.OnFailure Tests

    [Fact]
    public void OnFailure_WithFailure_ExecutesAction()
    {
        // Arrange
        var result = Result<int, string>.Err("error message");
        var executed = false;
        var capturedError = "";

        // Act
        var returnedResult = result.OnFailure(error =>
        {
            executed = true;
            capturedError = error;
        });

        // Assert
        Assert.True(executed);
        Assert.Equal("error message", capturedError);
        Assert.True(returnedResult.IsFailure);
    }

    [Fact]
    public void OnFailure_WithSuccess_DoesNotExecuteAction()
    {
        // Arrange
        var result = Result<int, string>.Ok(42);
        var executed = false;

        // Act
        var returnedResult = result.OnFailure(_ => executed = true);

        // Assert
        Assert.False(executed);
        Assert.True(returnedResult.IsSuccess);
    }

    [Fact]
    public void OnFailure_WithNullAction_ThrowsArgumentNullException()
    {
        // Arrange
        var result = Result<int, string>.Err("error");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            result.OnFailure(null!));
    }

    #endregion

    #region Result.Do Tests

    [Fact]
    public void Do_WithSuccess_ExecutesSuccessAction()
    {
        // Arrange
        var result = Result<int, string>.Ok(42);
        var successCalled = false;
        var failureCalled = false;

        // Act
        var returnedResult = result.Do(
            _ => successCalled = true,
            _ => failureCalled = true);

        // Assert
        Assert.True(successCalled);
        Assert.False(failureCalled);
        Assert.True(returnedResult.IsSuccess);
    }

    [Fact]
    public void Do_WithFailure_ExecutesFailureAction()
    {
        // Arrange
        var result = Result<int, string>.Err("error");
        var successCalled = false;
        var failureCalled = false;

        // Act
        var returnedResult = result.Do(
            _ => successCalled = true,
            _ => failureCalled = true);

        // Assert
        Assert.False(successCalled);
        Assert.True(failureCalled);
        Assert.True(returnedResult.IsFailure);
    }

    [Fact]
    public void Do_WithNullOnSuccess_ThrowsArgumentNullException()
    {
        // Arrange
        var result = Result<int, string>.Ok(42);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            result.Do(null!, _ => { }));
    }

    [Fact]
    public void Do_WithNullOnFailure_ThrowsArgumentNullException()
    {
        // Arrange
        var result = Result<int, string>.Ok(42);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            result.Do(_ => { }, null!));
    }

    #endregion

    #region Result.GetValueOrDefault Tests

    [Fact]
    public void GetValueOrDefault_WithSuccess_ReturnsValue()
    {
        // Arrange
        var result = Result<int, string>.Ok(42);

        // Act
        var value = result.GetValueOrDefault(0);

        // Assert
        Assert.Equal(42, value);
    }

    [Fact]
    public void GetValueOrDefault_WithFailure_ReturnsDefault()
    {
        // Arrange
        var result = Result<int, string>.Err("error");

        // Act
        var value = result.GetValueOrDefault(99);

        // Assert
        Assert.Equal(99, value);
    }

    #endregion

    #region Result.GetValueOrElse Tests

    [Fact]
    public void GetValueOrElse_WithSuccess_ReturnsValue()
    {
        // Arrange
        var result = Result<int, string>.Ok(42);
        var factoryCalled = false;

        // Act
        var value = result.GetValueOrElse(error =>
        {
            factoryCalled = true;
            return 99;
        });

        // Assert
        Assert.Equal(42, value);
        Assert.False(factoryCalled);
    }

    [Fact]
    public void GetValueOrElse_WithFailure_CallsFactory()
    {
        // Arrange
        var result = Result<int, string>.Err("error");
        var capturedError = "";

        // Act
        var value = result.GetValueOrElse(error =>
        {
            capturedError = error;
            return 99;
        });

        // Assert
        Assert.Equal(99, value);
        Assert.Equal("error", capturedError);
    }

    [Fact]
    public void GetValueOrElse_WithNullFactory_ThrowsArgumentNullException()
    {
        // Arrange
        var result = Result<int, string>.Err("error");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            result.GetValueOrElse(null!));
    }

    #endregion

    #region Result.GetValueOrThrow Tests

    [Fact]
    public void GetValueOrThrow_WithSuccess_ReturnsValue()
    {
        // Arrange
        var result = Result<int, string>.Ok(42);

        // Act
        var value = result.GetValueOrThrow(error =>
            new InvalidOperationException(error));

        // Assert
        Assert.Equal(42, value);
    }

    [Fact]
    public void GetValueOrThrow_WithFailure_ThrowsException()
    {
        // Arrange
        var result = Result<int, string>.Err("error message");

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            result.GetValueOrThrow(error =>
                new InvalidOperationException(error)));

        Assert.Equal("error message", exception.Message);
    }

    [Fact]
    public void GetValueOrThrow_WithNullFactory_ThrowsArgumentNullException()
    {
        // Arrange
        var result = Result<int, string>.Err("error");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            result.GetValueOrThrow(null!));
    }

    #endregion

    #region Result.ToOption Tests

    [Fact]
    public void ToOption_WithSuccess_ReturnsSome()
    {
        // Arrange
        var result = Result<int, string>.Ok(42);

        // Act
        var option = result.ToOption();

        // Assert
        Assert.True(option.IsSome());
        if (option is Option<int>.Some some)
        {
            Assert.Equal(42, some.Value);
        }
    }

    [Fact]
    public void ToOption_WithFailure_ReturnsNone()
    {
        // Arrange
        var result = Result<int, string>.Err("error");

        // Act
        var option = result.ToOption();

        // Assert
        Assert.True(option.IsNone());
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void Integration_OptionChaining()
    {
        // Arrange
        var option = new Option<int>.Some(42);
        var log = new List<string>();

        // Act
        var value = option
            .IfSome(v => log.Add($"Found: {v}"))
            .Filter(v => v > 10)
            .Map(v => v * 2)
            .IfSome(v => log.Add($"Doubled: {v}"))
            .GetOrDefault(0);

        // Assert
        Assert.Equal(84, value);
        Assert.Equal(2, log.Count);
        Assert.Contains("Found: 42", log);
        Assert.Contains("Doubled: 84", log);
    }

    [Fact]
    public void Integration_ResultChaining()
    {
        // Arrange
        var result = Result<int, string>.Ok(42);
        var log = new List<string>();

        // Act
        var value = result
            .OnSuccess(v => log.Add($"Success: {v}"))
            .Map(v => v * 2)
            .OnSuccess(v => log.Add($"Doubled: {v}"))
            .OnFailure(e => log.Add($"Error: {e}"))
            .GetValueOrDefault(0);

        // Assert
        Assert.Equal(84, value);
        Assert.Equal(2, log.Count);
        Assert.Contains("Success: 42", log);
        Assert.Contains("Doubled: 84", log);
    }

    [Fact]
    public void Integration_ConditionalExecution()
    {
        // Arrange
        var successResult = Result<int, string>.Ok(42);
        var errorResult = Result<int, string>.Err("error");

        var successLog = new List<string>();
        var errorLog = new List<string>();

        // Act
        successResult.Do(
            v => successLog.Add($"Value: {v}"),
            e => successLog.Add($"Error: {e}"));

        errorResult.Do(
            v => errorLog.Add($"Value: {v}"),
            e => errorLog.Add($"Error: {e}"));

        // Assert
        Assert.Single(successLog);
        Assert.Contains("Value: 42", successLog);

        Assert.Single(errorLog);
        Assert.Contains("Error: error", errorLog);
    }

    [Fact]
    public void Integration_FallbackChain()
    {
        // Arrange
        var noneOption = new Option<int>.None();

        // Act - Try multiple fallbacks
        var value = noneOption
            .GetOrElse(() =>
            {
                // Try fallback 1
                return new Option<int>.None().GetOrElse(() =>
                {
                    // Try fallback 2
                    return 99; // Final fallback
                });
            });

        // Assert
        Assert.Equal(99, value);
    }

    #endregion
}
