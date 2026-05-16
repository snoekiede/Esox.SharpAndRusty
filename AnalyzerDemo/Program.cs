using Esox.SharpAndRusty.Types;

namespace AnalyzerDemo;

class Program
{
    static void Main()
    {
        Console.WriteLine("=== Esox.SharpAndRusty Analyzer Demo ===\n");

        // ❌ BAD: These will trigger ESOX1001 warnings
        Console.WriteLine("Bad Examples (should show warnings in IDE):");
        // GetResult();  // Uncomment to see ESOX1001 warning
        // GetOption();  // Uncomment to see ESOX1002 warning
        Console.WriteLine("(Commented out to allow compilation)\n");

        // ✅ GOOD: Proper Result handling
        Console.WriteLine("Good Examples (no warnings):");
        
        // Method 1: Assign to variable
        var result1 = GetResult();
        Console.WriteLine($"Result 1: {result1}");

        // Method 2: Use Match
        var message = GetResult().Match(
            ok => $"Success: {ok}",
            err => $"Error: {err}");
        Console.WriteLine(message);

        // Method 3: Check with pattern matching
        if (GetResult().TryGetValue(out var value))
        {
            Console.WriteLine($"Got value: {value}");
        }

        // Method 4: Return it
        var result2 = ProcessAndReturn();
        Console.WriteLine($"Processed: {result2}");

        // Method 5: Explicit discard (not recommended but won't warn)
        _ = GetResult();

        Console.WriteLine("\n✅ All Result types handled properly!");

        // Same for Option
        Console.WriteLine("\nOption Examples:");
        var option = GetOption();
        if (option is Option<string>.Some(var name))
        {
            Console.WriteLine($"Name: {name}");
        }
        else
        {
            Console.WriteLine("No name provided");
        }
    }

    static Result<int, string> GetResult()
    {
        return Result<int, string>.Ok(42);
    }

    static Option<string> GetOption()
    {
        return new Option<string>.Some("Rust-like safety in C#!");
    }

    static Result<int, string> ProcessAndReturn()
    {
        // Returning Result is fine - it's being handled by the caller
        return GetResult();
    }
}


