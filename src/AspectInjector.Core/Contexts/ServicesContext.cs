using AspectInjector.Core.Contracts;
using System.Collections.Generic;

namespace AspectInjector.Core.Contexts
{
    public class ServicesContext
    {
        public IAspectCacheProvider InjectionCacheProvider { get; internal set; }

        public IAssemblyProcessor AssemblyProcessor { get; internal set; }

        public ILogger Log { get; internal set; }

        public string Prefix { get; internal set; }

        public IInjectionReader AspectReader { get; internal set; }

        public IEnumerable<IAspectReader> InjectionReaders { get; internal set; }

        public IEnumerable<IInjector> Injectors { get; set; }
    }
}