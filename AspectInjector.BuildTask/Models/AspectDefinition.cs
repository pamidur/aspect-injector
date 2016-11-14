using System.Collections.Generic;
using System.Text.RegularExpressions;
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

        public bool CanBeAppliedTo(MethodDefinition targetMethod, string targetName)
        {
            if (!string.IsNullOrEmpty(NameFilter) && !Regex.IsMatch(targetName, NameFilter))
                return false;
            
            if (AccessModifierFilter == 0)
                return true;

            if (targetMethod.IsPrivate)
                return (AccessModifierFilter & AccessModifiers.Private) != 0;

            if (targetMethod.IsFamily)
                return (AccessModifierFilter & AccessModifiers.Protected) != 0;

            if (targetMethod.IsAssembly)
                return (AccessModifierFilter & AccessModifiers.Internal) != 0;

            if (targetMethod.IsFamilyOrAssembly)
                return (AccessModifierFilter & AccessModifiers.ProtectedInternal) != 0;

            if (targetMethod.IsPublic)
                return (AccessModifierFilter & AccessModifiers.Public) != 0;

            return true;
        }
    }
}