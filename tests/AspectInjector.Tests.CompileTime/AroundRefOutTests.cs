using AspectInjector.Broker;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace AspectInjector.CompileTimeTests
{
    [TestClass]
    public class AroundRefOutTests : CompileTimeTestRunner
    {
        [TestMethod]
        public void Can_Pack_And_Unpack_Ref_And_Out_Into_Array()
        {
            PE_Integrity_Is_Ok();
        }

        public class TestClass
        {
            [Inject(typeof(TestAspectImplementation))]
            public object Do2(object obj, ref object objRef, out object objOut, int value, ref int valueRef, out int valueOut, ref long longRef, ref double doubleRef, ref char charRef)
            {
                objOut = new object();
                valueOut = 1;

                return new object();
            }

            [Inject(typeof(TestAspectImplementation))]
            public static object Do1(object obj, ref object objRef, out object objOut, int value, ref int valueRef, out int valueOut, ref long longRef, ref double doubleRef, ref char charRef)
            {
                objOut = new object();
                valueOut = 1;

                return new object();
            }
        }

        [Aspect(Aspect.Scope.Global)]
        public class TestAspectImplementation
        {
            [Advice(Advice.Type.Around, Advice.Target.Method)]
            public object AroundMethod([Advice.Argument(Advice.Argument.Source.Target)] Func<object[], object> target,
                [Advice.Argument(Advice.Argument.Source.Arguments)] object[] arguments)
            {
                return new object();
            }
        }
    }
}