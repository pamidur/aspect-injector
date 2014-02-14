using System;

namespace AspectInjector.Test.Aspects
{
    internal class TestMethodAspect
    {
        public TestMethodAspect()
        {
            Console.WriteLine("SampleMethodAspect constructor");
        }

        [Advice(Points = InjectionPoint.After)]
        public void After([AdviceArgument(Source = AdviceArgumentSource.Instance)] object targetInstance,
            [AdviceArgument(Source = AdviceArgumentSource.TargetName)] object targetName)
        {
            Console.WriteLine("SampleMethodAspect.After() called on {0}.{1}", targetInstance, targetName);
        }

        [Advice(Points = InjectionPoint.Before)]
        public void Before([AdviceArgument(Source = AdviceArgumentSource.Instance)] object targetInstance,
            [AdviceArgument(Source = AdviceArgumentSource.TargetName)] object targetName)
        {
            Console.WriteLine("SampleMethodAspect.Before() called on {0}.{1}", targetInstance, targetName);
        }
    }
}