using System;
using AspectInjector.Broker;
using Aspects.Universal.Aspects;

namespace Aspects.Universal.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true)]
    [Injection(typeof(MethodWrapperAspect), Inherited = true)]
    public abstract class MethodAspectAttribute : BaseMethodPointsAspectAttribute
    {
    }
}