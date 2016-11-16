using AspectInjector.Core.Contexts;
using AspectInjector.Core.Contracts;
using Mono.Cecil;
using System.Linq;

namespace AspectInjector.Core.Defaults
{
    public class DefaultModuleProcessor : IModuleProcessor
    {
        private ProcessingContext _context;
        protected ILogger Log { get; private set; }

        public void Init(ProcessingContext context)
        {
            _context = context;
            Log = context.Services.Log;
        }

        public void ProcessModule(ModuleDefinition module)
        {
            var aspects = _context.Services.AspectExtractor.ExtractAspects(module);

            foreach (var extrator in _context.Services.AdviceExtractors)
            {
                var advices = extrator.ExtractAdvices(module);
                _context.Services.AdviceCacheProvider.StoreAdvices(module, advices);
            }

            foreach (var aspect in aspects)
            {
                var matchedAdvices = _context.Services.AdviceCacheProvider.GetAdvices(aspect.AdviceHost).Where(a => a.IsApplicableFor(aspect)).ToList();

                foreach (var injector in _context.Services.Injectors)
                    foreach (var advice in matchedAdvices)
                        if (injector.CanApply(advice))
                            injector.Apply(aspect, advice);
            }
        }
    }
}