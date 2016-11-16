using AspectInjector.Core.Contexts;
using AspectInjector.Core.Contracts;
using Mono.Cecil;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AspectInjector.Core.Defaults
{
    public class EmbeddedResourceAdviceProvider : IAdviceCacheProvider
    {
        //todo:: better static cache with assembly update check
        private readonly ConcurrentDictionary<string, object> _adviceCache = new ConcurrentDictionary<string, object>();

        private ProcessingContext _context;
        private readonly JsonSerializer _serializer;
        private string _resourceName;

        protected ILogger Log { get; private set; }

        public EmbeddedResourceAdviceProvider()
        {
            _serializer = JsonSerializer.Create(new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                MissingMemberHandling = MissingMemberHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            });
        }

        public void Init(ProcessingContext context)
        {
            _context = context;
            _resourceName = $"{context.Services.Prefix}advices";
            Log = context.Services.Log;
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

            using (var buffer = new MemoryStream())
            {
                var writer = new StreamWriter(buffer);
                _serializer.Serialize(writer, newAdviceSet);

                buffer.Seek(0, SeekOrigin.Begin);

                resource = new EmbeddedResource(_resourceName, ManifestResourceAttributes.Private, buffer);
            }

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

                result = _serializer.Deserialize<List<IAdvice>>(new JsonTextReader(new StreamReader(((EmbeddedResource)resource).GetResourceStream())));
            }

            return (IEnumerable<IAdvice>)result;
        }

        private string GetCacheKey(ModuleDefinition module)
        {
            return $"{module.FullyQualifiedName}";
        }
    }
}