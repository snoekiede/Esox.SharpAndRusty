using Esox.SharpAndRusty.AspNetCore.Middleware;
using Esox.SharpAndRusty.AspNetCore.ModelBinding;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Esox.SharpAndRusty.AspNetCore;

/// <summary>
/// Extension methods for configuring ASP.NET Core services and middleware for Esox.SharpAndRusty types.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Esox.SharpAndRusty ASP.NET Core integration services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSharpAndRusty(this IServiceCollection services)
    {
        return services.AddSharpAndRusty(_ => { });
    }

    /// <summary>
    /// Adds Esox.SharpAndRusty ASP.NET Core integration services with configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Configuration action for options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSharpAndRusty(
        this IServiceCollection services,
        Action<SharpAndRustyOptions> configure)
    {
        var options = new SharpAndRustyOptions();
        configure(options);

        // Add model binder providers
        services.Configure<Microsoft.AspNetCore.Mvc.MvcOptions>(mvcOptions =>
        {
            if (options.EnableOptionModelBinding)
            {
                mvcOptions.ModelBinderProviders.Insert(0, new OptionModelBinderProvider());
            }
        });

        return services;
    }

    /// <summary>
    /// Adds the Result middleware for global exception handling.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseResultMiddleware(this IApplicationBuilder app)
    {
        return app.UseResultMiddleware(new ResultMiddlewareOptions());
    }

    /// <summary>
    /// Adds the Result middleware for global exception handling with options.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <param name="options">The middleware options.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseResultMiddleware(
        this IApplicationBuilder app,
        ResultMiddlewareOptions options)
    {
        return app.UseMiddleware<ResultMiddleware>(options);
    }

    /// <summary>
    /// Adds the Result middleware configured for development.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseResultMiddlewareDevelopment(this IApplicationBuilder app)
    {
        return app.UseResultMiddleware(ResultMiddlewareOptions.Development());
    }

    /// <summary>
    /// Adds the Result middleware configured for production.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseResultMiddlewareProduction(this IApplicationBuilder app)
    {
        return app.UseResultMiddleware(ResultMiddlewareOptions.Production());
    }
}

/// <summary>
/// Options for configuring Esox.SharpAndRusty ASP.NET Core integration.
/// </summary>
public class SharpAndRustyOptions
{
    /// <summary>
    /// Gets or sets whether to enable Option{T} model binding (default: true).
    /// </summary>
    public bool EnableOptionModelBinding { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to enable Result{T, E} model binding (default: false).
    /// Results are typically returned, not received as input.
    /// </summary>
    public bool EnableResultModelBinding { get; set; } = false;
}
