using Esox.SharpAndRusty.Types;

namespace AnalyzerDemo;

/// <summary>
/// Comprehensive analyzer test coverage for all scenarios defined in UnhandledResultAnalyzer.
/// This file exercises every code path in the IsResultUsed() method.
/// </summary>
class ComprehensiveAnalyzerTests
{
    static Result<int, string> GetResult() => Result<int, string>.Ok(42);
    static Option<string> GetOption() => new Option<string>.Some("test");
    static async Task<Result<int, string>> GetResultAsync() => Result<int, string>.Ok(42);

    // ❌ BAD: These should trigger warnings
    static void UnhandledCases()
    {
        // Standalone expression statement - should warn
        GetResult();  // ESOX1001
        GetOption();  // ESOX1002
    }

    // ✅ GOOD: All these should NOT trigger warnings
    static void EqualsValueClause()
    {
        // Assigned to a variable
        var result = GetResult();
        Console.WriteLine(result);
    }

    static void VariableDeclaration()
    {
        // Explicit type variable declaration
        Result<int, string> result = GetResult();
        Option<string> option = GetOption();
    }

    static Result<int, string> ReturnStatement()
    {
        // Returned from method
        return GetResult();
    }

    static void ArgumentSyntax()
    {
        // Used as method argument
        ProcessResult(GetResult());
        ProcessOption(GetOption());
    }

    static void BinaryExpression()
    {
        // Used in binary expression
        var isEqual = GetResult() == Result<int, string>.Ok(42);
        var isDifferent = GetOption() != new Option<string>.None();
    }

    static void IsPatternExpression()
    {
        // Used in pattern matching
        if (GetResult().TryGetValue(out var value))
        {
            Console.WriteLine(value);
        }

        if (GetOption() is Option<string>.Some(var name))
        {
            Console.WriteLine(name);
        }
    }

    static void SwitchExpression()
    {
        // Used in switch expression
        var message = GetOption() switch
        {
            Option<string>.Some(var s) => s,
            Option<string>.None => "none",
            _ => "unknown"
        };
    }

    static void SwitchStatement()
    {
        // Used in switch statement (on property)
        switch (GetResult().IsSuccess)
        {
            case true:
                Console.WriteLine("Success");
                break;
            case false:
                Console.WriteLine("Failure");
                break;
        }
    }

    static async Task AwaitExpression()
    {
        // Used with await
        var result = await GetResultAsync();
        Console.WriteLine(result);
    }

    static void MemberAccessExpression()
    {
        // Used in member access (chained method)
        var message = GetResult().Match(
            ok => $"OK: {ok}",
            err => $"Error: {err}");
        Console.WriteLine(message);

        // Chained property access
        var isSuccess = GetResult().IsSuccess;
    }

    static void InitializerExpression()
    {
        // Used in array initializer
        var results = new[] { GetResult(), GetResult() };

        // Used in list initializer
        var list = new List<Result<int, string>>
        {
            GetResult(),
            GetResult()
        };

        // Used in dictionary initializer
        var dict = new Dictionary<string, Result<int, string>>
        {
            { "first", GetResult() },
            { "second", GetResult() }
        };
    }

    static void CollectionExpression()
    {
        // Used in collection expression (C# 12)
        Result<int, string>[] array = [GetResult(), GetResult()];
        List<Option<string>> list = [GetOption(), GetOption()];
    }

    static void InterpolationSyntax()
    {
        // Used in string interpolation
        var message = $"Result: {GetResult()}, Option: {GetOption()}";
        Console.WriteLine(message);
    }

    static void ConditionalExpression()
    {
        // Used in ternary conditional
        var result = true ? GetResult() : Result<int, string>.Err("error");
        var option = false ? new Option<string>.None() : GetOption();
    }

    static void ThrowStatement()
    {
        // Used in throw statement
        var result = GetResult();
        if (result.IsFailure)
        {
            throw new InvalidOperationException(result.ToString());
        }
    }

    static void ThrowExpression()
    {
        // Used in throw expression
        var result = GetResult().IsSuccess 
            ? GetResult() 
            : throw new InvalidOperationException("Failed");
    }

    static void ExplicitDiscard()
    {
        // Explicitly discarded - shows intent
        _ = GetResult();
        _ = GetOption();
    }

    // Edge cases and combinations
    static void NestedExpressions()
    {
        // Nested in LINQ
        var results = new[] { 1, 2, 3 }
            .Select(_ => GetResult())
            .ToList();

        // Nested in lambda
        var func = () => GetResult();
        var result = func();

        // As lambda return
        Func<Result<int, string>> createResult = () => GetResult();
    }

    static void AsyncScenarios()
    {
        // Task.Run with Result
        var task = Task.Run(() => GetResult());
        var result = task.Result;

        // Async lambda
        Func<Task<Result<int, string>>> asyncFunc = async () => await GetResultAsync();
    }

    static void PropertyScenarios()
    {
        // Property that returns Result - should work the same
        var obj = new ResultContainer();
        var value = obj.Value;  // Used
        // obj.Value;  // Would trigger warning if uncommented
    }

    static void MultipleCalls()
    {
        // Multiple calls in same expression
        var both = (GetResult(), GetOption());
        Console.WriteLine(both);

        // In method chain
        var text = GetResult()
            .Match(ok => ok.ToString(), err => err)
            .ToUpper();
    }

    // Helper methods
    static void ProcessResult(Result<int, string> result) { }
    static void ProcessOption(Option<string> option) { }

    class ResultContainer
    {
        public Result<int, string> Value => GetResult();
    }
}




