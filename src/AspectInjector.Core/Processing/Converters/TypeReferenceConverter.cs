using AspectInjector.Core.Models;
using Mono.Cecil;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace AspectInjector.Core.Processing.Converters
{
    internal class TypeReferenceConverter : JsonConverter
    {
        private readonly ModuleDefinition _reference;

        public TypeReferenceConverter(ModuleDefinition reference)
        {
            _reference = reference;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(TypeReference).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jt = JToken.Load(reader);
            var fqn = FQN.FromString(jt.ToObject<string>());

            return fqn?.ToTypeReference(_reference);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var tr = (TypeReference)value;

            var fqn = FQN.FromTypeReference(tr).ToString();

            JToken.FromObject(fqn, serializer).WriteTo(writer);
        }
    }
}