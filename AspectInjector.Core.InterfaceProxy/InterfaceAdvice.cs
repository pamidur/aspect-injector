using AspectInjector.Core.Contracts;
using AspectInjector.Core.Models;
using Mono.Cecil;

namespace AspectInjector.Core.InterfaceProxy
{
    public class InterfaceAdvice : IAdvice
    {
        public TypeReference InterfaceType { get; set; }

        public TypeReference HostType { get; set; }

        public bool IsApplicableFor(Aspect aspect)
        {
            return aspect is Aspect<TypeDefinition>;
        }
    }
}