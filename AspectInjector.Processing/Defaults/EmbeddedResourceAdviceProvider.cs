using AspectInjector.Core.Contexts;
using AspectInjector.Core.Contracts;
using Mono.Cecil;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
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
            Log = context.Services.Log;
        }

        public IEnumerable<T> GetAdvices<T>(TypeReference type) where T : IAdvice
        {
            return ReadAdvicesFromModule<T>(type.Module).Where(a => a.HostType.FullName == type.FullName);
        }

        public void StoreAdvices<T>(ModuleDefinition toModule, IEnumerable<T> advices) where T : IAdvice
        {
            var existingAdvices = ReadAdvicesFromModule<T>(toModule);

            var newAdviceSet = advices.Union(existingAdvices).ToList();

            var resourceName = GetResourceName(typeof(T));
            var resource = toModule.Resources.FirstOrDefault(r => r.ResourceType == ResourceType.Embedded && r.Name == resourceName);

            if (resource != null)
                toModule.Resources.Remove(resource);

            using (var buffer = new MemoryStream())
            {
                var writer = new StreamWriter(buffer);
                _serializer.Serialize(writer, newAdviceSet);

                buffer.Seek(0, SeekOrigin.Begin);

                resource = new EmbeddedResource(resourceName, ManifestResourceAttributes.Private, buffer);
            }

            toModule.Resources.Add(resource);

            var cacheKey = GetCacheKey(toModule, typeof(T));
            object temp;
            _adviceCache.TryRemove(cacheKey, out temp);
        }

        private IEnumerable<T> ReadAdvicesFromModule<T>(ModuleDefinition module) where T : IAdvice
        {
            var cacheKey = GetCacheKey(module, typeof(T));

            object result;

            if (!_adviceCache.TryGetValue(cacheKey, out result))
            {
                var resourceName = GetResourceName(typeof(T));
                var resource = module.Resources.FirstOrDefault(r => r.ResourceType == ResourceType.Embedded && r.Name == resourceName);

                if (resource == null)
                    return new List<T>();

                result = _serializer.Deserialize<List<T>>(new JsonTextReader(new StreamReader(((EmbeddedResource)resource).GetResourceStream())));
            }

            return (IEnumerable<T>)result;
        }

        private string GetCacheKey(ModuleDefinition module, Type adviceType)
        {
            return $"{module.FullyQualifiedName}:{adviceType.FullName}";
        }

        private string GetResourceName(Type type)
        {
            return $"{_context.Services.Prefix}{type.Name}";
        }
    }
}