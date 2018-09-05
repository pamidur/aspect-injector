using AspectInjector.Core.Contracts;
using AspectInjector.Core.Extensions;
using AspectInjector.Core.Models;
using Mono.Cecil;
using System.Collections.Generic;
using System.Linq;

namespace AspectInjector.Core.Mixin
{
    public class MixinExtractor : IEffectExtractor
    {
        public IReadOnlyCollection<Effect> Extract(ICustomAttributeProvider host)
        {
            var source = host as TypeDefinition;

            if (source != null)
                return Extract(source);

            return new List<Effect>();
        }

        private IReadOnlyCollection<MixinEffect> Extract(TypeDefinition type)
        {
            var mixins = new List<MixinEffect>();

            foreach (var ca in type.CustomAttributes.ToList())
            {
                if (ca.AttributeType.FullName == WellKnownTypes.Mixin)
                {
                    type.CustomAttributes.Remove(ca);
                    mixins.Add(new MixinEffect { InterfaceType = ca.GetConstructorValue<TypeReference>(0) });
                }
            }

            return mixins;
        }
    }
}