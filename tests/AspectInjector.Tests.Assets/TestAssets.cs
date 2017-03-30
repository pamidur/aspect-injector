using System;
using static AspectInjector.Tests.Assets.IAssetIface1Wrapper<AspectInjector.Tests.Assets.Asset1>;

namespace AspectInjector.Tests.Assets
{
    public class TestAssets
    {
        public static int asset1 = 1;
        public static Asset1 asset2 = new Asset1();
        public static short asset3 = 2;
        public static IAssetIface1<Asset1> asset4 = new Asset2();
        public static Asset2 asset5 = new Asset2();
    }

    public class Asset1
    {
    }

    public class Asset2 : IAssetIface1<Asset1>
    {
        public Tuple<Asset1, Asset1> TestProperty
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public event EventHandler<Tuple<Asset1, Asset1>> TestEvent;

        public void EmptyMethod()
        {
            throw new NotImplementedException();
        }

        public Tuple<int, Asset1, Asset1, Asset1, T3> TestMethod<T3>(int a1, Asset1 a2, Asset1 a3, Asset1 a4, T3 a5, ref int ar1, ref Asset1 ar2, ref Asset1 ar3, ref Asset1 ar4, ref T3 ar5, out int ao1, out Asset1 ao2, out Asset1 ao3, out Asset1 ao4, out T3 ao5)
        {
            throw new NotImplementedException();
        }
    }

    public class IAssetIface1Wrapper<T1>
    {
        public interface IAssetIface1<T2>
        {
            Tuple<int, Asset1, T1, T2, T3> TestMethod<T3>(
                int a1, Asset1 a2, T1 a3, T2 a4, T3 a5,
                ref int ar1, ref Asset1 ar2, ref T1 ar3, ref T2 ar4, ref T3 ar5,
                out int ao1, out Asset1 ao2, out T1 ao3, out T2 ao4, out T3 ao5
                );

            Tuple<T1, T2> TestProperty { get; set; }

            event EventHandler<Tuple<T1, T2>> TestEvent;

            void EmptyMethod();
        }
    }
}