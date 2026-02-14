using Esox.SharpAndRusty.Types;
using Esox.SharpAndRusty.Extensions;

namespace Esox.SharpAndRust.Tests.Extensions;

/// <summary>
/// Tests for enhanced collection extensions: Either, Validation, Utility, and Dictionary extensions.
/// </summary>
public class CollectionExtensionsEnhancedTests
{
    #region Either Sequence Tests

    [Fact]
    public void SequenceLeft_AllLeft_ReturnsLeftWithAllValues()
    {
        // Arrange
        var eithers = new Either<int, string>[]
        {
            new Either<int, string>.Left(1),
            new Either<int, string>.Left(2),
            new Either<int, string>.Left(3)
        };

        // Act
        var result = eithers.SequenceLeft();

        // Assert
        Assert.True(result is Either<IEnumerable<int>, string>.Left);
        if (result is Either<IEnumerable<int>, string>.Left left)
        {
            Assert.Equal(new[] { 1, 2, 3 }, left.Value);
        }
    }

    [Fact]
    public void SequenceLeft_WithRight_ReturnsFirstRight()
    {
        // Arrange
        var eithers = new Either<int, string>[]
        {
            new Either<int, string>.Left(1),
            new Either<int, string>.Right("error1"),
            new Either<int, string>.Left(3),
            new Either<int, string>.Right("error2")
        };

        // Act
        var result = eithers.SequenceLeft();

        // Assert
        Assert.True(result is Either<IEnumerable<int>, string>.Right);
        if (result is Either<IEnumerable<int>, string>.Right right)
        {
            Assert.Equal("error1", right.Value);
        }
    }

    [Fact]
    public void SequenceLeft_EmptyCollection_ReturnsEmptyLeft()
    {
        // Arrange
        var eithers = Array.Empty<Either<int, string>>();

        // Act
        var result = eithers.SequenceLeft();

        // Assert
        Assert.True(result is Either<IEnumerable<int>, string>.Left);
        if (result is Either<IEnumerable<int>, string>.Left left)
        {
            Assert.Empty(left.Value);
        }
    }

    [Fact]
    public void SequenceRight_AllRight_ReturnsRightWithAllValues()
    {
        // Arrange
        var eithers = new Either<string, int>[]
        {
            new Either<string, int>.Right(1),
            new Either<string, int>.Right(2),
            new Either<string, int>.Right(3)
        };

        // Act
        var result = eithers.SequenceRight();

        // Assert
        Assert.True(result is Either<string, IEnumerable<int>>.Right);
        if (result is Either<string, IEnumerable<int>>.Right right)
        {
            Assert.Equal(new[] { 1, 2, 3 }, right.Value);
        }
    }

    [Fact]
    public void SequenceRight_WithLeft_ReturnsFirstLeft()
    {
        // Arrange
        var eithers = new Either<string, int>[]
        {
            new Either<string, int>.Right(1),
            new Either<string, int>.Left("error1"),
            new Either<string, int>.Right(3),
            new Either<string, int>.Left("error2")
        };

        // Act
        var result = eithers.SequenceRight();

        // Assert
        Assert.True(result is Either<string, IEnumerable<int>>.Left);
        if (result is Either<string, IEnumerable<int>>.Left left)
        {
            Assert.Equal("error1", left.Value);
        }
    }

    [Fact]
    public void SequenceRight_EmptyCollection_ReturnsEmptyRight()
    {
        // Arrange
        var eithers = Array.Empty<Either<string, int>>();

        // Act
        var result = eithers.SequenceRight();

        // Assert
        Assert.True(result is Either<string, IEnumerable<int>>.Right);
        if (result is Either<string, IEnumerable<int>>.Right right)
        {
            Assert.Empty(right.Value);
        }
    }

    #endregion

    #region Either Traverse Tests

