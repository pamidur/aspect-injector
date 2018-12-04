using AspectInjector.Analyzer.Analyzers;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using Xunit;

namespace AspectInjector.Analyzer.Test.Analyzers
{
    public class MixinAnalyzerTests : CorrectDefinitionsTests
    {
        [Fact]
        public void Can_Mixin_Only_Interfaces()
        {
            var test = 
@"using AspectInjector.Broker;
namespace TestNameSpace
{
    [Mixin(typeof(DummyClass))]
    [Aspect(Scope.Global)]
    class TypeClass
    {   
    }

    class DummyClass{}
}";

            var expected = DiagnosticResult.From(Rules.MixinSupportsOnlyInterfaces, 4, 6);
            VerifyCSharpDiagnostic(test, expected);
        }

        [Fact]
        public void Mixin_Should_Implement_Interface()
        {
            var test = 
@"using AspectInjector.Broker;
namespace TestNameSpace
{
    [Mixin(typeof(IDummyInterface))]
    [Aspect(Scope.Global)]
    class TypeClass
    {   
    }

    interface IDummyInterface{}
}";

            var expected = DiagnosticResult.From(Rules.MixinSupportsOnlyAspectInterfaces, 4, 6);
            VerifyCSharpDiagnostic(test, expected);
        }

        [Fact]
        public void Mixin_Should_Implement_Interface_With_BaseClass()
        {
            var test = 
@"using AspectInjector.Broker;
namespace TestNameSpace
{
    [Mixin(typeof(IDummyInterface))]
    [Mixin(typeof(IDummyInterface2))]
    [Aspect(Scope.Global)]
    class TypeClass : object
    {   
    }

    interface IDummyInterface{}
    interface IDummyInterface2{}
}";
            var expected = new[] {
                DiagnosticResult.From(Rules.MixinSupportsOnlyAspectInterfaces, 4, 6),
                DiagnosticResult.From(Rules.MixinSupportsOnlyAspectInterfaces, 5, 6),
            };

            VerifyCSharpDiagnostic(test, expected);
        }


        [Fact]
        public void Mixin_Should_Be_Part_Of_Aspect()
        {
            var test = 
@"using AspectInjector.Broker;
namespace TestNameSpace
{
    [Mixin(typeof(IDummyInterface))]
    class TypeClass : IDummyInterface
    {   
    }

    interface IDummyInterface{}
}";
            var expected = DiagnosticResult.From(Rules.EffectMustBePartOfAspect, 4, 6);
            VerifyCSharpDiagnostic(test, expected);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new MixinAttributeAnalyzer();
        }
    }
}
