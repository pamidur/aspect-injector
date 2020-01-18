using AspectInjector.Tests.Assets;
using System;

namespace AspectInjector.Tests.Runtime
{
    class ControlWrapper<T1>
    {
        class Control<T2> : IAssetIface1Wrapper<Asset2>.IAssetIface1<Asset1>, IAssetIface1Wrapper<Asset1>.IAssetIface1<Asset2>
        {
            Tuple<Asset2, Asset1> IAssetIface1Wrapper<Asset2>.IAssetIface1<Asset1>.TestProperty { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            Tuple<Asset1, Asset2> IAssetIface1Wrapper<Asset1>.IAssetIface1<Asset2>.TestProperty { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

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

            event EventHandler<Tuple<Asset1, Asset2>> IAssetIface1Wrapper<Asset1>.IAssetIface1<Asset2>.TestEvent
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

            void IAssetIface1Wrapper<Asset1>.IAssetIface1<Asset2>.EmptyMethod()
            {
                throw new NotImplementedException();
            }

            Tuple<int, Asset1, Asset2, Asset1, T3> IAssetIface1Wrapper<Asset2>.IAssetIface1<Asset1>.TestMethod<T3>(int a1, Asset1 a2, Asset2 a3, Asset1 a4, T3 a5, ref int ar1, ref Asset1 ar2, ref Asset2 ar3, ref Asset1 ar4, ref T3 ar5, out int ao1, out Asset1 ao2, out Asset2 ao3, out Asset1 ao4, out T3 ao5)
            {
                throw new NotImplementedException();
            }

            Tuple<int, Asset1, Asset1, Asset2, T3> IAssetIface1Wrapper<Asset1>.IAssetIface1<Asset2>.TestMethod<T3>(int a1, Asset1 a2, Asset1 a3, Asset2 a4, T3 a5, ref int ar1, ref Asset1 ar2, ref Asset1 ar3, ref Asset2 ar4, ref T3 ar5, out int ao1, out Asset1 ao2, out Asset1 ao3, out Asset2 ao4, out T3 ao5)
            {
                throw new NotImplementedException();
            }
        }
    }
}