    [Fact]
    public void TraverseLeft_AllSucceed_ReturnsLeftWithAllValues()
    {
        // Arrange
        var numbers = new[] { "1", "2", "3" };

        // Act
        var result = numbers.TraverseLeft<string, int, string>(s =>
            int.TryParse(s, out var n)
                ? new Either<int, string>.Left(n)
                : new Either<int, string>.Right($"Invalid: {s}")
        );

        // Assert
        Assert.True(result is Either<IEnumerable<int>, string>.Left);
        if (result is Either<IEnumerable<int>, string>.Left left)
        {
            Assert.Equal(new[] { 1, 2, 3 }, left.Value);
        }
    }

    [Fact]
    public void TraverseLeft_SomeFail_ReturnsFirstRight()
    {
        // Arrange
        var numbers = new[] { "1", "invalid", "3", "bad" };

        // Act
        var result = numbers.TraverseLeft<string, int, string>(s =>
            int.TryParse(s, out var n)
                ? new Either<int, string>.Left(n)
                : new Either<int, string>.Right($"Invalid: {s}")
        );

        // Assert
        Assert.True(result is Either<IEnumerable<int>, string>.Right);
        if (result is Either<IEnumerable<int>, string>.Right right)
        {
            Assert.Equal("Invalid: invalid", right.Value);
        }
    }

    [Fact]
    public void TraverseLeft_EmptyCollection_ReturnsEmptyLeft()
    {
        // Arrange
        var numbers = Array.Empty<string>();

        // Act
        var result = numbers.TraverseLeft<string, int, string>(s =>
            int.TryParse(s, out var n)
                ? new Either<int, string>.Left(n)
                : new Either<int, string>.Right($"Invalid: {s}")
        );

        // Assert
        Assert.True(result is Either<IEnumerable<int>, string>.Left);
        if (result is Either<IEnumerable<int>, string>.Left left)
        {
            Assert.Empty(left.Value);
        }
    }

    [Fact]
    public void TraverseRight_AllSucceed_ReturnsRightWithAllValues()
    {
        // Arrange
        var numbers = new[] { "1", "2", "3" };

        // Act
        var result = numbers.TraverseRight<string, string, int>(s =>
            int.TryParse(s, out var n)
                ? new Either<string, int>.Right(n)
                : new Either<string, int>.Left($"Invalid: {s}")
        );

        // Assert
        Assert.True(result is Either<string, IEnumerable<int>>.Right);
        if (result is Either<string, IEnumerable<int>>.Right right)
        {
            Assert.Equal(new[] { 1, 2, 3 }, right.Value);
        }
    }

    [Fact]
    public void TraverseRight_SomeFail_ReturnsFirstLeft()
    {
        // Arrange
        var numbers = new[] { "1", "invalid", "3", "bad" };

        // Act
        var result = numbers.TraverseRight<string, string, int>(s =>
            int.TryParse(s, out var n)
                ? new Either<string, int>.Right(n)
                : new Either<string, int>.Left($"Invalid: {s}")
        );

        // Assert
        Assert.True(result is Either<string, IEnumerable<int>>.Left);
        if (result is Either<string, IEnumerable<int>>.Left left)
        {
            Assert.Equal("Invalid: invalid", left.Value);
        }
    }

    [Fact]
    public void TraverseRight_EmptyCollection_ReturnsEmptyRight()
    {
        // Arrange
        var numbers = Array.Empty<string>();

        // Act
        var result = numbers.TraverseRight<string, string, int>(s =>
            int.TryParse(s, out var n)
                ? new Either<string, int>.Right(n)
                : new Either<string, int>.Left($"Invalid: {s}")
        );

        // Assert
        Assert.True(result is Either<string, IEnumerable<int>>.Right);
        if (result is Either<string, IEnumerable<int>>.Right right)
        {
            Assert.Empty(right.Value);
        }
    }

    #endregion

    #region Validation Traverse Tests

    [Fact]
    public void TraverseValidation_AllValid_ReturnsValidWithAllValues()
    {
        // Arrange
        var inputs = new[] { "1", "2", "3" };

        // Act
        var result = inputs.TraverseValidation<string, int, string>(s =>
            int.TryParse(s, out var n)
                ? Validation<int, string>.Valid(n)
                : Validation<int, string>.Invalid($"Invalid: {s}")
        );

        // Assert
        Assert.True(result.IsSuccess);
        if (result.TryGetValue(out var values))
        {
            Assert.Equal(new[] { 1, 2, 3 }, values);
        }
    }

