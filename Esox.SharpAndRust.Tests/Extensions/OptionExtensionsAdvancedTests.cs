using Esox.SharpAndRusty.Types;
using Esox.SharpAndRusty.Extensions;

namespace Esox.SharpAndRust.Tests.Extensions;

public class OptionExtensionsAdvancedTests
{
    #region Filter Tests

    [Fact]
    public void Filter_WithSome_PredicateTrue_ReturnsSome()
    {
        // Arrange
        Option<int> option = new Option<int>.Some(42);

        // Act
        var result = option.Filter(x => x > 10);

        // Assert
        Assert.True(result.IsSome());
        Assert.Equal(42, ((Option<int>.Some)result).Value);
    }

    [Fact]
    public void Filter_WithSome_PredicateFalse_ReturnsNone()
    {
        // Arrange
        Option<int> option = new Option<int>.Some(5);

        // Act
        var result = option.Filter(x => x > 10);

        // Assert
        Assert.True(result.IsNone());
    }

    [Fact]
    public void Filter_WithNone_ReturnsNone()
    {
        // Arrange
        Option<int> option = new Option<int>.None();
        var predicateCalled = false;

        // Act
        var result = option.Filter(x =>
        {
            predicateCalled = true;
            return true;
        });

        // Assert
        Assert.True(result.IsNone());
        Assert.False(predicateCalled); // Predicate should not be called for None
    }

    [Fact]
    public void Filter_CanChainMultipleFilters()
    {
        // Arrange
        Option<int> option = new Option<int>.Some(42);

        // Act
        var result = option
            .Filter(x => x > 10)
            .Filter(x => x < 100)
            .Filter(x => x % 2 == 0);

        // Assert
        Assert.True(result.IsSome());
        Assert.Equal(42, ((Option<int>.Some)result).Value);
    }

    [Fact]
    public void Filter_StopsAtFirstFailure()
    {
        // Arrange
        Option<int> option = new Option<int>.Some(5);

        // Act
        var result = option
            .Filter(x => x > 10) // Fails here
            .Filter(x => x < 100); // Should not execute

        // Assert
        Assert.True(result.IsNone());
    }

    #endregion

    #region Zip Tests

    [Fact]
    public void Zip_BothSome_ReturnsSomeTuple()
    {
        // Arrange
        Option<int> option1 = new Option<int>.Some(42);
        Option<string> option2 = new Option<string>.Some("hello");

        // Act
        var result = option1.Zip(option2);

        // Assert
        Assert.True(result.IsSome());
        var tuple = ((Option<(int, string)>.Some)result).Value;
        Assert.Equal(42, tuple.Item1);
        Assert.Equal("hello", tuple.Item2);
    }

    [Fact]
    public void Zip_FirstNone_ReturnsNone()
    {
        // Arrange
        Option<int> option1 = new Option<int>.None();
        Option<string> option2 = new Option<string>.Some("hello");

        // Act
        var result = option1.Zip(option2);

        // Assert
        Assert.True(result.IsNone());
    }

    [Fact]
    public void Zip_SecondNone_ReturnsNone()
    {
        // Arrange
        Option<int> option1 = new Option<int>.Some(42);
        Option<string> option2 = new Option<string>.None();

        // Act
        var result = option1.Zip(option2);

        // Assert
        Assert.True(result.IsNone());
    }

    [Fact]
    public void Zip_BothNone_ReturnsNone()
    {
        // Arrange
        Option<int> option1 = new Option<int>.None();
        Option<string> option2 = new Option<string>.None();

        // Act
        var result = option1.Zip(option2);

        // Assert
        Assert.True(result.IsNone());
    }

    #endregion

    #region ZipWith Tests

    [Fact]
    public void ZipWith_BothSome_ReturnsZippedValue()
    {
        // Arrange
        Option<int> option1 = new Option<int>.Some(10);
        Option<int> option2 = new Option<int>.Some(32);

        // Act
        var result = option1.ZipWith(option2, (a, b) => a + b);

        // Assert
        Assert.True(result.IsSome());
        Assert.Equal(42, ((Option<int>.Some)result).Value);
    }

