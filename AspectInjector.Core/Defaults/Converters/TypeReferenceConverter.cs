using AspectInjector.Core.Contexts;
using AspectInjector.Core.Extensions;
using AspectInjector.Core.Models;
using Mono.Cecil;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspectInjector.Core.Defaults.Converters
{
    internal class TypeReferenceConverter : JsonConverter
    {
        private readonly ProcessingContext _context;

        public TypeReferenceConverter(ProcessingContext context)
        {
            _context = context;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(TypeReference).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return null;
            //var jo = JObject.Load(reader);
            //var fqn = jo["fqn"];

            //var assm

            //_context.Resolver.Resolve()
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var tr = (TypeReference)value;
            JToken.FromObject(FQN.FromTypeReference(tr), serializer).WriteTo(writer);

            //var jo = new JObject();

            //jo.Add("fqn", );

            //jo.WriteTo(writer);
        }
    }
}