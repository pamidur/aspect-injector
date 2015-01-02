using AspectInjector.Broker;
using Mono.Cecil;

namespace AspectInjector.BuildTask.Contexts
{
    public class AspectInjectionContext
    {
        public AspectInjectionContext()
        {
        }

        public CustomAttributeArgument? AspectCustomData { get; set; }

        public MethodDefinition AspectFactory { get; set; }

        public TypeDefinition AspectType { get; set; }

        public AspectScope AspectScope { get; set; }

        public TargetMethodContext TargetMethodContext { get; set; }

        public string TargetName { get; set; }

        public TargetTypeContext TargetTypeContext { get; set; }
    }
}