using Esox.SharpAndRusty.Types;
using Esox.SharpAndRusty.Extensions;

namespace Esox.SharpAndRust.Tests.Types;

public class ExtendedResultTests
{
    [Fact]
    public void Ok_TryGetValue_ReturnsTrueAndValue()
    {
        var result = ExtendedResult<int, string>.Ok(42);

        var success = result.TryGetValue(out var value);

        Assert.True(success);
        Assert.Equal(42, value);
        Assert.False(result.TryGetError(out _));
    }

    [Fact]
    public void Err_TryGetError_ReturnsTrueAndError()
    {
        var result = ExtendedResult<int, string>.Err("boom");

        var failure = result.TryGetError(out var error);

        Assert.True(failure);
        Assert.Equal("boom", error);
        Assert.False(result.TryGetValue(out _));
    }

    [Fact]
    public void IsSuccess_ReturnsTrueForOk()
    {
        var result = ExtendedResult<int, string>.Ok(42);

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
    }

    [Fact]
    public void IsSuccess_ReturnsFalseForErr()
    {
        var result = ExtendedResult<int, string>.Err("error");

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void IsFailure_ReturnsTrueForErr()
    {
        var result = ExtendedResult<int, string>.Err("failure");

        Assert.True(result.IsFailure);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void IsFailure_ReturnsFalseForOk()
    {
        var result = ExtendedResult<int, string>.Ok(100);

        Assert.False(result.IsFailure);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void UnwrapOr_ReturnsDefaultOnFailure()
    {
        var result = ExtendedResult<int, string>.Err("e");
        var value = result.UnwrapOr(99);
        Assert.Equal(99, value);
    }

    [Fact]
    public void UnwrapOrElse_ComputesDefaultFromError()
    {
        var result = ExtendedResult<int, string>.Err("error");
        var value = result.UnwrapOrElse(e => e.Length);
        Assert.Equal(5, value);
    }

    [Fact]
    public void Inspect_RunsOnSuccess_AndReturnsSameSemanticInstance()
    {
        var result = ExtendedResult<int, string>.Ok(5);
        var observed = 0;

        var returned = result.Inspect(v => observed = v);

        Assert.Equal(5, observed);
        // Returned result is the same semantic state (still success with same value)
        Assert.True(returned.TryGetValue(out var value));
        Assert.Equal(5, value);
    }

    [Fact]
    public void Inspect_DoesNotRunOnFailure()
    {
        var result = ExtendedResult<int, string>.Err("err");
        var ran = false;

        var returned = result.Inspect(_ => ran = true);

        Assert.False(ran);
        Assert.True(returned.TryGetError(out _));
    }

    [Fact]
    public void InspectErr_RunsOnFailure()
    {
        var result = ExtendedResult<int, string>.Err("oops");
        string? seen = null;

        var returned = result.InspectErr(e => seen = e);

        Assert.Equal("oops", seen);
        Assert.True(returned.TryGetError(out _));
    }

    [Fact]
    public async Task TryAsync_Success()
    {
        async Task<int> Op()
        {
            await Task.Delay(1);
            return 7;
        }

        var result = await ExtendedResult<int, string>.TryAsync(Op, ex => ex.Message);

        Assert.True(result.TryGetValue(out var value));
        Assert.Equal(7, value);
    }

    [Fact]
    public async Task TryAsync_Failure()
    {
        async Task<int> Op()
        {
            await Task.Delay(1);
            throw new InvalidOperationException("nope");
        }

        var result = await ExtendedResult<int, string>.TryAsync(Op, ex => ex.Message);

        Assert.True(result.TryGetError(out var error));
        Assert.Contains("nope", error);
    }

    #region Pattern Matching Tests

    [Fact]
    public void Match_Success_ExecutesSuccessFunction()
    {
        var result = ExtendedResult<int, string>.Ok(42);

        var output = result.Match(
            success: value => $"Value: {value}",
            failure: error => $"Error: {error}"
        );

        Assert.Equal("Value: 42", output);
    }

    [Fact]
    public void Match_Failure_ExecutesFailureFunction()
    {
        var result = ExtendedResult<int, string>.Err("Something went wrong");

        var output = result.Match(
            success: value => $"Value: {value}",
            failure: error => $"Error: {error}"
        );

        Assert.Equal("Error: Something went wrong", output);
    }

    [Fact]
    public void Match_WithComplexTypes_Success()
    {
        var result = ExtendedResult<Person, ValidationError>.Ok(new Person("Alice", 30));

        var output = result.Match(
            success: person => $"{person.Name} is {person.Age}",
            failure: error => $"Validation failed: {error.Message}"
        );

        Assert.Equal("Alice is 30", output);
    }

    [Fact]
    public void Match_WithComplexTypes_Failure()
    {
        var result = ExtendedResult<Person, ValidationError>.Err(new ValidationError("Invalid age"));

        var output = result.Match(
            success: person => $"{person.Name} is {person.Age}",
            failure: error => $"Validation failed: {error.Message}"
        );

        Assert.Equal("Validation failed: Invalid age", output);
    }

    [Fact]
    public void Match_ThrowsArgumentNullException_WhenSuccessFunctionIsNull()
    {
        var result = ExtendedResult<int, string>.Ok(42);

        var exception = Assert.Throws<ArgumentNullException>(() =>
            result.Match(success: null!, failure: error => error)
        );

        Assert.Equal("success", exception.ParamName);
    }

    [Fact]
    public void Match_ThrowsArgumentNullException_WhenFailureFunctionIsNull()
    {
        var result = ExtendedResult<int, string>.Ok(42);

        var exception = Assert.Throws<ArgumentNullException>(() =>
            result.Match(success: value => value.ToString(), failure: null!)
        );

        Assert.Equal("failure", exception.ParamName);
    }

    [Fact]
    public void Match_CanReturnDifferentType()
    {
        var successResult = ExtendedResult<int, string>.Ok(42);
        var failureResult = ExtendedResult<int, string>.Err("error");

        var successOutput = successResult.Match(
            success: value => value * 2,
            failure: _ => 0
        );

        var failureOutput = failureResult.Match(
            success: value => value * 2,
            failure: _ => 0
        );

        Assert.Equal(84, successOutput);
        Assert.Equal(0, failureOutput);
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void Equals_TwoSuccessWithSameValue_AreEqual()
    {
        var result1 = ExtendedResult<int, string>.Ok(42);
        var result2 = ExtendedResult<int, string>.Ok(42);

        Assert.True(result1.Equals(result2));
        Assert.True(result2.Equals(result1));
        Assert.Equal(result1, result2);
    }

    [Fact]
    public void Equals_TwoSuccessWithDifferentValues_AreNotEqual()
    {
        var result1 = ExtendedResult<int, string>.Ok(42);
        var result2 = ExtendedResult<int, string>.Ok(99);

        Assert.False(result1.Equals(result2));
        Assert.False(result2.Equals(result1));
        Assert.NotEqual(result1, result2);
    }

    [Fact]
    public void Equals_TwoFailuresWithSameError_AreEqual()
    {
        var result1 = ExtendedResult<int, string>.Err("error");
        var result2 = ExtendedResult<int, string>.Err("error");

        Assert.True(result1.Equals(result2));
        Assert.True(result2.Equals(result1));
        Assert.Equal(result1, result2);
    }

    [Fact]
    public void Equals_TwoFailuresWithDifferentErrors_AreNotEqual()
    {
        var result1 = ExtendedResult<int, string>.Err("error1");
        var result2 = ExtendedResult<int, string>.Err("error2");

        Assert.False(result1.Equals(result2));
        Assert.False(result2.Equals(result1));
        Assert.NotEqual(result1, result2);
    }

    [Fact]
    public void Equals_SuccessAndFailure_AreNotEqual()
    {
        var success = ExtendedResult<int, string>.Ok(42);
        var failure = ExtendedResult<int, string>.Err("error");

        Assert.False(success.Equals(failure));
        Assert.False(failure.Equals(success));
        Assert.NotEqual(success, failure);
    }

    [Fact]
    public void Equals_WithNull_ReturnsFalse()
    {
        var result = ExtendedResult<int, string>.Ok(42);

        Assert.False(result.Equals(null));
    }

    [Fact]
    public void Equals_WithSameReference_ReturnsTrue()
    {
        var result = ExtendedResult<int, string>.Ok(42);

        Assert.True(result.Equals(result));
    }

    [Fact]
    public void Equals_WithComplexTypes_Success()
    {
        var person1 = new Person("Alice", 30);
        var person2 = new Person("Alice", 30);

        var result1 = ExtendedResult<Person, string>.Ok(person1);
        var result2 = ExtendedResult<Person, string>.Ok(person2);

        Assert.Equal(result1, result2);
    }

    [Fact]
    public void Equals_WithComplexTypes_Failure()
    {
        var error1 = new ValidationError("error");
        var error2 = new ValidationError("error");

        var result1 = ExtendedResult<int, ValidationError>.Err(error1);
        var result2 = ExtendedResult<int, ValidationError>.Err(error2);

        Assert.Equal(result1, result2);
    }

    [Fact]
    public void Equals_WithNullValues_Success()
    {
        var result1 = ExtendedResult<string?, int>.Ok(null);
        var result2 = ExtendedResult<string?, int>.Ok(null);

        Assert.Equal(result1, result2);
    }

    [Fact]
    public void Equals_WithNullErrors_Failure()
    {
        var result1 = ExtendedResult<int, string?>.Err(null);
        var result2 = ExtendedResult<int, string?>.Err(null);

        Assert.Equal(result1, result2);
    }

    #endregion

    #region GetHashCode Tests

    [Fact]
    public void GetHashCode_TwoSuccessWithSameValue_HaveSameHashCode()
    {
        var result1 = ExtendedResult<int, string>.Ok(42);
        var result2 = ExtendedResult<int, string>.Ok(42);

        Assert.Equal(result1.GetHashCode(), result2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_TwoFailuresWithSameError_HaveSameHashCode()
    {
        var result1 = ExtendedResult<int, string>.Err("error");
        var result2 = ExtendedResult<int, string>.Err("error");

        Assert.Equal(result1.GetHashCode(), result2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_SuccessAndFailure_HaveDifferentHashCodes()
    {
        var success = ExtendedResult<int, string>.Ok(42);
        var failure = ExtendedResult<int, string>.Err("error");

        // Hash codes should be different (though technically collisions are allowed)
        Assert.NotEqual(success.GetHashCode(), failure.GetHashCode());
    }

    [Fact]
    public void GetHashCode_WithNullValue_DoesNotThrow()
    {
        var result = ExtendedResult<string?, int>.Ok(null);

        var hashCode = result.GetHashCode();

        Assert.NotEqual(0, hashCode); // Should still compute a hash
    }

    [Fact]
    public void GetHashCode_WithNullError_DoesNotThrow()
    {
        var result = ExtendedResult<int, string?>.Err(null);

        var hashCode = result.GetHashCode();

        Assert.NotEqual(0, hashCode); // Should still compute a hash
    }

    [Fact]
    public void GetHashCode_UsableInHashSet()
    {
        var set = new HashSet<ExtendedResult<int, string>>
        {
            ExtendedResult<int, string>.Ok(1),
            ExtendedResult<int, string>.Ok(2),
            ExtendedResult<int, string>.Err("error"),
            ExtendedResult<int, string>.Ok(1) // Duplicate
        };

        Assert.Equal(3, set.Count); // Only unique items
    }

    [Fact]
    public void GetHashCode_UsableInDictionary()
    {
        var dict = new Dictionary<ExtendedResult<int, string>, string>
        {
            [ExtendedResult<int, string>.Ok(1)] = "one",
            [ExtendedResult<int, string>.Ok(2)] = "two",
            [ExtendedResult<int, string>.Err("error")] = "error"
        };

        Assert.Equal(3, dict.Count);
        Assert.Equal("one", dict[ExtendedResult<int, string>.Ok(1)]);
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_Success_ReturnsFormattedString()
    {
        var result = ExtendedResult<int, string>.Ok(42);

        var str = result.ToString();

        // Records have their own ToString, so check it contains the value
        Assert.Contains("Success", str);
        Assert.Contains("42", str);
    }

    [Fact]
    public void ToString_Failure_ReturnsFormattedString()
    {
        var result = ExtendedResult<int, string>.Err("Something went wrong");

        var str = result.ToString();

        // Records have their own ToString, so check it contains the error
        Assert.Contains("Failure", str);
        Assert.Contains("Something went wrong", str);
    }

    [Fact]
    public void ToString_WithComplexType_Success()
    {
        var person = new Person("Alice", 30);
        var result = ExtendedResult<Person, string>.Ok(person);

        var str = result.ToString();

        Assert.Contains("Alice", str);
        Assert.Contains("30", str);
    }

    [Fact]
    public void ToString_WithNull_Success()
    {
        var result = ExtendedResult<string?, int>.Ok(null);

        var str = result.ToString();

        Assert.Contains("Success", str);
    }

    [Fact]
    public void ToString_WithNull_Failure()
    {
        var result = ExtendedResult<int, string?>.Err(null);

        var str = result.ToString();

        Assert.Contains("Failure", str);
    }

    #endregion

    #region OrElse Tests

    [Fact]
    public void OrElse_Success_ReturnsOriginalResult()
    {
        var result = ExtendedResult<int, string>.Ok(42);

        var alternative = result.OrElse(error => ExtendedResult<int, string>.Ok(99));

        Assert.True(alternative.TryGetValue(out var value));
        Assert.Equal(42, value); // Original value, not alternative
    }

    [Fact]
    public void OrElse_Failure_ReturnsAlternativeResult()
    {
        var result = ExtendedResult<int, string>.Err("error");

        var alternative = result.OrElse(error => ExtendedResult<int, string>.Ok(99));

        Assert.True(alternative.TryGetValue(out var value));
        Assert.Equal(99, value);
    }

    [Fact]
    public void OrElse_Failure_CanReturnAnotherFailure()
    {
        var result = ExtendedResult<int, string>.Err("first error");

        var alternative = result.OrElse(error => ExtendedResult<int, string>.Err($"handled: {error}"));

        Assert.True(alternative.TryGetError(out var newError));
        Assert.Equal("handled: first error", newError);
    }

    [Fact]
    public void OrElse_ThrowsArgumentNullException_WhenAlternativeIsNull()
    {
        var result = ExtendedResult<int, string>.Err("error");

        var exception = Assert.Throws<ArgumentNullException>(() =>
            result.OrElse(null!)
        );

        Assert.Equal("alternative", exception.ParamName);
    }

    [Fact]
    public void OrElse_Chaining_MultipleAlternatives()
    {
        var result = ExtendedResult<int, string>.Err("error1")
            .OrElse(_ => ExtendedResult<int, string>.Err("error2"))
            .OrElse(_ => ExtendedResult<int, string>.Ok(42));

        Assert.True(result.TryGetValue(out var value));
        Assert.Equal(42, value);
    }

    #endregion

    #region Try Tests

    [Fact]
    public void Try_Success_ReturnsOk()
    {
        var result = ExtendedResult<int, string>.Try(() => 42, ex => ex.Message);

        Assert.True(result.TryGetValue(out var value));
        Assert.Equal(42, value);
    }

    [Fact]
    public void Try_Failure_ReturnsErr()
    {
        var result = ExtendedResult<int, string>.Try(
            () => throw new InvalidOperationException("boom"),
            ex => ex.Message
        );

        Assert.True(result.TryGetError(out var error));
        Assert.Contains("boom", error);
    }

    [Fact]
    public void Try_ThrowsArgumentNullException_WhenOperationIsNull()
    {
        var exception = Assert.Throws<ArgumentNullException>(() =>
            ExtendedResult<int, string>.Try(null!, ex => ex.Message)
        );

        Assert.Equal("operation", exception.ParamName);
    }

    [Fact]
    public void Try_ThrowsArgumentNullException_WhenErrorHandlerIsNull()
    {
        var exception = Assert.Throws<ArgumentNullException>(() =>
            ExtendedResult<int, string>.Try(() => 42, null!)
        );

        Assert.Equal("errorHandler", exception.ParamName);
    }

    [Fact]
    public void Try_WithComplexException_MapsToError()
    {
        var result = ExtendedResult<int, ValidationError>.Try(
            () => throw new ArgumentException("Invalid argument"),
            ex => new ValidationError(ex.Message)
        );

        Assert.True(result.TryGetError(out var error));
        Assert.Equal("Invalid argument", error.Message);
    }

    #endregion

    #region Record Pattern Matching Tests

    [Fact]
    public void RecordPattern_Success_CanDeconstruct()
    {
        var result = ExtendedResult<int, string>.Ok(42);

        var output = result switch
        {
            ExtendedResult<int, string>.Success { Value: var v } => $"Success: {v}",
            ExtendedResult<int, string>.Failure { Error: var e } => $"Failure: {e}",
            _ => "Unknown"
        };

        Assert.Equal("Success: 42", output);
    }

    [Fact]
    public void RecordPattern_Failure_CanDeconstruct()
    {
        var result = ExtendedResult<int, string>.Err("error");

        var output = result switch
        {
            ExtendedResult<int, string>.Success { Value: var v } => $"Success: {v}",
            ExtendedResult<int, string>.Failure { Error: var e } => $"Failure: {e}",
            _ => "Unknown"
        };

        Assert.Equal("Failure: error", output);
    }

    [Fact]
    public void RecordPattern_WithGuard_Success()
    {
        var result = ExtendedResult<int, string>.Ok(42);

        var output = result switch
        {
            ExtendedResult<int, string>.Success { Value: > 40 } => "High",
            ExtendedResult<int, string>.Success { Value: > 20 } => "Medium",
            ExtendedResult<int, string>.Success => "Low",
            _ => "Error"
        };

        Assert.Equal("High", output);
    }

    [Fact]
    public void RecordPattern_ComplexPatternMatching()
    {
        var results = new[]
        {
            ExtendedResult<int, string>.Ok(10),
            ExtendedResult<int, string>.Ok(50),
            ExtendedResult<int, string>.Err("error1"),
            ExtendedResult<int, string>.Ok(30),
            ExtendedResult<int, string>.Err("error2")
        };

        var summary = results.Select(r => r switch
        {
            ExtendedResult<int, string>.Success { Value: > 40 } => "High Success",
            ExtendedResult<int, string>.Success => "Low Success",
            ExtendedResult<int, string>.Failure => "Failure",
            _ => "Unknown"
        }).ToList();

        Assert.Contains("High Success", summary);
        Assert.Contains("Low Success", summary);
        Assert.Contains("Failure", summary);
    }

    #endregion

    // Helper types for testing
    private record Person(string Name, int Age);
    private record ValidationError(string Message);

    [Fact]
    public void Try_Success()
    {
        var result = ExtendedResult<int, string>.Try(() => 10, ex => ex.Message);
        Assert.True(result.TryGetValue(out var value));
        Assert.Equal(10, value);
    }

    [Fact]
    public void Try_Failure()
    {
        var result = ExtendedResult<int, string>.Try(() => throw new InvalidOperationException("x"), ex => ex.Message);
        Assert.True(result.TryGetError(out var error));
        Assert.Contains("x", error);
    }

    // Edge cases with null values/errors

    [Fact]
    public void Ok_WithNullValue_Works()
    {
        ExtendedResult<string?, string> result = ExtendedResult<string?, string>.Ok(null);

        Assert.True(result.TryGetValue(out var value));
        Assert.Null(value);

        // Unwrap returns null for success
        var unwrapped = result.Unwrap();
        Assert.Null(unwrapped);

        // Expect returns null on success
        var expected = result.Expect("ignored message");
        Assert.Null(expected);

        // UnwrapOr still returns success value (null), not default
        var or = result.UnwrapOr("fallback");
        Assert.Null(or);

        // Inspect gets called with null
        string? seen = "init";
        var returned = result.Inspect(v => seen = v);
        Assert.Null(seen);
        Assert.True(returned.TryGetValue(out _));
    }

    [Fact]
    public void Err_WithNullError_Works()
    {
        ExtendedResult<int, string?> result = ExtendedResult<int, string?>.Err(null);

        Assert.True(result.TryGetError(out var error));
        Assert.Null(error);
        Assert.False(result.TryGetValue(out _));

        // Unwrap throws
        Assert.Throws<InvalidOperationException>(() => result.Unwrap());

        // Expect throws with message prefix present
        var ex = Assert.Throws<InvalidOperationException>(() => result.Expect("Need value"));
        Assert.StartsWith("Need value:", ex.Message);

        // UnwrapOrElse factory receives null
        var computed = result.UnwrapOrElse(e => e?.Length ?? -1);
        Assert.Equal(-1, computed);

        // InspectErr gets called with null
        string? seen = "init";
        var returned = result.InspectErr(e => seen = e);
        Assert.Null(seen);
        Assert.True(returned.TryGetError(out _));
    }

    [Fact]
    public void Map_TransformsValue_OnSuccess()
    {
        var result = ExtendedResult<int, string>.Ok(5);
        var mapped = result.Map(x => x * 2);
        Assert.True(mapped.TryGetValue(out var value));
        Assert.Equal(10, value);
    }

    [Fact]
    public void Map_PreservesError_OnFailure()
    {
        var result = ExtendedResult<int, string>.Err("err");
        var mapped = result.Map(x => x * 2);
        Assert.True(mapped.TryGetError(out var error));
        Assert.Equal("err", error);
    }

    [Fact]
    public void Bind_Chains_OnSuccess()
    {
        var result = ExtendedResult<int, string>.Ok(3);
        var bound = result.Bind(x => ExtendedResult<int, string>.Ok(x + 4));
        Assert.True(bound.TryGetValue(out var value));
        Assert.Equal(7, value);
    }

    [Fact]
    public void Bind_Skips_OnFailure()
    {
        var result = ExtendedResult<int, string>.Err("boom");
        var bound = result.Bind(x => ExtendedResult<int, string>.Ok(x + 4));
        Assert.True(bound.TryGetError(out var error));
        Assert.Equal("boom", error);
    }

    [Fact]
    public void MapError_TransformsError_OnFailure()
    {
        var result = ExtendedResult<int, string>.Err("oops");
        var mapped = result.MapError(e => e.Length);
        Assert.True(mapped.TryGetError(out var error));
        Assert.Equal(4, error);
    }

    [Fact]
    public void MapError_PreservesValue_OnSuccess()
    {
        var result = ExtendedResult<int, string>.Ok(9);
        var mapped = result.MapError(e => e.Length);
        Assert.True(mapped.TryGetValue(out var value));
        Assert.Equal(9, value);
    }

    [Fact]
    public void Tap_InvokesBothBranches_Correctly()
    {
        // success path
        var s = ExtendedResult<int, string>.Ok(2);
        int seenValue = 0; string? seenError = null;
        var sReturned = s.Tap(val => seenValue = val, err => seenError = err);
        Assert.Equal(2, seenValue);
        Assert.Null(seenError);
        Assert.True(sReturned.TryGetValue(out var v));
        Assert.Equal(2, v);

        // failure path
        var f = ExtendedResult<int, string>.Err("E");
        seenValue = 0; seenError = null;
        var fReturned = f.Tap(val => seenValue = val, err => seenError = err);
        Assert.Equal(0, seenValue);
        Assert.Equal("E", seenError);
        Assert.True(fReturned.TryGetError(out var e2));
        Assert.Equal("E", e2);
    }

    [Fact]
    public void Linq_Select_ProjectsValue()
    {
        var query = from x in ExtendedResult<int, string>.Ok(10)
                    select x * 3;
        Assert.True(query.TryGetValue(out var value));
        Assert.Equal(30, value);
    }

    [Fact]
    public void Linq_SelectMany_ChainsTwo_Ok()
    {
        var query = from x in ExtendedResult<int, string>.Ok(10)
                    from y in ExtendedResult<int, string>.Ok(5)
                    select x + y;
        Assert.True(query.TryGetValue(out var value));
        Assert.Equal(15, value);
    }

    [Fact]
    public void Linq_SelectMany_PropagatesError()
    {
        var query = from x in ExtendedResult<int, string>.Ok(10)
                    from y in ExtendedResult<int, string>.Err("bad")
                    select x + y;
        Assert.True(query.TryGetError(out var error));
        Assert.Equal("bad", error);
    }

    [Fact]
    public void Combine_AllSuccess_ReturnsList()
    {
        var items = new[]
        {
            ExtendedResult<int, string>.Ok(1),
            ExtendedResult<int, string>.Ok(2),
            ExtendedResult<int, string>.Ok(3),
        };
        var combined = items.Combine();
        Assert.True(combined.TryGetValue(out var values));
        Assert.Equal(new[] {1,2,3}, values);
    }

    [Fact]
    public void Combine_FirstError_ReturnsError()
    {
        var items = new[]
        {
            ExtendedResult<int, string>.Ok(1),
            ExtendedResult<int, string>.Err("err2"),
            ExtendedResult<int, string>.Ok(3),
        };
        var combined = items.Combine();
        Assert.True(combined.TryGetError(out var error));
        Assert.Equal("err2", error);
    }

    [Fact]
    public void Partition_SplitsSuccessesAndFailures()
    {
        var items = new[]
        {
            ExtendedResult<int, string>.Ok(1),
            ExtendedResult<int, string>.Err("e1"),
            ExtendedResult<int, string>.Ok(2),
            ExtendedResult<int, string>.Err("e2"),
        };
        var (successes, failures) = items.Partition();
        Assert.Equal(new[] {1,2}, successes);
        Assert.Equal(new[] {"e1","e2"}, failures);
    }

    #region Implicit Conversion Tests

    [Fact]
    public void ImplicitConversion_FromValue_CreatesSuccessfulResult()
    {
        // Act
        ExtendedResult<int, string> result = 42;

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.TryGetValue(out var value));
        Assert.Equal(42, value);
    }

    [Fact]
    public void ImplicitConversion_FromError_CreatesFailedResult()
    {
        // Act
        ExtendedResult<int, string> result = "Error occurred";

        // Assert
        Assert.True(result.IsFailure);
        Assert.True(result.TryGetError(out var error));
        Assert.Equal("Error occurred", error);
    }

    [Fact]
    public void ImplicitConversion_WithComplexType_WorksCorrectly()
    {
        // Arrange
        var person = new Person("Alice", 25);

        // Act
        ExtendedResult<Person, string> result = person;

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.TryGetValue(out var value));
        Assert.Equal("Alice", value.Name);
        Assert.Equal(25, value.Age);
    }

    [Fact]
    public void ImplicitConversion_CanBeUsedInReturnStatements()
    {
        // Act
        var successResult = GetSuccessResult();
        var errorResult = GetErrorResult();

        // Assert
        Assert.True(successResult.IsSuccess);
        Assert.Equal(100, successResult.UnwrapOr(0));
        Assert.True(errorResult.IsFailure);
        Assert.True(errorResult.TryGetError(out var error));
        Assert.Equal("Something went wrong", error);

        // Local functions using implicit conversion
        static ExtendedResult<int, string> GetSuccessResult() => 100;
        static ExtendedResult<int, string> GetErrorResult() => "Something went wrong";
    }

    [Fact]
    public void ImplicitConversion_CanBeUsedWithNullableTypes()
    {
        // Act
        ExtendedResult<string?, int> resultWithNull = (string?)null;
        ExtendedResult<int, string?> errorWithNull = (string?)null;

        // Assert
        Assert.True(resultWithNull.IsSuccess);
        Assert.True(resultWithNull.TryGetValue(out var value));
        Assert.Null(value);

        Assert.True(errorWithNull.IsFailure);
        Assert.True(errorWithNull.TryGetError(out var error));
        Assert.Null(error);
    }

    [Fact]
    public void ImplicitConversion_WorksInMethodParameters()
    {
        // Act
        var successResult = ProcessResult(42);
        var errorResult = ProcessResult("Failed");

        // Assert
        Assert.Equal("Success: 42", successResult);
        Assert.Equal("Error: Failed", errorResult);

        // Local function accepting ExtendedResult via implicit conversion
        static string ProcessResult(ExtendedResult<int, string> result)
        {
            return result.Match(
                success: v => $"Success: {v}",
                failure: e => $"Error: {e}"
            );
        }
    }

    [Fact]
    public void ImplicitConversion_WorksWithPatternMatching()
    {
        // Arrange
        ExtendedResult<int, string> successResult = 42;
        ExtendedResult<int, string> errorResult = "error";

        // Act
        var successOutput = successResult switch
        {
            ExtendedResult<int, string>.Success { Value: var v } => $"Got: {v}",
            ExtendedResult<int, string>.Failure { Error: var e } => $"Error: {e}",
            _ => "Unknown"
        };

        var errorOutput = errorResult switch
        {
            ExtendedResult<int, string>.Success { Value: var v } => $"Got: {v}",
            ExtendedResult<int, string>.Failure { Error: var e } => $"Error: {e}",
            _ => "Unknown"
        };

        // Assert
        Assert.Equal("Got: 42", successOutput);
        Assert.Equal("Error: error", errorOutput);
    }

    [Fact]
    public void ImplicitConversion_WorksInCollections()
    {
        // Arrange & Act - Mix implicit conversions in array initialization
        ExtendedResult<int, string>[] results = new[]
        {
            ExtendedResult<int, string>.Ok(1), // Explicit
            2,                                   // Implicit success
            ExtendedResult<int, string>.Err("error1"), // Explicit
            "error2"                            // Implicit error
        };

        // Assert
        Assert.Equal(4, results.Length);
        Assert.True(results[0].IsSuccess);
        Assert.True(results[1].IsSuccess);
        Assert.True(results[2].IsFailure);
        Assert.True(results[3].IsFailure);

        Assert.Equal(1, results[0].UnwrapOr(0));
        Assert.Equal(2, results[1].UnwrapOr(0));
        Assert.Equal("error1", results[2].Match(_ => "", e => e));
        Assert.Equal("error2", results[3].Match(_ => "", e => e));
    }

    #endregion
}

