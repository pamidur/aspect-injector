using AspectInjector.Core.Extensions;
using AspectInjector.Core.Models;
using Mono.Cecil;

namespace AspectInjector.Core.InterfaceProxy
{
    public class InterfaceAdvice : Advice
    {
        public TypeReference InterfaceType { get; set; }

        protected override bool IsApplicableForAspect(Aspect aspect)
        {
            return aspect is Aspect<TypeDefinition>;
        }

        protected override bool IsEqualTo(Advice advice)
        {
            var other = (InterfaceAdvice)advice;
            return other.InterfaceType.GetFQN() == InterfaceType.GetFQN();
        }
    }
}