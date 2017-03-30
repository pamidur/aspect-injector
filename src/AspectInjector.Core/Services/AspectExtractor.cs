using AspectInjector.Broker;
using AspectInjector.Core.Contracts;
using AspectInjector.Core.Extensions;
using AspectInjector.Core.Models;
using Mono.Cecil;
using System.Collections.Generic;
using System.Linq;

namespace AspectInjector.Core.Services
{
    public class AspectExtractor : IAspectExtractor
    {
        private readonly IEnumerable<IEffectExtractor> _effectExtractors;
        private readonly ILogger _log;

        public AspectExtractor(IEnumerable<IEffectExtractor> effectExtractors, ILogger logger)
        {
            _effectExtractors = effectExtractors;
            _log = logger;
        }

        public IReadOnlyCollection<AspectDefinition> Extract(AssemblyDefinition assembly)
        {
            return assembly.Modules.SelectMany(m => m.GetTypes().Select(ReadAspect).Where(a => a != null)).Where(a => a.Validate(_log)).ToList();
        }

        public AspectDefinition ReadAspect(TypeDefinition type)
        {
            var effects = ExtractEffects(type).ToList();
            var aspect = ExtractAspectAttribute(type);

            if (aspect != null)
                return new AspectDefinition
                {
                    Host = type,
                    Scope = aspect.GetConstructorValue<Aspect.Scope>(0),
                    Factory = aspect.GetPropertyValue<Aspect>(au => au.Factory) as TypeReference,
                    Effects = effects
                };

            if (effects.Any())
                _log.LogError(CompilationMessage.From($"Type {type.FullName} has effects, but is not marked as an aspect. Concider using [Aspect] attribute.", type));

            return null;
        }

        private CustomAttribute ExtractAspectAttribute(TypeDefinition type)
        {
            var aspectUsage = type.CustomAttributes.FirstOrDefault(ca => ca.AttributeType.IsTypeOf(typeof(Aspect)));

            if (aspectUsage != null)
                type.CustomAttributes.Remove(aspectUsage);

            return aspectUsage;
        }

        private IEnumerable<Effect> ExtractEffects(TypeDefinition type)
        {
            var effects = Enumerable.Empty<Effect>();

            effects = effects.Concat(ExtractEffectsFromProvider(type));
            effects = effects.Concat(type.Methods.SelectMany(ExtractEffectsFromProvider));

            return effects;
        }

        private List<Effect> ExtractEffectsFromProvider(ICustomAttributeProvider host)
        {
            var effects = new List<Effect>();

            foreach (var extractor in _effectExtractors)
                effects.AddRange(extractor.Extract(host));

            return effects;
        }
    }
}