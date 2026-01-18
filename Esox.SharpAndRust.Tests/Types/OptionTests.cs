using Esox.SharpAndRusty.Types;

namespace Esox.SharpAndRust.Tests.Types;

public class OptionTests
{
    #region Creation Tests

    [Fact]
    public void Some_CreatesOptionWithValue()
    {
        // Arrange & Act
        var option = new Option<int>.Some(42);

        // Assert
        Assert.NotNull(option);
        Assert.IsType<Option<int>.Some>(option);
    }

    [Fact]
    public void None_CreatesEmptyOption()
    {
        // Arrange & Act
        var option = new Option<int>.None();

        // Assert
        Assert.NotNull(option);
        Assert.IsType<Option<int>.None>(option);
    }

    [Fact]
    public void Some_WithNullReferenceType_StoresNull()
    {
        // Arrange & Act
        var option = new Option<string?>.Some(null);

        // Assert
        Assert.NotNull(option);
        Assert.Null(option.Value);
    }

    [Fact]
    public void Some_WithComplexType_StoresValue()
    {
        // Arrange
        var user = new { Id = 1, Name = "John" };

        // Act
        var option = new Option<object>.Some(user);

        // Assert
        Assert.Equal(user, option.Value);
    }

    #endregion

    #region Value Access Tests

    [Fact]
    public void Some_Value_ReturnsStoredValue()
    {
        // Arrange
        var option = new Option<int>.Some(42);

        // Act
        var value = option.Value;

        // Assert
        Assert.Equal(42, value);
    }

    [Fact]
    public void Some_Value_CanAccessMultipleTimes()
    {
        // Arrange
        var option = new Option<string>.Some("test");

        // Act
        var value1 = option.Value;
        var value2 = option.Value;

        // Assert
        Assert.Equal("test", value1);
        Assert.Equal("test", value2);
        Assert.Equal(value1, value2);
    }

    #endregion

    #region Pattern Matching Tests

    [Fact]
    public void PatternMatch_Some_MatchesSomeCase()
    {
        // Arrange
        Option<int> option = new Option<int>.Some(42);

        // Act & Assert
        var result = option switch
        {
            Option<int>.Some(var value) => $"Value: {value}",
            Option<int>.None => "No value",
            _ => "Unknown"
        };

        Assert.Equal("Value: 42", result);
    }

    [Fact]
    public void PatternMatch_None_MatchesNoneCase()
    {
        // Arrange
        Option<int> option = new Option<int>.None();

        // Act & Assert
        var result = option switch
        {
            Option<int>.Some(var value) => $"Value: {value}",
            Option<int>.None => "No value",
            _ => "Unknown"
        };

        Assert.Equal("No value", result);
    }

    [Fact]
    public void PatternMatch_WithTypeCheck_WorksCorrectly()
    {
        // Arrange
        Option<int> someOption = new Option<int>.Some(42);
        Option<int> noneOption = new Option<int>.None();

        // Act & Assert
        Assert.True(someOption is Option<int>.Some);
        Assert.False(someOption is Option<int>.None);
        Assert.True(noneOption is Option<int>.None);
        Assert.False(noneOption is Option<int>.Some);
    }

    [Fact]
    public void PatternMatch_WithDeconstruction_ExtractsValue()
    {
        // Arrange
        Option<string> option = new Option<string>.Some("Hello");

        // Act
        if (option is Option<string>.Some(var value))
        {
            // Assert
            Assert.Equal("Hello", value);
        }
        else
        {
            Assert.Fail("Should have matched Some case");
        }
    }

