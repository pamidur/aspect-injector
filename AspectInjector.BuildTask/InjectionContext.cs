using AspectInjector.Broker;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AspectInjector.BuildTask
{
    internal class InjectionContext : IComparable<InjectionContext>
    {
        public TypeDefinition AspectType { get; set; }

        public string TargetName { get; set; }

        public object[] AspectCustomData { get; set; }

        public TargetMethodContext TargetMethodContext { get; set; }

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
            TargetMethodContext = other.TargetMethodContext;
            AdviceMethod = other.AdviceMethod;
            AdviceArgumentsSources = other.AdviceArgumentsSources;
            InjectionPoint = other.InjectionPoint;
        }

        public int CompareTo(InjectionContext other)
        {
            if (object.Equals(TargetMethodContext, other.TargetMethodContext))
            {
                if (IsAbortable) return -1;
                if (other.IsAbortable) return 1;
                return 0;
            }
            return TargetMethodContext.TargetMethod.FullName.CompareTo(other.TargetMethodContext.TargetMethod.FullName);
        }
    }
}