using AspectInjector.Analyzer.Analyzers;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using Xunit;

namespace AspectInjector.Analyzer.Test.Analyzers
{
    public class ArgumentAnalyzerTests : CorrectDefinitionsTests
    {
    
        [Fact]
        public void Argument_Should_Be_Part_Of_Advice()
        {
            var test =
@"using AspectInjector.Broker;
namespace TestNameSpace
{
    [Aspect(Aspect.Scope.Global)]
    class TypeClass
    {
        public static void Before([Advice.Argument(Advice.Argument.Source.Instance)] object i){}
    }
}";
            var expected = DiagnosticResult.From(Rules.ArgumentMustBePartOfAdvice, 7, 36);
            VerifyCSharpDiagnostic(test, expected);
        }

        [Fact]
        public void Argument_Types_Must_Be_Valid()
        {
            var test =
@"using AspectInjector.Broker;
namespace TestNameSpace
{
    [Aspect(Aspect.Scope.Global)]
    class TypeClass
    {
        [Advice(Advice.Kind.Before)]
        public void Before(
            [Advice.Argument(Advice.Argument.Source.Instance)] int i,
            [Advice.Argument(Advice.Argument.Source.Type)] int t,
            [Advice.Argument(Advice.Argument.Source.Method)] int m,
            [Advice.Argument(Advice.Argument.Source.Name)] int n,
            [Advice.Argument(Advice.Argument.Source.Arguments)] int a,
            [Advice.Argument(Advice.Argument.Source.ReturnType)] int rt,
            [Advice.Argument(Advice.Argument.Source.ReturnValue)] int rv,
            [Advice.Argument(Advice.Argument.Source.Target)] int target 
        )
        { }
    }
}";
            var expected = new[] {
                DiagnosticResult.From(Rules.ArgumentMustHaveValidType, 9, 14),
                DiagnosticResult.From(Rules.ArgumentMustHaveValidType, 10, 14),
                DiagnosticResult.From(Rules.ArgumentMustHaveValidType, 11, 14),
                DiagnosticResult.From(Rules.ArgumentMustHaveValidType, 12, 14),
                DiagnosticResult.From(Rules.ArgumentMustHaveValidType, 13, 14),
                DiagnosticResult.From(Rules.ArgumentMustHaveValidType, 14, 14),
                DiagnosticResult.From(Rules.ArgumentMustHaveValidType, 15, 14),
                DiagnosticResult.From(Rules.ArgumentIsAlwaysNull, 15, 14),
                DiagnosticResult.From(Rules.ArgumentMustHaveValidType, 16, 14),
                DiagnosticResult.From(Rules.ArgumentIsAlwaysNull, 16, 14),

            };
            VerifyCSharpDiagnostic(test, expected);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new ArgumentAttributeAnalyzer();
        }
    }
}
