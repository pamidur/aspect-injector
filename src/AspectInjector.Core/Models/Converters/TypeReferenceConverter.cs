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
            var jt = JToken.Load(reader);
            var token = new MetadataToken(jt.ToObject<uint>());

            if (token.TokenType == TokenType.TypeDef)
                return _reference.GetTypes().First(td => td.MetadataToken == token);

            return _reference.GetTypeReferences().First(td => td.MetadataToken == token);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var td = (TypeReference)value;

            var token = td.MetadataToken.ToUInt32();

            JToken.FromObject(token, serializer).WriteTo(writer);
        }
    }
}