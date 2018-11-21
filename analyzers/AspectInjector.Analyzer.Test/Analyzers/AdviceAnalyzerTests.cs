using AspectInjector.Analyzer.Analyzers;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using Xunit;

namespace AspectInjector.Analyzer.Test.Analyzers
{
    public class AdviceAnalyzerTests : CodeFixVerifier
    {
        [Fact]
        public void Advice_NoCode_NoDiagnostics()
        {
            var test = @"";
            VerifyCSharpDiagnostic(test);
        }

        [Fact]
        public void Advice_Must_Not_Be_Static()
        {
            var test =
@"using AspectInjector.Broker;
namespace TestNameSpace
{
    [Aspect(Aspect.Scope.Global)]
    class TypeClass
    {
        [Advice(Advice.Type.Before, Advice.Target.Method)]
        public static void Before(){}
    }
}";
            var expected = DiagnosticResult.From(Rules.AdviceMustHaveValidSingnature, 7, 10);
            VerifyCSharpDiagnostic(test, expected);
        }

        [Fact]
        public void Advice_Must_Be_Public()
        {
            var test =
@"using AspectInjector.Broker;
namespace TestNameSpace
{
    [Aspect(Aspect.Scope.Global)]
    class TypeClass
    {
        [Advice(Advice.Type.Before, Advice.Target.Method)]
        private void Before(){}
    }
}";

            var expected = DiagnosticResult.From(Rules.AdviceMustHaveValidSingnature, 7, 10);
            VerifyCSharpDiagnostic(test, expected);
        }

        [Fact]
        public void Advice_Must_Not_Be_Generic()
        {
            var test =
@"using AspectInjector.Broker;
namespace TestNameSpace
{
    [Aspect(Aspect.Scope.Global)]
    class TypeClass
    {
        [Advice(Advice.Type.Before, Advice.Target.Method)]
        public void Before<T>(){}
    }
}";

            var expected = DiagnosticResult.From(Rules.AdviceMustHaveValidSingnature, 7, 10);
            VerifyCSharpDiagnostic(test, expected);
        }

        [Fact]
        public void Advice_Inline_Must_Be_Void()
        {
            var test =
@"using AspectInjector.Broker;
namespace TestNameSpace
{
    [Aspect(Aspect.Scope.Global)]
    class TypeClass
    {
        [Advice(Advice.Type.Before, Advice.Target.Method)]
        public int Before(){ return 1; }
    }
}";

            var expected = DiagnosticResult.From(Rules.AdviceMustHaveValidSingnature, 7, 10);
            VerifyCSharpDiagnostic(test, expected);
        }

        [Fact]
        public void Advice_Around_Must_Be_Object()
        {
            var test =
@"using AspectInjector.Broker;
namespace TestNameSpace
{
    [Aspect(Aspect.Scope.Global)]
    class TypeClass
    {
        [Advice(Advice.Type.Around, Advice.Target.Method)]
        public void Around(){ }
    }
}";

            var expected = DiagnosticResult.From(Rules.AdviceMustHaveValidSingnature, 7, 10);
            VerifyCSharpDiagnostic(test, expected);
        }

        [Fact]
        public void Advice_Argument_Must_Be_Bound()
        {
            var test =
@"using AspectInjector.Broker;
namespace TestNameSpace
{
    [Aspect(Aspect.Scope.Global)]
    class TypeClass
    {
        [Advice(Advice.Type.Before, Advice.Target.Method)]
        public void Before(string data, int val, [Advice.Argument(Advice.Argument.Source.Instance)] object i){ }
    }
}";

            var expected = new[]{
                DiagnosticResult.From(Rules.AdviceArgumentMustBeBound, 8, 35),
                DiagnosticResult.From(Rules.AdviceArgumentMustBeBound, 8, 45),
            };
            VerifyCSharpDiagnostic(test, expected);
        }

        [Fact]
        public void Advice_Argument_Must_Be_Bound_Valid()
        {
            var test =
@"using AspectInjector.Broker;
namespace TestNameSpace
{
    [Aspect(Aspect.Scope.Global)]
    class TypeClass
    {
        [Advice(Advice.Type.Before, Advice.Target.Method)]
        public void Before([Advice.Argument(Advice.Argument.Source.Instance)] object i){ }
    }
}";

            VerifyCSharpDiagnostic(test);
        }

        [Fact]
        public void Advice_Should_Be_Part_Of_Aspect()
        {
            var test =
@"using AspectInjector.Broker;
namespace TestNameSpace
{
    class TypeClass
    {   
        [Advice(Advice.Type.Before, Advice.Target.Method)]
        public void Before(){}
    }
}";
            var expected = DiagnosticResult.From(Rules.EffectMustBePartOfAspect, 6, 10);
            VerifyCSharpDiagnostic(test, expected);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new AdviceAttributeAnalyzer();
        }
    }
}
