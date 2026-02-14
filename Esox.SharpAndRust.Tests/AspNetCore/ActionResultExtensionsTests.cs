using Esox.SharpAndRusty.AspNetCore;
using Esox.SharpAndRusty.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Esox.SharpAndRust.Tests.AspNetCore
{
    public class ActionResultExtensionsTests
    {
        #region Result<T, E> to ActionResult Tests

        [Fact]
        public void ToActionResult_WithSuccessResult_ReturnsOkResult()
        {
            // Arrange
            var result = Result<int, string>.Ok(42);

            // Act
            var actionResult = result.ToActionResult();

            // Assert
            Assert.IsType<OkObjectResult>(actionResult);
            var okResult = (OkObjectResult)actionResult;
            Assert.Equal(42, okResult.Value);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public void ToActionResult_WithErrorResult_ReturnsBadRequestResult()
        {
            // Arrange
            var result = Result<int, string>.Err("Something went wrong");

            // Act
            var actionResult = result.ToActionResult();

            // Assert
            Assert.IsType<ObjectResult>(actionResult);
            var objectResult = (ObjectResult)actionResult;
            Assert.Equal("Something went wrong", objectResult.Value);
            Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
        }

        [Fact]
        public void ToActionResult_WithErrorResultAndCustomStatusCode_ReturnsCustomStatusCode()
        {
            // Arrange
            var result = Result<int, string>.Err("Not found");

            // Act
            var actionResult = result.ToActionResult(StatusCodes.Status404NotFound);

            // Assert
            Assert.IsType<ObjectResult>(actionResult);
            var objectResult = (ObjectResult)actionResult;
            Assert.Equal("Not found", objectResult.Value);
            Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
        }

        [Fact]
        public void ToActionResult_WithSuccessResultAndError_ReturnsOkWithErrorMapping()
        {
            // Arrange
            var result = Result<string, Error>.Ok("Success");

            // Act
            var actionResult = result.ToActionResult();

            // Assert
            Assert.IsType<OkObjectResult>(actionResult);
            var okResult = (OkObjectResult)actionResult;
            Assert.Equal("Success", okResult.Value);
        }

        [Fact]
        public void ToActionResult_WithErrorResultAndErrorType_ReturnsProblemDetails()
        {
            // Arrange
            var error = Error.New("Validation failed", ErrorKind.InvalidInput);
            var result = Result<string, Error>.Err(error);

            // Act
            var actionResult = result.ToActionResult();

            // Assert
            Assert.IsType<ObjectResult>(actionResult);
            var objectResult = (ObjectResult)actionResult;
            Assert.IsType<ProblemDetails>(objectResult.Value);
            var problemDetails = (ProblemDetails)objectResult.Value;
            Assert.Contains("Validation failed", problemDetails.Detail);
            Assert.Equal(StatusCodes.Status400BadRequest, problemDetails.Status);
        }

        #endregion

        #region ToCreatedResult Tests

        [Fact]
        public void ToCreatedResult_WithSuccessResult_ReturnsCreatedResult()
        {
            // Arrange
            var result = Result<int, string>.Ok(42);
            var location = "/api/items/42";

            // Act
            var actionResult = result.ToCreatedResult(location);

            // Assert
            Assert.IsType<CreatedResult>(actionResult);
            var createdResult = (CreatedResult)actionResult;
            Assert.Equal(42, createdResult.Value);
            Assert.Equal(location, createdResult.Location);
        }

        [Fact]
        public void ToCreatedResult_WithErrorResult_ReturnsErrorResult()
        {
            // Arrange
            var result = Result<int, string>.Err("Creation failed");
            var location = "/api/items/42";

            // Act
            var actionResult = result.ToCreatedResult(location);

            // Assert
            Assert.IsType<ObjectResult>(actionResult);
            var objectResult = (ObjectResult)actionResult;
            Assert.Equal("Creation failed", objectResult.Value);
            Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
        }

        [Fact]
        public void ToCreatedResult_WithLocationSelector_GeneratesLocationFromValue()
        {
            // Arrange
            var user = new TestUser { Id = 123, Name = "John" };
            var result = Result<TestUser, Error>.Ok(user);

            // Act
            var actionResult = result.ToCreatedResult(u => $"/api/users/{u.Id}");

            // Assert
            Assert.IsType<CreatedResult>(actionResult);
            var createdResult = (CreatedResult)actionResult;
            Assert.Equal("/api/users/123", createdResult.Location);
            Assert.Equal(user, createdResult.Value);
        }

        [Fact]
        public void ToCreatedResult_WithLocationSelectorAndError_ReturnsProblemDetails()
        {
            // Arrange
            var error = Error.New("User creation failed", ErrorKind.InvalidOperation);
            var result = Result<TestUser, Error>.Err(error);

            // Act
            var actionResult = result.ToCreatedResult(u => $"/api/users/{u.Id}");

            // Assert
            Assert.IsType<ObjectResult>(actionResult);
            var objectResult = (ObjectResult)actionResult;
            Assert.IsType<ProblemDetails>(objectResult.Value);
        }

        #endregion

        #region ToNoContentResult Tests

        [Fact]
        public void ToNoContentResult_WithSuccessResult_ReturnsNoContentResult()
        {
            // Arrange
            var result = Result<Unit, string>.Ok(Unit.Value);

            // Act
            var actionResult = result.ToNoContentResult();

            // Assert
            Assert.IsType<NoContentResult>(actionResult);
            var noContentResult = (NoContentResult)actionResult;
            Assert.Equal(StatusCodes.Status204NoContent, noContentResult.StatusCode);
        }

        [Fact]
        public void ToNoContentResult_WithErrorResult_ReturnsErrorResult()
        {
            // Arrange
            var result = Result<Unit, string>.Err("Delete failed");

            // Act
            var actionResult = result.ToNoContentResult();

            // Assert
            Assert.IsType<ObjectResult>(actionResult);
            var objectResult = (ObjectResult)actionResult;
            Assert.Equal("Delete failed", objectResult.Value);
            Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
        }

        [Fact]
        public void ToNoContentResult_WithErrorType_ReturnsNoContentOnSuccess()
        {
            // Arrange
            var result = Result<Unit, Error>.Ok(Unit.Value);

            // Act
            var actionResult = result.ToNoContentResult();

            // Assert
            Assert.IsType<NoContentResult>(actionResult);
        }

        [Fact]
        public void ToNoContentResult_WithErrorTypeAndError_ReturnsProblemDetails()
        {
            // Arrange
            var error = Error.New("Operation failed", ErrorKind.InvalidOperation);
            var result = Result<Unit, Error>.Err(error);

            // Act
            var actionResult = result.ToNoContentResult();

            // Assert
            Assert.IsType<ObjectResult>(actionResult);
            var objectResult = (ObjectResult)actionResult;
            Assert.IsType<ProblemDetails>(objectResult.Value);
        }

        #endregion

        #region ToAcceptedResult Tests

        [Fact]
        public void ToAcceptedResult_WithSuccessResult_ReturnsAcceptedResult()
        {
            // Arrange
            var result = Result<int, Error>.Ok(42);
            var location = "/api/jobs/123";

            // Act
            var actionResult = result.ToAcceptedResult(location);

            // Assert
            Assert.IsType<AcceptedResult>(actionResult);
            var acceptedResult = (AcceptedResult)actionResult;
            Assert.Equal(42, acceptedResult.Value);
            Assert.Equal(location, acceptedResult.Location);
        }

        [Fact]
        public void ToAcceptedResult_WithSuccessResultAndNullLocation_ReturnsAcceptedWithEmptyLocation()
        {
            // Arrange
            var result = Result<int, Error>.Ok(42);

            // Act
            var actionResult = result.ToAcceptedResult();

            // Assert
            Assert.IsType<AcceptedResult>(actionResult);
            var acceptedResult = (AcceptedResult)actionResult;
            Assert.Equal(42, acceptedResult.Value);
            Assert.Equal(string.Empty, acceptedResult.Location);
        }

        [Fact]
        public void ToAcceptedResult_WithErrorResult_ReturnsProblemDetails()
        {
            // Arrange
            var error = Error.New("Job submission failed", ErrorKind.InvalidOperation);
            var result = Result<int, Error>.Err(error);

            // Act
            var actionResult = result.ToAcceptedResult("/api/jobs/123");

            // Assert
            Assert.IsType<ObjectResult>(actionResult);
            var objectResult = (ObjectResult)actionResult;
            Assert.IsType<ProblemDetails>(objectResult.Value);
        }

        #endregion

        #region Option<T> to ActionResult Tests

        [Fact]
        public void OptionToActionResult_WithSome_ReturnsOkResult()
        {
            // Arrange
            var option = new Option<int>.Some(42);

            // Act
            var actionResult = option.ToActionResult();

            // Assert
            Assert.IsType<OkObjectResult>(actionResult);
            var okResult = (OkObjectResult)actionResult;
            Assert.Equal(42, okResult.Value);
        }

        [Fact]
        public void OptionToActionResult_WithNone_ReturnsNotFoundResult()
        {
            // Arrange
            var option = new Option<int>.None();

            // Act
            var actionResult = option.ToActionResult();

            // Assert
            Assert.IsType<NotFoundResult>(actionResult);
        }

        [Fact]
        public void OptionToActionResult_WithNoneAndMessage_ReturnsNotFoundWithMessage()
        {
            // Arrange
            var option = new Option<int>.None();
            var message = "Item not found";

            // Act
            var actionResult = option.ToActionResult(message);

            // Assert
            Assert.IsType<NotFoundObjectResult>(actionResult);
            var notFoundResult = (NotFoundObjectResult)actionResult;
            
            // Verify the anonymous object has the message property
            var value = notFoundResult.Value;
            var messageProperty = value?.GetType().GetProperty("message");
            Assert.NotNull(messageProperty);
            Assert.Equal(message, messageProperty.GetValue(value));
        }

        [Fact]
        public void OptionToActionResult_WithCustomFunctions_UsesSomeFunction()
        {
            // Arrange
            var option = new Option<int>.Some(42);

            // Act
            var actionResult = option.ToActionResult(
                someResult: value => new CreatedResult("/api/items/42", value),
                noneResult: () => new NotFoundResult()
            );

            // Assert
            Assert.IsType<CreatedResult>(actionResult);
            var createdResult = (CreatedResult)actionResult;
            Assert.Equal(42, createdResult.Value);
        }

        [Fact]
        public void OptionToActionResult_WithCustomFunctions_UsesNoneFunction()
        {
            // Arrange
            var option = new Option<int>.None();

            // Act
            var actionResult = option.ToActionResult(
                someResult: value => new OkObjectResult(value),
                noneResult: () => new BadRequestObjectResult("Custom error")
            );

            // Assert
            Assert.IsType<BadRequestObjectResult>(actionResult);
            var badRequestResult = (BadRequestObjectResult)actionResult;
            Assert.Equal("Custom error", badRequestResult.Value);
        }

        #endregion

        #region Either<L, R> to ActionResult Tests

        [Fact]
        public void EitherToActionResult_WithLeft_ReturnsOkResult()
        {
            // Arrange
            var either = new Either<int, string>.Left(42);

            // Act
            var actionResult = either.ToActionResult();

            // Assert
            Assert.IsType<OkObjectResult>(actionResult);
            var okResult = (OkObjectResult)actionResult;
            Assert.Equal(42, okResult.Value);
        }

        [Fact]
        public void EitherToActionResult_WithRight_ReturnsErrorResult()
        {
            // Arrange
            var either = new Either<int, string>.Right("Error occurred");

            // Act
            var actionResult = either.ToActionResult();

            // Assert
            Assert.IsType<ObjectResult>(actionResult);
            var objectResult = (ObjectResult)actionResult;
            Assert.Equal("Error occurred", objectResult.Value);
            Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
        }

        [Fact]
        public void EitherToActionResult_WithRightAndCustomStatusCode_ReturnsCustomStatusCode()
        {
            // Arrange
            var either = new Either<int, string>.Right("Forbidden");

            // Act
            var actionResult = either.ToActionResult(StatusCodes.Status403Forbidden);

            // Assert
            Assert.IsType<ObjectResult>(actionResult);
            var objectResult = (ObjectResult)actionResult;
            Assert.Equal("Forbidden", objectResult.Value);
            Assert.Equal(StatusCodes.Status403Forbidden, objectResult.StatusCode);
        }

        [Fact]
        public void EitherToActionResult_WithLeftAndError_ReturnsOkResult()
        {
            // Arrange
            var either = new Either<int, Error>.Left(42);

            // Act
            var actionResult = either.ToActionResult();

            // Assert
            Assert.IsType<OkObjectResult>(actionResult);
            var okResult = (OkObjectResult)actionResult;
            Assert.Equal(42, okResult.Value);
        }

        [Fact]
        public void EitherToActionResult_WithRightAndError_ReturnsProblemDetails()
        {
            // Arrange
            var error = Error.New("Authorization failed", ErrorKind.PermissionDenied);
            var either = new Either<int, Error>.Right(error);

            // Act
            var actionResult = either.ToActionResult();

            // Assert
            Assert.IsType<ObjectResult>(actionResult);
            var objectResult = (ObjectResult)actionResult;
            Assert.IsType<ProblemDetails>(objectResult.Value);
            var problemDetails = (ProblemDetails)objectResult.Value;
            Assert.Equal(StatusCodes.Status403Forbidden, problemDetails.Status);
        }

        #endregion

        #region Validation<T, E> to ActionResult Tests

        [Fact]
        public void ValidationToActionResult_WithValid_ReturnsOkResult()
        {
            // Arrange
            var validation = Validation<int, string>.Valid(42);

            // Act
            var actionResult = validation.ToActionResult();

            // Assert
            Assert.IsType<OkObjectResult>(actionResult);
            var okResult = (OkObjectResult)actionResult;
            Assert.Equal(42, okResult.Value);
        }

        [Fact]
        public void ValidationToActionResult_WithInvalid_ReturnsBadRequestWithErrors()
        {
            // Arrange
            var validation = Validation<int, string>.Invalid(new[] { "Error 1", "Error 2" });

            // Act
            var actionResult = validation.ToActionResult();

            // Assert
            Assert.IsType<BadRequestObjectResult>(actionResult);
            var badRequestResult = (BadRequestObjectResult)actionResult;
            var value = badRequestResult.Value;
            
            // Check if the value has an errors property
            var errorsProperty = value?.GetType().GetProperty("errors");
            Assert.NotNull(errorsProperty);
            var errors = errorsProperty.GetValue(value) as IEnumerable<string>;
            Assert.NotNull(errors);
            Assert.Equal(2, errors.Count());
            Assert.Contains("Error 1", errors);
            Assert.Contains("Error 2", errors);
        }

        [Fact]
        public void ValidationToActionResult_WithValidAndError_ReturnsOkResult()
        {
            // Arrange
            var validation = Validation<int, Error>.Valid(42);

            // Act
            var actionResult = validation.ToActionResult();

            // Assert
            Assert.IsType<OkObjectResult>(actionResult);
            var okResult = (OkObjectResult)actionResult;
            Assert.Equal(42, okResult.Value);
        }

        [Fact]
        public void ValidationToActionResult_WithInvalidAndError_ReturnsValidationProblemDetails()
        {
            // Arrange
            var error1 = Error.New("Field1 is required", ErrorKind.InvalidInput);
            var error2 = Error.New("Field2 must be positive", ErrorKind.InvalidInput);
            var validation = Validation<int, Error>.Invalid(new[] { error1, error2 });

            // Act
            var actionResult = validation.ToActionResult();

            // Assert
            Assert.IsType<BadRequestObjectResult>(actionResult);
            var badRequestResult = (BadRequestObjectResult)actionResult;
            Assert.IsType<ValidationProblemDetails>(badRequestResult.Value);
            var problemDetails = (ValidationProblemDetails)badRequestResult.Value;
            Assert.Equal("One or more validation errors occurred.", problemDetails.Title);
            Assert.Equal(StatusCodes.Status400BadRequest, problemDetails.Status);
            Assert.NotEmpty(problemDetails.Errors);
            Assert.Equal(2, problemDetails.Errors.Count);
        }

        [Fact]
        public void ToValidationResult_WithValid_ReturnsNull()
        {
            // Arrange
            var validation = Validation<object, string>.Valid(new object());

            // Act
            var actionResult = validation.ToValidationResult(
                keySelector: error => "field",
                messageSelector: error => error
            );

            // Assert
            Assert.Null(actionResult);
        }

        [Fact]
        public void ToValidationResult_WithInvalid_ReturnsValidationProblemDetails()
        {
            // Arrange
            var errors = new[]
            {
                new ValidationError { Field = "email", Message = "Email is required" },
                new ValidationError { Field = "email", Message = "Email must be valid" },
                new ValidationError { Field = "password", Message = "Password is too short" }
            };
            var validation = Validation<object, ValidationError>.Invalid(errors);

            // Act
            var actionResult = validation.ToValidationResult(
                keySelector: e => e.Field,
                messageSelector: e => e.Message
            );

            // Assert
            Assert.NotNull(actionResult);
            Assert.IsType<BadRequestObjectResult>(actionResult);
            var badRequestResult = (BadRequestObjectResult)actionResult;
            Assert.IsType<ValidationProblemDetails>(badRequestResult.Value);
            
            var problemDetails = (ValidationProblemDetails)badRequestResult.Value;
            Assert.Equal("One or more validation errors occurred.", problemDetails.Title);
            Assert.Equal(StatusCodes.Status400BadRequest, problemDetails.Status);
            
            // Check email field has 2 errors
            Assert.True(problemDetails.Errors.ContainsKey("email"));
            Assert.Equal(2, problemDetails.Errors["email"].Length);
            
            // Check password field has 1 error
            Assert.True(problemDetails.Errors.ContainsKey("password"));
            Assert.Single(problemDetails.Errors["password"]);
        }

        #endregion

        #region Edge Cases and Complex Scenarios

        [Fact]
        public void ToActionResult_WithComplexSuccessValue_SerializesCorrectly()
        {
            // Arrange
            var complexValue = new
            {
                Id = 1,
                Name = "Test",
                Tags = new[] { "tag1", "tag2" },
                Nested = new { Value = 42 }
            };
            var result = Result<dynamic, string>.Ok(complexValue);

            // Act
            var actionResult = result.ToActionResult();

            // Assert
            Assert.IsType<OkObjectResult>(actionResult);
            var okResult = (OkObjectResult)actionResult;
            Assert.Equal(complexValue, okResult.Value);
        }

        [Fact]
        public void ToCreatedResult_WithNullValue_HandlesGracefully()
        {
            // Arrange
            string? nullValue = null;
            var result = Result<string?, Error>.Ok(nullValue);

            // Act
            var actionResult = result.ToCreatedResult("/api/items/null");

            // Assert
            Assert.IsType<CreatedResult>(actionResult);
            var createdResult = (CreatedResult)actionResult;
            Assert.Null(createdResult.Value);
        }

        [Fact]
        public void ToActionResult_WithDifferentErrorKinds_MapsToDifferentStatusCodes()
        {
            // Arrange - Test various error kinds
            var notFoundError = Error.New("Not found", ErrorKind.NotFound);
            var permissionError = Error.New("Forbidden", ErrorKind.PermissionDenied);
            var timeoutError = Error.New("Timeout", ErrorKind.Timeout);

            // Act
            var notFoundResult = Result<int, Error>.Err(notFoundError).ToActionResult();
            var permissionResult = Result<int, Error>.Err(permissionError).ToActionResult();
            var timeoutResult = Result<int, Error>.Err(timeoutError).ToActionResult();

            // Assert - All should return ObjectResult with ProblemDetails
            Assert.IsType<ObjectResult>(notFoundResult);
            Assert.IsType<ObjectResult>(permissionResult);
            Assert.IsType<ObjectResult>(timeoutResult);

            var notFoundProblem = ((ObjectResult)notFoundResult).Value as ProblemDetails;
            var permissionProblem = ((ObjectResult)permissionResult).Value as ProblemDetails;
            var timeoutProblem = ((ObjectResult)timeoutResult).Value as ProblemDetails;

            Assert.Equal(StatusCodes.Status404NotFound, notFoundProblem?.Status);
            Assert.Equal(StatusCodes.Status403Forbidden, permissionProblem?.Status);
            Assert.Equal(StatusCodes.Status408RequestTimeout, timeoutProblem?.Status);
        }

        [Fact]
        public void ToNoContentResult_WithUnitValue_AlwaysReturnsNoContent()
        {
            // Arrange
            var result = Result<Unit, Error>.Ok(Unit.Value);

            // Act
            var actionResult1 = result.ToNoContentResult();
            var actionResult2 = result.ToNoContentResult();

            // Assert - Both should be NoContentResult
            Assert.IsType<NoContentResult>(actionResult1);
            Assert.IsType<NoContentResult>(actionResult2);
            Assert.Equal(
                ((NoContentResult)actionResult1).StatusCode,
                ((NoContentResult)actionResult2).StatusCode
            );
        }

        [Fact]
        public void OptionToActionResult_WithSomeComplexType_ReturnsValue()
        {
            // Arrange
            var user = new TestUser { Id = 999, Name = "Jane Doe" };
            var option = new Option<TestUser>.Some(user);

            // Act
            var actionResult = option.ToActionResult();

            // Assert
            Assert.IsType<OkObjectResult>(actionResult);
            var okResult = (OkObjectResult)actionResult;
            var returnedUser = okResult.Value as TestUser;
            Assert.NotNull(returnedUser);
            Assert.Equal(999, returnedUser.Id);
            Assert.Equal("Jane Doe", returnedUser.Name);
        }

        [Fact]
        public void EitherToActionResult_WithComplexLeftType_ReturnsCorrectly()
        {
            // Arrange
            var data = new { Success = true, Data = "test" };
            var either = new Either<dynamic, string>.Left(data);

            // Act
            var actionResult = either.ToActionResult();

            // Assert
            Assert.IsType<OkObjectResult>(actionResult);
            var okResult = (OkObjectResult)actionResult;
            Assert.Equal(data, okResult.Value);
        }

        #endregion

        #region Helper Classes

        private class TestUser
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
        }

        private class ValidationError
        {
            public string Field { get; set; } = string.Empty;
            public string Message { get; set; } = string.Empty;
        }

        #endregion
    }
}
