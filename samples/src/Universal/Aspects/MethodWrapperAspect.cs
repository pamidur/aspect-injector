using System;
using System.Reflection;
using AspectInjector.Broker;

namespace Aspects.Universal.Aspects
{
    [Aspect(Scope.Global)]
    public class MethodWrapperAspect : BaseUniversalWrapperAspect
    {
        [Advice(Kind.Around, Targets = Target.Method)]
        public object Handle(
            [Argument(Source.Instance)] object? instance,
            [Argument(Source.Type)] Type type,
            [Argument(Source.Metadata)] MethodBase method,
            [Argument(Source.Target)] Func<object[], object> target,
            [Argument(Source.Name)] string name,
            [Argument(Source.Arguments)] object[] args,
            [Argument(Source.ReturnType)] Type returnType,
            [Argument(Source.Triggers)] Attribute[] triggers)
        {
            return BaseHandle(instance, type, method, target, name, args, returnType, triggers);
        }
    }
}
