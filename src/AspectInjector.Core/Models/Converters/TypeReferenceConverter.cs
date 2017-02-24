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

            var tokenRefs = value.Split(new[] { ":" }, StringSplitOptions.RemoveEmptyEntries);

            var token = new MetadataToken(uint.Parse(tokenRefs[0]));

            if (token.TokenType == TokenType.TypeDef)
                return _reference.GetTypes().First(td => td.MetadataToken == token);

            if (token.TokenType == TokenType.TypeRef)
                _reference.GetTypeReferences().First(td => td.MetadataToken == token);

            if (token.TokenType == TokenType.TypeSpec)
            {
                var gtoken = new MetadataToken(uint.Parse(tokenRefs[1]));
                var type = _reference.GetTypes().First(td => td.MetadataToken == gtoken);

                var generic = new GenericInstanceType(type);
                tokenRefs.Skip(2)
                    .Select(t => new MetadataToken(uint.Parse(t))).ToList()
                    .ForEach(t => generic.GenericArguments.Add(_reference.GetTypes().First(td => td.MetadataToken == t)));

                return generic;
            }

            throw new NotSupportedException($"Not supported token type {token.TokenType.ToString()}");
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var td = (TypeReference)value;

            var token = td.MetadataToken.ToUInt32().ToString();

            if (td is GenericInstanceType)
            {
                token += $":{td.GetElementType().Resolve().MetadataToken.ToUInt32()}";

                foreach (var ga in ((GenericInstanceType)td).GenericArguments)
                    token += $":{ga.Resolve().MetadataToken.ToUInt32()}";
            }

            JToken.FromObject(token, serializer).WriteTo(writer);
        }
    }
}