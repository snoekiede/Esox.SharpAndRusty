using Esox.SharpAndRusty.Types;
using Esox.SharpAndRusty.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Esox.SharpAndRusty.AspNetCore;

/// <summary>
/// Provides extension methods for converting functional types to ASP.NET Core action results.
/// </summary>
public static class ActionResultExtensions
{
    #region Result to ActionResult

    /// <summary>
    /// Converts a Result to an IActionResult, mapping Ok to 200 OK and Err to a problem details response.
    /// </summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E">The error type.</typeparam>
    /// <param name="result">The result to convert.</param>
    /// <param name="statusCode">The status code to use for errors (default: 400 Bad Request).</param>
    /// <returns>An IActionResult representing the result.</returns>
    public static IActionResult ToActionResult<T, E>(
        this Result<T, E> result,
        int statusCode = StatusCodes.Status400BadRequest)
    {
        return result.Match<IActionResult>(
            success: value => new OkObjectResult(value),
            failure: error => new ObjectResult(error) { StatusCode = statusCode }
        );
    }

    /// <summary>
    /// Converts a Result with Error type to an IActionResult with automatic status code mapping.
    /// </summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <param name="result">The result to convert.</param>
    /// <returns>An IActionResult with appropriate status code based on ErrorKind.</returns>
    public static IActionResult ToActionResult<T>(this Result<T, Error> result)
    {
        return result.Match<IActionResult>(
            success: value => new OkObjectResult(value),
            failure: error => error.ToProblemDetails()
        );
    }

    /// <summary>
    /// Converts a Result to a CreatedResult (201) on success, or problem details on error.
    /// </summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E">The error type.</typeparam>
    /// <param name="result">The result to convert.</param>
    /// <param name="location">The URI of the created resource.</param>
    /// <param name="errorStatusCode">The status code to use for errors (default: 400).</param>
    /// <returns>An IActionResult representing the result.</returns>
    public static IActionResult ToCreatedResult<T, E>(
        this Result<T, E> result,
        string location,
        int errorStatusCode = StatusCodes.Status400BadRequest)
    {
        return result.Match<IActionResult>(
            success: value => new CreatedResult(location, value),
            failure: error => new ObjectResult(error) { StatusCode = errorStatusCode }
        );
    }

    /// <summary>
    /// Converts a Result to a CreatedResult with automatic location generation using a function.
    /// </summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <param name="result">The result to convert.</param>
    /// <param name="locationSelector">Function to generate the location URI from the success value.</param>
    /// <returns>An IActionResult representing the result.</returns>
    public static IActionResult ToCreatedResult<T>(
        this Result<T, Error> result,
        Func<T, string> locationSelector)
    {
        return result.Match<IActionResult>(
            success: value => new CreatedResult(locationSelector(value), value),
            failure: error => error.ToProblemDetails()
        );
    }

    /// <summary>
    /// Converts a Result to a NoContentResult (204) on success, or problem details on error.
    /// </summary>
    /// <typeparam name="E">The error type.</typeparam>
    /// <param name="result">The result to convert.</param>
    /// <param name="errorStatusCode">The status code to use for errors (default: 400).</param>
    /// <returns>An IActionResult representing the result.</returns>
    public static IActionResult ToNoContentResult<E>(
        this Result<Unit, E> result,
        int errorStatusCode = StatusCodes.Status400BadRequest)
    {
        return result.Match<IActionResult>(
            success: _ => new NoContentResult(),
            failure: error => new ObjectResult(error) { StatusCode = errorStatusCode }
        );
    }

    /// <summary>
    /// Converts a Result with Unit success type to a NoContentResult with automatic error handling.
    /// </summary>
    /// <param name="result">The result to convert.</param>
    /// <returns>An IActionResult representing the result.</returns>
    public static IActionResult ToNoContentResult(this Result<Unit, Error> result)
    {
        return result.Match<IActionResult>(
            success: _ => new NoContentResult(),
            failure: error => error.ToProblemDetails()
        );
    }

    /// <summary>
    /// Converts a Result to an AcceptedResult (202) on success, or problem details on error.
    /// </summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <param name="result">The result to convert.</param>
    /// <param name="location">The URI where the status of the operation can be monitored.</param>
    /// <returns>An IActionResult representing the result.</returns>
    public static IActionResult ToAcceptedResult<T>(
        this Result<T, Error> result,
        string? location = null)
    {
        return result.Match<IActionResult>(
            success: value => location != null
                ? new AcceptedResult(location, value)
                : new AcceptedResult(string.Empty, value),
            failure: error => error.ToProblemDetails()
        );
    }

    #endregion

    #region Option to ActionResult

    /// <summary>
    /// Converts an Option to an IActionResult, mapping Some to 200 OK and None to 404 Not Found.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="option">The option to convert.</param>
    /// <returns>An IActionResult representing the option.</returns>
    public static IActionResult ToActionResult<T>(this Option<T> option)
    {
        return option switch
        {
            Option<T>.Some some => new OkObjectResult(some.Value),
            Option<T>.None => new NotFoundResult(),
            _ => throw new InvalidOperationException("Option is in an invalid state.")
        };
    }