    [Fact]
    public void ZipWith_DifferentTypes_WorksCorrectly()
    {
        // Arrange
        Option<int> option1 = new Option<int>.Some(42);
        Option<string> option2 = new Option<string>.Some("years");

        // Act
        var result = option1.ZipWith(option2, (num, unit) => $"{num} {unit}");

        // Assert
        Assert.True(result.IsSome());
        Assert.Equal("42 years", ((Option<string>.Some)result).Value);
    }

    [Fact]
    public void ZipWith_FirstNone_ReturnsNone()
    {
        // Arrange
        Option<int> option1 = new Option<int>.None();
        Option<int> option2 = new Option<int>.Some(32);

        // Act
        var result = option1.ZipWith(option2, (a, b) => a + b);

        // Assert
        Assert.True(result.IsNone());
    }

    [Fact]
    public void ZipWith_SecondNone_ReturnsNone()
    {
        // Arrange
        Option<int> option1 = new Option<int>.Some(10);
        Option<int> option2 = new Option<int>.None();

        // Act
        var result = option1.ZipWith(option2, (a, b) => a + b);

        // Assert
        Assert.True(result.IsNone());
    }

    #endregion

    #region Flatten Tests

    [Fact]
    public void Flatten_SomeContainingSome_ReturnsInnerSome()
    {
        // Arrange
        var inner = new Option<int>.Some(42);
        Option<Option<int>> nested = new Option<Option<int>>.Some(inner);

        // Act
        var result = nested.Flatten();

        // Assert
        Assert.True(result.IsSome());
        Assert.Equal(42, ((Option<int>.Some)result).Value);
    }

    [Fact]
    public void Flatten_SomeContainingNone_ReturnsNone()
    {
        // Arrange
        var inner = new Option<int>.None();
        Option<Option<int>> nested = new Option<Option<int>>.Some(inner);

        // Act
        var result = nested.Flatten();

        // Assert
        Assert.True(result.IsNone());
    }

    [Fact]
    public void Flatten_None_ReturnsNone()
    {
        // Arrange
        Option<Option<int>> nested = new Option<Option<int>>.None();

        // Act
        var result = nested.Flatten();

        // Assert
        Assert.True(result.IsNone());
    }

    [Fact]
    public void Flatten_WithMapAndBind_WorksCorrectly()
    {
        // Arrange
        Option<int> option = new Option<int>.Some(42);

        // Act - Bind naturally handles flattening
        var result = option.Bind(x => new Option<string>.Some(x.ToString()));

        // Assert
        Assert.True(result.IsSome());
        if (result is Option<string>.Some some)
        {
            Assert.Equal("42", some.Value);
        }
    }

    #endregion

    #region And Tests

    [Fact]
    public void And_FirstSome_ReturnsSecond()
    {
        // Arrange
        Option<int> option1 = new Option<int>.Some(42);
        Option<string> option2 = new Option<string>.Some("hello");

        // Act
        var result = option1.And(option2);

        // Assert
        Assert.True(result.IsSome());
        Assert.Equal("hello", ((Option<string>.Some)result).Value);
    }

    [Fact]
    public void And_FirstNone_ReturnsNone()
    {
        // Arrange
        Option<int> option1 = new Option<int>.None();
        Option<string> option2 = new Option<string>.Some("hello");

        // Act
        var result = option1.And(option2);

        // Assert
        Assert.True(result.IsNone());
    }

    [Fact]
    public void And_BothSomeButSecondNone_ReturnsNone()
    {
        // Arrange
        Option<int> option1 = new Option<int>.Some(42);
        Option<string> option2 = new Option<string>.None();

        // Act
        var result = option1.And(option2);

        // Assert
        Assert.True(result.IsNone());
    }

    [Fact]
    public void And_CanChainConditions()
    {
        // Arrange
        Option<int> option = new Option<int>.Some(42);

        // Act
        var result = option
            .And(new Option<string>.Some("hello"))
            .And(new Option<bool>.Some(true));

        // Assert
        Assert.True(result.IsSome());
    }

    #endregion

    #region Or Tests

