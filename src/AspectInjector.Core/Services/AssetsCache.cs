using AspectInjector.Core.Models;
using AspectInjector.Core.Models.Converters;
using Mono.Cecil;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AspectInjector.Core.Services
{
    public class AssetsCache : ServiceBase
    {
        private readonly Dictionary<string, Assets> _assetsCache = new Dictionary<string, Assets>();

        public AssetsCache(Logger logger) : base(logger)
        {
        }

        public AspectDefinition ReadAspect(TypeDefinition type)
        {
            return GetAssets(type.Module).Aspects.FirstOrDefault(a => a.Host == type);
        }

        public void Cache(IEnumerable<AspectDefinition> aspects)
        {
            foreach (var aspect in aspects)
                Cache(aspect);
        }

        public void Cache(AspectDefinition aspect)
        {
            var assets = GetAssets(aspect.Host.Module);

            assets.Aspects.RemoveAll(a => a.Host == aspect.Host);
            assets.Aspects.Add(aspect);
        }

        private Assets GetAssets(ModuleDefinition module)
        {
            EnsureCacheLoaded(module);
            return _assetsCache[module.FullyQualifiedName];
        }

        private void EnsureCacheLoaded(ModuleDefinition module)
        {
            if (!_assetsCache.ContainsKey(module.FullyQualifiedName))
                _assetsCache.Add(module.FullyQualifiedName, ReadAssetsFromModule(module));
        }

        public void FlushCache(AssemblyDefinition assembly)
        {
            foreach (var module in assembly.Modules)
                FlushCacheToModule(module);
        }

        private void FlushCacheToModule(ModuleDefinition module)
        {
            var assets = _assetsCache[module.FullyQualifiedName];

            if (assets == null) // cache was neither loaded nor created
                return;

            var serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                MissingMemberHandling = MissingMemberHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Auto,
            };

            serializerSettings.Converters.Add(new TypeReferenceConverter(module));
            serializerSettings.Converters.Add(new MethodDefinitionConverter(module));

            var json = JsonConvert.SerializeObject(assets, serializerSettings);
            var content = Encoding.UTF8.GetBytes(json);

            UpdateResource(Constants.AssetsResourceName, content, module);
        }

        private Assets ReadAssetsFromModule(ModuleDefinition module)
        {
            var serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                MissingMemberHandling = MissingMemberHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Auto,
            };

            serializerSettings.Converters.Add(new TypeReferenceConverter(module));
            serializerSettings.Converters.Add(new MethodDefinitionConverter(module));

            var resource = module.Resources.FirstOrDefault(r => r.ResourceType == ResourceType.Embedded && r.Name == Constants.AssetsResourceName);
            if (resource == null)
                return new Assets();

            var json = Encoding.UTF8.GetString(((EmbeddedResource)resource).GetResourceData());

            return JsonConvert.DeserializeObject<Assets>(json, serializerSettings);
        }

        private void UpdateResource(string name, byte[] content, ModuleDefinition module)
        {
            var resource = module.Resources.FirstOrDefault(r => r.ResourceType == ResourceType.Embedded && r.Name == name);

            if (resource != null)
                module.Resources.Remove(resource);

            resource = new EmbeddedResource(name, ManifestResourceAttributes.Private, content);
            module.Resources.Add(resource);
        }
    }
}