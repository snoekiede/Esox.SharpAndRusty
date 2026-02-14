using Esox.SharpAndRusty.Types;
using Esox.SharpAndRusty.Extensions;

namespace Esox.SharpAndRust.Tests.Extensions;

public class CollectionExtensionsTests
{
    #region Option Sequence Tests

    [Fact]
    public void Sequence_Option_AllSome_ReturnsSomeWithAllValues()
    {
        // Arrange
        var options = new[]
        {
            new Option<int>.Some(1),
            new Option<int>.Some(2),
            new Option<int>.Some(3)
        };

        // Act
        var result = options.Sequence();

        // Assert
        Assert.True(result.IsSome());
        if (result is Option<IEnumerable<int>>.Some some)
        {
            Assert.Equal(new[] { 1, 2, 3 }, some.Value);
        }
    }

    [Fact]
    public void Sequence_Option_WithNone_ReturnsNone()
    {
        // Arrange
        Option<int>[] options =
        {
            new Option<int>.Some(1),
            new Option<int>.None(),
            new Option<int>.Some(3)
        };

        // Act
        var result = options.Sequence();

        // Assert
        Assert.True(result.IsNone());
    }

    [Fact]
    public void Sequence_Option_EmptyCollection_ReturnsSomeEmptyCollection()
    {
        // Arrange
        Option<int>[] options = Array.Empty<Option<int>>();

        // Act
        var result = options.Sequence();

        // Assert
        Assert.True(result.IsSome());
        if (result is Option<IEnumerable<int>>.Some some)
        {
            Assert.Empty(some.Value);
        }
    }

    [Fact]
    public void Sequence_Option_PreservesOrder()
    {
        // Arrange
        var options = new[]
        {
            new Option<string>.Some("first"),
            new Option<string>.Some("second"),
            new Option<string>.Some("third")
        };

        // Act
        var result = options.Sequence();

        // Assert
        Assert.True(result.IsSome());
        if (result is Option<IEnumerable<string>>.Some some)
        {
            Assert.Equal(new[] { "first", "second", "third" }, some.Value);
        }
    }

    #endregion

    #region Option Traverse Tests

    [Fact]
    public void Traverse_Option_AllSucceed_ReturnsSomeWithAllValues()
    {
        // Arrange
        var strings = new[] { "1", "2", "3" };

        // Act
        var result = strings.Traverse<string, int>(s =>
            int.TryParse(s, out var n)
                ? new Option<int>.Some(n)
                : new Option<int>.None());

        // Assert
        Assert.True(result.IsSome());
        if (result is Option<IEnumerable<int>>.Some some)
        {
            Assert.Equal(new[] { 1, 2, 3 }, some.Value);
        }
    }

    [Fact]
    public void Traverse_Option_WithFailure_ReturnsNone()
    {
        // Arrange
        var strings = new[] { "1", "invalid", "3" };

        // Act
        var result = strings.Traverse<string, int>(s =>
            int.TryParse(s, out var n)
                ? new Option<int>.Some(n)
                : new Option<int>.None());

        // Assert
        Assert.True(result.IsNone());
    }

    [Fact]
    public void Traverse_Option_EmptyCollection_ReturnsSomeEmptyCollection()
    {
        // Arrange
        var strings = Array.Empty<string>();

        // Act
        var result = strings.Traverse(s => new Option<int>.Some(s.Length));

        // Assert
        Assert.True(result.IsSome());
        if (result is Option<IEnumerable<int>>.Some some)
        {
            Assert.Empty(some.Value);
        }
    }

    [Fact]
    public void Traverse_Option_WithTransformation_WorksCorrectly()
    {
        // Arrange
        var numbers = new[] { 1, 2, 3, 4, 5 };

        // Act - Only accept even numbers
        var result = numbers.Traverse<int, int>(n =>
            n % 2 == 0
                ? new Option<int>.Some(n * 2)
                : new Option<int>.None());

        // Assert - Should fail because not all are even
        Assert.True(result.IsNone());
    }

    #endregion

    #region Option CollectSome Tests

    [Fact]
    public void CollectSome_MixedOptions_ReturnsOnlySomeValues()
    {
        // Arrange
        Option<int>[] options =
        {
            new Option<int>.Some(1),
            new Option<int>.None(),
            new Option<int>.Some(3),
            new Option<int>.None(),
            new Option<int>.Some(5)
        };

        // Act
        var result = options.CollectSome().ToList();

        // Assert
        Assert.Equal(new[] { 1, 3, 5 }, result);
    }

