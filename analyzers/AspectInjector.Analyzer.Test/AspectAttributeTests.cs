using AspectInjector.Analyzer.Analyzers;
using AspectInjector.Analyzer.CodeFixes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using Xunit;

namespace AspectInjector.Analyzer.Test
{
    public class AspectAttributeTests : CodeFixVerifier
    {
        [Fact]
        public void NoCode_NoDiagnostics()
        {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }


        [Fact]
        public void Aspect_Must_Not_Be_Static()
        {
            var test =
@"using AspectInjector.Broker;
    namespace TestNameSpace
    {
            [Aspect(Aspect.Scope.Global)]
            static class TypeClass
            {
        [Advice(Advice.Type.Before, Advice.Target.Method)]
        public void Before(){}
            }
    }";
            var expected = DiagnosticResult.From(Rules.AspectMustHaveValidSignature, 4, 14);
            VerifyCSharpDiagnostic(test, expected);

            var fixtest =
@"using AspectInjector.Broker;
    namespace TestNameSpace
    {
            [Aspect(Aspect.Scope.Global)]
            class TypeClass
            {
        [Advice(Advice.Type.Before, Advice.Target.Method)]
        public void Before(){}
            }
    }";
            VerifyCSharpFix(test, fixtest);
        }

        [Fact]
        public void Aspect_Must_Not_Be_Abstract()
        {
            var test =
@"using AspectInjector.Broker;
    namespace TestNameSpace
    {
            [Aspect(Aspect.Scope.Global)]
            abstract class TypeClass<T>
            {
        [Advice(Advice.Type.Before, Advice.Target.Method)]
        public void Before(){}
            }
    }";

            var fixtest =
@"using AspectInjector.Broker;
    namespace TestNameSpace
    {
            [Aspect(Aspect.Scope.Global)]
            class TypeClass
            {
        [Advice(Advice.Type.Before, Advice.Target.Method)]
        public void Before(){}
            }
    }";
            VerifyCSharpFix(test, fixtest);
        }

        [Fact]
        public void Aspect_Must_Not_Be_Generic()
        {
            var test =
@"using AspectInjector.Broker;
namespace TestNameSpace
{
    [Aspect(Aspect.Scope.Global)]
    class TypeClass<T>
    {
        [Advice(Advice.Type.Before, Advice.Target.Method)]
        public void Before(){}
    }
}";

            var expected = DiagnosticResult.From(Rules.AspectMustHaveValidSignature, 4, 6);
            VerifyCSharpDiagnostic(test, expected);
        }

        [Fact]
        public void Aspect_Must_Have_Parameterless_Ctor()
        {
            var test =
@"using AspectInjector.Broker;
namespace TestNameSpace
{
    [Aspect(Aspect.Scope.Global)]
    class TypeClass
    {
        public TypeClass(string value){}

        [Advice(Advice.Type.Before, Advice.Target.Method)]
        public void Before(){}
    }
}";

            var expected = DiagnosticResult.From(Rules.AspectMustHaveContructorOrFactory, 4, 6);
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
    [Aspect(Aspect.Scope.Global, Factory = typeof(FakeFactory))]
    class TypeClass
    {
        public TypeClass(string value){}

        [Advice(Advice.Type.Before, Advice.Target.Method)]
        public void Before(){}
    }

    class FakeFactory {
        public object GetInstance(Type type){
            return null;
        }
    }
}";

            var expected = DiagnosticResult.From(Rules.AspectFactoryMustContainFactoryMethod, 5, 6);
            VerifyCSharpDiagnostic(test, expected);
        }

        [Fact]
        public void Aspect_Factory_Must_Have_Factory_Method_Valid()
        {
            var test =
@"using AspectInjector.Broker;
using System;
namespace TestNameSpace
{
    [Aspect(Aspect.Scope.Global, Factory = typeof(FakeFactory))]
    class TypeClass
    {
        public TypeClass(string value){}

        [Advice(Advice.Type.Before, Advice.Target.Method)]
        public void Before(){}
    }

    class FakeFactory {
        public static object GetInstance(Type type){
            return null;
        }
    }
}";            
            VerifyCSharpDiagnostic(test);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new AspectCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new AspectAttributeAnalyzer();
        }
    }
}
