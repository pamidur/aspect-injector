using Mono.Cecil;
using System.Collections.Generic;
using System.Linq;

namespace AspectInjector.Core.Services
{
    public class AssemblyProcessor : ServiceBase
    {
        private readonly AspectExtractor _aspectExtractor;
        private readonly AspectWeaver _assectWeaver;
        private readonly AssetsCache _cache;
        private readonly IEnumerable<EffectWeaverBase> _effectWeavers;
        private readonly InjectionCollector _injectionCollector;
        private readonly Janitor _janitor;

        public AssemblyProcessor(Janitor janitor, AspectExtractor aspectExtractor, AssetsCache cache, InjectionCollector injectionCollector, AspectWeaver assectWeaver, IEnumerable<EffectWeaverBase> effectWeavers, Logger logger) : base(logger)
        {
            _aspectExtractor = aspectExtractor;
            _injectionCollector = injectionCollector;
            _cache = cache;
            _assectWeaver = assectWeaver;
            _effectWeavers = effectWeavers;
            _janitor = janitor;
        }

        public void ProcessAssembly(AssemblyDefinition assembly)
        {
            var aspects = _aspectExtractor.Extract(assembly);

            _cache.Cache(aspects);

            var injections = _injectionCollector.Collect(assembly);

            _janitor.CleanupAssembly(assembly);

            if (Log.IsErrorThrown)
                return;

            _cache.FlushCache(assembly);

            //todo:: inject singletons into aspects

            foreach (var injector in _effectWeavers.OrderByDescending(i => i.Priority))
            {
                Log.LogInformation($"Executing {injector.GetType().Name}");

                foreach (var prioritizedInjections in injections.GroupBy(i => i.Priority).OrderByDescending(a => a.Key))
                {
                    foreach (var injection in prioritizedInjections.OrderByDescending(i => i.Effect.Priority))
                        if (injector.CanWeave(injection))
                            injector.Weave(injection);
                }
            }
        }
    }
}