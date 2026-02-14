using Esox.SharpAndRusty.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Esox.SharpAndRusty.AspNetCore.Middleware;

/// <summary>
/// Middleware that catches unhandled exceptions and converts them to standardized ProblemDetails responses.
/// </summary>
public class ResultMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ResultMiddleware> _logger;
    private readonly ResultMiddlewareOptions _options;

    /// <summary>
    /// Initializes a new instance of the ResultMiddleware class.
    /// </summary>
    public ResultMiddleware(
        RequestDelegate next,
        ILogger<ResultMiddleware> logger,
        ResultMiddlewareOptions? options = null)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options ?? new ResultMiddlewareOptions();
    }

    /// <summary>
    /// Invokes the middleware.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex) when (_options.HandleException(ex))
        {
            _logger.LogError(ex, "Unhandled exception occurred");

            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var error = Error.FromException(exception);

        if (_options.IncludeStackTrace && error.StackTrace == null)
        {
            error = error.CaptureStackTrace(includeFileInfo: _options.IncludeFileInfo);
        }

        // Add request context metadata
        error = error
            .WithMetadata("trace_id", context.TraceIdentifier)
            .WithMetadata("path", context.Request.Path.ToString());

        if (_options.CustomMetadataProvider != null)
        {
            error = _options.CustomMetadataProvider(context, error);
        }

        var problemDetails = error.ToProblemDetailsObject();
        problemDetails.Instance = context.Request.Path;

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;

        var json = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = _options.WriteIndented
        });

        await context.Response.WriteAsync(json);
    }
}

/// <summary>
/// Options for configuring the ResultMiddleware.
/// </summary>
public class ResultMiddlewareOptions
{
    /// <summary>
    /// Gets or sets whether to include stack traces in error responses (default: false).
    /// Should only be enabled in development environments.
    /// </summary>
    public bool IncludeStackTrace { get; set; } = false;

    /// <summary>
    /// Gets or sets whether to include file information in stack traces (default: false).
    /// </summary>
    public bool IncludeFileInfo { get; set; } = false;

    /// <summary>
    /// Gets or sets whether to write indented JSON (default: false).
    /// </summary>
    public bool WriteIndented { get; set; } = false;

    /// <summary>
    /// Gets or sets a function to determine which exceptions should be handled (default: all exceptions).
    /// </summary>
    public Func<Exception, bool> HandleException { get; set; } = _ => true;

    /// <summary>
    /// Gets or sets a function to add custom metadata to errors.
    /// </summary>
    public Func<HttpContext, Error, Error>? CustomMetadataProvider { get; set; }

    /// <summary>
    /// Creates options configured for development environment.
    /// </summary>
    public static ResultMiddlewareOptions Development() => new()
    {
        IncludeStackTrace = true,
        IncludeFileInfo = true,
        WriteIndented = true
    };

    /// <summary>
    /// Creates options configured for production environment.
    /// </summary>
    public static ResultMiddlewareOptions Production() => new()
    {
        IncludeStackTrace = false,
        IncludeFileInfo = false,
        WriteIndented = false,
        CustomMetadataProvider = (context, error) => error
            .WithMetadata("request_id", context.TraceIdentifier)
    };
}
