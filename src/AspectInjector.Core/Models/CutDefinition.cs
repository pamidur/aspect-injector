using Mono.Cecil;
using Newtonsoft.Json;
using static AspectInjector.Broker.Cut;

namespace AspectInjector.Core.Models
{
    public class CutDefinition
    {
        public ICustomAttributeProvider Target { get; set; }

        public TypeReference Aspect { get; set; }

        public TypeReference RoutableDataType { get; set; }

        public string NameFilter { get; set; }

        public AccessModifier AccessFilter { get; set; }

        public MemberScope ScopeFilter { get; set; }

        public ushort Priority { get; set; }

        [JsonIgnore]
        public virtual bool IsSpec { get; } = false;
    }
}