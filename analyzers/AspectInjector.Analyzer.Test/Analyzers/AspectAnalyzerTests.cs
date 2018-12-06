using AspectInjector.Analyzer.Analyzers;
using AspectInjector.Rules;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using Xunit;

namespace AspectInjector.Analyzer.Test.Analyzers
{
    public class AspectAnalyzerTests : CorrectDefinitionsTests
    {
        [Fact]
        public void Aspect_Must_Not_Be_Static()
        {
            var test =
@"using AspectInjector.Broker;
    namespace TestNameSpace
    {
            [Aspect(Scope.Global)]
            static class TypeClass
            {
        [Advice(Advice.Type.Before, Target.Method)]
        public void Before(){}
            }
    }";
            var expected = DiagnosticResult.From(AspectRules.AspectMustHaveValidSignature, 4, 14);
            VerifyCSharpDiagnostic(test, expected);
        }

        [Fact]
        public void Aspect_Must_Not_Be_Abstract()
        {
            var test =
@"using AspectInjector.Broker;
    namespace TestNameSpace
    {
            [Aspect(Scope.Global)]
            abstract class TypeClass
            {
        [Advice(Advice.Type.Before, Target.Method)]
        public void Before(){}
            }
    }";

            var expected = DiagnosticResult.From(AspectRules.AspectMustHaveValidSignature, 4, 14);
            VerifyCSharpDiagnostic(test, expected);
        }

        [Fact]
        public void Aspect_Must_Not_Be_Generic()
        {
            var test =
@"using AspectInjector.Broker;
namespace TestNameSpace
{
    [Aspect(Scope.Global)]
    class TypeClass<T>
    {
        [Advice(Advice.Type.Before, Target.Method)]
        public void Before(){}
    }
}";

            var expected = DiagnosticResult.From(AspectRules.AspectMustHaveValidSignature, 4, 6);
            VerifyCSharpDiagnostic(test, expected);
        }

        [Fact]
        public void Aspect_Must_Have_Parameterless_Ctor()
        {
            var test =
@"using AspectInjector.Broker;
namespace TestNameSpace
{
    [Aspect(Scope.Global)]
    class TypeClass
    {
        public TypeClass(string value){}

        [Advice(Advice.Type.Before, Target.Method)]
        public void Before(){}
    }
}";

            var expected = DiagnosticResult.From(AspectRules.AspectMustHaveContructorOrFactory, 4, 6);
            VerifyCSharpDiagnostic(test, expected);
        }

        [Fact]
        public void Aspect_Factory_Must_Have_Factory_Method()
        {
            var test =
@"using AspectInjector.Broker;
using System;
namespace TestNameSpace
{    
    [Aspect(Scope.Global, Factory = typeof(FakeFactory))]
    class TypeClass
    {
        public TypeClass(string value){}

        [Advice(Advice.Type.Before, Target.Method)]
        public void Before(){}
    }

    class FakeFactory {
        public object GetInstance(Type type){
            return null;
        }
    }
}";

            var expected = DiagnosticResult.From(AspectRules.AspectFactoryMustContainFactoryMethod, 5, 6);
            VerifyCSharpDiagnostic(test, expected);
        }

        [Fact]
        public void Aspect_Should_Have_Effects()
        {
            var test =
@"using AspectInjector.Broker;
namespace TestNameSpace
{
    [Aspect(Scope.Global)]
    class TypeClass
    {
    }
}";
            var expected = DiagnosticResult.From(AspectRules.AspectShouldContainEffect, 4, 6);
            VerifyCSharpDiagnostic(test, expected);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new AspectAttributeAnalyzer();
        }
    }
}
