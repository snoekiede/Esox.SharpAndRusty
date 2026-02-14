using Esox.SharpAndRusty.Types;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Esox.SharpAndRusty.AspNetCore.ModelBinding;

/// <summary>
/// Model binder for Option{T} types that treats missing/null values as None instead of validation errors.
/// </summary>
public class OptionModelBinder : IModelBinder
{
    private readonly IModelBinder _innerBinder;

    /// <summary>
    /// Initializes a new instance of the OptionModelBinder class.
    /// </summary>
    /// <param name="innerBinder">The inner model binder for type T.</param>
    public OptionModelBinder(IModelBinder innerBinder)
    {
        _innerBinder = innerBinder ?? throw new ArgumentNullException(nameof(innerBinder));
    }

    /// <summary>
    /// Attempts to bind a model.
    /// </summary>
    public async Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext == null)
        {
            throw new ArgumentNullException(nameof(bindingContext));
        }

        // Try to bind the inner value
        var innerContext = DefaultModelBindingContext.CreateBindingContext(
            bindingContext.ActionContext,
            bindingContext.ValueProvider,
            GetInnerMetadata(bindingContext),
            null, // bindingInfo
            bindingContext.ModelName);

        await _innerBinder.BindModelAsync(innerContext);

        if (innerContext.Result.IsModelSet)
        {
            // Value was successfully bound - wrap in Some
            var value = innerContext.Result.Model;
            var optionType = bindingContext.ModelType;
            var valueType = optionType.GetGenericArguments()[0];

            // Create Some instance using reflection
            var someType = typeof(Option<>).MakeGenericType(valueType)
                .GetNestedType("Some")!;
            var someInstance = Activator.CreateInstance(someType, value);

            bindingContext.Result = ModelBindingResult.Success(someInstance);
        }
        else
        {
            // Value was not bound (missing, null, or validation error) - create None
            var optionType = bindingContext.ModelType;
            var valueType = optionType.GetGenericArguments()[0];

            var noneType = typeof(Option<>).MakeGenericType(valueType)
                .GetNestedType("None")!;
            var noneInstance = Activator.CreateInstance(noneType);

            bindingContext.Result = ModelBindingResult.Success(noneInstance);
        }
    }

    private static ModelMetadata GetInnerMetadata(ModelBindingContext context)
    {
        var optionType = context.ModelMetadata.ModelType;
        var valueType = optionType.GetGenericArguments()[0];

        return context.ModelMetadata.GetMetadataForType(valueType);
    }
}

/// <summary>
/// Model binder provider for Option{T} types.
/// </summary>
public class OptionModelBinderProvider : IModelBinderProvider
{
    /// <summary>
    /// Gets a model binder for Option{T} types.
    /// </summary>
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        var modelType = context.Metadata.ModelType;

        // Check if this is an Option<T> type
        if (modelType.IsGenericType &&
            modelType.GetGenericTypeDefinition() == typeof(Option<>))
        {
            var valueType = modelType.GetGenericArguments()[0];
            var innerMetadata = context.MetadataProvider.GetMetadataForType(valueType);

            // Get binder for the inner type
            var innerBinder = context.CreateBinder(innerMetadata);

            return new OptionModelBinder(innerBinder);
        }

        return null;
    }
}
