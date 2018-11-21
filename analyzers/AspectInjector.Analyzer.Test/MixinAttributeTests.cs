using AspectInjector.Analyzer.Analyzers;
using AspectInjector.Analyzer.CodeFixes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using Xunit;

namespace AspectInjector.Analyzer.Test
{
    public class MixinAnalyzerTests : CodeFixVerifier
    {
        [Fact]
        public void NoCode_NoDiagnostics()
        {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }

        [Fact]
        public void Can_Mixin_Only_Interfaces()
        {
            var test = @"  
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using AspectInjector.Broker;

    namespace TestNameSpace
    {
        [Mixin(typeof(DummyClass))]
        [Aspect(Aspect.Scope.Global)]
        class TypeClass
        {   
        }

        interface IDummyInterface{}
        class DummyClass{}
    }";
            var expected = new DiagnosticResult
            {
                Id = Rules.MixinSupportsOnlyInterfaces.Id,
                Message = string.Format((string)Rules.MixinSupportsOnlyInterfaces.MessageFormat, "DummyClass"),
                Severity = DiagnosticSeverity.Error,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 12, 10)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);

            //        var fixtest = @"
            //using System;
            //using System.Collections.Generic;
            //using System.Linq;
            //using System.Text;
            //using System.Threading.Tasks;
            //using System.Diagnostics;

            //namespace ConsoleApplication1
            //{
            //    class TYPENAME
            //    {   
            //    }
            //}";
            //        VerifyCSharpFix(test, fixtest);
        }

        [Fact]
        public void Mixin_Should_Implement_Interface()
        {
            var test = @"  
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using AspectInjector.Broker;

    namespace TestNameSpace
    {
        [Mixin(typeof(IDummyInterface))]
        [Aspect(Aspect.Scope.Global)]
        class TypeClass
        {   
        }

        interface IDummyInterface{}
        class DummyClass{}
    }";
            var expected = new DiagnosticResult
            {
                Id = Rules.MixinSupportsOnlyAspectInterfaces.Id,
                Message = string.Format((string)Rules.MixinSupportsOnlyAspectInterfaces.MessageFormat, "TypeClass", "IDummyInterface"),
                Severity = DiagnosticSeverity.Error,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 12, 10)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"  
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using AspectInjector.Broker;

    namespace TestNameSpace
    {
        [Mixin(typeof(IDummyInterface))]
        [Aspect(Aspect.Scope.Global)]
        class TypeClass : IDummyInterface
        {   
        }

        interface IDummyInterface{}
        class DummyClass{}
    }";
            VerifyCSharpFix(test, fixtest);
        }

        [Fact]
        public void Mixin_Should_Implement_Interface_With_BaseClass()
        {
            var test = @"  
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using AspectInjector.Broker;

    namespace TestNameSpace
    {
        [Mixin(typeof(IDummyInterface))]
        [Mixin(typeof(IDummyInterface2))]
        [Aspect(Aspect.Scope.Global)]
        class TypeClass : object
        {   
        }

        interface IDummyInterface{}
        interface IDummyInterface2{}
        class DummyClass{}
    }";
            var expected1 = new DiagnosticResult
            {
                Id = Rules.MixinSupportsOnlyAspectInterfaces.Id,
                Message = string.Format((string)Rules.MixinSupportsOnlyAspectInterfaces.MessageFormat, "TypeClass", "IDummyInterface"),
                Severity = DiagnosticSeverity.Error,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 12, 10),
                        }
            };

            var expected2 = new DiagnosticResult
            {
                Id = Rules.MixinSupportsOnlyAspectInterfaces.Id,
                Message = string.Format((string)Rules.MixinSupportsOnlyAspectInterfaces.MessageFormat, "TypeClass", "IDummyInterface2"),
                Severity = DiagnosticSeverity.Error,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 13, 10),
                        }
            };

            VerifyCSharpDiagnostic(test, expected1, expected2);

            var fixtest = @"  
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using AspectInjector.Broker;

    namespace TestNameSpace
    {
        [Mixin(typeof(IDummyInterface))]
        [Mixin(typeof(IDummyInterface2))]
        [Aspect(Aspect.Scope.Global)]
        class TypeClass : object, IDummyInterface, IDummyInterface2
        {   
        }

        interface IDummyInterface{}
        interface IDummyInterface2{}
        class DummyClass{}
    }";
            VerifyCSharpFix(test, fixtest);
        }


        [Fact]
        public void Mixin_Should_Be_Part_Of_Aspect()
        {
            var test = @"  
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using AspectInjector.Broker;

    namespace TestNameSpace
    {
        [Mixin(typeof(IDummyInterface))]
        class TypeClass : IDummyInterface
        {   
        }

        interface IDummyInterface{}
        class DummyClass{}
    }";
            var expected = new DiagnosticResult
            {
                Id = Rules.EffectMustBePartOfAspect.Id,
                Message = string.Format((string)Rules.EffectMustBePartOfAspect.MessageFormat, "TypeClass"),
                Severity = DiagnosticSeverity.Error,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 12, 10)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);

//            var fixtest = @"
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Diagnostics;
//using AspectInjector.Broker;

//    namespace TestNameSpace
//    {
//        [Mixin(typeof(IDummyInterface))]
//        [Aspect(Aspect.Scope.Global)]
//        class TypeClass : IDummyInterface
//        {   
//        }

//        interface IDummyInterface{}
//        class DummyClass{}
//    }";
//            VerifyCSharpFix(test, fixtest);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new MixinCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new MixinAttributeAnalyzer();
        }
    }
}
