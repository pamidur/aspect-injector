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

        public Type AspectReader { get; private set; }

        public Type AssemblyProcessor { get; private set; }

        public Type InjectionCacheProvider { get; private set; }

        public IReadOnlyCollection<Type> InjectionReaders { get; private set; } = new List<Type>();

        public IReadOnlyCollection<Type> Injectors { get; private set; } = new List<Type>();

        public ProcessingConfiguration RegisterInjectionReader<T>()
            where T : class, IAspectReader
        {
            ((List<Type>)InjectionReaders).Add(typeof(T));
            return this;
        }

        public ProcessingConfiguration RegisterInjector<T>()
            where T : class, IInjector
        {
            ((List<Type>)Injectors).Add(typeof(T));
            return this;
        }

        public ProcessingConfiguration SetInjectionCacheProvider<T>() where T : class, IAspectCacheProvider
        {
            InjectionCacheProvider = typeof(T);
            return this;
        }

        public ProcessingConfiguration SetAssemblyProcessor<T>() where T : class, IAssemblyProcessor
        {
            AssemblyProcessor = typeof(T);
            return this;
        }

        public ProcessingConfiguration SetAspectReader<T>() where T : class, IInjectionReader
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
                    InjectionCacheProvider = (IAspectCacheProvider)Activator.CreateInstance(InjectionCacheProvider),
                    AspectReader = (IInjectionReader)Activator.CreateInstance(AspectReader),
                    AssemblyProcessor = (IAssemblyProcessor)Activator.CreateInstance(AssemblyProcessor),
                    InjectionReaders = InjectionReaders.Select(e => (IAspectReader)Activator.CreateInstance(e)).ToList(),
                    Injectors = Injectors.Select(i => (IInjector)Activator.CreateInstance(i)).ToList(),
                }
            };

            context.Services.InjectionCacheProvider.Init(context);
            context.Services.AspectReader.Init(context);
            context.Services.AssemblyProcessor.Init(context);
            context.Services.InjectionReaders.ToList().ForEach(e => e.Init(context));
            context.Services.Injectors.ToList().ForEach(i => i.Init(context));

            return context;
        }

        public void Validate()
        {
            if (Log == null)
                throw new Exception("Log should be set.");

            if (InjectionCacheProvider == null)
                throw new Exception("InjectionCacheProvider should be set.");

            if (AspectReader == null)
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
            .SetInjectionCacheProvider<EmbeddedResourceInjectionProvider>()
            .SetAssemblyProcessor<AssemblyProcessor>()
            .SetAspectReader<AspectReader>();
        }
    }
}