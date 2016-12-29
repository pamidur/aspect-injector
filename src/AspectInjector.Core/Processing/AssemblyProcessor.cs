using AspectInjector.Core.Contexts;
using AspectInjector.Core.Contracts;
using Mono.Cecil;
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
            var injections = _context.Services.InjectionCollector.Collect(assembly).ToList();

            foreach (var module in assembly.Modules)
            {
                var aspects = _context.Services.AspectReader.Read(module);
                _context.Services.AspectCache.Cache(module, aspects);
            }

            if (Log.IsErrorThrown)
            {
                Log.LogError("Preprocessing assembly fails. Terminating compilation...");
                return;
            }

            foreach (var injector in _context.Services.Weavers.OrderByDescending(i => i.Priority))
            {
                Log.LogInformation($"Executing {injector.GetType().Name}");

                foreach (var injection in injections.OrderByDescending(a => a.Priority))
                {
                    var aspect = _context.Services.AspectCache.GetAspect(injection.Source);

                    var effects = aspect.Effects.Where(i => i.IsApplicableFor(injection)).ToList();

                    foreach (var effect in effects.OrderByDescending(i => i.Priority))
                        if (injector.CanApply(effect))
                            injector.Apply(injection, effect);
                }
            }
        }
    }
}