    [Fact]
    public void TraverseValidation_SomeInvalid_AccumulatesAllErrors()
    {
        // Arrange
        var inputs = new[] { "1", "invalid", "3", "bad" };

        // Act
        var result = inputs.TraverseValidation<string, int, string>(s =>
            int.TryParse(s, out var n)
                ? Validation<int, string>.Valid(n)
                : Validation<int, string>.Invalid($"Invalid: {s}")
        );

        // Assert
        Assert.True(result.IsFailure);
        if (result.TryGetErrors(out var errors))
        {
            Assert.Equal(2, errors.Count);
            Assert.Contains("Invalid: invalid", errors);
            Assert.Contains("Invalid: bad", errors);
        }
    }

    [Fact]
    public void TraverseValidation_EmptyCollection_ReturnsEmptyValid()
    {
        // Arrange
        var inputs = Array.Empty<string>();

        // Act
        var result = inputs.TraverseValidation<string, int, string>(s =>
            int.TryParse(s, out var n)
                ? Validation<int, string>.Valid(n)
                : Validation<int, string>.Invalid($"Invalid: {s}")
        );

        // Assert
        Assert.True(result.IsSuccess);
        if (result.TryGetValue(out var values))
        {
            Assert.Empty(values);
        }
    }

    [Fact]
    public void TraverseValidation_MultipleErrorsPerItem_AccumulatesAll()
    {
        // Arrange
        var inputs = new[] { "1", "bad", "3" };

        // Act
        var result = inputs.TraverseValidation<string, int, string>(s =>
            int.TryParse(s, out var n)
                ? Validation<int, string>.Valid(n)
                : Validation<int, string>.Invalid(new[] { $"Parse error: {s}", $"Format error: {s}" })
        );

        // Assert
        Assert.True(result.IsFailure);
        if (result.TryGetErrors(out var errors))
        {
            Assert.Equal(2, errors.Count);
            Assert.Contains("Parse error: bad", errors);
            Assert.Contains("Format error: bad", errors);
        }
    }

    #endregion

    #region Validation Partition Tests

    [Fact]
    public void PartitionValidations_MixedValidations_PartitionsCorrectly()
    {
        // Arrange
        var validations = new[]
        {
            Validation<int, string>.Valid(1),
            Validation<int, string>.Invalid(new[] { "error1", "error2" }),
            Validation<int, string>.Valid(3),
            Validation<int, string>.Invalid("error3")
        };

        // Act
        var (valid, invalid) = validations.PartitionValidations();

        // Assert
        Assert.Equal(new[] { 1, 3 }, valid);
        Assert.Equal(2, invalid.Count);
        Assert.Equal(2, invalid[0].Count);
        Assert.Contains("error1", invalid[0]);
        Assert.Contains("error2", invalid[0]);
        Assert.Single(invalid[1]);
        Assert.Contains("error3", invalid[1]);
    }

    [Fact]
    public void PartitionValidations_AllValid_ReturnsAllValid()
    {
        // Arrange
        var validations = new[]
        {
            Validation<int, string>.Valid(1),
            Validation<int, string>.Valid(2),
            Validation<int, string>.Valid(3)
        };

        // Act
        var (valid, invalid) = validations.PartitionValidations();

        // Assert
        Assert.Equal(new[] { 1, 2, 3 }, valid);
        Assert.Empty(invalid);
    }

    [Fact]
    public void PartitionValidations_AllInvalid_ReturnsAllInvalid()
    {
        // Arrange
        var validations = new[]
        {
            Validation<int, string>.Invalid("error1"),
            Validation<int, string>.Invalid(new[] { "error2", "error3" }),
            Validation<int, string>.Invalid("error4")
        };

        // Act
        var (valid, invalid) = validations.PartitionValidations();

        // Assert
        Assert.Empty(valid);
        Assert.Equal(3, invalid.Count);
    }

