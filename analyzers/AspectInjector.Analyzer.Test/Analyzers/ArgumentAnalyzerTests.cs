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
            var expected = DiagnosticResult.From(EffectRules.ArgumentMustBePartOfAdvice.AsDescriptor(), 7, 36);
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
                DiagnosticResult.From(EffectRules.ArgumentMustHaveValidType.AsDescriptor(), 9, 14),
                DiagnosticResult.From(EffectRules.ArgumentMustHaveValidType.AsDescriptor(), 10, 14),
                DiagnosticResult.From(EffectRules.ArgumentMustHaveValidType.AsDescriptor(), 11, 14),
                DiagnosticResult.From(EffectRules.ArgumentMustHaveValidType.AsDescriptor(), 12, 14),
                DiagnosticResult.From(EffectRules.ArgumentMustHaveValidType.AsDescriptor(), 13, 14),
                DiagnosticResult.From(EffectRules.ArgumentMustHaveValidType.AsDescriptor(), 14, 14),
                DiagnosticResult.From(EffectRules.ArgumentMustHaveValidType.AsDescriptor(), 15, 14),
                DiagnosticResult.From(EffectRules.ArgumentIsAlwaysNull.AsDescriptor(), 15, 14),
                DiagnosticResult.From(EffectRules.ArgumentMustHaveValidType.AsDescriptor(), 16, 14),
                DiagnosticResult.From(EffectRules.ArgumentIsAlwaysNull.AsDescriptor(), 16, 14),
                DiagnosticResult.From(EffectRules.ArgumentMustHaveValidType.AsDescriptor(), 17, 14),

            };
            VerifyCSharpDiagnostic(test, expected);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new ArgumentAttributeAnalyzer();
        }
    }
}
