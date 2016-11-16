using AspectInjector.Core.Advices.Interface;
using AspectInjector.Core.Advices.MethodCall;
using AspectInjector.Core.Contexts;
using AspectInjector.Core.Contracts;
using AspectInjector.Core.Defaults;
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
        public IReadOnlyCollection<AdviceTypesConfiguration> AdviceTypes { get; private set; } = new List<AdviceTypesConfiguration>();

        public AdviceTypesConfiguration<T> RegisterAdvice<T>()
            where T : class, IAdvice
        {
            var advice = new AdviceTypesConfiguration<T>(this);
            ((List<AdviceTypesConfiguration>)AdviceTypes).Add(advice);
            return advice;
        }

        public ProcessingConfiguration SetAdviceCacheProvider<T>() where T : class, IAdviceCacheProvider
        {
            AdviceCacheProvider = typeof(T);
            return this;
        }

        public ProcessingConfiguration SetModuleProcessor<T>() where T : class, IModuleProcessor
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
                Resolver = resolver
            };

            //foreach (var adviceType in AdviceTypes)
            //{
            //    var extractor = (IAdviceExtractor<IAdvice>)Activator.CreateInstance(adviceType.Extractor);
            //    adviceType.Type
            //}

            context.Services = new ServicesContext
            {
                Log = Log,
                Prefix = Prefix,
                AdviceCacheProvider = (IAdviceCacheProvider)Activator.CreateInstance(AdviceCacheProvider),
                AspectExtractor = (IAspectExtractor)Activator.CreateInstance(AspectExtractor),
                ModuleProcessor = (IModuleProcessor)Activator.CreateInstance(ModuleProcessor),
                // Advices = compiledAdvices
            };

            context.Services.AdviceCacheProvider.Init(context);
            context.Services.AspectExtractor.Init(context);
            context.Services.ModuleProcessor.Init(context);

            //init everything

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

            foreach (var adviceType in AdviceTypes)
            {
                if (adviceType.Extractor == null)
                    throw new Exception($"Extractor should be set for {adviceType.Type.Name}.");

                if (!adviceType.Injectors.Any())
                    throw new Exception($"Advice {adviceType.Type.Name} should have at least one injector.");
            }
        }

        public static ProcessingConfiguration Default { get; private set; }

        static ProcessingConfiguration()
        {
            Default = new ProcessingConfiguration()
            .SetPrefix("__a$")
            .SetLogger(new ConsoleLogger())
            .SetAdviceCacheProvider<EmbeddedResourceAdviceProvider>()
            .SetModuleProcessor<DefaultModuleProcessor>()
            .SetAspectExtractor<DefaultAspectExtractor>()
            .RegisterAdvice<MethodCallAdvice>()
                .SetExtractor<MethodCallAdviceExtractor>()
                .AddInjector<MethodCallInjector>()
                .Done()
            .RegisterAdvice<InterfaceAdvice>()
                .SetExtractor<InterfaceAdviceExtractor>()
                .AddInjector<InterfaceInjector>()
                .Done();
        }
    }
}