using AspectInjector.Tests.Assets;

namespace AspectInjector.Tests.Runtime.After
{
    public class Instance_Tests : Global_Tests
    {
        protected override string Token => InstanceAspect.AfterExecuted;
    }
}