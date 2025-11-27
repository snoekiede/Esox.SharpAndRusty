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
            failure: _ => "Error"
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
            success: _ => "Success",
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
        var mapped = result.Map(int.Parse);

        // Assert
        var value = mapped.Match(
            success: v => v,
            failure: _ => 0
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
        var mapped = result.Map(x =>
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
            failure: _ => "Error"
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
            success: _ => "Success",
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
        var bound = result.Bind(_ => Result<string, string>.Err("Binder error"));

        // Assert
        var error = bound.Match(
            success: _ => "Success",
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
            failure: _ => "Error"
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
            .Bind(_ => Result<int, string>.Err("Second binder failed"))
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
            .Map(x => x * 2)
            .Bind(x => Result<string, string>.Ok($"Value: {x}"));

        // Assert
        var output = final.Match(
            success: v => v,
            failure: _ => "Error"
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
            .Map(int.Parse)
            .Bind(x => x > 5 ? Result<int, string>.Ok(x) : Result<int, string>.Err("Too small"))
            .Map<int, string, string>(x => $"Valid number: {x}")
            .Bind(x => Result<string, string>.Ok(x.ToUpper()));

        // Assert
        var output = final.Match(
            success: v => v,
            failure: _ => "Error"
        );
        Assert.Equal("VALID NUMBER: 10", output);
        Assert.True(final.IsSuccess);
    }

    #region SelectMany Tests (LINQ Integration)

    [Fact]
    public void SelectMany_SingleParameter_ChainsSuccessfulOperations()
    {
        // Arrange
        var result = Result<int, string>.Ok(10);

        // Act
        var final = result.SelectMany(x => Result<int, string>.Ok(x * 2));

        // Assert
        Assert.True(final.IsSuccess);
        Assert.Equal(20, final.UnwrapOr(0));
    }

    [Fact]
    public void SelectMany_SingleParameter_PropagatesOriginalError()
    {
        // Arrange
        var result = Result<int, string>.Err("Original error");

        // Act
        var final = result.SelectMany(x => Result<int, string>.Ok(x * 2));

        // Assert
        Assert.True(final.IsFailure);
        Assert.True(final.TryGetError(out var error));
        Assert.Equal("Original error", error);
    }

    [Fact]
    public void SelectMany_SingleParameter_PropagatesSelectorError()
    {
        // Arrange
        var result = Result<int, string>.Ok(10);

        // Act
        var final = result.SelectMany(_ => Result<int, string>.Err("Selector error"));

        // Assert
        Assert.True(final.IsFailure);
        Assert.True(final.TryGetError(out var error));
        Assert.Equal("Selector error", error);
    }

    [Fact]
    public void SelectMany_SingleParameter_DoesNotInvokeSelectorOnError()
    {
        // Arrange
        var result = Result<int, string>.Err("Error");
        var selectorInvoked = false;

        // Act
        var final = result.SelectMany(x =>
        {
            selectorInvoked = true;
            return Result<int, string>.Ok(x * 2);
        });

        // Assert
        Assert.False(selectorInvoked);
        Assert.True(final.IsFailure);
    }

    [Fact]
    public void SelectMany_TwoParameters_ProjectsBothValuesOnSuccess()
    {
        // Arrange
        var result = Result<int, string>.Ok(10);

        // Act
        var final = result.SelectMany(
            x => Result<int, string>.Ok(x + 5),
            (x, y) => x + y
        );

        // Assert
        Assert.True(final.IsSuccess);
        Assert.Equal(25, final.UnwrapOr(0)); // 10 + 15
    }

    [Fact]
    public void SelectMany_TwoParameters_PropagatesOriginalError()
    {
        // Arrange
        var result = Result<int, string>.Err("Original error");

        // Act
        var final = result.SelectMany(
            x => Result<int, string>.Ok(x + 5),
            (x, y) => x + y
        );

        // Assert
        Assert.True(final.IsFailure);
        Assert.True(final.TryGetError(out var error));
        Assert.Equal("Original error", error);
    }

    [Fact]
    public void SelectMany_TwoParameters_PropagatesSelectorError()
    {
        // Arrange
        var result = Result<int, string>.Ok(10);

        // Act
        var final = result.SelectMany(
            _ => Result<int, string>.Err("Selector error"),
            (x, y) => x + y
        );

        // Assert
        Assert.True(final.IsFailure);
        Assert.True(final.TryGetError(out var error));
        Assert.Equal("Selector error", error);
    }

    [Fact]
    public void SelectMany_TwoParameters_DoesNotInvokeProjectorOnError()
    {
        // Arrange
        var result = Result<int, string>.Ok(10);
        var projectorInvoked = false;

        // Act
        var final = result.SelectMany(
            _ => Result<int, string>.Err("Selector error"),
            (x, y) =>
            {
                projectorInvoked = true;
                return x + y;
            }
        );

        // Assert
        Assert.False(projectorInvoked);
        Assert.True(final.IsFailure);
    }

    [Fact]
    public void LinqQuerySyntax_SimpleQuery_WorksCorrectly()
    {
        // Arrange & Act
        var result = from x in Result<int, string>.Ok(10)
                     from y in Result<int, string>.Ok(20)
                     select x + y;

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(30, result.UnwrapOr(0));
    }

    [Fact]
    public void LinqQuerySyntax_StopsOnFirstError()
    {
        // Arrange & Act
        var result = from x in Result<int, string>.Ok(10)
                     from y in Result<int, string>.Err("Error in y")
                     from z in Result<int, string>.Ok(30)
                     select x + y + z;

        // Assert
        Assert.True(result.IsFailure);
        Assert.True(result.TryGetError(out var error));
        Assert.Equal("Error in y", error);
    }

    [Fact]
    public void LinqQuerySyntax_ComplexQuery_WorksCorrectly()
    {
        // Arrange
        Result<int, string> ParseInt(string s) =>
            int.TryParse(s, out var value)
                ? Result<int, string>.Ok(value)
                : Result<int, string>.Err($"Cannot parse '{s}'");

        Result<int, string> Divide(int numerator, int denominator) =>
            denominator != 0
                ? Result<int, string>.Ok(numerator / denominator)
                : Result<int, string>.Err("Division by zero");

        // Act
        var result = from x in ParseInt("100")
                     from y in ParseInt("5")
                     from z in Divide(x, y)
                     select $"Result: {z}";

        // Assert
        Assert.True(result.IsSuccess);
        var output = result.Match(v => v, _ => "Error");
        Assert.Equal("Result: 20", output);
    }

    [Fact]
    public void LinqQuerySyntax_WithWhereClause_WorksCorrectly()
    {
        // Arrange & Act
        var result = (from x in Result<int, string>.Ok(10)
                      from y in Result<int, string>.Ok(20)
                      select x + y)
            .Bind(sum => sum > 25 
                ? Result<int, string>.Ok(sum) 
                : Result<int, string>.Err("Sum must be greater than 25"));

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(30, result.UnwrapOr(0));
    }

    [Fact]
    public void LinqQuerySyntax_MultipleFromClauses_ProjectsCorrectly()
    {
        // Arrange & Act
        var result = from a in Result<int, string>.Ok(2)
                     from b in Result<int, string>.Ok(3)
                     from c in Result<int, string>.Ok(4)
                     select a * b * c;

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(24, result.UnwrapOr(0));
    }

    [Fact]
    public void LinqQuerySyntax_WithConditionalLogic_WorksCorrectly()
    {
        // Arrange
        Result<int, string> ValidatePositive(int value) =>
            value > 0
                ? Result<int, string>.Ok(value)
                : Result<int, string>.Err("Value must be positive");

        // Act
        var result = from x in Result<int, string>.Ok(10)
                     from validated in ValidatePositive(x)
                     select validated * 2;

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(20, result.UnwrapOr(0));
    }

    [Fact]
    public void LinqQuerySyntax_FailsValidation_ReturnsError()
    {
        // Arrange
        Result<int, string> ValidatePositive(int value) =>
            value > 0
                ? Result<int, string>.Ok(value)
                : Result<int, string>.Err("Value must be positive");

        // Act
        var result = from x in Result<int, string>.Ok(-5)
                     from validated in ValidatePositive(x)
                     select validated * 2;

        // Assert
        Assert.True(result.IsFailure);
        Assert.True(result.TryGetError(out var error));
        Assert.Equal("Value must be positive", error);
    }

    [Fact]
    public void LinqQuerySyntax_MixedWithMap_WorksCorrectly()
    {
        // Arrange & Act
        var result = (from x in Result<int, string>.Ok(5)
                      from y in Result<int, string>.Ok(10)
                      select x + y)
            .Map(sum => $"Total: {sum}");

        // Assert
        Assert.True(result.IsSuccess);
        var output = result.Match(v => v, _ => "Error");
        Assert.Equal("Total: 15", output);
    }

    [Fact]
    public void LinqQuerySyntax_WithDifferentTypes_WorksCorrectly()
    {
        // Arrange & Act
        var result = from name in Result<string, string>.Ok("John")
                     from age in Result<int, string>.Ok(30)
                     select $"{name} is {age} years old";

        // Assert
        Assert.True(result.IsSuccess);
        var output = result.Match(v => v, _ => "Error");
        Assert.Equal("John is 30 years old", output);
    }

    #endregion
}
