using AspectInjector.Core.Contracts;
using System.Collections.Generic;

namespace AspectInjector.Core.Contexts
{
    public class ServicesContext
    {
        public IAspectCache AspectCache { get; internal set; }

        public IAssemblyProcessor AssemblyProcessor { get; internal set; }

        public ILogger Log { get; internal set; }

        public string Prefix { get; internal set; }

        public IInjectionCollector InjectionCollector { get; internal set; }

        public IAspectReader AspectReader { get; internal set; }

        public IEnumerable<IEffectReader> EffectReaders { get; internal set; }

        public IEnumerable<IWeaver> Weavers { get; set; }
    }
}