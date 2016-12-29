using AspectInjector.Broker;
using Mono.Cecil;
using System.Collections.Generic;

namespace AspectInjector.BuildTask.Models
{
    internal class AspectDefinition
    {
        public TypeDefinition AdviceClassType { get; set; }

        public string NameFilter { get; set; }

        public AccessModifiers AccessModifierFilter { get; set; }

        public IEnumerable<CustomAttribute> RoutableData { get; set; }
    }
}