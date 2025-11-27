using Esox.SharpAndRusty.Types;
using Esox.SharpAndRusty.Extensions;

namespace Esox.SharpAndRust.Tests.Extensions;

public class ResultExtensionsAdvancedTests
{
    #region MapError Tests

    [Fact]
    public void MapError_TransformsErrorType()
    {
        // Arrange
        var result = Result<int, string>.Err("404");

        // Act
        var mapped = result.MapError(int.Parse);

        // Assert
        Assert.True(mapped.IsFailure);
        Assert.True(mapped.TryGetError(out var error));
        Assert.Equal(404, error);
    }

    [Fact]
    public void MapError_PreservesSuccessValue()
    {
        // Arrange
        var result = Result<int, string>.Ok(42);

        // Act
        var mapped = result.MapError(msg => msg.Length);

        // Assert
        Assert.True(mapped.IsSuccess);
        Assert.Equal(42, mapped.UnwrapOr(0));
    }

    [Fact]
    public void MapError_ThrowsOnNullMapper()
    {
        // Arrange
        var result = Result<int, string>.Err("error");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            result.MapError((Func<string, int>)null!));
    }

    #endregion

    #region Expect Tests

    [Fact]
    public void Expect_ReturnsValueOnSuccess()
    {
        // Arrange
        var result = Result<int, string>.Ok(42);

        // Act
        var value = result.Expect("User ID is required");

        // Assert
        Assert.Equal(42, value);
    }

    [Fact]
    public void Expect_ThrowsWithCustomMessageOnFailure()
    {
        // Arrange
        var result = Result<int, string>.Err("Not found");

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            result.Expect("User ID is required"));
        
        Assert.Contains("User ID is required", exception.Message);
        Assert.Contains("Not found", exception.Message);
    }

    [Fact]
    public void Expect_ThrowsOnNullMessage()
    {
        // Arrange
        var result = Result<int, string>.Ok(42);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            result.Expect(null!));
    }

    #endregion

    #region Tap Tests

    [Fact]
    public void Tap_ExecutesBothActionsOnSuccess()
    {
        // Arrange
        var result = Result<int, string>.Ok(42);
        var successCalled = false;
        var failureCalled = false;

        // Act
        var returned = result.Tap(
            onSuccess: _ => successCalled = true,
            onFailure: _ => failureCalled = true
        );

        // Assert
        Assert.True(successCalled);
        Assert.False(failureCalled);
        Assert.True(returned.IsSuccess);
        Assert.Equal(42, returned.UnwrapOr(0));
    }

    [Fact]
    public void Tap_ExecutesBothActionsOnFailure()
    {
        // Arrange
        var result = Result<int, string>.Err("Error");
        var successCalled = false;
        var failureCalled = false;

        // Act
        var returned = result.Tap(
            onSuccess: _ => successCalled = true,
            onFailure: _ => failureCalled = true
        );

        // Assert
        Assert.False(successCalled);
        Assert.True(failureCalled);
        Assert.True(returned.IsFailure);
    }

    [Fact]
    public void Tap_ThrowsOnNullActions()
    {
        // Arrange
        var result = Result<int, string>.Ok(42);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            result.Tap(null!, _ => { }));
        
        Assert.Throws<ArgumentNullException>(() =>
            result.Tap(_ => { }, null!));
    }

    #endregion

    #region Contains Tests

    [Fact]
    public void Contains_ReturnsTrueForMatchingValue()
    {
        // Arrange
        var result = Result<int, string>.Ok(42);

        // Act
        var contains = result.Contains(42);

        // Assert
        Assert.True(contains);
    }

    [Fact]
    public void Contains_ReturnsFalseForDifferentValue()
    {
        // Arrange
        var result = Result<int, string>.Ok(42);

        // Act
        var contains = result.Contains(43);

        // Assert
        Assert.False(contains);
    }

    [Fact]
    public void Contains_ReturnsFalseForFailure()
    {
        // Arrange
        var result = Result<int, string>.Err("Error");

        // Act
        var contains = result.Contains(42);

        // Assert
        Assert.False(contains);
    }

    [Fact]
    public void Contains_WorksWithReferenceTypes()
    {
        // Arrange
        var result = Result<string, int>.Ok("Hello");

        // Act
        var contains = result.Contains("Hello");

        // Assert
        Assert.True(contains);
    }

    [Fact]
    public void Contains_HandlesNull()
    {
        // Arrange
        var result = Result<string?, int>.Ok(null);

        // Act
        var contains = result.Contains(null);

        // Assert
        Assert.True(contains);
    }

    #endregion

    #region Combine Tests

    [Fact]
    public void Combine_ReturnsAllSuccessValues()
    {
        // Arrange
        var results = new[]
        {
            Result<int, string>.Ok(1),
            Result<int, string>.Ok(2),
            Result<int, string>.Ok(3)
        };

        // Act
        var combined = results.Combine();

        // Assert
        Assert.True(combined.IsSuccess);
        combined.TryGetValue(out var values);
        Assert.Equal(new[] { 1, 2, 3 }, values);
    }

    [Fact]
    public void Combine_ReturnsFirstError()
    {
        // Arrange
        var results = new[]
        {
            Result<int, string>.Ok(1),
            Result<int, string>.Err("Error 1"),
            Result<int, string>.Err("Error 2")
        };

        // Act
        var combined = results.Combine();

        // Assert
        Assert.True(combined.IsFailure);
        combined.TryGetError(out var error);
        Assert.Equal("Error 1", error);
    }

    [Fact]
    public void Combine_HandlesEmptyCollection()
    {
        // Arrange
        var results = Array.Empty<Result<int, string>>();

        // Act
        var combined = results.Combine();

        // Assert
        Assert.True(combined.IsSuccess);
        combined.TryGetValue(out var values);
        Assert.Empty(values);
    }

    [Fact]
    public void Combine_ThrowsOnNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            ResultExtensions.Combine<int, string>(null!));
    }

    #endregion

    #region Partition Tests

    [Fact]
    public void Partition_SeparatesSuccessesAndFailures()
    {
        // Arrange
        var results = new[]
        {
            Result<int, string>.Ok(1),
            Result<int, string>.Err("Error 1"),
            Result<int, string>.Ok(2),
            Result<int, string>.Err("Error 2"),
            Result<int, string>.Ok(3)
        };

        // Act
        var (successes, failures) = results.Partition();

        // Assert
        Assert.Equal(new[] { 1, 2, 3 }, successes);
        Assert.Equal(new[] { "Error 1", "Error 2" }, failures);
    }

    [Fact]
    public void Partition_HandlesAllSuccesses()
    {
        // Arrange
        var results = new[]
        {
            Result<int, string>.Ok(1),
            Result<int, string>.Ok(2),
            Result<int, string>.Ok(3)
        };

        // Act
        var (successes, failures) = results.Partition();

        // Assert
        Assert.Equal(new[] { 1, 2, 3 }, successes);
        Assert.Empty(failures);
    }

    [Fact]
    public void Partition_HandlesAllFailures()
    {
        // Arrange
        var results = new[]
        {
            Result<int, string>.Err("Error 1"),
            Result<int, string>.Err("Error 2"),
            Result<int, string>.Err("Error 3")
        };

        // Act
        var (successes, failures) = results.Partition();

        // Assert
        Assert.Empty(successes);
        Assert.Equal(new[] { "Error 1", "Error 2", "Error 3" }, failures);
    }

    [Fact]
    public void Partition_ThrowsOnNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            ResultExtensions.Partition<int, string>(null!));
    }

    #endregion

    #region Argument Validation Tests

    [Fact]
    public void Map_ThrowsOnNullMapper()
    {
        // Arrange
        var result = Result<int, string>.Ok(42);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            result.Map((Func<int, int>)null!));
    }

    [Fact]
    public void Bind_ThrowsOnNullBinder()
    {
        // Arrange
        var result = Result<int, string>.Ok(42);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            result.Bind((Func<int, Result<int, string>>)null!));
    }

    [Fact]
    public void Select_ThrowsOnNullSelector()
    {
        // Arrange
        var result = Result<int, string>.Ok(42);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            result.Select((Func<int, int>)null!));
    }

    [Fact]
    public void SelectMany_SingleParameter_ThrowsOnNullSelector()
    {
        // Arrange
        var result = Result<int, string>.Ok(42);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            result.SelectMany((Func<int, Result<int, string>>)null!));
    }

    [Fact]
    public void SelectMany_TwoParameters_ThrowsOnNullSelector()
    {
        // Arrange
        var result = Result<int, string>.Ok(42);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            result.SelectMany((Func<int, Result<int, string>>)null!, (x, y) => x + y));
    }

    [Fact]
    public void SelectMany_TwoParameters_ThrowsOnNullProjector()
    {
        // Arrange
        var result = Result<int, string>.Ok(42);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            result.SelectMany(x => Result<int, string>.Ok(x), (Func<int, int, int>)null!));
    }

    [Fact]
    public void Where_ThrowsOnNullPredicate()
    {
        // Arrange
        var result = Result<int, string>.Ok(42);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            result.Where(null!));
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void MapError_CanBeChainedWithOtherOperations()
    {
        // Arrange
        var result = Result<int, string>.Err("404");

        // Act
        var final = result
            .MapError(int.Parse)
            .MapError(code => $"Error code: {code}")
            .OrElse(error => Result<int, string>.Ok(0));

        // Assert
        Assert.True(final.IsSuccess);
        Assert.Equal(0, final.UnwrapOr(-1));
    }

    [Fact]
    public void ComplexChain_WithNewExtensions()
    {
        // Arrange
        var result = Result<int, string>.Ok(10);

        // Act
        var final = result
            .Map(x => x * 2)
            .Tap(
                onSuccess: value => Console.WriteLine($"Success: {value}"),
                onFailure: error => Console.WriteLine($"Error: {error}")
            )
            .Bind(x => x > 15 
                ? Result<int, string>.Ok(x) 
                : Result<int, string>.Err("Too small"))
            .MapError(msg => msg.ToUpper());

        // Assert
        Assert.True(final.IsSuccess);
        Assert.Equal(20, final.UnwrapOr(0));
    }

    [Fact]
    public void Expect_ProvidesContextInComplexChains()
    {
        // Arrange & Act
        var value = Result<int, string>.Ok(42)
            .Map(x => x * 2)
            .Bind(x => Result<int, string>.Ok(x + 10))
            .Expect("Final value should be available");

        // Assert
        Assert.Equal(94, value);
    }

    #endregion
}
