using AspectInjector.Core.Extensions;
using AspectInjector.Core.Models;
using Mono.Cecil;

namespace AspectInjector.Core.InterfaceProxy
{
    internal class InterfaceAdvice : Advice
    {
        public TypeReference InterfaceType { get; set; }

        protected override bool IsApplicableForAspect(Aspect aspect)
        {
            return aspect.TargetType == AspectTargetType.TypeDefinition;
        }

        protected override bool IsEqualTo(Advice advice)
        {
            var other = (InterfaceAdvice)advice;
            return other.InterfaceType.GetFQN() == InterfaceType.GetFQN();
        }
    }
}