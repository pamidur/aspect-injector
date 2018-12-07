using AspectInjector.Broker;
using Xunit;
using System;
using System.IO;

namespace AspectInjector.Tests.Interfaces
{
    public class GenericInterfacesTests
    {
        [Fact]
        public void Interfaces_OpenGenericMethod()
        {
            var ti = (ITestInterface)new GenericMethodTestClass();
            var r1 = ti.Get<string>("data");

            Assert.Equal("datawashere", r1);

            var r2 = ti.Get<int>(1);

            Assert.Equal(2, r2);
        }

        [MyAspect]
        public class GenericMethodTestClass
        {
            public GenericMethodTestClass()
            {
            }
        }

        public interface ITestInterface
        {
            TO Get<TO>(TO data);
        }

        [Mixin(typeof(ITestInterface))]
        [Aspect(Scope.Global)]
        [Injection(typeof(MyAspect))]
        public class MyAspect : Attribute, ITestInterface
        {
            TO ITestInterface.Get<TO>(TO data)
            {
                if (data is string)
                    return (TO)(object)(data.ToString() + "washere");

                return (TO)(object)(((int)(object)data) + 1);
            }
        }
    }


    public class GenericInterfacesTests3
    {
        [Fact]
        public void Interfaces_OpenGenericMethodInClosedGenericType()
        {
            var data4 = "ref";

            var ti = (IfaceWrapClass<string>.ITestInterface<StreamReader>)new OpenGenericTestClass();
            var r1 = ti.Get<string>("data", "123", new StreamReader(new MemoryStream()), ref data4);

            Assert.Equal("data123washereref", r1);

            var r2 = ti.Get<int>(1, "123", new StreamReader(new MemoryStream()), ref data4);

            Assert.Equal("5", r2);
        }

        [MyAspect]
        public class OpenGenericTestClass
        {
            public OpenGenericTestClass()
            {
            }
        }

        public class IfaceWrapClass<TH>
            where TH : class
        {
            public interface ITestInterface<T1> : ITestInterface2<TH, T1>
                where T1 : TextReader
            {
            }

            public interface ITestInterface2<T1, T2>
                where T1 : TH
            {
                T1 Get<TO>(TO data, T1 data2, T2 data3, ref string data4);
            }
        }

        [Mixin(typeof(IfaceWrapClass<string>.ITestInterface<StreamReader>))]
        [Aspect(Scope.Global)]
        [Injection(typeof(MyAspect))]
        public class MyAspect : Attribute, IfaceWrapClass<string>.ITestInterface<StreamReader>
        {
            public string Get<TO>(TO data, string data2, StreamReader data3, ref string data4)
            {
                if (data is string)
                    return (data.ToString() + data2 + "washere" + data4);

                return (((int)(object)data) + 1 + data2.Length).ToString();
            }
        }
    }
}