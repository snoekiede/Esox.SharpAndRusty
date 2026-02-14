using Esox.SharpAndRusty.Types;
using Esox.SharpAndRusty.Extensions;

namespace Esox.SharpAndRust.Tests.Extensions;

public class LinqExtensionsTests
{
    #region Option LINQ Tests

    [Fact]
    public void Option_Select_WithSome_TransformsValue()
    {
        // Arrange
        var option = new Option<int>.Some(42);

        // Act
        var result = from x in option
                     select x * 2;

        // Assert
        Assert.True(result.IsSome());
        if (result is Option<int>.Some some)
        {
            Assert.Equal(84, some.Value);
        }
    }

    [Fact]
    public void Option_Select_WithNone_ReturnsNone()
    {
        // Arrange
        var option = new Option<int>.None();

        // Act
        var result = from x in option
                     select x * 2;

        // Assert
        Assert.True(result.IsNone());
    }

    [Fact]
    public void Option_SelectMany_WithMultipleFrom_ChainsCorrectly()
    {
        // Arrange
        var option1 = new Option<int>.Some(10);
        var option2 = new Option<int>.Some(20);

        // Act
        var result = from x in option1
                     from y in option2
                     select x + y;

        // Assert
        Assert.True(result.IsSome());
        if (result is Option<int>.Some some)
        {
            Assert.Equal(30, some.Value);
        }
    }

    [Fact]
    public void Option_SelectMany_WithNoneInFirst_ReturnsNone()
    {
        // Arrange
        var option1 = new Option<int>.None();
        var option2 = new Option<int>.Some(20);

        // Act
        var result = from x in option1
                     from y in option2
                     select x + y;

        // Assert
        Assert.True(result.IsNone());
    }

    [Fact]
    public void Option_SelectMany_WithNoneInSecond_ReturnsNone()
    {
        // Arrange
        var option1 = new Option<int>.Some(10);
        var option2 = new Option<int>.None();

        // Act
        var result = from x in option1
                     from y in option2
                     select x + y;

        // Assert
        Assert.True(result.IsNone());
    }

    [Fact]
    public void Option_Where_WithSomeAndPredicateTrue_ReturnsSome()
    {
        // Arrange
        var option = new Option<int>.Some(42);

        // Act
        var result = from x in option
                     where x > 10
                     select x;

        // Assert
        Assert.True(result.IsSome());
        if (result is Option<int>.Some some)
        {
            Assert.Equal(42, some.Value);
        }
    }

    [Fact]
    public void Option_Where_WithSomeAndPredicateFalse_ReturnsNone()
    {
        // Arrange
        var option = new Option<int>.Some(5);

        // Act
        var result = from x in option
                     where x > 10
                     select x;

        // Assert
        Assert.True(result.IsNone());
    }

    [Fact]
    public void Option_ComplexQuery_WithMultipleOperations_WorksCorrectly()
    {
        // Arrange
        var option = new Option<int>.Some(10);

        // Act
        var result = from x in option
                     where x > 5
                     from y in new Option<int>.Some(20)
                     where y > 15
                     select x + y;

        // Assert
        Assert.True(result.IsSome());
        if (result is Option<int>.Some some)
        {
            Assert.Equal(30, some.Value);
        }
    }

    [Fact]
    public void Option_ComplexQuery_WithFailingWhere_ReturnsNone()
    {
        // Arrange
        var option = new Option<int>.Some(10);

        // Act
        var result = from x in option
                     where x > 5
                     from y in new Option<int>.Some(20)
                     where y > 50  // This fails
                     select x + y;

        // Assert
        Assert.True(result.IsNone());
    }

    [Fact]
    public void Option_NestedQuery_WorksCorrectly()
    {
        // Arrange
        Option<int> GetValue(int x) => new Option<int>.Some(x * 2);

        var option = new Option<int>.Some(5);

        // Act
        var result = from x in option
                     from y in GetValue(x)
                     from z in GetValue(y)
                     select z;

        // Assert
        Assert.True(result.IsSome());
        if (result is Option<int>.Some some)
        {
            Assert.Equal(20, some.Value); // 5 * 2 * 2
        }
    }

    #endregion

    #region Option LINQ Exception Tests