    [Fact]
    public void Or_FirstSome_ReturnsFirst()
    {
        // Arrange
        Option<int> option1 = new Option<int>.Some(42);
        Option<int> option2 = new Option<int>.Some(99);

        // Act
        var result = option1.Or(option2);

        // Assert
        Assert.True(result.IsSome());
        Assert.Equal(42, ((Option<int>.Some)result).Value);
    }

    [Fact]
    public void Or_FirstNone_ReturnsSecond()
    {
        // Arrange
        Option<int> option1 = new Option<int>.None();
        Option<int> option2 = new Option<int>.Some(99);

        // Act
        var result = option1.Or(option2);

        // Assert
        Assert.True(result.IsSome());
        Assert.Equal(99, ((Option<int>.Some)result).Value);
    }

    [Fact]
    public void Or_BothNone_ReturnsNone()
    {
        // Arrange
        Option<int> option1 = new Option<int>.None();
        Option<int> option2 = new Option<int>.None();

        // Act
        var result = option1.Or(option2);

        // Assert
        Assert.True(result.IsNone());
    }

    [Fact]
    public void Or_CanChainFallbacks()
    {
        // Arrange
        Option<int> primary = new Option<int>.None();
        Option<int> secondary = new Option<int>.None();
        Option<int> tertiary = new Option<int>.Some(42);

        // Act
        var result = primary.Or(secondary).Or(tertiary);

        // Assert
        Assert.True(result.IsSome());
        Assert.Equal(42, ((Option<int>.Some)result).Value);
    }

    #endregion

    #region Xor Tests

    [Fact]
    public void Xor_OnlyFirstSome_ReturnsFirst()
    {
        // Arrange
        Option<int> option1 = new Option<int>.Some(42);
        Option<int> option2 = new Option<int>.None();

        // Act
        var result = option1.Xor(option2);

        // Assert
        Assert.True(result.IsSome());
        Assert.Equal(42, ((Option<int>.Some)result).Value);
    }

    [Fact]
    public void Xor_OnlySecondSome_ReturnsSecond()
    {
        // Arrange
        Option<int> option1 = new Option<int>.None();
        Option<int> option2 = new Option<int>.Some(99);

        // Act
        var result = option1.Xor(option2);

        // Assert
        Assert.True(result.IsSome());
        Assert.Equal(99, ((Option<int>.Some)result).Value);
    }

    [Fact]
    public void Xor_BothSome_ReturnsNone()
    {
        // Arrange
        Option<int> option1 = new Option<int>.Some(42);
        Option<int> option2 = new Option<int>.Some(99);

        // Act
        var result = option1.Xor(option2);

        // Assert
        Assert.True(result.IsNone());
    }

    [Fact]
    public void Xor_BothNone_ReturnsNone()
    {
        // Arrange
        Option<int> option1 = new Option<int>.None();
        Option<int> option2 = new Option<int>.None();

        // Act
        var result = option1.Xor(option2);

        // Assert
        Assert.True(result.IsNone());
    }

    #endregion

    #region Inspect Tests

    [Fact]
    public void Inspect_WithSome_CallsAction()
    {
        // Arrange
        Option<int> option = new Option<int>.Some(42);
        var inspectedValue = 0;

        // Act
        var result = option.Inspect(x => inspectedValue = x);

        // Assert
        Assert.Equal(42, inspectedValue);
        Assert.True(result.IsSome());
        Assert.Equal(42, ((Option<int>.Some)result).Value);
    }

    [Fact]
    public void Inspect_WithNone_DoesNotCallAction()
    {
        // Arrange
        Option<int> option = new Option<int>.None();
        var actionCalled = false;

        // Act
        var result = option.Inspect(x => actionCalled = true);

        // Assert
        Assert.False(actionCalled);
        Assert.True(result.IsNone());
    }

