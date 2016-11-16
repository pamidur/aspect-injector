using AspectInjector.Core.Contracts;
using AspectInjector.Core.Models;
using Mono.Cecil;

namespace AspectInjector.Core.Advices.Interface
{
    public class InterfaceAdvice : IAdvice
    {
        public TypeDefinition InterfaceType { get; set; }

        public TypeReference HostType { get; set; }

        public bool IsApplicableFor(Aspect aspect)
        {
            return aspect is Aspect<TypeDefinition>;
        }
    }
}