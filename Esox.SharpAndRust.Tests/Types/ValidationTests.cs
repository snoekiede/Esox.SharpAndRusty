using Esox.SharpAndRusty.Types;
using Esox.SharpAndRusty.Extensions;
using System.Collections.Immutable;

namespace Esox.SharpAndRust.Tests.Types;

public class ValidationTests
{
    #region Construction Tests

    [Fact]
    public void Valid_CreatesSuccessfulValidation()
    {
        // Arrange & Act
        var validation = Validation<int, string>.Valid(42);

        // Assert
        Assert.True(validation.IsSuccess);
        Assert.False(validation.IsFailure);
        Assert.True(validation.TryGetValue(out var value));
        Assert.Equal(42, value);
    }

    [Fact]
    public void Invalid_WithSingleError_CreatesFailedValidation()
    {
        // Arrange & Act
        var validation = Validation<int, string>.Invalid("error");

        // Assert
        Assert.False(validation.IsSuccess);
        Assert.True(validation.IsFailure);
        Assert.True(validation.TryGetErrors(out var errors));
        Assert.Single(errors);
        Assert.Equal("error", errors[0]);
    }

    [Fact]
    public void Invalid_WithMultipleErrors_CreatesFailedValidation()
    {
        // Arrange & Act
        var validation = Validation<int, string>.Invalid(new[] { "error1", "error2", "error3" });

        // Assert
        Assert.True(validation.IsFailure);
        Assert.True(validation.TryGetErrors(out var errors));
        Assert.Equal(3, errors.Count);
        Assert.Equal(new[] { "error1", "error2", "error3" }, errors);
    }

    #endregion

    #region TryGet Tests

    [Fact]
    public void TryGetValue_WithSuccess_ReturnsTrue()
    {
        // Arrange
        var validation = Validation<int, string>.Valid(42);

        // Act
        var success = validation.TryGetValue(out var value);

        // Assert
        Assert.True(success);
        Assert.Equal(42, value);
    }

    [Fact]
    public void TryGetValue_WithFailure_ReturnsFalse()
    {
        // Arrange
        var validation = Validation<int, string>.Invalid("error");

        // Act
        var success = validation.TryGetValue(out var value);

        // Assert
        Assert.False(success);
        Assert.Equal(default, value);
    }

    [Fact]
    public void TryGetErrors_WithFailure_ReturnsTrue()
    {
        // Arrange
        var validation = Validation<int, string>.Invalid(new[] { "error1", "error2" });

        // Act
        var success = validation.TryGetErrors(out var errors);

        // Assert
        Assert.True(success);
        Assert.Equal(2, errors.Count);
    }

    [Fact]
    public void TryGetErrors_WithSuccess_ReturnsFalse()
    {
        // Arrange
        var validation = Validation<int, string>.Valid(42);

        // Act
        var success = validation.TryGetErrors(out var errors);

        // Assert
        Assert.False(success);
        Assert.Empty(errors);
    }

    #endregion

    #region Match Tests

    [Fact]
    public void Match_WithSuccess_ExecutesSuccessFunction()
    {
        // Arrange
        var validation = Validation<int, string>.Valid(42);

        // Act
        var result = validation.Match(
            onSuccess: v => $"Value: {v}",
            onFailure: errors => $"Errors: {string.Join(", ", errors)}");

        // Assert
        Assert.Equal("Value: 42", result);
    }

    [Fact]
    public void Match_WithFailure_ExecutesFailureFunction()
    {
        // Arrange
        var validation = Validation<int, string>.Invalid(new[] { "error1", "error2" });

        // Act
        var result = validation.Match(
            onSuccess: v => $"Value: {v}",
            onFailure: errors => $"Errors: {string.Join(", ", errors)}");

        // Assert
        Assert.Equal("Errors: error1, error2", result);
    }

    #endregion

    #region Map Tests

