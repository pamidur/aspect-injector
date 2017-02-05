using AspectInjector.Core.Contracts;
using AspectInjector.Core.Extensions;
using AspectInjector.Core.Services;
using Mono.Cecil;
using System.Collections.Generic;
using System.Linq;

namespace AspectInjector.Core.Mixin
{
    public class MixinExtractor : EffectExtractorBase<TypeDefinition, MixinEffect>
    {
        public MixinExtractor(ILogger logger) : base(logger)
        {
        }

        protected override IReadOnlyCollection<MixinEffect> Extract(TypeDefinition type)
        {
            var mixins = new List<MixinEffect>();

            foreach (var ca in type.CustomAttributes.ToList())
            {
                if (ca.AttributeType.IsTypeOf(typeof(Broker.Mixin)))
                {
                    type.CustomAttributes.Remove(ca);
                    mixins.Add(new MixinEffect { InterfaceType = ca.GetConstructorValue<TypeReference>(0) });
                }
            }

            return mixins;
        }
    }
}