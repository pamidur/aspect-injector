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
    }

    public interface IAssetIface1<T>
    {
    }
}