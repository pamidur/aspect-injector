using AspectInjector.Core.Contracts;
using System.Collections.Generic;

namespace AspectInjector.Core.Contexts
{
    public class ServicesContext
    {
        public IInjectionCacheProvider InjectionCacheProvider { get; internal set; }

        public IAssemblyProcessor AssemblyProcessor { get; internal set; }

        public ILogger Log { get; internal set; }

        public string Prefix { get; internal set; }

        public IAspectReader AspectReader { get; internal set; }

        public IEnumerable<IInjectionReader> InjectionReaders { get; internal set; }

        public IEnumerable<IInjector> Injectors { get; set; }
    }
}