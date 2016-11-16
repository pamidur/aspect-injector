using AspectInjector.Core.Contracts;
using System.Collections.Generic;

namespace AspectInjector.Core.Contexts
{
    public class ServicesContext
    {
        public IAdviceCacheProvider AdviceCacheProvider { get; internal set; }

        public IModuleProcessor ModuleProcessor { get; internal set; }

        public ILogger Log { get; internal set; }

        public string Prefix { get; internal set; }

        public IAspectExtractor AspectExtractor { get; internal set; }

        public IEnumerable<IAdviceExtractor> AdviceExtractors { get; internal set; }

        public IEnumerable<IInjector> Injectors { get; set; }
    }
}