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
            }
    }";
            var expected = new DiagnosticResult
            {
                Id = Rules.AspectMustNotBeStatic.Id,
                Message = string.Format((string)Rules.AspectMustNotBeStatic.MessageFormat, "TypeClass"),
                Severity = DiagnosticSeverity.Error,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 4, 14)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest =
@"using AspectInjector.Broker;
    namespace TestNameSpace
    {
            [Aspect(Aspect.Scope.Global)]
            class TypeClass
            {
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
            abstract class TypeClass
            {
            }
    }";
            var expected = new DiagnosticResult
            {
                Id = Rules.AspectMustNotBeAbstract.Id,
                Message = string.Format((string)Rules.AspectMustNotBeAbstract.MessageFormat, "TypeClass"),
                Severity = DiagnosticSeverity.Error,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 4, 14)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest =
@"using AspectInjector.Broker;
    namespace TestNameSpace
    {
            [Aspect(Aspect.Scope.Global)]
            class TypeClass
            {
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
    }
}";
            var expected = new DiagnosticResult
            {
                Id = Rules.AspectMustNotBeGeneric.Id,
                Message = string.Format((string)Rules.AspectMustNotBeGeneric.MessageFormat, "TypeClass"),
                Severity = DiagnosticSeverity.Error,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 4, 6)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);

//            var fixtest =
//@"using AspectInjector.Broker;
//    namespace TestNameSpace
//    {
//            [Aspect(Aspect.Scope.Global)]
//            class TypeClass
//            {
//            }
//    }";
//            VerifyCSharpFix(test, fixtest);
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
    }
}";
            var expected = new DiagnosticResult
            {
                Id = Rules.AspectMustHaveContructorOrFactory.Id,
                Message = string.Format((string)Rules.AspectMustHaveContructorOrFactory.MessageFormat, "TypeClass"),
                Severity = DiagnosticSeverity.Error,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 4, 6)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);

            //            var fixtest =
            //@"using AspectInjector.Broker;
            //    namespace TestNameSpace
            //    {
            //            [Aspect(Aspect.Scope.Global)]
            //            class TypeClass
            //            {
            //            }
            //    }";
            //            VerifyCSharpFix(test, fixtest);
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
    }

    class FakeFactory {
        public object GetInstance(Type type){
            return null;
        }
    }
}";
            var expected = new DiagnosticResult
            {
                Id = Rules.AspectFactoryMustContainFactoryMethod.Id,
                Message = string.Format((string)Rules.AspectFactoryMustContainFactoryMethod.MessageFormat, "TypeClass"),
                Severity = DiagnosticSeverity.Error,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 5, 6)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);

            //            var fixtest =
            //@"using AspectInjector.Broker;
            //    namespace TestNameSpace
            //    {
            //            [Aspect(Aspect.Scope.Global)]
            //            class TypeClass
            //            {
            //            }
            //    }";
            //            VerifyCSharpFix(test, fixtest);
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
