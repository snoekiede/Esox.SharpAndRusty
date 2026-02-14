using Esox.SharpAndRusty.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Esox.SharpAndRusty.AspNetCore;

/// <summary>
/// Provides extension methods for converting Error types to RFC 7807 ProblemDetails responses.
/// </summary>
public static class ProblemDetailsExtensions
{
    /// <summary>
    /// Converts an Error to an IActionResult with RFC 7807 ProblemDetails format.
    /// The HTTP status code is automatically determined from the ErrorKind.
    /// </summary>
    /// <param name="error">The error to convert.</param>
    /// <returns>An ObjectResult containing a ProblemDetails instance.</returns>
    public static IActionResult ToProblemDetails(this Error error)
    {
        var statusCode = error.Kind.ToStatusCode();
        var problemDetails = error.ToProblemDetailsObject();
        problemDetails.Status = statusCode;

        return new ObjectResult(problemDetails)
        {
            StatusCode = statusCode
        };
    }

    /// <summary>
    /// Converts an Error to a ProblemDetails object.
    /// </summary>
    /// <param name="error">The error to convert.</param>
    /// <returns>A ProblemDetails instance.</returns>
    public static ProblemDetails ToProblemDetailsObject(this Error error)
    {
        var problemDetails = new ProblemDetails
        {
            Title = error.Kind.ToTitle(),
            Detail = error.GetFullMessage(),
            Status = error.Kind.ToStatusCode(),
            Type = error.Kind.ToRfc7807Type()
        };

        // Note: ASP.NET Core 2.3.9 ProblemDetails doesn't have Extensions property
        // If you need to include error chains and metadata, upgrade to a newer ASP.NET Core version
        // or use a custom ProblemDetails class that inherits from the base

        return problemDetails;
    }

    /// <summary>
    /// Maps an ErrorKind to an appropriate HTTP status code.
    /// </summary>
    /// <param name="kind">The error kind.</param>
    /// <returns>The corresponding HTTP status code.</returns>
    public static int ToStatusCode(this ErrorKind kind)
    {
        return kind switch
        {
            ErrorKind.NotFound => StatusCodes.Status404NotFound,
            ErrorKind.PermissionDenied => StatusCodes.Status403Forbidden,
            ErrorKind.ConnectionRefused => StatusCodes.Status503ServiceUnavailable,
            ErrorKind.ConnectionReset => StatusCodes.Status503ServiceUnavailable,
            ErrorKind.Timeout => StatusCodes.Status408RequestTimeout,
            ErrorKind.Interrupted => 499, // Client Closed Request (non-standard but widely used)
            ErrorKind.InvalidInput => StatusCodes.Status400BadRequest,
            ErrorKind.NotSupported => StatusCodes.Status501NotImplemented,
            ErrorKind.InvalidOperation => StatusCodes.Status400BadRequest,
            ErrorKind.Io => StatusCodes.Status500InternalServerError,
            ErrorKind.ParseError => StatusCodes.Status400BadRequest,
            ErrorKind.ResourceExhausted => StatusCodes.Status429TooManyRequests,
            ErrorKind.AlreadyExists => StatusCodes.Status409Conflict,
            ErrorKind.InvalidState => StatusCodes.Status400BadRequest,
            ErrorKind.Other => StatusCodes.Status500InternalServerError,
            _ => StatusCodes.Status500InternalServerError
        };
    }

    /// <summary>
    /// Maps an ErrorKind to a human-readable title for ProblemDetails.
    /// </summary>
    /// <param name="kind">The error kind.</param>
    /// <returns>A descriptive title.</returns>
    public static string ToTitle(this ErrorKind kind)
    {
        return kind switch
        {
            ErrorKind.NotFound => "Not Found",
            ErrorKind.PermissionDenied => "Forbidden",
            ErrorKind.ConnectionRefused => "Service Unavailable",
            ErrorKind.ConnectionReset => "Service Unavailable",
            ErrorKind.Timeout => "Request Timeout",
            ErrorKind.Interrupted => "Request Cancelled",
            ErrorKind.InvalidInput => "Bad Request",
            ErrorKind.NotSupported => "Not Implemented",
            ErrorKind.InvalidOperation => "Bad Request",
            ErrorKind.Io => "Internal Server Error",
            ErrorKind.ParseError => "Bad Request",
            ErrorKind.ResourceExhausted => "Too Many Requests",
            ErrorKind.AlreadyExists => "Conflict",
            ErrorKind.InvalidState => "Invalid State",
            ErrorKind.Other => "Internal Server Error",
            _ => "Internal Server Error"
        };
    }

    /// <summary>
    /// Maps an ErrorKind to an RFC 7807 problem type URI.
    /// </summary>
    /// <param name="kind">The error kind.</param>
    /// <returns>A problem type URI.</returns>
    public static string ToRfc7807Type(this ErrorKind kind)
    {
        var kebabCase = kind.ToString()
            .Select((c, i) => i > 0 && char.IsUpper(c) ? $"-{char.ToLower(c)}" : char.ToLower(c).ToString())
            .Aggregate((a, b) => a + b);

        return $"https://tools.ietf.org/html/rfc7231#section-6.{kind.ToStatusCode() / 100}.{kind.ToStatusCode() % 100}";
    }

    private static List<object> GetErrorChain(Error error)
    {
        var chain = new List<object>();
        var current = error;

        while (current != null)
        {
            var errorInfo = new Dictionary<string, object>
            {
                ["message"] = current.Message,
                ["kind"] = current.Kind.ToString()
            };

            if (current.StackTrace != null)
            {
                errorInfo["stackTrace"] = current.StackTrace;
            }

            chain.Add(errorInfo);
            current = current.Source;
        }

        return chain;
    }
}

/// <summary>
/// Extension status codes not in the standard ASP.NET Core StatusCodes class.
/// </summary>
public static class ExtendedStatusCodes
{
    /// <summary>
    /// HTTP status code 499 - Client Closed Request (Nginx extension).
    /// Used when the client closes the connection before the server finishes processing.
    /// </summary>
    public const int Status499ClientClosedRequest = 499;
}
