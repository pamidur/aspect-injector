using System;
using AspectInjector.Broker;
using Aspects.Universal.Aspects;

namespace Aspects.Universal.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true)]
    [Injection(typeof(PublicMethodWrapperAspect), Inherited = true)]
    public abstract class PublicMethodAspectAttribute: BaseMethodPointsAspectAttribute
    {
    }
}