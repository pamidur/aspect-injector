using AspectInjector.Core.Contracts;
using AspectInjector.Core.Defaults;
using AspectInjector.Core.Models;
using Mono.Cecil;
using System.Linq;

namespace AspectInjector.Core.Processing
{
    public class AssemblyProcessor : ServiceBase, IAssemblyProcessor
    {
        public AssemblyProcessor()
        {
        }

        public void ProcessAssembly(AssemblyDefinition assembly)
        {
            var cuts = Enumerable.Empty<CutDefinition>();
            var aspects = Enumerable.Empty<AspectDefinition>();

            foreach (var module in assembly.Modules)
            {
                cuts = cuts.Concat(Context.Services.)
            }
        }

        public void ProcessAssembly1(AssemblyDefinition assembly)
        {
            var injections = _context.Services.InjectionCollector.Collect(assembly).ToList();

            foreach (var module in assembly.Modules)
            {
                var aspects = _context.Services.AspectReader.Extract(module);
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