    [Fact]
    public void Inspect_CanChainWithOtherOperations()
    {
        // Arrange
        Option<int> option = new Option<int>.Some(10);
        var log = new List<string>();

        // Act
        var result = option
            .Inspect(x => log.Add($"Initial: {x}"))
            .Map(x => x * 2)
            .Inspect(x => log.Add($"After map: {x}"))
            .Filter(x => x > 15)
            .Inspect(x => log.Add($"After filter: {x}"));

        // Assert
        Assert.Equal(3, log.Count);
        Assert.Contains("Initial: 10", log);
        Assert.Contains("After map: 20", log);
        Assert.Contains("After filter: 20", log);
    }

    #endregion

    #region InspectNone Tests

    [Fact]
    public void InspectNone_WithNone_CallsAction()
    {
        // Arrange
        Option<int> option = new Option<int>.None();
        var actionCalled = false;

        // Act
        var result = option.InspectNone(() => actionCalled = true);

        // Assert
        Assert.True(actionCalled);
        Assert.True(result.IsNone());
    }

    [Fact]
    public void InspectNone_WithSome_DoesNotCallAction()
    {
        // Arrange
        Option<int> option = new Option<int>.Some(42);
        var actionCalled = false;

        // Act
        var result = option.InspectNone(() => actionCalled = true);

        // Assert
        Assert.False(actionCalled);
        Assert.True(result.IsSome());
    }

    [Fact]
    public void InspectNone_CanChainForDebugging()
    {
        // Arrange
        Option<int> option = new Option<int>.None();
        var log = new List<string>();

        // Act
        var result = option
            .InspectNone(() => log.Add("First check: None"))
            .Or(new Option<int>.None())
            .InspectNone(() => log.Add("Second check: Still None"))
            .Or(new Option<int>.Some(42));

        // Assert
        Assert.Equal(2, log.Count);
        Assert.True(result.IsSome());
    }

    #endregion

    #region OkOr Tests

