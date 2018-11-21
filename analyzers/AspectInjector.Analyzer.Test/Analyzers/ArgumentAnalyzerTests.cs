using AspectInjector.Analyzer.Analyzers;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using Xunit;

namespace AspectInjector.Analyzer.Test.Analyzers
{
    public class ArgumentAnalyzerTests : CodeFixVerifier
    {
        [Fact]
        public void NoCode_NoDiagnostics()
        {
            var test = @"";
            VerifyCSharpDiagnostic(test);
        }      

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

//        [Fact]
//        public void Argument_Target_Should_Be_Null_For_()
//        {
//            var test =
//@"using AspectInjector.Broker;
//namespace TestNameSpace
//{
//    [Aspect(Aspect.Scope.Global)]
//    class TypeClass
//    {
//        public static void Before([Advice.Argument(Advice.Argument.Source.Instance)] object i){}
//    }
//}";
//            var expected = DiagnosticResult.From(Rules.ArgumentMustBePartOfAdvice, 7, 36);
//            VerifyCSharpDiagnostic(test, expected);
//        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new ArgumentAttributeAnalyzer();
        }
    }
}
