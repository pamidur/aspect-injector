using AspectInjector.Broker;
using AspectInjector.Tests.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspectInjector.Tests.Runtime.Mixins
{
    [Aspect(Scope.Global)]
    [Mixin(typeof(IAssetIface1Wrapper<Asset2>.IAssetIface1<Asset1>))]
    [Injection(typeof(AttrTest))]
    public class AttrTest : Attribute, IAssetIface1Wrapper<Asset2>.IAssetIface1<Asset1>
    {
        Tuple<Asset2, Asset1> IAssetIface1Wrapper<Asset2>.IAssetIface1<Asset1>.TestProperty { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        event EventHandler<Tuple<Asset2, Asset1>> IAssetIface1Wrapper<Asset2>.IAssetIface1<Asset1>.TestEvent
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        void IAssetIface1Wrapper<Asset2>.IAssetIface1<Asset1>.EmptyMethod()
        {
            throw new NotImplementedException();
        }

        Tuple<int, Asset1, Asset2, Asset1, T3> IAssetIface1Wrapper<Asset2>.IAssetIface1<Asset1>.TestMethod<T3>(int a1, Asset1 a2, Asset2 a3, Asset1 a4, T3 a5, ref int ar1, ref Asset1 ar2, ref Asset2 ar3, ref Asset1 ar4, ref T3 ar5, out int ao1, out Asset1 ao2, out Asset2 ao3, out Asset1 ao4, out T3 ao5)
        {
            throw new NotImplementedException();
        }
    }



    [AttrTest]
    class TestClass3
    {

    }

    //internal class TestClassWrapper<T1>
    //{
    //    [InjectInstanceAspect]
    //    [InjectGlobalAspect]
    //    private class TestClass<T2>
    //    {

    //    }
    //}
}