    [Fact]
    public void PatternMatch_ComplexType_WorksCorrectly()
    {
        // Arrange
        var user = new { Id = 1, Name = "Alice" };
        Option<object> option = new Option<object>.Some(user);

        // Act & Assert
        var result = option switch
        {
            Option<object>.Some(var value) => value.ToString(),
            Option<object>.None => "No user",
            _ => "Unknown"
        };

        Assert.Equal(user.ToString(), result);
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void Some_Equality_SameValue_ReturnsTrue()
    {
        // Arrange
        var option1 = new Option<int>.Some(42);
        var option2 = new Option<int>.Some(42);

        // Act & Assert
        Assert.Equal(option1, option2);
        Assert.True(option1.Equals(option2));
        Assert.True(option1 == option2);
        Assert.False(option1 != option2);
    }

    [Fact]
    public void Some_Equality_DifferentValue_ReturnsFalse()
    {
        // Arrange
        var option1 = new Option<int>.Some(42);
        var option2 = new Option<int>.Some(43);

        // Act & Assert
        Assert.NotEqual(option1, option2);
        Assert.False(option1.Equals(option2));
        Assert.False(option1 == option2);
        Assert.True(option1 != option2);
    }

    [Fact]
    public void None_Equality_AlwaysEqual()
    {
        // Arrange
        var option1 = new Option<int>.None();
        var option2 = new Option<int>.None();

        // Act & Assert
        Assert.Equal(option1, option2);
        Assert.True(option1.Equals(option2));
        Assert.True(option1 == option2);
        Assert.False(option1 != option2);
    }

    [Fact]
    public void Some_NotEqual_None()
    {
        // Arrange
        Option<int> someOption = new Option<int>.Some(42);
        Option<int> noneOption = new Option<int>.None();

        // Act & Assert
        Assert.NotEqual<Option<int>>(someOption, noneOption);
        Assert.False(someOption.Equals(noneOption));
        Assert.False(someOption == noneOption);
        Assert.True(someOption != noneOption);
    }

    [Fact]
    public void Some_WithReferenceType_EqualityByValue()
    {
        // Arrange
        var option1 = new Option<string>.Some("test");
        var option2 = new Option<string>.Some("test");
        var option3 = new Option<string>.Some("other");

        // Act & Assert
        Assert.Equal(option1, option2);
        Assert.NotEqual(option1, option3);
    }

    [Fact]
    public void Some_WithNull_EqualityWorks()
    {
        // Arrange
        var option1 = new Option<string?>.Some(null);
        var option2 = new Option<string?>.Some(null);
        var option3 = new Option<string?>.Some("test");

        // Act & Assert
        Assert.Equal(option1, option2);
        Assert.NotEqual(option1, option3);
    }

    #endregion

    #region GetHashCode Tests

    [Fact]
    public void Some_GetHashCode_SameValue_ReturnsSameHash()
    {
        // Arrange
        var option1 = new Option<int>.Some(42);
        var option2 = new Option<int>.Some(42);

        // Act
        var hash1 = option1.GetHashCode();
        var hash2 = option2.GetHashCode();

        // Assert
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void Some_GetHashCode_DifferentValue_ReturnsDifferentHash()
    {
        // Arrange
        var option1 = new Option<int>.Some(42);
        var option2 = new Option<int>.Some(43);

        // Act
        var hash1 = option1.GetHashCode();
        var hash2 = option2.GetHashCode();

        // Assert
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void None_GetHashCode_AlwaysSame()
    {
        // Arrange
        var option1 = new Option<int>.None();
        var option2 = new Option<int>.None();

        // Act
        var hash1 = option1.GetHashCode();
        var hash2 = option2.GetHashCode();

        // Assert
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void Some_GetHashCode_CanBeUsedInHashSet()
    {
        // Arrange
        var option1 = new Option<int>.Some(42);
        var option2 = new Option<int>.Some(42);
        var option3 = new Option<int>.Some(43);

        var set = new HashSet<Option<int>> { option1 };

        // Act & Assert
        Assert.Contains(option2, set); // Should find duplicate
        Assert.DoesNotContain(option3, set);
    }

    [Fact]
    public void Option_CanBeUsedAsDictionaryKey()
    {
        // Arrange
        var dict = new Dictionary<Option<string>, int>
        {
            { new Option<string>.Some("key1"), 1 },
            { new Option<string>.Some("key2"), 2 },
            { new Option<string>.None(), 3 }
        };

        // Act & Assert
        Assert.Equal(1, dict[new Option<string>.Some("key1")]);
        Assert.Equal(2, dict[new Option<string>.Some("key2")]);
        Assert.Equal(3, dict[new Option<string>.None()]);
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void Some_ToString_ShowsValue()
    {
        // Arrange
        var option = new Option<int>.Some(42);

        // Act
        var result = option.ToString();

        // Assert
        Assert.NotNull(result);
        Assert.Contains("42", result);
    }

    [Fact]
    public void None_ToString_ShowsNone()
    {
        // Arrange
        var option = new Option<int>.None();

        // Act
        var result = option.ToString();

        // Assert
        Assert.NotNull(result);
        Assert.Contains("None", result);
    }

    [Fact]
    public void Some_ToString_WithString_ShowsValue()
    {
        // Arrange
        var option = new Option<string>.Some("test");

        // Act
        var result = option.ToString();

        // Assert
        Assert.Contains("test", result);
    }

    #endregion

    #region Type Tests

    [Fact]
    public void Option_IsAbstractRecord()
    {
        // Arrange
        var someOption = new Option<int>.Some(42);
        var noneOption = new Option<int>.None();

        // Act & Assert
        Assert.IsAssignableFrom<Option<int>>(someOption);
        Assert.IsAssignableFrom<Option<int>>(noneOption);
    }

    [Fact]
    public void Option_DifferentGenericTypes_AreDistinct()
    {
        // Arrange
        var intOption = new Option<int>.Some(42);
        var stringOption = new Option<string>.Some("42");

        // Act & Assert
        Assert.IsType<Option<int>.Some>(intOption);
        Assert.IsType<Option<string>.Some>(stringOption);
        // Cannot compare different generic types directly
    }

    [Fact]
    public void Option_SupportsNullableValueTypes()
    {
        // Arrange & Act
        var option1 = new Option<int?>.Some(42);
        var option2 = new Option<int?>.Some(null);
        var option3 = new Option<int?>.None();

        // Assert
        Assert.Equal(42, option1.Value);
        Assert.Null(option2.Value);
        Assert.IsType<Option<int?>.None>(option3);
    }

    #endregion

    #region Collection Tests

    [Fact]
    public void Option_CanBeStoredInList()
    {
        // Arrange & Act
        var list = new List<Option<int>>
        {
            new Option<int>.Some(1),
            new Option<int>.Some(2),
            new Option<int>.None(),
            new Option<int>.Some(3)
        };

        // Assert
        Assert.Equal(4, list.Count);
        Assert.IsType<Option<int>.Some>(list[0]);
        Assert.IsType<Option<int>.Some>(list[1]);
        Assert.IsType<Option<int>.None>(list[2]);
        Assert.IsType<Option<int>.Some>(list[3]);
    }

    [Fact]
    public void Option_CanBeFilteredWithLINQ()
    {
        // Arrange
        var options = new List<Option<int>>
        {
            new Option<int>.Some(1),
            new Option<int>.Some(2),
            new Option<int>.None(),
            new Option<int>.Some(3)
        };

        // Act
        var someOptions = options.OfType<Option<int>.Some>().ToList();
        var noneOptions = options.OfType<Option<int>.None>().ToList();

        // Assert
        Assert.Equal(3, someOptions.Count);
        Assert.Single(noneOptions);
    }

    [Fact]
    public void Option_CanExtractValuesFromSome()
    {
        // Arrange
        var options = new List<Option<int>>
        {
            new Option<int>.Some(1),
            new Option<int>.Some(2),
            new Option<int>.None(),
            new Option<int>.Some(3)
        };

        // Act
        var values = options
            .OfType<Option<int>.Some>()
            .Select(o => o.Value)
            .ToList();

        // Assert
        Assert.Equal(new[] { 1, 2, 3 }, values);
    }

    #endregion

    #region Record Functionality Tests

    [Fact]
    public void Some_WithClause_CreatesNewInstance()
    {
        // Arrange
        var option1 = new Option<int>.Some(42);

        // Act
        var option2 = option1 with { Value = 43 };

        // Assert
        Assert.Equal(42, option1.Value);
        Assert.Equal(43, option2.Value);
        Assert.NotEqual(option1, option2);
    }

    [Fact]
    public void Some_WithClause_MaintainsType()
    {
        // Arrange
        var option1 = new Option<string>.Some("hello");

        // Act
        var option2 = option1 with { Value = "world" };

        // Assert
        Assert.IsType<Option<string>.Some>(option2);
        Assert.Equal("world", option2.Value);
    }

    [Fact]
    public void Some_PositionalRecord_AccessValue()
    {
        // Arrange
        var option = new Option<int>.Some(42);

        // Act
        // Records provide positional syntax for construction
        var recreated = new Option<int>.Some(42);

        // Assert
        Assert.Equal(option, recreated);
        Assert.Equal(42, option.Value);
    }

    [Fact]
    public void Record_PrintMembers_WorksForSome()
    {
        // Arrange
        var option = new Option<int>.Some(42);

        // Act
        var result = option.ToString();

        // Assert
        // Records automatically include type and property in ToString
        Assert.Contains("Some", result);
        Assert.Contains("42", result);
    }

    [Fact]
    public void Record_PrintMembers_WorksForNone()
    {
        // Arrange
        var option = new Option<int>.None();

        // Act
        var result = option.ToString();

        // Assert
        Assert.Contains("None", result);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Some_WithDefaultValue_StoresDefault()
    {
        // Arrange & Act
        var option = new Option<int>.Some(default);

        // Assert
        Assert.Equal(0, option.Value);
    }

    [Fact]
    public void Some_WithComplexObject_StoresReference()
    {
        // Arrange
        var list = new List<int> { 1, 2, 3 };
        var option = new Option<List<int>>.Some(list);

        // Act
        option.Value.Add(4);

        // Assert
        Assert.Equal(4, list.Count);
        Assert.Same(list, option.Value);
    }

    [Fact]
    public void Option_WithTuples_WorksCorrectly()
    {
        // Arrange & Act
        var option = new Option<(int, string)>.Some((42, "test"));

        // Assert
        Assert.Equal((42, "test"), option.Value);
    }

    [Fact]
    public void Option_WithNestedOption_WorksCorrectly()
    {
        // Arrange & Act
        var innerOption = new Option<int>.Some(42);
        var outerOption = new Option<Option<int>>.Some(innerOption);

        // Assert
        Assert.IsType<Option<Option<int>>.Some>(outerOption);
        Assert.Equal(innerOption, outerOption.Value);
    }

    [Fact]
    public void Option_DifferentTypesOfNone_AreEqual()
    {
        // Arrange
        var none1 = new Option<int>.None();
        var none2 = new Option<int>.None();
        var none3 = new Option<int>.None();

        // Act & Assert
        Assert.Equal(none1, none2);
        Assert.Equal(none2, none3);
        Assert.Equal(none1, none3);
    }

    #endregion

    #region Null Handling Tests

    [Fact]
    public void Some_DoesNotEqualNull()
    {
        // Arrange
        var option = new Option<int>.Some(42);

        // Act & Assert
        Assert.False(option.Equals(null));
        Assert.False(option == null);
        Assert.True(option != null);
    }

    [Fact]
    public void None_DoesNotEqualNull()
    {
        // Arrange
        var option = new Option<int>.None();

        // Act & Assert
        Assert.False(option.Equals(null));
        Assert.False(option == null);
        Assert.True(option != null);
    }

    #endregion
}
