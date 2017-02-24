using Mono.Cecil;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace AspectInjector.Core.Models.Converters
{
    internal class ParameterDefinitionConverter : JsonConverter
    {
        private readonly ModuleDefinition _reference;

        public ParameterDefinitionConverter(ModuleDefinition reference)
        {
            _reference = reference;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ParameterDefinition);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var value = JToken.Load(reader).Value<string>();
            if (string.IsNullOrEmpty(value))
                return null;

            var tokenRefs = value.Split(new[] { ":" }, StringSplitOptions.RemoveEmptyEntries);

            var tdToken = new MetadataToken(uint.Parse(tokenRefs[0]));
            var mdToken = new MetadataToken(uint.Parse(tokenRefs[1]));
            var pdIndex = int.Parse(tokenRefs[2]);

            return _reference.GetTypes().First(td => td.MetadataToken == tdToken).Methods.First(md => md.MetadataToken == mdToken).Parameters.First(p => p.Index == pdIndex);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var pd = (ParameterDefinition)value;
            var md = (MethodDefinition)pd.Method;
            var td = md.DeclaringType;

            var tokenRef = $"{td.MetadataToken.ToUInt32()}:{md.MetadataToken.ToUInt32()}:{pd.Index}";

            JToken.FromObject(tokenRef, serializer).WriteTo(writer);
        }
    }
}