    [Fact]
    public void CollectSome_AllNone_ReturnsEmptyCollection()
    {
        // Arrange
        var options = new[]
        {
            new Option<int>.None(),
            new Option<int>.None(),
            new Option<int>.None()
        };

        // Act
        var result = options.CollectSome();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void CollectSome_AllSome_ReturnsAllValues()
    {
        // Arrange
        var options = new[]
        {
            new Option<int>.Some(1),
            new Option<int>.Some(2),
            new Option<int>.Some(3)
        };

        // Act
        var result = options.CollectSome();

        // Assert
        Assert.Equal(new[] { 1, 2, 3 }, result);
    }

    [Fact]
    public void CollectSome_PreservesOrder()
    {
        // Arrange
        Option<string>[] options =
        {
            new Option<string>.Some("c"),
            new Option<string>.None(),
            new Option<string>.Some("a"),
            new Option<string>.Some("b")
        };

        // Act
        var result = options.CollectSome().ToList();

        // Assert
        Assert.Equal(new[] { "c", "a", "b" }, result);
    }

    #endregion

    #region Option PartitionOptions Tests

    [Fact]
    public void PartitionOptions_MixedOptions_PartitionsCorrectly()
    {
        // Arrange
        Option<int>[] options =
        {
            new Option<int>.Some(1),
            new Option<int>.None(),
            new Option<int>.Some(3),
            new Option<int>.None(),
            new Option<int>.Some(5)
        };

        // Act
        var (values, noneCount) = options.PartitionOptions();

        // Assert
        Assert.Equal(new[] { 1, 3, 5 }, values.ToArray());
        Assert.Equal(2, noneCount);
    }

    [Fact]
    public void PartitionOptions_AllSome_ReturnsAllValuesZeroNone()
    {
        // Arrange
        var options = new[]
        {
            new Option<int>.Some(1),
            new Option<int>.Some(2),
            new Option<int>.Some(3)
        };

        // Act
        var (values, noneCount) = options.PartitionOptions();

        // Assert
        Assert.Equal(new[] { 1, 2, 3 }, values);
        Assert.Equal(0, noneCount);
    }

    [Fact]
    public void PartitionOptions_AllNone_ReturnsEmptyValuesWithCount()
    {
        // Arrange
        var options = new[]
        {
            new Option<int>.None(),
            new Option<int>.None(),
            new Option<int>.None()
        };

        // Act
        var (values, noneCount) = options.PartitionOptions();

        // Assert
        Assert.Empty(values);
        Assert.Equal(3, noneCount);
    }

    #endregion

    #region Result Sequence Tests

    [Fact]
    public void Sequence_Result_AllOk_ReturnsOkWithAllValues()
    {
        // Arrange
        var results = new[]
        {
            Result<int, string>.Ok(1),
            Result<int, string>.Ok(2),
            Result<int, string>.Ok(3)
        };

        // Act
        var combined = results.Sequence();

        // Assert
        Assert.True(combined.IsSuccess);
        Assert.True(combined.TryGetValue(out var values));
        Assert.Equal(new[] { 1, 2, 3 }, values);
    }

    [Fact]
    public void Sequence_Result_WithError_ReturnsFirstError()
    {
        // Arrange
        var results = new[]
        {
            Result<int, string>.Ok(1),
            Result<int, string>.Err("error1"),
            Result<int, string>.Err("error2")
        };

        // Act
        var combined = results.Sequence();

        // Assert
        Assert.True(combined.IsFailure);
        Assert.True(combined.TryGetError(out var error));
        Assert.Equal("error1", error);
    }

    [Fact]
    public void Sequence_Result_EmptyCollection_ReturnsOkEmptyCollection()
    {
        // Arrange
        var results = Array.Empty<Result<int, string>>();

        // Act
        var combined = results.Sequence();

        // Assert
        Assert.True(combined.IsSuccess);
        Assert.True(combined.TryGetValue(out var values));
        Assert.Empty(values);
    }

    [Fact]
    public void Sequence_Result_PreservesOrder()
    {
        // Arrange
        var results = new[]
        {
            Result<string, int>.Ok("first"),
            Result<string, int>.Ok("second"),
            Result<string, int>.Ok("third")
        };

        // Act
        var combined = results.Sequence();

        // Assert
        Assert.True(combined.IsSuccess);
        Assert.True(combined.TryGetValue(out var values));
        Assert.Equal(new[] { "first", "second", "third" }, values);
    }

    #endregion

    #region Result Traverse Tests

    [Fact]
    public void Traverse_Result_AllSucceed_ReturnsOkWithAllValues()
    {
        // Arrange
        var strings = new[] { "1", "2", "3" };

        // Act
        var result = strings.Traverse<string, int, string>(s =>
            int.TryParse(s, out var n)
                ? Result<int, string>.Ok(n)
                : Result<int, string>.Err($"Invalid: {s}"));

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.TryGetValue(out var values));
        Assert.Equal(new[] { 1, 2, 3 }, values);
    }

