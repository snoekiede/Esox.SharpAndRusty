using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Esox.SharpAndRusty.Analyzers
{
    /// <summary>
    /// Analyzer that flags unhandled Result and Option types, similar to Rust's #[must_use] attribute.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class UnhandledResultAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticIdResult = "ESOX1001";
        public const string DiagnosticIdOption = "ESOX1002";

        private const string Category = "Usage";

        private static readonly LocalizableString TitleResult = "Result value must be used";
        private static readonly LocalizableString MessageFormatResult = "Result<{0}, {1}> returned by '{2}' must be used. Ignoring a Result may hide errors.";
        private static readonly LocalizableString DescriptionResult = "Methods returning Result<T, E> must have their return value handled. This prevents accidentally ignoring potential errors, similar to Rust's behavior.";

        private static readonly LocalizableString TitleOption = "Option value must be used";
        private static readonly LocalizableString MessageFormatOption = "Option<{0}> returned by '{1}' must be used. Ignoring an Option may hide missing values.";
        private static readonly LocalizableString DescriptionOption = "Methods returning Option<T> must have their return value handled. This prevents accidentally ignoring missing values, similar to Rust's behavior.";

        private static readonly DiagnosticDescriptor RuleResult = new DiagnosticDescriptor(
            DiagnosticIdResult,
            TitleResult,
            MessageFormatResult,
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: DescriptionResult,
            helpLinkUri: "https://github.com/snoekiede/Esox.SharpAndRusty/wiki/ESOX1001");

        private static readonly DiagnosticDescriptor RuleOption = new DiagnosticDescriptor(
            DiagnosticIdOption,
            TitleOption,
            MessageFormatOption,
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: DescriptionOption,
            helpLinkUri: "https://github.com/snoekiede/Esox.SharpAndRusty/wiki/ESOX1002");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(RuleResult, RuleOption);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            // Analyze invocation expressions (method calls)
            context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
            
            // Analyze member access expressions (property access)
            context.RegisterSyntaxNodeAction(AnalyzeMemberAccess, SyntaxKind.SimpleMemberAccessExpression);
        }

        private void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;
            
            // Get the symbol for the method being invoked
            var symbolInfo = context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken);
            if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
                return;

            // Check if return type is Result or Option
            var returnType = methodSymbol.ReturnType;
            if (!IsResultOrOptionType(returnType, out var isResult, out var typeArgs))
                return;

            // Check if the result is being used
            if (IsResultUsed(invocation))
                return;

            // Report diagnostic
            ReportDiagnostic(context, invocation.GetLocation(), isResult, typeArgs, methodSymbol.Name);
        }

        private void AnalyzeMemberAccess(SyntaxNodeAnalysisContext context)
        {
            var memberAccess = (MemberAccessExpressionSyntax)context.Node;
            
            // Only analyze if this is a standalone expression statement
            if (memberAccess.Parent is InvocationExpressionSyntax)
                return; // Handled by AnalyzeInvocation
            
            if (memberAccess.Parent is not ExpressionStatementSyntax)
                return;

            // Get the symbol for the member being accessed
            var symbolInfo = context.SemanticModel.GetSymbolInfo(memberAccess, context.CancellationToken);
            if (symbolInfo.Symbol is not IPropertySymbol propertySymbol)
                return;

            // Check if return type is Result or Option
            var returnType = propertySymbol.Type;
            if (!IsResultOrOptionType(returnType, out var isResult, out var typeArgs))
                return;

            // Report diagnostic
            ReportDiagnostic(context, memberAccess.GetLocation(), isResult, typeArgs, propertySymbol.Name);
        }

        private bool IsResultOrOptionType(ITypeSymbol type, out bool isResult, out ImmutableArray<ITypeSymbol> typeArguments)
        {
            isResult = false;
            typeArguments = ImmutableArray<ITypeSymbol>.Empty;

            if (type is not INamedTypeSymbol namedType)
                return false;

            var fullName = GetFullTypeName(namedType);

            // Check for Result<T, E>
            if (fullName == "Esox.SharpAndRusty.Types.Result" && namedType.TypeArguments.Length == 2)
            {
                isResult = true;
                typeArguments = namedType.TypeArguments;
                return true;
            }

            // Check for Option<T>
            if (fullName == "Esox.SharpAndRusty.Types.Option" && namedType.TypeArguments.Length == 1)
            {
                isResult = false;
                typeArguments = namedType.TypeArguments;
                return true;
            }

            return false;
        }

        private string GetFullTypeName(INamedTypeSymbol type)
        {
            if (type.ContainingNamespace == null || type.ContainingNamespace.IsGlobalNamespace)
                return type.Name;

            return $"{type.ContainingNamespace.ToDisplayString()}.{type.Name}";
        }

        private bool IsResultUsed(ExpressionSyntax expression)
        {
            var parent = expression.Parent;

            // Check various ways the result might be used
            return parent switch
            {
                // Assigned to a variable: var x = GetResult();
                EqualsValueClauseSyntax => true,
                
                // Used in a variable declaration: Result<int, string> x = GetResult();
                VariableDeclarationSyntax => true,
                
                // Returned from a method: return GetResult();
                ReturnStatementSyntax => true,
                
                // Used as an argument: DoSomething(GetResult());
                ArgumentSyntax => true,
                
                // Used in a binary expression: if (GetResult() == something)
                BinaryExpressionSyntax => true,
                
                // Used in pattern matching: if (GetResult() is Result<int, string>.Ok ok)
                IsPatternExpressionSyntax => true,
                
                // Used in switch expression: GetResult() switch { ... }
                SwitchExpressionSyntax => true,
                
                // Used in switch statement: switch (GetResult()) { ... }
                SwitchStatementSyntax => true,
                
                // Used with await: await GetResult();
                AwaitExpressionSyntax => true,
                
                // Used in member access: GetResult().Match(...)
                MemberAccessExpressionSyntax => true,
                
                // Used in array initializer: new[] { GetResult() }
                InitializerExpressionSyntax => true,
                
                // Used in collection expression: [GetResult()]
                CollectionExpressionSyntax => true,
                
                // Used in interpolation: $"{GetResult()}"
                InterpolationSyntax => true,
                
                // Used in conditional: condition ? GetResult() : other
                ConditionalExpressionSyntax => true,
                
                // Used in throw: throw new Exception(GetResult().ToString())
                ThrowStatementSyntax => true,
                ThrowExpressionSyntax => true,
                
                // Standalone expression statement: GetResult(); <- NOT USED
                ExpressionStatementSyntax => false,
                
                // Default case
                _ => true // Be conservative - assume it's used if we don't recognize the pattern
            };
        }

        private void ReportDiagnostic(
            SyntaxNodeAnalysisContext context,
            Location location,
            bool isResult,
            ImmutableArray<ITypeSymbol> typeArgs,
            string memberName)
        {
            if (isResult && typeArgs.Length == 2)
            {
                var diagnostic = Diagnostic.Create(
                    RuleResult,
                    location,
                    typeArgs[0].ToDisplayString(),
                    typeArgs[1].ToDisplayString(),
                    memberName);
                context.ReportDiagnostic(diagnostic);
            }
            else if (!isResult && typeArgs.Length == 1)
            {
                var diagnostic = Diagnostic.Create(
                    RuleOption,
                    location,
                    typeArgs[0].ToDisplayString(),
                    memberName);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}

