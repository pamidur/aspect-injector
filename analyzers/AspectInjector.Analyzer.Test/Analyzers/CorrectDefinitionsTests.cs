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
    [Aspect(Aspect.Scope.Global, Factory = typeof(FakeFactory))]
    class TypeClass : ITestInterface
    {
        public TypeClass(string value){}

        [Advice(Advice.Kind.Before)]
        public void Before(
            [Advice.Argument(Advice.Argument.Source.Instance)] object i,
            [Advice.Argument(Advice.Argument.Source.Type)] Type t,
            [Advice.Argument(Advice.Argument.Source.Method)] System.Reflection.MethodBase m,            
            [Advice.Argument(Advice.Argument.Source.Name)] string n,
            [Advice.Argument(Advice.Argument.Source.Arguments)] object[] a,
            [Advice.Argument(Advice.Argument.Source.ReturnType)] Type rt            
        ){}

        [Advice(Advice.Kind.After)]
        public void After(
            [Advice.Argument(Advice.Argument.Source.Instance)] object i,
            [Advice.Argument(Advice.Argument.Source.Type)] Type t,
            [Advice.Argument(Advice.Argument.Source.Method)] System.Reflection.MethodBase m,            
            [Advice.Argument(Advice.Argument.Source.Name)] string n,
            [Advice.Argument(Advice.Argument.Source.Arguments)] object[] a,
            [Advice.Argument(Advice.Argument.Source.ReturnType)] Type rt,            
            [Advice.Argument(Advice.Argument.Source.ReturnValue)] object rv            
        ){}

        [Advice(Advice.Kind.Around)]
        public object Around(
            [Advice.Argument(Advice.Argument.Source.Instance)] object i,
            [Advice.Argument(Advice.Argument.Source.Type)] Type t,
            [Advice.Argument(Advice.Argument.Source.Method)] System.Reflection.MethodBase m,            
            [Advice.Argument(Advice.Argument.Source.Name)] string n,
            [Advice.Argument(Advice.Argument.Source.Arguments)] object[] a,
            [Advice.Argument(Advice.Argument.Source.ReturnType)] Type rt,            
            [Advice.Argument(Advice.Argument.Source.Target)] Func<object[],object> target          
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
