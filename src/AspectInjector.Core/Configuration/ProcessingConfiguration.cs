using AspectInjector.Core.Contexts;
using AspectInjector.Core.Contracts;
using AspectInjector.Core.Defaults;
using AspectInjector.Core.Fluent;
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

        public Type InjectionCollector { get; private set; }

        public Type AspectReader { get; private set; }

        public Type AssemblyProcessor { get; private set; }

        public Type AspectCache { get; private set; }

        public IReadOnlyCollection<Type> EffectReaders { get; private set; } = new List<Type>();

        public IReadOnlyCollection<Type> Weavers { get; private set; } = new List<Type>();

        public ProcessingConfiguration RegisterEffectReader<T>()
            where T : class, IAspectReader
        {
            ((List<Type>)EffectReaders).Add(typeof(T));
            return this;
        }

        public ProcessingConfiguration RegisterWeaver<T>()
            where T : class, IWeaver
        {
            ((List<Type>)Weavers).Add(typeof(T));
            return this;
        }

        public ProcessingConfiguration SetAspectCache<T>() where T : class, IAspectCache
        {
            AspectCache = typeof(T);
            return this;
        }

        public ProcessingConfiguration SetAssemblyProcessor<T>() where T : class, IAssemblyProcessor
        {
            AssemblyProcessor = typeof(T);
            return this;
        }

        public ProcessingConfiguration SetInjectionCollector<T>() where T : class, IInjectionCollector
        {
            InjectionCollector = typeof(T);
            return this;
        }

        public ProcessingConfiguration SetAspectReader<T>() where T : class, IInjectionCollector
        {
            AspectReader = typeof(T);
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
                Editors = new EditorFactory(Prefix),
                Services = new ServicesContext
                {
                    Log = Log,
                    Prefix = Prefix,
                    AspectCache = (IAspectCache)Activator.CreateInstance(AspectCache),
                    InjectionCollector = (IInjectionCollector)Activator.CreateInstance(InjectionCollector),
                    AspectReader = (IAspectReader)Activator.CreateInstance(AspectReader),
                    AssemblyProcessor = (IAssemblyProcessor)Activator.CreateInstance(AssemblyProcessor),
                    EffectReaders = EffectReaders.Select(e => (IEffectReader)Activator.CreateInstance(e)).ToList(),
                    Weavers = Weavers.Select(i => (IWeaver)Activator.CreateInstance(i)).ToList(),
                }
            };

            context.Services.AspectCache.Init(context);
            context.Services.InjectionCollector.Init(context);
            context.Services.AssemblyProcessor.Init(context);
            context.Services.EffectReaders.ToList().ForEach(e => e.Init(context));
            context.Services.Weavers.ToList().ForEach(i => i.Init(context));

            return context;
        }

        public void Validate()
        {
            if (Log == null)
                throw new Exception("Log should be set.");

            if (AspectCache == null)
                throw new Exception("InjectionCacheProvider should be set.");

            if (InjectionCollector == null)
                throw new Exception("AspectReader should be set.");

            if (AssemblyProcessor == null)
                throw new Exception("AssemblyProcessor should be set.");

            if (string.IsNullOrWhiteSpace(Prefix))
                throw new Exception("Prefix should be set.");
        }

        public static ProcessingConfiguration Default { get; private set; }

        static ProcessingConfiguration()
        {
            Default = new ProcessingConfiguration()
            .SetPrefix("__a$_")
            .SetLogger(new TraceLogger())
            .SetAspectCache<EmbeddedResourceAspectCache>()
            .SetAssemblyProcessor<AssemblyProcessor>()
            .SetAspectReader<InjectionCollector>()
            .SetInjectionCollector<InjectionCollector>();
        }
    }
}