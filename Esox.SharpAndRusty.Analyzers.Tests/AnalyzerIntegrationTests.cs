using System.Diagnostics;

namespace Esox.SharpAndRusty.Analyzers.Tests;

public class AnalyzerIntegrationTests
{
    [Fact]
    public void AnalyzerDemoProject_IsWiredForAnalyzerIntegration()
    {
        var solutionRoot = GetSolutionRoot();
        var demoProjectPath = Path.Combine(solutionRoot, "AnalyzerDemo", "AnalyzerDemo.csproj");
        var warningDemoPath = Path.Combine(solutionRoot, "AnalyzerDemo", "AnalyzerWarningsDemo.cs");

        Assert.True(File.Exists(demoProjectPath), $"Expected AnalyzerDemo project at '{demoProjectPath}'.");
        Assert.True(File.Exists(warningDemoPath), $"Expected warning demo file at '{warningDemoPath}'.");

        var csprojText = File.ReadAllText(demoProjectPath);
        Assert.Contains("Esox.SharpAndRusty.Analyzers\\Esox.SharpAndRusty.Analyzers.csproj", csprojText);
        Assert.Contains("OutputItemType=\"Analyzer\"", csprojText);
        Assert.Contains("ReferenceOutputAssembly=\"false\"", csprojText);

        var demoSource = File.ReadAllText(warningDemoPath);
        Assert.Contains("GetValue();", demoSource);
        Assert.Contains("GetName();", demoSource);

        var output = RunDotnetBuild(demoProjectPath);
        Assert.Contains("Build succeeded", output);
    }

    private static string GetSolutionRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);

        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "Esox.SharpAndRusty.slnx")))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate solution root containing Esox.SharpAndRusty.slnx.");
    }

    private static string RunDotnetBuild(string projectPath)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"build \"{projectPath}\" -nologo",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi) ?? throw new InvalidOperationException("Failed to start dotnet process.");

        var standardOutput = process.StandardOutput.ReadToEnd();
        var standardError = process.StandardError.ReadToEnd();

        process.WaitForExit();

        var combinedOutput = standardOutput + Environment.NewLine + standardError;

        Assert.True(process.ExitCode == 0, $"dotnet build failed with exit code {process.ExitCode}.{Environment.NewLine}{combinedOutput}");

        return combinedOutput;
    }
}