    [Fact]
    public void PartitionValidations_EmptyCollection_ReturnsEmptyBoth()
    {
        // Arrange
        var validations = Array.Empty<Validation<int, string>>();

        // Act
        var (valid, invalid) = validations.PartitionValidations();

        // Assert
        Assert.Empty(valid);
        Assert.Empty(invalid);
    }

    #endregion

    #region FirstOk Tests

    [Fact]
    public void FirstOk_WithOk_ReturnsFirstOk()
    {
        // Arrange
        var results = new[]
        {
            Result<int, string>.Err("error1"),
            Result<int, string>.Ok(42),
            Result<int, string>.Err("error2"),
            Result<int, string>.Ok(99)
        };

        // Act
        var result = results.FirstOk();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.TryGetValue(out var value));
        Assert.Equal(42, value);
    }

    [Fact]
    public void FirstOk_AllErr_ReturnsErrWithAllErrors()
    {
        // Arrange
        var results = new[]
        {
            Result<int, string>.Err("error1"),
            Result<int, string>.Err("error2"),
            Result<int, string>.Err("error3")
        };

        // Act
        var result = results.FirstOk();

        // Assert
        Assert.True(result.IsFailure);
        Assert.True(result.TryGetError(out var errors));
        Assert.Equal(new[] { "error1", "error2", "error3" }, errors);
    }

    [Fact]
    public void FirstOk_EmptyCollection_ReturnsErrWithEmptyErrors()
    {
        // Arrange
        var results = Array.Empty<Result<int, string>>();

        // Act
        var result = results.FirstOk();

        // Assert
        Assert.True(result.IsFailure);
        Assert.True(result.TryGetError(out var errors));
        Assert.Empty(errors);
    }

    [Fact]
    public void FirstOk_FirstIsOk_ShortCircuits()
    {
        // Arrange
        var callCount = 0;
        var results = Enumerable.Range(0, 1000).Select(i =>
        {
            callCount++;
            return i == 0 ? Result<int, string>.Ok(42) : Result<int, string>.Err($"error{i}");
        });

        // Act
        var result = results.FirstOk();

        // Assert
        Assert.Equal(1, callCount); // Should short-circuit after first item
        Assert.True(result.IsSuccess);
    }

    #endregion

    #region FirstSome Tests

    [Fact]
    public void FirstSome_WithSome_ReturnsFirstSome()
    {
        // Arrange
        Option<int>[] options =
        {
            new Option<int>.None(),
            new Option<int>.Some(42),
            new Option<int>.None(),
            new Option<int>.Some(99)
        };

        // Act
        var result = options.FirstSome();

        // Assert
        Assert.True(result is Option<int>.Some);
        if (result is Option<int>.Some some)
        {
            Assert.Equal(42, some.Value);
        }
    }

    [Fact]
    public void FirstSome_AllNone_ReturnsNone()
    {
        // Arrange
        Option<int>[] options =
        {
            new Option<int>.None(),
            new Option<int>.None(),
            new Option<int>.None()
        };

        // Act
        var result = options.FirstSome();

        // Assert
        Assert.True(result is Option<int>.None);
    }

    [Fact]
    public void FirstSome_EmptyCollection_ReturnsNone()
    {
        // Arrange
        var options = Array.Empty<Option<int>>();

        // Act
        var result = options.FirstSome();

        // Assert
        Assert.True(result is Option<int>.None);
    }

    [Fact]
    public void FirstSome_FirstIsSome_ShortCircuits()
    {
        // Arrange
        var callCount = 0;
        var options = Enumerable.Range(0, 1000).Select<int, Option<int>>(i =>
        {
            callCount++;
            return i == 0 ? new Option<int>.Some(42) : new Option<int>.None();
        });

        // Act
        var result = options.FirstSome();

        // Assert
        Assert.Equal(1, callCount); // Should short-circuit after first item
        Assert.True(result is Option<int>.Some);
    }

    #endregion

    #region Choose Tests

    [Fact]
    public void Choose_FindsFirstValidTransformation()
    {
        // Arrange
        var strings = new[] { "invalid", "42", "99", "bad" };

        // Act
        var result = strings.Choose<string, int>(s =>
            int.TryParse(s, out var n)
                ? new Option<int>.Some(n)
                : new Option<int>.None()
        );

        // Assert
        Assert.True(result is Option<int>.Some);
        if (result is Option<int>.Some some)
        {
            Assert.Equal(42, some.Value);
        }
    }

    [Fact]
    public void Choose_NoValidTransformation_ReturnsNone()
    {
        // Arrange
        var strings = new[] { "invalid", "bad", "wrong" };

        // Act
        var result = strings.Choose<string, int>(s =>
            int.TryParse(s, out var n)
                ? new Option<int>.Some(n)
                : new Option<int>.None()
        );

        // Assert
        Assert.True(result is Option<int>.None);
    }

    [Fact]
    public void Choose_EmptyCollection_ReturnsNone()
    {
        // Arrange
        var strings = Array.Empty<string>();

        // Act
        var result = strings.Choose<string, int>(s =>
            int.TryParse(s, out var n)
                ? new Option<int>.Some(n)
                : new Option<int>.None()
        );

        // Assert
        Assert.True(result is Option<int>.None);
    }

    [Fact]
    public void Choose_ShortCircuitsOnFirstSome()
    {
        // Arrange
        var callCount = 0;
        var strings = Enumerable.Range(0, 1000).Select(i =>
        {
            callCount++;
            return i == 5 ? "42" : "invalid";
        });

        // Act
        var result = strings.Choose<string, int>(s =>
            int.TryParse(s, out var n)
                ? new Option<int>.Some(n)
                : new Option<int>.None()
        );

        // Assert
        Assert.Equal(6, callCount); // Should stop after finding first valid (index 5)
        Assert.True(result is Option<int>.Some);
    }

    #endregion

    #region SequenceAll Tests

    [Fact]
    public void SequenceAll_AllOk_ReturnsOkWithAllValues()
    {
        // Arrange
        var results = new[]
        {
            Result<int, string>.Ok(1),
            Result<int, string>.Ok(2),
            Result<int, string>.Ok(3)
        };

        // Act
        var result = results.SequenceAll();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.TryGetValue(out var values));
        Assert.Equal(new[] { 1, 2, 3 }, values);
    }

    [Fact]
    public void SequenceAll_SomeErr_AccumulatesAllErrors()
    {
        // Arrange
        var results = new[]
        {
            Result<int, string>.Ok(1),
            Result<int, string>.Err("error1"),
            Result<int, string>.Ok(3),
            Result<int, string>.Err("error2")
        };

        // Act
        var result = results.SequenceAll();

        // Assert
        Assert.True(result.IsFailure);
        Assert.True(result.TryGetError(out var errors));
        Assert.Equal(new[] { "error1", "error2" }, errors);
    }

    [Fact]
    public void SequenceAll_ComparedWithSequence_DifferentBehavior()
    {
        // Arrange
        var results = new[]
        {
            Result<int, string>.Ok(1),
            Result<int, string>.Err("error1"),
            Result<int, string>.Ok(3),
            Result<int, string>.Err("error2")
        };

        // Act
        var sequenceAll = results.SequenceAll();
        var sequence = results.Sequence();

        // Assert - SequenceAll accumulates all errors
        Assert.True(sequenceAll.IsFailure);
        Assert.True(sequenceAll.TryGetError(out var allErrors));
        Assert.Equal(2, allErrors.Count());
        Assert.Contains("error1", allErrors);
        Assert.Contains("error2", allErrors);

        // Assert - Sequence short-circuits on first error
        Assert.True(sequence.IsFailure);
        Assert.True(sequence.TryGetError(out var firstError));
        Assert.Equal("error1", firstError);
    }

    [Fact]
    public void SequenceAll_EmptyCollection_ReturnsEmptyOk()
    {
        // Arrange
        var results = Array.Empty<Result<int, string>>();

        // Act
        var result = results.SequenceAll();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.TryGetValue(out var values));
        Assert.Empty(values);
    }

    #endregion

    #region AnyOk/AllOk Tests

    [Fact]
    public void AnyOk_WithOk_ReturnsTrue()
    {
        // Arrange
        var results = new[]
        {
            Result<int, string>.Err("error1"),
            Result<int, string>.Ok(42),
            Result<int, string>.Err("error2")
        };

        // Act
        var hasOk = results.AnyOk();

        // Assert
        Assert.True(hasOk);
    }

    [Fact]
    public void AnyOk_AllErr_ReturnsFalse()
    {
        // Arrange
        var results = new[]
        {
            Result<int, string>.Err("error1"),
            Result<int, string>.Err("error2")
        };

        // Act
        var hasOk = results.AnyOk();

        // Assert
        Assert.False(hasOk);
    }

    [Fact]
    public void AnyOk_EmptyCollection_ReturnsFalse()
    {
        // Arrange
        var results = Array.Empty<Result<int, string>>();

        // Act
        var hasOk = results.AnyOk();

        // Assert
        Assert.False(hasOk);
    }

    [Fact]
    public void AllOk_AllOk_ReturnsTrue()
    {
        // Arrange
        var results = new[]
        {
            Result<int, string>.Ok(1),
            Result<int, string>.Ok(2),
            Result<int, string>.Ok(3)
        };

        // Act
        var allOk = results.AllOk();

        // Assert
        Assert.True(allOk);
    }

    [Fact]
    public void AllOk_WithErr_ReturnsFalse()
    {
        // Arrange
        var results = new[]
        {
            Result<int, string>.Ok(1),
            Result<int, string>.Err("error"),
            Result<int, string>.Ok(3)
        };

        // Act
        var allOk = results.AllOk();

        // Assert
        Assert.False(allOk);
    }

    [Fact]
    public void AllOk_EmptyCollection_ReturnsTrue()
    {
        // Arrange
        var results = Array.Empty<Result<int, string>>();

        // Act
        var allOk = results.AllOk();

        // Assert
        Assert.True(allOk); // Empty collection vacuously satisfies "all"
    }

    #endregion

    #region AnySome/AllSome Tests

    [Fact]
    public void AnySome_WithSome_ReturnsTrue()
    {
        // Arrange
        Option<int>[] options =
        {
            new Option<int>.None(),
            new Option<int>.Some(42),
            new Option<int>.None()
        };

        // Act
        var hasSome = options.AnySome();

        // Assert
        Assert.True(hasSome);
    }

    [Fact]
    public void AnySome_AllNone_ReturnsFalse()
    {
        // Arrange
        Option<int>[] options =
        {
            new Option<int>.None(),
            new Option<int>.None()
        };

        // Act
        var hasSome = options.AnySome();

        // Assert
        Assert.False(hasSome);
    }

    [Fact]
    public void AnySome_EmptyCollection_ReturnsFalse()
    {
        // Arrange
        var options = Array.Empty<Option<int>>();

        // Act
        var hasSome = options.AnySome();

        // Assert
        Assert.False(hasSome);
    }

    [Fact]
    public void AllSome_AllSome_ReturnsTrue()
    {
        // Arrange
        var options = new[]
        {
            new Option<int>.Some(1),
            new Option<int>.Some(2),
            new Option<int>.Some(3)
        };

        // Act
        var allSome = options.AllSome();

        // Assert
        Assert.True(allSome);
    }

    [Fact]
    public void AllSome_WithNone_ReturnsFalse()
    {
        // Arrange
        Option<int>[] options =
        {
            new Option<int>.Some(1),
            new Option<int>.None(),
            new Option<int>.Some(3)
        };

        // Act
        var allSome = options.AllSome();

        // Assert
        Assert.False(allSome);
    }

    [Fact]
    public void AllSome_EmptyCollection_ReturnsTrue()
    {
        // Arrange
        var options = Array.Empty<Option<int>>();

        // Act
        var allSome = options.AllSome();

        // Assert
        Assert.True(allSome); // Empty collection vacuously satisfies "all"
    }

    #endregion

    #region Dictionary Extension Tests

    [Fact]
    public void ToOkDictionary_OnlyOk_ReturnsAllPairs()
    {
        // Arrange
        var results = new[]
        {
            Result<KeyValuePair<string, int>, string>.Ok(new KeyValuePair<string, int>("a", 1)),
            Result<KeyValuePair<string, int>, string>.Ok(new KeyValuePair<string, int>("b", 2)),
            Result<KeyValuePair<string, int>, string>.Ok(new KeyValuePair<string, int>("c", 3))
        };

        // Act
        var dict = results.ToOkDictionary();

        // Assert
        Assert.Equal(3, dict.Count);
        Assert.Equal(1, dict["a"]);
        Assert.Equal(2, dict["b"]);
        Assert.Equal(3, dict["c"]);
    }

    [Fact]
    public void ToOkDictionary_MixedResults_ReturnsOnlyOkPairs()
    {
        // Arrange
        var results = new[]
        {
            Result<KeyValuePair<string, int>, string>.Ok(new KeyValuePair<string, int>("a", 1)),
            Result<KeyValuePair<string, int>, string>.Err("error1"),
            Result<KeyValuePair<string, int>, string>.Ok(new KeyValuePair<string, int>("b", 2)),
            Result<KeyValuePair<string, int>, string>.Err("error2")
        };

        // Act
        var dict = results.ToOkDictionary();

        // Assert
        Assert.Equal(2, dict.Count);
        Assert.Equal(1, dict["a"]);
        Assert.Equal(2, dict["b"]);
    }

    [Fact]
    public void ToOkDictionary_DuplicateKeys_KeepsFirstOccurrence()
    {
        // Arrange
        var results = new[]
        {
            Result<KeyValuePair<string, int>, string>.Ok(new KeyValuePair<string, int>("a", 1)),
            Result<KeyValuePair<string, int>, string>.Ok(new KeyValuePair<string, int>("a", 999)),
            Result<KeyValuePair<string, int>, string>.Ok(new KeyValuePair<string, int>("b", 2))
        };

        // Act
        var dict = results.ToOkDictionary();

        // Assert
        Assert.Equal(2, dict.Count);
        Assert.Equal(1, dict["a"]); // First occurrence kept
        Assert.Equal(2, dict["b"]);
    }

    [Fact]
    public void ToOkDictionary_EmptyCollection_ReturnsEmptyDictionary()
    {
        // Arrange
        var results = Array.Empty<Result<KeyValuePair<string, int>, string>>();

        // Act
        var dict = results.ToOkDictionary();

        // Assert
        Assert.Empty(dict);
    }

    [Fact]
    public void ToSomeDictionary_OnlySome_ReturnsAllPairs()
    {
        // Arrange
        var options = new[]
        {
            new Option<KeyValuePair<string, int>>.Some(new KeyValuePair<string, int>("a", 1)),
            new Option<KeyValuePair<string, int>>.Some(new KeyValuePair<string, int>("b", 2)),
            new Option<KeyValuePair<string, int>>.Some(new KeyValuePair<string, int>("c", 3))
        };

        // Act
        var dict = options.ToSomeDictionary();

        // Assert
        Assert.Equal(3, dict.Count);
        Assert.Equal(1, dict["a"]);
        Assert.Equal(2, dict["b"]);
        Assert.Equal(3, dict["c"]);
    }

    [Fact]
    public void ToSomeDictionary_MixedOptions_ReturnsOnlySomePairs()
    {
        // Arrange
        var options = new Option<KeyValuePair<string, int>>[]
        {
            new Option<KeyValuePair<string, int>>.Some(new KeyValuePair<string, int>("a", 1)),
            new Option<KeyValuePair<string, int>>.None(),
            new Option<KeyValuePair<string, int>>.Some(new KeyValuePair<string, int>("b", 2)),
            new Option<KeyValuePair<string, int>>.None()
        };

        // Act
        var dict = options.ToSomeDictionary();

        // Assert
        Assert.Equal(2, dict.Count);
        Assert.Equal(1, dict["a"]);
        Assert.Equal(2, dict["b"]);
    }

    [Fact]
    public void ToSomeDictionary_DuplicateKeys_KeepsFirstOccurrence()
    {
        // Arrange
        var options = new[]
        {
            new Option<KeyValuePair<string, int>>.Some(new KeyValuePair<string, int>("a", 1)),
            new Option<KeyValuePair<string, int>>.Some(new KeyValuePair<string, int>("a", 999)),
            new Option<KeyValuePair<string, int>>.Some(new KeyValuePair<string, int>("b", 2))
        };

        // Act
        var dict = options.ToSomeDictionary();

        // Assert
        Assert.Equal(2, dict.Count);
        Assert.Equal(1, dict["a"]); // First occurrence kept
        Assert.Equal(2, dict["b"]);
    }

    [Fact]
    public void ToSomeDictionary_EmptyCollection_ReturnsEmptyDictionary()
    {
        // Arrange
        var options = Array.Empty<Option<KeyValuePair<string, int>>>();

        // Act
        var dict = options.ToSomeDictionary();

        // Assert
        Assert.Empty(dict);
    }

    #endregion

    #region Real-World Scenarios

    [Fact]
    public void FormValidation_AccumulatesAllFieldErrors()
    {
        // Arrange - Simulating form field validation
        var form = new
        {
            Name = "",
            Email = "invalid-email",
            Age = "abc"
        };

        var validations = new[]
        {
            ValidateName(form.Name),
            ValidateEmail(form.Email),
            ValidateAge(form.Age)
        };

        // Act - Use TraverseValidation alternative (sequence is in ValidationExtensions)
        var result = validations.TraverseValidation<Validation<string, string>, string, string>(v => v);

        // Assert - All errors collected
        Assert.True(result.IsFailure);
        if (result.TryGetErrors(out var errors))
        {
            Assert.Equal(3, errors.Count);
            Assert.Contains("Name is required", errors);
            Assert.Contains("Invalid email format", errors);
            Assert.Contains("Age must be a number", errors);
        }

        // Helper methods
        Validation<string, string> ValidateName(string name) =>
            string.IsNullOrWhiteSpace(name)
                ? Validation<string, string>.Invalid("Name is required")
                : Validation<string, string>.Valid(name);

        Validation<string, string> ValidateEmail(string email) =>
            email.Contains("@")
                ? Validation<string, string>.Valid(email)
                : Validation<string, string>.Invalid("Invalid email format");

        Validation<string, string> ValidateAge(string age) =>
            int.TryParse(age, out var _)
                ? Validation<string, string>.Valid(age)
                : Validation<string, string>.Invalid("Age must be a number");
    }

    [Fact]
    public void BatchProcessing_UsingPartitionResults()
    {
        // Arrange - Simulating batch file processing
        var files = new[] { "file1.txt", "file2.txt", "corrupt.txt", "file4.txt" };
        var results = files.Select(ProcessFile);

        // Act
        var (successes, failures) = results.PartitionResults();

        // Assert
        Assert.Equal(3, successes.Count);
        Assert.Single(failures);
        Assert.Contains("Failed to process corrupt.txt", failures);

        // Helper method
        Result<string, string> ProcessFile(string filename) =>
            filename.Contains("corrupt")
                ? Result<string, string>.Err($"Failed to process {filename}")
                : Result<string, string>.Ok($"Processed {filename}");
    }

    [Fact]
    public void ConfigurationLoading_UseFirstValid()
    {
        // Arrange - Try multiple config sources
        var sources = new[] { "config.local.json", "config.dev.json", "config.json" };
        var results = sources.Select(LoadConfig);

        // Act
        var config = results.FirstOk();

        // Assert
        Assert.True(config.IsSuccess);
        if (config.TryGetValue(out var value))
        {
            Assert.Equal("config.json", value); // Falls back to default config
        }

        // Helper method
        Result<string, string> LoadConfig(string source) =>
            source == "config.json"
                ? Result<string, string>.Ok(source)
                : Result<string, string>.Err($"{source} not found");
    }

    #endregion
}
