using Esox.SharpAndRusty.Types;

namespace AnalyzerDemo;

/// <summary>
/// This class intentionally has unhandled Result and Option calls
/// to demonstrate the ESOX1001 and ESOX1002 analyzer warnings.
/// </summary>
class AnalyzerWarningsDemo
{
    static Result<int, string> GetValue()
    {
        return Result<int, string>.Ok(42);
    }

    static Option<string> GetName()
    {
        return new Option<string>.Some("Test");
    }

    static void BadExample()
    {
        // This should trigger ESOX1001 warning
        GetValue();

        // This should trigger ESOX1002 warning
        GetName();
    }

    static void GoodExample()
    {
        // These are properly handled - no warnings
        var value = GetValue();
        var name = GetName();
        
        Console.WriteLine($"Value: {value}, Name: {name}");
    }
}

