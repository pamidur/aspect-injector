using Mono.Cecil;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using static AspectInjector.Broker.Aspect;

namespace AspectInjector.Core.Models
{
    public class AspectDefinition
    {
        public TypeDefinition Host { get; set; }

        public List<Effect> Effects { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public Scope Scope { get; set; }

        public TypeReference Factory { get; set; }
    }
}