using AspectInjector.Broker;
using System;
using System.Threading.Tasks;

namespace AspectInjector.GenericTests
{
    [Aspect(Scope.Global)]
    [Injection(typeof(TestAspect))]
    public class TestAspect : Attribute
    {
        [Advice(Kind.Around)]
        public object Around(
            [Argument(Source.Target)] Func<object[],object> target,
            [Argument(Source.Instance)] object obj,
            [Argument(Source.Arguments)] object[] args
            )
        {
            return null;
        }

        [Advice(Kind.After)]
        public void After(
            [Argument(Source.Instance)] object obj,
            [Argument(Source.Arguments)] object[] args

            )
        {

        }
    }

    public class Wrapper<TWrapper>
    {       

        public class Class1<TType>
        {
            [TestAspect]
            private async Task<Func<TMethod, TType, TWrapper>> Do<TMethod>(TMethod tmethod, TType type, Func<TMethod, TType> func)
            {
                await Task.Delay(0);
                return default;
            }

            [TestAspect]
            private TType Do1<TMethod>(TMethod tmethod, TType type, Func<TMethod, TType> func)
            {
                return default;
            }

            [TestAspect]
            private TMethod Do2<TMethod>(TMethod tmethod, TType type, Func<TMethod, TType> func)
            {
                return default;
            }

            [TestAspect]
            private TWrapper Do3<TMethod>(TWrapper wrapper, TMethod tmethod, TType type, Func<TMethod, TType> func)
            {
                return default;
            }

            [TestAspect]
            private TWrapper Do4<TMethod>(TWrapper wrapper, TMethod tmethod, TType type, Func<TMethod, TType> func)
            {
                return default;
            }
        }
    }
}
