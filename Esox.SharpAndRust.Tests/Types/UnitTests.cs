using Esox.SharpAndRusty.Types;
using Xunit;

namespace Esox.SharpAndRust.Tests.Types
{
    public class UnitTests
    {
        [Fact]
        public void Unit_Value_IsAvailable()
        {
            // Act
            var unit = Unit.Value;

            // Assert - Unit is a value type, so it always has a value
            Assert.Equal(default(Unit), unit);
        }

        [Fact]
        public void Unit_DefaultValue_EqualsStaticValue()
        {
            // Arrange
            var defaultUnit = default(Unit);
            var staticUnit = Unit.Value;

            // Assert
            Assert.Equal(defaultUnit, staticUnit);
        }

        [Fact]
        public void Unit_Equals_AlwaysReturnsTrue()
        {
            // Arrange
            var unit1 = Unit.Value;
            var unit2 = default(Unit);
            var unit3 = new Unit();

            // Assert
            Assert.True(unit1.Equals(unit2));
            Assert.True(unit2.Equals(unit3));
            Assert.True(unit1.Equals(unit3));
        }

        [Fact]
        public void Unit_EqualsOperator_AlwaysReturnsTrue()
        {
            // Arrange
            var unit1 = Unit.Value;
            var unit2 = default(Unit);

            // Assert
            Assert.True(unit1 == unit2);
            Assert.True(unit2 == unit1);
        }

        [Fact]
        public void Unit_NotEqualsOperator_AlwaysReturnsFalse()
        {
            // Arrange
            var unit1 = Unit.Value;
            var unit2 = default(Unit);

            // Assert
            Assert.False(unit1 != unit2);
            Assert.False(unit2 != unit1);
        }

        [Fact]
        public void Unit_GetHashCode_IsConsistent()
        {
            // Arrange
            var unit1 = Unit.Value;
            var unit2 = default(Unit);
            var unit3 = new Unit();

            // Assert
            Assert.Equal(unit1.GetHashCode(), unit2.GetHashCode());
            Assert.Equal(unit2.GetHashCode(), unit3.GetHashCode());
        }

        [Fact]
        public void Unit_ToString_ReturnsExpectedValue()
        {
            // Arrange
            var unit = Unit.Value;

            // Act
            var str = unit.ToString();

            // Assert
            Assert.Equal("()", str);
        }

        [Fact]
        public void Unit_CanBeUsedInResult()
        {
            // Arrange & Act
            var success = Result<Unit, string>.Ok(Unit.Value);
            var failure = Result<Unit, string>.Err("Error occurred");

            // Assert
            Assert.True(success.IsSuccess);
            Assert.True(failure.IsFailure);
        }

        [Fact]
        public void Unit_ResultOk_CanBeMatched()
        {
            // Arrange
            var result = Result<Unit, string>.Ok(Unit.Value);

            // Act
            var message = result.Match(
                success: _ => "Operation succeeded",
                failure: error => $"Error: {error}"
            );

            // Assert
            Assert.Equal("Operation succeeded", message);
        }

        [Fact]
        public void Unit_ResultErr_CanBeMatched()
        {
            // Arrange
            var result = Result<Unit, string>.Err("Something went wrong");

            // Act
            var message = result.Match(
                success: _ => "Operation succeeded",
                failure: error => $"Error: {error}"
            );

            // Assert
            Assert.Equal("Error: Something went wrong", message);
        }

        [Fact]
        public void Unit_WithError_CanBeUsed()
        {
            // Arrange
            var error = Error.New("Logger has already been terminated")
                .WithKind(ErrorKind.InvalidState);

            // Act
            var result = Result<Unit, Error>.Err(error);

            // Assert
            Assert.True(result.IsFailure);
            Assert.True(result.TryGetError(out var retrievedError));
            Assert.Equal(ErrorKind.InvalidState, retrievedError.Kind);
            Assert.Contains("Logger has already been terminated", retrievedError.Message);
        }

        [Fact]
        public void Unit_SuccessResult_CanBeInspected()
        {
            // Arrange
            var result = Result<Unit, string>.Ok(Unit.Value);
            var inspected = false;

            // Act
            result.Inspect(_ => inspected = true);

            // Assert
            Assert.True(inspected);
        }

        [Fact]
        public void Unit_FailureResult_DoesNotInspectSuccess()
        {
            // Arrange
            var result = Result<Unit, string>.Err("Error");
            var inspected = false;

            // Act
            result.Inspect(_ => inspected = true);

            // Assert
            Assert.False(inspected);
        }

        [Fact]
        public void Unit_CanBeReturnedFromMethod()
        {
            // Act
            var result = PerformOperation(true);

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void Unit_MethodCanReturnError()
        {
            // Act
            var result = PerformOperation(false);

            // Assert
            Assert.True(result.IsFailure);
            Assert.True(result.TryGetError(out var error));
            Assert.Contains("Operation failed", error);
        }

        [Fact]
        public void Unit_MultipleUnitsAreEqual()
        {
            // Arrange
            var units = new[] 
            { 
                Unit.Value, 
                default(Unit), 
                new Unit(),
                GetUnit()
            };

            // Assert - all units should be equal to each other
            for (int i = 0; i < units.Length; i++)
            {
                for (int j = 0; j < units.Length; j++)
                {
                    Assert.Equal(units[i], units[j]);
                    Assert.True(units[i] == units[j]);
                    Assert.False(units[i] != units[j]);
                }
            }
        }

        [Fact]
        public void Unit_EqualsObject_WithNull_ReturnsFalse()
        {
            // Arrange
            var unit = Unit.Value;

            // Act
            var equals = unit.Equals(null);

            // Assert
            Assert.False(equals);
        }

        [Fact]
        public void Unit_EqualsObject_WithDifferentType_ReturnsFalse()
        {
            // Arrange
            var unit = Unit.Value;
            var other = "not a unit";

            // Act
            var equals = unit.Equals(other);

            // Assert
            Assert.False(equals);
        }

        [Fact]
        public void Unit_EqualsObject_WithUnit_ReturnsTrue()
        {
            // Arrange
            var unit = Unit.Value;
            object other = default(Unit);

            // Act
            var equals = unit.Equals(other);

            // Assert
            Assert.True(equals);
        }

        // Helper methods
        private static Result<Unit, string> PerformOperation(bool succeed)
        {
            if (succeed)
                return Result<Unit, string>.Ok(Unit.Value);
            
            return Result<Unit, string>.Err("Operation failed");
        }

        private static Unit GetUnit()
        {
            return Unit.Value;
        }
    }
}
