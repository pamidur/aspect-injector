using AspectInjector.Broker;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            [Incut(typeof(TestAspectImplementation))]
            public object Do2(object obj, ref object objRef, out object objOut, int value, ref int valueRef, out int valueOut, ref long longRef, ref double doubleRef, ref char charRef)
            {
                objOut = new object();
                valueOut = 1;

                return new object();
            }

            [Incut(typeof(TestAspectImplementation))]
            public static object Do1(object obj, ref object objRef, out object objOut, int value, ref int valueRef, out int valueOut, ref long longRef, ref double doubleRef, ref char charRef)
            {
                objOut = new object();
                valueOut = 1;

                return new object();
            }
        }

        public class TestAspectImplementation
        {
            [Advice(Advice.Type.Around, Advice.Target.Method)]
            public object AroundMethod([AdviceArgument(AdviceArgument.Source.Target)] Func<object[], object> target,
                [AdviceArgument(AdviceArgument.Source.Arguments)] object[] arguments)
            {
                return new object();
            }
        }
    }
}