    /// <summary>
    /// Converts an Option to an IActionResult with a custom 404 message.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="option">The option to convert.</param>
    /// <param name="notFoundMessage">The message to return when None.</param>
    /// <returns>An IActionResult representing the option.</returns>
    public static IActionResult ToActionResult<T>(
        this Option<T> option,
        string notFoundMessage)
    {
        return option switch
        {
            Option<T>.Some some => new OkObjectResult(some.Value),
            Option<T>.None => new NotFoundObjectResult(new { message = notFoundMessage }),
            _ => throw new InvalidOperationException("Option is in an invalid state.")
        };
    }

    /// <summary>
    /// Converts an Option to a custom IActionResult using provided functions.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="option">The option to convert.</param>
    /// <param name="someResult">Function to create result for Some case.</param>
    /// <param name="noneResult">Function to create result for None case.</param>
    /// <returns>An IActionResult representing the option.</returns>
    public static IActionResult ToActionResult<T>(
        this Option<T> option,
        Func<T, IActionResult> someResult,
        Func<IActionResult> noneResult)
    {
        return option switch
        {
            Option<T>.Some some => someResult(some.Value),
            Option<T>.None => noneResult(),
            _ => throw new InvalidOperationException("Option is in an invalid state.")
        };
    }

    #endregion

    #region Either to ActionResult

    /// <summary>
    /// Converts an Either to an IActionResult, mapping Left to 200 OK and Right to error response.
    /// </summary>
    /// <typeparam name="L">The left (success) type.</typeparam>
    /// <typeparam name="R">The right (error) type.</typeparam>
    /// <param name="either">The either to convert.</param>
    /// <param name="rightStatusCode">The status code for Right values (default: 400).</param>
    /// <returns>An IActionResult representing the either.</returns>
    public static IActionResult ToActionResult<L, R>(
        this Either<L, R> either,
        int rightStatusCode = StatusCodes.Status400BadRequest)
    {
        return either.Match<IActionResult>(
            onLeft: value => new OkObjectResult(value),
            onRight: error => new ObjectResult(error) { StatusCode = rightStatusCode }
        );
    }

    /// <summary>
    /// Converts an Either with Error as Right type to an IActionResult with automatic status code mapping.
    /// </summary>
    /// <typeparam name="L">The left (success) type.</typeparam>
    /// <param name="either">The either to convert.</param>
    /// <returns>An IActionResult with appropriate status code based on ErrorKind.</returns>
    public static IActionResult ToActionResult<L>(this Either<L, Error> either)
    {
        return either.Match<IActionResult>(
            onLeft: value => new OkObjectResult(value),
            onRight: error => error.ToProblemDetails()
        );
    }

    #endregion

    #region Validation to ActionResult

    /// <summary>
    /// Converts a Validation to an IActionResult, mapping Valid to 200 OK and Invalid to 400 Bad Request with all errors.
    /// </summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E">The error type.</typeparam>
    /// <param name="validation">The validation to convert.</param>
    /// <returns>An IActionResult representing the validation.</returns>
    public static IActionResult ToActionResult<T, E>(this Validation<T, E> validation)
    {
        return validation.Match<IActionResult>(
            onSuccess: value => new OkObjectResult(value),
            onFailure: errors => new BadRequestObjectResult(new { errors = errors })
        );
    }

    /// <summary>
    /// Converts a Validation with Error type to an IActionResult with RFC 7807 ProblemDetails format.
    /// </summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <param name="validation">The validation to convert.</param>
    /// <returns>An IActionResult with ProblemDetails format for validation errors.</returns>
    public static IActionResult ToActionResult<T>(this Validation<T, Error> validation)
    {
        return validation.Match<IActionResult>(
            onSuccess: value => new OkObjectResult(value),
            onFailure: errors =>
            {
                var problemDetails = new ValidationProblemDetails
                {
                    Title = "One or more validation errors occurred.",
                    Status = StatusCodes.Status400BadRequest
                };

                // Add errors to the dictionary
                foreach (var (error, index) in errors.Select((e, i) => (e, i)))
                {
                    problemDetails.Errors.Add($"field{index}", new[] { error.Message });
                }

                return new BadRequestObjectResult(problemDetails);
            }
        );
    }

    /// <summary>
    /// Converts a Validation to a ModelStateDictionary for use with existing validation infrastructure.
    /// </summary>
    /// <typeparam name="E">The error type.</typeparam>
    /// <param name="validation">The validation to convert.</param>
    /// <param name="keySelector">Function to extract the field key from each error.</param>
    /// <param name="messageSelector">Function to extract the error message from each error.</param>
    /// <returns>A BadRequestObjectResult with ValidationProblemDetails if invalid, or null if valid.</returns>
    public static IActionResult? ToValidationResult<E>(
        this Validation<object, E> validation,
        Func<E, string> keySelector,
        Func<E, string> messageSelector)
    {
        if (validation.IsFailure && validation.TryGetErrors(out var errors))
        {
            var problemDetails = new ValidationProblemDetails
            {
                Title = "One or more validation errors occurred.",
                Status = StatusCodes.Status400BadRequest
            };

            // Group errors by key and add to dictionary
            var errorGroups = errors.GroupBy(keySelector);
            foreach (var group in errorGroups)
            {
                problemDetails.Errors.Add(group.Key, group.Select(messageSelector).ToArray());
            }

            return new BadRequestObjectResult(problemDetails);
        }

        return null;
    }

    #endregion
}
