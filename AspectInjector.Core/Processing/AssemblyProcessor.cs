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
                    var injections = extrator.ReadInjections(module);
                    _context.Services.InjectionCacheProvider.StoreInjections(module, injections);
                }

                if (Log.IsErrorThrown)
                    throw new Exception("Compilation error occurred.");
            }

            foreach (var injector in _context.Services.Injectors.OrderByDescending(i => i.Priority))
            {
                Log.LogInformation($"Executing {injector.GetType().Name}");

                foreach (var aspect in aspects.OrderByDescending(a => a.Priority))
                {
                    var matchednjections = _context.Services.InjectionCacheProvider.GetInjections(aspect.InjectionHost).Where(i => i.IsApplicableFor(aspect)).ToList();

                    foreach (var injection in matchednjections.OrderByDescending(i => i.Priority))
                        if (injector.CanApply(injection))
                            injector.Apply(aspect, injection);
                }
            }
        }
    }
}