    [Fact]
    public void Traverse_Result_WithFailure_ReturnsFirstError()
    {
        // Arrange
        var strings = new[] { "1", "invalid", "3" };

        // Act
        var result = strings.Traverse<string, int, string>(s =>
            int.TryParse(s, out var n)
                ? Result<int, string>.Ok(n)
                : Result<int, string>.Err($"Invalid: {s}"));

        // Assert
        Assert.True(result.IsFailure);
        Assert.True(result.TryGetError(out var error));
        Assert.Equal("Invalid: invalid", error);
    }

    [Fact]
    public void Traverse_Result_EmptyCollection_ReturnsOkEmptyCollection()
    {
        // Arrange
        var strings = Array.Empty<string>();

        // Act
        var result = strings.Traverse<string, int, string>(s =>
            Result<int, string>.Ok(s.Length));

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.TryGetValue(out var values));
        Assert.Empty(values);
    }

    [Fact]
    public void Traverse_Result_WithValidation_WorksCorrectly()
    {
        // Arrange
        var ages = new[] { 25, 30, 150 }; // 150 is invalid

        // Act
        var result = ages.Traverse<int, int, string>(age =>
            age >= 0 && age <= 120
                ? Result<int, string>.Ok(age)
                : Result<int, string>.Err($"Invalid age: {age}"));

        // Assert
        Assert.True(result.IsFailure);
        Assert.True(result.TryGetError(out var error));
        Assert.Equal("Invalid age: 150", error);
    }

    #endregion

    #region Result CollectOk Tests

    [Fact]
    public void CollectOk_MixedResults_ReturnsOnlyOkValues()
    {
        // Arrange
        var results = new[]
        {
            Result<int, string>.Ok(1),
            Result<int, string>.Err("error1"),
            Result<int, string>.Ok(3),
            Result<int, string>.Err("error2"),
            Result<int, string>.Ok(5)
        };

        // Act
        var values = results.CollectOk();

        // Assert
        Assert.Equal(new[] { 1, 3, 5 }, values);
    }

    [Fact]
    public void CollectOk_AllErr_ReturnsEmptyCollection()
    {
        // Arrange
        var results = new[]
        {
            Result<int, string>.Err("error1"),
            Result<int, string>.Err("error2"),
            Result<int, string>.Err("error3")
        };

        // Act
        var values = results.CollectOk();

        // Assert
        Assert.Empty(values);
    }

    [Fact]
    public void CollectOk_AllOk_ReturnsAllValues()
    {
        // Arrange
        var results = new[]
        {
            Result<int, string>.Ok(1),
            Result<int, string>.Ok(2),
            Result<int, string>.Ok(3)
        };

        // Act
        var values = results.CollectOk();

        // Assert
        Assert.Equal(new[] { 1, 2, 3 }, values);
    }

    #endregion

    #region Result CollectErr Tests

    [Fact]
    public void CollectErr_MixedResults_ReturnsOnlyErrors()
    {
        // Arrange
        var results = new[]
        {
            Result<int, string>.Ok(1),
            Result<int, string>.Err("error1"),
            Result<int, string>.Ok(3),
            Result<int, string>.Err("error2"),
            Result<int, string>.Ok(5)
        };

        // Act
        var errors = results.CollectErr();

        // Assert
        Assert.Equal(new[] { "error1", "error2" }, errors);
    }

    [Fact]
    public void CollectErr_AllOk_ReturnsEmptyCollection()
    {
        // Arrange
        var results = new[]
        {
            Result<int, string>.Ok(1),
            Result<int, string>.Ok(2),
            Result<int, string>.Ok(3)
        };

        // Act
        var errors = results.CollectErr();

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void CollectErr_AllErr_ReturnsAllErrors()
    {
        // Arrange
        var results = new[]
        {
            Result<int, string>.Err("error1"),
            Result<int, string>.Err("error2"),
            Result<int, string>.Err("error3")
        };

        // Act
        var errors = results.CollectErr();

        // Assert
        Assert.Equal(new[] { "error1", "error2", "error3" }, errors);
    }

    #endregion

    #region Result PartitionResults Tests

    [Fact]
    public void PartitionResults_MixedResults_PartitionsCorrectly()
    {
        // Arrange
        var results = new[]
        {
            Result<int, string>.Ok(1),
            Result<int, string>.Err("error1"),
            Result<int, string>.Ok(3),
            Result<int, string>.Err("error2"),
            Result<int, string>.Ok(5)
        };

        // Act
        var (successes, failures) = results.PartitionResults();

        // Assert
        Assert.Equal(new[] { 1, 3, 5 }, successes);
        Assert.Equal(new[] { "error1", "error2" }, failures);
    }

    [Fact]
    public void PartitionResults_AllOk_ReturnsAllSuccessesNoFailures()
    {
        // Arrange
        var results = new[]
        {
            Result<int, string>.Ok(1),
            Result<int, string>.Ok(2),
            Result<int, string>.Ok(3)
        };

        // Act
        var (successes, failures) = results.PartitionResults();

        // Assert
        Assert.Equal(new[] { 1, 2, 3 }, successes);
        Assert.Empty(failures);
    }

    [Fact]
    public void PartitionResults_AllErr_ReturnsNoSuccessesAllFailures()
    {
        // Arrange
        var results = new[]
        {
            Result<int, string>.Err("error1"),
            Result<int, string>.Err("error2"),
            Result<int, string>.Err("error3")
        };

        // Act
        var (successes, failures) = results.PartitionResults();

        // Assert
        Assert.Empty(successes);
        Assert.Equal(new[] { "error1", "error2", "error3" }, failures);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void Integration_Option_ValidateAllUsers_WorksCorrectly()
    {
        // Arrange
        var userIds = new[] { 1, 2, 3, 4, 5 };

        // Simulate looking up users - 3 doesn't exist
        Option<string> GetUser(int id) =>
            id != 3
                ? new Option<string>.Some($"User{id}")
                : new Option<string>.None();

        // Act
        var result = userIds.Traverse(GetUser);

        // Assert - Should fail because user 3 doesn't exist
        Assert.True(result.IsNone());
    }

    [Fact]
    public void Integration_Result_ParseAndValidate_WorksCorrectly()
    {
        // Arrange
        var inputs = new[] { "10", "20", "30" };

        // Act
        var traversed = inputs.Traverse<string, int, string>(s =>
                int.TryParse(s, out var n)
                    ? Result<int, string>.Ok(n)
                    : Result<int, string>.Err($"Parse error: {s}"));

        Result<IEnumerable<int>, string> result;
        if (traversed.TryGetValue(out var values))
        {
            var sum = values.Sum();
            result = sum <= 100
                ? Result<IEnumerable<int>, string>.Ok(values)
                : Result<IEnumerable<int>, string>.Err($"Sum too large: {sum}");
        }
        else
        {
            result = traversed;
        }

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.TryGetValue(out var finalValues));
        Assert.Equal(60, finalValues.Sum());
    }

    [Fact]
    public void Integration_Result_CollectErrors_ForReporting()
    {
        // Arrange
        var inputs = new[] { "10", "invalid1", "20", "invalid2", "30" };

        // Act
        var results = inputs.Select(s =>
            int.TryParse(s, out var n)
                ? Result<int, string>.Ok(n)
                : Result<int, string>.Err($"Invalid: {s}"));

        var (successes, failures) = results.PartitionResults();

        // Assert
        Assert.Equal(new[] { 10, 20, 30 }, successes);
        Assert.Equal(new[] { "Invalid: invalid1", "Invalid: invalid2" }, failures);
    }

    [Fact]
    public void Integration_Option_ChainWithSequence_WorksCorrectly()
    {
        // Arrange
        var values = new[] { 1, 2, 3, 4, 5 };

        // Act - Square only even numbers
        var result = values.Traverse<int, int>(n => n % 2 == 0
                ? new Option<int>.Some(n * n)
                : new Option<int>.None());

        // Assert - Should fail because not all are even
        Assert.True(result.IsNone());

        // Act - Try with only even numbers
        var evenOnly = new[] { 2, 4, 6 };
        var result2 = evenOnly.Traverse<int, int>(n => new Option<int>.Some(n * n));

        // Assert
        Assert.True(result2.IsSome());
        if (result2 is Option<IEnumerable<int>>.Some some)
        {
            Assert.Equal(new[] { 4, 16, 36 }, some.Value);
        }
    }

    [Fact]
    public void Integration_Result_ValidationPipeline_WorksCorrectly()
    {
        // Arrange
        var ages = new[] { "25", "30", "35" };

        // Act - Parse, validate, and transform
        var result = ages
            .Traverse<string, int, string>(s =>
                int.TryParse(s, out var n)
                    ? Result<int, string>.Ok(n)
                    : Result<int, string>.Err($"Parse error: {s}"))
            .Map<IEnumerable<int>, string, IEnumerable<int>>(values =>
                values.Where(age => age >= 18 && age <= 120))
            .Map<IEnumerable<int>, string, IEnumerable<string>>(validAges =>
                validAges.Select(age => $"Age: {age}"));

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.TryGetValue(out var descriptions));
        Assert.Equal(new[] { "Age: 25", "Age: 30", "Age: 35" }, descriptions);
    }

    #endregion
}
