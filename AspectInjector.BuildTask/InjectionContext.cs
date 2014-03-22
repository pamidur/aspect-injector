using System.Collections.Generic;
using System.Linq;
using AspectInjector.Broker;
using Mono.Cecil;

namespace AspectInjector.BuildTask
{
    internal class InjectionContext
    {
        public TypeDefinition AspectType { get; set; }
        public string TargetName { get; set; }
        public MethodDefinition TargetMethod { get; set; }
        public MethodDefinition AdviceMethod { get; set; }
        public List<AdviceArgumentSource> AdviceArgumentsSources { get; set; }
        public InjectionPoints InjectionPoint { get; set; }

        public bool IsAbortable 
        {
            get { return AdviceArgumentsSources != null && AdviceArgumentsSources.Any(s => s == AdviceArgumentSource.AbortFlag); }
        }

        public InjectionContext()
        {
        }

        public InjectionContext(InjectionContext other)
        {
            AspectType = other.AspectType;
            TargetName = other.TargetName;
            TargetMethod = other.TargetMethod;
            AdviceMethod = other.AdviceMethod;
            AdviceArgumentsSources = other.AdviceArgumentsSources;
            InjectionPoint = other.InjectionPoint;
        }
    }
}
