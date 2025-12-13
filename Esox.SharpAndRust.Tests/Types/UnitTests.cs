using Esox.SharpAndRusty.Types;
using Esox.SharpAndRusty.Extensions;

namespace Esox.SharpAndRust.Tests.Types;

public class UnitTests
{
    #region Singleton and Creation Tests

    [Fact]
    public void Value_ReturnsSingletonInstance()
    {
        // Arrange & Act
        var unit1 = Unit.Value;
        var unit2 = Unit.Value;

        // Assert
        Assert.Equal(unit1, unit2);
    }

    [Fact]
    public void DefaultConstructor_CreatesValidUnit()
    {
        // Arrange & Act
        var unit = default(Unit);

        // Assert
        Assert.Equal(Unit.Value, unit);
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void Equals_Unit_AlwaysReturnsTrue()
    {
        // Arrange
        var unit1 = Unit.Value;
        var unit2 = default(Unit);

        // Act & Assert
        Assert.True(unit1.Equals(unit2));
        Assert.True(unit2.Equals(unit1));
    }

    [Fact]
    public void Equals_Object_ReturnsTrueForUnitType()
    {
        // Arrange
        var unit = Unit.Value;
        object obj = Unit.Value;

        // Act & Assert
        Assert.True(unit.Equals(obj));
    }

    [Fact]
    public void Equals_Object_ReturnsFalseForNonUnitType()
    {
        // Arrange
        var unit = Unit.Value;
        object obj = 42;

        // Act & Assert
        Assert.False(unit.Equals(obj));
    }

    [Fact]
    public void Equals_Object_ReturnsFalseForNull()
    {
        // Arrange
        var unit = Unit.Value;
        object? obj = null;

        // Act & Assert
        Assert.False(unit.Equals(obj));
    }

    #endregion

    #region Operator Tests

    [Fact]
    public void EqualityOperator_AlwaysReturnsTrue()
    {
        // Arrange
        var unit1 = Unit.Value;
        var unit2 = default(Unit);

        // Act & Assert
        Assert.True(unit1 == unit2);
        Assert.True(unit2 == unit1);
    }

    [Fact]
    public void InequalityOperator_AlwaysReturnsFalse()
    {
        // Arrange
        var unit1 = Unit.Value;
        var unit2 = default(Unit);

        // Act & Assert
        Assert.False(unit1 != unit2);
        Assert.False(unit2 != unit1);
    }

    [Fact]
    public void LessThanOperator_AlwaysReturnsFalse()
    {
        // Arrange
        var unit1 = Unit.Value;
        var unit2 = default(Unit);

        // Act & Assert
        Assert.False(unit1 < unit2);
        Assert.False(unit2 < unit1);
    }

    [Fact]
    public void GreaterThanOperator_AlwaysReturnsFalse()
    {
        // Arrange
        var unit1 = Unit.Value;
        var unit2 = default(Unit);

        // Act & Assert
        Assert.False(unit1 > unit2);
        Assert.False(unit2 > unit1);
    }

    [Fact]
    public void LessThanOrEqualOperator_AlwaysReturnsTrue()
    {
        // Arrange
        var unit1 = Unit.Value;
        var unit2 = default(Unit);

        // Act & Assert
        Assert.True(unit1 <= unit2);
        Assert.True(unit2 <= unit1);
    }

    [Fact]
    public void GreaterThanOrEqualOperator_AlwaysReturnsTrue()
    {
        // Arrange
        var unit1 = Unit.Value;
        var unit2 = default(Unit);

        // Act & Assert
        Assert.True(unit1 >= unit2);
        Assert.True(unit2 >= unit1);
    }

    #endregion

    #region Hash Code Tests

    [Fact]
    public void GetHashCode_ReturnsSameValueForAllUnits()
    {
        // Arrange
        var unit1 = Unit.Value;
        var unit2 = default(Unit);

        // Act
        var hash1 = unit1.GetHashCode();
        var hash2 = unit2.GetHashCode();

        // Assert
        Assert.Equal(hash1, hash2);
        Assert.Equal(0, hash1);
    }

    [Fact]
    public void GetHashCode_ConsistentWithEquals()
    {
        // Arrange
        var unit1 = Unit.Value;
        var unit2 = default(Unit);

        // Act & Assert - If objects are equal, hash codes must be equal
        if (unit1.Equals(unit2))
        {
            Assert.Equal(unit1.GetHashCode(), unit2.GetHashCode());
        }
    }

    #endregion

    #region CompareTo Tests

    [Fact]
    public void CompareTo_AlwaysReturnsZero()
    {
        // Arrange
        var unit1 = Unit.Value;
        var unit2 = default(Unit);

        // Act
        var result1 = unit1.CompareTo(unit2);
        var result2 = unit2.CompareTo(unit1);

        // Assert
        Assert.Equal(0, result1);
        Assert.Equal(0, result2);
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_ReturnsEmptyParentheses()
    {
        // Arrange
        var unit = Unit.Value;

        // Act
        var result = unit.ToString();

        // Assert
        Assert.Equal("()", result);
    }

    #endregion

    #region Result Integration Tests

    [Fact]
    public void Unit_WorksAsResultSuccessType()
    {
        // Arrange
        var successResult = Result<Unit, string>.Ok(Unit.Value);
        var errorResult = Result<Unit, string>.Err("Error occurred");

        // Act & Assert
        Assert.True(successResult.IsSuccess);
        Assert.True(errorResult.IsFailure);
    }

    [Fact]
    public void Unit_WorksInResultMatch()
    {
        // Arrange
        var result = Result<Unit, string>.Ok(Unit.Value);
        var matchCalled = false;

        // Act
        var message = result.Match(
            success: _ =>
            {
                matchCalled = true;
                return "Success";
            },
            failure: error => $"Error: {error}"
        );

        // Assert
        Assert.True(matchCalled);
        Assert.Equal("Success", message);
    }

    [Fact]
    public void Unit_WorksInResultMap()
    {
        // Arrange
        var result = Result<Unit, string>.Ok(Unit.Value);

        // Act
        var mapped = result.Map(_ => "Mapped");

        // Assert
        Assert.True(mapped.IsSuccess);
        Assert.Equal("Mapped", mapped.UnwrapOr("Default"));
    }

    [Fact]
    public void Unit_WorksInResultBind()
    {
        // Arrange
        var result = Result<Unit, string>.Ok(Unit.Value);

        // Act
        var bound = result.Bind(_ => Result<string, string>.Ok("Bound"));

        // Assert
        Assert.True(bound.IsSuccess);
        Assert.Equal("Bound", bound.UnwrapOr("Default"));
    }

    [Fact]
    public void Unit_WorksWithLinqSyntax()
    {
        // Arrange
        var result1 = Result<Unit, string>.Ok(Unit.Value);
        var result2 = Result<Unit, string>.Ok(Unit.Value);

        // Act
        var combined = from _ in result1
                       from __ in result2
                       select Unit.Value;

        // Assert
        Assert.True(combined.IsSuccess);
    }

    [Fact]
    public void Unit_PropagatesErrorInLinqSyntax()
    {
        // Arrange
        var result1 = Result<Unit, string>.Ok(Unit.Value);
        var result2 = Result<Unit, string>.Err("Error");

        // Act
        var combined = from _ in result1
                       from __ in result2
                       select Unit.Value;

        // Assert
        Assert.True(combined.IsFailure);
        Assert.True(combined.TryGetError(out var error));
        Assert.Equal("Error", error);
    }

    #endregion

    #region Use Case Tests

    [Fact]
    public void Unit_ValidateInputUseCase()
    {
        // Arrange
        Result<Unit, string> ValidateInput(string input)
        {
            if (string.IsNullOrEmpty(input))
                return Result<Unit, string>.Err("Input cannot be empty");
            
            return Result<Unit, string>.Ok(Unit.Value);
        }

        // Act
        var successResult = ValidateInput("valid input");
        var errorResult = ValidateInput("");

        // Assert
        Assert.True(successResult.IsSuccess);
        Assert.True(errorResult.IsFailure);
        
        // Verify error message
        Assert.True(errorResult.TryGetError(out var error));
        Assert.Equal("Input cannot be empty", error);
    }

    [Fact]
    public void Unit_ChainingOperationsWithNoValue()
    {
        // Arrange
        Result<Unit, string> Step1() => Result<Unit, string>.Ok(Unit.Value);
        Result<Unit, string> Step2() => Result<Unit, string>.Ok(Unit.Value);
        Result<Unit, string> Step3() => Result<Unit, string>.Ok(Unit.Value);

        // Act
        var result = Step1()
            .Bind(_ => Step2())
            .Bind(_ => Step3());

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Unit_ErrorPropagationInChain()
    {
        // Arrange
        Result<Unit, string> Step1() => Result<Unit, string>.Ok(Unit.Value);
        Result<Unit, string> Step2() => Result<Unit, string>.Err("Step 2 failed");
        Result<Unit, string> Step3() => Result<Unit, string>.Ok(Unit.Value);

        // Act
        var result = Step1()
            .Bind(_ => Step2())
            .Bind(_ => Step3());

        // Assert
        Assert.True(result.IsFailure);
        Assert.True(result.TryGetError(out var error));
        Assert.Equal("Step 2 failed", error);
    }

    #endregion

    #region Collection Tests

    [Fact]
    public void Unit_WorksInCollections()
    {
        // Arrange
        var list = new List<Unit> { Unit.Value, default, Unit.Value };

        // Act
        var distinct = list.Distinct().ToList();

        // Assert
        Assert.Single(distinct);
        Assert.Equal(Unit.Value, distinct[0]);
    }

    [Fact]
    public void Unit_WorksInDictionary()
    {
        // Arrange
        var dict = new Dictionary<Unit, string>
        {
            [Unit.Value] = "First",
            [default(Unit)] = "Second"  // Should overwrite "First"
        };

        // Act & Assert
        Assert.Single(dict);
        Assert.Equal("Second", dict[Unit.Value]);
    }

    [Fact]
    public void Unit_WorksInHashSet()
    {
        // Arrange
        var set = new HashSet<Unit> { Unit.Value, default, Unit.Value };

        // Act & Assert
        Assert.Single(set);
        Assert.Contains(Unit.Value, set);
    }

    #endregion
}
