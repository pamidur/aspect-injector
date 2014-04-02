using AspectInjector.Broker;
using AspectInjector.BuildTask.Contracts;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AspectInjector.BuildTask.Contexts
{
    public class AdviceInjectionContext : IInjectionContext
    {
        public List<AdviceArgumentSource> AdviceArgumentsSources { get; set; }
        public MethodDefinition AdviceMethod { get; set; }
        public InjectionPoints InjectionPoint { get; set; }

        public AspectContext AspectContext { get; set; }

        public bool IsAbortable
        {
            get { return AdviceArgumentsSources != null && AdviceArgumentsSources.Any(s => s == AdviceArgumentSource.AbortFlag); }
        }
    }
}
