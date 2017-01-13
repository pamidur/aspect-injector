using AspectInjector.Core.Contracts;
using AspectInjector.Core.Defaults;
using AspectInjector.Core.Processing;
using System;
using System.Collections.Generic;

namespace AspectInjector.Core
{
    public class Configuration : IConfiguration
    {
        public static Configuration Default { get; private set; }
            = new Configuration("__a$_")
            .SetLogger(new TraceLogger())
            .SetCache<EmbeddedResourceAspectCache>()
            .SetAssemblyProcessor<AssemblyProcessor>()
            .SetAspectReader<InjectionCollector>()
            .SetInjectionCollector<InjectionCollector>()
            ;

        public Configuration(string prefix)
        {
            _prefix = prefix;
        }

        private readonly List<object> _services = new List<object>();

        private bool _sealed = false;
        private readonly string _prefix;

        private void CheckSealed()
        {
            if (_sealed)
                throw new Exception("This configuration is validated and sealed. Consider create new one.");
        }

        private T EnsureServiceInit<T>(T service) where T : class
        {
            if (service is IInitializable)
            {
                ((IInitializable)service).Init(this);
            }
            return service;
        }

        string IConfiguration.Prefix
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        T IConfiguration.GetService<T>()
        {
            throw new NotImplementedException();
        }

        IEnumerable<T> IConfiguration.GetServices<T>()
        {
            throw new NotImplementedException();
        }
    }
}