    [Fact]
    public void Map_WithSuccess_TransformsValue()
    {
        // Arrange
        var validation = Validation<int, string>.Valid(42);

        // Act
        var result = validation.Map(x => x * 2);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.TryGetValue(out var value));
        Assert.Equal(84, value);
    }

    [Fact]
    public void Map_WithFailure_ReturnsErrorsUnchanged()
    {
        // Arrange
        var validation = Validation<int, string>.Invalid(new[] { "error1", "error2" });

        // Act
        var result = validation.Map(x => x * 2);

        // Assert
        Assert.True(result.IsFailure);
        Assert.True(result.TryGetErrors(out var errors));
        Assert.Equal(2, errors.Count);
    }

    [Fact]
    public void MapErrors_WithFailure_TransformsErrors()
    {
        // Arrange
        var validation = Validation<int, string>.Invalid(new[] { "error1", "error2" });

        // Act
        var result = validation.MapErrors(e => e.ToUpper());

        // Assert
        Assert.True(result.IsFailure);
        Assert.True(result.TryGetErrors(out var errors));
        Assert.Equal(new[] { "ERROR1", "ERROR2" }, errors);
    }

    [Fact]
    public void MapErrors_WithSuccess_ReturnsValueUnchanged()
    {
        // Arrange
        var validation = Validation<int, string>.Valid(42);

        // Act
        var result = validation.MapErrors(e => e.ToUpper());

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.TryGetValue(out var value));
        Assert.Equal(42, value);
    }

    #endregion

    #region Result Conversion Tests

    [Fact]
    public void ToResult_WithErrorCombiner_Success()
    {
        // Arrange
        var validation = Validation<int, string>.Valid(42);

        // Act
        var result = validation.ToResult(errors => string.Join("; ", errors));

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.TryGetValue(out var value));
        Assert.Equal(42, value);
    }

    [Fact]
    public void ToResult_WithErrorCombiner_CombinesErrors()
    {
        // Arrange
        var validation = Validation<int, string>.Invalid(new[] { "error1", "error2", "error3" });

        // Act
        var result = validation.ToResult(errors => string.Join("; ", errors));

        // Assert
        Assert.True(result.IsFailure);
        Assert.True(result.TryGetError(out var error));
        Assert.Equal("error1; error2; error3", error);
    }

    [Fact]
    public void ToResultFirstError_WithSuccess_ReturnsOk()
    {
        // Arrange
        var validation = Validation<int, string>.Valid(42);

        // Act
        var result = validation.ToResultFirstError();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.TryGetValue(out var value));
        Assert.Equal(42, value);
    }

    [Fact]
    public void ToResultFirstError_WithFailure_ReturnsFirstError()
    {
        // Arrange
        var validation = Validation<int, string>.Invalid(new[] { "error1", "error2", "error3" });

        // Act
        var result = validation.ToResultFirstError();

        // Assert
        Assert.True(result.IsFailure);
        Assert.True(result.TryGetError(out var error));
        Assert.Equal("error1", error);
    }

    #endregion

    #region Apply Tests (Applicative Functor)

    [Fact]
    public void Apply_TwoValidations_BothSuccess_CombinesValues()
    {
        // Arrange
        var v1 = Validation<string, string>.Valid("John");
        var v2 = Validation<int, string>.Valid(30);

        // Act
        var result = ValidationExtensions.Apply(
            v1, v2,
            (name, age) => $"{name} is {age} years old");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.TryGetValue(out var value));
        Assert.Equal("John is 30 years old", value);
    }

    [Fact]
    public void Apply_TwoValidations_FirstFails_ReturnsFirstError()
    {
        // Arrange
        var v1 = Validation<string, string>.Invalid("Name error");
        var v2 = Validation<int, string>.Valid(30);

        // Act
        var result = ValidationExtensions.Apply(
            v1, v2,
            (name, age) => $"{name} is {age} years old");

        // Assert
        Assert.True(result.IsFailure);
        Assert.True(result.TryGetErrors(out var errors));
        Assert.Single(errors);
        Assert.Equal("Name error", errors[0]);
    }

    [Fact]
    public void Apply_TwoValidations_SecondFails_ReturnsSecondError()
    {
        // Arrange
        var v1 = Validation<string, string>.Valid("John");
        var v2 = Validation<int, string>.Invalid("Age error");

        // Act
        var result = ValidationExtensions.Apply(
            v1, v2,
            (name, age) => $"{name} is {age} years old");

        // Assert
        Assert.True(result.IsFailure);
        Assert.True(result.TryGetErrors(out var errors));
        Assert.Single(errors);
        Assert.Equal("Age error", errors[0]);
    }

    [Fact]
    public void Apply_TwoValidations_BothFail_AccumulatesErrors()
    {
        // Arrange
        var v1 = Validation<string, string>.Invalid("Name error");
        var v2 = Validation<int, string>.Invalid("Age error");

        // Act
        var result = ValidationExtensions.Apply(
            v1, v2,
            (name, age) => $"{name} is {age} years old");

        // Assert
        Assert.True(result.IsFailure);
        Assert.True(result.TryGetErrors(out var errors));
        Assert.Equal(2, errors.Count);
        Assert.Contains("Name error", errors);
        Assert.Contains("Age error", errors);
    }

    [Fact]
    public void Apply_ThreeValidations_AllSuccess_CombinesValues()
    {
        // Arrange
        var v1 = Validation<string, string>.Valid("John");
        var v2 = Validation<string, string>.Valid("john@example.com");
        var v3 = Validation<int, string>.Valid(30);

        // Act
        var result = ValidationExtensions.Apply(
            v1, v2, v3,
            (name, email, age) => new TestUser(name, email, age));

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.TryGetValue(out var user));
        Assert.Equal("John", user.Name);
        Assert.Equal("john@example.com", user.Email);
        Assert.Equal(30, user.Age);
    }

    [Fact]
    public void Apply_ThreeValidations_AllFail_AccumulatesAllErrors()
    {
        // Arrange
        var v1 = Validation<string, string>.Invalid("Name error");
        var v2 = Validation<string, string>.Invalid("Email error");
        var v3 = Validation<int, string>.Invalid("Age error");

        // Act
        var result = ValidationExtensions.Apply(
            v1, v2, v3,
            (name, email, age) => new TestUser(name, email, age));

        // Assert
        Assert.True(result.IsFailure);
        Assert.True(result.TryGetErrors(out var errors));
        Assert.Equal(3, errors.Count);
        Assert.Contains("Name error", errors);
        Assert.Contains("Email error", errors);
        Assert.Contains("Age error", errors);
    }

    [Fact]
    public void Apply_FourValidations_AllSuccess_CombinesValues()
    {
        // Arrange
        var v1 = Validation<string, string>.Valid("John");
        var v2 = Validation<string, string>.Valid("john@example.com");
        var v3 = Validation<int, string>.Valid(30);
        var v4 = Validation<string, string>.Valid("USA");

        // Act
        var result = ValidationExtensions.Apply(
            v1, v2, v3, v4,
            (name, email, age, country) => $"{name}, {email}, {age}, {country}");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.TryGetValue(out var value));
        Assert.Equal("John, john@example.com, 30, USA", value);
    }

    [Fact]
    public void Apply_FourValidations_MixedResults_AccumulatesAllErrors()
    {
        // Arrange
        var v1 = Validation<string, string>.Valid("John");
        var v2 = Validation<string, string>.Invalid("Email error");
        var v3 = Validation<int, string>.Valid(30);
        var v4 = Validation<string, string>.Invalid("Country error");

        // Act
        var result = ValidationExtensions.Apply(
            v1, v2, v3, v4,
            (name, email, age, country) => $"{name}, {email}, {age}, {country}");

        // Assert
        Assert.True(result.IsFailure);
        Assert.True(result.TryGetErrors(out var errors));
        Assert.Equal(2, errors.Count);
        Assert.Contains("Email error", errors);
        Assert.Contains("Country error", errors);
    }

    #endregion

    #region Bind Tests

    [Fact]
    public void Bind_WithSuccess_ChainsValidation()
    {
        // Arrange
        var validation = Validation<int, string>.Valid(42);

        // Act
        var result = validation.Bind(x =>
            x > 0
                ? Validation<string, string>.Valid($"Positive: {x}")
                : Validation<string, string>.Invalid("Not positive"));

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.TryGetValue(out var value));
        Assert.Equal("Positive: 42", value);
    }

    [Fact]
    public void Bind_WithFailure_ReturnsErrorsUnchanged()
    {
        // Arrange
        var validation = Validation<int, string>.Invalid(new[] { "error1", "error2" });

        // Act
        var result = validation.Bind(x =>
            Validation<string, string>.Valid($"Value: {x}"));

        // Assert
        Assert.True(result.IsFailure);
        Assert.True(result.TryGetErrors(out var errors));
        Assert.Equal(2, errors.Count);
    }

    [Fact]
    public void Bind_SuccessThenFailure_ReturnsNewError()
    {
        // Arrange
        var validation = Validation<int, string>.Valid(42);

        // Act
        var result = validation.Bind(x =>
            Validation<string, string>.Invalid("Validation failed"));

        // Assert
        Assert.True(result.IsFailure);
        Assert.True(result.TryGetErrors(out var errors));
        Assert.Single(errors);
        Assert.Equal("Validation failed", errors[0]);
    }

    #endregion

    #region Sequence Tests

    [Fact]
    public void Sequence_AllSuccess_ReturnsAllValues()
    {
        // Arrange
        var validations = new[]
        {
            Validation<int, string>.Valid(1),
            Validation<int, string>.Valid(2),
            Validation<int, string>.Valid(3)
        };

        // Act
        var result = validations.Sequence();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.TryGetValue(out var values));
        Assert.Equal(new[] { 1, 2, 3 }, values);
    }

    [Fact]
    public void Sequence_SomeFail_AccumulatesAllErrors()
    {
        // Arrange
        var validations = new[]
        {
            Validation<int, string>.Valid(1),
            Validation<int, string>.Invalid("error1"),
            Validation<int, string>.Valid(3),
            Validation<int, string>.Invalid("error2")
        };

        // Act
        var result = validations.Sequence();

        // Assert
        Assert.True(result.IsFailure);
        Assert.True(result.TryGetErrors(out var errors));
        Assert.Equal(2, errors.Count);
        Assert.Contains("error1", errors);
        Assert.Contains("error2", errors);
    }

    #endregion

    #region Action Tests

    [Fact]
    public void OnSuccess_WithSuccess_ExecutesAction()
    {
        // Arrange
        var validation = Validation<int, string>.Valid(42);
        var executed = false;
        var capturedValue = 0;

        // Act
        var result = validation.OnSuccess(value =>
        {
            executed = true;
            capturedValue = value;
        });

        // Assert
        Assert.True(executed);
        Assert.Equal(42, capturedValue);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void OnFailure_WithFailure_ExecutesAction()
    {
        // Arrange
        var validation = Validation<int, string>.Invalid(new[] { "error1", "error2" });
        var executed = false;
        var capturedErrors = ImmutableList<string>.Empty;

        // Act
        var result = validation.OnFailure(errors =>
        {
            executed = true;
            capturedErrors = errors;
        });

        // Assert
        Assert.True(executed);
        Assert.Equal(2, capturedErrors.Count);
        Assert.True(result.IsFailure);
    }

    #endregion

    #region Integration Tests - Form Validation

    [Fact]
    public void Integration_FormValidation_AllFieldsValid()
    {
        // Arrange
        var form = new RegistrationForm("John Doe", "john@example.com", 30, "password123");

        // Act
        var validation = ValidateForm(form);

        // Assert
        Assert.True(validation.IsSuccess);
        Assert.True(validation.TryGetValue(out var user));
        Assert.Equal("John Doe", user.Name);
    }

    [Fact]
    public void Integration_FormValidation_AllFieldsInvalid_AccumulatesAllErrors()
    {
        // Arrange
        var form = new RegistrationForm("", "invalid-email", 15, "short");

        // Act
        var validation = ValidateForm(form);

        // Assert
        Assert.True(validation.IsFailure);
        Assert.True(validation.TryGetErrors(out var errors));
        Assert.Equal(4, errors.Count);
        Assert.Contains(errors, e => e.Contains("Name"));
        Assert.Contains(errors, e => e.Contains("Email"));
        Assert.Contains(errors, e => e.Contains("Age"));
        Assert.Contains(errors, e => e.Contains("Password"));
    }

    [Fact]
    public void Integration_FormValidation_SomeFieldsInvalid_ShowsSpecificErrors()
    {
        // Arrange
        var form = new RegistrationForm("John Doe", "invalid-email", 30, "password123");

        // Act
        var validation = ValidateForm(form);

        // Assert
        Assert.True(validation.IsFailure);
        Assert.True(validation.TryGetErrors(out var errors));
        Assert.Single(errors);
        Assert.Contains("Email", errors[0]);
    }

    private Validation<TestUser, string> ValidateForm(RegistrationForm form)
    {
        return ValidationExtensions.Apply(
            ValidateName(form.Name),
            ValidateEmail(form.Email),
            ValidateAge(form.Age),
            ValidatePassword(form.Password),
            (name, email, age, password) => new TestUser(name, email, age));
    }

    private Validation<string, string> ValidateName(string name)
    {
        return !string.IsNullOrWhiteSpace(name) && name.Length >= 3
            ? Validation<string, string>.Valid(name)
            : Validation<string, string>.Invalid("Name must be at least 3 characters");
    }

    private Validation<string, string> ValidateEmail(string email)
    {
        return email.Contains("@") && email.Contains(".")
            ? Validation<string, string>.Valid(email)
            : Validation<string, string>.Invalid("Email must be valid");
    }

    private Validation<int, string> ValidateAge(int age)
    {
        return age >= 18 && age <= 120
            ? Validation<int, string>.Valid(age)
            : Validation<int, string>.Invalid("Age must be between 18 and 120");
    }

    private Validation<string, string> ValidatePassword(string password)
    {
        return password.Length >= 8
            ? Validation<string, string>.Valid(password)
            : Validation<string, string>.Invalid("Password must be at least 8 characters");
    }

    #endregion

    #region Integration Tests - Config Validation

    [Fact]
    public void Integration_ConfigValidation_AllValid()
    {
        // Arrange
        var config = new AppConfig("localhost:5432", "localhost:6379", true, 30);

        // Act
        var validation = ValidateConfig(config);

        // Assert
        Assert.True(validation.IsSuccess);
    }

    [Fact]
    public void Integration_ConfigValidation_MultipleErrors()
    {
        // Arrange
        var config = new AppConfig("", "", false, -1);

        // Act
        var validation = ValidateConfig(config);

        // Assert
        Assert.True(validation.IsFailure);
        Assert.True(validation.TryGetErrors(out var errors));
        Assert.True(errors.Count >= 2); // At least database and timeout errors
    }

    private Validation<AppConfig, string> ValidateConfig(AppConfig config)
    {
        return ValidationExtensions.Apply(
            ValidateDatabase(config.DatabaseUrl),
            ValidateCache(config.CacheUrl),
            ValidateLogging(config.EnableLogging),
            ValidateTimeout(config.TimeoutSeconds),
            (db, cache, logging, timeout) => config);
    }

    private Validation<string, string> ValidateDatabase(string url)
    {
        return !string.IsNullOrWhiteSpace(url)
            ? Validation<string, string>.Valid(url)
            : Validation<string, string>.Invalid("Database URL is required");
    }

    private Validation<string, string> ValidateCache(string url)
    {
        return !string.IsNullOrWhiteSpace(url)
            ? Validation<string, string>.Valid(url)
            : Validation<string, string>.Invalid("Cache URL is required");
    }

    private Validation<bool, string> ValidateLogging(bool enabled)
    {
        return Validation<bool, string>.Valid(enabled); // Always valid
    }

    private Validation<int, string> ValidateTimeout(int seconds)
    {
        return seconds > 0 && seconds <= 300
            ? Validation<int, string>.Valid(seconds)
            : Validation<int, string>.Invalid("Timeout must be between 1 and 300 seconds");
    }

    #endregion

    #region Comparison with Result

    [Fact]
    public void Comparison_Result_StopsAtFirstError()
    {
        // Arrange
        var form = new RegistrationForm("", "invalid-email", 15, "short");

        // Act - Using Result (stops at first error)
        var result = ValidateNameResult(form.Name)
            .Bind(_ => ValidateEmailResult(form.Email))
            .Bind(_ => ValidateAgeResult(form.Age))
            .Bind(_ => ValidatePasswordResult(form.Password));

        // Assert - Only sees first error
        Assert.True(result.IsFailure);
        Assert.True(result.TryGetError(out var error));
        Assert.Contains("Name", error);
    }

    private Result<string, string> ValidateNameResult(string name)
    {
        return !string.IsNullOrWhiteSpace(name) && name.Length >= 3
            ? Result<string, string>.Ok(name)
            : Result<string, string>.Err("Name must be at least 3 characters");
    }

    private Result<string, string> ValidateEmailResult(string email)
    {
        return email.Contains("@")
            ? Result<string, string>.Ok(email)
            : Result<string, string>.Err("Email must be valid");
    }

    private Result<int, string> ValidateAgeResult(int age)
    {
        return age >= 18
            ? Result<int, string>.Ok(age)
            : Result<int, string>.Err("Age must be at least 18");
    }

    private Result<string, string> ValidatePasswordResult(string password)
    {
        return password.Length >= 8
            ? Result<string, string>.Ok(password)
            : Result<string, string>.Err("Password must be at least 8 characters");
    }

    #endregion

    #region Helper Types

    private record TestUser(string Name, string Email, int Age);
    private record RegistrationForm(string Name, string Email, int Age, string Password);
    private record AppConfig(string DatabaseUrl, string CacheUrl, bool EnableLogging, int TimeoutSeconds);

    #endregion
}