    [Fact]
    public void Option_Select_WithNullSelector_ThrowsArgumentNullException()
    {
        // Arrange
        var option = new Option<int>.Some(42);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            option.Select<int, int>(null!));
    }

    [Fact]
    public void Option_SelectMany_WithNullCollectionSelector_ThrowsArgumentNullException()
    {
        // Arrange
        var option = new Option<int>.Some(42);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            option.SelectMany<int, int, int>(null!, (x, y) => x + y));
    }

    [Fact]
    public void Option_SelectMany_WithNullResultSelector_ThrowsArgumentNullException()
    {
        // Arrange
        var option = new Option<int>.Some(42);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            option.SelectMany(
                x => new Option<int>.Some(x),
                (Func<int, int, int>)null!));
    }

    [Fact]
    public void Option_Where_WithNullPredicate_ThrowsArgumentNullException()
    {
        // Arrange
        var option = new Option<int>.Some(42);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            option.Where(null!));
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void Integration_Option_UserLookupWithLinq_WorksCorrectly()
    {
        // Arrange
        Option<int> GetUserId(string username) =>
            username == "admin"
                ? new Option<int>.Some(1)
                : new Option<int>.None();

        Option<string> GetUserEmail(int userId) =>
            userId > 0
                ? new Option<string>.Some($"user{userId}@example.com")
                : new Option<string>.None();

        // Act
        var result = from userId in GetUserId("admin")
                     from email in GetUserEmail(userId)
                     select new { userId, email };

        // Assert
        Assert.True(result.IsSome());
        result.Match(
            value =>
            {
                Assert.Equal(1, value.userId);
                Assert.Equal("user1@example.com", value.email);
            },
            () => Assert.Fail("Expected Some but got None"));
    }

    [Fact]
    public void Integration_Option_ValidationChain_WorksCorrectly()
    {
        // Arrange
        Option<int> ParseInt(string s) =>
            int.TryParse(s, out var n)
                ? new Option<int>.Some(n)
                : new Option<int>.None();

        // Act
        var result = from x in ParseInt("10")
                     where x > 0
                     from y in ParseInt("20")
                     where y > 0
                     select x + y;

        // Assert
        Assert.True(result.IsSome());
        if (result is Option<int>.Some some)
        {
            Assert.Equal(30, some.Value);
        }
    }

    [Fact]
    public void Integration_Option_DataProcessingPipeline_WorksCorrectly()
    {
        // Arrange
        var data = new Option<string>.Some("42");

        Option<int> Parse(string s) =>
            int.TryParse(s, out var n)
                ? new Option<int>.Some(n)
                : new Option<int>.None();

        Option<int> Validate(int n) =>
            n > 0 && n < 100
                ? new Option<int>.Some(n)
                : new Option<int>.None();

        // Act
        var result = from str in data
                     from parsed in Parse(str)
                     from validated in Validate(parsed)
                     select validated * 2;

        // Assert
        Assert.True(result.IsSome());
        if (result is Option<int>.Some some)
        {
            Assert.Equal(84, some.Value);
        }
    }

    [Fact]
    public void Integration_Option_EarlyTermination_StopsAtFirstNone()
    {
        // Arrange
        var callCount = 0;

        Option<int> GetValue(int x)
        {
            callCount++;
            return x > 0
                ? new Option<int>.Some(x * 2)
                : new Option<int>.None();
        }

        // Act
        var result = from x in new Option<int>.Some(5)
                     from y in GetValue(x)      // Called
                     from z in GetValue(-1)      // Called, returns None
                     from w in GetValue(100)     // Not called
                     select w;

        // Assert
        Assert.True(result.IsNone());
        Assert.Equal(2, callCount); // Only first two calls, third returns None so fourth not called
    }

    [Fact]
    public void Integration_Option_ComplexBusinessLogic_WorksCorrectly()
    {
        // Arrange
        Option<(int Id, string Name, int Age)> GetUser(int id) =>
            id == 1
                ? new Option<(int, string, int)>.Some((1, "Alice", 30))
                : new Option<(int, string, int)>.None();

        Option<(int Id, int UserId, decimal Amount)> GetLastOrder(int userId) =>
            userId == 1
                ? new Option<(int, int, decimal)>.Some((101, 1, 99.99m))
                : new Option<(int, int, decimal)>.None();

        // Act
        var result = from user in GetUser(1)
                     where user.Age >= 18
                     from order in GetLastOrder(user.Id)
                     where order.Amount > 50
                     select new
                     {
                         UserName = user.Name,
                         OrderId = order.Id,
                         Amount = order.Amount
                     };

        // Assert
        Assert.True(result.IsSome());
        result.Match(
            value =>
            {
                Assert.Equal("Alice", value.UserName);
                Assert.Equal(101, value.OrderId);
                Assert.Equal(99.99m, value.Amount);
            },
            () => Assert.Fail("Expected Some but got None"));
    }

    #endregion

    #region Comparison with Method Syntax

    [Fact]
    public void Comparison_QuerySyntax_EquivalentToMethodSyntax()
    {
        // Arrange
        var option1 = new Option<int>.Some(10);
        var option2 = new Option<int>.Some(20);

        // Act - Query syntax
        var queryResult = from x in option1
                          from y in option2
                          select x + y;

        // Act - Method syntax
        var methodResult = option1.Bind(x =>
            option2.Map(y => x + y));

        // Assert
        Assert.True(queryResult.IsSome());
        Assert.True(methodResult.IsSome());

        if (queryResult is Option<int>.Some querySome &&
            methodResult is Option<int>.Some methodSome)
        {
            Assert.Equal(querySome.Value, methodSome.Value);
        }
    }

    [Fact]
    public void Comparison_ComplexQuery_EquivalentToMethodChain()
    {
        // Arrange
        var option = new Option<int>.Some(10);

        // Act - Query syntax
        var queryResult = from x in option
                          where x > 5
                          from y in new Option<int>.Some(20)
                          where y > 15
                          select x + y;

        // Act - Method syntax
        var methodResult = option
            .Filter(x => x > 5)
            .Bind(x => new Option<int>.Some(20)
                .Filter(y => y > 15)
                .Map(y => x + y));

        // Assert
        Assert.True(queryResult.IsSome());
        Assert.True(methodResult.IsSome());

        if (queryResult is Option<int>.Some querySome &&
            methodResult is Option<int>.Some methodSome)
        {
            Assert.Equal(querySome.Value, methodSome.Value);
        }
    }

    #endregion
}
