using AspectInjector.Core.Contexts;
using AspectInjector.Core.Contracts;
using AspectInjector.Core.Processing.EqualityComparers;
using Mono.Cecil;
using System;
using System.Linq;

namespace AspectInjector.Core.Processing
{
    public class AssemblyProcessor : IAssemblyProcessor
    {
        private ProcessingContext _context;
        protected ILogger Log { get; private set; }

        public void Init(ProcessingContext context)
        {
            _context = context;
            Log = context.Services.Log;
        }

        public void ProcessAssembly(AssemblyDefinition assembly)
        {
            var aspects = _context.Services.AspectReader.ReadAspects(assembly).ToList();

            foreach (var module in assembly.Modules)
            {
                foreach (var extrator in _context.Services.InjectionReaders)
                {
                    var injections = extrator.ReadEffects(module);
                    _context.Services.InjectionCacheProvider.CacheEffects(module, injections);
                }
            }

            if (Log.IsErrorThrown)
            {
                Log.LogError("Preprocessing assembly fails. Terminating compilation...");
                return;
            }

            foreach (var injector in _context.Services.Injectors.OrderByDescending(i => i.Priority))
            {
                Log.LogInformation($"Executing {injector.GetType().Name}");

                foreach (var aspect in aspects.OrderByDescending(a => a.Priority))
                {
                    var matchednjections = _context.Services.InjectionCacheProvider.GetEffects(aspect.Aspect).Where(i => i.IsApplicableFor(aspect)).ToList();

                    foreach (var injection in matchednjections.OrderByDescending(i => i.Priority))
                        if (injector.CanApply(injection))
                            injector.Apply(aspect, injection);
                }
            }
        }
    }
}