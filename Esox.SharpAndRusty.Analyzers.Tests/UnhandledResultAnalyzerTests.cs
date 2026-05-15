using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;

namespace Esox.SharpAndRusty.Analyzers.Tests
{
    public class UnhandledResultAnalyzerTests
    {
        [Fact]
        public async Task UnhandledResult_ReportsDiagnostic()
        {
            var test = @"
using Esox.SharpAndRusty.Types;

public class TestClass
{
    public Result<int, string> GetResult() => Result<int, string>.Ok(42);

    public void TestMethod()
    {
        {|#0:GetResult()|};
    }
}";

            var expected = new DiagnosticResult(UnhandledResultAnalyzer.DiagnosticIdResult, DiagnosticSeverity.Warning)
                .WithLocation(0)
                .WithArguments("int", "string", "GetResult");

            await VerifyAnalyzerAsync(test, expected);
        }

        [Fact]
        public async Task UnhandledOption_ReportsDiagnostic()
        {
            var test = @"
using Esox.SharpAndRusty.Types;

public class TestClass
{
    public Option<int> GetOption() => new Option<int>.Some(42);

    public void TestMethod()
    {
        {|#0:GetOption()|};
    }
}";

            var expected = new DiagnosticResult(UnhandledResultAnalyzer.DiagnosticIdOption, DiagnosticSeverity.Warning)
                .WithLocation(0)
                .WithArguments("int", "GetOption");

            await VerifyAnalyzerAsync(test, expected);
        }

        [Fact]
        public async Task ResultAssignedToVariable_NoDiagnostic()
        {
            var test = @"
using Esox.SharpAndRusty.Types;

public class TestClass
{
    public Result<int, string> GetResult() => Result<int, string>.Ok(42);

    public void TestMethod()
    {
        var result = GetResult();
    }
}";

            await VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task ResultReturned_NoDiagnostic()
        {
            var test = @"
using Esox.SharpAndRusty.Types;

public class TestClass
{
    public Result<int, string> GetResult() => Result<int, string>.Ok(42);

    public Result<int, string> TestMethod()
    {
        return GetResult();
    }
}";

            await VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task ResultUsedInMatch_NoDiagnostic()
        {
            var test = @"
using Esox.SharpAndRusty.Types;

public class TestClass
{
    public Result<int, string> GetResult() => Result<int, string>.Ok(42);

    public void TestMethod()
    {
        GetResult().Match(
            ok => System.Console.WriteLine(ok),
            err => System.Console.WriteLine(err));
    }
}";

            await VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task ResultUsedAsArgument_NoDiagnostic()
        {
            var test = @"
using Esox.SharpAndRusty.Types;

public class TestClass
{
    public Result<int, string> GetResult() => Result<int, string>.Ok(42);

    public void UseResult(Result<int, string> result) { }

    public void TestMethod()
    {
        UseResult(GetResult());
    }
}";

            await VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task ResultUsedInPatternMatching_NoDiagnostic()
        {
            var test = @"
using Esox.SharpAndRusty.Types;

public class TestClass
{
    public Result<int, string> GetResult() => Result<int, string>.Ok(42);

    public void TestMethod()
    {
        if (GetResult().TryGetValue(out var value))
        {
            System.Console.WriteLine(value);
        }
    }
}";

            await VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task ResultUsedInSwitchExpression_NoDiagnostic()
        {
            var test = @"
using Esox.SharpAndRusty.Types;

public class TestClass
{
    public Result<int, string> GetResult() => Result<int, string>.Ok(42);

    public void TestMethod()
    {
        var message = GetResult().IsSuccess ? ""Success"" : ""Failure"";
    }
}";

            await VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task ResultExplicitlyDiscarded_NoDiagnostic()
        {
            var test = @"
using Esox.SharpAndRusty.Types;

public class TestClass
{
    public Result<int, string> GetResult() => Result<int, string>.Ok(42);

    public void TestMethod()
    {
        _ = GetResult();
    }
}";

            await VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task MultipleUnhandledResults_ReportsMultipleDiagnostics()
        {
            var test = @"
using Esox.SharpAndRusty.Types;

public class TestClass
{
    public Result<int, string> GetResult() => Result<int, string>.Ok(42);
    public Option<string> GetOption() => new Option<string>.Some(""test"");

    public void TestMethod()
    {
        {|#0:GetResult()|};
        {|#1:GetOption()|};
    }
}";

            var expected1 = new DiagnosticResult(UnhandledResultAnalyzer.DiagnosticIdResult, DiagnosticSeverity.Warning)
                .WithLocation(0)
                .WithArguments("int", "string", "GetResult");

            var expected2 = new DiagnosticResult(UnhandledResultAnalyzer.DiagnosticIdOption, DiagnosticSeverity.Warning)
                .WithLocation(1)
                .WithArguments("string", "GetOption");

            await VerifyAnalyzerAsync(test, expected1, expected2);
        }

        [Fact]
        public async Task ResultUsedInAwaitExpression_NoDiagnostic()
        {
            var test = @"
using System.Threading.Tasks;
using Esox.SharpAndRusty.Types;

public class TestClass
{
    public Task<Result<int, string>> GetResultAsync() => 
        Task.FromResult(Result<int, string>.Ok(42));

    public async Task TestMethod()
    {
        var result = await GetResultAsync();
    }
}";

            await VerifyAnalyzerAsync(test);
        }

        private static async Task VerifyAnalyzerAsync(string source, params DiagnosticResult[] expected)
        {
            var test = new CSharpAnalyzerTest<UnhandledResultAnalyzer, XUnitVerifier>
            {
                TestCode = source,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            };

            // Add reference to Esox.SharpAndRusty
            test.TestState.AdditionalReferences.Add(typeof(Esox.SharpAndRusty.Types.Result<,>).Assembly);

            // Manually add the analyzer assembly
            var analyzerPath = Path.Combine(
                Path.GetDirectoryName(typeof(UnhandledResultAnalyzerTests).Assembly.Location)!,
                "..", "..", "..", "..", "Esox.SharpAndRusty.Analyzers", "bin", "Debug", "netstandard2.0",
                "Esox.SharpAndRusty.Analyzers.dll");
            
            if (File.Exists(analyzerPath))
            {
                test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", ""));
            }

            test.ExpectedDiagnostics.AddRange(expected);

            await test.RunAsync();
        }
    }
}

