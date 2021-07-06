using System;
using AspectInjector.Broker;
using Aspects.Universal.Aspects;

namespace Aspects.Universal.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true)]
    [Injection(typeof(PublicWrapperAspect), Inherited = true)]
    public abstract class PublicAspectAttribute : BaseMethodPointsAspectAttribute
    {
    }
}