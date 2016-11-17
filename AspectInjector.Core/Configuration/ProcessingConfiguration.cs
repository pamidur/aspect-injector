using AspectInjector.Core.Contexts;
using AspectInjector.Core.Contracts;
using AspectInjector.Core.Defaults;
using AspectInjector.Core.Processing;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AspectInjector.Core.Configuration
{
    public class ProcessingConfiguration
    {
        public ILogger Log { get; private set; }

        public string Prefix { get; private set; }

        public Type AspectExtractor { get; private set; }

        public Type ModuleProcessor { get; private set; }

        public Type AdviceCacheProvider { get; private set; }

        public IReadOnlyCollection<Type> Extractors { get; private set; } = new List<Type>();

        public IReadOnlyCollection<Type> Injectors { get; private set; } = new List<Type>();

        public ProcessingConfiguration RegisterAdviceExtractor<T>()
            where T : class, IAdviceExtractor
        {
            ((List<Type>)Extractors).Add(typeof(T));
            return this;
        }

        public ProcessingConfiguration RegisterInjector<T>()
            where T : class, IInjector
        {
            ((List<Type>)Injectors).Add(typeof(T));
            return this;
        }

        public ProcessingConfiguration SetAdviceCacheProvider<T>() where T : class, IAdviceCacheProvider
        {
            AdviceCacheProvider = typeof(T);
            return this;
        }

        public ProcessingConfiguration SetModuleProcessor<T>() where T : class, IAssemblyProcessor
        {
            ModuleProcessor = typeof(T);
            return this;
        }

        public ProcessingConfiguration SetAspectExtractor<T>() where T : class, IAspectExtractor
        {
            AspectExtractor = typeof(T);
            return this;
        }

        public ProcessingConfiguration SetLogger(ILogger logger)
        {
            Log = logger;
            return this;
        }

        public ProcessingConfiguration SetPrefix(string prefix)
        {
            Prefix = prefix;
            return this;
        }

        internal ProcessingContext CreateContext(AssemblyDefinition assembly, IAssemblyResolver resolver)
        {
            Validate();

            var context = new ProcessingContext
            {
                Assembly = assembly,
                Resolver = resolver,
                Services = new ServicesContext
                {
                    Log = Log,
                    Prefix = Prefix,
                    AdviceCacheProvider = (IAdviceCacheProvider)Activator.CreateInstance(AdviceCacheProvider),
                    AspectExtractor = (IAspectExtractor)Activator.CreateInstance(AspectExtractor),
                    AssemblyProcessor = (IAssemblyProcessor)Activator.CreateInstance(ModuleProcessor),
                    AdviceExtractors = Extractors.Select(e => (IAdviceExtractor)Activator.CreateInstance(e)).ToList(),
                    Injectors = Injectors.Select(i => (IInjector)Activator.CreateInstance(i)).ToList(),
                }
            };

            context.Services.AdviceCacheProvider.Init(context);
            context.Services.AspectExtractor.Init(context);
            context.Services.AssemblyProcessor.Init(context);
            context.Services.AdviceExtractors.ToList().ForEach(e => e.Init(context));
            context.Services.Injectors.ToList().ForEach(i => i.Init(context));

            return context;
        }

        public void Validate()
        {
            if (Log == null)
                throw new Exception("Log should be set.");

            if (AdviceCacheProvider == null)
                throw new Exception("AdviceProvider should be set.");

            if (AspectExtractor == null)
                throw new Exception("AspectExtractor should be set.");

            if (ModuleProcessor == null)
                throw new Exception("AspectExtractor should be set.");

            if (string.IsNullOrWhiteSpace(Prefix))
                throw new Exception("Prefix should be set.");
        }

        public static ProcessingConfiguration Default { get; private set; }

        static ProcessingConfiguration()
        {
            Default = new ProcessingConfiguration()
            .SetPrefix("__a$")
            .SetLogger(new ConsoleLogger())
            .SetAdviceCacheProvider<EmbeddedResourceAdviceProvider>()
            .SetModuleProcessor<AssemblyProcessor>()
            .SetAspectExtractor<AspectExtractor>();
        }
    }
}