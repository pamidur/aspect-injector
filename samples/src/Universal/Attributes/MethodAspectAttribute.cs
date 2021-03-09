using System;
using AspectInjector.Broker;
using Aspects.Universal.Aspects;

namespace Aspects.Universal.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true)]
    [Injection(typeof(MethodWrapperAspect))]
    public abstract class MethodAspectAttribute : BaseUniversalWrapperAttribute
    {
    }
}