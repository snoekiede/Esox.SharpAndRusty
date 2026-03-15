using Esox.SharpAndRusty.AspNetCore.ModelBinding;
using Esox.SharpAndRusty.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Moq;
using Xunit;

namespace Esox.SharpAndRust.Tests.AspNetCore;

public class OptionModelBinderTests
{
    #region OptionModelBinder Constructor Tests

    [Fact]
    public void Constructor_WithValidInnerBinder_DoesNotThrow()
    {
        // Arrange
        var mockInnerBinder = new Mock<IModelBinder>();

        // Act & Assert
        var exception = Record.Exception(() => new OptionModelBinder(mockInnerBinder.Object));
        Assert.Null(exception);
    }

    [Fact]
    public void Constructor_WithNullInnerBinder_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new OptionModelBinder(null!));
        Assert.Equal("innerBinder", exception.ParamName);
    }

    #endregion

    #region BindModelAsync - Null Context Tests

    [Fact]
    public async Task BindModelAsync_WithNullContext_ThrowsArgumentNullException()
    {
        // Arrange
        var mockInnerBinder = new Mock<IModelBinder>();
        var binder = new OptionModelBinder(mockInnerBinder.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await binder.BindModelAsync(null!));
        Assert.Equal("bindingContext", exception.ParamName);
    }

    #endregion

    #region BindModelAsync - Success Binding Tests (Some)

    [Fact]
    public async Task BindModelAsync_WithSuccessfulInnerBinding_ReturnsSome()
    {
        // Arrange
        var (binder, context, mockInnerBinder) = CreateBinderAndContext<int>();
        var boundValue = 42;

        // Configure inner binder to succeed
        mockInnerBinder.Setup(b => b.BindModelAsync(It.IsAny<ModelBindingContext>()))
            .Callback<ModelBindingContext>(ctx =>
            {
                ctx.Result = ModelBindingResult.Success(boundValue);
            })
            .Returns(Task.CompletedTask);

        // Act
        await binder.BindModelAsync(context);

        // Assert
        Assert.True(context.Result.IsModelSet);
        Assert.NotNull(context.Result.Model);
        var option = Assert.IsType<Option<int>.Some>(context.Result.Model);
        Assert.Equal(boundValue, option.Value);
    }

    [Fact]
    public async Task BindModelAsync_WithStringValue_ReturnsSomeString()
    {
        // Arrange
        var (binder, context, mockInnerBinder) = CreateBinderAndContext<string>();
        var boundValue = "hello world";

        mockInnerBinder.Setup(b => b.BindModelAsync(It.IsAny<ModelBindingContext>()))
            .Callback<ModelBindingContext>(ctx =>
            {
                ctx.Result = ModelBindingResult.Success(boundValue);
            })
            .Returns(Task.CompletedTask);

        // Act
        await binder.BindModelAsync(context);

        // Assert
        Assert.True(context.Result.IsModelSet);
        var option = Assert.IsType<Option<string>.Some>(context.Result.Model);
        Assert.Equal(boundValue, option.Value);
    }

    [Fact]
    public async Task BindModelAsync_WithComplexType_ReturnsSomeComplexType()
    {
        // Arrange
        var (binder, context, mockInnerBinder) = CreateBinderAndContext<TestPerson>();
        var boundValue = new TestPerson { Name = "Alice", Age = 30 };

        mockInnerBinder.Setup(b => b.BindModelAsync(It.IsAny<ModelBindingContext>()))
            .Callback<ModelBindingContext>(ctx =>
            {
                ctx.Result = ModelBindingResult.Success(boundValue);
            })
            .Returns(Task.CompletedTask);

        // Act
        await binder.BindModelAsync(context);

        // Assert
        Assert.True(context.Result.IsModelSet);
        var option = Assert.IsType<Option<TestPerson>.Some>(context.Result.Model);
        Assert.Equal(boundValue.Name, option.Value.Name);
        Assert.Equal(boundValue.Age, option.Value.Age);
    }

    [Fact]
    public async Task BindModelAsync_WithNullableType_ReturnsSomeNullable()
    {
        // Arrange
        var (binder, context, mockInnerBinder) = CreateBinderAndContext<int?>();
        int? boundValue = 42;

        mockInnerBinder.Setup(b => b.BindModelAsync(It.IsAny<ModelBindingContext>()))
            .Callback<ModelBindingContext>(ctx =>
            {
                ctx.Result = ModelBindingResult.Success(boundValue);
            })
            .Returns(Task.CompletedTask);

        // Act
        await binder.BindModelAsync(context);

        // Assert
        Assert.True(context.Result.IsModelSet);
        var option = Assert.IsType<Option<int?>.Some>(context.Result.Model);
        Assert.Equal(boundValue, option.Value);
    }

    [Fact]
    public async Task BindModelAsync_WithBooleanTrue_ReturnsSomeTrue()
    {
        // Arrange
        var (binder, context, mockInnerBinder) = CreateBinderAndContext<bool>();
        var boundValue = true;

        mockInnerBinder.Setup(b => b.BindModelAsync(It.IsAny<ModelBindingContext>()))
            .Callback<ModelBindingContext>(ctx =>
            {
                ctx.Result = ModelBindingResult.Success(boundValue);
            })
            .Returns(Task.CompletedTask);

        // Act
        await binder.BindModelAsync(context);

        // Assert
        Assert.True(context.Result.IsModelSet);
        var option = Assert.IsType<Option<bool>.Some>(context.Result.Model);
        Assert.True(option.Value);
    }

    [Fact]
    public async Task BindModelAsync_WithBooleanFalse_ReturnsSomeFalse()
    {
        // Arrange
        var (binder, context, mockInnerBinder) = CreateBinderAndContext<bool>();
        var boundValue = false;

        mockInnerBinder.Setup(b => b.BindModelAsync(It.IsAny<ModelBindingContext>()))
            .Callback<ModelBindingContext>(ctx =>
            {
                ctx.Result = ModelBindingResult.Success(boundValue);
            })
            .Returns(Task.CompletedTask);

        // Act
        await binder.BindModelAsync(context);

        // Assert
        Assert.True(context.Result.IsModelSet);
        var option = Assert.IsType<Option<bool>.Some>(context.Result.Model);
        Assert.False(option.Value);
    }

    [Fact]
    public async Task BindModelAsync_WithGuid_ReturnsSomeGuid()
    {
        // Arrange
        var (binder, context, mockInnerBinder) = CreateBinderAndContext<Guid>();
        var boundValue = Guid.NewGuid();

        mockInnerBinder.Setup(b => b.BindModelAsync(It.IsAny<ModelBindingContext>()))
            .Callback<ModelBindingContext>(ctx =>
            {
                ctx.Result = ModelBindingResult.Success(boundValue);
            })
            .Returns(Task.CompletedTask);

        // Act
        await binder.BindModelAsync(context);

        // Assert
        Assert.True(context.Result.IsModelSet);
        var option = Assert.IsType<Option<Guid>.Some>(context.Result.Model);
        Assert.Equal(boundValue, option.Value);
    }

    [Fact]
    public async Task BindModelAsync_WithDateTime_ReturnsSomeDateTime()
    {
        // Arrange
        var (binder, context, mockInnerBinder) = CreateBinderAndContext<DateTime>();
        var boundValue = DateTime.UtcNow;

        mockInnerBinder.Setup(b => b.BindModelAsync(It.IsAny<ModelBindingContext>()))
            .Callback<ModelBindingContext>(ctx =>
            {
                ctx.Result = ModelBindingResult.Success(boundValue);
            })
            .Returns(Task.CompletedTask);

        // Act
        await binder.BindModelAsync(context);

        // Assert
        Assert.True(context.Result.IsModelSet);
        var option = Assert.IsType<Option<DateTime>.Some>(context.Result.Model);
        Assert.Equal(boundValue, option.Value);
    }

    [Fact]
    public async Task BindModelAsync_WithDecimal_ReturnsSomeDecimal()
    {
        // Arrange
        var (binder, context, mockInnerBinder) = CreateBinderAndContext<decimal>();
        var boundValue = 123.45m;

        mockInnerBinder.Setup(b => b.BindModelAsync(It.IsAny<ModelBindingContext>()))
            .Callback<ModelBindingContext>(ctx =>
            {
                ctx.Result = ModelBindingResult.Success(boundValue);
            })
            .Returns(Task.CompletedTask);

        // Act
        await binder.BindModelAsync(context);

        // Assert
        Assert.True(context.Result.IsModelSet);
        var option = Assert.IsType<Option<decimal>.Some>(context.Result.Model);
        Assert.Equal(boundValue, option.Value);
    }

    [Fact]
    public async Task BindModelAsync_WithCollectionType_ReturnsSomeCollection()
    {
        // Arrange
        var (binder, context, mockInnerBinder) = CreateBinderAndContext<List<int>>();
        var boundValue = new List<int> { 1, 2, 3, 4, 5 };

        mockInnerBinder.Setup(b => b.BindModelAsync(It.IsAny<ModelBindingContext>()))
            .Callback<ModelBindingContext>(ctx =>
            {
                ctx.Result = ModelBindingResult.Success(boundValue);
            })
            .Returns(Task.CompletedTask);

        // Act
        await binder.BindModelAsync(context);

        // Assert
        Assert.True(context.Result.IsModelSet);
        var option = Assert.IsType<Option<List<int>>.Some>(context.Result.Model);
        Assert.Equal(boundValue, option.Value);
    }

    #endregion

    #region BindModelAsync - Failed Binding Tests (None)

    [Fact]
    public async Task BindModelAsync_WithFailedInnerBinding_ReturnsNone()
    {
        // Arrange
        var (binder, context, mockInnerBinder) = CreateBinderAndContext<int>();

        // Configure inner binder to fail
        mockInnerBinder.Setup(b => b.BindModelAsync(It.IsAny<ModelBindingContext>()))
            .Callback<ModelBindingContext>(ctx =>
            {
                ctx.Result = ModelBindingResult.Failed();
            })
            .Returns(Task.CompletedTask);

        // Act
        await binder.BindModelAsync(context);

        // Assert
        Assert.True(context.Result.IsModelSet);
        Assert.NotNull(context.Result.Model);
        Assert.IsType<Option<int>.None>(context.Result.Model);
    }

    [Fact]
    public async Task BindModelAsync_WithMissingValue_ReturnsNone()
    {
        // Arrange
        var (binder, context, mockInnerBinder) = CreateBinderAndContext<string>();

        // Configure inner binder to return no model (not set)
        mockInnerBinder.Setup(b => b.BindModelAsync(It.IsAny<ModelBindingContext>()))
            .Returns(Task.CompletedTask);

        // Act
        await binder.BindModelAsync(context);

        // Assert
        Assert.True(context.Result.IsModelSet);
        Assert.IsType<Option<string>.None>(context.Result.Model);
    }

    [Fact]
    public async Task BindModelAsync_WithNullValueFromInnerBinder_ReturnsSomeNull()
    {
        // Arrange
        var (binder, context, mockInnerBinder) = CreateBinderAndContext<string>();

        // Configure inner binder to succeed with null value
        mockInnerBinder.Setup(b => b.BindModelAsync(It.IsAny<ModelBindingContext>()))
            .Callback<ModelBindingContext>(ctx =>
            {
                ctx.Result = ModelBindingResult.Success(null);
            })
            .Returns(Task.CompletedTask);

        // Act
        await binder.BindModelAsync(context);

        // Assert
        Assert.True(context.Result.IsModelSet);
        var option = Assert.IsType<Option<string>.Some>(context.Result.Model);
        Assert.Null(option.Value);
    }

    [Fact]
    public async Task BindModelAsync_WithValidationError_ReturnsNone()
    {
        // Arrange
        var (binder, context, mockInnerBinder) = CreateBinderAndContext<int>();

        // Configure inner binder to fail with validation error
        mockInnerBinder.Setup(b => b.BindModelAsync(It.IsAny<ModelBindingContext>()))
            .Callback<ModelBindingContext>(ctx =>
            {
                ctx.ModelState.AddModelError("key", "Invalid value");
                ctx.Result = ModelBindingResult.Failed();
            })
            .Returns(Task.CompletedTask);

        // Act
        await binder.BindModelAsync(context);

        // Assert
        Assert.True(context.Result.IsModelSet);
        Assert.IsType<Option<int>.None>(context.Result.Model);
    }

    #endregion

    #region Edge Cases and Complex Scenarios

    [Fact]
    public async Task BindModelAsync_WithDifferentModelNames_PassesCorrectModelName()
    {
        // Arrange
        var modelName = "customModelName";
        var (binder, context, mockInnerBinder) = CreateBinderAndContext<int>(modelName);
        string? capturedModelName = null;

        mockInnerBinder.Setup(b => b.BindModelAsync(It.IsAny<ModelBindingContext>()))
            .Callback<ModelBindingContext>(ctx =>
            {
                capturedModelName = ctx.ModelName;
                ctx.Result = ModelBindingResult.Success(42);
            })
            .Returns(Task.CompletedTask);

        // Act
        await binder.BindModelAsync(context);

        // Assert
        Assert.Equal(modelName, capturedModelName);
    }

    [Fact]
    public async Task BindModelAsync_WithNestedOptionType_HandlesCorrectly()
    {
        // Arrange
        var (binder, context, mockInnerBinder) = CreateBinderAndContext<Option<int>>();
        var boundValue = new Option<int>.Some(42);

        mockInnerBinder.Setup(b => b.BindModelAsync(It.IsAny<ModelBindingContext>()))
            .Callback<ModelBindingContext>(ctx =>
            {
                ctx.Result = ModelBindingResult.Success(boundValue);
            })
            .Returns(Task.CompletedTask);

        // Act
        await binder.BindModelAsync(context);

        // Assert
        Assert.True(context.Result.IsModelSet);
        var option = Assert.IsType<Option<Option<int>>.Some>(context.Result.Model);
        var innerOption = Assert.IsType<Option<int>.Some>(option.Value);
        Assert.Equal(42, innerOption.Value);
    }

    [Fact]
    public async Task BindModelAsync_WithEnum_ReturnsSomeEnum()
    {
        // Arrange
        var (binder, context, mockInnerBinder) = CreateBinderAndContext<TestEnum>();
        var boundValue = TestEnum.Value2;

        mockInnerBinder.Setup(b => b.BindModelAsync(It.IsAny<ModelBindingContext>()))
            .Callback<ModelBindingContext>(ctx =>
            {
                ctx.Result = ModelBindingResult.Success(boundValue);
            })
            .Returns(Task.CompletedTask);

        // Act
        await binder.BindModelAsync(context);

        // Assert
        Assert.True(context.Result.IsModelSet);
        var option = Assert.IsType<Option<TestEnum>.Some>(context.Result.Model);
        Assert.Equal(TestEnum.Value2, option.Value);
    }

    [Fact]
    public async Task BindModelAsync_WithRecord_ReturnsSomeRecord()
    {
        // Arrange
        var (binder, context, mockInnerBinder) = CreateBinderAndContext<TestRecord>();
        var boundValue = new TestRecord(42, "test");

        mockInnerBinder.Setup(b => b.BindModelAsync(It.IsAny<ModelBindingContext>()))
            .Callback<ModelBindingContext>(ctx =>
            {
                ctx.Result = ModelBindingResult.Success(boundValue);
            })
            .Returns(Task.CompletedTask);

        // Act
        await binder.BindModelAsync(context);

        // Assert
        Assert.True(context.Result.IsModelSet);
        var option = Assert.IsType<Option<TestRecord>.Some>(context.Result.Model);
        Assert.Equal(boundValue.Id, option.Value.Id);
        Assert.Equal(boundValue.Name, option.Value.Name);
    }

    [Fact]
    public async Task BindModelAsync_WithStruct_ReturnsSomeStruct()
    {
        // Arrange
        var (binder, context, mockInnerBinder) = CreateBinderAndContext<TestStruct>();
        var boundValue = new TestStruct { X = 10, Y = 20 };

        mockInnerBinder.Setup(b => b.BindModelAsync(It.IsAny<ModelBindingContext>()))
            .Callback<ModelBindingContext>(ctx =>
            {
                ctx.Result = ModelBindingResult.Success(boundValue);
            })
            .Returns(Task.CompletedTask);

        // Act
        await binder.BindModelAsync(context);

        // Assert
        Assert.True(context.Result.IsModelSet);
        var option = Assert.IsType<Option<TestStruct>.Some>(context.Result.Model);
        Assert.Equal(boundValue.X, option.Value.X);
        Assert.Equal(boundValue.Y, option.Value.Y);
    }

    [Fact]
    public async Task BindModelAsync_CallsInnerBinderExactlyOnce()
    {
        // Arrange
        var (binder, context, mockInnerBinder) = CreateBinderAndContext<int>();

        mockInnerBinder.Setup(b => b.BindModelAsync(It.IsAny<ModelBindingContext>()))
            .Callback<ModelBindingContext>(ctx =>
            {
                ctx.Result = ModelBindingResult.Success(42);
            })
            .Returns(Task.CompletedTask);

        // Act
        await binder.BindModelAsync(context);

        // Assert
        mockInnerBinder.Verify(b => b.BindModelAsync(It.IsAny<ModelBindingContext>()), Times.Once);
    }

    #endregion

    #region Helper Methods

    private static (OptionModelBinder binder, ModelBindingContext context, Mock<IModelBinder> mockInnerBinder) 
        CreateBinderAndContext<T>(string modelName = "testModel")
    {
        var mockInnerBinder = new Mock<IModelBinder>();
        var binder = new OptionModelBinder(mockInnerBinder.Object);

        var httpContext = new DefaultHttpContext();
        var actionContext = new ActionContext(httpContext, new Microsoft.AspNetCore.Routing.RouteData(), new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor());

        var metadataProvider = new EmptyModelMetadataProvider();
        var metadata = metadataProvider.GetMetadataForType(typeof(Option<T>));

        var valueProvider = new Mock<IValueProvider>();
        var modelState = new ModelStateDictionary();

        var context = new DefaultModelBindingContext
        {
            ActionContext = actionContext,
            ModelMetadata = metadata,
            ModelName = modelName,
            ValueProvider = valueProvider.Object,
            ModelState = modelState
        };

        return (binder, context, mockInnerBinder);
    }

    #endregion

    #region Test Types

    public class TestPerson
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }

    public enum TestEnum
    {
        Value1,
        Value2,
        Value3
    }

    public record TestRecord(int Id, string Name);

    public struct TestStruct
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

    #endregion
}

