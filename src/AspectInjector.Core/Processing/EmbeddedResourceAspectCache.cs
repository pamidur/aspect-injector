using AspectInjector.Core.Contexts;
using AspectInjector.Core.Contracts;
using AspectInjector.Core.Extensions;
using AspectInjector.Core.Models;
using AspectInjector.Core.Processing.Converters;
using Mono.Cecil;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;

namespace AspectInjector.Core.Processing
{
    public class EmbeddedResourceAspectCache : IAspectCache
    {
        //todo:: better static cache with assembly update check
        private readonly ConcurrentDictionary<string, object> _injectionCache = new ConcurrentDictionary<string, object>();

        private ProcessingContext _context;
        private string _resourceName;

        protected ILogger Log { get; private set; }

        public void Init(ProcessingContext context)
        {
            _context = context;
            _resourceName = $"{context.Services.Prefix}injections";
            Log = context.Services.Log;
        }

        public AspectDefinition Read(TypeReference type)
        {
            return ReadInjectionsFromModule(type.Module).Where(a => a.Aspect.IsTypeOf(type));
        }

        public void Cache(ModuleDefinition toModule, IEnumerable<Effect> injections)
        {
            var serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                MissingMemberHandling = MissingMemberHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Auto,
            };

            serializerSettings.Converters.Add(new TypeReferenceConverter(_context, toModule));

            var existingInjections = ReadInjectionsFromModule(toModule);

            var newInjectionSet = injections.Union(existingInjections).ToList();

            var resource = toModule.Resources.FirstOrDefault(r => r.ResourceType == ResourceType.Embedded && r.Name == _resourceName);

            if (resource != null)
                toModule.Resources.Remove(resource);

            var json = JsonConvert.SerializeObject(newInjectionSet, serializerSettings);

            resource = new EmbeddedResource(_resourceName, ManifestResourceAttributes.Private, Encoding.UTF8.GetBytes(json));

            toModule.Resources.Add(resource);

            var cacheKey = GetCacheKey(toModule);
            object temp;
            _injectionCache.TryRemove(cacheKey, out temp);
        }

        private IEnumerable<Effect> ReadInjectionsFromModule(TypeDefinition type)
        {
            var module = type.Module;

            var serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                MissingMemberHandling = MissingMemberHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Auto,
            };

            serializerSettings.Converters.Add(new TypeReferenceConverter(_context, module));

            var cacheKey = GetCacheKey(module);

            object result;

            if (!_injectionCache.TryGetValue(cacheKey, out result))
            {
                var resource = module.Resources.FirstOrDefault(r => r.ResourceType == ResourceType.Embedded && r.Name == _resourceName);

                if (resource == null)
                    return new List<Effect>();

                var json = Encoding.UTF8.GetString(((EmbeddedResource)resource).GetResourceData());

                result = JsonConvert.DeserializeObject<List<Effect>>(json, serializerSettings);
            }

            return (IEnumerable<Effect>)result;
        }

        private string GetCacheKey(ModuleDefinition module)
        {
            return $"{module.FullyQualifiedName}";
        }

        public AspectDefinition GetAspect(TypeReference host)
        {
            throw new NotImplementedException();
        }

        public void Cache(ModuleDefinition toModule, IEnumerable<AspectDefinition> aspect)
        {
            throw new NotImplementedException();
        }
    }
}