using Esox.SharpAndRusty.Types;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Esox.SharpAndRusty.AspNetCore.ModelBinding;

/// <summary>
/// Model binder for <see cref="Option{T}"/> types that treats missing, null, or invalid values as <see cref="Option{T}.None"/> 
/// instead of generating model binding errors.
/// </summary>
/// <remarks>
/// <para>
/// This binder delegates to the appropriate inner binder for type T, then wraps the result in an Option{T}.
/// If the inner binder successfully binds a value, it's wrapped in <see cref="Option{T}.Some"/>.
/// If the inner binder fails (value is missing, null, or invalid), a <see cref="Option{T}.None"/> is created.
/// </para>
/// <para>
/// This allows optional parameters in API endpoints without triggering validation errors for missing values.
/// The implicit conversion operator in <see cref="Option{T}"/> also handles null values by converting them to None,
/// preventing the anti-pattern of Some(null).
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // In your controller:
/// [HttpGet]
/// public IActionResult GetUser([FromQuery] Option&lt;int&gt; age, [FromQuery] Option&lt;string&gt; name)
/// {
///     return age switch
///     {
///         Option&lt;int&gt;.Some s when s.Value > 0 => Ok($"Age: {s.Value}"),
///         _ => Ok("Age not specified")
///     };
/// }
/// </code>
/// </example>
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
    /// Attempts to bind a model to an <see cref="Option{T}"/> instance.
    /// </summary>
    /// <param name="bindingContext">The model binding context containing request data and metadata.</param>
    /// <returns>A task that represents the asynchronous bind operation.</returns>
    /// <remarks>
    /// This method delegates binding to the inner type's model binder. If binding succeeds and produces a value,
    /// that value is wrapped in <see cref="Option{T}.Some"/>. If binding fails or the value is null, 
    /// a <see cref="Option{T}.None"/> is returned. No model state errors are added for missing or null values,
    /// allowing truly optional parameters.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="bindingContext"/> is null.</exception>
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
            // Value was successfully bound - use implicit conversion or wrap in Some
            // The implicit conversion handles null values automatically by creating None
            var value = innerContext.Result.Model;
            var valueType = bindingContext.ModelMetadata.ModelType.GetGenericArguments()[0];

            // Create Some instance - use reflection to construct the appropriate type
            var someType = typeof(Option<>.Some).MakeGenericType(valueType);
            var someInstance = Activator.CreateInstance(someType, value);

            bindingContext.Result = ModelBindingResult.Success(someInstance);
        }
        else
        {
            // Value was not bound (missing, null, or validation error) - create None
            // This provides a clean way to represent "no value provided" without errors
            var valueType = bindingContext.ModelMetadata.ModelType.GetGenericArguments()[0];

            // Create None instance
            var noneType = typeof(Option<>.None).MakeGenericType(valueType);
            var noneInstance = Activator.CreateInstance(noneType);

            bindingContext.Result = ModelBindingResult.Success(noneInstance);
        }
    }

    /// <summary>
    /// Gets the metadata for the inner type T from the Option{T} context.
    /// </summary>
    /// <param name="context">The binding context containing the Option{T} metadata.</param>
    /// <returns>The metadata for the inner type T.</returns>
    private static ModelMetadata GetInnerMetadata(ModelBindingContext context)
    {
        var optionType = context.ModelMetadata.ModelType;
        var valueType = optionType.GetGenericArguments()[0];

        return context.ModelMetadata.GetMetadataForType(valueType);
    }
}

/// <summary>
/// Model binder provider for <see cref="Option{T}"/> types.
/// </summary>
/// <remarks>
/// <para>
/// Register this provider in your ASP.NET Core application to enable automatic model binding for Option{T} parameters.
/// </para>
/// <para>
/// Registration example:
/// <code>
/// builder.Services.AddControllers(options =>
/// {
///     options.ModelBinderProviders.Insert(0, new OptionModelBinderProvider());
/// });
/// </code>
/// </para>
/// <para>
/// Once registered, controller action parameters of type Option{T} will automatically be bound,
/// with missing or null values resulting in None rather than binding errors.
/// </para>
/// </remarks>
public class OptionModelBinderProvider : IModelBinderProvider
{
    /// <summary>
    /// Gets a model binder for <see cref="Option{T}"/> types, or null if the model type is not an Option{T}.
    /// </summary>
    /// <param name="context">The provider context containing model metadata and binder creation services.</param>
    /// <returns>
    /// An <see cref="OptionModelBinder"/> if the model type is Option{T}; otherwise, null to allow
    /// other providers to handle the binding.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
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
