using Esox.SharpAndRusty.Types;
using Esox.SharpAndRusty.Extensions;

namespace Esox.SharpAndRust.Tests.Types;

public class EitherTests
{
    #region Construction Tests

    [Fact]
    public void Left_Construction_CreatesLeftEither()
    {
        // Arrange & Act
        var either = new Either<int, string>.Left(42);

        // Assert
        Assert.True(either.IsLeft);
        Assert.False(either.IsRight);
        Assert.True(either.TryGetLeft(out var value));
        Assert.Equal(42, value);
    }

    [Fact]
    public void Right_Construction_CreatesRightEither()
    {
        // Arrange & Act
        var either = new Either<int, string>.Right("Hello");

        // Assert
        Assert.False(either.IsLeft);
        Assert.True(either.IsRight);
        Assert.True(either.TryGetRight(out var value));
        Assert.Equal("Hello", value);
    }

    #endregion

    #region TryGet Tests

    [Fact]
    public void TryGetLeft_WithLeft_ReturnsTrue()
    {
        // Arrange
        var either = new Either<int, string>.Left(42);

        // Act
        var success = either.TryGetLeft(out var value);

        // Assert
        Assert.True(success);
        Assert.Equal(42, value);
    }

    [Fact]
    public void TryGetLeft_WithRight_ReturnsFalse()
    {
        // Arrange
        var either = new Either<int, string>.Right("Hello");

        // Act
        var success = either.TryGetLeft(out var value);

        // Assert
        Assert.False(success);
        Assert.Equal(default, value);
    }

    [Fact]
    public void TryGetRight_WithRight_ReturnsTrue()
    {
        // Arrange
        var either = new Either<int, string>.Right("Hello");

        // Act
        var success = either.TryGetRight(out var value);

        // Assert
        Assert.True(success);
        Assert.Equal("Hello", value);
    }

    [Fact]
    public void TryGetRight_WithLeft_ReturnsFalse()
    {
        // Arrange
        var either = new Either<int, string>.Left(42);

        // Act
        var success = either.TryGetRight(out var value);

        // Assert
        Assert.False(success);
        Assert.Equal(default, value);
    }

    #endregion

    #region Match Tests

    [Fact]
    public void Match_WithLeft_ExecutesLeftFunction()
    {
        // Arrange
        var either = new Either<int, string>.Left(42);

        // Act
        var result = either.Match(
            onLeft: x => $"Left: {x}",
            onRight: x => $"Right: {x}");

        // Assert
        Assert.Equal("Left: 42", result);
    }

    [Fact]
    public void Match_WithRight_ExecutesRightFunction()
    {
        // Arrange
        var either = new Either<int, string>.Right("Hello");

        // Act
        var result = either.Match(
            onLeft: x => $"Left: {x}",
            onRight: x => $"Right: {x}");

        // Assert
        Assert.Equal("Right: Hello", result);
    }

