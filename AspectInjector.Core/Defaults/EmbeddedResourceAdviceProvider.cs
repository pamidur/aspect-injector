using AspectInjector.Core.Contexts;
using AspectInjector.Core.Contracts;
using AspectInjector.Core.Defaults.Converters;
using Mono.Cecil;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AspectInjector.Core.Defaults
{
    public class EmbeddedResourceAdviceProvider : IAdviceCacheProvider
    {
        //todo:: better static cache with assembly update check
        private readonly ConcurrentDictionary<string, object> _adviceCache = new ConcurrentDictionary<string, object>();

        private ProcessingContext _context;
        private string _resourceName;
        private JsonSerializerSettings _serializerSettings;

        protected ILogger Log { get; private set; }

        public void Init(ProcessingContext context)
        {
            _context = context;
            _resourceName = $"{context.Services.Prefix}advices";
            Log = context.Services.Log;

            _serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                MissingMemberHandling = MissingMemberHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Auto,
            };

            _serializerSettings.Converters.Add(new TypeReferenceConverter(context));
        }

        public IEnumerable<IAdvice> GetAdvices(TypeReference type)
        {
            return ReadAdvicesFromModule(type.Module).Where(a => a.HostType.FullName == type.FullName);
        }

        public void StoreAdvices(ModuleDefinition toModule, IEnumerable<IAdvice> advices)
        {
            var existingAdvices = ReadAdvicesFromModule(toModule);

            var newAdviceSet = advices.Union(existingAdvices).ToList();

            var resource = toModule.Resources.FirstOrDefault(r => r.ResourceType == ResourceType.Embedded && r.Name == _resourceName);

            if (resource != null)
                toModule.Resources.Remove(resource);

            var json = JsonConvert.SerializeObject(newAdviceSet, _serializerSettings);

            resource = new EmbeddedResource(_resourceName, ManifestResourceAttributes.Private, Encoding.UTF8.GetBytes(json));

            toModule.Resources.Add(resource);

            var cacheKey = GetCacheKey(toModule);
            object temp;
            _adviceCache.TryRemove(cacheKey, out temp);
        }

        private IEnumerable<IAdvice> ReadAdvicesFromModule(ModuleDefinition module)
        {
            var cacheKey = GetCacheKey(module);

            object result;

            if (!_adviceCache.TryGetValue(cacheKey, out result))
            {
                var resource = module.Resources.FirstOrDefault(r => r.ResourceType == ResourceType.Embedded && r.Name == _resourceName);

                if (resource == null)
                    return new List<IAdvice>();

                var json = Encoding.UTF8.GetString(((EmbeddedResource)resource).GetResourceData());

                result = JsonConvert.DeserializeObject<List<IAdvice>>(json, _serializerSettings);
            }

            return (IEnumerable<IAdvice>)result;
        }

        private string GetCacheKey(ModuleDefinition module)
        {
            return $"{module.FullyQualifiedName}";
        }
    }
}