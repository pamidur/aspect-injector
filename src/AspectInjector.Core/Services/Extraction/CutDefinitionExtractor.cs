using AspectInjector.Core.Contracts;
using AspectInjector.Core.Models;
using AspectInjector.Core.Processing;
using AspectInjector.Core.Services;
using AspectInjector.Core.Services.Abstract;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AspectInjector.Core.Extraction
{
    public class CutDefinitionExtractor : ServiceBase
    {
        private readonly Cache _cache;

        public CutDefinitionExtractor(Logger logger, Cache cache) : base(logger)
        {
            _cache = cache;
        }

        private IEnumerable<CutDefinition> ExtractCuts(ICustomAttributeProvider target);

        public IEnumerable<AspectDefinition> Extract(ModuleDefinition module)
        {
            return Validate(module.Types.SelectMany(ReadAspects));
        }

        public IEnumerable<AspectDefinition> ReadAspects(TypeDefinition type)
        {
            var result = Enumerable.Empty<AspectDefinition>();

            foreach (var reader in _context.Services.EffectReaders)
            {
                var injections = reader.ReadEffects(type);
            }

            result = result.Concat(type.NestedTypes.SelectMany(ReadAspects));
            return result;
        }

        private IEnumerable<AspectDefinition> Validate(IEnumerable<AspectDefinition> result)
        {
            throw new NotImplementedException();
        }

        public void Cleanup(ModuleDefinition module)
        {
            foreach (var reader in _context.Services.EffectReaders)
                reader.Cleanup(module);
        }
    }
}