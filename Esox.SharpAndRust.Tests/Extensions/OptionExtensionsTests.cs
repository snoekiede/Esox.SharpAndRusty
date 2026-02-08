using Esox.SharpAndRusty.Types;
using Esox.SharpAndRusty.Extensions;

namespace Esox.SharpAndRust.Tests.Extensions;

public class OptionExtensionsTests
{
    #region IsSome Tests

    [Fact]
    public void IsSome_WithSome_ReturnsTrue()
    {
        // Arrange
        var option = new Option<int>.Some(42);

        // Act
        var result = option.IsSome();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsSome_WithNone_ReturnsFalse()
    {
        // Arrange
        var option = new Option<int>.None();

        // Act
        var result = option.IsSome();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsSome_WithReferenceType_WorksCorrectly()
    {
        // Arrange
        var someOption = new Option<string>.Some("test");
        var noneOption = new Option<string>.None();

        // Act & Assert
        Assert.True(someOption.IsSome());
        Assert.False(noneOption.IsSome());
    }

    #endregion

    #region IsNone Tests

    [Fact]
    public void IsNone_WithNone_ReturnsTrue()
    {
        // Arrange
        var option = new Option<int>.None();

        // Act
        var result = option.IsNone();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsNone_WithSome_ReturnsFalse()
    {
        // Arrange
        var option = new Option<int>.Some(42);

        // Act
        var result = option.IsNone();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsNone_WithReferenceType_WorksCorrectly()
    {
        // Arrange
        var someOption = new Option<string>.Some("test");
        var noneOption = new Option<string>.None();

        // Act & Assert
        Assert.False(someOption.IsNone());
        Assert.True(noneOption.IsNone());
    }

    [Fact]
    public void IsSome_And_IsNone_AreComplementary()
    {
        // Arrange
        var someOption = new Option<int>.Some(42);
        var noneOption = new Option<int>.None();

        // Act & Assert
        Assert.True(someOption.IsSome() != someOption.IsNone());
        Assert.True(noneOption.IsSome() != noneOption.IsNone());
    }

    #endregion

    #region GetValueOrDefault Tests

    [Fact]
    public void GetValueOrDefault_WithSome_ReturnsValue()
    {
        // Arrange
        var option = new Option<int>.Some(42);

        // Act
        var result = option.GetValueOrDefault(0);

        // Assert
        Assert.Equal(42, result);
    }

    [Fact]
    public void GetValueOrDefault_WithNone_ReturnsDefault()
    {
        // Arrange
        var option = new Option<int>.None();

        // Act
        var result = option.GetValueOrDefault(99);

        // Assert
        Assert.Equal(99, result);
    }

    [Fact]
    public void GetValueOrDefault_WithReferenceType_WorksCorrectly()
    {
        // Arrange
        var someOption = new Option<string>.Some("hello");
        var noneOption = new Option<string>.None();

        // Act
        var someResult = someOption.GetValueOrDefault("default");
        var noneResult = noneOption.GetValueOrDefault("default");

        // Assert
        Assert.Equal("hello", someResult);
        Assert.Equal("default", noneResult);
    }

    [Fact]
    public void GetValueOrDefault_WithNullDefault_WorksCorrectly()
    {
        // Arrange
        var option = new Option<string?>.None();

        // Act
        var result = option.GetValueOrDefault(null);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetValueOrDefault_WithComplexType_ReturnsCorrectValue()
    {
        // Arrange
        var user = new { Id = 1, Name = "Alice" };
        var defaultUser = new { Id = 0, Name = "Default" };
        var someOption = new Option<object>.Some(user);
        var noneOption = new Option<object>.None();

        // Act
        var someResult = someOption.GetValueOrDefault(defaultUser);
        var noneResult = noneOption.GetValueOrDefault(defaultUser);

        // Assert
        Assert.Same(user, someResult);
        Assert.Same(defaultUser, noneResult);
    }

    #endregion

    #region GetValueOrElse Tests

    [Fact]
    public void GetValueOrElse_WithSome_ReturnsValue()
    {
        // Arrange
        var option = new Option<int>.Some(42);
        var factoryCalled = false;

        // Act
        var result = option.GetValueOrElse(() =>
        {
            factoryCalled = true;
            return 99;
        });

        // Assert
        Assert.Equal(42, result);
        Assert.False(factoryCalled); // Factory should not be called
    }

    [Fact]
    public void GetValueOrElse_WithNone_CallsFactory()
    {
        // Arrange
        var option = new Option<int>.None();
        var factoryCalled = false;

        // Act
        var result = option.GetValueOrElse(() =>
        {
            factoryCalled = true;
            return 99;
        });

        // Assert
        Assert.Equal(99, result);
        Assert.True(factoryCalled);
    }

    [Fact]
    public void GetValueOrElse_WithNone_OnlyCallsFactoryOnce()
    {
        // Arrange
        var option = new Option<int>.None();
        var callCount = 0;

        // Act
        var result = option.GetValueOrElse(() =>
        {
            callCount++;
            return 99;
        });

        // Assert
        Assert.Equal(1, callCount);
    }

    [Fact]
    public void GetValueOrElse_WithReferenceType_WorksCorrectly()
    {
        // Arrange
        var someOption = new Option<string>.Some("hello");
        var noneOption = new Option<string>.None();

        // Act
        var someResult = someOption.GetValueOrElse(() => "default");
        var noneResult = noneOption.GetValueOrElse(() => "default");

        // Assert
        Assert.Equal("hello", someResult);
        Assert.Equal("default", noneResult);
    }

    [Fact]
    public void GetValueOrElse_FactoryCanPerformComplexLogic()
    {
        // Arrange
        var option = new Option<int>.None();
        var counter = 0;

        // Act
        var result = option.GetValueOrElse(() =>
        {
            counter += 10;
            counter *= 2;
            return counter;
        });

        // Assert
        Assert.Equal(20, result);
    }

    #endregion

    #region Map Tests

    [Fact]
    public void Map_WithSome_TransformsValue()
    {
        // Arrange
        var option = new Option<int>.Some(42);

        // Act
        var result = option.Map(x => x * 2);

        // Assert
        Assert.True(result.IsSome());
        Assert.Equal(84, ((Option<int>.Some)result).Value);
    }

    [Fact]
    public void Map_WithNone_ReturnsNone()
    {
        // Arrange
        var option = new Option<int>.None();

        // Act
        var result = option.Map(x => x * 2);

        // Assert
        Assert.True(result.IsNone());
        Assert.IsType<Option<int>.None>(result);
    }

    [Fact]
    public void Map_CanChangeType()
    {
        // Arrange
        var option = new Option<int>.Some(42);

        // Act
        var result = option.Map(x => $"Value: {x}");

        // Assert
        Assert.True(result.IsSome());
        Assert.Equal("Value: 42", ((Option<string>.Some)result).Value);
    }

    [Fact]
    public void Map_CanChainMultipleMaps()
    {
        // Arrange
        var option = new Option<int>.Some(10);

        // Act
        var result = option
            .Map(x => x * 2)
            .Map(x => x + 5)
            .Map(x => $"Result: {x}");

        // Assert
        Assert.True(result.IsSome());
        Assert.Equal("Result: 25", ((Option<string>.Some)result).Value);
    }

    [Fact]
    public void Map_WithNullMapper_ThrowsNullReferenceException()
    {
        // Arrange
        var option = new Option<int>.Some(42);
        Func<int, string> mapper = null!;

        // Act & Assert
        Assert.Throws<NullReferenceException>(() => option.Map(mapper));
    }

    [Fact]
    public void Map_StopsAtFirstNone()
    {
        // Arrange
        var noneOption = new Option<int>.None();
        var mapperCalled = false;

        // Act
        var result = noneOption.Map(x =>
        {
            mapperCalled = true;
            return x * 2;
        });

        // Assert
        Assert.True(result.IsNone());
        Assert.False(mapperCalled); // Mapper should not be called for None
    }

    [Fact]
    public void Map_WithComplexTransformation_WorksCorrectly()
    {
        // Arrange
        var option = new Option<string>.Some("hello");

        // Act
        var result = option.Map(s => (s, s.Length));

        // Assert
        Assert.True(result.IsSome());
        if (result is Option<(string, int)>.Some some)
        {
            Assert.Equal("hello", some.Value.Item1);
            Assert.Equal(5, some.Value.Item2);
        }
    }

    #endregion

    #region Bind Tests

    [Fact]
    public void Bind_WithSome_CallsBinder()
    {
        // Arrange
        var option = new Option<int>.Some(42);

        // Act
        var result = option.Bind(x => new Option<string>.Some($"Value: {x}"));

        // Assert
        Assert.True(result.IsSome());
        Assert.Equal("Value: 42", ((Option<string>.Some)result).Value);
    }

    [Fact]
    public void Bind_WithNone_ReturnsNone()
    {
        // Arrange
        var option = new Option<int>.None();

        // Act
        var result = option.Bind(x => new Option<string>.Some($"Value: {x}"));

        // Assert
        Assert.True(result.IsNone());
        Assert.IsType<Option<string>.None>(result);
    }

    [Fact]
    public void Bind_BinderReturnsNone_ReturnsNone()
    {
        // Arrange
        var option = new Option<int>.Some(42);

        // Act
        var result = option.Bind(x => new Option<string>.None());

        // Assert
        Assert.True(result.IsNone());
    }

    [Fact]
    public void Bind_CanChainMultipleBinds()
    {
        // Arrange
        var option = new Option<int>.Some(10);

        // Act
        var result = option
            .Bind(x => new Option<int>.Some(x * 2))
            .Bind(x => new Option<int>.Some(x + 5))
            .Bind(x => new Option<string>.Some($"Result: {x}"));

        // Assert
        Assert.True(result.IsSome());
        Assert.Equal("Result: 25", ((Option<string>.Some)result).Value);
    }

    [Fact]
    public void Bind_StopsAtFirstNone()
    {
        // Arrange
        var option = new Option<int>.Some(10);
        var thirdBindCalled = false;

        // Act
        var result = option
            .Bind(x => new Option<int>.Some(x * 2))
            .Bind(x => new Option<int>.None()) // Returns None here
            .Bind(x => 
            {
                thirdBindCalled = true;
                return new Option<int>.Some(x + 100);
            });

        // Assert
        Assert.True(result.IsNone());
        Assert.False(thirdBindCalled); // Should not be called
    }

    [Fact]
    public void Bind_WithNullBinder_ThrowsNullReferenceException()
    {
        // Arrange
        var option = new Option<int>.Some(42);
        Func<int, Option<string>> binder = null!;

        // Act & Assert
        Assert.Throws<NullReferenceException>(() => option.Bind(binder));
    }

    [Fact]
    public void Bind_ConditionalLogic_WorksCorrectly()
    {
        // Arrange
        Option<int> largeOption = new Option<int>.Some(15);
        Option<int> smallOption = new Option<int>.Some(5);

        // Define a function that does conditional binding
        static Option<string> ConditionalBind(int x) =>
            x > 10 
                ? new Option<string>.Some("Large") 
                : new Option<string>.None();

        // Act - Bind with conditional logic
        var largeResult = largeOption.Bind(ConditionalBind);
        var smallResult = smallOption.Bind(ConditionalBind);

        // Assert
        Assert.True(largeResult.IsSome());
        Assert.True(smallResult.IsNone()); // 5 is not > 10
    }

    [Fact]
    public void Bind_MapVsBind_Difference()
    {
        // Arrange
        var option = new Option<int>.Some(42);

        // Act - Map always wraps in Some
        var mapResult = option.Map(x => x > 10 ? x : 0);
        
        // Act - Bind can return None
        var bindResult = option.Bind(x => 
            new Option<int>.Some(x) // Always returns Some for this test
        );

        // Assert
        Assert.True(mapResult.IsSome()); // Map always returns Some for Some input
        Assert.True(bindResult.IsSome()); // Bind can control Some/None
        Assert.Equal(42, ((Option<int>.Some)mapResult).Value);
        Assert.Equal(42, ((Option<int>.Some)bindResult).Value);
    }

    #endregion

    #region Match (Action) Tests

    [Fact]
    public void Match_Action_WithSome_CallsOnSome()
    {
        // Arrange
        var option = new Option<int>.Some(42);
        var someValue = 0;
        var noneCalled = false;

        // Act
        option.Match(
            onSome: value => someValue = value,
            onNone: () => noneCalled = true
        );

        // Assert
        Assert.Equal(42, someValue);
        Assert.False(noneCalled);
    }

    [Fact]
    public void Match_Action_WithNone_CallsOnNone()
    {
        // Arrange
        var option = new Option<int>.None();
        var someValue = 0;
        var noneCalled = false;

        // Act
        option.Match(
            onSome: value => someValue = value,
            onNone: () => noneCalled = true
        );

        // Assert
        Assert.Equal(0, someValue); // Should not be modified
        Assert.True(noneCalled);
    }

    [Fact]
    public void Match_Action_OnSomeCanPerformSideEffects()
    {
        // Arrange
        var option = new Option<string>.Some("test");
        var log = new List<string>();

        // Act
        option.Match(
            onSome: value =>
            {
                log.Add($"Found: {value}");
                log.Add($"Length: {value.Length}");
            },
            onNone: () => log.Add("Not found")
        );

        // Assert
        Assert.Equal(2, log.Count);
        Assert.Contains("Found: test", log);
        Assert.Contains("Length: 4", log);
    }

    [Fact]
    public void Match_Action_OnNoneCanPerformSideEffects()
    {
        // Arrange
        var option = new Option<string>.None();
        var log = new List<string>();

        // Act
        option.Match(
            onSome: value => log.Add($"Found: {value}"),
            onNone: () =>
            {
                log.Add("Not found");
                log.Add("Using default");
            }
        );

        // Assert
        Assert.Equal(2, log.Count);
        Assert.Contains("Not found", log);
        Assert.Contains("Using default", log);
    }

    #endregion

    #region Match (Func) Tests

    [Fact]
    public void Match_Func_WithSome_ReturnsOnSomeResult()
    {
        // Arrange
        var option = new Option<int>.Some(42);

        // Act
        var result = option.Match(
            onSome: value => $"Value: {value}",
            onNone: () => "No value"
        );

        // Assert
        Assert.Equal("Value: 42", result);
    }

    [Fact]
    public void Match_Func_WithNone_ReturnsOnNoneResult()
    {
        // Arrange
        var option = new Option<int>.None();

        // Act
        var result = option.Match(
            onSome: value => $"Value: {value}",
            onNone: () => "No value"
        );

        // Assert
        Assert.Equal("No value", result);
    }

    [Fact]
    public void Match_Func_CanReturnDifferentTypes()
    {
        // Arrange
        var someOption = new Option<int>.Some(42);
        var noneOption = new Option<int>.None();

        // Act
        var someResult = someOption.Match(
            onSome: value => value * 2,
            onNone: () => 0
        );

        var noneResult = noneOption.Match(
            onSome: value => value * 2,
            onNone: () => 0
        );

        // Assert
        Assert.Equal(84, someResult);
        Assert.Equal(0, noneResult);
    }

    [Fact]
    public void Match_Func_WithComplexLogic_WorksCorrectly()
    {
        // Arrange
        var option = new Option<string>.Some("hello");

        // Act
        var result = option.Match(
            onSome: value => new { Text = value, Length = value.Length },
            onNone: () => new { Text = "default", Length = 0 }
        );

        // Assert
        Assert.Equal("hello", result.Text);
        Assert.Equal(5, result.Length);
    }

    [Fact]
    public void Match_Func_CanBeUsedForConditionalLogic()
    {
        // Arrange
        var option = new Option<int>.Some(15);

        // Act
        var result = option.Match(
            onSome: value => value > 10 ? "Large" : "Small",
            onNone: () => "Unknown"
        );

        // Assert
        Assert.Equal("Large", result);
    }

    [Fact]
    public void Match_Func_WithNullOnSome_ThrowsNullReferenceException()
    {
        // Arrange
        var option = new Option<int>.Some(42);
        Func<int, string> onSome = null!;

        // Act & Assert
        Assert.Throws<NullReferenceException>(() => 
            option.Match(onSome, () => "default")
        );
    }

    [Fact]
    public void Match_Func_WithNullOnNone_WorksIfSome()
    {
        // Arrange
        var option = new Option<int>.Some(42);
        Func<string> onNone = null!;

        // Act
        var result = option.Match(
            onSome: value => $"Value: {value}",
            onNone: onNone
        );

        // Assert
        Assert.Equal("Value: 42", result);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void OptionExtensions_RealWorldScenario_UserLookup()
    {
        // Arrange
        var userOption = new Option<string>.Some("Alice");

        // Act
        var greeting = userOption
            .Map(name => name.ToUpper())
            .Match(
                onSome: name => $"Hello, {name}!",
                onNone: () => "Hello, Guest!"
            );

        // Assert
        Assert.Equal("Hello, ALICE!", greeting);
    }

    [Fact]
    public void OptionExtensions_RealWorldScenario_ConfigurationValue()
    {
        // Arrange
        var configOption = new Option<int>.None();

        // Act
        var timeout = configOption
            .Map(value => value * 1000) // Convert to milliseconds
            .GetValueOrDefault(5000); // Default 5 seconds

        // Assert
        Assert.Equal(5000, timeout);
    }

    [Fact]
    public void OptionExtensions_RealWorldScenario_ValidationChain()
    {
        // Arrange
        Option<int> inputOption = new Option<int>.Some(25);

        // Define validation functions
        static Option<int> ValidatePositive(int value) =>
            value > 0 ? new Option<int>.Some(value) : new Option<int>.None();
            
        static Option<int> ValidateLessThan100(int value) =>
            value < 100 ? new Option<int>.Some(value) : new Option<int>.None();

        // Act
        var result = inputOption
            .Bind(ValidatePositive)
            .Bind(ValidateLessThan100)
            .Map(value => $"Valid age: {value}");

        // Assert
        Assert.True(result.IsSome());
        if (result is Option<string>.Some some)
        {
            Assert.Equal("Valid age: 25", some.Value);
        }
    }

    [Fact]
    public void OptionExtensions_ComplexChaining_WorksCorrectly()
    {
        // Arrange
        Option<int> option = new Option<int>.Some(10);

        // Define filter function
        static Option<int> FilterGreaterThan15(int x) =>
            x > 15 ? new Option<int>.Some(x) : new Option<int>.None();

        // Act
        var result = option
            .Map(x => x * 2) // 20
            .Bind(FilterGreaterThan15) // Some(20)
            .Map(x => x + 5) // 25
            .Match(
                onSome: x => $"Final: {x}",
                onNone: () => "Failed"
            );

        // Assert
        Assert.Equal("Final: 25", result);
    }

    [Fact]
    public void OptionExtensions_MixedWithSwitchExpression_WorksCorrectly()
    {
        // Arrange
        var option = new Option<int>.Some(42);

        // Act
        var result = option.Map(x => x * 2);
        var message = result switch
        {
            Option<int>.Some(var value) when value > 50 => $"Large: {value}",
            Option<int>.Some(var value) => $"Small: {value}",
            Option<int>.None => "None",
            _ => "Unknown"
        };

        // Assert
        Assert.Equal("Large: 84", message);
    }

    #endregion
}
