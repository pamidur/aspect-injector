using AspectInjector.Core.Extensions;
using Mono.Cecil;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace AspectInjector.Core.Models.Converters
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
            var value = JToken.Load(reader).Value<string>();
            if (string.IsNullOrEmpty(value))
                return null;

            var tr = FQN.FromString(value).ToTypeReference(_reference);

            if (objectType == typeof(TypeDefinition))
                return tr.Resolve();

            return tr;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var td = (TypeReference)value;
            JToken.FromObject(td.GetFQN().ToString(), serializer).WriteTo(writer);
        }
    }
}