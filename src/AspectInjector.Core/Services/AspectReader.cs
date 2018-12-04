using AspectInjector.Broker;
using AspectInjector.Core.Contracts;
using AspectInjector.Core.Extensions;
using AspectInjector.Core.Models;
using Mono.Cecil;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace AspectInjector.Core.Services
{
    public class AspectReader : IAspectReader
    {
        private static readonly ConcurrentDictionary<TypeDefinition, AspectDefinition> _cache = new ConcurrentDictionary<TypeDefinition, AspectDefinition>();

        private readonly IEnumerable<IEffectReader> _effectExtractors;
        private readonly ILogger _log;

        public AspectReader(IEnumerable<IEffectReader> effectExtractors, ILogger logger)
        {
            _effectExtractors = effectExtractors;
            _log = logger;
        }

        public IReadOnlyCollection<AspectDefinition> ReadAll(AssemblyDefinition assembly)
        {
            return assembly.Modules.SelectMany(m => m.GetTypes().Select(Read).Where(a => a != null)).Where(a => a.Validate(_log)).ToList();
        }

        public AspectDefinition Read(TypeDefinition type)
        {
            if (!_cache.TryGetValue(type, out var aspectDef))
            {
                var effects = ExtractEffects(type).ToList();
                var aspect = ExtractAspectAttribute(type);

                if (aspect != null)                
                    aspectDef = new AspectDefinition
                    {
                        Host = type,
                        Scope = aspect.GetConstructorValue<Scope>(0),
                        Factory = aspect.GetPropertyValue<TypeReference>(nameof(Aspect.Factory)),
                        Effects = effects
                    };                
                else if (effects.Any())
                    _log.LogWarning(CompilationMessage.From($"Type {type.FullName} has effects, but is not marked as an aspect. Concider using [Aspect] attribute.", type));

                _cache.AddOrUpdate(type, aspectDef, (k, o) => aspectDef);
            }

            return aspectDef;
        }

        private CustomAttribute ExtractAspectAttribute(TypeDefinition type)
        {
            var aspectUsage = type.CustomAttributes.FirstOrDefault(ca => ca.AttributeType.FullName == WellKnownTypes.Aspect);
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
                effects.AddRange(extractor.Read(host));

            return effects;
        }
    }
}