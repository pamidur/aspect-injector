using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using TestHelper;
using AspectInjector.Analyzer;
using AspectInjector.Analyzer.Mixin;
using Xunit;

namespace AspectInjector.Analyzer.Test
{
    public class UnitTest : CodeFixVerifier
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
                Id = Rules.CanMixinOnlyInterfaces.Id,
                Message = string.Format((string)Rules.CanMixinOnlyInterfaces.MessageFormat, "DummyClass"),
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

        //protected override CodeFixProvider GetCSharpCodeFixProvider()
        //{
        //    return new AspectInjectorAnalyzerCodeFixProvider();
        //}

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new MixinAttributeAnalyzer();
        }
    }
}
