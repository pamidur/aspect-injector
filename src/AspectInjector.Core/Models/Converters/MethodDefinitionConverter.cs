using Mono.Cecil;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace AspectInjector.Core.Models.Converters
{
    internal class MethodDefinitionConverter : JsonConverter
    {
        private readonly ModuleDefinition _reference;

        public MethodDefinitionConverter(ModuleDefinition reference)
        {
            _reference = reference;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(MethodDefinition);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var value = JToken.Load(reader).Value<string>();
            if (string.IsNullOrEmpty(value))
                return null;

            var tokenRefs = value.Split(new[] { ":" }, StringSplitOptions.RemoveEmptyEntries);

            var tdToken = new MetadataToken(uint.Parse(tokenRefs[0]));
            var mdToken = new MetadataToken(uint.Parse(tokenRefs[1]));

            return _reference.GetTypes().First(td => td.MetadataToken == tdToken).Methods.First(md => md.MetadataToken == mdToken);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var md = (MethodDefinition)value;
            var td = md.DeclaringType;

            var tokenRef = $"{td.MetadataToken.ToUInt32()}:{md.MetadataToken.ToUInt32()}";

            JToken.FromObject(tokenRef, serializer).WriteTo(writer);
        }
    }
}