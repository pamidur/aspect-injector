using AspectInjector.Broker;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace AspectInjector.GenericTests
{
    [Aspect(Scope.Global)]
    [Injection(typeof(TestAspect))]
    [Mixin(typeof(Wrapper2<int>.IInterface<int, int>))]
    [Mixin(typeof(INotifyPropertyChanged))]
    public class TestAspect : Attribute, Wrapper2<int>.IInterface<int, int>, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [Advice(Kind.Around)]
        public object Around(
            [Argument(Source.Target)] Func<object[], object> target,
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

        public Tuple<int, int, int, TMethod> Do<TMethod>(Tuple<int, int, int, TMethod> data)
        {
            throw new NotImplementedException();
        }
    }

    [TestAspect]
    class TestClass
    {

    }

    class Wrapper2<TWrapper2>
    {

        public interface IInterface<TInterface1, TInterface2>
        {
            Tuple<TWrapper2, TInterface1, TInterface2, TMethod> Do<TMethod>(Tuple<TWrapper2, TInterface1, TInterface2, TMethod> data);
        }
    }

    public class Wrapper<TWrapper>
    {

        public class Class1<TType> : Wrapper2<TWrapper>.IInterface<String, TWrapper>
        {
            public event PropertyChangedEventHandler PropertyChanged;

            public Tuple<TWrapper, string, TWrapper, TMethod> Do<TMethod>(Tuple<TWrapper, string, TWrapper, TMethod> data)
            {
                throw new NotImplementedException();
            }

            [TestAspect]
            private async Task<Func<TMethod, TType, TWrapper>> Do<TMethod>(TMethod tmethod, TType type, Func<TMethod, TType> func)
            {
                await Task.Delay(0);
                return default;
            }

            [TestAspect]
            private TType Do1<TMethod>(TMethod tmethod, TType type, Func<TMethod, TType> func, Tuple<TWrapper, TType, TMethod> t)
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
