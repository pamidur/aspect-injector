using AspectInjector.Core.Contexts;
using AspectInjector.Core.Contracts;
using AspectInjector.Core.Models;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AspectInjector.Core.Processing
{
    public class AspectReader : IAspectReader
    {
        private ProcessingContext _context;

        public void Init(ProcessingContext context)
        {
            _context = context;
        }

        public IEnumerable<AspectDefinition> Read(ModuleDefinition module)
        {
            return Validate(module.Types.SelectMany(ReadAspects));
        }

        public IEnumerable<AspectDefinition> ReadAspects(TypeDefinition type)
        {
            var result = Enumerable.Empty<AspectDefinition>();

            foreach (var reader in _context.Services.EffectReaders)
            {
                var injections = reader.ReadEffects(module);
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