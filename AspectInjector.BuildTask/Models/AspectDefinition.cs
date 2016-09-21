using System.Collections.Generic;
using AspectInjector.Broker;
using AspectInjector.BuildTask.Extensions;
using Mono.Cecil;

namespace AspectInjector.BuildTask.Models
{
    internal class AspectDefinition
    {
        public TypeDefinition AdviceClassType { get; }

        public string NameFilter { get; }

        public AccessModifiers AccessModifierFilter { get; }

        public IEnumerable<CustomAttribute> RoutableData { get; }

        public AspectDefinition(CustomAttribute attr, CustomAttribute baseAttr)
        {
            AdviceClassType = ((TypeReference)attr.ConstructorArguments[0].Value).Resolve();
            NameFilter = (string)attr.GetPropertyValue("NameFilter");
            AccessModifierFilter = (AccessModifiers)(attr.GetPropertyValue("AccessModifierFilter") ?? 0);
            RoutableData = baseAttr == null ? new List<CustomAttribute>() : new List<CustomAttribute> { baseAttr };
        }
    }
}