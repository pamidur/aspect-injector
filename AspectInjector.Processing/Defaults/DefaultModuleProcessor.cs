using AspectInjector.Core.Contexts;
using AspectInjector.Core.Contracts;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            foreach (var extrator in _context.Services.Extractors)
            {
                var advices = extrator.ExtractAdvices(module);
            }

            foreach (var injector in _context.Services.Injectors)
            {
                //injector.Apply()
            }
        }

        public void ProcessModule<T>(ModuleDefinition module) where T : IAdvice
        {
            //extract advices
            //cache advices
            // find matching advices
            //inject

            //foreach (var adviceType in _config.AdviceTypes)
            //{
            //    var method = _processModuleMethod.MakeGenericMethod(adviceType.AdviceType);
            //    foreach (var module in context.Assembly.Modules)
            //        method.Invoke(context.Services.ModuleProcessor, new[] { module });
            //}
        }
    }
}