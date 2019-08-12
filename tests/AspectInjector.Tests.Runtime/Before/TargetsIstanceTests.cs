using AspectInjector.Tests.Assets;

namespace AspectInjector.Tests.Runtime.Before
{
    public class Instance_Tests : Global_Tests
    {
        protected override string Token => InstanceAspect.BeforeExecuted;
    }
}