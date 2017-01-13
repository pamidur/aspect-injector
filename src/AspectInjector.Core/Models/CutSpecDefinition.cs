using Mono.Cecil;

namespace AspectInjector.Core.Models
{
    public class CutSpecDefinition : CutDefinition
    {
        public new TypeReference Target
        {
            get { return (TypeReference)base.Target; }
            set { base.Target = value.Resolve(); }
        }

        public override bool IsSpec { get; } = true;
    }
}