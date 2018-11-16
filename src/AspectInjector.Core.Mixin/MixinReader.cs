using AspectInjector.Core.Contracts;
using AspectInjector.Core.Extensions;
using AspectInjector.Core.Models;
using Mono.Cecil;
using System.Collections.Generic;
using System.Linq;

namespace AspectInjector.Core.Mixin
{
    public class MixinReader : IEffectReader
    {
        public IReadOnlyCollection<Effect> Read(ICustomAttributeProvider host)
        {
            if (host is TypeDefinition source)
                return Extract(source);

            return new List<Effect>();
        }

        private IReadOnlyCollection<MixinEffect> Extract(TypeDefinition type)
        {
            var mixins = new List<MixinEffect>();

            foreach (var ca in type.CustomAttributes.ToList())
            {
                if (ca.AttributeType.FullName == WellKnownTypes.Mixin)
                    mixins.Add(new MixinEffect { InterfaceType = ca.GetConstructorValue<TypeReference>(0) });
            }

            return mixins;
        }
    }
}