public class OptionModelBinderProviderTests
{
    #region Constructor and Null Handling Tests

    [Fact]
    public void GetBinder_WithNullContext_ThrowsArgumentNullException()
    {
        // Arrange
        var provider = new OptionModelBinderProvider();

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => provider.GetBinder(null!));
        Assert.Equal("context", exception.ParamName);
    }

    #endregion

    #region GetBinder - Option Type Tests

    [Fact]
    public void GetBinder_WithOptionIntType_ReturnsOptionModelBinder()
    {
        // Arrange
        var provider = new OptionModelBinderProvider();
        var context = CreateProviderContext<Option<int>>();

        // Act
        var binder = provider.GetBinder(context);

        // Assert
        Assert.NotNull(binder);
        Assert.IsType<OptionModelBinder>(binder);
    }

    [Fact]
    public void GetBinder_WithOptionStringType_ReturnsOptionModelBinder()
    {
        // Arrange
        var provider = new OptionModelBinderProvider();
        var context = CreateProviderContext<Option<string>>();

        // Act
        var binder = provider.GetBinder(context);

        // Assert
        Assert.NotNull(binder);
        Assert.IsType<OptionModelBinder>(binder);
    }

    [Fact]
    public void GetBinder_WithOptionComplexType_ReturnsOptionModelBinder()
    {
        // Arrange
        var provider = new OptionModelBinderProvider();
        var context = CreateProviderContext<Option<TestPerson>>();

        // Act
        var binder = provider.GetBinder(context);

        // Assert
        Assert.NotNull(binder);
        Assert.IsType<OptionModelBinder>(binder);
    }

    [Fact]
    public void GetBinder_WithOptionNullableType_ReturnsOptionModelBinder()
    {
        // Arrange
        var provider = new OptionModelBinderProvider();
        var context = CreateProviderContext<Option<int?>>();

        // Act
        var binder = provider.GetBinder(context);

        // Assert
        Assert.NotNull(binder);
        Assert.IsType<OptionModelBinder>(binder);
    }

    [Fact]
    public void GetBinder_WithNestedOption_ReturnsOptionModelBinder()
    {
        // Arrange
        var provider = new OptionModelBinderProvider();
        var context = CreateProviderContext<Option<Option<int>>>();

        // Act
        var binder = provider.GetBinder(context);

        // Assert
        Assert.NotNull(binder);
        Assert.IsType<OptionModelBinder>(binder);
    }

    #endregion

    #region GetBinder - Non-Option Type Tests

    [Fact]
    public void GetBinder_WithIntType_ReturnsNull()
    {
        // Arrange
        var provider = new OptionModelBinderProvider();
        var context = CreateProviderContext<int>();

        // Act
        var binder = provider.GetBinder(context);

        // Assert
        Assert.Null(binder);
    }

    [Fact]
    public void GetBinder_WithStringType_ReturnsNull()
    {
        // Arrange
        var provider = new OptionModelBinderProvider();
        var context = CreateProviderContext<string>();

        // Act
        var binder = provider.GetBinder(context);

        // Assert
        Assert.Null(binder);
    }

    [Fact]
    public void GetBinder_WithComplexType_ReturnsNull()
    {
        // Arrange
        var provider = new OptionModelBinderProvider();
        var context = CreateProviderContext<TestPerson>();

        // Act
        var binder = provider.GetBinder(context);

        // Assert
        Assert.Null(binder);
    }

    [Fact]
    public void GetBinder_WithListType_ReturnsNull()
    {
        // Arrange
        var provider = new OptionModelBinderProvider();
        var context = CreateProviderContext<List<int>>();

        // Act
        var binder = provider.GetBinder(context);

        // Assert
        Assert.Null(binder);
    }

    [Fact]
    public void GetBinder_WithNullableType_ReturnsNull()
    {
        // Arrange
        var provider = new OptionModelBinderProvider();
        var context = CreateProviderContext<int?>();

        // Act
        var binder = provider.GetBinder(context);

        // Assert
        Assert.Null(binder);
    }

    [Fact]
    public void GetBinder_WithResultType_ReturnsNull()
    {
        // Arrange
        var provider = new OptionModelBinderProvider();
        var context = CreateProviderContext<Result<int, string>>();

        // Act
        var binder = provider.GetBinder(context);

        // Assert
        Assert.Null(binder);
    }

    #endregion

    #region GetBinder - Edge Cases

    [Fact]
    public void GetBinder_WithOptionGuid_ReturnsOptionModelBinder()
    {
        // Arrange
        var provider = new OptionModelBinderProvider();
        var context = CreateProviderContext<Option<Guid>>();

        // Act
        var binder = provider.GetBinder(context);

        // Assert
        Assert.NotNull(binder);
        Assert.IsType<OptionModelBinder>(binder);
    }

    [Fact]
    public void GetBinder_WithOptionDateTime_ReturnsOptionModelBinder()
    {
        // Arrange
        var provider = new OptionModelBinderProvider();
        var context = CreateProviderContext<Option<DateTime>>();

        // Act
        var binder = provider.GetBinder(context);

        // Assert
        Assert.NotNull(binder);
        Assert.IsType<OptionModelBinder>(binder);
    }

    [Fact]
    public void GetBinder_WithOptionDecimal_ReturnsOptionModelBinder()
    {
        // Arrange
        var provider = new OptionModelBinderProvider();
        var context = CreateProviderContext<Option<decimal>>();

        // Act
        var binder = provider.GetBinder(context);

        // Assert
        Assert.NotNull(binder);
        Assert.IsType<OptionModelBinder>(binder);
    }

    [Fact]
    public void GetBinder_WithOptionBool_ReturnsOptionModelBinder()
    {
        // Arrange
        var provider = new OptionModelBinderProvider();
        var context = CreateProviderContext<Option<bool>>();

        // Act
        var binder = provider.GetBinder(context);

        // Assert
        Assert.NotNull(binder);
        Assert.IsType<OptionModelBinder>(binder);
    }

    [Fact]
    public void GetBinder_WithOptionEnum_ReturnsOptionModelBinder()
    {
        // Arrange
        var provider = new OptionModelBinderProvider();
        var context = CreateProviderContext<Option<TestEnum>>();

        // Act
        var binder = provider.GetBinder(context);

        // Assert
        Assert.NotNull(binder);
        Assert.IsType<OptionModelBinder>(binder);
    }

    [Fact]
    public void GetBinder_WithOptionRecord_ReturnsOptionModelBinder()
    {
        // Arrange
        var provider = new OptionModelBinderProvider();
        var context = CreateProviderContext<Option<TestRecord>>();

        // Act
        var binder = provider.GetBinder(context);

        // Assert
        Assert.NotNull(binder);
        Assert.IsType<OptionModelBinder>(binder);
    }

    [Fact]
    public void GetBinder_WithOptionStruct_ReturnsOptionModelBinder()
    {
        // Arrange
        var provider = new OptionModelBinderProvider();
        var context = CreateProviderContext<Option<TestStruct>>();

        // Act
        var binder = provider.GetBinder(context);

        // Assert
        Assert.NotNull(binder);
        Assert.IsType<OptionModelBinder>(binder);
    }

    [Fact]
    public void GetBinder_WithOptionListType_ReturnsOptionModelBinder()
    {
        // Arrange
        var provider = new OptionModelBinderProvider();
        var context = CreateProviderContext<Option<List<int>>>();

        // Act
        var binder = provider.GetBinder(context);

        // Assert
        Assert.NotNull(binder);
        Assert.IsType<OptionModelBinder>(binder);
    }

    #endregion

    #region Provider Consistency Tests

    [Fact]
    public void GetBinder_CalledMultipleTimesWithSameType_ReturnsNewInstances()
    {
        // Arrange
        var provider = new OptionModelBinderProvider();
        var context = CreateProviderContext<Option<int>>();

        // Act
        var binder1 = provider.GetBinder(context);
        var binder2 = provider.GetBinder(context);

        // Assert
        Assert.NotNull(binder1);
        Assert.NotNull(binder2);
        Assert.NotSame(binder1, binder2);
    }

    [Fact]
    public void GetBinder_WithDifferentOptionTypes_ReturnsBindersForEach()
    {
        // Arrange
        var provider = new OptionModelBinderProvider();
        var intContext = CreateProviderContext<Option<int>>();
        var stringContext = CreateProviderContext<Option<string>>();

        // Act
        var intBinder = provider.GetBinder(intContext);
        var stringBinder = provider.GetBinder(stringContext);

        // Assert
        Assert.NotNull(intBinder);
        Assert.NotNull(stringBinder);
        Assert.IsType<OptionModelBinder>(intBinder);
        Assert.IsType<OptionModelBinder>(stringBinder);
    }

    #endregion

    #region Helper Methods

    private static ModelBinderProviderContext CreateProviderContext<T>()
    {
        var metadataProvider = new EmptyModelMetadataProvider();
        var metadata = metadataProvider.GetMetadataForType(typeof(T));

        var mockContext = new Mock<ModelBinderProviderContext>();
        mockContext.SetupGet(c => c.Metadata).Returns(metadata);
        mockContext.SetupGet(c => c.MetadataProvider).Returns(metadataProvider);

        // Mock CreateBinder to return a simple mock binder
        mockContext.Setup(c => c.CreateBinder(It.IsAny<ModelMetadata>()))
            .Returns(new Mock<IModelBinder>().Object);

        return mockContext.Object;
    }

    #endregion

    #region Test Types

    public class TestPerson
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }

    public enum TestEnum
    {
        Value1,
        Value2,
        Value3
    }

    public record TestRecord(int Id, string Name);

    public struct TestStruct
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

    #endregion
}