    [Fact]
    public void OkOr_WithSome_ReturnsOk()
    {
        // Arrange
        Option<int> option = new Option<int>.Some(42);

        // Act
        var result = option.OkOr("Error occurred");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.TryGetValue(out var value));
        Assert.Equal(42, value);
    }

    [Fact]
    public void OkOr_WithNone_ReturnsErr()
    {
        // Arrange
        Option<int> option = new Option<int>.None();

        // Act
        var result = option.OkOr("Error occurred");

        // Assert
        Assert.True(result.IsFailure);
        Assert.True(result.TryGetError(out var error));
        Assert.Equal("Error occurred", error);
    }

    [Fact]
    public void OkOr_CanUseWithErrorType()
    {
        // Arrange
        Option<int> option = new Option<int>.None();
        var error = Error.New("Value not found", ErrorKind.NotFound);

        // Act
        var result = option.OkOr(error);

        // Assert
        Assert.True(result.IsFailure);
        Assert.True(result.TryGetError(out var actualError));
        Assert.Equal("Value not found", actualError.Message);
    }

    #endregion

    #region OkOrElse Tests

    [Fact]
    public void OkOrElse_WithSome_ReturnsOk()
    {
        // Arrange
        Option<int> option = new Option<int>.Some(42);
        var factoryCalled = false;

        // Act
        var result = option.OkOrElse(() =>
        {
            factoryCalled = true;
            return "Error";
        });

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(factoryCalled); // Factory should not be called
    }

    [Fact]
    public void OkOrElse_WithNone_CallsFactoryAndReturnsErr()
    {
        // Arrange
        Option<int> option = new Option<int>.None();
        var factoryCalled = false;

        // Act
        var result = option.OkOrElse(() =>
        {
            factoryCalled = true;
            return "Error occurred";
        });

        // Assert
        Assert.True(result.IsFailure);
        Assert.True(factoryCalled);
        Assert.True(result.TryGetError(out var error));
        Assert.Equal("Error occurred", error);
    }

    [Fact]
    public void OkOrElse_FactoryCanCreateComplexError()
    {
        // Arrange
        Option<int> option = new Option<int>.None();

        // Act
        var result = option.OkOrElse(() => 
            Error.New("Value not found")
                .WithContext("During user lookup")
                .WithMetadata("attemptTime", DateTime.UtcNow)
                .WithKind(ErrorKind.NotFound)
        );

        // Assert
        Assert.True(result.IsFailure);
        Assert.True(result.TryGetError(out var error));
        Assert.Equal(ErrorKind.NotFound, error.Kind);
    }

    #endregion

    #region ToNullable Tests

    [Fact]
    public void ToNullable_WithSome_ReturnsValue()
    {
        // Arrange
        Option<int> option = new Option<int>.Some(42);

        // Act
        int? result = option.ToNullable();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void ToNullable_WithNone_ReturnsNull()
    {
        // Arrange
        Option<int> option = new Option<int>.None();

        // Act
        int? result = option.ToNullable();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ToNullable_WorksWithDifferentValueTypes()
    {
        // Arrange
        Option<bool> boolOption = new Option<bool>.Some(true);
        Option<DateTime> dateOption = new Option<DateTime>.None();

        // Act
        bool? boolResult = boolOption.ToNullable();
        DateTime? dateResult = dateOption.ToNullable();

        // Assert
        Assert.True(boolResult.HasValue);
        Assert.True(boolResult.Value);
        Assert.False(dateResult.HasValue);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void AdvancedExtensions_ComplexChaining_WorksCorrectly()
    {
        // Arrange
        Option<int> option = new Option<int>.Some(10);
        var log = new List<string>();

        // Act
        var result = option
            .Inspect(x => log.Add($"Start: {x}"))
            .Filter(x => x > 5)
            .Map(x => x * 2)
            .Inspect(x => log.Add($"After map: {x}"))
            .OkOr("Failed");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, log.Count);
        result.Inspect(x => Assert.Equal(20, x));
    }

    [Fact]
    public void AdvancedExtensions_ZipAndMap_WorkTogether()
    {
        // Arrange
        Option<int> age = new Option<int>.Some(25);
        Option<string> name = new Option<string>.Some("Alice");

        // Act
        var result = age.Zip(name)
            .Map(tuple => $"{tuple.Item2} is {tuple.Item1} years old");

        // Assert
        Assert.True(result.IsSome());
        Assert.Equal("Alice is 25 years old", ((Option<string>.Some)result).Value);
    }

    [Fact]
    public void AdvancedExtensions_FilterAndOr_CreateFallbackChain()
    {
        // Arrange
        Option<int> primary = new Option<int>.Some(5);
        Option<int> secondary = new Option<int>.Some(42);

        // Act
        var result = primary
            .Filter(x => x > 10) // Fails
            .Or(secondary); // Falls back to secondary

        // Assert
        Assert.True(result.IsSome());
        Assert.Equal(42, ((Option<int>.Some)result).Value);
    }

    [Fact]
    public void AdvancedExtensions_XorForExclusiveChoice_WorksCorrectly()
    {
        // Arrange
        Option<string> userInput = new Option<string>.Some("manual");
        Option<string> defaultValue = new Option<string>.Some("default");

        // Act - Want value if exactly one source has it
        var result = userInput.Xor(defaultValue);

        // Assert - Both have values, so None
        Assert.True(result.IsNone());

        // Arrange - Only one has value
        userInput = new Option<string>.None();

        // Act
        result = userInput.Xor(defaultValue);

        // Assert - Now we get the default
        Assert.True(result.IsSome());
        Assert.Equal("default", ((Option<string>.Some)result).Value);
    }

    [Fact]
    public void AdvancedExtensions_InspectForDebugging_PreservesChain()
    {
        // Arrange
        var debugLog = new List<string>();
        Option<int> option = new Option<int>.Some(10);

        // Act
        var result = option
            .Inspect(x => debugLog.Add($"Initial value: {x}"))
            .Map(x => x * 2)
            .Inspect(x => debugLog.Add($"Doubled: {x}"))
            .Filter(x => x > 15)
            .Inspect(x => debugLog.Add($"Passed filter: {x}"))
            .InspectNone(() => debugLog.Add("No value"))
            .Map(x => x + 5);

        // Assert
        Assert.True(result.IsSome());
        Assert.Equal(25, ((Option<int>.Some)result).Value);
        Assert.Equal(3, debugLog.Count);
        Assert.DoesNotContain("No value", debugLog);
    }

    #endregion
}
