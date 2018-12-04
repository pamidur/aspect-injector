using TestHelper;
using Xunit;

namespace AspectInjector.Analyzer.Test.Analyzers
{
    public abstract class CorrectDefinitionsTests : CodeFixVerifier
    {
        [Fact]
        public void No_Code_Doesnt_Throw_Analysis_Error()
        {
            var test = @"";
            VerifyCSharpDiagnostic(test);
        }

        [Fact]
        public void All_Valid_Doesnt_Throw_Analysis_Error()
        {
            var test =
@"using AspectInjector.Broker;
using System;
namespace TestNameSpace
{
    [Mixin(typeof(ITestInterface))]
    [Aspect(Scope.Global, Factory = typeof(FakeFactory))]
    class TypeClass : ITestInterface
    {
        public TypeClass(string value){}

        [Advice(Kind.Before)]
        public void Before(
            [Argument(Source.Instance)] object i,
            [Argument(Source.Type)] Type t,
            [Argument(Source.Method)] System.Reflection.MethodBase m,            
            [Argument(Source.Name)] string n,
            [Argument(Source.Arguments)] object[] a,
            [Argument(Source.ReturnType)] Type rt            
        ){}

        [Advice(Kind.After)]
        public void After(
            [Argument(Source.Instance)] object i,
            [Argument(Source.Type)] Type t,
            [Argument(Source.Method)] System.Reflection.MethodBase m,            
            [Argument(Source.Name)] string n,
            [Argument(Source.Arguments)] object[] a,
            [Argument(Source.ReturnType)] Type rt,            
            [Argument(Source.ReturnValue)] object rv            
        ){}

        [Advice(Kind.Around)]
        public object Around(
            [Argument(Source.Instance)] object i,
            [Argument(Source.Type)] Type t,
            [Argument(Source.Method)] System.Reflection.MethodBase m,            
            [Argument(Source.Name)] string n,
            [Argument(Source.Arguments)] object[] a,
            [Argument(Source.ReturnType)] Type rt,            
            [Argument(Source.Target)] Func<object[],object> target          
        ){ return null; }

    }

    interface ITestInterface {}

    class FakeFactory {
        public static object GetInstance(Type type){
            return null;
        }
    }
}";
            VerifyCSharpDiagnostic(test);
        }
    }
}
