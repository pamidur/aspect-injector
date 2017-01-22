using AspectInjector.Broker;
using AspectInjector.Core.Extensions;
using AspectInjector.Core.Models;
using Mono.Cecil;
using System.Collections.Generic;
using System.Linq;

namespace AspectInjector.Core.Services
{
    public class AspectExtractor : ServiceBase
    {
        private readonly IEnumerable<EffectExtractorBase> _effectExtractors;

        public AspectExtractor(IEnumerable<EffectExtractorBase> effectExtractors, Logger logger) : base(logger)
        {
            _effectExtractors = effectExtractors;
        }

        public IEnumerable<AspectDefinition> Extract(AssemblyDefinition assembly)
        {
            return Validate(assembly.Modules.SelectMany(m => m.GetTypes().Select(ReadAspect).Where(a => a != null)));
        }

        public AspectDefinition ReadAspect(TypeDefinition type)
        {
            var effects = ExtractEffects(type).ToList();
            var aspect = ExtractAspectAttribute(type);

            if (effects.Any())
            {
                if (aspect == null)
                    Log.LogError(CompilationMessage.From($"Type {type.FullName} has the effects, but is not marked as an aspect. Concider using [Aspect] attribute.", type));
                else
                    return new AspectDefinition
                    {
                        Host = type,
                        Scope = aspect.GetConstructorValue<Aspect.Scope>(0),
                        Factory = aspect.GetPropertyValue<Aspect>(au => au.Factory) as TypeReference,
                        Effects = effects
                    };
            }

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

            effects.Concat(ExtractEffectsFromProvider(type));
            effects.Concat(type.Methods.SelectMany(ExtractEffectsFromProvider));

            effects.Concat(type.NestedTypes.SelectMany(ExtractEffects));

            return effects;
        }

        private IEnumerable<Effect> ExtractEffectsFromProvider(ICustomAttributeProvider host)
        {
            var effects = Enumerable.Empty<Effect>();

            foreach (var extractor in _effectExtractors)
                effects.Concat(extractor.Extract(host));

            return effects;
        }

        private IEnumerable<AspectDefinition> Validate(IEnumerable<AspectDefinition> result)
        {
            foreach (var aspect in result)
            {
                if (!aspect.Effects.Any())
                    Log.LogWarning(CompilationMessage.From($"Type {aspect.Host.FullName} has defined as an aspect, but lacks any effect.", aspect.Host));

                if (!aspect.Host.IsPublic)
                    Log.LogWarning(CompilationMessage.From($"Type {aspect.Host.FullName} is not public.", aspect.Host));

                yield return aspect;
            }
        }
    }
}