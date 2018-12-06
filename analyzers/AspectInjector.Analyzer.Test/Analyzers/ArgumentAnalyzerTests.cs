using AspectInjector.Analyzer.Analyzers;
using AspectInjector.Rules;
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
    [Aspect(Scope.Global)]
    class TypeClass
    {
        public static void Before([Argument(Source.Instance)] object i){}
    }
}";
            var expected = DiagnosticResult.From(EffectRules.ArgumentMustBePartOfAdvice, 7, 36);
            VerifyCSharpDiagnostic(test, expected);
        }

        [Fact]
        public void Argument_Types_Must_Be_Valid()
        {
            var test =
@"using AspectInjector.Broker;
namespace TestNameSpace
{
    [Aspect(Scope.Global)]
    class TypeClass
    {
        [Advice(Kind.Before)]
        public void Before(
            [Argument(Source.Instance)] int i,
            [Argument(Source.Type)] int t,
            [Argument(Source.Method)] int m,
            [Argument(Source.Name)] int n,
            [Argument(Source.Arguments)] int a,
            [Argument(Source.ReturnType)] int rt,
            [Argument(Source.ReturnValue)] int rv,
            [Argument(Source.Target)] int target, 
            [Argument(Source.Injections)] int inj
        )
        { }
    }
}";
            var expected = new[] {
                DiagnosticResult.From(EffectRules.ArgumentMustHaveValidType, 9, 14),
                DiagnosticResult.From(EffectRules.ArgumentMustHaveValidType, 10, 14),
                DiagnosticResult.From(EffectRules.ArgumentMustHaveValidType, 11, 14),
                DiagnosticResult.From(EffectRules.ArgumentMustHaveValidType, 12, 14),
                DiagnosticResult.From(EffectRules.ArgumentMustHaveValidType, 13, 14),
                DiagnosticResult.From(EffectRules.ArgumentMustHaveValidType, 14, 14),
                DiagnosticResult.From(EffectRules.ArgumentMustHaveValidType, 15, 14),
                DiagnosticResult.From(EffectRules.ArgumentIsAlwaysNull, 15, 14),
                DiagnosticResult.From(EffectRules.ArgumentMustHaveValidType, 16, 14),
                DiagnosticResult.From(EffectRules.ArgumentIsAlwaysNull, 16, 14),
                DiagnosticResult.From(EffectRules.ArgumentMustHaveValidType, 17, 14),

            };
            VerifyCSharpDiagnostic(test, expected);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new ArgumentAttributeAnalyzer();
        }
    }
}