    [Fact]
    public void Match_WithNullOnLeft_ThrowsArgumentNullException()
    {
        // Arrange
        var either = new Either<int, string>.Left(42);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            either.Match(null!, x => x));
    }

    [Fact]
    public void Match_WithNullOnRight_ThrowsArgumentNullException()
    {
        // Arrange
        var either = new Either<int, string>.Right("Hello");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            either.Match(x => x.ToString(), null!));
    }

    [Fact]
    public void Match_ActionOverload_WithLeft_ExecutesLeftAction()
    {
        // Arrange
        var either = new Either<int, string>.Left(42);
        var leftCalled = false;
        var rightCalled = false;

        // Act
        either.Match(
            onLeft: _ => leftCalled = true,
            onRight: _ => rightCalled = true);

        // Assert
        Assert.True(leftCalled);
        Assert.False(rightCalled);
    }

    [Fact]
    public void Match_ActionOverload_WithRight_ExecutesRightAction()
    {
        // Arrange
        var either = new Either<int, string>.Right("Hello");
        var leftCalled = false;
        var rightCalled = false;

        // Act
        either.Match(
            onLeft: _ => leftCalled = true,
            onRight: _ => rightCalled = true);

        // Assert
        Assert.False(leftCalled);
        Assert.True(rightCalled);
    }

    #endregion

    #region Map Tests

    [Fact]
    public void MapLeft_WithLeft_TransformsLeftValue()
    {
        // Arrange
        var either = new Either<int, string>.Left(42);

        // Act
        var result = either.MapLeft(x => x * 2);

        // Assert
        Assert.True(result.IsLeft);
        Assert.True(result.TryGetLeft(out var value));
        Assert.Equal(84, value);
    }

    [Fact]
    public void MapLeft_WithRight_ReturnsRightUnchanged()
    {
        // Arrange
        var either = new Either<int, string>.Right("Hello");

        // Act
        var result = either.MapLeft(x => x * 2);

        // Assert
        Assert.True(result.IsRight);
        Assert.True(result.TryGetRight(out var value));
        Assert.Equal("Hello", value);
    }

    [Fact]
    public void MapRight_WithRight_TransformsRightValue()
    {
        // Arrange
        var either = new Either<int, string>.Right("Hello");

        // Act
        var result = either.MapRight(x => x.ToUpper());

        // Assert
        Assert.True(result.IsRight);
        Assert.True(result.TryGetRight(out var value));
        Assert.Equal("HELLO", value);
    }

    [Fact]
    public void MapRight_WithLeft_ReturnsLeftUnchanged()
    {
        // Arrange
        var either = new Either<int, string>.Left(42);

        // Act
        var result = either.MapRight(x => x.ToUpper());

        // Assert
        Assert.True(result.IsLeft);
        Assert.True(result.TryGetLeft(out var value));
        Assert.Equal(42, value);
    }

    [Fact]
    public void Map_WithLeft_TransformsLeftValue()
    {
        // Arrange
        var either = new Either<int, string>.Left(42);

        // Act
        var result = either.Map(
            leftMapper: x => x.ToString(),
            rightMapper: x => x.Length.ToString());

        // Assert
        Assert.True(result.IsLeft);
        Assert.True(result.TryGetLeft(out var value));
        Assert.Equal("42", value);
    }

    [Fact]
    public void Map_WithRight_TransformsRightValue()
    {
        // Arrange
        var either = new Either<int, string>.Right("Hello");

        // Act
        var result = either.Map(
            leftMapper: x => x.ToString(),
            rightMapper: x => x.Length.ToString());

        // Assert
        Assert.True(result.IsRight);
        Assert.True(result.TryGetRight(out var value));
        Assert.Equal("5", value);
    }

    #endregion

    #region Swap Tests

    [Fact]
    public void Swap_WithLeft_BecomesRight()
    {
        // Arrange
        var either = new Either<int, string>.Left(42);

        // Act
        var swapped = either.Swap();

        // Assert
        Assert.True(swapped.IsRight);
        Assert.True(swapped.TryGetRight(out var value));
        Assert.Equal(42, value);
    }

    [Fact]
    public void Swap_WithRight_BecomesLeft()
    {
        // Arrange
        var either = new Either<int, string>.Right("Hello");

        // Act
        var swapped = either.Swap();

        // Assert
        Assert.True(swapped.IsLeft);
        Assert.True(swapped.TryGetLeft(out var value));
        Assert.Equal("Hello", value);
    }

    [Fact]
    public void Swap_Twice_ReturnsOriginal()
    {
        // Arrange
        var either = new Either<int, string>.Left(42);

        // Act
        var result = either.Swap().Swap();

        // Assert
        Assert.True(result.IsLeft);
        Assert.True(result.TryGetLeft(out var value));
        Assert.Equal(42, value);
    }

    #endregion

    #region Option Conversion Tests

    [Fact]
    public void LeftOption_WithLeft_ReturnsSome()
    {
        // Arrange
        var either = new Either<int, string>.Left(42);

        // Act
        var option = either.LeftOption();

        // Assert
        Assert.True(option.IsSome());
        if (option is Option<int>.Some some)
        {
            Assert.Equal(42, some.Value);
        }
    }

    [Fact]
    public void LeftOption_WithRight_ReturnsNone()
    {
        // Arrange
        var either = new Either<int, string>.Right("Hello");

        // Act
        var option = either.LeftOption();

        // Assert
        Assert.True(option.IsNone());
    }

    [Fact]
    public void RightOption_WithRight_ReturnsSome()
    {
        // Arrange
        var either = new Either<int, string>.Right("Hello");

        // Act
        var option = either.RightOption();

        // Assert
        Assert.True(option.IsSome());
        if (option is Option<string>.Some some)
        {
            Assert.Equal("Hello", some.Value);
        }
    }

    [Fact]
    public void RightOption_WithLeft_ReturnsNone()
    {
        // Arrange
        var either = new Either<int, string>.Left(42);

        // Act
        var option = either.RightOption();

        // Assert
        Assert.True(option.IsNone());
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_WithLeft_ShowsLeftValue()
    {
        // Arrange
        var either = new Either<int, string>.Left(42);

        // Act
        var str = either.ToString();

        // Assert
        Assert.Equal("Left(42)", str);
    }

    [Fact]
    public void ToString_WithRight_ShowsRightValue()
    {
        // Arrange
        var either = new Either<int, string>.Right("Hello");

        // Act
        var str = either.ToString();

        // Assert
        Assert.Equal("Right(Hello)", str);
    }

    #endregion

    #region Extension Method Tests

    [Fact]
    public void BindRight_WithRight_ChainsTransformation()
    {
        // Arrange
        var either = new Either<string, int>.Right(42);

        // Act
        var result = either.BindRight<string, int, string>(x =>
            x > 0
                ? new Either<string, string>.Right($"Positive: {x}")
                : new Either<string, string>.Left("Not positive"));

        // Assert
        Assert.True(result.IsRight);
        Assert.True(result.TryGetRight(out var value));
        Assert.Equal("Positive: 42", value);
    }

    [Fact]
    public void BindRight_WithLeft_ReturnsLeftUnchanged()
    {
        // Arrange
        var either = new Either<string, int>.Left("error");

        // Act
        var result = either.BindRight(x =>
            new Either<string, string>.Right($"Value: {x}"));

        // Assert
        Assert.True(result.IsLeft);
        Assert.True(result.TryGetLeft(out var value));
        Assert.Equal("error", value);
    }

    [Fact]
    public void BindLeft_WithLeft_ChainsTransformation()
    {
        // Arrange
        var either = new Either<int, string>.Left(42);

        // Act
        var result = either.BindLeft<int, string, string>(x =>
            x > 0
                ? new Either<string, string>.Left($"Positive: {x}")
                : new Either<string, string>.Right("Not positive"));

        // Assert
        Assert.True(result.IsLeft);
        Assert.True(result.TryGetLeft(out var value));
        Assert.Equal("Positive: 42", value);
    }

    [Fact]
    public void IfLeft_WithLeft_ExecutesAction()
    {
        // Arrange
        var either = new Either<int, string>.Left(42);
        var executed = false;
        var capturedValue = 0;

        // Act
        either.IfLeft(value =>
        {
            executed = true;
            capturedValue = value;
        });

        // Assert
        Assert.True(executed);
        Assert.Equal(42, capturedValue);
    }

    [Fact]
    public void IfRight_WithRight_ExecutesAction()
    {
        // Arrange
        var either = new Either<int, string>.Right("Hello");
        var executed = false;
        var capturedValue = "";

        // Act
        either.IfRight(value =>
        {
            executed = true;
            capturedValue = value;
        });

        // Assert
        Assert.True(executed);
        Assert.Equal("Hello", capturedValue);
    }

    [Fact]
    public void GetLeftOrDefault_WithLeft_ReturnsValue()
    {
        // Arrange
        var either = new Either<int, string>.Left(42);

        // Act
        var value = either.GetLeftOrDefault(0);

        // Assert
        Assert.Equal(42, value);
    }

    [Fact]
    public void GetLeftOrDefault_WithRight_ReturnsDefault()
    {
        // Arrange
        var either = new Either<int, string>.Right("Hello");

        // Act
        var value = either.GetLeftOrDefault(99);

        // Assert
        Assert.Equal(99, value);
    }

    [Fact]
    public void GetRightOrDefault_WithRight_ReturnsValue()
    {
        // Arrange
        var either = new Either<int, string>.Right("Hello");

        // Act
        var value = either.GetRightOrDefault("Default");

        // Assert
        Assert.Equal("Hello", value);
    }

    [Fact]
    public void GetRightOrDefault_WithLeft_ReturnsDefault()
    {
        // Arrange
        var either = new Either<int, string>.Left(42);

        // Act
        var value = either.GetRightOrDefault("Default");

        // Assert
        Assert.Equal("Default", value);
    }

    #endregion

    #region Collection Extension Tests

    [Fact]
    public void Partition_SplitsEithersCorrectly()
    {
        // Arrange
        Either<int, string>[] eithers = new Either<int, string>[]
        {
            new Either<int, string>.Left(1),
            new Either<int, string>.Right("A"),
            new Either<int, string>.Left(2),
            new Either<int, string>.Right("B"),
            new Either<int, string>.Left(3)
        };

        // Act
        var (lefts, rights) = eithers.Partition<int, string>();

        // Assert
        Assert.Equal(3, lefts.Count);
        Assert.Equal(2, rights.Count);
        Assert.Equal(new[] { 1, 2, 3 }, lefts);
        Assert.Equal(new[] { "A", "B" }, rights);
    }

    [Fact]
    public void Lefts_CollectsAllLeftValues()
    {
        // Arrange
        Either<int, string>[] eithers = new Either<int, string>[]
        {
            new Either<int, string>.Left(1),
            new Either<int, string>.Right("A"),
            new Either<int, string>.Left(2),
            new Either<int, string>.Right("B"),
            new Either<int, string>.Left(3)
        };

        // Act
        var lefts = eithers.Lefts<int, string>().ToList();

        // Assert
        Assert.Equal(3, lefts.Count);
        Assert.Equal(new[] { 1, 2, 3 }, lefts);
    }

    [Fact]
    public void Rights_CollectsAllRightValues()
    {
        // Arrange
        Either<int, string>[] eithers = new Either<int, string>[]
        {
            new Either<int, string>.Left(1),
            new Either<int, string>.Right("A"),
            new Either<int, string>.Left(2),
            new Either<int, string>.Right("B"),
            new Either<int, string>.Left(3)
        };

        // Act
        var rights = eithers.Rights<int, string>().ToList();

        // Assert
        Assert.Equal(2, rights.Count);
        Assert.Equal(new[] { "A", "B" }, rights);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void Integration_ConfigLoading()
    {
        // Simulate loading config from file OR environment
        Either<FileConfig, EnvConfig> LoadConfig(bool fileExists)
        {
            if (fileExists)
                return new Either<FileConfig, EnvConfig>.Left(new FileConfig("config.json"));
            else
                return new Either<FileConfig, EnvConfig>.Right(new EnvConfig("ENV_VAR"));
        }

        // Act - file exists
        var fileResult = LoadConfig(true);
        var fileMessage = fileResult.Match(
            onLeft: fc => $"Using file: {fc.Path}",
            onRight: ec => $"Using env: {ec.VarName}");

        // Act - file doesn't exist
        var envResult = LoadConfig(false);
        var envMessage = envResult.Match(
            onLeft: fc => $"Using file: {fc.Path}",
            onRight: ec => $"Using env: {ec.VarName}");

        // Assert
        Assert.Equal("Using file: config.json", fileMessage);
        Assert.Equal("Using env: ENV_VAR", envMessage);
    }

    [Fact]
    public void Integration_DataSourceSelection()
    {
        // Simulate choosing between cache and database
        Either<CachedData, DatabaseData> GetData(bool cached)
        {
            if (cached)
                return new Either<CachedData, DatabaseData>.Left(new CachedData(42, DateTime.UtcNow));
            else
                return new Either<CachedData, DatabaseData>.Right(new DatabaseData(42, true));
        }

        // Act
        var cachedResult = GetData(true);
        var dbResult = GetData(false);

        // Assert
        Assert.True(cachedResult.IsLeft);
        Assert.True(dbResult.IsRight);

        var cachedValue = cachedResult.LeftOption();
        var dbValue = dbResult.RightOption();

        Assert.True(cachedValue.IsSome());
        Assert.True(dbValue.IsSome());
    }

    [Fact]
    public void Integration_TransformationChain()
    {
        // Arrange
        var either = new Either<int, string>.Right("Hello");

        // Act
        var result = either
            .MapRight(s => s.ToUpper())
            .MapRight(s => s.Length)
            .BindRight<int, int, double>(len => len > 0
                ? new Either<int, double>.Right(len * 1.5)
                : new Either<int, double>.Left(-1));

        // Assert
        Assert.True(result.IsRight);
        Assert.True(result.TryGetRight(out var value));
        Assert.Equal(7.5, value);
    }

    #endregion

    #region Helper Types

    private record FileConfig(string Path);
    private record EnvConfig(string VarName);
    private record CachedData(int Id, DateTime CachedAt);
    private record DatabaseData(int Id, bool Fresh);

    #endregion
}
