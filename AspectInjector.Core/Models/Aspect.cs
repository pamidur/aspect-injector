using AspectInjector.Broker;
using Mono.Cecil;
using System.Collections.Generic;

namespace AspectInjector.Core.Models
{
    public class Aspect<TTarget> : Aspect
        where TTarget : class, ICustomAttributeProvider
    {
        public TTarget Target { get; set; }
    }

    public abstract class Aspect
    {
        public AspectTargetType TargetType { get; protected set; }

        public TypeReference AdviceHost { get; set; }

        public string NameFilter { get; set; }

        public AccessModifiers AccessModifierFilter { get; set; }

        public IEnumerable<CustomAttribute> RoutableData { get; set; }
    }
}